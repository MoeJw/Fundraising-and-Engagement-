using System.Collections.Generic;
using FundraisingandEngagement.StripeIntegration.Helpers;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeCard : StripeEntityWithId
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		public string AccountId
		{
			get;
			set;
		}

		[JsonProperty("address_city")]
		public string AddressCity
		{
			get;
			set;
		}

		[JsonProperty("address_country")]
		public string AddressCountry
		{
			get;
			set;
		}

		[JsonProperty("address_line1")]
		public string AddressLine1
		{
			get;
			set;
		}

		[JsonProperty("address_line1_check")]
		public string AddressLine1Check
		{
			get;
			set;
		}

		[JsonProperty("address_line2")]
		public string AddressLine2
		{
			get;
			set;
		}

		[JsonProperty("address_state")]
		public string AddressState
		{
			get;
			set;
		}

		[JsonProperty("address_zip")]
		public string AddressZip
		{
			get;
			set;
		}

		[JsonProperty("address_zip_check")]
		public string AddressZipCheck
		{
			get;
			set;
		}

		[JsonProperty("available_payout_methods")]
		public string[] AvailablePayoutMethods
		{
			get;
			set;
		}

		[JsonProperty("brand")]
		public string Brand
		{
			get;
			set;
		}

		[JsonProperty("country")]
		public string Country
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

		[JsonProperty("cvc_check")]
		public string CvcCheck
		{
			get;
			set;
		}

		[JsonProperty("default_for_currency")]
		public bool DefaultForCurrency
		{
			get;
			set;
		}

		[JsonProperty("dynamic_last4")]
		public string DynamicLast4
		{
			get;
			set;
		}

		[JsonProperty("exp_month")]
		public int ExpirationMonth
		{
			get;
			set;
		}

		[JsonProperty("exp_year")]
		public int ExpirationYear
		{
			get;
			set;
		}

		[JsonProperty("fingerprint")]
		public string Fingerprint
		{
			get;
			set;
		}

		[JsonProperty("funding")]
		public string Funding
		{
			get;
			set;
		}

		[JsonProperty("last4")]
		public string Last4
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

		[JsonProperty("name")]
		public string Name
		{
			get;
			set;
		}

		public string RecipientId
		{
			get;
			set;
		}

		[JsonProperty("three_d_secure")]
		public string ThreeDSecure
		{
			get;
			set;
		}

		[JsonProperty("tokenization_method")]
		public string TokenizationMethod
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
