using System;

namespace Xamarin.Services.DeviceMotion
{
	public partial class DeviceMotionService : IDisposable
#if INCLUDE_INTERFACES
		, IDeviceMotionService
#endif
	{
		private bool disposed = false;

		protected virtual void OnSensorValueChanged(SensorValueChangedEventArgs e)
			=> SensorValueChanged?.Invoke(this, e);

		public event SensorValueChangedEventHandler SensorValueChanged;

		partial void OnDispose(bool disposing);

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				OnDispose(disposing);

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~DeviceMotionService()
		{
			Dispose(false);
		}
	}

	public class SensorValueChangedEventArgs : EventArgs
	{
		public MotionSensorType SensorType { get; set; }

		public MotionValue Value { get; set; }

		public MotionSensorValueType ValueType { get; set; }
	}

	public delegate void SensorValueChangedEventHandler(object sender, SensorValueChangedEventArgs e);
}
