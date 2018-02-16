using Xamarin.Forms;
using Xamarin.Services.DeviceMotion;

namespace SampleServices
{
	public partial class DeviceMotionPage : ContentPage
	{
		public DeviceMotionPage()
		{
			InitializeComponent();

			var motion = new Xamarin.Services.DeviceMotion.DeviceMotionService();
			motion.SensorValueChanged += OnSensorValueChanged;

			motion.Start(MotionSensorType.Accelerometer, MotionSensorDelay.Default);
			motion.Start(MotionSensorType.Compass, MotionSensorDelay.Default);
			motion.Start(MotionSensorType.Gyroscope, MotionSensorDelay.Default);
			motion.Start(MotionSensorType.Magnetometer, MotionSensorDelay.Default);
		}

		private void OnSensorValueChanged(object sender, SensorValueChangedEventArgs e)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				switch (e.SensorType)
				{
					case MotionSensorType.Accelerometer:
						labelAcc.Text = e.Value.ToString();
						break;
					case MotionSensorType.Gyroscope:
						labelGyro.Text = e.Value.ToString();
						break;
					case MotionSensorType.Magnetometer:
						labelMagnet.Text = e.Value.ToString();
						break;
					case MotionSensorType.Compass:
						labelCompass.Text = e.Value.ToString();
						break;
				}
			});
		}
	}
}
