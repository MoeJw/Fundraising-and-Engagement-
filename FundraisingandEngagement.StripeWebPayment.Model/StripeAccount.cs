using System.Collections.Generic;
using FundraisingandEngagement.StripeIntegration.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeAccount : StripeEntityWithId
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("business_logo")]
		public string BusinessLogoFileId
		{
			get;
			set;
		}

		[JsonProperty("business_name")]
		public string BusinessName
		{
			get;
			set;
		}

		[JsonProperty("business_primary_color")]
		public string BusinessPrimaryColor
		{
			get;
			set;
		}

		[JsonProperty("business_url")]
		public string BusinessUrl
		{
			get;
			set;
		}

		[JsonProperty("charges_enabled")]
		public bool ChargesEnabled
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

		[JsonProperty("currencies_supported")]
		public string[] CurrenciesSupported
		{
			get;
			set;
		}

		[JsonProperty("debit_negative_balances")]
		public bool DebitNegativeBalances
		{
			get;
			set;
		}

		[JsonProperty("decline_charge_on")]
		public StripeDeclineChargeOn DeclineChargeOn
		{
			get;
			set;
		}

		[JsonProperty("default_currency")]
		public string DefaultCurrency
		{
			get;
			set;
		}

		[JsonProperty("details_submitted")]
		public bool DetailsSubmitted
		{
			get;
			set;
		}

		[JsonProperty("display_name")]
		public string DisplayName
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

		[JsonProperty("external_accounts")]
		[JsonConverter(typeof(SourceListConverter))]
		public StripeList<Source> ExternalAccounts
		{
			get;
			set;
		}

		[JsonProperty("legal_entity")]
		public StripeLegalEntity LegalEntity
		{
			get;
			set;
		}

		[JsonProperty("managed")]
		public bool Managed
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

		[JsonProperty("product_description")]
		public string ProductDescription
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

		[JsonProperty("transfer_statement_descriptor")]
		public string TransferStatementDescriptor
		{
			get;
			set;
		}

		[JsonProperty("support_email")]
		public string SupportEmail
		{
			get;
			set;
		}

		[JsonProperty("support_phone")]
		public string SupportPhone
		{
			get;
			set;
		}

		[JsonProperty("support_url")]
		public string SupportUrl
		{
			get;
			set;
		}

		[JsonProperty("timezone")]
		public string Timezone
		{
			get;
			set;
		}

		[JsonProperty("tos_acceptance")]
		public StripeTermsOfServiceAcceptance TermsOfServiceAcceptance
		{
			get;
			set;
		}

		[JsonProperty("transfer_schedule")]
		public StripeTransferSchedule TransferSchedule
		{
			get;
			set;
		}

		[JsonProperty("transfers_enabled")]
		public bool TransfersEnabled
		{
			get;
			set;
		}

		[JsonProperty("verification")]
		public StripeAccountVerification AccountVerification
		{
			get;
			set;
		}

		[JsonProperty("keys")]
		public StripeManagedAccountKeys ManagedAccountKeys
		{
			get;
			set;
		}
	}
}
