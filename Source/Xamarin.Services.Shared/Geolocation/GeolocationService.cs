using System;

namespace Xamarin.Services.Geolocation
{
	public partial class GeolocationService
#if INCLUDE_INTERFACES
		: IGeolocationService
#endif
	{
		public event EventHandler<PositionErrorEventArgs> PositionError;

		public event EventHandler<PositionEventArgs> PositionChanged;

		protected virtual void OnPositionError(PositionErrorEventArgs e)
			=> PositionError?.Invoke(this, e);

		protected virtual void OnPositionChanged(PositionEventArgs e)
			=> PositionChanged?.Invoke(this, e);
	}
}
