using static System.Math;

namespace Xamarin.Services.Geolocation
{
	public static class Util
	{
		public static double CalculateDistance(double latitudeStart, double longitudeStart,
			double latitudeEnd, double longitudeEnd, DistanceUnits units = DistanceUnits.Miles)
		{
			if (latitudeEnd == latitudeStart && longitudeEnd == longitudeStart)
				return 0;

			var rlat1 = PI * latitudeStart / 180.0;
			var rlat2 = PI * latitudeEnd / 180.0;
			var theta = longitudeStart - longitudeEnd;
			var rtheta = PI * theta / 180.0;
			var dist = Sin(rlat1) * Sin(rlat2) + Cos(rlat1) * Cos(rlat2) * Cos(rtheta);
			dist = Acos(dist);
			dist = dist * 180.0 / PI;
			var final = dist * 60.0 * 1.1515;
			if (double.IsNaN(final) || double.IsInfinity(final) || double.IsNegativeInfinity(final) ||
				double.IsPositiveInfinity(final) || final < 0)
				return 0;

			if (units == DistanceUnits.Kilometers)
				return MilesToKilometers(final);

			return final;
		}

		public static double CalculateDistance(this Position positionStart, Position positionEnd, DistanceUnits units = DistanceUnits.Miles) =>
			CalculateDistance(positionStart.Latitude, positionStart.Longitude, positionEnd.Latitude, positionEnd.Longitude, units);

		public static double MilesToKilometers(double miles) => miles * 1.609344;

		public static double KilometersToMiles(double kilometers) => kilometers * .62137119;

		public enum DistanceUnits
		{
			Kilometers,
			Miles
		}
	}
}
