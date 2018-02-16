using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Services.Geolocation
{
#if INCLUDE_INTERFACES
	public interface IGeolocationService
	{
		event EventHandler<PositionErrorEventArgs> PositionError;

		event EventHandler<PositionEventArgs> PositionChanged;

		double DesiredAccuracy { get; set; }

		bool IsListening { get; }

		bool IsHeadingSupported { get; }

		bool IsSupported { get; }

		bool IsEnabled { get; }

		Task<Position> GetLastKnownLocationAsync();

		Task<Position> GetPositionAsync(TimeSpan? timeout = null, CancellationToken? token = null, bool includeHeading = false);

		Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null);

		Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null);

		Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null);

		Task<bool> StopListeningAsync();
	}
#endif
}
