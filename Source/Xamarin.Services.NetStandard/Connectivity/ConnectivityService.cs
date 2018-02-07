using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Connectivity
{
	public partial class ConnectivityService
	{
		public bool IsConnected => throw new NotImplementedException();

		public IEnumerable<ConnectionType> ConnectionTypes => throw new NotImplementedException();

		public IEnumerable<ulong> Bandwidths => throw new NotImplementedException();

		private void OnDispose(bool disposing) => throw new NotImplementedException();

		public Task<bool> IsReachable(string host, int msTimeout = 5000) => throw new NotImplementedException();

		public Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000) => throw new NotImplementedException();
	}
}
