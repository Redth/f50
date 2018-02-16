using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CoreLocation;
using Foundation;
using Xamarin.Services.Permissions;
#if __IOS__ || __TVOS__
using UIKit;
#elif __MACOS__
using AppKit;
#endif

namespace Xamarin.Services.Geolocation
{
	public partial class GeolocationService
	{
		private bool deferringUpdates;
		private readonly CLLocationManager manager;
		private bool isListening;
		private Position lastPosition;
		private ListenerSettings listenerSettings;

		public GeolocationService()
		{
			DesiredAccuracy = 100;
			manager = GetManager();
			manager.AuthorizationChanged += OnAuthorizationChanged;
			manager.Failed += OnFailed;

#if __IOS__
			if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
				manager.LocationsUpdated += OnLocationsUpdated;
			else
				manager.UpdatedLocation += OnUpdatedLocation;

			manager.UpdatedHeading += OnUpdatedHeading;
#elif __MACOS__ || __TVOS__
            manager.LocationsUpdated += OnLocationsUpdated;
#endif

#if __IOS__ || __MACOS__
			manager.DeferredUpdatesFinished += OnDeferredUpdatedFinished;
#endif

#if __TVOS__
			RequestAuthorization();
#endif
		}

		public double DesiredAccuracy { get; set; }

		public bool IsListening => isListening;

#if __IOS__ || __MACOS__
		public bool IsHeadingSupported => CLLocationManager.HeadingAvailable;
#elif __TVOS__
		public bool IsHeadingSupported => false;
#endif

		public bool IsSupported => true; //all iOS devices support Geolocation

		public bool IsEnabled
		{
			get
			{
				var status = CLLocationManager.Status;
				return CLLocationManager.LocationServicesEnabled;
			}
		}

		public async Task<Position> GetLastKnownLocationAsync()
		{
#if __IOS__
			var hasPermission = await CheckPermissions(Permission.LocationWhenInUse);
			if (!hasPermission)
				throw new GeolocationException(GeolocationError.Unauthorized);
#endif
			var m = GetManager();
			var newLocation = m?.Location;

			if (newLocation == null)
				return null;

			var position = new Position();
			position.Accuracy = newLocation.HorizontalAccuracy;
			position.Altitude = newLocation.Altitude;
			position.AltitudeAccuracy = newLocation.VerticalAccuracy;
			position.Latitude = newLocation.Coordinate.Latitude;
			position.Longitude = newLocation.Coordinate.Longitude;

#if !__TVOS__
			position.Speed = newLocation.Speed;
#endif

			try
			{
				position.Timestamp = new DateTimeOffset(newLocation.Timestamp.ToDateTime());
			}
			catch (Exception ex)
			{
				position.Timestamp = DateTimeOffset.UtcNow;
			}

			return position;
		}

		public async Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
		{
#if __IOS__
			var permission = Permission.Location;
			var hasPermission = await CheckPermissions(permission);
			if (!hasPermission)
				throw new GeolocationException(GeolocationError.Unauthorized);
#endif

			var timeoutMilliseconds = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : Timeout.Infinite;

			if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
				throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive or Timeout.Infinite");

			if (!cancelToken.HasValue)
				cancelToken = CancellationToken.None;

			TaskCompletionSource<Position> tcs;
			if (!IsListening)
			{
				var m = GetManager();
				m.DesiredAccuracy = DesiredAccuracy;
#if __IOS__
				// permit background updates if background location mode is enabled
				if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
				{
					var backgroundModes = NSBundle.MainBundle.InfoDictionary[(NSString)"UIBackgroundModes"] as NSArray;
					var allow = backgroundModes != null && (backgroundModes.Contains((NSString)"Location") || backgroundModes.Contains((NSString)"location"));

					if (allow)
					{
						allow = await CheckPermissions(Permission.LocationAlways);

					}
					m.AllowsBackgroundLocationUpdates = allow;
				}

				// always prevent location update pausing since we're only listening for a single update.
				if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
					m.PausesLocationUpdatesAutomatically = false;
#endif

				tcs = new TaskCompletionSource<Position>(m);
				var singleListener = new GeolocationSingleUpdateDelegate(m, DesiredAccuracy, includeHeading, timeoutMilliseconds, cancelToken.Value);
				m.Delegate = singleListener;

#if __IOS__ || __MACOS__
				m.StartUpdatingLocation();
#elif __TVOS__
                m.RequestLocation();
#endif


#if __IOS__
				if (includeHeading && IsHeadingSupported)
					m.StartUpdatingHeading();
#endif

				return await singleListener.Task;
			}


			tcs = new TaskCompletionSource<Position>();
			if (lastPosition == null)
			{
				if (cancelToken != CancellationToken.None)
				{
					cancelToken.Value.Register(() => tcs.TrySetCanceled());
				}

				EventHandler<PositionErrorEventArgs> gotError = null;
				gotError = (s, e) =>
				{
					tcs.TrySetException(new GeolocationException(e.Error));
					PositionError -= gotError;
				};

				PositionError += gotError;

				EventHandler<PositionEventArgs> gotPosition = null;
				gotPosition = (s, e) =>
				{
					tcs.TrySetResult(e.Position);
					PositionChanged -= gotPosition;
				};

				PositionChanged += gotPosition;
			}
			else
				tcs.SetResult(lastPosition);


			return await tcs.Task;
		}

