using System;
using Android.App;
using Android.Content;

namespace Xamarin.Services
{
	internal static class Extensions
	{
		public static void StartNewTopMostActivity(this Intent intent)
		{
			if (intent == null)
				throw new ArgumentNullException(nameof(intent));

			intent.SetFlags(ActivityFlags.ClearTop);
			intent.SetFlags(ActivityFlags.NewTask);

			Application.Context.StartActivity(intent);
		}
	}
}
