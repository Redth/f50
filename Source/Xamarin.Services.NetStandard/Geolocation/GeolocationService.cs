using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Xamarin.Services.Geolocation
{
	public class GeolocationService
#if !EXCLUDE_INTERFACES
		: IGeolocationService
#endif
	{
		public GeolocationService() => throw new NotImplementedException();

		public event EventHandler<PositionErrorEventArgs> PositionError;

		public event EventHandler<PositionEventArgs> PositionChanged;

		public double DesiredAccuracy
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public bool IsListening => throw new NotImplementedException();

		public bool IsHeadingSupported => throw new NotImplementedException();

		public bool IsSupported => throw new NotImplementedException();

		public bool IsEnabled => throw new NotImplementedException();

		public async Task<Position> GetLastKnownLocationAsync() => throw new NotImplementedException();

		public async Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false) => throw new NotImplementedException();

		public async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null) => throw new NotImplementedException();

		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null) => throw new NotImplementedException();

		public async Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null) => throw new NotImplementedException();

		public Task<bool> StopListeningAsync() => throw new NotImplementedException();
	}
}
