using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Locations;
using System.Threading;
using Android.App;
using Android.OS;
using System.Linq;
using Android.Content;
using Xamarin.Services.Permissions;

namespace Xamarin.Services.Geolocation
{
	public partial class GeolocationService
	{
		string[] allProviders;
		LocationManager locationManager;

		GeolocationContinuousListener listener;
		GeolocationSingleListener singleListener = null;

		readonly object positionSync = new object();
		Position lastPosition;

		public GeolocationService()
		{
			DesiredAccuracy = 100;
		}

		string[] Providers => Manager.GetProviders(enabledOnly: false).ToArray();
		string[] IgnoredProviders => new string[] { LocationManager.PassiveProvider, "local_database" };

		public static string[] ProvidersToUse { get; set; } = new string[] { };

		public static string[] ProvidersToUseWhileListening { get; set; } = new string[] { };

		LocationManager Manager
		{
			get
			{
				if (locationManager == null)
					locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);

				return locationManager;
			}
		}

		public bool IsListening => listener != null;

		public double DesiredAccuracy { get; set; }

		public bool IsHeadingSupported => true;

		public bool IsSupported => Providers.Length > 0;

		public bool IsEnabled => Providers.Any(p => !IgnoredProviders.Contains(p) &&
			Manager.IsProviderEnabled(p));

		public async Task<Position> GetLastKnownLocationAsync()
		{
			var hasPermission = await CheckPermissions();
			if (!hasPermission)
				throw new GeolocationException(GeolocationError.Unauthorized);

			Location bestLocation = null;
			foreach (var provider in Providers)
			{
				var location = Manager.GetLastKnownLocation(provider);
				if (location != null && GeolocationUtils.IsBetterLocation(location, bestLocation))
					bestLocation = location;
			}

			return bestLocation?.ToPosition();

		}

		async Task<bool> CheckPermissions()
		{
			var status = await PermissionsService.Current.CheckPermissionStatusAsync(Xamarin.Services.Permissions.Permission.Location);
			if (status != PermissionStatus.Granted)
			{
				Console.WriteLine("Currently does not have Location permissions, requesting permissions");

				var request = await PermissionsService.Current.RequestPermissionsAsync(Xamarin.Services.Permissions.Permission.Location);

				if (request[Xamarin.Services.Permissions.Permission.Location] != Xamarin.Services.Permissions.PermissionStatus.Granted)
				{
					Console.WriteLine("Location permission denied, can not get positions async.");
					return false;
				}
			}

			return true;
		}

