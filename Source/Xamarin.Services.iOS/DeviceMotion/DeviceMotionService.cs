using System.Collections.Generic;
using System.Diagnostics;
using Foundation;
using CoreMotion;
using CoreLocation;

namespace Xamarin.Services.DeviceMotion
{
	public partial class DeviceMotionService
	{
		private double ms = 1000.0;
		private CMMotionManager motionManager;
		private CLLocationManager locationManager;
		private IDictionary<MotionSensorType, bool> sensorStatus;

		public DeviceMotionService()
		{
			motionManager = new CMMotionManager();
			locationManager = new CLLocationManager();
			locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
			locationManager.HeadingFilter = 1;

			sensorStatus = new Dictionary<MotionSensorType, bool>(){
				{ MotionSensorType.Accelerometer, false},
				{ MotionSensorType.Gyroscope, false},
				{ MotionSensorType.Magnetometer, false},
				{ MotionSensorType.Compass, false}
			};
		}

		public void Start(MotionSensorType sensorType, MotionSensorDelay interval)
		{

			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (motionManager.AccelerometerAvailable)
					{
						motionManager.AccelerometerUpdateInterval = (double)interval / ms;
						motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, OnAccelerometerChanged);
					}
					else
					{
						Debug.WriteLine("Accelerometer not available");
					}
					break;
				case MotionSensorType.Gyroscope:
					if (motionManager.GyroAvailable)
					{
						motionManager.GyroUpdateInterval = (double)interval / ms;
						motionManager.StartGyroUpdates(NSOperationQueue.CurrentQueue, OnGyroscopeChanged);
					}
					else
					{
						Debug.WriteLine("Gyroscope not available");
					}
					break;
				case MotionSensorType.Magnetometer:
					if (motionManager.MagnetometerAvailable)
					{
						motionManager.MagnetometerUpdateInterval = (double)interval / ms;
						motionManager.StartMagnetometerUpdates(NSOperationQueue.CurrentQueue, OnMagnometerChanged);
					}
					else
					{
						Debug.WriteLine("Magnetometer not available");
					}
					break;
				case MotionSensorType.Compass:
					if (CLLocationManager.HeadingAvailable)
					{
						locationManager.StartUpdatingHeading();
						locationManager.UpdatedHeading += OnHeadingChanged;
					}
					else
					{
						Debug.WriteLine("Compass not available");
					}
					break;
			}
			sensorStatus[sensorType] = true;
		}

		private void OnHeadingChanged(object sender, CLHeadingUpdatedEventArgs e)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Single,
				SensorType = MotionSensorType.Compass,
				Value = new MotionValue { Value = e.NewHeading.TrueHeading }
			});
		}

		private void OnMagnometerChanged(CMMagnetometerData data, NSError error)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Vector,
				SensorType = MotionSensorType.Magnetometer,
				Value = new MotionVector() { X = data.MagneticField.X, Y = data.MagneticField.Y, Z = data.MagneticField.Z }
			});
		}

		private void OnAccelerometerChanged(CMAccelerometerData data, NSError error)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Vector,
				SensorType = MotionSensorType.Accelerometer,
				Value = new MotionVector() { X = data.Acceleration.X, Y = data.Acceleration.Y, Z = data.Acceleration.Z }
			});
		}

		private void OnGyroscopeChanged(CMGyroData data, NSError error)
		{
			OnSensorValueChanged(new SensorValueChangedEventArgs
			{
				ValueType = MotionSensorValueType.Vector,
				SensorType = MotionSensorType.Gyroscope,
				Value = new MotionVector() { X = data.RotationRate.x, Y = data.RotationRate.y, Z = data.RotationRate.z }
			});
		}

		public void Stop(MotionSensorType sensorType)
		{
			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (motionManager.AccelerometerActive)
						motionManager.StopAccelerometerUpdates();
					else
						Debug.WriteLine("Accelerometer not available");
					break;
				case MotionSensorType.Gyroscope:
					if (motionManager.GyroActive)
						motionManager.StopGyroUpdates();
					else
						Debug.WriteLine("Gyroscope not available");
					break;
				case MotionSensorType.Magnetometer:
					if (motionManager.MagnetometerActive)
						motionManager.StopMagnetometerUpdates();
					else
						Debug.WriteLine("Magnetometer not available");
					break;
				case MotionSensorType.Compass:
					if (CLLocationManager.HeadingAvailable)
					{
						locationManager.StopUpdatingHeading();
						locationManager.UpdatedHeading -= OnHeadingChanged;
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
