using System;
using Android.Content;
using Android.OS;
using Android.App;

namespace Xamarin.Services
{
	public class Platform
	{
		public static void Init(Activity activity, Bundle bundle)
		{
			lifecycleListener = new ActivityLifecycleContextListener();
			application = activity.Application;
			application.RegisterActivityLifecycleCallbacks(lifecycleListener);
		}

		public static void Uninit()
		{
			if (lifecycleListener != null)
			{
				if (application != null)
					application.UnregisterActivityLifecycleCallbacks(lifecycleListener);
				lifecycleListener.Dispose();
				lifecycleListener = null;
			}
		}

		public static void OnRequestPermissionsResult(int requestCode, string[] permissions, global::Android.Content.PM.Permission[] grantResults)
		{
			Permissions.PermissionsService.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		static Application application;
		static ActivityLifecycleContextListener lifecycleListener;

		internal static Context CurrentContext
		{
			get { return lifecycleListener?.Context ?? application?.ApplicationContext; }
		}

		internal static Activity CurrentActivity
		{
			get { return lifecycleListener?.Activity; }
		}
	}
}
