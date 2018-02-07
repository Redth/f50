using Windows.Storage;
using System;
using System.Diagnostics;

namespace Xamarin.Services.Settings
{
	public partial class SettingsService
	{
		private readonly object locker = new object();

		public void Remove(string key, string fileName = null)
		{
			lock (locker)
			{
				var settings = GetAppSettings(fileName);
				// If the key exists remove
				if (settings.Values.ContainsKey(key))
				{
					settings.Values.Remove(key);
				}
			}
		}

		public void Clear(string fileName = null)
		{
			lock (locker)
			{
				try
				{
					var settings = GetAppSettings(fileName);
					settings.Values.Clear();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to clear all defaults. Message: " + ex.Message);
				}
			}
		}

		public bool Contains(string key, string fileName = null)
		{
			lock (locker)
			{
				try
				{
					var settings = GetAppSettings(fileName);
					return settings.Values.ContainsKey(key);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to check " + key + " Message: " + ex.Message);
				}

				return false;
			}
		}

		public bool OpenAppSettings() => false;

		private ApplicationDataContainer GetAppSettings(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				return ApplicationData.Current.LocalSettings;

			if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(fileName))
				ApplicationData.Current.LocalSettings.CreateContainer(fileName, ApplicationDataCreateDisposition.Always);

			return ApplicationData.Current.LocalSettings.Containers[fileName];
		}

		private T GetValueOrDefaultInternal<T>(string key, T defaultValue, string fileName)
		{
			object value;
			lock (locker)
			{
				var settings = GetAppSettings(fileName);

				if (typeof(T) == typeof(decimal))
				{
					string savedDecimal;
					// If the key exists, retrieve the value.
					if (settings.Values.ContainsKey(key))
					{
						savedDecimal = Convert.ToString(settings.Values[key]);
					}
					// Otherwise, use the default value.
					else
					{
						savedDecimal = defaultValue == null ? default(decimal).ToString() : defaultValue.ToString();
					}

					value = Convert.ToDecimal(savedDecimal, System.Globalization.CultureInfo.InvariantCulture);

					return null != value ? (T)value : defaultValue;
				}
				else if (typeof(T) == typeof(DateTime))
				{
					string savedTime = null;
					// If the key exists, retrieve the value.
					if (settings.Values.ContainsKey(key))
					{
						savedTime = Convert.ToString(settings.Values[key]);
					}

					if (string.IsNullOrWhiteSpace(savedTime))
					{
						value = defaultValue;
					}
					else
					{
						var ticks = Convert.ToInt64(savedTime, System.Globalization.CultureInfo.InvariantCulture);
						if (ticks >= 0)
						{
							//Old value, stored before update to UTC values
							value = new DateTime(ticks);
						}
						else
						{
							//New value, UTC
							value = new DateTime(-ticks, DateTimeKind.Utc);
						}
					}

					return (T)value;
				}

				// If the key exists, retrieve the value.
				if (settings.Values.ContainsKey(key))
				{
					var tempValue = settings.Values[key];
					if (tempValue != null)
						value = (T)tempValue;
					else
						value = defaultValue;
				}
				// Otherwise, use the default value.
				else
				{
					value = defaultValue;
				}
			}

			return null != value ? (T)value : defaultValue;
		}

		private bool AddOrUpdateValueInternal<T>(string key, T value, string fileName)
		{
			if (value == null)
			{
				Remove(key, fileName);
				return true;
			}

			return AddOrUpdateValueCore(key, value, fileName);
		}

		private bool AddOrUpdateValueCore(string key, object value, string fileName)
		{
			bool valueChanged = false;
			lock (locker)
			{
				var settings = GetAppSettings(fileName);
				if (value is decimal)
				{
					return AddOrUpdateValueInternal(key, Convert.ToString(Convert.ToDecimal(value), System.Globalization.CultureInfo.InvariantCulture), fileName);
				}
				else if (value is DateTime)
				{
					return AddOrUpdateValueInternal(key, Convert.ToString(-(Convert.ToDateTime(value)).ToUniversalTime().Ticks, System.Globalization.CultureInfo.InvariantCulture), fileName);
				}

				// If the key exists
				if (settings.Values.ContainsKey(key))
				{

					// If the value has changed
					if (settings.Values[key] != value)
					{
						// Store key new value
						settings.Values[key] = value;
						valueChanged = true;
					}
				}
				// Otherwise create the key.
				else
				{
					//settings.CreateContainer(key, ApplicationDataCreateDisposition.Always);
					settings.Values[key] = value;
					valueChanged = true;
				}
			}

			return valueChanged;
		}
	}
}
