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
    [Activity (Label = "SampleServices.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate (Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate (bundle);

            global::Xamarin.Services.Platform.Init (this, bundle);

            global::Xamarin.Forms.Forms.Init (this, bundle);

            LoadApplication (new App ());
        }

        public override void OnRequestPermissionsResult (int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::Xamarin.Services.Permissions.PermissionsService.Current.OnRequestPermissionsResult (requestCode, permissions, grantResults);
        }

        protected override void OnDestroy ()
        {
            global::Xamarin.Services.Platform.Uninit ();

            base.OnDestroy ();
        }
    }
}
