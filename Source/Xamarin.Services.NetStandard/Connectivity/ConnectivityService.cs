using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Connectivity
{
	public class ConnectivityService
#if !EXCLUDE_INTERFACES
		: IConnectivityService
#endif
	{
		public bool IsConnected => throw new NotImplementedException();

		public IEnumerable<ConnectionType> ConnectionTypes => throw new NotImplementedException();

		public IEnumerable<ulong> Bandwidths => throw new NotImplementedException();

		public event ConnectivityChangedEventHandler ConnectivityChanged;
		public event ConnectivityTypeChangedEventHandler ConnectivityTypeChanged;

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsReachable(string host, int msTimeout = 5000)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000)
		{
			throw new NotImplementedException();
		}
	}
}
