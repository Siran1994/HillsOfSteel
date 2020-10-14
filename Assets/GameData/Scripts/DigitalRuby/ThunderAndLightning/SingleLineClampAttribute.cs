namespace DigitalRuby.ThunderAndLightning
{
	public class SingleLineClampAttribute : SingleLineAttribute
	{
		public SingleLineClampAttribute(string tooltip, double minValue, double maxValue) : base(tooltip)
		{
			MinValue = minValue;
			MaxValue = maxValue;
		}

		public double MinValue { get; private set; }
		public double MaxValue { get; private set; }
	}
}
