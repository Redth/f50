namespace Xamarin.Services.DeviceMotion
{
#if INCLUDE_INTERFACES
	public interface IDeviceMotionService
	{
		event SensorValueChangedEventHandler SensorValueChanged;

		void Start(MotionSensorType sensorType, MotionSensorDelay interval = MotionSensorDelay.Default);

		void Stop(MotionSensorType sensorType);

		bool IsActive(MotionSensorType sensorType);
	}
#endif
}
