using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SampleServices.Droid
{
	[Activity(Label = "SampleServices.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			// One single simple init call which allows the android app to keep track of current activity/context
			global::Xamarin.Services.Platform.Init(this, bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			LoadApplication(new App());
		}

		// This is required to handle permissions automatically for the user
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			global::Xamarin.Services.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		// I don't think anyone ever really does this, and I'm not sure it's actually necessary, but I've added it
		// for good measure.  Realistically if the app is torn down, I don't think we need to worry about this
		// There's really no cleanup or saving of any sort of state that isn't reinstantiated when the app is created
		// again anyway.
		protected override void OnDestroy()
		{
			global::Xamarin.Services.Platform.Uninit();

			base.OnDestroy();
		}
	}
}
