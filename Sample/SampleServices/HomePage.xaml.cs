using System;
using Xamarin.Forms;

namespace SampleServices
{
	public partial class HomePage : ContentPage
	{
		public HomePage()
		{
			InitializeComponent();

			samples.ItemsSource = new[] {
				new SampleItem(
					"Text-to-Speech", typeof(TextToSpeechPage),
					"Demonstrates how easy it is to get your device to talk."),
				new SampleItem(
					"Geolocation", typeof(GeolocationPage),
					"Demonstrates how easy it is for your device to get your current location."),
				new SampleItem(
					"Connectivity", typeof(ConnectivityPage),
					"Demonstrates how easy it is to check and track your internet connectivity."),
				new SampleItem(
					"Settings", typeof(SettingsPage),
					"Demonstrates how easy it is to read and save app settings."),
			};
		}

		private async void OnSampleSelected(object sender, SelectedItemChangedEventArgs e)
		{
			var item = e.SelectedItem as SampleItem;

			if (item == null)
				return;

			var page = Activator.CreateInstance(item.SamplePage) as Page;

			if (page != null)
				await Navigation.PushAsync(page);

			samples.SelectedItem = null;
		}

		private class SampleItem
		{
			public SampleItem(string name, Type samplePage, string description)
			{
				Name = name;
				Description = description;
				SamplePage = samplePage;
			}

			public string Name { get; set; }

			public string Description { get; set; }

			public Type SamplePage { get; set; }
		}
	}
}
