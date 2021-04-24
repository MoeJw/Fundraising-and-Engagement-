using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class ListOptions : BaseOptions
	{
		[JsonProperty("limit")]
		public long? Limit
		{
			get;
			set;
		}

		[JsonProperty("starting_after")]
		public string StartingAfter
		{
			get;
			set;
		}

		[JsonProperty("ending_before")]
		public string EndingBefore
		{
			get;
			set;
		}
	}
}
