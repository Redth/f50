using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;

namespace Xamarin.Services.Geolocation
{
	public class GeolocationService
#if !EXCLUDE_INTERFACES
		: IGeolocationService
#endif
	{
		bool isListening;
		double desiredAccuracy;
		Windows.Devices.Geolocation.Geolocator locator = new Windows.Devices.Geolocation.Geolocator();

		public GeolocationService()
		{
			DesiredAccuracy = 100;
		}

		public event EventHandler<PositionEventArgs> PositionChanged;

		public event EventHandler<PositionErrorEventArgs> PositionError;

		public bool IsHeadingSupported => false;

		public bool IsSupported
		{
			get
			{
				var status = GetGeolocatorStatus();

				while (status == PositionStatus.Initializing)
				{
					Task.Delay(10).Wait();
					status = GetGeolocatorStatus();
				}

				return status != PositionStatus.NotAvailable;
			}
		}

		public bool IsEnabled
		{
			get
			{
				var status = GetGeolocatorStatus();

				while (status == PositionStatus.Initializing)
				{
					Task.Delay(10).Wait();
					status = GetGeolocatorStatus();
				}

				return status != PositionStatus.Disabled && status != PositionStatus.NotAvailable;
			}
		}

		public double DesiredAccuracy
		{
			get { return desiredAccuracy; }
			set
			{
				desiredAccuracy = value;
				GetGeolocator().DesiredAccuracy = (value < 100) ? PositionAccuracy.High : PositionAccuracy.Default;
			}
		}

		public bool IsListening => isListening;


		public Task<Position> GetLastKnownLocationAsync() =>
			Task.Factory.StartNew<Position>(() => { return null; });

		public Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
		{
			var timeoutMilliseconds = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : Timeout.Infite;

			if (timeoutMilliseconds < 0 && timeoutMilliseconds != Timeout.Infite)
				throw new ArgumentOutOfRangeException(nameof(timeout));

			if (!cancelToken.HasValue)
				cancelToken = CancellationToken.None;

			var pos = GetGeolocator().GetGeopositionAsync(TimeSpan.FromTicks(0), TimeSpan.FromDays(365));
			cancelToken.Value.Register(o => ((IAsyncOperation<Geoposition>)o).Cancel(), pos);


			var timer = new Timeout(timeoutMilliseconds, pos.Cancel);

			var tcs = new TaskCompletionSource<Position>();

			pos.Completed = (op, s) =>
			{
				timer.Cancel();

				switch (s)
				{
					case AsyncStatus.Canceled:
						tcs.SetCanceled();
						break;
					case AsyncStatus.Completed:
						tcs.SetResult(GetPosition(op.GetResults()));
						break;
					case AsyncStatus.Error:
						var ex = op.ErrorCode;
						if (ex is UnauthorizedAccessException)
							ex = new GeolocationException(GeolocationError.Unauthorized, ex);

						tcs.SetException(ex);
						break;
				}
			};

			return tcs.Task;
		}

		void SetMapKey(string mapKey)
		{
			if (string.IsNullOrWhiteSpace(mapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
			{
				System.Diagnostics.Debug.WriteLine("Map API key is required on UWP to reverse geolocate.");
				throw new ArgumentNullException(nameof(mapKey));

			}

			if (!string.IsNullOrWhiteSpace(mapKey))
				MapService.ServiceToken = mapKey;
		}

		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null)
		{

			if (position == null)
				throw new ArgumentNullException(nameof(position));

			SetMapKey(mapKey);

			var queryResults =
				await MapLocationFinder.FindLocationsAtAsync(
						new Geopoint(new BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude })).AsTask();

			return queryResults?.Locations.ToAddresses();
		}

		public async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null)
		{
			if (address == null)
				throw new ArgumentNullException(nameof(address));

			SetMapKey(mapKey);

			var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);
			var positions = new List<Position>();
			if (queryResults?.Locations == null)
				return positions;

			foreach (var p in queryResults.Locations)
				positions.Add(new Position
				{
					Latitude = p.Point.Position.Latitude,
					Longitude = p.Point.Position.Longitude
				});

			return positions;
		}

		public Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
		{
			if (minimumTime.TotalMilliseconds <= 0 && minimumDistance <= 0)
				throw new ArgumentException("You must specify either a minimumTime or minimumDistance, setting a minimumDistance will always take precedence over minTime");

			if (minimumTime.TotalMilliseconds < 0)
				throw new ArgumentOutOfRangeException(nameof(minimumTime));

			if (minimumDistance < 0)
				throw new ArgumentOutOfRangeException(nameof(minimumDistance));

			if (isListening)
				throw new InvalidOperationException();

			isListening = true;

			var loc = GetGeolocator();

			loc.ReportInterval = (uint)minimumTime.TotalMilliseconds;
			loc.MovementThreshold = minimumDistance;
			loc.PositionChanged += OnLocatorPositionChanged;
			loc.StatusChanged += OnLocatorStatusChanged;

			return Task.FromResult(true);
		}

		public Task<bool> StopListeningAsync()
		{
			if (!isListening)
				return Task.FromResult(true);

			locator.PositionChanged -= OnLocatorPositionChanged;
			locator.StatusChanged -= OnLocatorStatusChanged;
			isListening = false;

			return Task.FromResult(true);
		}

		private async void OnLocatorStatusChanged(Windows.Devices.Geolocation.Geolocator sender, StatusChangedEventArgs e)
		{
			GeolocationError error;
			switch (e.Status)
			{
				case PositionStatus.Disabled:
					error = GeolocationError.Unauthorized;
					break;

				case PositionStatus.NoData:
					error = GeolocationError.PositionUnavailable;
					break;

				default:
					return;
			}

			if (isListening)
			{
				await StopListeningAsync();
				OnPositionError(new PositionErrorEventArgs(error));
			}

			locator = null;
		}

		private void OnLocatorPositionChanged(Windows.Devices.Geolocation.Geolocator sender, PositionChangedEventArgs e)
		{
			OnPositionChanged(new PositionEventArgs(GetPosition(e.Position)));
		}

		private void OnPositionChanged(PositionEventArgs e) => PositionChanged?.Invoke(this, e);

		private void OnPositionError(PositionErrorEventArgs e) => PositionError?.Invoke(this, e);

		private Windows.Devices.Geolocation.Geolocator GetGeolocator()
		{
			var loc = locator;
			if (loc == null)
			{
				locator = new Windows.Devices.Geolocation.Geolocator();
				locator.StatusChanged += OnLocatorStatusChanged;
				loc = locator;
			}

			return loc;
		}

		private PositionStatus GetGeolocatorStatus()
		{
			var loc = GetGeolocator();
			return loc.LocationStatus;
		}

		private static Position GetPosition(Geoposition position)
		{
			var pos = new Position
			{
				Accuracy = position.Coordinate.Accuracy,
				Latitude = position.Coordinate.Point.Position.Latitude,
				Longitude = position.Coordinate.Point.Position.Longitude,
				Timestamp = position.Coordinate.Timestamp.ToUniversalTime(),
			};

			if (position.Coordinate.Heading != null)
				pos.Heading = position.Coordinate.Heading.Value;

			if (position.Coordinate.Speed != null)
				pos.Speed = position.Coordinate.Speed.Value;

			if (position.Coordinate.AltitudeAccuracy.HasValue)
				pos.AltitudeAccuracy = position.Coordinate.AltitudeAccuracy.Value;

			pos.Altitude = position.Coordinate.Point.Position.Altitude;

			return pos;
		}
	}
}
