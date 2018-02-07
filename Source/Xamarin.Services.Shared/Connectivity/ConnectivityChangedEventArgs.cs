using System;

namespace Xamarin.Services.Connectivity
{
	public class ConnectivityChangedEventArgs : EventArgs
	{
		public bool IsConnected { get; set; }
	}
}
