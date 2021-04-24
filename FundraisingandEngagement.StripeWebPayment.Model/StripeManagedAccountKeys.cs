using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeManagedAccountKeys : StripeEntity
	{
		[JsonProperty("secret")]
		public string Secret
		{
			get;
			set;
		}

		[JsonProperty("publishable")]
		public string Publishable
		{
			get;
			set;
		}
	}
}
