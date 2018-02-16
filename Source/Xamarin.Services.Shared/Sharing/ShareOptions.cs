namespace Xamarin.Services.Sharing
{
	public class ShareOptions
	{
		public string ChooserTitle { get; set; } = null;

		public ShareAppControlType ExcludedAppControlTypes { get; set; } = 0;

		public ShareUIActivityType[] ExcludedUIActivityTypes { get; set; } = null;

		public ShareRectangle PopoverAnchorRectangle { get; set; } = null;
	}
}
