using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeCustomerCreateOptions
	{
		[JsonProperty("account_balance")]
		public int? AccountBalance
		{
			get;
			set;
		}

		[JsonProperty("coupon")]
		public string CouponId
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

		[JsonProperty("plan")]
		public string PlanId
		{
			get;
			set;
		}

		[JsonProperty("quantity")]
		public int? Quantity
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

		[JsonProperty("tax_percent")]
		public decimal? TaxPercent
		{
			get;
			set;
		}

		[JsonProperty("validate")]
		public bool? Validate
		{
			get;
			set;
		}

		public DateTime? TrialEnd
		{
			get;
			set;
		}

		public bool EndTrialNow
		{
			get;
			set;
		}

		[JsonProperty("trial_end")]
		internal string TrialEndInternal
		{
			get
			{
				if (EndTrialNow)
				{
					return "now";
				}
				if (TrialEnd.HasValue)
				{
					return TrialEnd.Value.ConvertDateTimeToEpoch().ToString();
				}
				return null;
			}
		}
	}
}
