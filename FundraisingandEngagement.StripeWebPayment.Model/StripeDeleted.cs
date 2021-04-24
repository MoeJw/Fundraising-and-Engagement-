using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeDeleted : StripeEntityWithId
	{
		[JsonProperty("deleted")]
		public bool Deleted
		{
			get;
			set;
		}
	}
}
