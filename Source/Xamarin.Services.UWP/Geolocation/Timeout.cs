using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Services.Geolocation
{
	internal class Timeout
	{
		public Timeout(int timeout, Action timesup)
		{
			if (timeout == Infite)
				return; // nothing to do
			if (timeout < 0)
				throw new ArgumentOutOfRangeException("timeout");
			if (timesup == null)
				throw new ArgumentNullException("timesup");

			Task.Delay(TimeSpan.FromMilliseconds(timeout), canceller.Token)
				.ContinueWith(t =>
				{
					if (!t.IsCanceled)
						timesup();
				});
		}

		public void Cancel()
		{
			canceller.Cancel();
		}

		private readonly CancellationTokenSource canceller = new CancellationTokenSource();

		public const int Infite = -1;
	}
}
