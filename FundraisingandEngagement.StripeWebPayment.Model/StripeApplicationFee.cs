using System;
using FundraisingandEngagement.Stripe.Infrastructure;
using FundraisingandEngagement.StripeIntegration.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeApplicationFee : StripeEntityWithId
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("livemode")]
		public bool LiveMode
		{
			get;
			set;
		}

		public string AccountId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeAccount Account
		{
			get;
			set;
		}

		[JsonProperty("account")]
		internal object InternalAccount
		{
			set
			{
				ExpandableProperty<StripeAccount>.Map(value, delegate(string s)
				{
					AccountId = s;
				}, delegate(StripeAccount o)
				{
					Account = o;
				});
			}
		}

		[JsonProperty("amount")]
		public int Amount
		{
			get;
			set;
		}

		[JsonProperty("application")]
		public string ApplicationId
		{
			get;
			set;
		}

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
			set
			{
				ExpandableProperty<StripeBalanceTransaction>.Map(value, delegate(string s)
				{
					BalanceTransactionId = s;
				}, delegate(StripeBalanceTransaction o)
				{
					BalanceTransaction = o;
				});
			}
		}

		public string CardId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeCard Card
		{
			get;
			set;
		}

		[JsonProperty("card")]
		internal object InternalCard
		{
			set
			{
				ExpandableProperty<StripeCard>.Map(value, delegate(string s)
				{
					CardId = s;
				}, delegate(StripeCard o)
				{
					Card = o;
				});
			}
		}

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
			set
			{
				ExpandableProperty<StripeCharge>.Map(value, delegate(string s)
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

		[JsonProperty("refunded")]
		public bool Refunded
		{
			get;
			set;
		}

		[JsonProperty("refunds")]
		public StripeList<StripeApplicationFeeRefund> StripeApplicationFeeRefundList
		{
			get;
			set;
		}

		[JsonProperty("amount_refunded")]
		public int AmountRefunded
		{
			get;
			set;
		}
	}
}
