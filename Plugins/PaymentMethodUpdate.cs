using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Moneris;

namespace Plugins
{
	public class PaymentMethodUpdate : PluginBase
	{
		private const string PostImageAlias = "msnfp_paymentmethod";

		public PaymentMethodUpdate(string unsecure, string secure)
			: base(typeof(PaymentMethodUpdate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered PaymentMethodUpdate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			string empty = string.Empty;
			Guid empty2 = Guid.Empty;
			if (!pluginExecutionContext.InputParameters.Contains("Target") || !(pluginExecutionContext.InputParameters["Target"] is Entity))
			{
				return;
			}
			localContext.TracingService.Trace("---------Entering PaymentMethodUpdate.cs Main Function---------");
			Entity entity = (Entity)pluginExecutionContext.InputParameters["Target"];
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity2 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			Entity entity3 = organizationService.Retrieve("msnfp_paymentmethod", entity.Id, GetColumnSet());
			if (entity3 == null)
			{
				throw new ArgumentNullException("msnfp_paymentmethodid");
			}
			if (entity2 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			if (entity3.Contains("msnfp_isreusable") && entity3.Contains("msnfp_type") && entity3.Contains("msnfp_billing_line1") && entity3.Contains("msnfp_paymentprocessorid") && entity3.Contains("msnfp_customerid") && entity3.Contains("msnfp_authtoken") && entity3.Contains("msnfp_cclast4"))
			{
				if (!(bool)entity3["msnfp_isreusable"] || ((OptionSetValue)entity3["msnfp_type"]).Value != 844060000 || (string)entity3["msnfp_billing_line1"] == null)
				{
					localContext.TracingService.Trace("Payment Method is not reusable, has no street 1 or is not a credit card. Exiting plugin.");
					return;
				}
				Entity paymentProcessorForPaymentMethod = getPaymentProcessorForPaymentMethod(entity3, localContext, organizationService);
				if (paymentProcessorForPaymentMethod != null)
				{
					localContext.TracingService.Trace("Payment Processor retrieved.");
					if (paymentProcessorForPaymentMethod.Contains("msnfp_paymentgatewaytype"))
					{
						if (((OptionSetValue)paymentProcessorForPaymentMethod["msnfp_paymentgatewaytype"]).Value == 844060000)
						{
						}
						if (((OptionSetValue)paymentProcessorForPaymentMethod["msnfp_paymentgatewaytype"]).Value == 844060001)
						{
						}
						if (((OptionSetValue)paymentProcessorForPaymentMethod["msnfp_paymentgatewaytype"]).Value != 844060002)
						{
						}
					}
				}
				localContext.TracingService.Trace("---------Exiting PaymentMethodUpdate.cs---------");
			}
			else
			{
				localContext.TracingService.Trace("Payment Method is not reusable, has no street 1, is not a credit card or has no payment processor. Exiting plugin.");
			}
		}

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_paymentmethodid", "msnfp_customerid", "msnfp_isreusable", "msnfp_type", "msnfp_billing_line1", "msnfp_emailaddress1", "msnfp_telephone1", "msnfp_billing_postalcode", "msnfp_cclast4", "msnfp_authtoken", "msnfp_firstname", "msnfp_lastname", "msnfp_paymentprocessorid", "msnfp_ccexpmmyy", "msnfp_ccbrandcode", "msnfp_nameonfile", "msnfp_stripecustomerid");
		}

		private Entity getPaymentProcessorForPaymentMethod(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering getPaymentProcessorForPaymentMethod().");
			if (paymentMethod.Contains("msnfp_paymentprocessorid"))
			{
				return service.Retrieve("msnfp_paymentprocessor", ((EntityReference)paymentMethod["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_apikey", "msnfp_name", "msnfp_storeid", "msnfp_avsvalidation", "msnfp_cvdvalidation", "msnfp_testmode", "msnfp_paymentgatewaytype", "msnfp_iatsagentcode", "msnfp_iatspassword", "msnfp_stripeservicekey"));
			}
			localContext.TracingService.Trace("No payment processor is assigned to this payment method. Exiting plugin.");
			throw new ArgumentNullException("msnfp_paymentprocessorid");
		}

		private AvsInfo AssignAVSValidationFieldsFromPaymentMethod(Entity paymentMethod, AvsInfo avsCheck, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering AssignAVSValidationFieldsFromPaymentMethod().");
			try
			{
				if (!paymentMethod.Contains("msnfp_billing_line1") || !paymentMethod.Contains("msnfp_billing_postalcode"))
				{
					localContext.TracingService.Trace("Customer is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
					throw new Exception("Customer is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
				}
				string[] array = ((string)paymentMethod["msnfp_billing_line1"]).Split(' ');
				if (array.Length <= 1)
				{
					localContext.TracingService.Trace("Could not split address for AVS Validation. Please ensure the Street 1 billing address on the payment method is in the form '123 Example Street'. Exiting plugin.");
					throw new ArgumentNullException("msnfp_billing_line1");
				}
				string text = (string)paymentMethod["msnfp_billing_line1"];
				localContext.TracingService.Trace("Unformatted Street Name: " + text);
				text = text.Replace(array[0], "").Trim(' ');
				localContext.TracingService.Trace("Formatted Street Name: " + text);
				localContext.TracingService.Trace("Formatted Street Number: " + array[0]);
				avsCheck.SetAvsStreetNumber(array[0]);
				avsCheck.SetAvsStreetName(text);
				avsCheck.SetAvsZipCode((string)paymentMethod["msnfp_billing_postalcode"]);
				if (paymentMethod.Contains("msnfp_emailaddress1"))
				{
					avsCheck.SetAvsEmail((string)paymentMethod["msnfp_emailaddress1"]);
				}
				avsCheck.SetAvsShipMethod("G");
				if (paymentMethod.Contains("msnfp_telephone1"))
				{
					avsCheck.SetAvsCustPhone((string)paymentMethod["msnfp_telephone1"]);
				}
				localContext.TracingService.Trace("Updated AVS Check variable successfully.");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("AssignAVSValidationFieldsFromPaymentMethod() Error: " + ex.ToString());
				throw new Exception("AssignAVSValidationFieldsFromPaymentMethod() Error: " + ex.ToString());
			}
			return avsCheck;
		}
	}
}
