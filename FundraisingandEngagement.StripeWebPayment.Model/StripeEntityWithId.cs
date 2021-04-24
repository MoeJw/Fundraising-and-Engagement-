using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public abstract class StripeEntityWithId : StripeEntity
	{
		[JsonProperty("id")]
		public string Id
		{
			get;
			set;
		}
	}
}
