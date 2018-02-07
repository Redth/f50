using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Xamarin.Services.Geolocation
{
	public partial class GeolocationService
	{
		public GeolocationService() => throw new NotImplementedException();

		public double DesiredAccuracy
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public bool IsListening => throw new NotImplementedException();

		public bool IsHeadingSupported => throw new NotImplementedException();

		public bool IsSupported => throw new NotImplementedException();

		public bool IsEnabled => throw new NotImplementedException();

		public Task<Position> GetLastKnownLocationAsync() => throw new NotImplementedException();

		public Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false) => throw new NotImplementedException();

		public Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null) => throw new NotImplementedException();

		public Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null) => throw new NotImplementedException();

		public Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null) => throw new NotImplementedException();

		public Task<bool> StopListeningAsync() => throw new NotImplementedException();
	}
}
