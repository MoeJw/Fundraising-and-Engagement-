using System;
using System.Collections.Generic;
using FundraisingandEngagement.Stripe.Infrastructure;
using FundraisingandEngagement.StripeIntegration.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeTransfer : StripeEntityWithId
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

		[JsonProperty("amount")]
		public int Amount
		{
			get;
			set;
		}

		public string ApplicationFeeId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeApplicationFee ApplicationFee
		{
			get;
			set;
		}

		[JsonProperty("application_fee")]
		internal object InternalApplicationFee
		{
			set
			{
				ExpandableProperty<StripeApplicationFee>.Map(value, delegate(string s)
				{
					ApplicationFeeId = s;
				}, delegate(StripeApplicationFee o)
				{
					ApplicationFee = o;
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

		[JsonProperty("date")]
		[JsonConverter(typeof(StripeDateTimeConverter))]
		public DateTime Date
		{
			get;
			set;
		}

		[JsonProperty("reversals")]
		public StripeList<StripeTransferReversal> StripeTransferReversalList
		{
			get;
			set;
		}

		[JsonProperty("reversed")]
		public bool Reversed
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

		[JsonProperty("type")]
		public string Type
		{
			get;
			set;
		}

		[JsonProperty("amount_reversed")]
		public int AmountReversed
		{
			get;
			set;
		}

		public string BalanceTransactionId
		{
			get;
			set;
		}

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

		[JsonProperty("description")]
		public string Description
		{
			get;
			set;
		}

		[JsonProperty("destination")]
		public string Destination
		{
			get;
			set;
		}

		[JsonProperty("destination_payment")]
		public string DestinationPayment
		{
			get;
			set;
		}

		[JsonProperty("failure_code")]
		public string FailureCode
		{
			get;
			set;
		}

		[JsonProperty("failure_message")]
		public string FailureMessage
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

		[JsonProperty("bank_account")]
		public StripeBankAccount StripeBankAccount
		{
			get;
			set;
		}

		[JsonProperty("card")]
		public StripeCard Card
		{
			get;
			set;
		}

		[JsonProperty("source_transaction")]
		public string SourceTransactionId
		{
			get;
			set;
		}

		[JsonProperty("source_type")]
		public string SourceType
		{
			get;
			set;
		}

		[Obsolete("Recipients are deprecated. Use Destination or Connect instead.")]
		public string RecipientId
		{
			get;
			set;
		}

		[Obsolete("Recipients are deprecated. Use Destination or Connect instead.")]
		[JsonIgnore]
		public StripeRecipient Recipient
		{
			get;
			set;
		}

		[JsonProperty("recipient")]
		internal object InternalRecipient
		{
			set
			{
				ExpandableProperty<StripeRecipient>.Map(value, delegate(string s)
				{
					RecipientId = s;
				}, delegate(StripeRecipient o)
				{
					Recipient = o;
				});
			}
		}

		[JsonProperty("statement_descriptor")]
		public string StatementDescriptor
		{
			get;
			set;
		}
	}
}
