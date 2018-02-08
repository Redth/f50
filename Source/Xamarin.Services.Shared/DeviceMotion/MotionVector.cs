using System;

namespace Xamarin.Services.DeviceMotion
{
	public class MotionVector : MotionValue
	{
		public double X { get; set; }

		public double Y { get; set; }

		public double Z { get; set; }

		public override string ToString()
		{
			return string.Format("X={0}, Y={0}, Z={0}", X, Y, Z);
		}

		public override double? Value
		{
			get
			{
				return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
			}
		}
	}
}