		public async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null)
		{
			if (address == null)
				throw new ArgumentNullException(nameof(address));

			using (var geocoder = new CLGeocoder())
			{
				var positionList = await geocoder.GeocodeAddressAsync(address);
				return positionList.Select(p => new Position
				{
					Latitude = p.Location.Coordinate.Latitude,
					Longitude = p.Location.Coordinate.Longitude
				});
			}
		}

		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null)
		{
			if (position == null)
				throw new ArgumentNullException(nameof(position));

			using (var geocoder = new CLGeocoder())
			{
				var addressList = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(position.Latitude, position.Longitude));
				return addressList.ToAddresses();
			}
		}

		public async Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
		{


			if (minimumDistance < 0)
				throw new ArgumentOutOfRangeException(nameof(minimumDistance));

			if (isListening)
				throw new InvalidOperationException("Already listening");

			// if no settings were passed in, instantiate the default settings. need to check this and create default settings since
			// previous calls to StartListeningAsync might have already configured the location manager in a non-default way that the
			// caller of this method might not be expecting. the caller should expect the defaults if they pass no settings.
			if (listenerSettings == null)
				listenerSettings = new ListenerSettings();

#if __IOS__

			var permission = Permission.Location;
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				if (listenerSettings.AllowBackgroundUpdates)
					permission = Permission.LocationAlways;
				else
					permission = Permission.LocationWhenInUse;
			}

			var hasPermission = await CheckPermissions(permission);


			if (!hasPermission)
				throw new GeolocationException(GeolocationError.Unauthorized);
#endif

			// keep reference to settings so that we can stop the listener appropriately later
			this.listenerSettings = listenerSettings;

			var desiredAccuracy = DesiredAccuracy;

			// set background flag
#if __IOS__
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
				manager.AllowsBackgroundLocationUpdates = listenerSettings.AllowBackgroundUpdates;

			// configure location update pausing
			if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
			{
				manager.PausesLocationUpdatesAutomatically = listenerSettings.PauseLocationUpdatesAutomatically;

				switch (listenerSettings.ActivityType)
				{
					case ActivityType.AutomotiveNavigation:
						manager.ActivityType = CLActivityType.AutomotiveNavigation;
						break;
					case ActivityType.Fitness:
						manager.ActivityType = CLActivityType.Fitness;
						break;
					case ActivityType.OtherNavigation:
						manager.ActivityType = CLActivityType.OtherNavigation;
						break;
					default:
						manager.ActivityType = CLActivityType.Other;
						break;
				}
			}
#endif

			// to use deferral, CLLocationManager.DistanceFilter must be set to CLLocationDistance.None, and CLLocationManager.DesiredAccuracy must be 
			// either CLLocation.AccuracyBest or CLLocation.AccuracyBestForNavigation. deferral only available on iOS 6.0 and above.
			if (CanDeferLocationUpdate && listenerSettings.DeferLocationUpdates)
			{
				minimumDistance = CLLocationDistance.FilterNone;
				desiredAccuracy = CLLocation.AccuracyBest;
			}

			isListening = true;
			manager.DesiredAccuracy = desiredAccuracy;
			manager.DistanceFilter = minimumDistance;

#if __IOS__ || __MACOS__
			if (listenerSettings.ListenForSignificantChanges)
				manager.StartMonitoringSignificantLocationChanges();
			else
				manager.StartUpdatingLocation();
#elif __TVOS__
            //not supported
#endif

#if __IOS__
			if (includeHeading && CLLocationManager.HeadingAvailable)
				manager.StartUpdatingHeading();
