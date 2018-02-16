using System;

namespace Xamarin.Services.Settings
{
	public partial class SettingsService
	{
		private bool AddOrUpdateValueInternal<T>(string key, T value, string fileName = null)
			=> throw new NotImplementedException();

		private T GetValueOrDefaultInternal<T>(string key, T defaultValue = default(T), string fileName = null)
			=> throw new NotImplementedException();

		public void Clear(string fileName = null)
			=> throw new NotImplementedException();

		public bool Contains(string key, string fileName = null)
			=> throw new NotImplementedException();

		public bool OpenAppSettings()
			=> throw new NotImplementedException();

		public void Remove(string key, string fileName = null)
			=> throw new NotImplementedException();
	}
}
