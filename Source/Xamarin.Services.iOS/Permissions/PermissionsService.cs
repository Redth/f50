﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;
using AVFoundation;
using Foundation;
using AddressBook;
using CoreMotion;
using EventKit;
using UIKit;
using Photos;
using System.Diagnostics;
using Speech;
using MediaPlayer;

namespace Xamarin.Services.Permissions
{
	public partial class PermissionsService
	{
		CLLocationManager locationManager;
		ABAddressBook addressBook;
		EKEventStore eventStore;
		CMMotionActivityManager activityManager;

		static PermissionsService current;

		internal static PermissionsService Current
		{
			get
			{
				if (current == null)
					current = new PermissionsService();
				return current;
			}
		}

		public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission)
		{
			return Task.FromResult(false);
		}

		public Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission)
		{
			switch (permission)
			{
				case Permission.Calendar:
					return Task.FromResult(GetEventPermissionStatus(EKEntityType.Event));
				case Permission.Camera:
					return Task.FromResult(GetAVPermissionStatus(AVMediaType.Video));
				case Permission.Contacts:
					return Task.FromResult(ContactsPermissionStatus);
				case Permission.Location:
				case Permission.LocationAlways:
				case Permission.LocationWhenInUse:
					return Task.FromResult(GetLocationPermissionStatus(permission));
				case Permission.MediaLibrary:
					return Task.FromResult(MediaLibraryPermissionStatus);
				case Permission.Microphone:
					return Task.FromResult(GetAVPermissionStatus(AVMediaType.Audio));
				//case Permission.NotificationsLocal:
				//    break;
				//case Permission.NotificationsRemote:
				//    break;
				case Permission.Photos:
					return Task.FromResult(PhotosPermissionStatus);
				case Permission.Reminders:
					return Task.FromResult(GetEventPermissionStatus(EKEntityType.Reminder));
				case Permission.Sensors:
					return Task.FromResult((CMMotionActivityManager.IsActivityAvailable ? PermissionStatus.Granted : PermissionStatus.Denied));
				case Permission.Speech:
					return Task.FromResult(SpeechPermissionStatus);
			}
			return Task.FromResult(PermissionStatus.Granted);
		}

		public async Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions)
		{
			var results = new Dictionary<Permission, PermissionStatus>();
			foreach (var permission in permissions)
			{
				if (results.ContainsKey(permission))
					continue;

				switch (permission)
				{
					case Permission.Calendar:
						results.Add(permission, await RequestEventPermission(EKEntityType.Event).ConfigureAwait(false));
						break;
					case Permission.Camera:
						try
						{
							var authCamera = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video).ConfigureAwait(false);
							results.Add(permission, (authCamera ? PermissionStatus.Granted : PermissionStatus.Denied));
						}
						catch (Exception ex)
						{
							Debug.WriteLine("Unable to get camera permission: " + ex);
							results.Add(permission, PermissionStatus.Unknown);
						}
						break;
					case Permission.Contacts:
						results.Add(permission, await RequestContactsPermission().ConfigureAwait(false));
						break;
					case Permission.LocationWhenInUse:
					case Permission.LocationAlways:
					case Permission.Location:
						results.Add(permission, await RequestLocationPermission(permission).ConfigureAwait(false));
						break;
					case Permission.MediaLibrary:
						results.Add(permission, await RequestMediaLibraryPermission().ConfigureAwait(false));
						break;
					case Permission.Microphone:
						try
						{
							var authMic = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Audio).ConfigureAwait(false);
							results.Add(permission, (authMic ? PermissionStatus.Granted : PermissionStatus.Denied));
						}
						catch (Exception ex)
						{
							Debug.WriteLine("Unable to get microphone permission: " + ex);
							results.Add(permission, PermissionStatus.Unknown);
						}
						break;
					case Permission.Photos:
						results.Add(permission, await RequestPhotosPermission().ConfigureAwait(false));
						break;
					case Permission.Reminders:
						results.Add(permission, await RequestEventPermission(EKEntityType.Reminder).ConfigureAwait(false));
						break;
					case Permission.Sensors:
						results.Add(permission, await RequestSensorsPermission().ConfigureAwait(false));
						break;
					case Permission.Speech:
						results.Add(permission, await RequestSpeechPermission().ConfigureAwait(false));
						break;
				}

				if (!results.ContainsKey(permission))
					results.Add(permission, PermissionStatus.Granted);
			}

			return results;
		}

		#region AV: Camera and Microphone

		PermissionStatus GetAVPermissionStatus(NSString mediaType)
		{
			var status = AVCaptureDevice.GetAuthorizationStatus(mediaType);
			switch (status)
			{
				case AVAuthorizationStatus.Authorized:
					return PermissionStatus.Granted;
				case AVAuthorizationStatus.Denied:
					return PermissionStatus.Denied;
				case AVAuthorizationStatus.Restricted:
					return PermissionStatus.Restricted;
				default:
					return PermissionStatus.Unknown;
			}
		}
		#endregion

		#region Contacts
		PermissionStatus ContactsPermissionStatus
		{
			get
			{
				var status = ABAddressBook.GetAuthorizationStatus();
				switch (status)
				{
					case ABAuthorizationStatus.Authorized:
						return PermissionStatus.Granted;
					case ABAuthorizationStatus.Denied:
						return PermissionStatus.Denied;
					case ABAuthorizationStatus.Restricted:
						return PermissionStatus.Restricted;
					default:
						return PermissionStatus.Unknown;
				}
			}
		}

		Task<PermissionStatus> RequestContactsPermission()
		{

			if (ContactsPermissionStatus != PermissionStatus.Unknown)
				return Task.FromResult(ContactsPermissionStatus);

			addressBook = new ABAddressBook();

			var tcs = new TaskCompletionSource<PermissionStatus>();


			addressBook.RequestAccess((success, error) =>
				{
					tcs.TrySetResult((success ? PermissionStatus.Granted : PermissionStatus.Denied));
				});

			return tcs.Task;
		}
		#endregion

		#region Events and Reminders
		PermissionStatus GetEventPermissionStatus(EKEntityType eventType)
		{
			var status = EKEventStore.GetAuthorizationStatus(eventType);
			switch (status)
			{
				case EKAuthorizationStatus.Authorized:
					return PermissionStatus.Granted;
				case EKAuthorizationStatus.Denied:
					return PermissionStatus.Denied;
				case EKAuthorizationStatus.Restricted:
					return PermissionStatus.Restricted;
				default:
					return PermissionStatus.Unknown;
			}

		}

		async Task<PermissionStatus> RequestEventPermission(EKEntityType eventType)
		{

			if (GetEventPermissionStatus(eventType) == PermissionStatus.Granted)
				return PermissionStatus.Granted;

			eventStore = new EKEventStore();

			var results = await eventStore.RequestAccessAsync(eventType).ConfigureAwait(false);

			return results.Item1 ? PermissionStatus.Granted : PermissionStatus.Denied;
		}
		#endregion

		#region Location

		Task<PermissionStatus> RequestLocationPermission(Permission permission = Permission.Location)
		{
			if (CLLocationManager.Status == CLAuthorizationStatus.AuthorizedWhenInUse && permission == Permission.LocationAlways)
			{
				//dont' do anything and request it
			}
			else if (GetLocationPermissionStatus(permission) != PermissionStatus.Unknown)
				return Task.FromResult(GetLocationPermissionStatus(permission));

			if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				return Task.FromResult(PermissionStatus.Unknown);
			}

			locationManager = new CLLocationManager();

			EventHandler<CLAuthorizationChangedEventArgs> authCallback = null;
			var tcs = new TaskCompletionSource<PermissionStatus>();

			authCallback = (sender, e) =>
				{
					if (e.Status == CLAuthorizationStatus.NotDetermined)
						return;

					locationManager.AuthorizationChanged -= authCallback;

					tcs.TrySetResult(GetLocationPermissionStatus(permission));

				};

			locationManager.AuthorizationChanged += authCallback;


			var info = NSBundle.MainBundle.InfoDictionary;


			if (permission == Permission.Location)
			{
				if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
					locationManager.RequestAlwaysAuthorization();
				else if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
					locationManager.RequestWhenInUseAuthorization();
				else
					throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
			}
			else if (permission == Permission.LocationAlways)
			{
				if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
					locationManager.RequestAlwaysAuthorization();
				else
					throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");

			}
			else
			{
				if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
					locationManager.RequestWhenInUseAuthorization();
				else
					throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");

			}

			return tcs.Task;
		}

		PermissionStatus GetLocationPermissionStatus(Permission permission)
		{

			if (!CLLocationManager.LocationServicesEnabled)
				return PermissionStatus.Disabled;

			var status = CLLocationManager.Status;

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				//if checking for always then check to see if we have it really, else denied
				if (permission == Permission.LocationAlways)
				{
					switch (status)
					{
						case CLAuthorizationStatus.AuthorizedAlways:
							return PermissionStatus.Granted;
						case CLAuthorizationStatus.AuthorizedWhenInUse:
						case CLAuthorizationStatus.Denied:
							return PermissionStatus.Denied;
						case CLAuthorizationStatus.Restricted:
							return PermissionStatus.Restricted;
						default:
							return PermissionStatus.Unknown;
					}
				}

				switch (status)
				{
					case CLAuthorizationStatus.AuthorizedAlways:
					case CLAuthorizationStatus.AuthorizedWhenInUse:
						return PermissionStatus.Granted;
					case CLAuthorizationStatus.Denied:
						return PermissionStatus.Denied;
					case CLAuthorizationStatus.Restricted:
						return PermissionStatus.Restricted;
					default:
						return PermissionStatus.Unknown;
				}
			}

			switch (status)
			{
				case CLAuthorizationStatus.Authorized:
					return PermissionStatus.Granted;
				case CLAuthorizationStatus.Denied:
					return PermissionStatus.Denied;
				case CLAuthorizationStatus.Restricted:
					return PermissionStatus.Restricted;
				default:
					return PermissionStatus.Unknown;
			}



		}
		#endregion

		#region Notifications
		/*PermissionStatus NotificationLocalPermissionState
        {
            get
            {
                var currentSettings = UIApplication.SharedApplication.CurrentUserNotificationSettings;

                if (currentSettings == null || notificationLocalSettings.Types == UIUserNotificationType.None)
                {
                    return PermissionStatus.Denied;
                }

                return PermissionStatus.Granted;
            }
        }

        Task<PermissionStatus> RequestNotificationLocalPermission()
        {
            if (NotificationLocalPermissionState == PermissionStatus.Granted)
                return Task.FromResult(PermissionStatus.Granted);

            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("DidRegisterUserNotificationSettings")
        }*/
		#endregion

		#region Photos
		PermissionStatus PhotosPermissionStatus
		{
			get
			{
				var status = PHPhotoLibrary.AuthorizationStatus;
				switch (status)
				{
					case PHAuthorizationStatus.Authorized:
						return PermissionStatus.Granted;
					case PHAuthorizationStatus.Denied:
						return PermissionStatus.Denied;
					case PHAuthorizationStatus.Restricted:
						return PermissionStatus.Restricted;
					default:
						return PermissionStatus.Unknown;
				}
			}
		}

		Task<PermissionStatus> RequestPhotosPermission()
		{

			if (PhotosPermissionStatus != PermissionStatus.Unknown)
				return Task.FromResult(PhotosPermissionStatus);

			var tcs = new TaskCompletionSource<PermissionStatus>();

			PHPhotoLibrary.RequestAuthorization(status =>
				{
					switch (status)
					{
						case PHAuthorizationStatus.Authorized:
							tcs.TrySetResult(PermissionStatus.Granted);
							break;
						case PHAuthorizationStatus.Denied:
							tcs.TrySetResult(PermissionStatus.Denied);
							break;
						case PHAuthorizationStatus.Restricted:
							tcs.TrySetResult(PermissionStatus.Restricted);
							break;
						default:
							tcs.TrySetResult(PermissionStatus.Unknown);
							break;
					}
				});

			return tcs.Task;
		}

		#endregion

		#region Sensors
		async Task<PermissionStatus> RequestSensorsPermission()
		{
			if (CMMotionActivityManager.IsActivityAvailable)
				return PermissionStatus.Granted;

			activityManager = new CMMotionActivityManager();

			try
			{
				var results = await activityManager.QueryActivityAsync(NSDate.DistantPast, NSDate.DistantFuture, NSOperationQueue.MainQueue).ConfigureAwait(false);
				if (results != null)
					return PermissionStatus.Granted;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to query activity manager: " + ex.Message);
				return PermissionStatus.Denied;
			}

			return PermissionStatus.Unknown;
		}
		#endregion

		#region Speech
		Task<PermissionStatus> RequestSpeechPermission()
		{
			if (SpeechPermissionStatus != PermissionStatus.Unknown)
				return Task.FromResult(SpeechPermissionStatus);


			if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
			{
				return Task.FromResult(PermissionStatus.Unknown);
			}

			var tcs = new TaskCompletionSource<PermissionStatus>();

			SFSpeechRecognizer.RequestAuthorization(status =>
			{
				switch (status)
				{
					case SFSpeechRecognizerAuthorizationStatus.Authorized:
						tcs.TrySetResult(PermissionStatus.Granted);
						break;
					case SFSpeechRecognizerAuthorizationStatus.Denied:
						tcs.TrySetResult(PermissionStatus.Denied);
						break;
					case SFSpeechRecognizerAuthorizationStatus.Restricted:
						tcs.TrySetResult(PermissionStatus.Restricted);
						break;
					default:
						tcs.TrySetResult(PermissionStatus.Unknown);
						break;
				}
			});
			return tcs.Task;
		}



		PermissionStatus SpeechPermissionStatus
		{
			get
			{
				var status = SFSpeechRecognizer.AuthorizationStatus;
				switch (status)
				{
					case SFSpeechRecognizerAuthorizationStatus.Authorized:
						return PermissionStatus.Granted;
					case SFSpeechRecognizerAuthorizationStatus.Denied:
						return PermissionStatus.Denied;
					case SFSpeechRecognizerAuthorizationStatus.Restricted:
						return PermissionStatus.Restricted;
					default:
						return PermissionStatus.Unknown;
				}
			}
		}
		#endregion

		#region MediaLib
		PermissionStatus MediaLibraryPermissionStatus
		{
			get
			{
				//Opening settings only open in iOS 9.3+
				if (!UIDevice.CurrentDevice.CheckSystemVersion(9, 3))
					return PermissionStatus.Unknown;

				var status = MPMediaLibrary.AuthorizationStatus;
				switch (status)
				{
					case MPMediaLibraryAuthorizationStatus.Authorized:
						return PermissionStatus.Granted;
					case MPMediaLibraryAuthorizationStatus.Denied:
						return PermissionStatus.Denied;
					case MPMediaLibraryAuthorizationStatus.Restricted:
						return PermissionStatus.Restricted;
					default:
						return PermissionStatus.Unknown;
				}
			}
		}

		Task<PermissionStatus> RequestMediaLibraryPermission()
		{

			//Opening settings only open in iOS 9.3+
			if (!UIDevice.CurrentDevice.CheckSystemVersion(9, 3))
				return Task.FromResult(PermissionStatus.Unknown);

			if (MediaLibraryPermissionStatus != PermissionStatus.Unknown)
				return Task.FromResult(MediaLibraryPermissionStatus);

			var tcs = new TaskCompletionSource<PermissionStatus>();

			MPMediaLibrary.RequestAuthorization(status =>
			{
				switch (status)
				{
					case MPMediaLibraryAuthorizationStatus.Authorized:
						tcs.TrySetResult(PermissionStatus.Granted);
						break;
					case MPMediaLibraryAuthorizationStatus.Denied:
						tcs.TrySetResult(PermissionStatus.Denied);
						break;
					case MPMediaLibraryAuthorizationStatus.Restricted:
						tcs.TrySetResult(PermissionStatus.Restricted);
						break;
					default:
						tcs.TrySetResult(PermissionStatus.Unknown);
						break;
				}
			});

			return tcs.Task;
		}

		#endregion

		public bool OpenAppSettings()
		{
			//Opening settings only open in iOS 8+
			if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
				return false;

			try
			{
				UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
