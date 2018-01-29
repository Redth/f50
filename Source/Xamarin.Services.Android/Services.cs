using System;
using Android.Content;
using Android.OS;
using Android.App;

namespace Xamarin.Services
{
    public class Platform
    {
        public static void Init (Activity activity, Bundle bundle)
        {
            lifecycleListener = new ActivityLifecycleContextListener ();
            application = activity.Application;
            application.RegisterActivityLifecycleCallbacks (lifecycleListener);
        }

        public static void Uninit ()
        {
            if (lifecycleListener != null) {
                if (application != null)
                    application.UnregisterActivityLifecycleCallbacks (lifecycleListener);
                lifecycleListener.Dispose ();
                lifecycleListener = null;
            }
        }

        static Application application;
        static ActivityLifecycleContextListener lifecycleListener;

        public static Context CurrentContext {
            get { return lifecycleListener?.Context ?? application?.ApplicationContext; }
        }

        public static Activity CurrentActivity {
            get { return lifecycleListener?.Activity; }
        }
    }
}
