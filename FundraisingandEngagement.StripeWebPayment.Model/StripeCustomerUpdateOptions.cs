using System.Collections.Generic;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeCustomerUpdateOptions
	{
		[JsonProperty("account_balance")]
		public int? AccountBalance
		{
			get;
			set;
		}

		[JsonProperty("source")]
		public string SourceToken
		{
			get;
			set;
		}

		[JsonProperty("source")]
		public CardCreateNestedOptions SourceCard
		{
			get;
			set;
		}

		[JsonProperty("coupon")]
		public string Coupon
		{
			get;
			set;
		}

		[JsonProperty("default_source")]
		public string DefaultSource
		{
			get;
			set;
		}

		[JsonProperty("description")]
		public string Description
		{
			get;
			set;
		}

		[JsonProperty("email")]
		public string Email
		{
			get;
			set;
		}

		[JsonProperty("metadata")]
		public Dictionary<string, string> Metadata
		{
			get;
			set;
		}
	}
}
