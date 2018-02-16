using Xamarin.Services.Sharing;
using System;
using UIKit;

namespace Xamarin.Services.Sharing
{
	static class ShareColorExtensions
	{
		public static UIColor ToUIColor(this ShareColor color)
		{
			if (color == null)
				throw new ArgumentNullException(nameof(color));

			return new UIColor(
				color.R / 255f,
				color.G / 255f,
				color.B / 255f,
				color.A / 255f);
		}
	}
}
