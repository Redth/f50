namespace Xamarin.Services.Sharing
{
	public class ShareColor
	{
		public int A { get; set; }

		public int R { get; set; }

		public int G { get; set; }

		public int B { get; set; }

		public ShareColor()
		{
		}

		public ShareColor(int r, int g, int b)
			: this(r, g, b, 255)
		{
		}

		public ShareColor(int r, int g, int b, int a)
		{
			A = a;
			R = r;
			G = g;
			B = b;
		}
	}
}
