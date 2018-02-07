using System;

namespace Xamarin.Services.Settings
{
	public partial class SettingsService
#if INCLUDE_INTERFACES
		: ISettingsService
#endif
	{
		public decimal GetValueOrDefault(string key, decimal defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public bool GetValueOrDefault(string key, bool defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public long GetValueOrDefault(string key, long defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public string GetValueOrDefault(string key, string defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public int GetValueOrDefault(string key, int defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public float GetValueOrDefault(string key, float defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public DateTime GetValueOrDefault(string key, DateTime defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public Guid GetValueOrDefault(string key, Guid defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public double GetValueOrDefault(string key, double defaultValue, string fileName = null)
			=> GetValueOrDefaultInternal(key, defaultValue, fileName);

		public bool AddOrUpdateValue(string key, decimal value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, bool value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, long value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, string value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, int value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, float value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, DateTime value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, Guid value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);

		public bool AddOrUpdateValue(string key, double value, string fileName = null)
			=> AddOrUpdateValueInternal(key, value, fileName);
	}
}
