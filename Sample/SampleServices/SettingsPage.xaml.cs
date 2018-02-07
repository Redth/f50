using System;
using Xamarin.Forms;
using Xamarin.Services.Settings;

namespace SampleServices
{
	public partial class SettingsPage : ContentPage
	{
		Xamarin.Services.Settings.SettingsService settings;

		public SettingsPage()
		{
			InitializeComponent();

			settings = new Xamarin.Services.Settings.SettingsService();

			settingText.Text = settings.GetValueOrDefault("TheSettingKey", string.Empty);
		}

		private void SaveSetting_Clicked(object sender, EventArgs e)
		{
			settings.AddOrUpdateValue("TheSettingKey", settingText.Text);
		}

		private void LoadSetting_Clicked(object sender, EventArgs e)
		{
			settingText.Text = settings.GetValueOrDefault("TheSettingKey", string.Empty);
		}
	}
}
