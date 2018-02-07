using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Services.Geolocation
{
	public class Position
	{
		public Position()
		{
		}

		public Position(double latitude, double longitude)
		{

			Timestamp = DateTimeOffset.UtcNow;
			Latitude = latitude;
			Longitude = longitude;
		}

		public Position(Position position)
		{
			if (position == null)
				throw new ArgumentNullException("position");

			Timestamp = position.Timestamp;
			Latitude = position.Latitude;
			Longitude = position.Longitude;
			Altitude = position.Altitude;
			AltitudeAccuracy = position.AltitudeAccuracy;
			Accuracy = position.Accuracy;
			Heading = position.Heading;
			Speed = position.Speed;
		}

		public DateTimeOffset Timestamp { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double Altitude { get; set; }

		public double Accuracy { get; set; }

		public double AltitudeAccuracy { get; set; }

		public double Heading { get; set; }

		public double Speed { get; set; }
	}

	public class PositionEventArgs : EventArgs
	{
		public PositionEventArgs(Position position)
		{
			Position = position ?? throw new ArgumentNullException("position");
		}

		public Position Position { get; private set; }
	}

	public class GeolocationException : Exception
	{
		public GeolocationException(GeolocationError error)
		  : base("A geolocation error occured: " + error)
		{
			if (!Enum.IsDefined(typeof(GeolocationError), error))
				throw new ArgumentException("error is not a valid GelocationError member", "error");

			Error = error;
		}

		public GeolocationException(GeolocationError error, Exception innerException)
		  : base("A geolocation error occured: " + error, innerException)
		{
			if (!Enum.IsDefined(typeof(GeolocationError), error))
				throw new ArgumentException("error is not a valid GelocationError member", "error");

			Error = error;
		}

		public GeolocationError Error { get; private set; }
	}

	public class PositionErrorEventArgs : EventArgs
	{
		public PositionErrorEventArgs(GeolocationError error)
		{
			Error = error;
		}

		public GeolocationError Error { get; private set; }
	}

	public enum GeolocationError
	{
		PositionUnavailable,

		Unauthorized
	}
}
