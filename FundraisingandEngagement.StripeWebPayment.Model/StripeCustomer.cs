using FundraisingandEngagement.StripeIntegration.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeCustomer : StripeEntityWithId
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("account_balance")]
		public int AccountBalance
		{
			get;
			set;
		}

		[JsonProperty("business_vat_id")]
		public string BusinessVatId
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

		[JsonProperty("deleted")]
		public bool? Deleted
		{
			get;
			set;
		}

		public string DefaultCustomerBankAccountId
		{
			get;
			set;
		}

		public string DefaultSourceId
		{
			get;
			set;
		}

		[JsonIgnore]
		public Source DefaultSource
		{
			get;
			set;
		}

		[JsonProperty("default_source_type")]
		public string DefaultSourceType
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

		[JsonProperty("sources")]
		public StripeList<Source> Sources
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
	}
}
