using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using FundraisingandEngagement.StripeIntegration.Helpers;
using FundraisingandEngagement.StripeWebPayment.Model;
using FundraisingandEngagement.StripeWebPayment.Service;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Moneris;
using Plugins.AzureModels;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class PaymentMethodCreate : PluginBase
	{
		public PaymentMethodCreate(string unsecure, string secure)
			: base(typeof(PaymentMethodCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered PaymentMethodCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			Entity entity = null;
			string messageName = pluginExecutionContext.MessageName;
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity2 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity2 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			Entity configurationRecordByUser = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			int stage = pluginExecutionContext.Stage;
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering PaymentMethodCreate.cs Main Function---------");
				localContext.TracingService.Trace("Executing stage {0}", pluginExecutionContext.Stage);
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						switch (stage)
						{
						case 40:
							AddOrUpdateThisRecordWithAzure(entity3, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
							break;
						case 10:
							localContext.TracingService.Trace("Check for payment processor and auth token.");
							if (entity3.GetAttributeValue<EntityReference>("msnfp_paymentprocessorid") != null && entity3.GetAttributeValue<OptionSetValue>("msnfp_type") != null && entity3.GetAttributeValue<string>("msnfp_authtoken") == null)
							{
								localContext.TracingService.Trace("No auth token found. Payment processor found. Registering new customer card to gateway.");
								Entity entity4 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)entity3["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentgatewaytype"));
								if (((OptionSetValue)entity3["msnfp_type"]).Value == 844060000)
								{
									if (((OptionSetValue)entity4["msnfp_paymentgatewaytype"]).Value == 844060000)
									{
										RegisterPaymentMethodWithMonerisVault(entity3, localContext, organizationService, messageName);
									}
									else if (((OptionSetValue)entity4["msnfp_paymentgatewaytype"]).Value == 844060001)
									{
										RegisterPaymentMethodWithStripeVault(entity3, localContext, organizationService, messageName);
									}
									else if (((OptionSetValue)entity4["msnfp_paymentgatewaytype"]).Value == 844060002)
									{
										RegisterPaymentMethodWithiATSVault(entity3, localContext, organizationService, messageName);
									}
									else
									{
										localContext.TracingService.Trace("((OptionSetValue)paymentProcessor[msnfp_paymentgatewaytype]).Value == " + ((OptionSetValue)entity4["msnfp_paymentgatewaytype"]).Value);
									}
								}
								else
								{
									localContext.TracingService.Trace("Card is not reusable or is not a credit card - ignore.");
								}
							}
							else if (entity3.GetAttributeValue<EntityReference>("msnfp_paymentprocessorid") != null)
							{
								localContext.TracingService.Trace("Auth token found. Avoiding registration of profile process for the gateway.");
							}
							break;
						}
					}
					else if (messageName == "Update")
					{
						switch (stage)
						{
						case 40:
							entity = organizationService.Retrieve("msnfp_paymentmethod", entity3.Id, GetColumnSet());
							AddOrUpdateThisRecordWithAzure(entity, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
							break;
						case 10:
						{
							localContext.TracingService.Trace("Payment Method exists already, updating: " + entity3.Id.ToString());
							Entity entity5 = organizationService.Retrieve("msnfp_paymentmethod", entity3.Id, GetColumnSet());
							if (entity5 != null)
							{
								if (entity5.GetAttributeValue<string>("msnfp_authtoken") != null && !entity3.Contains("msnfp_authtoken"))
								{
									entity3["msnfp_authtoken"] = entity5.GetAttributeValue<string>("msnfp_authtoken");
								}
								if (entity5.GetAttributeValue<string>("msnfp_nameonfile") != null && !entity3.Contains("msnfp_nameonfile"))
								{
									localContext.TracingService.Trace("Assigning msnfp_nameonfile: " + entity5.GetAttributeValue<string>("msnfp_nameonfile"));
									entity3["msnfp_nameonfile"] = entity5.GetAttributeValue<string>("msnfp_nameonfile");
								}
								if (entity5.GetAttributeValue<EntityReference>("msnfp_paymentprocessorid") != null && !entity3.Contains("msnfp_paymentprocessorid"))
								{
									localContext.TracingService.Trace("Assigning msnfp_paymentprocessorid: " + entity5.GetAttributeValue<EntityReference>("msnfp_paymentprocessorid").Id.ToString());
									entity3["msnfp_paymentprocessorid"] = entity5.GetAttributeValue<EntityReference>("msnfp_paymentprocessorid");
								}
								if (entity5.GetAttributeValue<EntityReference>("msnfp_customerid") != null && !entity3.Contains("msnfp_customerid"))
								{
									localContext.TracingService.Trace("Assigning msnfp_customerid: " + entity5.GetAttributeValue<EntityReference>("msnfp_customerid").Id.ToString());
									entity3["msnfp_customerid"] = entity5.GetAttributeValue<EntityReference>("msnfp_customerid");
								}
								if (entity5.Contains("msnfp_isreusable") && !entity3.Contains("msnfp_isreusable"))
								{
									localContext.TracingService.Trace("Assigning msnfp_isreusable: " + entity5.GetAttributeValue<bool>("msnfp_isreusable"));
									entity3["msnfp_isreusable"] = entity5.GetAttributeValue<bool>("msnfp_isreusable");
								}
								if (entity5.GetAttributeValue<OptionSetValue>("msnfp_type") != null && !entity3.Contains("msnfp_type"))
								{
									localContext.TracingService.Trace("Assigning msnfp_type: " + entity5.GetAttributeValue<OptionSetValue>("msnfp_type").Value);
									entity3["msnfp_type"] = entity5.GetAttributeValue<OptionSetValue>("msnfp_type");
								}
								if (entity5.GetAttributeValue<string>("msnfp_telephone1") != null && !entity3.Contains("msnfp_telephone1"))
								{
									localContext.TracingService.Trace("Assigning msnfp_telephone1: " + entity5.GetAttributeValue<string>("msnfp_telephone1").ToString());
									entity3["msnfp_telephone1"] = entity5.GetAttributeValue<string>("msnfp_telephone1");
								}
								if (entity5.GetAttributeValue<string>("msnfp_emailaddress1") != null && !entity3.Contains("msnfp_emailaddress1"))
								{
									localContext.TracingService.Trace("Assigning msnfp_emailaddress1: " + entity5.GetAttributeValue<string>("msnfp_emailaddress1").ToString());
									entity3["msnfp_emailaddress1"] = entity5.GetAttributeValue<string>("msnfp_emailaddress1");
								}
								if (entity5.GetAttributeValue<string>("msnfp_billing_line1") != null && !entity3.Contains("msnfp_billing_line1"))
								{
									localContext.TracingService.Trace("Assigning msnfp_billing_line1: " + entity5.GetAttributeValue<string>("msnfp_billing_line1").ToString());
									entity3["msnfp_billing_line1"] = entity5.GetAttributeValue<string>("msnfp_billing_line1");
								}
								if (entity5.GetAttributeValue<string>("msnfp_stripecustomerid") != null && !entity3.Contains("msnfp_stripecustomerid"))
								{
									localContext.TracingService.Trace("Assigning msnfp_stripecustomerid: " + entity5.GetAttributeValue<string>("msnfp_stripecustomerid").ToString());
									entity3["msnfp_stripecustomerid"] = entity5.GetAttributeValue<string>("msnfp_stripecustomerid");
								}
							}
							localContext.TracingService.Trace("Checking payment method fields.");
							if (entity3.Contains("msnfp_type") && entity3.Contains("msnfp_billing_line1") && entity3.Contains("msnfp_paymentprocessorid") && entity3.Contains("msnfp_customerid") && entity3.Contains("msnfp_authtoken") && entity3.Contains("msnfp_cclast4"))
							{
								if (((OptionSetValue)entity3["msnfp_type"]).Value != 844060000 || (string)entity3["msnfp_billing_line1"] == null)
								{
									localContext.TracingService.Trace("Payment Method is not reusable, has no street 1 or is not a credit card. Exiting plugin.");
									break;
								}
								Entity paymentProcessorForPaymentMethod = getPaymentProcessorForPaymentMethod(entity3, localContext, organizationService);
								if (paymentProcessorForPaymentMethod == null)
								{
									break;
								}
								localContext.TracingService.Trace("Payment Processor retrieved.");
								if (!paymentProcessorForPaymentMethod.Contains("msnfp_paymentgatewaytype"))
								{
									break;
								}
								if (((OptionSetValue)paymentProcessorForPaymentMethod["msnfp_paymentgatewaytype"]).Value == 844060000)
								{
									localContext.TracingService.Trace("Gateway Type = Moneris");
									if (pluginExecutionContext.Depth == 1 && entity3["msnfp_authtoken"] != null)
									{
										updateMonerisVaultProfile(entity3, paymentProcessorForPaymentMethod, localContext, organizationService, messageName);
									}
								}
								if (((OptionSetValue)paymentProcessorForPaymentMethod["msnfp_paymentgatewaytype"]).Value == 844060001)
								{
									localContext.TracingService.Trace("Gateway Type = Stripe");
									if (pluginExecutionContext.Depth == 1 && entity3["msnfp_authtoken"] != null)
									{
										UpdateStripeCreditCard(entity3, paymentProcessorForPaymentMethod, localContext, organizationService, messageName);
									}
								}
								if (((OptionSetValue)paymentProcessorForPaymentMethod["msnfp_paymentgatewaytype"]).Value == 844060002)
								{
									localContext.TracingService.Trace("Gateway Type = iATS");
									if (pluginExecutionContext.Depth == 1 && entity3["msnfp_authtoken"] != null)
									{
										UpdateIatsCustomerCreditCard(entity3, paymentProcessorForPaymentMethod, localContext, organizationService, messageName);
									}
								}
							}
							else
							{
								localContext.TracingService.Trace("Payment Method is not reusable, has no street 1, is not a credit card or has no payment processor. Exiting plugin.");
							}
							break;
						}
						}
					}
					if (entity3.GetAttributeValue<string>("msnfp_cclast4") != null && entity3.GetAttributeValue<string>("msnfp_cclast4").Trim().Length > 4)
					{
						localContext.TracingService.Trace("Improperly Masked Credit Card Number. Can't save.");
						throw new Exception("Improperly Masked Credit Card Number. Can't save.");
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting workflow.");
				}
			}
			if (messageName == "Delete")
			{
				entity = organizationService.Retrieve("msnfp_paymentmethod", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(entity, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting PaymentMethodCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_paymentmethodid", "msnfp_name", "msnfp_identifier", "msnfp_bankactnumber", "msnfp_bankactrtnumber", "msnfp_bankname", "msnfp_banktypecode", "msnfp_ccbrandcode", "msnfp_expirydate", "msnfp_ccexpmmyy", "msnfp_expirydate", "msnfp_billing_city", "msnfp_cclast4", "msnfp_ccexpmmyy", "msnfp_emailaddress1", "msnfp_firstname", "msnfp_lastname", "msnfp_nameonfile", "msnfp_paymentprocessorid", "msnfp_customerid", "msnfp_stripecustomerid", "msnfp_telephone1", "msnfp_billing_line1", "msnfp_authtoken", "msnfp_billing_line2", "msnfp_isreusable", "msnfp_billing_line3", "msnfp_billing_postalcode", "msnfp_billing_state", "msnfp_billing_country", "msnfp_abafinancialinstitutionname", "statecode", "statuscode", "msnfp_type", "msnfp_nameonbankaccount", "createdon");
		}

		private void RegisterPaymentMethodWithMonerisVault(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			localContext.TracingService.Trace("Entering RegisterPaymentMethodWithMonerisVault().");
			Entity entity = null;
			if (!paymentMethod.Contains("msnfp_cclast4") || !paymentMethod.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				throw new ArgumentNullException("msnfp_cclast4 or msnfp_ccexpmmyy is null");
			}
			entity = getPaymentProcessorForPaymentMethod(paymentMethod, localContext, service);
			localContext.TracingService.Trace("Put gathered payment information into vault profile object.");
			string storeId = (string)entity["msnfp_storeid"];
			string apiToken = (string)entity["msnfp_apikey"];
			string pan = ((string)paymentMethod["msnfp_cclast4"]).Trim();
			string text = (string)paymentMethod["msnfp_ccexpmmyy"];
			string cryptType = "7";
			string procCountryCode = "CA";
			bool statusCheck = false;
			string str = text.Substring(0, 2);
			string str2 = text.Substring(2, 2);
			localContext.TracingService.Trace("Old Expiry format (MMYY):" + text);
			text = str2 + str;
			localContext.TracingService.Trace("Moneris Expiry format (YYMM):" + text);
			string phone = "";
			string email = "";
			string note = "Created in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
			string custId = ((EntityReference)paymentMethod["msnfp_customerid"]).Id.ToString();
			if (paymentMethod.Contains("msnfp_telephone1"))
			{
				phone = (string)paymentMethod["msnfp_telephone1"];
			}
			if (paymentMethod.Contains("msnfp_emailaddress1"))
			{
				email = (string)paymentMethod["msnfp_emailaddress1"];
			}
			ResAddCC resAddCC = new ResAddCC();
			resAddCC.SetPan(pan);
			resAddCC.SetExpDate(text);
			resAddCC.SetCryptType(cryptType);
			resAddCC.SetCustId(custId);
			resAddCC.SetPhone(phone);
			resAddCC.SetEmail(email);
			resAddCC.SetNote(note);
			resAddCC.SetGetCardType("true");
			AvsInfo avsCheck = new AvsInfo();
			localContext.TracingService.Trace("Check for AVS Validation.");
			if (paymentMethod.Contains("msnfp_ccbrandcode"))
			{
				if (((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value == 844060004)
				{
					if (entity.Contains("msnfp_avsvalidation"))
					{
						if ((bool)entity["msnfp_avsvalidation"])
						{
							localContext.TracingService.Trace("AVS Validation = True");
							if (!paymentMethod.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("No Donor. Exiting plugin.");
								throw new ArgumentNullException("msnfp_customerid");
							}
							try
							{
								localContext.TracingService.Trace("Entering address information for AVS validation.");
								avsCheck = AssignAVSValidationFieldsFromPaymentMethod(paymentMethod, avsCheck, localContext, service);
								resAddCC.SetAvsInfo(avsCheck);
							}
							catch
							{
								localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
								throw new Exception("Unable to set AVSValidation fields. Please ensure the address fields are valid for the customer (" + ((EntityReference)paymentMethod["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)paymentMethod["msnfp_customerid"]).Id.ToString());
							}
						}
						else
						{
							localContext.TracingService.Trace("AVS Validation = False");
						}
					}
				}
				else
				{
					localContext.TracingService.Trace("Could not do AVS Validation as the card type is not supported. AVS is only supported by Visa(844060000 credit,844060002 debit), MasterCard(844060001 credit,844060003 debit), Discover(844060008) and American Express(844060004).");
					localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)paymentMethod["msnfp_ccbrandcode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
			}
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			httpsPostRequest.SetProcCountryCode(procCountryCode);
			if (entity.Contains("msnfp_testmode"))
			{
				if ((bool)entity["msnfp_testmode"])
				{
					localContext.TracingService.Trace("Test Mode is Enabled.");
					httpsPostRequest.SetTestMode(state: true);
				}
				else
				{
					localContext.TracingService.Trace("Test Mode is Disabled.");
					httpsPostRequest.SetTestMode(state: false);
				}
			}
			else
			{
				localContext.TracingService.Trace("Test Mode not set. Defaulting to test mode enabled.");
				httpsPostRequest.SetTestMode(state: true);
			}
			httpsPostRequest.SetStoreId(storeId);
			httpsPostRequest.SetApiToken(apiToken);
			httpsPostRequest.SetTransaction(resAddCC);
			httpsPostRequest.SetStatusCheck(statusCheck);
			localContext.TracingService.Trace("Attempting to create the new user profile in the Moneris Vault.");
			httpsPostRequest.Send();
			try
			{
				Receipt receipt = httpsPostRequest.GetReceipt();
				localContext.TracingService.Trace("---------Moneris Response---------");
				localContext.TracingService.Trace("DataKey = " + receipt.GetDataKey());
				localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
				localContext.TracingService.Trace("Message = " + receipt.GetMessage());
				localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
				localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
				localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
				localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
				localContext.TracingService.Trace("ResSuccess = " + receipt.GetResSuccess());
				localContext.TracingService.Trace("PaymentType = " + receipt.GetPaymentType());
				localContext.TracingService.Trace("Cust ID = " + receipt.GetResDataCustId());
				localContext.TracingService.Trace("Phone = " + receipt.GetResDataPhone());
				localContext.TracingService.Trace("Email = " + receipt.GetResDataEmail());
				localContext.TracingService.Trace("Note = " + receipt.GetResDataNote());
				localContext.TracingService.Trace("Exp Date = " + receipt.GetResDataExpdate());
				localContext.TracingService.Trace("Crypt Type = " + receipt.GetResDataCryptType());
				localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
				localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
				localContext.TracingService.Trace("Avs Street Number = " + receipt.GetResDataAvsStreetNumber());
				localContext.TracingService.Trace("Avs Street Name = " + receipt.GetResDataAvsStreetName());
				localContext.TracingService.Trace("Avs Zipcode = " + receipt.GetResDataAvsZipcode());
				localContext.TracingService.Trace("---------End Moneris Response---------");
				try
				{
					paymentMethod["msnfp_authtoken"] = receipt.GetDataKey();
					if (receipt.GetDataKey().Length > 0)
					{
						localContext.TracingService.Trace("Masked Card Number");
						MaskPaymentMethod(localContext, paymentMethod, receipt.GetDataKey(), null, null, messageName);
					}
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error, could not assign data id to auth token. Data key: " + receipt.GetDataKey());
					localContext.TracingService.Trace("Error: " + ex.ToString());
					throw new ArgumentNullException("msnfp_authtoken");
				}
			}
			catch (Exception ex2)
			{
				localContext.TracingService.Trace("Error processing response from payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex2.ToString());
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
		}

		private void RegisterPaymentMethodWithStripeVault(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			string empty = string.Empty;
			string text = "";
			string empty2 = string.Empty;
			Guid empty3 = Guid.Empty;
			Entity entity = null;
			Entity entity2 = null;
			string text2 = Guid.NewGuid().ToString();
			localContext.TracingService.Trace("Entering RegisterPaymentMethodWithStripeVault().");
			if (paymentMethod.Contains("msnfp_customerid"))
			{
				empty2 = ((EntityReference)paymentMethod["msnfp_customerid"]).LogicalName;
				empty3 = ((EntityReference)paymentMethod["msnfp_customerid"]).Id;
				entity = ((!(empty2 == "account")) ? service.Retrieve("contact", empty3, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "mobilephone", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")) : service.Retrieve("account", empty3, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "telephone3", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")));
				localContext.TracingService.Trace("Obtained customer information.");
				try
				{
					StripeCustomer stripeCustomer = null;
					string text3 = null;
					BaseStipeRepository baseStipeRepository = new BaseStipeRepository();
					localContext.TracingService.Trace("Getting payment processor.");
					entity2 = getPaymentProcessorForPaymentMethod(paymentMethod, localContext, service);
					string text4 = entity2["msnfp_stripeservicekey"].ToString();
					StripeConfiguration.SetApiKey(text4);
					localContext.TracingService.Trace("Setting up Stripe objects");
					string custName = ((entity.LogicalName == "account") ? entity["name"].ToString() : (entity["firstname"].ToString() + entity["lastname"].ToString()));
					string custEmail = (entity.Contains("emailaddress1") ? entity["emailaddress1"].ToString() : string.Empty);
					localContext.TracingService.Trace("Extracted customer info");
					stripeCustomer = new CustomerService().GetStripeCustomer(custName, custEmail, text4);
					localContext.TracingService.Trace("Obtained stripeCustomer object");
					StripeTokenCreateOptions stripeTokenCreateOptions = new StripeTokenCreateOptions();
					string text5 = (paymentMethod.Contains("msnfp_ccexpmmyy") ? paymentMethod["msnfp_ccexpmmyy"].ToString() : string.Empty);
					stripeTokenCreateOptions.Card = new StripeCreditCardOptions
					{
						Number = paymentMethod["msnfp_cclast4"].ToString(),
						ExpirationYear = text5.Substring(text5.Length - 2),
						ExpirationMonth = text5.Substring(0, text5.Length - 2)
					};
					StripeTokenService stripeTokenService = new StripeTokenService();
					localContext.TracingService.Trace("Creating token");
					StripeToken stripeToken = stripeTokenService.Create(stripeTokenCreateOptions);
					StripeCard stripeCard = new StripeCard();
					stripeCard.SourceToken = stripeToken.Id;
					string url = $"https://api.stripe.com/v1/customers/{stripeCustomer.Id}/sources";
					localContext.TracingService.Trace("Creating Stripe profile.");
					StripeCard stripeCard2 = baseStipeRepository.Create(stripeCard, url, text4);
					if (string.IsNullOrEmpty(stripeCard2.Id))
					{
						throw new Exception("Unable to add card to customer");
					}
					text3 = stripeCard2.Id;
					text = stripeCard2.Brand;
					localContext.TracingService.Trace("Returned Card Id- " + text3);
					ITracingService tracingService = localContext.TracingService;
					Guid guid = empty3;
					tracingService.Trace("Returned Stripe Customer Id- " + guid.ToString());
					localContext.TracingService.Trace("Updating Payment Method Entity.");
					MaskPaymentMethod(localContext, paymentMethod, text3, text, stripeCustomer.Id, messageName);
					localContext.TracingService.Trace("Payment Method Entity Updated.");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error : " + ex.Message);
					throw new Exception("Error : " + ex.Message);
				}
				return;
			}
			localContext.TracingService.Trace("msnfp_customerid is null. Exiting plugin.");
			throw new ArgumentNullException("msnfp_customerid");
		}

		private void RegisterPaymentMethodWithiATSVault(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			Guid empty3 = Guid.Empty;
			Entity entity = null;
			Entity entity2 = null;
			string text = Guid.NewGuid().ToString();
			string empty4 = string.Empty;
			string empty5 = string.Empty;
			string cardId = null;
			if (paymentMethod.Contains("msnfp_customerid"))
			{
				empty2 = ((EntityReference)paymentMethod["msnfp_customerid"]).LogicalName;
				empty3 = ((EntityReference)paymentMethod["msnfp_customerid"]).Id;
				entity = ((!(empty2 == "account")) ? service.Retrieve("contact", empty3, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "mobilephone", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid", "transactioncurrencyid")) : service.Retrieve("account", empty3, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "telephone3", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid", "transactioncurrencyid")));
				localContext.TracingService.Trace("Obtained customer information.");
				try
				{
					entity2 = getPaymentProcessorForPaymentMethod(paymentMethod, localContext, service);
					localContext.TracingService.Trace("Payment processor retrieved.");
					if (entity2 != null)
					{
						empty4 = entity2.GetAttributeValue<string>("msnfp_iatsagentcode");
						empty5 = entity2.GetAttributeValue<string>("msnfp_iatspassword");
						localContext.TracingService.Trace("Create new customer for iATS payment.");
						string text2 = (paymentMethod.Contains("msnfp_ccexpmmyy") ? paymentMethod.GetAttributeValue<string>("msnfp_ccexpmmyy") : string.Empty);
						string str = text2.Substring(text2.Length - 2);
						string str2 = text2.Substring(0, text2.Length - 2);
						text2 = str2 + "/" + str;
						string creditCardNum = (paymentMethod.Contains("msnfp_cclast4") ? paymentMethod.GetAttributeValue<string>("msnfp_cclast4") : string.Empty);
						CreateCreditCardCustomerCode createCreditCardCustomerCode = new CreateCreditCardCustomerCode();
						createCreditCardCustomerCode.lastName = ((entity.LogicalName == "contact") ? entity.GetAttributeValue<string>("lastname") : string.Empty);
						createCreditCardCustomerCode.firstName = ((entity.LogicalName == "account") ? entity.GetAttributeValue<string>("name") : entity.GetAttributeValue<string>("firstname"));
						createCreditCardCustomerCode.agentCode = empty4;
						createCreditCardCustomerCode.password = empty5;
						createCreditCardCustomerCode.beginDate = DateTime.Today;
						createCreditCardCustomerCode.endDate = DateTime.Today.AddDays(1.0);
						createCreditCardCustomerCode.country = entity.GetAttributeValue<string>("address1_country");
						createCreditCardCustomerCode.creditCardExpiry = text2;
						createCreditCardCustomerCode.creditCardNum = creditCardNum;
						createCreditCardCustomerCode.recurring = false;
						createCreditCardCustomerCode.address = entity.GetAttributeValue<string>("address1_line1");
						createCreditCardCustomerCode.city = entity.GetAttributeValue<string>("address1_city");
						createCreditCardCustomerCode.zipCode = entity.GetAttributeValue<string>("address1_postalcode");
						createCreditCardCustomerCode.state = entity.GetAttributeValue<string>("address1_stateorprovince");
						createCreditCardCustomerCode.email = entity.GetAttributeValue<string>("emailaddress1");
						createCreditCardCustomerCode.creditCardCustomerName = ((entity.LogicalName == "account") ? entity.GetAttributeValue<string>("name") : (entity.GetAttributeValue<string>("firstname") + " " + entity.GetAttributeValue<string>("lastname")));
						localContext.TracingService.Trace("Creating customer.");
						XmlDocument xmlDocument = iATSProcess.CreateCreditCardCustomerCode(createCreditCardCustomerCode);
						localContext.TracingService.Trace("Customer created.");
						XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT");
						foreach (XmlNode item in elementsByTagName)
						{
							string innerText = item.InnerText;
							if (innerText.Contains("OK"))
							{
								cardId = xmlDocument.GetElementsByTagName("CUSTOMERCODE")[0].InnerText;
							}
						}
						localContext.TracingService.Trace("Mask and update the credit card.");
						MaskPaymentMethod(localContext, paymentMethod, cardId, null, null, messageName);
						return;
					}
					localContext.TracingService.Trace("paymentProcessor object is null. Exiting plugin.");
					throw new ArgumentNullException("paymentProcessor");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error : " + ex.Message);
					throw new Exception("Error : " + ex.Message);
				}
			}
			localContext.TracingService.Trace("msnfp_customerid is null. Exiting plugin.");
			throw new ArgumentNullException("msnfp_customerid");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string str = "PaymentMethod";
			string text = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			localContext.TracingService.Trace("Got API URL: " + text);
			if (text != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_paymentmethodid"].ToString());
				MSNFP_PaymentMethod mSNFP_PaymentMethod = new MSNFP_PaymentMethod();
				mSNFP_PaymentMethod.PaymentMethodId = (Guid)queriedEntityRecord["msnfp_paymentmethodid"];
				mSNFP_PaymentMethod.Name = (queriedEntityRecord.Contains("msnfp_name") ? ((string)queriedEntityRecord["msnfp_name"]) : string.Empty);
				mSNFP_PaymentMethod.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_PaymentMethod.Name);
				if (queriedEntityRecord.Contains("msnfp_bankactnumber") && queriedEntityRecord["msnfp_bankactnumber"] != null)
				{
					mSNFP_PaymentMethod.BankActNumber = (string)queriedEntityRecord["msnfp_bankactnumber"];
					localContext.TracingService.Trace("Got msnfp_bankactnumber.");
				}
				else
				{
					mSNFP_PaymentMethod.BankActNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bankactnumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_bankactrtnumber") && queriedEntityRecord["msnfp_bankactrtnumber"] != null)
				{
					mSNFP_PaymentMethod.BankActRtNumber = (string)queriedEntityRecord["msnfp_bankactrtnumber"];
					localContext.TracingService.Trace("Got msnfp_bankactrtnumber.");
				}
				else
				{
					mSNFP_PaymentMethod.BankActRtNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bankactrtnumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_bankname") && queriedEntityRecord["msnfp_bankname"] != null)
				{
					mSNFP_PaymentMethod.BankName = (string)queriedEntityRecord["msnfp_bankname"];
					localContext.TracingService.Trace("Got msnfp_bankname.");
				}
				else
				{
					mSNFP_PaymentMethod.BankName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bankname.");
				}
				if (queriedEntityRecord.Contains("msnfp_banktypecode") && queriedEntityRecord["msnfp_banktypecode"] != null)
				{
					mSNFP_PaymentMethod.BankTypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_banktypecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_banktypecode.");
				}
				else
				{
					mSNFP_PaymentMethod.BankTypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_banktypecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_ccbrandcode") && queriedEntityRecord["msnfp_ccbrandcode"] != null)
				{
					mSNFP_PaymentMethod.CcBrandCode = ((OptionSetValue)queriedEntityRecord["msnfp_ccbrandcode"]).Value;
					localContext.TracingService.Trace("Got msnfp_ccbrandcode.");
				}
				else
				{
					mSNFP_PaymentMethod.CcBrandCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ccbrandcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_ccexpmmyy") && queriedEntityRecord["msnfp_ccexpmmyy"] != null)
				{
					string text2 = (string)queriedEntityRecord["msnfp_ccexpmmyy"];
					try
					{
						mSNFP_PaymentMethod.CcExpDate = DateTime.ParseExact(text2, "MMyy", CultureInfo.InvariantCulture).AddMonths(1).AddDays(-1.0);
						localContext.TracingService.Trace("Got msnfp_expirydate from msnfp_ccexpmmyy (" + text2 + ")");
						localContext.TracingService.Trace("CcExpDate: " + mSNFP_PaymentMethod.CcExpDate.ToString());
						localContext.TracingService.Trace("Updating payment method field msnfp_expirydate from null to above CcExpDate.");
						Entity entity = new Entity(queriedEntityRecord.LogicalName, queriedEntityRecord.Id);
						entity["msnfp_expirydate"] = mSNFP_PaymentMethod.CcExpDate;
						service.Update(entity);
						localContext.TracingService.Trace("Updated record. Continuing with JSON request.");
					}
					catch (Exception ex)
					{
						mSNFP_PaymentMethod.CcExpDate = null;
						localContext.TracingService.Trace("Did NOT find msnfp_expirydate. Could not convert from MMYY date: " + text2);
						localContext.TracingService.Trace("Conversion Error: " + ex.ToString());
					}
				}
				else
				{
					mSNFP_PaymentMethod.CcExpDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_expirydate.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_city") && queriedEntityRecord["msnfp_billing_city"] != null)
				{
					mSNFP_PaymentMethod.BillingCity = (string)queriedEntityRecord["msnfp_billing_city"];
					localContext.TracingService.Trace("Got msnfp_billing_city.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingCity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_city.");
				}
				if (queriedEntityRecord.Contains("msnfp_cclast4") && queriedEntityRecord["msnfp_cclast4"] != null)
				{
					if (queriedEntityRecord["msnfp_cclast4"].ToString().Length > 4)
					{
						mSNFP_PaymentMethod.CcLast4 = queriedEntityRecord["msnfp_cclast4"].ToString().Substring(queriedEntityRecord["msnfp_cclast4"].ToString().Length - 4);
					}
					else
					{
						mSNFP_PaymentMethod.CcLast4 = (string)queriedEntityRecord["msnfp_cclast4"];
					}
					localContext.TracingService.Trace("Got msnfp_cclast4.");
				}
				else
				{
					mSNFP_PaymentMethod.CcLast4 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_cclast4.");
				}
				if (queriedEntityRecord.Contains("msnfp_ccexpmmyy") && queriedEntityRecord["msnfp_ccexpmmyy"] != null)
				{
					mSNFP_PaymentMethod.CcExpMmYy = (string)queriedEntityRecord["msnfp_ccexpmmyy"];
					localContext.TracingService.Trace("Got msnfp_ccexpmmyy.");
				}
				else
				{
					mSNFP_PaymentMethod.CcExpMmYy = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ccexpmmyy.");
				}
				if (queriedEntityRecord.Contains("msnfp_emailaddress1") && queriedEntityRecord["msnfp_emailaddress1"] != null)
				{
					mSNFP_PaymentMethod.Emailaddress1 = (string)queriedEntityRecord["msnfp_emailaddress1"];
					localContext.TracingService.Trace("Got msnfp_emailaddress1.");
				}
				else
				{
					mSNFP_PaymentMethod.Emailaddress1 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_emailaddress1.");
				}
				if (queriedEntityRecord.Contains("msnfp_firstname") && queriedEntityRecord["msnfp_firstname"] != null)
				{
					mSNFP_PaymentMethod.FirstName = (string)queriedEntityRecord["msnfp_firstname"];
					localContext.TracingService.Trace("Got msnfp_firstname.");
				}
				else
				{
					mSNFP_PaymentMethod.FirstName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_firstname.");
				}
				if (queriedEntityRecord.Contains("msnfp_lastname") && queriedEntityRecord["msnfp_lastname"] != null)
				{
					mSNFP_PaymentMethod.LastName = (string)queriedEntityRecord["msnfp_lastname"];
					localContext.TracingService.Trace("Got msnfp_lastname.");
				}
				else
				{
					mSNFP_PaymentMethod.LastName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lastname.");
				}
				if (queriedEntityRecord.Contains("msnfp_nameonfile") && queriedEntityRecord["msnfp_nameonfile"] != null)
				{
					mSNFP_PaymentMethod.NameOnFile = (string)queriedEntityRecord["msnfp_nameonfile"];
					localContext.TracingService.Trace("Got msnfp_nameonfile.");
				}
				else
				{
					mSNFP_PaymentMethod.NameOnFile = null;
					localContext.TracingService.Trace("Did NOT find msnfp_nameonfile.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentprocessorid") && queriedEntityRecord["msnfp_paymentprocessorid"] != null)
				{
					mSNFP_PaymentMethod.PaymentProcessorId = ((EntityReference)queriedEntityRecord["msnfp_paymentprocessorid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentprocessorid.");
				}
				else
				{
					Entity entity2 = service.Retrieve("systemuser", context.InitiatingUserId, new ColumnSet("msnfp_configurationid"));
					if (entity2.Contains("msnfp_configurationid") && entity2["msnfp_configurationid"] != null)
					{
						ColumnSet columnSet = new ColumnSet("msnfp_configurationid", "msnfp_paymentprocessorid");
						Entity entity3 = service.Retrieve("msnfp_configuration", ((EntityReference)entity2["msnfp_configurationid"]).Id, columnSet);
						if (entity3.Contains("msnfp_paymentprocessorid") && entity3["msnfp_paymentprocessorid"] != null)
						{
							mSNFP_PaymentMethod.PaymentProcessorId = ((EntityReference)entity3["msnfp_paymentprocessorid"]).Id;
							localContext.TracingService.Trace("Got msnfp_paymentprocessorid from configuration file.");
						}
						else
						{
							mSNFP_PaymentMethod.PaymentProcessorId = null;
							localContext.TracingService.Trace("Did NOT find msnfp_paymentprocessorid.");
						}
					}
					else
					{
						mSNFP_PaymentMethod.PaymentProcessorId = null;
						localContext.TracingService.Trace("Did NOT find msnfp_paymentprocessorid.");
					}
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_PaymentMethod.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_PaymentMethod.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_PaymentMethod.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_PaymentMethod.CustomerId = null;
					mSNFP_PaymentMethod.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_stripecustomerid") && queriedEntityRecord["msnfp_stripecustomerid"] != null)
				{
					mSNFP_PaymentMethod.StripeCustomerId = (string)queriedEntityRecord["msnfp_stripecustomerid"];
					localContext.TracingService.Trace("Got msnfp_stripecustomerid.");
				}
				else
				{
					mSNFP_PaymentMethod.StripeCustomerId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_stripecustomerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone1") && queriedEntityRecord["msnfp_telephone1"] != null)
				{
					mSNFP_PaymentMethod.Telephone1 = (string)queriedEntityRecord["msnfp_telephone1"];
					localContext.TracingService.Trace("Got msnfp_telephone1.");
				}
				else
				{
					mSNFP_PaymentMethod.Telephone1 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone1.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line1") && queriedEntityRecord["msnfp_billing_line1"] != null)
				{
					mSNFP_PaymentMethod.BillingLine1 = (string)queriedEntityRecord["msnfp_billing_line1"];
					localContext.TracingService.Trace("Got msnfp_billing_line1.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingLine1 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line1.");
				}
				if (queriedEntityRecord.Contains("msnfp_authtoken") && queriedEntityRecord["msnfp_authtoken"] != null)
				{
					mSNFP_PaymentMethod.AuthToken = (string)queriedEntityRecord["msnfp_authtoken"];
					localContext.TracingService.Trace("Got msnfp_authtoken.");
				}
				else
				{
					mSNFP_PaymentMethod.AuthToken = null;
					localContext.TracingService.Trace("Did NOT find msnfp_authtoken.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line2") && queriedEntityRecord["msnfp_billing_line2"] != null)
				{
					mSNFP_PaymentMethod.BillingLine2 = (string)queriedEntityRecord["msnfp_billing_line2"];
					localContext.TracingService.Trace("Got msnfp_billing_line2.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingLine2 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line2.");
				}
				if (queriedEntityRecord.Contains("msnfp_isreusable") && queriedEntityRecord["msnfp_isreusable"] != null)
				{
					mSNFP_PaymentMethod.IsReusable = (bool)queriedEntityRecord["msnfp_isreusable"];
					localContext.TracingService.Trace("Got msnfp_isreusable.");
				}
				else
				{
					mSNFP_PaymentMethod.IsReusable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_isreusable.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line3") && queriedEntityRecord["msnfp_billing_line3"] != null)
				{
					mSNFP_PaymentMethod.BillingLine3 = (string)queriedEntityRecord["msnfp_billing_line3"];
					localContext.TracingService.Trace("Got msnfp_billing_line3.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingLine3 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line3.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_postalcode") && queriedEntityRecord["msnfp_billing_postalcode"] != null)
				{
					mSNFP_PaymentMethod.BillingPostalCode = (string)queriedEntityRecord["msnfp_billing_postalcode"];
					localContext.TracingService.Trace("Got msnfp_billing_postalcode.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingPostalCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_postalcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_state") && queriedEntityRecord["msnfp_billing_state"] != null)
				{
					mSNFP_PaymentMethod.BillingState = (string)queriedEntityRecord["msnfp_billing_state"];
					localContext.TracingService.Trace("Got msnfp_billing_state.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingState = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_state.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_country") && queriedEntityRecord["msnfp_billing_country"] != null)
				{
					mSNFP_PaymentMethod.BillingCountry = (string)queriedEntityRecord["msnfp_billing_country"];
					localContext.TracingService.Trace("Got msnfp_billing_country.");
				}
				else
				{
					mSNFP_PaymentMethod.BillingCountry = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_country.");
				}
				if (queriedEntityRecord.Contains("msnfp_abafinancialinstitutionname") && queriedEntityRecord["msnfp_abafinancialinstitutionname"] != null)
				{
					mSNFP_PaymentMethod.AbaFinancialInstitutionName = (string)queriedEntityRecord["msnfp_abafinancialinstitutionname"];
					localContext.TracingService.Trace("Got msnfp_abafinancialinstitutionname.");
				}
				else
				{
					mSNFP_PaymentMethod.AbaFinancialInstitutionName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_abafinancialinstitutionname.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_PaymentMethod.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_PaymentMethod.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_PaymentMethod.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_PaymentMethod.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (queriedEntityRecord.Contains("msnfp_type") && queriedEntityRecord["msnfp_type"] != null)
				{
					mSNFP_PaymentMethod.Type = ((OptionSetValue)queriedEntityRecord["msnfp_type"]).Value;
					localContext.TracingService.Trace("Got msnfp_type.");
				}
				else
				{
					mSNFP_PaymentMethod.Type = null;
					localContext.TracingService.Trace("Did NOT find msnfp_type.");
				}
				if (queriedEntityRecord.Contains("msnfp_nameonbankaccount") && queriedEntityRecord["msnfp_nameonbankaccount"] != null)
				{
					mSNFP_PaymentMethod.NameAsItAppearsOnTheAccount = (string)queriedEntityRecord["msnfp_nameonbankaccount"];
					localContext.TracingService.Trace("Got msnfp_nameonbankaccount.");
				}
				else
				{
					mSNFP_PaymentMethod.NameAsItAppearsOnTheAccount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_nameonbankaccount.");
				}
				if (messageName == "Create")
				{
					mSNFP_PaymentMethod.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_PaymentMethod.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_PaymentMethod.CreatedOn = null;
				}
				mSNFP_PaymentMethod.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_PaymentMethod.Deleted = true;
					mSNFP_PaymentMethod.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_PaymentMethod.Deleted = false;
					mSNFP_PaymentMethod.DeletedDate = null;
				}
				mSNFP_PaymentMethod.EventPackage = new HashSet<MSNFP_EventPackage>();
				mSNFP_PaymentMethod.PaymentSchedule = new HashSet<MSNFP_PaymentSchedule>();
				mSNFP_PaymentMethod.Transaction = new HashSet<MSNFP_Transaction>();
				localContext.TracingService.Trace("JSON object created");
				if (messageName == "Create")
				{
					text = text + str + "/CreatePaymentMethod";
				}
				else if (messageName == "Update" || messageName == "Delete")
				{
					text = text + str + "/UpdatePaymentMethod";
				}
				MemoryStream memoryStream = new MemoryStream();
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_PaymentMethod));
				localContext.TracingService.Trace("Attempt to create JSON via serialization.");
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_PaymentMethod);
				byte[] array = memoryStream.ToArray();
				memoryStream.Close();
				string @string = Encoding.UTF8.GetString(array, 0, array.Length);
				WebAPIClient webAPIClient = new WebAPIClient();
				webAPIClient.Headers[HttpRequestHeader.ContentType] = "application/json";
				webAPIClient.Headers["Padlock"] = (string)configurationRecord["msnfp_apipadlocktoken"];
				webAPIClient.Encoding = Encoding.UTF8;
				localContext.TracingService.Trace("---------Preparing JSON---------");
				localContext.TracingService.Trace("Converted to json API URL : " + text);
				localContext.TracingService.Trace("---------End of Preparing JSON---------");
				localContext.TracingService.Trace("Sending data to Azure.");
				string str2 = webAPIClient.UploadString(text, @string);
				localContext.TracingService.Trace("Got response.");
				localContext.TracingService.Trace("Response: " + str2);
			}
			else
			{
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting workflow.");
			}
		}

		private void updateMonerisVaultProfile(Entity creditCard, Entity paymentProcessor, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			localContext.TracingService.Trace("Entering updateMonerisVaultProfile().");
			if (!creditCard.Contains("msnfp_cclast4") || !creditCard.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				return;
			}
			localContext.TracingService.Trace("Put gathered payment information into vault profile object.");
			string storeId = (string)paymentProcessor["msnfp_storeid"];
			string apiToken = (string)paymentProcessor["msnfp_apikey"];
			bool testMode = paymentProcessor.Contains("msnfp_testmode") && (bool)paymentProcessor["msnfp_testmode"];
			string text = (string)creditCard["msnfp_cclast4"];
			string text2 = (string)creditCard["msnfp_ccexpmmyy"];
			string cryptType = "7";
			string procCountryCode = "CA";
			bool statusCheck = false;
			string str = text2.Substring(0, 2);
			string str2 = text2.Substring(2, 2);
			localContext.TracingService.Trace("Old Expiry format (MMYY):" + text2);
			text2 = str2 + str;
			localContext.TracingService.Trace("Moneris Expiry format (YYMM):" + text2);
			string phone = "";
			string email = "";
			string note = "Modified in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
			string custId = ((EntityReference)creditCard["msnfp_customerid"]).Id.ToString();
			if (creditCard.Contains("msnfp_telephone1"))
			{
				phone = (string)creditCard["msnfp_telephone1"];
			}
			if (creditCard.Contains("msnfp_emailaddress1"))
			{
				email = (string)creditCard["msnfp_emailaddress1"];
			}
			string dataKey = (string)creditCard["msnfp_authtoken"];
			ResUpdateCC resUpdateCC = new ResUpdateCC();
			resUpdateCC.SetDataKey(dataKey);
			resUpdateCC.SetCustId(custId);
			if (text != null && text.Length > 5)
			{
				try
				{
					if (IsDigitsOnly(text.Trim()))
					{
						resUpdateCC.SetPan(text.Trim());
					}
				}
				catch
				{
				}
			}
			resUpdateCC.SetExpDate(text2);
			resUpdateCC.SetPhone(phone);
			resUpdateCC.SetEmail(email);
			resUpdateCC.SetNote(note);
			resUpdateCC.SetCryptType(cryptType);
			AvsInfo avsCheck = new AvsInfo();
			localContext.TracingService.Trace("Check for AVS Validation.");
			if (creditCard.Contains("msnfp_ccbrandcode"))
			{
				if (((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value == 844060004)
				{
					if (paymentProcessor.Contains("msnfp_avsvalidation"))
					{
						if ((bool)paymentProcessor["msnfp_avsvalidation"])
						{
							localContext.TracingService.Trace("AVS Validation = True");
							try
							{
								localContext.TracingService.Trace("Entering address information for AVS validation.");
								avsCheck = AssignAVSValidationFieldsFromPaymentMethod(creditCard, avsCheck, localContext, service);
								resUpdateCC.SetAvsInfo(avsCheck);
							}
							catch
							{
								localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
								throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)creditCard["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)creditCard["msnfp_customerid"]).Id.ToString());
							}
						}
						else
						{
							localContext.TracingService.Trace("AVS Validation = False");
						}
					}
				}
				else
				{
					localContext.TracingService.Trace("Could not do AVS Validation as the card type is not supported. AVS is only supported by Visa(844060000 credit,844060002 debit), MasterCard(844060001 credit,844060003 debit), Discover(844060008) and American Express(844060004).");
					localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)creditCard["msnfp_ccbrandcode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
			}
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			httpsPostRequest.SetProcCountryCode(procCountryCode);
			if (paymentProcessor.Contains("msnfp_testmode"))
			{
				if ((bool)paymentProcessor["msnfp_testmode"])
				{
					localContext.TracingService.Trace("Test Mode is Enabled.");
					httpsPostRequest.SetTestMode(state: true);
				}
				else
				{
					localContext.TracingService.Trace("Test Mode is Disabled.");
					httpsPostRequest.SetTestMode(state: false);
				}
			}
			else
			{
				localContext.TracingService.Trace("Test Mode not set. Defaulting to test mode enabled.");
				httpsPostRequest.SetTestMode(state: true);
			}
			httpsPostRequest.SetStoreId(storeId);
			httpsPostRequest.SetApiToken(apiToken);
			httpsPostRequest.SetTestMode(testMode);
			httpsPostRequest.SetTransaction(resUpdateCC);
			httpsPostRequest.SetStatusCheck(statusCheck);
			localContext.TracingService.Trace("Attempting to create the new user profile in the Moneris Vault.");
			httpsPostRequest.Send();
			try
			{
				Receipt receipt = httpsPostRequest.GetReceipt();
				localContext.TracingService.Trace("---------Moneris Response---------");
				localContext.TracingService.Trace("DataKey = " + receipt.GetDataKey());
				localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
				localContext.TracingService.Trace("Message = " + receipt.GetMessage());
				localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
				localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
				localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
				localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
				localContext.TracingService.Trace("ResSuccess = " + receipt.GetResSuccess());
				localContext.TracingService.Trace("PaymentType = " + receipt.GetPaymentType());
				localContext.TracingService.Trace("Cust ID = " + receipt.GetResDataCustId());
				localContext.TracingService.Trace("Phone = " + receipt.GetResDataPhone());
				localContext.TracingService.Trace("Email = " + receipt.GetResDataEmail());
				localContext.TracingService.Trace("Note = " + receipt.GetResDataNote());
				localContext.TracingService.Trace("Exp Date = " + receipt.GetResDataExpdate());
				localContext.TracingService.Trace("Crypt Type = " + receipt.GetResDataCryptType());
				localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
				localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
				localContext.TracingService.Trace("Avs Street Number = " + receipt.GetResDataAvsStreetNumber());
				localContext.TracingService.Trace("Avs Street Name = " + receipt.GetResDataAvsStreetName());
				localContext.TracingService.Trace("Avs Zipcode = " + receipt.GetResDataAvsZipcode());
				localContext.TracingService.Trace("---------End Moneris Response---------");
				try
				{
					if (text == null)
					{
						return;
					}
					if (IsDigitsOnly(text.Trim()))
					{
						creditCard["msnfp_authtoken"] = receipt.GetDataKey();
						if (receipt.GetDataKey().Length > 0)
						{
							localContext.TracingService.Trace("Masking Card Number.");
							MaskPaymentMethod(localContext, creditCard, receipt.GetDataKey(), null, null, messageName);
						}
					}
					else
					{
						localContext.TracingService.Trace("Credit Card already Masked.");
					}
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error, could not assign data id to auth token.");
					localContext.TracingService.Trace("Error: " + ex.ToString());
					throw new ArgumentNullException("msnfp_authtoken");
				}
			}
			catch (Exception ex2)
			{
				localContext.TracingService.Trace("Error processing response from payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex2.ToString());
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
		}

		private void UpdateStripeCreditCard(Entity creditCard, Entity paymentProcessor, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			localContext.TracingService.Trace("Entering UpdateStripeCreditCard().");
			try
			{
				if (!creditCard.Contains("msnfp_cclast4") || !creditCard.Contains("msnfp_ccexpmmyy"))
				{
					localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
					return;
				}
				string attributeValue = creditCard.GetAttributeValue<string>("msnfp_stripecustomerid");
				string attributeValue2 = paymentProcessor.GetAttributeValue<string>("msnfp_stripeservicekey");
				if (!string.IsNullOrEmpty(attributeValue) && !string.IsNullOrEmpty(attributeValue2))
				{
					BaseStipeRepository baseStipeRepository = new BaseStipeRepository();
					StripeCustomer stripeCustomer = new StripeCustomer();
					stripeCustomer.Id = attributeValue;
					StripeConfiguration.SetApiKey(attributeValue2);
					StripeTokenCreateOptions stripeTokenCreateOptions = new StripeTokenCreateOptions();
					string attributeValue3 = creditCard.GetAttributeValue<string>("msnfp_cclast4");
					string attributeValue4 = creditCard.GetAttributeValue<string>("msnfp_ccexpmmyy");
					string attributeValue5 = creditCard.GetAttributeValue<string>("msnfp_nameonfile");
					stripeTokenCreateOptions.Card = new StripeCreditCardOptions
					{
						Number = attributeValue3,
						ExpirationYear = attributeValue4.Substring(attributeValue4.Length - 2),
						ExpirationMonth = attributeValue4.Substring(0, attributeValue4.Length - 2)
					};
					localContext.TracingService.Trace("Create token for new card.");
					StripeTokenService stripeTokenService = new StripeTokenService();
					StripeToken stripeToken = stripeTokenService.Create(stripeTokenCreateOptions);
					StripeCard stripeCard = new StripeCard();
					stripeCard.SourceToken = stripeToken.Id;
					string url = $"https://api.stripe.com/v1/customers/{stripeCustomer.Id}/sources";
					StripeCard stripeCard2 = baseStipeRepository.Create(stripeCard, url, attributeValue2);
					if (string.IsNullOrEmpty(stripeCard2.Id))
					{
						throw new Exception("UpdateStripeCreditCard - Unable to add card to customer. Returned stripeCard.Id is null or empty.");
					}
					string id = stripeCard2.Id;
					string brand = stripeCard2.Brand;
					localContext.TracingService.Trace("Credit card updated successfully.");
					MaskPaymentMethod(localContext, creditCard, id, brand, attributeValue, messageName);
					return;
				}
				localContext.TracingService.Trace("Error processing response from Stripe payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Customer Key or SecretKey not present.");
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("Error processing response from Stripe payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex.ToString());
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
		}

		private void UpdateIatsCustomerCreditCard(Entity creditCard, Entity paymentProcessor, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			localContext.TracingService.Trace("Entering UpdateIatsCustomerCreditCard().");
			if (!creditCard.Contains("msnfp_cclast4") || !creditCard.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				return;
			}
			string attributeValue = paymentProcessor.GetAttributeValue<string>("msnfp_iatsagentcode");
			string attributeValue2 = paymentProcessor.GetAttributeValue<string>("msnfp_iatspassword");
			string attributeValue3 = creditCard.GetAttributeValue<string>("msnfp_cclast4");
			string attributeValue4 = creditCard.GetAttributeValue<string>("msnfp_ccexpmmyy");
			string attributeValue5 = creditCard.GetAttributeValue<string>("msnfp_nameonfile");
			string str = attributeValue4.Substring(0, 2);
			string str2 = attributeValue4.Substring(2, 2);
			attributeValue4 = str + "/" + str2;
			localContext.TracingService.Trace("New Expiry format (MMYY):" + attributeValue4);
			string text = "Modified in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
			string attributeValue6 = creditCard.GetAttributeValue<string>("msnfp_authtoken");
			try
			{
				UpdateIatsCard(attributeValue3, attributeValue, attributeValue2, attributeValue6, attributeValue4, attributeValue5, localContext);
				MaskPaymentMethod(localContext, creditCard, attributeValue6, null, null, messageName);
				localContext.TracingService.Trace("Masked Card Number.");
				localContext.TracingService.Trace("Updated credit card record");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("Error processing response from iATS payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex.ToString());
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
		}

		private void UpdateIatsCard(string cardNum, string agentCode, string password, string customerCode, string expDate, string custNameOnCard, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("Entering UpdateIatsCard().");
			try
			{
				GetCustomerCodeDetail getCustomerCodeDetail = new GetCustomerCodeDetail();
				getCustomerCodeDetail.agentCode = agentCode;
				getCustomerCodeDetail.password = password;
				getCustomerCodeDetail.customerCode = customerCode;
				XmlDocument customerCodeDetail = iATSProcess.GetCustomerCodeDetail(getCustomerCodeDetail);
				if (customerCodeDetail.InnerText.Contains("Success") && customerCodeDetail.GetElementsByTagName("EXP").Count > 0)
				{
					localContext.TracingService.Trace("Successfully retrieved customer from iATS account.");
					string innerText = customerCodeDetail.GetElementsByTagName("FLN")[0].InnerText;
					UpdateCreditCardCustomerCode updateCreditCardCustomerCode = new UpdateCreditCardCustomerCode();
					updateCreditCardCustomerCode.agentCode = agentCode;
					updateCreditCardCustomerCode.password = password;
					updateCreditCardCustomerCode.customerCode = customerCode;
					updateCreditCardCustomerCode.creditCardNum = cardNum;
					updateCreditCardCustomerCode.updateCreditCardNum = true;
					updateCreditCardCustomerCode.creditCardExpiry = expDate;
					updateCreditCardCustomerCode.beginDate = DateTime.Today;
					updateCreditCardCustomerCode.endDate = DateTime.Today.AddDays(1.0);
					updateCreditCardCustomerCode.recurring = false;
					updateCreditCardCustomerCode.address = customerCodeDetail.GetElementsByTagName("ADD")[0].InnerText;
					updateCreditCardCustomerCode.city = customerCodeDetail.GetElementsByTagName("CTY")[0].InnerText;
					updateCreditCardCustomerCode.state = customerCodeDetail.GetElementsByTagName("ST")[0].InnerText;
					updateCreditCardCustomerCode.companyName = customerCodeDetail.GetElementsByTagName("CO")[0].InnerText;
					updateCreditCardCustomerCode.country = customerCodeDetail.GetElementsByTagName("CNT")[0].InnerText;
					updateCreditCardCustomerCode.creditCardCustomerName = custNameOnCard;
					updateCreditCardCustomerCode.email = customerCodeDetail.GetElementsByTagName("EM")[0].InnerText;
					updateCreditCardCustomerCode.fax = customerCodeDetail.GetElementsByTagName("FX")[0].InnerText;
					updateCreditCardCustomerCode.firstName = innerText.Split(' ')[0];
					updateCreditCardCustomerCode.lastName = string.Join(" ", innerText.Split(' ').Skip(1));
					updateCreditCardCustomerCode.mop = customerCodeDetail.GetElementsByTagName("MB")[0].InnerText;
					updateCreditCardCustomerCode.phone = customerCodeDetail.GetElementsByTagName("PH")[0].InnerText;
					updateCreditCardCustomerCode.zipCode = customerCodeDetail.GetElementsByTagName("ZC")[0].InnerText;
					updateCreditCardCustomerCode.comment = "Modified in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
					XmlDocument xmlDocument = iATSProcess.UpdateCreditCardCustomerCode(updateCreditCardCustomerCode);
					if (xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText.Contains("OK"))
					{
						string innerText2 = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
						localContext.TracingService.Trace("Card updated successfully - " + innerText2);
						return;
					}
					string innerText3 = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
					localContext.TracingService.Trace("Error Details- " + innerText3);
					throw new Exception(innerText3);
				}
				localContext.TracingService.Trace("Error Details- " + customerCodeDetail.InnerText);
				throw new Exception("Error getting response from payment gateway");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("Error processing response from iATS payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex.ToString());
				throw new InvalidPluginExecutionException("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.", ex);
			}
		}

		private Entity getPaymentProcessorForPaymentMethod(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service)
		{
			if (paymentMethod.Contains("msnfp_paymentprocessorid"))
			{
				return service.Retrieve("msnfp_paymentprocessor", ((EntityReference)paymentMethod["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_apikey", "msnfp_name", "msnfp_storeid", "msnfp_avsvalidation", "msnfp_cvdvalidation", "msnfp_testmode", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword", "msnfp_paymentgatewaytype"));
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
					throw new Exception("Donor (" + ((EntityReference)paymentMethod["msnfp_customerid"]).LogicalName + ") with id = " + ((EntityReference)paymentMethod["msnfp_customerid"]).Id.ToString() + " is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
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

		private void MaskPaymentMethod(LocalPluginContext localContext, Entity primarypaymentMethod, string cardId, string cardBrand, string customerId, string messageName)
		{
			localContext.TracingService.Trace("Inside the method MaskPaymentMethod. ");
			string str = (string)(primarypaymentMethod["msnfp_cclast4"] = primarypaymentMethod["msnfp_cclast4"].ToString().Substring(primarypaymentMethod["msnfp_cclast4"].ToString().Length - 4));
			if (cardBrand != null)
			{
				switch (cardBrand)
				{
				case "MasterCard":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060001);
					break;
				case "Visa":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060000);
					break;
				case "American Express":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060004);
					break;
				case "Discover":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060008);
					break;
				case "Diners Club":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060005);
					break;
				case "UnionPay":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060009);
					break;
				case "JCB":
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
					break;
				default:
					primarypaymentMethod["msnfp_ccbrandcode"] = new OptionSetValue(844060010);
					break;
				}
			}
			localContext.TracingService.Trace("Last 4 Digits of CC Number : " + str);
			primarypaymentMethod["msnfp_authtoken"] = cardId;
			primarypaymentMethod["msnfp_stripecustomerid"] = customerId;
			localContext.TracingService.Trace("Credit card record updated...MaskPaymentMethod");
		}

		private bool IsDigitsOnly(string str)
		{
			foreach (char c in str)
			{
				if (c < '0' || c > '9')
				{
					return false;
				}
				if (!char.IsDigit(c))
				{
					return false;
				}
			}
			return true;
		}
	}
}
