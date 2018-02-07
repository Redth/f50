using System;

namespace Xamarin.Services.Settings
{
	public class SettingsService
#if !EXCLUDE_INTERFACES
		: ISettingsService
#endif
	{
		public bool AddOrUpdateValue(string key, decimal value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, bool value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, long value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, string value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, int value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, float value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, DateTime value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, Guid value, string fileName = null) => throw new NotImplementedException();

		public bool AddOrUpdateValue(string key, double value, string fileName = null) => throw new NotImplementedException();

		public void Clear(string fileName = null) => throw new NotImplementedException();

		public bool Contains(string key, string fileName = null) => throw new NotImplementedException();

		public decimal GetValueOrDefault(string key, decimal defaultValue, string fileName = null) => throw new NotImplementedException();

		public bool GetValueOrDefault(string key, bool defaultValue, string fileName = null) => throw new NotImplementedException();

		public long GetValueOrDefault(string key, long defaultValue, string fileName = null) => throw new NotImplementedException();

		public string GetValueOrDefault(string key, string defaultValue, string fileName = null) => throw new NotImplementedException();

		public int GetValueOrDefault(string key, int defaultValue, string fileName = null) => throw new NotImplementedException();

		public float GetValueOrDefault(string key, float defaultValue, string fileName = null) => throw new NotImplementedException();

		public DateTime GetValueOrDefault(string key, DateTime defaultValue, string fileName = null) => throw new NotImplementedException();

		public Guid GetValueOrDefault(string key, Guid defaultValue, string fileName = null) => throw new NotImplementedException();

		public double GetValueOrDefault(string key, double defaultValue, string fileName = null) => throw new NotImplementedException();

		public bool OpenAppSettings() => throw new NotImplementedException();

		public void Remove(string key, string fileName = null) => throw new NotImplementedException();
	}
}
