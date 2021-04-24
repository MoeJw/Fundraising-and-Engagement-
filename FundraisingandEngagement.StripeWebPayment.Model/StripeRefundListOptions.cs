using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeRefundListOptions : StripeListOptions
	{
		[JsonProperty("charge")]
		public string ChargeId
		{
			get;
			set;
		}
	}
}
