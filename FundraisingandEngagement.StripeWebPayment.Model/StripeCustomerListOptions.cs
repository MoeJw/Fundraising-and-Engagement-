using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeCustomerListOptions : StripeListOptions
	{
		[JsonProperty("created")]
		public StripeDateFilter Created
		{
			get;
			set;
		}
	}
}
