using System;
using Android.Graphics;
using Xamarin.Services.Sharing;

namespace Xamarin.Services.Sharing
{
	static class ShareColorExtensions
	{
		public static Color ToNativeColor(this ShareColor color)
		{
			if (color == null)
				throw new ArgumentNullException(nameof(color));

			return new Color(
				color.R,
				color.G,
				color.B,
				color.A);
		}
	}
}
