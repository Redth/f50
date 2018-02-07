using System;

namespace Xamarin.Services.Connectivity
{
	public partial class ConnectivityService : IDisposable
#if INCLUDE_INTERFACES
		, IConnectivityService
#endif
	{
		private bool disposed = false;

		protected virtual void OnConnectivityChanged(ConnectivityChangedEventArgs e)
			=> ConnectivityChanged?.Invoke(this, e);

		protected virtual void OnConnectivityTypeChanged(ConnectivityTypeChangedEventArgs e)
			=> ConnectivityTypeChanged?.Invoke(this, e);

		public event ConnectivityChangedEventHandler ConnectivityChanged;

		public event ConnectivityTypeChangedEventHandler ConnectivityTypeChanged;

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				OnDispose(disposing);

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ConnectivityService()
		{
			Dispose(false);
		}
	}
}
