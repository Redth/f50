using Android.Hardware;
using Android.Content;
using Android.App;
using System;
using System.Collections.Generic;

namespace Xamarin.Services.DeviceMotion
{
	public partial class DeviceMotionService
	{
		private SensorEventListener sensorListener;
		private SensorManager sensorManager;
		private Sensor sensorAccelerometer;
		private Sensor sensorGyroscope;
		private Sensor sensorMagnetometer;
		private Sensor sensorCompass;

		private IDictionary<MotionSensorType, bool> sensorStatus;

		public DeviceMotionService()
		{
			sensorManager = (SensorManager)Application.Context.GetSystemService(Context.SensorService);
			sensorListener = new SensorEventListener(OnSensorValueChanged);
			sensorAccelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
			sensorGyroscope = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
			sensorMagnetometer = sensorManager.GetDefaultSensor(SensorType.MagneticField);
			sensorCompass = sensorManager.GetDefaultSensor(SensorType.Orientation);
			sensorStatus = new Dictionary<MotionSensorType, bool> {
				{ MotionSensorType.Accelerometer, false },
				{ MotionSensorType.Gyroscope, false },
				{ MotionSensorType.Magnetometer, false },
				{ MotionSensorType.Compass, false }
			};
		}

		public void Start(MotionSensorType sensorType, MotionSensorDelay interval = MotionSensorDelay.Default)
		{
			SensorDelay delay = SensorDelay.Normal;
			switch (interval)
			{
				case MotionSensorDelay.Fastest:
					delay = SensorDelay.Fastest;
					break;
				case MotionSensorDelay.Game:
					delay = SensorDelay.Game;
					break;
				case MotionSensorDelay.Ui:
					delay = SensorDelay.Ui;
					break;
			}

			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (sensorAccelerometer != null)
						sensorManager.RegisterListener(sensorListener, sensorAccelerometer, delay);
					else
						Console.WriteLine("Accelerometer not available");
					break;
				case MotionSensorType.Gyroscope:
					if (sensorGyroscope != null)
						sensorManager.RegisterListener(sensorListener, sensorGyroscope, delay);
					else
						Console.WriteLine("Gyroscope not available");
					break;
				case MotionSensorType.Magnetometer:
					if (sensorMagnetometer != null)
						sensorManager.RegisterListener(sensorListener, sensorMagnetometer, delay);
					else
						Console.WriteLine("Magnetometer not available");
					break;
				case MotionSensorType.Compass:
					if (sensorCompass != null)
						sensorManager.RegisterListener(sensorListener, sensorCompass, delay);
					else
						Console.WriteLine("Compass not available");
					break;
			}
			sensorStatus[sensorType] = true;
		}

		public void Stop(MotionSensorType sensorType)
		{
			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (sensorAccelerometer != null)
						sensorManager.UnregisterListener(sensorListener, sensorAccelerometer);
					else
						Console.WriteLine("Accelerometer not available");
					break;
				case MotionSensorType.Gyroscope:
					if (sensorGyroscope != null)
						sensorManager.UnregisterListener(sensorListener, sensorGyroscope);
					else
						Console.WriteLine("Gyroscope not available");
					break;
				case MotionSensorType.Magnetometer:
					if (sensorMagnetometer != null)
						sensorManager.UnregisterListener(sensorListener, sensorMagnetometer);
					else
						Console.WriteLine("Magnetometer not available");
					break;
				case MotionSensorType.Compass:
					if (sensorCompass != null)
						sensorManager.UnregisterListener(sensorListener, sensorCompass);
					else
						Console.WriteLine("Compass not available");
					break;
			}
			sensorStatus[sensorType] = false;
		}

		public bool IsActive(MotionSensorType sensorType)
		{
			return sensorStatus[sensorType];
		}

		partial void OnDispose(bool disposing)
		{
			if (disposing)
			{
				sensorListener?.Dispose();
				sensorListener = null;
			}
		}

		private class SensorEventListener : Java.Lang.Object, ISensorEventListener
		{
			private Action<SensorValueChangedEventArgs> OnSensorValueChanged;

			public SensorEventListener(Action<SensorValueChangedEventArgs> onSensorValueChanged)
			{
				OnSensorValueChanged = onSensorValueChanged;
			}

			protected override void Dispose(bool disposing)
			{
				OnSensorValueChanged = null;
				base.Dispose(disposing);
			}

			public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
			{
			}

			public void OnSensorChanged(SensorEvent e)
			{
				switch (e.Sensor.Type)
				{
					case SensorType.Accelerometer:
						OnSensorValueChanged(new SensorValueChangedEventArgs()
						{
							ValueType = MotionSensorValueType.Vector,
							SensorType = MotionSensorType.Accelerometer,
							Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] }
						});

						break;
					case SensorType.Gyroscope:
						OnSensorValueChanged(new SensorValueChangedEventArgs()
						{
							ValueType = MotionSensorValueType.Vector,
							SensorType = MotionSensorType.Gyroscope,
							Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] }
						});

						break;
					case SensorType.MagneticField:
						OnSensorValueChanged(new SensorValueChangedEventArgs()
						{
							ValueType = MotionSensorValueType.Vector,
							SensorType = MotionSensorType.Magnetometer,
							Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] }
						});

						break;
					case SensorType.Orientation:
						OnSensorValueChanged(new SensorValueChangedEventArgs()
						{
							ValueType = MotionSensorValueType.Single,
							SensorType = MotionSensorType.Compass,
							Value = new MotionValue() { Value = e.Values[0] }
						});
						break;
				}
			}
		}
	}
}
