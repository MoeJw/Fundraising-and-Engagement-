using System.Collections.Generic;
using FundraisingandEngagement.StripeIntegration.Helpers;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeCharge : StripeEntityWithId
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

		[JsonProperty("amount_refunded")]
		public int AmountRefunded
		{
			get;
			set;
		}

		[JsonProperty("captured")]
		public bool? Captured
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

		public string CustomerId
		{
			get;
			set;
		}

		[JsonIgnore]
		public StripeCustomer Customer
		{
			get;
			set;
		}

		[JsonProperty("customer")]
		internal object InternalCustomer
		{
			set
			{
				StringOrObject<StripeCustomer>.Map(value, delegate(string s)
				{
					CustomerId = s;
				}, delegate(StripeCustomer o)
				{
					Customer = o;
				});
			}
		}

		[JsonProperty("description")]
		public string Description
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

		[JsonProperty("fraud_details")]
		public Dictionary<string, string> FraudDetails
		{
			get;
			set;
		}

		public string InvoiceId
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

		[JsonProperty("metadata")]
		public Dictionary<string, string> Metadata
		{
			get;
			set;
		}

		public string OnBehalfOfId
		{
			get;
			set;
		}

		[JsonProperty("paid")]
		public bool Paid
		{
			get;
			set;
		}

		[JsonProperty("receipt_email")]
		public string ReceiptEmail
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

		[JsonProperty("refunded")]
		public bool Refunded
		{
			get;
			set;
		}

		public string ReviewId
		{
			get;
			set;
		}

		[JsonProperty("source")]
		public Source Source
		{
			get;
			set;
		}

		[JsonProperty("statement_descriptor")]
		public string StatementDescriptor
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

		[JsonProperty("transfer_group")]
		public string TransferGroup
		{
			get;
			set;
		}
	}
}
