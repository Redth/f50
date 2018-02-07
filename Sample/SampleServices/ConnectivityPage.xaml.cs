using Xamarin.Forms;
using Xamarin.Services.Connectivity;

namespace SampleServices
{
	public partial class ConnectivityPage : ContentPage
	{
		public ConnectivityPage()
		{
			InitializeComponent();

			var connectivity = new Xamarin.Services.Connectivity.ConnectivityService();

			// subscribe to connectivity updates
			connectivity.ConnectivityTypeChanged += (_, e) => { UpdateText(e); };

			// get current connectivity
			UpdateText(new ConnectivityTypeChangedEventArgs
			{
				IsConnected = connectivity.IsConnected,
				ConnectionTypes = connectivity.ConnectionTypes
			});
		}

		private void UpdateText(ConnectivityTypeChangedEventArgs e)
		{
			var connected = e.IsConnected ? "Connected" : "Not connected";
			var type = string.Join(", ", e.ConnectionTypes);

			labelConnectivity.Text = $"{connected}: {type}";
		}
	}
}
