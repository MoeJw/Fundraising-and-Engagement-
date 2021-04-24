using System;
using FundraisingandEngagement.Stripe.Infrastructure;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeTermsOfServiceAcceptance : StripeEntity
	{
		[JsonProperty("date")]
		[JsonConverter(typeof(StripeDateTimeConverter))]
		public DateTime? Date
		{
			get;
			set;
		}

		[JsonProperty("ip")]
		public string Ip
		{
			get;
			set;
		}

		[JsonProperty("user_agent")]
		public string UserAgent
		{
			get;
			set;
		}
	}
}
