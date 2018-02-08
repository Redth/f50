using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Sensors;

namespace Xamarin.Services.DeviceMotion
{
	public partial class DeviceMotionService
	{
		private Accelerometer accelerometer;
		private Gyrometer gyrometer;
		private Compass compass;
		private Magnetometer magnetometer;

		private double ms = 1000.0;
		private Dictionary<MotionSensorType, bool> sensorStatus;

		public DeviceMotionService()
		{
			try
			{
				accelerometer = Accelerometer.GetDefault();
				gyrometer = Gyrometer.GetDefault();
				compass = Compass.GetDefault();
				magnetometer = Magnetometer.GetDefault();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			sensorStatus = new Dictionary<MotionSensorType, bool>()
			{
				{MotionSensorType.Accelerometer, false},
				{MotionSensorType.Gyroscope, false},
				{MotionSensorType.Magnetometer, false},
				{MotionSensorType.Compass, false}
			};
		}

		public void Start(MotionSensorType sensorType, MotionSensorDelay interval = MotionSensorDelay.Default)
		{
			uint delay = (uint)((double)Convert.ToInt32(interval) / ms);

			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (accelerometer != null)
					{
						accelerometer.ReportInterval = delay;
						accelerometer.ReadingChanged += AccelerometerReadingChanged;
					}
					else
					{
						Debug.WriteLine("Accelerometer not available");
					}
					break;
				case MotionSensorType.Gyroscope:
					if (gyrometer != null)
					{
						gyrometer.ReportInterval = delay;
						gyrometer.ReadingChanged += GyrometerReadingChanged;
					}
					else
					{
						Debug.WriteLine("Gyrometer not available");
					}
					break;
				case MotionSensorType.Magnetometer:
					if (magnetometer != null)
					{

						magnetometer.ReportInterval = delay;
						magnetometer.ReadingChanged += MagnetometerReadingChanged;
					}
					else
					{
						Debug.WriteLine("Magnetometer not available");
					}
					break;
				case MotionSensorType.Compass:

					if (compass != null)
					{

						compass.ReportInterval = delay;
						compass.ReadingChanged += CompassReadingChanged;
					}
					else
					{
						Debug.WriteLine("Compass not available");
					}

					break;

			}
			sensorStatus[sensorType] = true;
		}

		void MagnetometerReadingChanged(Magnetometer sender, MagnetometerReadingChangedEventArgs args)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Vector,
				SensorType = MotionSensorType.Magnetometer,
				Value = new MotionVector { X = args.Reading.MagneticFieldX, Y = args.Reading.MagneticFieldY, Z = args.Reading.MagneticFieldZ }
			});
		}

		void CompassReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
		{
			if (args.Reading.HeadingTrueNorth != null)
				OnSensorValueChanged(new SensorValueChangedEventArgs
				{
					ValueType = MotionSensorValueType.Single,
					SensorType = MotionSensorType.Compass,
					Value = new MotionValue { Value = args.Reading.HeadingTrueNorth }
				});
		}

		void GyrometerReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Vector,
				SensorType = MotionSensorType.Gyroscope,
				Value = new MotionVector { X = args.Reading.AngularVelocityX, Y = args.Reading.AngularVelocityY, Z = args.Reading.AngularVelocityZ }
			});

		}

		void AccelerometerReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Vector,
				SensorType = MotionSensorType.Accelerometer,
				Value = new MotionVector { X = args.Reading.AccelerationX, Y = args.Reading.AccelerationY, Z = args.Reading.AccelerationZ }
			});
		}

		public void Stop(MotionSensorType sensorType)
		{
			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (accelerometer != null)
					{
						accelerometer.ReadingChanged -= AccelerometerReadingChanged;
					}
					else
					{
						Debug.WriteLine("Accelerometer not available");
					}
					break;
				case MotionSensorType.Gyroscope:
					if (gyrometer != null)
					{
						gyrometer.ReadingChanged -= GyrometerReadingChanged;
					}
					else
					{
						Debug.WriteLine("Gyrometer not available");
					}
					break;
				case MotionSensorType.Magnetometer:
					if (magnetometer != null)
					{
						magnetometer.ReadingChanged -= MagnetometerReadingChanged;
					}
					else
					{
						Debug.WriteLine("Magnetometer not available");
					}
					break;
				case MotionSensorType.Compass:
					if (compass != null)
					{
						compass.ReadingChanged -= CompassReadingChanged;
					}
					else
					{
						Debug.WriteLine("Compass not available");
					}
					break;
			}
			sensorStatus[sensorType] = false;
		}

		public bool IsActive(MotionSensorType sensorType)
		{
			return sensorStatus[sensorType];
		}
	}
}
