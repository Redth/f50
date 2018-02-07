using System;

namespace Xamarin.Services.Geolocation
{
	public class ListenerSettings
	{
		public bool AllowBackgroundUpdates { get; set; } = false;

		public bool PauseLocationUpdatesAutomatically { get; set; } = true;

		public ActivityType ActivityType { get; set; } = ActivityType.Other;

		public bool ListenForSignificantChanges { get; set; } = false;

		public bool DeferLocationUpdates { get; set; } = false;

		public double? DeferralDistanceMeters { get; set; } = 500;

		public TimeSpan? DeferralTime { get; set; } = TimeSpan.FromMinutes(5);
	}
}
