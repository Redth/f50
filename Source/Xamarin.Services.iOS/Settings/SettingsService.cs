using System;
using Foundation;
using UIKit;

namespace Xamarin.Services.Settings
{
	public partial class SettingsService
	{
		private readonly object locker = new object();

		public void Remove(string key, string fileName = null)
		{
			lock (locker)
			{
				var defaults = GetUserDefaults(fileName);
				try
				{
					if (defaults[key] != null)
					{
						defaults.RemoveObject(key);
						defaults.Synchronize();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to remove: " + key, " Message: " + ex.Message);
				}
			}
		}

		public void Clear(string fileName = null)
		{
			lock (locker)
			{
				var defaults = GetUserDefaults(fileName);
				try
				{
					var items = defaults.ToDictionary();

					foreach (var item in items.Keys)
					{
						if (item is NSString nsString)
							defaults.RemoveObject(nsString);
					}
					defaults.Synchronize();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to clear all defaults. Message: " + ex.Message);
				}
			}
		}

		public bool Contains(string key, string fileName = null)
		{
			lock (locker)
			{
				var defaults = GetUserDefaults(fileName);
				try
				{
					var setting = defaults[key];
					return setting != null;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to clear all defaults. Message: " + ex.Message);
				}

				return false;
			}
		}

		public bool OpenAppSettings()
		{
			//Opening settings only open in iOS 8+
			if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
				return false;

			try
			{
				UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
				return true;
			}
			catch
			{
				return false;
			}
		}

		private T GetValueOrDefaultInternal<T>(string key, T defaultValue = default(T), string fileName = null)
		{
			lock (locker)
			{
				var defaults = GetUserDefaults(fileName);

				if (defaults[key] == null)
					return defaultValue;

				Type typeOf = typeof(T);
				if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					typeOf = Nullable.GetUnderlyingType(typeOf);
				}
				object value = null;
				var typeCode = Type.GetTypeCode(typeOf);
				switch (typeCode)
				{
					case TypeCode.Decimal:
						var savedDecimal = defaults.StringForKey(key);
						value = Convert.ToDecimal(savedDecimal, System.Globalization.CultureInfo.InvariantCulture);
						break;
					case TypeCode.Boolean:
						value = defaults.BoolForKey(key);
						break;
					case TypeCode.Int64:
						var savedInt64 = defaults.StringForKey(key);
						value = Convert.ToInt64(savedInt64, System.Globalization.CultureInfo.InvariantCulture);
						break;
					case TypeCode.Double:
						value = defaults.DoubleForKey(key);
						break;
					case TypeCode.String:
						value = defaults.StringForKey(key);
						break;
					case TypeCode.Int32:
						value = (Int32)defaults.IntForKey(key);
						break;
					case TypeCode.Single:
						value = defaults.FloatForKey(key);
						break;

					case TypeCode.DateTime:
						var savedTime = defaults.StringForKey(key);
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
						break;
					default:

						if (defaultValue is Guid)
						{
							var outGuid = Guid.Empty;
							var savedGuid = defaults.StringForKey(key);
							if (string.IsNullOrWhiteSpace(savedGuid))
							{
								value = outGuid;
							}
							else
							{
								Guid.TryParse(savedGuid, out outGuid);
								value = outGuid;
							}
						}
						else
						{
							throw new ArgumentException($"Value of type {typeCode} is not supported.");
						}

						break;
				}


				return null != value ? (T)value : defaultValue;
			}
		}

		private bool AddOrUpdateValueInternal<T>(string key, T value, string fileName = null)
		{
			if (value == null)
			{
				Remove(key, fileName);
				return true;
			}

			Type typeOf = typeof(T);
			if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				typeOf = Nullable.GetUnderlyingType(typeOf);
			}
			var typeCode = Type.GetTypeCode(typeOf);
			return AddOrUpdateValueCore(key, value, typeCode, fileName);
		}

		private bool AddOrUpdateValueCore(string key, object value, TypeCode typeCode, string fileName)
		{
			lock (locker)
			{
				var defaults = GetUserDefaults(fileName);
				switch (typeCode)
				{
					case TypeCode.Decimal:
						defaults.SetString(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), key);
						break;
					case TypeCode.Boolean:
						defaults.SetBool(Convert.ToBoolean(value), key);
						break;
					case TypeCode.Int64:
						defaults.SetString(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), key);
						break;
					case TypeCode.Double:
						defaults.SetDouble(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture), key);
						break;
					case TypeCode.String:
						defaults.SetString(Convert.ToString(value), key);
						break;
					case TypeCode.Int32:
						defaults.SetInt(Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture), key);
						break;
					case TypeCode.Single:
						defaults.SetFloat(Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture), key);
						break;
					case TypeCode.DateTime:
						defaults.SetString(Convert.ToString(-(Convert.ToDateTime(value)).ToUniversalTime().Ticks), key);
						break;
					default:
						if (value is Guid)
						{
							if (value == null)
								value = Guid.Empty;

							defaults.SetString(((Guid)value).ToString(), key);
						}
						else
						{
							throw new ArgumentException($"Value of type {typeCode} is not supported.");
						}
						break;
				}
				try
				{
					defaults.Synchronize();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to save: " + key, " Message: " + ex.Message);
				}
			}

			return true;
		}

		private NSUserDefaults GetUserDefaults(string fileName = null) =>
			string.IsNullOrWhiteSpace(fileName) ?
			NSUserDefaults.StandardUserDefaults :
			new NSUserDefaults(fileName, NSUserDefaultsType.SuiteName);
	}
}
