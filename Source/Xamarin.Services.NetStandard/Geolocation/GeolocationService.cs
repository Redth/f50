using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Xamarin.Services.Geolocation
{
    /// <summary>
    /// Implementation for Geolocator
    /// </summary>
    public class GeolocationService
#if !EXCLUDE_INTERFACES
        : IGeolocationService
#endif
    {
        /// <summary>
        /// Constructor for implementation
        /// </summary>
        public GeolocationService() => throw new NotImplementedException();

        /// <summary>
        /// Position error event handler
        /// </summary>
        public event EventHandler<PositionErrorEventArgs> PositionError;

        /// <summary>
        /// Position changed event handler
        /// </summary>
        public event EventHandler<PositionEventArgs> PositionChanged;

        /// <summary>
        /// Desired accuracy in meters
        /// </summary>
        public double DesiredAccuracy
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Gets if you are listening for location changes
        ///
        public bool IsListening => throw new NotImplementedException();

        /// <summary>
        /// Gets if device supports heading
        /// </summary>
        public bool IsHeadingSupported => throw new NotImplementedException();

        /// <summary>
        /// Gets if geolocation is available on device
        /// </summary>
        public bool IsSupported => throw new NotImplementedException();

        /// <summary>
        /// Gets if geolocation is enabled on device
        /// </summary>
        public bool IsEnabled => throw new NotImplementedException();

        /// <summary>
        /// Gets the last known and most accurate location.
        /// This is usually cached and best to display first before querying for full position.
        /// </summary>
        /// <returns>Best and most recent location or null if none found</returns>
        public async Task<Position> GetLastKnownLocationAsync() => throw new NotImplementedException();

        /// <summary>
        /// Gets position async with specified parameters
        /// </summary>
        /// <param name="timeout">Timeout to wait, Default Infinite</param>
        /// <param name="cancelToken">Cancelation token</param>
        /// <param name="includeHeading">If you would like to include heading</param>
        /// <returns>Position</returns>
        public async Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false) => throw new NotImplementedException();

        /// <summary>
        /// Retrieve positions for address.
        /// </summary>
        /// <param name="address">Desired address</param>
        /// <param name="mapKey">Map Key required only on UWP</param>
        /// <returns>Positions of the desired address</returns>
        public async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null) => throw new NotImplementedException();

        /// <summary>
        /// Retrieve addresses for position.
        /// </summary>
        /// <param name="position">Desired position (latitude and longitude)</param>
        /// <returns>Addresses of the desired position</returns>
        public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null) => throw new NotImplementedException();

        /// <summary>
        /// Start listening for changes
        /// </summary>
        /// <param name="minimumTime">Time</param>
        /// <param name="minimumDistance">Distance</param>
        /// <param name="includeHeading">Include heading or not</param>
        /// <param name="listenerSettings">Optional settings (iOS only)</param>
        public async Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null) => throw new NotImplementedException();

        /// <summary>
        /// Stop listening
        /// </summary>
        public Task<bool> StopListeningAsync() => throw new NotImplementedException();
    }
}