		public async Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
		{
			var timeoutMilliseconds = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : Timeout.Infinite;

			if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
				throw new ArgumentOutOfRangeException(nameof(timeout), "timeout must be greater than or equal to 0");

			if (!cancelToken.HasValue)
				cancelToken = CancellationToken.None;

			var hasPermission = await CheckPermissions();
			if (!hasPermission)
				throw new GeolocationException(GeolocationError.Unauthorized);

			var tcs = new TaskCompletionSource<Position>();

			if (!IsListening)
			{
				var providers = new List<string>();
				if (ProvidersToUse == null || ProvidersToUse.Length == 0)
					providers.AddRange(Providers);
				else
				{
					//only add providers requested.
					foreach (var provider in Providers)
					{
						if (ProvidersToUse?.Contains(provider) ?? false)
							continue;

						providers.Add(provider);
					}
				}


				void SingleListnerFinishCallback()
				{
					if (singleListener == null)
						return;

					for (var i = 0; i < providers.Count; ++i)
						Manager.RemoveUpdates(singleListener);
				}

				singleListener = new GeolocationSingleListener(Manager,
					(float)DesiredAccuracy,
					timeoutMilliseconds,
					providers.Where(Manager.IsProviderEnabled),
					finishedCallback: SingleListnerFinishCallback);

				if (cancelToken != CancellationToken.None)
				{
					cancelToken.Value.Register(() =>
					{
						singleListener.Cancel();

						for (var i = 0; i < providers.Count; ++i)
							Manager.RemoveUpdates(singleListener);
					}, true);
				}

				try
				{
					var looper = Looper.MyLooper() ?? Looper.MainLooper;

					var enabled = 0;
					for (var i = 0; i < providers.Count; ++i)
					{
						if (Manager.IsProviderEnabled(providers[i]))
							enabled++;

						Manager.RequestLocationUpdates(providers[i], 0, 0, singleListener, looper);
					}

					if (enabled == 0)
					{
						for (int i = 0; i < providers.Count; ++i)
							Manager.RemoveUpdates(singleListener);

						tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
						return await tcs.Task;
					}
				}
				catch (Java.Lang.SecurityException ex)
				{
					tcs.SetException(new GeolocationException(GeolocationError.Unauthorized, ex));
					return await tcs.Task;
				}

				return await singleListener.Task;
			}

			// If we're already listening, just use the current listener
			lock (positionSync)
			{
				if (lastPosition == null)
				{
					if (cancelToken != CancellationToken.None)
					{
						cancelToken.Value.Register(() => tcs.TrySetCanceled());
					}

					EventHandler<PositionEventArgs> gotPosition = null;
					gotPosition = (s, e) =>
					{
						tcs.TrySetResult(e.Position);
						PositionChanged -= gotPosition;
					};

					PositionChanged += gotPosition;
				}
				else
				{
					tcs.SetResult(lastPosition);
				}
			}

			return await tcs.Task;
		}

		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null)
		{
			if (position == null)
				throw new ArgumentNullException(nameof(position));

			using (var geocoder = new Geocoder(Application.Context))
			{
				var addressList = await geocoder.GetFromLocationAsync(position.Latitude, position.Longitude, 10);
				return addressList.ToAddresses();
			}
		}

		public async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null)
		{
			if (address == null)
				throw new ArgumentNullException(nameof(address));

			using (var geocoder = new Geocoder(Application.Context))
			{
				var addressList = await geocoder.GetFromLocationNameAsync(address, 10);
				return addressList.Select(p => new Position
				{
					Latitude = p.Latitude,
					Longitude = p.Longitude
				});
			}
		}

		List<string> listeningProviders { get; } = new List<string>();

		public async Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
		{
			var hasPermission = await CheckPermissions();
			if (!hasPermission)
				throw new GeolocationException(GeolocationError.Unauthorized);


			var minTimeMilliseconds = minimumTime.TotalMilliseconds;
			if (minTimeMilliseconds < 0)
				throw new ArgumentOutOfRangeException(nameof(minimumTime));
			if (minimumDistance < 0)
				throw new ArgumentOutOfRangeException(nameof(minimumDistance));
			if (IsListening)
				throw new InvalidOperationException("This Geolocator is already listening");

			var providers = Providers;
			listener = new GeolocationContinuousListener(Manager, minimumTime, providers);
			listener.PositionChanged += OnListenerPositionChanged;
			listener.PositionError += OnListenerPositionError;

			var looper = Looper.MyLooper() ?? Looper.MainLooper;
			listeningProviders.Clear();
			for (var i = 0; i < providers.Length; ++i)
			{
				var provider = providers[i];

				//we have limited set of providers
				if (ProvidersToUseWhileListening != null &&
					ProvidersToUseWhileListening.Length > 0)
				{
					//the provider is not in the list, so don't use it.
					if (!ProvidersToUseWhileListening.Contains(provider))
						continue;
				}


				listeningProviders.Add(provider);
				Manager.RequestLocationUpdates(provider, (long)minTimeMilliseconds, (float)minimumDistance, listener, looper);
			}
			return true;
		}

		public Task<bool> StopListeningAsync()
		{
			if (listener == null)
				return Task.FromResult(true);

			if (listeningProviders == null)
				return Task.FromResult(true);

			var providers = listeningProviders;
			listener.PositionChanged -= OnListenerPositionChanged;
			listener.PositionError -= OnListenerPositionError;

			for (var i = 0; i < providers.Count; i++)
			{
				try
				{
					Manager.RemoveUpdates(listener);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("Unable to remove updates: " + ex);
				}
			}

			listener = null;
			return Task.FromResult(true);
		}

		private void OnListenerPositionChanged(object sender, PositionEventArgs e)
		{
			if (!IsListening) // ignore anything that might come in afterwards
				return;

			lock (positionSync)
			{
				lastPosition = e.Position;
				OnPositionChanged(e);
			}
		}

		private async void OnListenerPositionError(object sender, PositionErrorEventArgs e)
		{
			await StopListeningAsync();

			OnPositionError(e);
		}
	}
}
