using System;
using System.Collections.Generic;
using FundraisingandEngagement.Stripe.Infrastructure;
using FundraisingandEngagement.StripeIntegration.Helpers;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeRefund : StripeEntityWithId
	{
		[JsonProperty("id")]
		public new string Id
		{
			get;
			set;
		}

		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("amount")]
		public long Amount
		{
			get;
			set;
		}

		[JsonIgnore]
		public string BalanceTransactionId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeBalanceTransaction BalanceTransaction
		{
			get;
			set;
		}

		[JsonProperty("balance_transaction")]
		internal object InternalBalanceTransaction
		{
			get
			{
				return ((object)BalanceTransaction) ?? ((object)BalanceTransactionId);
			}
			set
			{
				StringOrObject<StripeBalanceTransaction>.Map(value, delegate(string s)
				{
					BalanceTransactionId = s;
				}, delegate(StripeBalanceTransaction o)
				{
					BalanceTransaction = o;
				});
			}
		}

		[JsonIgnore]
		public string ChargeId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeCharge Charge
		{
			get;
			set;
		}

		[JsonProperty("charge")]
		internal object InternalCharge
		{
			get
			{
				return ((object)Charge) ?? ((object)ChargeId);
			}
			set
			{
				StringOrObject<StripeCharge>.Map(value, delegate(string s)
				{
					ChargeId = s;
				}, delegate(StripeCharge o)
				{
					Charge = o;
				});
			}
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

		[JsonProperty("description")]
		public string Description
		{
			get;
			set;
		}

		[JsonIgnore]
		public string FailureBalanceTransactionId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeBalanceTransaction FailureBalanceTransaction
		{
			get;
			set;
		}

		[JsonProperty("failure_balance_transaction")]
		internal object InternalFailureBalanceTransaction
		{
			get
			{
				return ((object)FailureBalanceTransaction) ?? ((object)FailureBalanceTransactionId);
			}
			set
			{
				StringOrObject<StripeBalanceTransaction>.Map(value, delegate(string s)
				{
					FailureBalanceTransactionId = s;
				}, delegate(StripeBalanceTransaction o)
				{
					FailureBalanceTransaction = o;
				});
			}
		}

		[JsonProperty("failure_reason")]
		public string FailureReason
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

		[JsonProperty("reason")]
		public string Reason
		{
			get;
			set;
		}

		[JsonProperty("receipt_number")]
		public string ReceiptNumber
		{
			get;
			set;
		}

		[JsonProperty("status")]
		public string Status
		{
			get;
			set;
		}
	}
}
