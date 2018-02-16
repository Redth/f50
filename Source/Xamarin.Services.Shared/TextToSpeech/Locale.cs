namespace Xamarin.Services.TextToSpeech
{
	public struct Locale
	{
		public string Language { get; set; }

		public string Country { get; set; }

		public string DisplayName { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Country))
				return Language;
			return $"{Language}-{Country}";
		}
	}
}
