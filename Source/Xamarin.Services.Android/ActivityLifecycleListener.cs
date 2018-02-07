using System;
using Android.App;
using Android.Content;

namespace Xamarin.Services
{
	internal class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		Activity currentActivity = null;

		public Context Context
		{
			get
			{
				return currentActivity ?? Application.Context;
			}
		}

		public Activity Activity
		{
			get
			{
				return currentActivity;
			}
		}

		public void OnActivityCreated(Activity activity, global::Android.OS.Bundle savedInstanceState)
		{
		}

		public void OnActivityDestroyed(Activity activity)
		{
		}

		public void OnActivityPaused(Activity activity)
		{
		}

		public void OnActivityResumed(Activity activity)
		{
			currentActivity = activity;
		}

		public void OnActivitySaveInstanceState(Activity activity, global::Android.OS.Bundle outState)
		{
		}

		public void OnActivityStarted(Activity activity)
		{
		}

		public void OnActivityStopped(Activity activity)
		{
		}
	}
}
