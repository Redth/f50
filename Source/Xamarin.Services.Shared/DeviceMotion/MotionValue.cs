namespace Xamarin.Services.DeviceMotion
{
	public class MotionValue
	{
		public virtual double? Value { get; set; }

		public override string ToString()
		{
			return string.Format("Value = {0}", Value);
		}
	}
}
