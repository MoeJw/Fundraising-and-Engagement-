using System;
using FundraisingandEngagement.Stripe.Infrastructure;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeToken : StripeEntityWithId
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

		[JsonProperty("created")]
		[JsonConverter(typeof(StripeDateTimeConverter))]
		public DateTime? Created
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

		[JsonProperty("used")]
		public bool? Used
		{
			get;
			set;
		}

		[JsonProperty("bank_account[id]")]
		public string BankAccountId
		{
			get;
			set;
		}

		[JsonProperty("bank_account[object]")]
		public string BankAccountObject
		{
			get;
			set;
		}

		[JsonProperty("bank_account[country]")]
		public string BankAccountCountry
		{
			get;
			set;
		}

		[JsonProperty("bank_account[currency]")]
		public string BankAccountCurrency
		{
			get;
			set;
		}

		[JsonProperty("bank_account[last4]")]
		public string BankAccountLast4
		{
			get;
			set;
		}

		[JsonProperty("bank_account[status]")]
		public string BankAccountStatus
		{
			get;
			set;
		}

		[JsonProperty("bank_account[bank_name]")]
		public string BankAccountName
		{
			get;
			set;
		}

		[JsonProperty("bank_account[fingerprint]")]
		public string BankAccountFingerprint
		{
			get;
			set;
		}

		[JsonProperty("bank_account[routing_number]")]
		public string BankAccountRoutingNumber
		{
			get;
			set;
		}

		[JsonProperty("card")]
		public StripeCard StripeCard
		{
			get;
			set;
		}

		[Obsolete("This property is not valid on tokens and will be removed in a later version.")]
		[JsonProperty("description")]
		public string Description
		{
			get;
			set;
		}
	}
}
