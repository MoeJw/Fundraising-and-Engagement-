using System;
using System.Collections.Generic;
using FundraisingandEngagement.Stripe.Infrastructure;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeTransferReversal : StripeEntityWithId
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("amount")]
		public int Amount
		{
			get;
			set;
		}

		[JsonProperty("created")]
		[JsonConverter(typeof(StripeDateTimeConverter))]
		public DateTime Created
		{
			get;
			set;
		}

		[JsonProperty("currency")]
		public string Currency
		{
			get;
			set;
		}

		[JsonProperty("balance_transaction")]
		public string BalanceTransactionId
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

		[JsonProperty("transfer")]
		public string TransferId
		{
			get;
			set;
		}

		[JsonProperty("refund_application_fee")]
		public int? RefundApplicationFee
		{
			get;
			set;
		}
	}
}
