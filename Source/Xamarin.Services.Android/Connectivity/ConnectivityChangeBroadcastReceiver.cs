using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;

namespace Xamarin.Services.Connectivity
{
	[BroadcastReceiver(Enabled = true, Label = "Connectivity Plugin Broadcast Receiver")]
	//[IntentFilter(new[] { "android.net.conn.CONNECTIVITY_CHANGE" })]
	[global::Android.Runtime.Preserve(AllMembers = true)]
	public class ConnectivityChangeBroadcastReceiver : BroadcastReceiver
	{
		public static Action<ConnectivityChangedEventArgs> ConnectionChanged;

		public static Action<ConnectivityTypeChangedEventArgs> ConnectionTypeChanged;

		private bool isConnected;
		private ConnectivityManager connectivityManager;

		public ConnectivityChangeBroadcastReceiver()
		{
			isConnected = IsConnected;
		}

		ConnectivityManager ConnectivityManager
		{
			get
			{
				if (connectivityManager == null || connectivityManager.Handle == IntPtr.Zero)
					connectivityManager = (ConnectivityManager)(Application.Context.GetSystemService(Context.ConnectivityService));

				return connectivityManager;
			}
		}

		bool IsConnected => ConnectivityManager.GetIsConnected();

		public override async void OnReceive(Context context, Intent intent)
		{
			if (intent.Action != ConnectivityManager.ConnectivityAction)
				return;

			//await 500ms to ensure that the the connection manager updates
			await Task.Delay(500);

			var connectionChangedAction = ConnectionChanged;
			var newConnection = IsConnected;
			if (connectionChangedAction != null)
			{
				if (newConnection != isConnected)
				{
					isConnected = newConnection;

					connectionChangedAction(new ConnectivityChangedEventArgs { IsConnected = isConnected });
				}
			}

			var connectionTypeChangedAction = ConnectionTypeChanged;
			if (connectionTypeChangedAction != null)
			{

				var connectionTypes = ConnectivityManager.GetConnectionTypes();

				connectionTypeChangedAction(new ConnectivityTypeChangedEventArgs { IsConnected = newConnection, ConnectionTypes = connectionTypes });
			}
		}
	}
}
