using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Runtime;

namespace Xamarin.Services.Settings
{
	public partial class SettingsService
	{
		private readonly object locker = new object();

		public void Remove(string key, string fileName = null)
		{
			if (Application.Context == null)
				return;

			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreference(fileName))
				{
					using (var sharedPreferencesEditor = sharedPreferences.Edit())
					{
						sharedPreferencesEditor.Remove(key);
						sharedPreferencesEditor.Commit();
					}
				}
			}
		}

		public void Clear(string fileName = null)
		{
			if (Application.Context == null)
				return;

			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreference(fileName))
				{
					using (var sharedPreferencesEditor = sharedPreferences.Edit())
					{
						sharedPreferencesEditor.Clear();
						sharedPreferencesEditor.Commit();
					}
				}
			}
		}

		public bool Contains(string key, string fileName = null)
		{
			if (Application.Context == null)
				return false;

			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreference(fileName))
				{
					if (sharedPreferences == null)
						return false;

					return sharedPreferences.Contains(key);
				}
			}
		}

		public bool OpenAppSettings()
		{
			var context = Application.Context;
			if (context == null)
				return false;

			try
			{
				var settingsIntent = new Intent();
				settingsIntent.SetAction(global::Android.Provider.Settings.ActionApplicationDetailsSettings);
				settingsIntent.AddCategory(Intent.CategoryDefault);
				settingsIntent.SetData(global::Android.Net.Uri.Parse("package:" + context.PackageName));
				settingsIntent.AddFlags(ActivityFlags.NewTask);
				settingsIntent.AddFlags(ActivityFlags.ClearTask);
				settingsIntent.AddFlags(ActivityFlags.NoHistory);
				settingsIntent.AddFlags(ActivityFlags.ExcludeFromRecents);
				context.StartActivity(settingsIntent);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private T GetValueOrDefaultInternal<T>(string key, T defaultValue, string fileName)
		{
			if (Application.Context == null)
				return defaultValue;

			if (!Contains(key, fileName))
			{
				return defaultValue;
			}

			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreference(fileName))
				{
					return GetValueOrDefaultCore(sharedPreferences, key, defaultValue, fileName);
				}
			}
		}

		private T GetValueOrDefaultCore<T>(ISharedPreferences sharedPreferences, string key, T defaultValue, string fileName)
		{
			Type typeOf = typeof(T);
			if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				typeOf = Nullable.GetUnderlyingType(typeOf);
			}

			object value = null;
			var typeCode = Type.GetTypeCode(typeOf);
			bool resave = false;
			switch (typeCode)
			{
				case TypeCode.Decimal:
					//Android doesn't have decimal in shared prefs so get string and convert
					var savedDecimal = string.Empty;
					try
					{
						savedDecimal = sharedPreferences.GetString(key, string.Empty);
					}
					catch (Java.Lang.ClassCastException)
					{
						Console.WriteLine("Settings 1.5 change, have to remove key.");

						try
						{
							Console.WriteLine("Attempting to get old value.");
							savedDecimal = sharedPreferences.GetLong(key, (long)Convert.ToDecimal(defaultValue, CultureInfo.InvariantCulture)).ToString();
							Console.WriteLine("Old value has been parsed and will be updated and saved.");
						}
						catch (Java.Lang.ClassCastException)
						{
							Console.WriteLine("Could not parse old value, will be lost.");
						}
						Remove(key, fileName);
						resave = true;
					}
					if (string.IsNullOrWhiteSpace(savedDecimal))
						value = Convert.ToDecimal(defaultValue, CultureInfo.InvariantCulture);
					else
						value = Convert.ToDecimal(savedDecimal, CultureInfo.InvariantCulture);

					if (resave)
						AddOrUpdateValueInternal(key, value, null);

					break;
				case TypeCode.Boolean:
					value = sharedPreferences.GetBoolean(key, Convert.ToBoolean(defaultValue));
					break;
				case TypeCode.Int64:
					value = sharedPreferences.GetLong(key, Convert.ToInt64(defaultValue, CultureInfo.InvariantCulture));
					break;
				case TypeCode.String:
					value = sharedPreferences.GetString(key, Convert.ToString(defaultValue));
					break;
				case TypeCode.Double:
					//Android doesn't have double, so must get as string and parse.
					var savedDouble = string.Empty;
					try
					{
						savedDouble = sharedPreferences.GetString(key, string.Empty);
					}
					catch (Java.Lang.ClassCastException)
					{
						Console.WriteLine("Settings 1.5  change, have to remove key.");

						try
						{
							Console.WriteLine("Attempting to get old value.");
							savedDouble =
								sharedPreferences.GetLong(key,
									(long)Convert.ToDouble(defaultValue, CultureInfo.InvariantCulture))
									.ToString();

							Console.WriteLine("Old value has been parsed and will be updated and saved.");
						}
						catch (Java.Lang.ClassCastException)
						{
							Console.WriteLine("Could not parse old value, will be lost.");
						}
						Remove(key, fileName);
						resave = true;
					}

					if (string.IsNullOrWhiteSpace(savedDouble))
						value = defaultValue;
					else
					{

						if (!double.TryParse(savedDouble, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double outDouble))
						{
							var maxString = Convert.ToString(double.MaxValue, CultureInfo.InvariantCulture);
							outDouble = savedDouble.Equals(maxString) ? double.MaxValue : double.MinValue;
						}

						value = outDouble;
					}

					if (resave)
						AddOrUpdateValueInternal(key, value, fileName);

					break;
				case TypeCode.Int32:
					value = sharedPreferences.GetInt(key, Convert.ToInt32(defaultValue, CultureInfo.InvariantCulture));
					break;
				case TypeCode.Single:
					value = sharedPreferences.GetFloat(key, Convert.ToSingle(defaultValue, CultureInfo.InvariantCulture));
					break;
				case TypeCode.DateTime:
					if (sharedPreferences.Contains(key))
					{
						var ticks = sharedPreferences.GetLong(key, 0);
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
					else
					{
						return defaultValue;
					}
					break;
				default:

					if (defaultValue is Guid)
					{
						var outGuid = Guid.Empty;
						Guid.TryParse(sharedPreferences.GetString(key, Guid.Empty.ToString()), out outGuid);
						value = outGuid;
					}
					else
					{
						throw new ArgumentException($"Value of type {typeCode} is not supported.");
					}

					break;
			}

			return null != value ? (T)value : defaultValue;
		}

		private bool AddOrUpdateValueInternal<T>(string key, T value, string fileName)
		{
			if (Application.Context == null)
				return false;

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
				using (var sharedPreferences = GetSharedPreference(fileName))
				{
					using (var sharedPreferencesEditor = sharedPreferences.Edit())
					{
						switch (typeCode)
						{
							case TypeCode.Decimal:
								sharedPreferencesEditor.PutString(key,
									Convert.ToString(value, CultureInfo.InvariantCulture));
								break;
							case TypeCode.Boolean:
								sharedPreferencesEditor.PutBoolean(key, Convert.ToBoolean(value));
								break;
							case TypeCode.Int64:
								sharedPreferencesEditor.PutLong(key, (long)Convert.ToInt64(value, CultureInfo.InvariantCulture));
								break;
							case TypeCode.String:
								sharedPreferencesEditor.PutString(key, Convert.ToString(value));
								break;
							case TypeCode.Double:
								var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
								sharedPreferencesEditor.PutString(key, valueString);
								break;
							case TypeCode.Int32:
								sharedPreferencesEditor.PutInt(key, Convert.ToInt32(value, CultureInfo.InvariantCulture));
								break;
							case TypeCode.Single:
								sharedPreferencesEditor.PutFloat(key,
									Convert.ToSingle(value, CultureInfo.InvariantCulture));
								break;
							case TypeCode.DateTime:
								sharedPreferencesEditor.PutLong(key, -(Convert.ToDateTime(value)).ToUniversalTime().Ticks);
								break;
							default:
								if (value is Guid)
								{
									sharedPreferencesEditor.PutString(key, ((Guid)value).ToString());
								}
								else
								{
									throw new ArgumentException($"Value of type {typeCode} is not supported.");
								}
								break;
						}

						sharedPreferencesEditor.Commit();
					}
				}
			}

			return true;
		}

		private ISharedPreferences GetSharedPreference(string fileName)
		{
			return string.IsNullOrWhiteSpace(fileName) ?
				PreferenceManager.GetDefaultSharedPreferences(Application.Context) :
				Application.Context.GetSharedPreferences(fileName, FileCreationMode.Private);
		}
	}
}
