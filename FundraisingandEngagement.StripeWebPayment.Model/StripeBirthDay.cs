using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeBirthDay : StripeEntity
	{
		[JsonProperty("day")]
		public int? Day
		{
			get;
			set;
		}

		[JsonProperty("month")]
		public int? Month
		{
			get;
			set;
		}

		[JsonProperty("year")]
		public int? Year
		{
			get;
			set;
		}
	}
}