#endif

			return true;
		}

		public Task<bool> StopListeningAsync()
		{
			if (!isListening)
				return Task.FromResult(true);

			isListening = false;
#if __IOS__
			if (CLLocationManager.HeadingAvailable)
				manager.StopUpdatingHeading();

			// it looks like deferred location updates can apply to the standard service or significant change service. disallow deferral in either case.
			if ((listenerSettings?.DeferLocationUpdates ?? false) && CanDeferLocationUpdate)
				manager.DisallowDeferredLocationUpdates();
#endif


#if __IOS__ || __MACOS__
			if (listenerSettings?.ListenForSignificantChanges ?? false)
				manager.StopMonitoringSignificantLocationChanges();
			else
				manager.StopUpdatingLocation();
#endif

			listenerSettings = null;
			lastPosition = null;

			return Task.FromResult(true);
		}

		private void RequestAuthorization()
		{
#if __IOS__
			var info = NSBundle.MainBundle.InfoDictionary;

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
					manager.RequestAlwaysAuthorization();
				else if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
					manager.RequestWhenInUseAuthorization();
				else
					throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
			}
#elif __MACOS__
            //nothing to do here.
#elif __TVOS__
            var info = NSBundle.MainBundle.InfoDictionary;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                    manager.RequestWhenInUseAuthorization();
                else
                    throw new UnauthorizedAccessException("On tvOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
            }
#endif
		}

		private CLLocationManager GetManager()
		{
			CLLocationManager m = null;
			new NSObject().InvokeOnMainThread(() => m = new CLLocationManager());
			return m;
		}

#if __IOS__
		private void OnUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
		{
			if (e.NewHeading.TrueHeading == -1)
				return;

			var p = (lastPosition == null) ? new Position() : new Position(lastPosition);

			p.Heading = e.NewHeading.TrueHeading;

			lastPosition = p;

			OnPositionChanged(new PositionEventArgs(p));
		}
#endif

		private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
		{
			foreach (var location in e.Locations)
				UpdatePosition(location);

			// defer future location updates if requested
			if ((listenerSettings?.DeferLocationUpdates ?? false) && !deferringUpdates && CanDeferLocationUpdate)
			{
#if __IOS__
				manager.AllowDeferredLocationUpdatesUntil(listenerSettings.DeferralDistanceMeters == null ? CLLocationDistance.MaxDistance : listenerSettings.DeferralDistanceMeters.GetValueOrDefault(),
					listenerSettings.DeferralTime == null ? CLLocationManager.MaxTimeInterval : listenerSettings.DeferralTime.GetValueOrDefault().TotalSeconds);
#endif

				deferringUpdates = true;
			}
		}

#if __IOS__ || __MACOS__
		private void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e) => UpdatePosition(e.NewLocation);
#endif

		private void UpdatePosition(CLLocation location)
		{
			var p = (lastPosition == null) ? new Position() : new Position(this.lastPosition);

			if (location.HorizontalAccuracy > -1)
			{
				p.Accuracy = location.HorizontalAccuracy;
				p.Latitude = location.Coordinate.Latitude;
				p.Longitude = location.Coordinate.Longitude;
			}

			if (location.VerticalAccuracy > -1)
			{
				p.Altitude = location.Altitude;
				p.AltitudeAccuracy = location.VerticalAccuracy;
			}

#if __IOS__ || __MACOS__
			if (location.Speed > -1)
				p.Speed = location.Speed;
#endif

			try
			{
				var date = location.Timestamp.ToDateTime();
				p.Timestamp = new DateTimeOffset(date);
			}
			catch (Exception ex)
			{
				p.Timestamp = DateTimeOffset.UtcNow;
			}


			lastPosition = p;

			OnPositionChanged(new PositionEventArgs(p));

			location.Dispose();
		}

		private async void OnFailed(object sender, NSErrorEventArgs e)
		{
			if ((CLError)(int)e.Error.Code == CLError.Network)
			{
				await StopListeningAsync();
				OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
			}
		}

		private async void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
		{
			if (e.Status == CLAuthorizationStatus.Denied || e.Status == CLAuthorizationStatus.Restricted)
			{
				await StopListeningAsync();
				OnPositionError(new PositionErrorEventArgs(GeolocationError.Unauthorized));
			}
		}

		private void OnDeferredUpdatedFinished(object sender, NSErrorEventArgs e) => deferringUpdates = false;

#if __IOS__
		private bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable && UIDevice.CurrentDevice.CheckSystemVersion(6, 0);
#elif __MACOS__
		private bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable;
#elif __TVOS__
		private bool CanDeferLocationUpdate => false;
#endif

#if __IOS__
		private async Task<bool> CheckPermissions(Permissions.Permission permission)
		{
			var status = await PermissionsService.Current.CheckPermissionStatusAsync(permission);
			if (status != PermissionStatus.Granted)
			{
				Console.WriteLine("Currently does not have Location permissions, requesting permissions");

				var request = await PermissionsService.Current.RequestPermissionsAsync(permission);

				if (request[permission] != PermissionStatus.Granted)
				{
					Console.WriteLine("Location permission denied, can not get positions async.");
					return false;
				}
			}

			return true;
		}
#endif
	}
}
