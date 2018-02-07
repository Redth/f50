using System;
using System.Collections.Generic;

namespace Xamarin.Services.Connectivity
{
	public class ConnectivityTypeChangedEventArgs : EventArgs
	{
		public bool IsConnected { get; set; }

		public IEnumerable<ConnectionType> ConnectionTypes { get; set; }
	}
}
