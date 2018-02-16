using System;

namespace Xamarin.Services.Settings
{
#if INCLUDE_INTERFACES
	public interface ISettingsService
	{
		Decimal GetValueOrDefault(string key, Decimal defaultValue, string fileName = null);

		Boolean GetValueOrDefault(string key, Boolean defaultValue, string fileName = null);

		Int64 GetValueOrDefault(string key, Int64 defaultValue, string fileName = null);

		String GetValueOrDefault(string key, String defaultValue, string fileName = null);

		Int32 GetValueOrDefault(string key, Int32 defaultValue, string fileName = null);

		Single GetValueOrDefault(string key, Single defaultValue, string fileName = null);

		DateTime GetValueOrDefault(string key, DateTime defaultValue, string fileName = null);

		Guid GetValueOrDefault(string key, Guid defaultValue, string fileName = null);

		Double GetValueOrDefault(string key, Double defaultValue, string fileName = null);

		bool AddOrUpdateValue(string key, Decimal value, string fileName = null);

		bool AddOrUpdateValue(string key, Boolean value, string fileName = null);

		bool AddOrUpdateValue(string key, Int64 value, string fileName = null);

		bool AddOrUpdateValue(string key, String value, string fileName = null);

		bool AddOrUpdateValue(string key, Int32 value, string fileName = null);

		bool AddOrUpdateValue(string key, Single value, string fileName = null);

		bool AddOrUpdateValue(string key, DateTime value, string fileName = null);

		bool AddOrUpdateValue(string key, Guid value, string fileName = null);

		bool AddOrUpdateValue(string key, Double value, string fileName = null);

		void Remove(string key, string fileName = null);

		void Clear(string fileName = null);

		bool Contains(string key, string fileName = null);

		bool OpenAppSettings();
	}
#endif
}
