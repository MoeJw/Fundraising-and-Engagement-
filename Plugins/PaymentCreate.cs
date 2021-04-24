using System;
using System.Collections.Generic;
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
	public class PaymentCreate : PluginBase
	{
		public PaymentCreate(string unsecure, string secure)
			: base(typeof(PaymentCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered PaymentCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(organizationService);
			string messageName = pluginExecutionContext.MessageName;
			Entity entity = null;
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity2 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity2 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			entity = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering PaymentCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (entity3 != null)
				{
					Entity entity4 = organizationService.Retrieve("msnfp_payment", entity3.Id, GetColumnSet());
					if (entity4 == null)
					{
						throw new ArgumentNullException("msnfp_paymentid");
					}
					if (entity4.Contains("msnfp_name") && entity4.Contains("statuscode"))
					{
						localContext.TracingService.Trace("Payment name (msnfp_name): " + (string)entity4["msnfp_name"]);
						localContext.TracingService.Trace("Payment status reason (statuscode): " + ((OptionSetValue)entity4["statuscode"]).Value);
					}
					else
					{
						localContext.TracingService.Trace("No payment name given.");
					}
					if (entity4.Contains("statuscode") && entity4.Contains("msnfp_paymenttype") && entity4.Contains("msnfp_paymentmethodid"))
					{
						if (((OptionSetValue)entity4["statuscode"]).Value == 844060000 && ((OptionSetValue)entity4["msnfp_paymenttype"]).Value == 844060002)
						{
							Entity entity5 = organizationService.Retrieve("msnfp_paymentmethod", ((EntityReference)entity4["msnfp_paymentmethodid"]).Id, new ColumnSet("msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode", "msnfp_customerid"));
							if (entity5 != null)
							{
								localContext.TracingService.Trace("Obtained payment method for this payment.");
								Entity entity6 = null;
								if (entity5.Contains("msnfp_paymentprocessorid"))
								{
									localContext.TracingService.Trace("Getting payment processor for transaction.");
									entity6 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)entity5["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentgatewaytype"));
									if (entity6.Contains("msnfp_paymentgatewaytype"))
									{
										localContext.TracingService.Trace("Obtained payment gateway for this payment.");
										localContext.TracingService.Trace("Transaction identifier present- " + entity4.Contains("msnfp_transactionidentifier"));
										if (!entity4.Contains("msnfp_transactionidentifier") && messageName == "Create")
										{
											localContext.TracingService.Trace("Payment Gateway- " + ((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value);
											if (((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value == 844060000)
											{
												processMonerisVaultPayment(entity4, localContext, organizationService);
											}
											else if (((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value == 844060001)
											{
												processStripePayment(entity, entity4, localContext, organizationService, singleTransactionYN: false);
											}
											else if (((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value == 844060002)
											{
												ProcessIatsPayment(entity, entity4, localContext, organizationService, singleTransactionYN: false);
											}
											else
											{
												localContext.TracingService.Trace("((OptionSetValue)paymentProcessor[msnfp_paymentgatewaytype]).Value" + ((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value);
											}
											if (!entity5.Contains("msnfp_isreusable") || !(bool)entity5["msnfp_isreusable"])
											{
												entity4["msnfp_paymentmethodid"] = null;
												removePaymentMethod(entity5, localContext, organizationService);
											}
										}
									}
								}
								else
								{
									localContext.TracingService.Trace("There is no payment processor. No payment processed.");
								}
							}
						}
					}
					else
					{
						localContext.TracingService.Trace("No payment type or payment method given.");
					}
					if (messageName == "Create")
					{
						entity4 = organizationService.Retrieve("msnfp_payment", entity3.Id, GetColumnSet());
						UpdateEventPackageTotals(entity4, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(entity4, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						UpdateEventPackageTotals(entity4, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(entity4, entity, localContext, organizationService, pluginExecutionContext);
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting plugin.");
				}
			}
			if (messageName == "Delete")
			{
				Entity entity3 = organizationService.Retrieve("msnfp_payment", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting PaymentCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventpackageid", "statuscode", "msnfp_paymentid", "msnfp_customerid", "msnfp_amount", "msnfp_amount_refunded", "msnfp_name", "msnfp_transactionfraudcode", "msnfp_transactionidentifier", "msnfp_transactionresult", "msnfp_paymenttype", "msnfp_paymentprocessorid", "msnfp_paymentmethodid", "msnfp_ccbrandcodepayment", "msnfp_invoiceidentifier", "msnfp_responseid", "msnfp_amount_balance", "msnfp_configurationid", "msnfp_daterefunded", "msnfp_chequenumber", "statecode", "createdon");
		}

		private void ProcessIatsPayment(Entity configurationRecord, Entity paymentRecord, LocalPluginContext localContext, IOrganizationService service, bool singleTransactionYN)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			Entity entity = null;
			string empty3 = string.Empty;
			Guid empty4 = Guid.Empty;
			Entity entity2 = null;
			Entity entity3 = null;
			decimal num = default(decimal);
			string text = Guid.NewGuid().ToString();
			string text2 = "";
			string agentCode = string.Empty;
			string password = string.Empty;
			XmlDocument xmlDocument = null;
			string text3 = null;
			bool flag = false;
			if (paymentRecord.Contains("transactioncurrencyid") && paymentRecord["transactioncurrencyid"] != null)
			{
				Entity entity4 = service.Retrieve("transactioncurrency", ((EntityReference)paymentRecord["transactioncurrencyid"]).Id, new ColumnSet("isocurrencycode"));
				if (entity4 != null)
				{
					empty2 = (entity4.Contains("isocurrencycode") ? ((string)entity4["isocurrencycode"]) : string.Empty);
				}
			}
			int num2 = (configurationRecord.Contains("msnfp_sche_retryinterval") ? ((int)configurationRecord["msnfp_sche_retryinterval"]) : 0);
			try
			{
				entity = getPaymentMethodForPayment(paymentRecord, localContext, service);
				localContext.TracingService.Trace("Payment method retrieved");
				if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
				{
					localContext.TracingService.Trace("processiATSPayment - Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
					if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
					{
						setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					}
					return;
				}
				if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
				{
					localContext.TracingService.Trace("processIatsPayment - Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
					removePaymentMethod(entity, localContext, service);
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					return;
				}
				entity3 = getPaymentProcessorForPaymentMethod(entity, paymentRecord, localContext, service);
				localContext.TracingService.Trace("Payment processor retrieved.");
				if (entity3 != null)
				{
					agentCode = entity3.GetAttributeValue<string>("msnfp_iatsagentcode");
					password = entity3.GetAttributeValue<string>("msnfp_iatspassword");
				}
				if (paymentRecord.Contains("msnfp_customerid"))
				{
					empty3 = ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName;
					empty4 = ((EntityReference)paymentRecord["msnfp_customerid"]).Id;
					entity2 = ((!(empty3 == "account")) ? service.Retrieve("contact", empty4, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "mobilephone", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")) : service.Retrieve("account", empty4, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "telephone3", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")));
				}
				if (paymentRecord.Contains("msnfp_amount"))
				{
					num = ((Money)paymentRecord["msnfp_amount"]).Value;
				}
				localContext.TracingService.Trace("Donation Amount : " + num);
				if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value == 844060000)
				{
					localContext.TracingService.Trace("iATS credit card payment.");
					if (entity.Contains("msnfp_authtoken"))
					{
						text3 = entity["msnfp_authtoken"] as string;
					}
					else
					{
						localContext.TracingService.Trace("Create new customer for iATS payment.");
						flag = true;
						string text4 = (entity.Contains("msnfp_ccexpmmyy") ? entity.GetAttributeValue<string>("msnfp_ccexpmmyy") : string.Empty);
						string str = text4.Substring(text4.Length - 2);
						string str2 = text4.Substring(0, text4.Length - 2);
						text4 = str2 + "/" + str;
						string creditCardNum = (entity.Contains("msnfp_cclast4") ? entity.GetAttributeValue<string>("msnfp_cclast4") : string.Empty);
						CreateCreditCardCustomerCode createCreditCardCustomerCode = new CreateCreditCardCustomerCode();
						createCreditCardCustomerCode.lastName = ((entity2.LogicalName == "contact") ? entity2.GetAttributeValue<string>("lastname") : string.Empty);
						createCreditCardCustomerCode.firstName = ((entity2.LogicalName == "account") ? entity2.GetAttributeValue<string>("name") : entity2.GetAttributeValue<string>("firstname"));
						createCreditCardCustomerCode.agentCode = agentCode;
						createCreditCardCustomerCode.password = password;
						createCreditCardCustomerCode.beginDate = DateTime.Today;
						createCreditCardCustomerCode.endDate = DateTime.Today.AddDays(1.0);
						createCreditCardCustomerCode.country = entity2.GetAttributeValue<string>("address1_country");
						createCreditCardCustomerCode.creditCardExpiry = text4;
						createCreditCardCustomerCode.creditCardNum = creditCardNum;
						createCreditCardCustomerCode.recurring = false;
						createCreditCardCustomerCode.address = entity2.GetAttributeValue<string>("address1_line1");
						createCreditCardCustomerCode.city = entity2.GetAttributeValue<string>("address1_city");
						createCreditCardCustomerCode.zipCode = entity2.GetAttributeValue<string>("address1_postalcode");
						createCreditCardCustomerCode.state = entity2.GetAttributeValue<string>("address1_stateorprovince");
						createCreditCardCustomerCode.email = entity2.GetAttributeValue<string>("emailaddress1");
						createCreditCardCustomerCode.creditCardCustomerName = ((entity2.LogicalName == "account") ? entity2.GetAttributeValue<string>("name") : (entity2.GetAttributeValue<string>("firstname") + " " + entity2.GetAttributeValue<string>("lastname")));
						XmlDocument xmlDocument2 = iATSProcess.CreateCreditCardCustomerCode(createCreditCardCustomerCode);
						localContext.TracingService.Trace(xmlDocument2.InnerXml);
						XmlNodeList elementsByTagName = xmlDocument2.GetElementsByTagName("AUTHORIZATIONRESULT");
						foreach (XmlNode item in elementsByTagName)
						{
							string innerText = item.InnerText;
							localContext.TracingService.Trace("Auth Result- " + item.InnerText);
							if (innerText.Contains("OK"))
							{
								text3 = xmlDocument2.GetElementsByTagName("CUSTOMERCODE")[0].InnerText;
							}
						}
						localContext.TracingService.Trace("Mask the credit card.");
						MaskStripeCreditCard(localContext, entity, text3, null, null);
					}
					if (!string.IsNullOrEmpty(text3))
					{
						localContext.TracingService.Trace("Payment Method is Credit Card.");
						ProcessCreditCardWithCustomerCode processCreditCardWithCustomerCode = new ProcessCreditCardWithCustomerCode();
						processCreditCardWithCustomerCode.agentCode = agentCode;
						processCreditCardWithCustomerCode.password = password;
						processCreditCardWithCustomerCode.customerCode = text3;
						processCreditCardWithCustomerCode.invoiceNum = (paymentRecord.Contains("msnfp_missioninvoiceidentifier") ? ((string)paymentRecord["msnfp_missioninvoiceidentifier"]) : text);
						processCreditCardWithCustomerCode.total = $"{num:0.00}";
						localContext.TracingService.Trace("Donation Amount : " + $"{num:0.00}");
						processCreditCardWithCustomerCode.comment = "Debited by Dynamics 365 on " + DateTime.Now.ToString();
						xmlDocument = iATSProcess.ProcessCreditCardWithCustomerCode(processCreditCardWithCustomerCode);
						localContext.TracingService.Trace("Process complete to Payment with Credit Card.");
					}
				}
				if (xmlDocument != null)
				{
					XmlNodeList elementsByTagName2 = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT");
					Entity entity5 = new Entity("msnfp_response");
					entity5["msnfp_paymentid"] = new EntityReference("msnfp_payment", (Guid)paymentRecord["msnfp_paymentid"]);
					entity5["msnfp_identifier"] = "Response for " + (string)paymentRecord["msnfp_name"];
					if (paymentRecord.Contains("msnfp_eventpackageid") && paymentRecord["msnfp_eventpackageid"] != null)
					{
						entity5["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)paymentRecord["msnfp_eventpackageid"]).Id);
					}
					foreach (XmlNode item2 in elementsByTagName2)
					{
						entity5["msnfp_response"] = item2.InnerText;
						string innerText2 = item2.InnerText;
						if (innerText2.Contains("OK"))
						{
							localContext.TracingService.Trace("Got successful response from iATS payment gateway.");
							text2 = text2 + "---------Start iATS Response---------" + Environment.NewLine;
							text2 = text2 + "TransStatus = " + item2.InnerText + Environment.NewLine;
							text2 = text2 + "TransAmount = " + num + Environment.NewLine;
							text2 = text2 + "Auth Token = " + text3 + Environment.NewLine;
							text2 += "---------End iATS Response---------";
							localContext.TracingService.Trace("processiATSTransaction - Got successful response from iATS payment gateway.");
							entity5["msnfp_response"] = text2;
							paymentRecord["msnfp_transactionresult"] = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
							paymentRecord["msnfp_transactionidentifier"] = xmlDocument.GetElementsByTagName("TRANSACTIONID")[0].InnerText;
							paymentRecord["statuscode"] = new OptionSetValue(844060000);
						}
						else
						{
							localContext.TracingService.Trace("Got failure response from iATS payment gateway.");
							entity5["msnfp_response"] = "FAILED";
							localContext.TracingService.Trace("Status code updated to failed");
							paymentRecord["statuscode"] = new OptionSetValue(844060003);
							paymentRecord["msnfp_transactionresult"] = "FAILED";
							localContext.TracingService.Trace("Gateway Response Message." + xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText);
						}
					}
					paymentRecord["msnfp_invoiceidentifier"] = text;
					Guid id = service.Create(entity5);
					bool flag2 = true;
					paymentRecord["msnfp_responseid"] = new EntityReference("msnfp_response", id);
				}
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("processStripePayment - error : " + ex.Message);
				paymentRecord["statuscode"] = new OptionSetValue(844060003);
				localContext.TracingService.Trace("processStripePayment - Status code updated to failed");
				paymentRecord["msnfp_transactionresult"] = "FAILED";
			}
			service.Update(paymentRecord);
			localContext.TracingService.Trace("processIatsPayment - Entity Updated.");
		}

		private void processStripePayment(Entity configurationRecord, Entity paymentRecord, LocalPluginContext localContext, IOrganizationService service, bool singleTransactionYN)
		{
			string empty = string.Empty;
			string text = string.Empty;
			Entity entity = null;
			string text2 = "";
			string empty2 = string.Empty;
			Guid empty3 = Guid.Empty;
			Entity entity2 = null;
			Entity entity3 = null;
			decimal d = default(decimal);
			bool flag = false;
			string text3 = Guid.NewGuid().ToString();
			string str = "";
			if (paymentRecord.Contains("transactioncurrencyid") && paymentRecord["transactioncurrencyid"] != null)
			{
				Entity entity4 = service.Retrieve("transactioncurrency", ((EntityReference)paymentRecord["transactioncurrencyid"]).Id, new ColumnSet("isocurrencycode"));
				if (entity4 != null)
				{
					text = (entity4.Contains("isocurrencycode") ? ((string)entity4["isocurrencycode"]) : string.Empty);
				}
			}
			int num = (configurationRecord.Contains("msnfp_sche_retryinterval") ? ((int)configurationRecord["msnfp_sche_retryinterval"]) : 0);
			try
			{
				StripeCustomer stripeCustomer = null;
				string text4 = null;
				BaseStipeRepository baseStipeRepository = new BaseStipeRepository();
				entity = getPaymentMethodForPayment(paymentRecord, localContext, service);
				if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
				{
					localContext.TracingService.Trace("processStripePayment - Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
					if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
					{
						setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					}
					return;
				}
				if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
				{
					localContext.TracingService.Trace("processStripePayment - Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
					removePaymentMethod(entity, localContext, service);
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					return;
				}
				entity3 = getPaymentProcessorForPaymentMethod(entity, paymentRecord, localContext, service);
				string text5 = entity3["msnfp_stripeservicekey"].ToString();
				StripeConfiguration.SetApiKey(text5);
				if (paymentRecord.Contains("msnfp_customerid"))
				{
					empty2 = ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName;
					empty3 = ((EntityReference)paymentRecord["msnfp_customerid"]).Id;
					entity2 = ((!(empty2 == "account")) ? service.Retrieve("contact", empty3, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "mobilephone", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")) : service.Retrieve("account", empty3, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "telephone3", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")));
				}
				if (entity.Contains("msnfp_stripecustomerid") && entity["msnfp_stripecustomerid"] != null && entity.Contains("msnfp_authtoken") && entity["msnfp_authtoken"] != null)
				{
					localContext.TracingService.Trace("processStripePayment - Existing Card use");
					string customerId = entity["msnfp_stripecustomerid"].ToString();
					text4 = entity["msnfp_authtoken"].ToString();
					int? num2 = ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value;
					if (num2.HasValue)
					{
						text2 = num2 switch
						{
							844060001 => "MasterCard", 
							844060000 => "Visa", 
							844060004 => "American Express", 
							844060008 => "Discover", 
							844060005 => "Diners Club", 
							844060009 => "UnionPay", 
							844060006 => "JCB", 
							_ => "Unknown", 
						};
					}
					StripeConfiguration.SetApiKey(text5);
					StripeCustomerService stripeCustomerService = new StripeCustomerService();
					stripeCustomer = stripeCustomerService.Get(customerId);
				}
				else
				{
					localContext.TracingService.Trace("processStripePayment - New Card use");
					flag = true;
					string custName = ((entity2.LogicalName == "account") ? entity2["name"].ToString() : (entity2["firstname"].ToString() + entity2["lastname"].ToString()));
					string custEmail = (entity2.Contains("emailaddress1") ? entity2["emailaddress1"].ToString() : string.Empty);
					localContext.TracingService.Trace("processStripePayment - extracting customer info - done");
					stripeCustomer = new CustomerService().GetStripeCustomer(custName, custEmail, text5);
					localContext.TracingService.Trace("processStripePayment - obtained stripeCustomer");
					StripeTokenCreateOptions stripeTokenCreateOptions = new StripeTokenCreateOptions();
					string text6 = (entity.Contains("msnfp_ccexpmmyy") ? entity["msnfp_ccexpmmyy"].ToString() : string.Empty);
					stripeTokenCreateOptions.Card = new StripeCreditCardOptions
					{
						Number = entity["msnfp_cclast4"].ToString(),
						ExpirationYear = text6.Substring(text6.Length - 2),
						ExpirationMonth = text6.Substring(0, text6.Length - 2)
					};
					StripeTokenService stripeTokenService = new StripeTokenService();
					StripeToken stripeToken = stripeTokenService.Create(stripeTokenCreateOptions);
					StripeCard stripeCard = new StripeCard();
					stripeCard.SourceToken = stripeToken.Id;
					string url = $"https://api.stripe.com/v1/customers/{stripeCustomer.Id}/sources";
					StripeCard stripeCard2 = baseStipeRepository.Create(stripeCard, url, text5);
					if (string.IsNullOrEmpty(stripeCard2.Id))
					{
						throw new Exception("processStripePayment - Unable to add card to customer");
					}
					text4 = stripeCard2.Id;
					text2 = stripeCard2.Brand;
					MaskStripeCreditCard(localContext, entity, text4, text2, stripeCustomer.Id);
				}
				if (paymentRecord.Contains("msnfp_amount"))
				{
					d = ((Money)paymentRecord["msnfp_amount"]).Value;
				}
				int amount = Convert.ToInt32((d * 100m).ToString().Split('.')[0]);
				StripeCharge stripeCharge = new StripeCharge();
				stripeCharge.Amount = amount;
				stripeCharge.Currency = (string.IsNullOrEmpty(text) ? "CAD" : text);
				stripeCharge.Customer = stripeCustomer;
				Source source = new Source();
				source.Id = text4;
				stripeCharge.Source = source;
				stripeCharge.Description = (paymentRecord.Contains("msnfp_invoiceidentifier") ? ((string)paymentRecord["msnfp_invoiceidentifier"]) : text3);
				StripeCharge stripeCharge2 = baseStipeRepository.Create(stripeCharge, "https://api.stripe.com/v1/charges", text5);
				Entity entity5 = new Entity("msnfp_response");
				entity5["msnfp_paymentid"] = new EntityReference("msnfp_payment", (Guid)paymentRecord["msnfp_paymentid"]);
				entity5["msnfp_identifier"] = "Response for " + (string)paymentRecord["msnfp_name"];
				if (paymentRecord.Contains("msnfp_eventpackageid") && paymentRecord["msnfp_eventpackageid"] != null)
				{
					entity5["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)paymentRecord["msnfp_eventpackageid"]).Id);
				}
				if (!string.IsNullOrEmpty(stripeCharge2.FailureMessage))
				{
					entity5["msnfp_response"] = "FAILED";
					paymentRecord["statuscode"] = new OptionSetValue(844060003);
					paymentRecord["msnfp_transactionresult"] = "FAILED";
				}
				else
				{
					localContext.TracingService.Trace("processStripeOneTimeTransaction - stripePayment.Id : " + stripeCharge2.Id);
					if (stripeCharge2 != null)
					{
						localContext.TracingService.Trace("processStripeOneTimeTransaction - stripePayment.InvoiceId : " + stripeCharge2.InvoiceId);
						localContext.TracingService.Trace("processStripeOneTimeTransaction - stripePayment.Status : " + stripeCharge2.Status.ToString());
						if (stripeCharge2.Status.Equals("succeeded"))
						{
							str = str + "---------Start Stripe Response---------" + Environment.NewLine;
							str = str + "TransAmount = " + stripeCharge2.Status + Environment.NewLine;
							str = str + "TransAmount = " + d + Environment.NewLine;
							str = str + "Auth Token = " + text4 + Environment.NewLine;
							str += "---------End Stripe Response---------";
							localContext.TracingService.Trace("processStripePayment - Got successful response from Stripe payment gateway.");
							entity5["msnfp_response"] = str;
							paymentRecord["msnfp_transactionresult"] = stripeCharge2.Status;
							paymentRecord["msnfp_transactionidentifier"] = stripeCharge2.Id;
							paymentRecord["statuscode"] = new OptionSetValue(844060000);
							if (text2 != null)
							{
								localContext.TracingService.Trace("Card Type Response Code = " + text2);
								switch (text2)
								{
								case "MasterCard":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060001);
									break;
								case "Visa":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060000);
									break;
								case "American Express":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060004);
									break;
								case "Discover":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060008);
									break;
								case "Diners Club":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060005);
									break;
								case "UnionPay":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060009);
									break;
								case "JCB":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060006);
									break;
								default:
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060010);
									break;
								}
							}
							localContext.TracingService.Trace("processStripePayment - Updated Payment Record.");
						}
						else
						{
							localContext.TracingService.Trace("processStripePayment - Got failure response from payment gateway.");
							entity5["msnfp_response"] = stripeCharge2.StripeResponse.ToString();
							paymentRecord["statuscode"] = new OptionSetValue(844060003);
							localContext.TracingService.Trace("processStripePayment - Status code updated to failed");
							paymentRecord["msnfp_transactionidentifier"] = stripeCharge2.Id;
							paymentRecord["msnfp_transactionresult"] = stripeCharge2.Status;
							localContext.TracingService.Trace("Gateway Response Message." + stripeCharge2.Status);
						}
					}
				}
				paymentRecord["msnfp_invoiceidentifier"] = text3;
				Guid id = service.Create(entity5);
				bool flag2 = true;
				paymentRecord["msnfp_responseid"] = new EntityReference("msnfp_response", id);
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("processStripePayment - error : " + ex.Message);
				paymentRecord["statuscode"] = new OptionSetValue(844060003);
				localContext.TracingService.Trace("processStripePayment - Status code updated to failed");
				paymentRecord["msnfp_transactionresult"] = "FAILED";
			}
			service.Update(paymentRecord);
			localContext.TracingService.Trace("processStripePayment - Entity Updated.");
		}

		private string processMonerisOneTimePayment(Entity paymentRecord, LocalPluginContext localContext, IOrganizationService service)
		{
			string result = "";
			localContext.TracingService.Trace("Entering processMonerisOneTimePayment().");
			Entity entity = null;
			Entity entity2 = null;
			localContext.TracingService.Trace("Gathering transaction data from target id.");
			entity = getPaymentMethodForPayment(paymentRecord, localContext, service);
			if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
			{
				localContext.TracingService.Trace("Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
				if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
				{
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				}
				return result;
			}
			if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				removePaymentMethod(entity, localContext, service);
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				return result;
			}
			entity2 = getPaymentProcessorForPaymentMethod(entity, paymentRecord, localContext, service);
			localContext.TracingService.Trace("Put gathered payment information into purchase object.");
			string text = Guid.NewGuid().ToString();
			string storeId = (string)entity2["msnfp_storeid"];
			string apiToken = (string)entity2["msnfp_apikey"];
			string amount = ((Money)paymentRecord["msnfp_amount"]).Value.ToString();
			string pan = (string)entity["msnfp_cclast4"];
			string text2 = (string)entity["msnfp_ccexpmmyy"];
			string cryptType = "7";
			string procCountryCode = "CA";
			bool statusCheck = false;
			string str = text2.Substring(0, 2);
			string str2 = text2.Substring(2, 2);
			localContext.TracingService.Trace("Old Expiry format (MMYY):" + text2);
			text2 = str2 + str;
			localContext.TracingService.Trace("Moneris Expiry format (YYMM):" + text2);
			localContext.TracingService.Trace("Creating Moneris purchase object.");
			Purchase purchase = new Purchase();
			purchase.SetOrderId(text);
			purchase.SetAmount(amount);
			purchase.SetPan(pan);
			purchase.SetExpDate(text2);
			purchase.SetCryptType(cryptType);
			purchase.SetDynamicDescriptor("2134565");
			localContext.TracingService.Trace("Check for AVS Validation.");
			AvsInfo avsCheck = new AvsInfo();
			if (entity.Contains("msnfp_ccbrandcode"))
			{
				if (((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060004)
				{
					if (entity2.Contains("msnfp_avsvalidation"))
					{
						if ((bool)entity2["msnfp_avsvalidation"])
						{
							localContext.TracingService.Trace("AVS Validation = True");
							if (!paymentRecord.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("No Donor. Exiting plugin.");
								setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
								throw new ArgumentNullException("msnfp_customerid");
							}
							try
							{
								localContext.TracingService.Trace("Entering address information for AVS validation.");
								avsCheck = AssignAVSValidationFieldsFromPaymentMethod(paymentRecord, entity, avsCheck, localContext, service);
								purchase.SetAvsInfo(avsCheck);
							}
							catch
							{
								localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
								setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
								throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString());
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
					localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
			}
			localContext.TracingService.Trace("Creating HttpsPostRequest object.");
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			try
			{
				httpsPostRequest.SetProcCountryCode(procCountryCode);
				if (entity2.Contains("msnfp_testmode"))
				{
					if ((bool)entity2["msnfp_testmode"])
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
				httpsPostRequest.SetTransaction(purchase);
				httpsPostRequest.SetStatusCheck(statusCheck);
				localContext.TracingService.Trace("Sending Moneris HttpsPostRequest.");
				httpsPostRequest.Send();
				localContext.TracingService.Trace("HttpsPostRequest sent successfully!");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("HttpsPostRequest Error: " + ex.ToString());
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				removePaymentMethod(entity, localContext, service);
				return result;
			}
			try
			{
				Receipt receipt = httpsPostRequest.GetReceipt();
				string str3 = "";
				localContext.TracingService.Trace("---------Moneris Response---------");
				localContext.TracingService.Trace("CardType = " + receipt.GetCardType());
				localContext.TracingService.Trace("TransAmount = " + receipt.GetTransAmount());
				localContext.TracingService.Trace("TxnNumber = " + receipt.GetTxnNumber());
				localContext.TracingService.Trace("ReceiptId = " + receipt.GetReceiptId());
				localContext.TracingService.Trace("TransType = " + receipt.GetTransType());
				localContext.TracingService.Trace("ReferenceNum = " + receipt.GetReferenceNum());
				localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
				localContext.TracingService.Trace("ISO = " + receipt.GetISO());
				localContext.TracingService.Trace("BankTotals = " + receipt.GetBankTotals());
				localContext.TracingService.Trace("Message = " + receipt.GetMessage());
				localContext.TracingService.Trace("AuthCode = " + receipt.GetAuthCode());
				localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
				localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
				localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
				localContext.TracingService.Trace("Ticket = " + receipt.GetTicket());
				localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
				localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
				localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
				localContext.TracingService.Trace("ITD Response = " + receipt.GetITDResponse());
				localContext.TracingService.Trace("IsVisaDebit = " + receipt.GetIsVisaDebit());
				localContext.TracingService.Trace("---------End Moneris Response---------");
				str3 = str3 + "---------Moneris Response---------" + Environment.NewLine;
				str3 = str3 + "CardType = " + receipt.GetCardType() + Environment.NewLine;
				str3 = str3 + "TransAmount = " + receipt.GetTransAmount() + Environment.NewLine;
				str3 = str3 + "TxnNumber = " + receipt.GetTxnNumber() + Environment.NewLine;
				str3 = str3 + "ReceiptId = " + receipt.GetReceiptId() + Environment.NewLine;
				str3 = str3 + "TransType = " + receipt.GetTransType() + Environment.NewLine;
				str3 = str3 + "ReferenceNum = " + receipt.GetReferenceNum() + Environment.NewLine;
				str3 = str3 + "ResponseCode = " + receipt.GetResponseCode() + Environment.NewLine;
				str3 = str3 + "ISO = " + receipt.GetISO() + Environment.NewLine;
				str3 = str3 + "BankTotals = " + receipt.GetBankTotals() + Environment.NewLine;
				str3 = str3 + "Message = " + receipt.GetMessage() + Environment.NewLine;
				str3 = str3 + "AuthCode = " + receipt.GetAuthCode() + Environment.NewLine;
				str3 = str3 + "Complete = " + receipt.GetComplete() + Environment.NewLine;
				str3 = str3 + "TransDate = " + receipt.GetTransDate() + Environment.NewLine;
				str3 = str3 + "TransTime = " + receipt.GetTransTime() + Environment.NewLine;
				str3 = str3 + "Ticket = " + receipt.GetTicket() + Environment.NewLine;
				str3 = str3 + "TimedOut = " + receipt.GetTimedOut() + Environment.NewLine;
				str3 = str3 + "Avs Response = " + receipt.GetAvsResultCode() + Environment.NewLine;
				str3 = str3 + "Cvd Response = " + receipt.GetCvdResultCode() + Environment.NewLine;
				str3 = str3 + "ITD Response = " + receipt.GetITDResponse() + Environment.NewLine;
				str3 = str3 + "IsVisaDebit = " + receipt.GetIsVisaDebit() + Environment.NewLine;
				str3 += "---------End Moneris Response---------";
				if (receipt.GetResponseCode() != null)
				{
					if (!int.TryParse(receipt.GetResponseCode(), out var result2))
					{
						localContext.TracingService.Trace("Error: Response code is not a number = " + receipt.GetResponseCode());
						setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
						removePaymentMethod(entity, localContext, service);
						return result;
					}
					if (result2 < 50)
					{
						setStatusCodeOnPayment(paymentRecord, 844060000, localContext, service);
					}
					else
					{
						setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
						removePaymentMethod(entity, localContext, service);
					}
				}
				localContext.TracingService.Trace("Creating response record with response: " + receipt.GetMessage());
				Entity entity3 = new Entity("msnfp_response");
				entity3["msnfp_identifier"] = "Response for " + (string)paymentRecord["msnfp_name"];
				entity3["msnfp_response"] = str3;
				entity3["msnfp_paymentid"] = new EntityReference("msnfp_payment", (Guid)paymentRecord["msnfp_paymentid"]);
				if (paymentRecord.Contains("msnfp_eventpackageid") && paymentRecord["msnfp_eventpackageid"] != null)
				{
					entity3["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)paymentRecord["msnfp_eventpackageid"]).Id);
				}
				Guid guid = service.Create(entity3);
				bool flag = true;
				ITracingService tracingService = localContext.TracingService;
				Guid guid2 = guid;
				tracingService.Trace("Response created (" + guid2.ToString() + "). Linking response record to transaction.");
				paymentRecord["msnfp_responseid"] = new EntityReference("msnfp_response", guid);
				if (receipt.GetResponseCode() != null && int.TryParse(receipt.GetResponseCode(), out var result3) && result3 < 50)
				{
					localContext.TracingService.Trace("Setting msnfp_transactionidentifier = " + receipt.GetReferenceNum());
					localContext.TracingService.Trace("Setting msnfp_transactionnumber = " + receipt.GetTxnNumber());
					localContext.TracingService.Trace("Setting order_id = " + text);
					paymentRecord["msnfp_transactionidentifier"] = receipt.GetReferenceNum();
					paymentRecord["msnfp_transactionnumber"] = receipt.GetTxnNumber();
					paymentRecord["msnfp_invoiceidentifier"] = text;
					paymentRecord["msnfp_transactionresult"] = "Approved - " + result3;
					if (receipt.GetCardType() != null)
					{
						localContext.TracingService.Trace("Card Type Response Code = " + receipt.GetCardType());
						switch (receipt.GetCardType())
						{
						case "M":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060001);
							break;
						case "V":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060000);
							break;
						case "AX":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060004);
							break;
						case "NO":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060008);
							break;
						case "D":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060007);
							break;
						case "DC":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060005);
							break;
						case "C1":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060006);
							break;
						case "JCB":
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060006);
							break;
						default:
							paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060010);
							break;
						}
					}
				}
				try
				{
					entity = service.Retrieve("msnfp_paymentmethod", ((EntityReference)paymentRecord["msnfp_paymentmethodid"]).Id, new ColumnSet("msnfp_cclast4", "msnfp_ccexpmmyy", "msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode"));
					if (entity == null)
					{
						localContext.TracingService.Trace("Clear Payment Method lookup on this payment.");
						paymentRecord["msnfp_paymentmethodid"] = null;
					}
				}
				catch (Exception)
				{
					localContext.TracingService.Trace("Could not find Payment Method. Clear Payment Method lookup on this payment record.");
					paymentRecord["msnfp_paymentmethodid"] = null;
				}
				service.Update(paymentRecord);
				localContext.TracingService.Trace("Setting return response code: " + receipt.GetResponseCode());
				result = receipt.GetResponseCode();
			}
			catch (Exception ex3)
			{
				localContext.TracingService.Trace("Receipt Error: " + ex3.ToString());
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				removePaymentMethod(entity, localContext, service);
			}
			return result;
		}

		private void processMonerisVaultPayment(Entity paymentRecord, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering processMonerisVaultPayment().");
			Entity entity = null;
			Entity entity2 = null;
			localContext.TracingService.Trace("Gathering transaction data from target id.");
			entity = getPaymentMethodForPayment(paymentRecord, localContext, service);
			if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
			{
				localContext.TracingService.Trace("Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
				if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
				{
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				}
				return;
			}
			if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				removePaymentMethod(entity, localContext, service);
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				return;
			}
			entity2 = getPaymentProcessorForPaymentMethod(entity, paymentRecord, localContext, service);
			if (!entity.Contains("msnfp_authtoken") || entity["msnfp_authtoken"] == null)
			{
				localContext.TracingService.Trace("No data id found for customer. Attempting to process the payment and if successful create a new Moneris Vault profile with this transaction.");
				string text = processMonerisOneTimePayment(paymentRecord, localContext, service);
				if (int.TryParse(text, out var result))
				{
					if (result < 50)
					{
						localContext.TracingService.Trace("Response was Approved. Now add to vault.");
						addMonerisVaultProfile(paymentRecord, localContext, service);
					}
					else
					{
						localContext.TracingService.Trace("Response code: " + text + ". Please check payment details. Exiting plugin.");
						setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					}
				}
			}
			else
			{
				if (!entity.Contains("msnfp_authtoken"))
				{
					return;
				}
				localContext.TracingService.Trace("Data id found for customer.");
				string custId = ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString();
				localContext.TracingService.Trace("Put gathered payment information into purchase object.");
				string text2 = Guid.NewGuid().ToString();
				string storeId = (string)entity2["msnfp_storeid"];
				string apiToken = (string)entity2["msnfp_apikey"];
				string amount = ((Money)paymentRecord["msnfp_amount"]).Value.ToString();
				string procCountryCode = "CA";
				bool statusCheck = false;
				string dataKey = (string)entity["msnfp_authtoken"];
				string cryptType = "7";
				string dynamicDescriptor = "Created in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
				localContext.TracingService.Trace("Creating ResPurchaseCC object.");
				ResPurchaseCC resPurchaseCC = new ResPurchaseCC();
				resPurchaseCC.SetDataKey(dataKey);
				resPurchaseCC.SetOrderId(text2);
				resPurchaseCC.SetCustId(custId);
				resPurchaseCC.SetAmount(amount);
				resPurchaseCC.SetCryptType(cryptType);
				resPurchaseCC.SetDynamicDescriptor(dynamicDescriptor);
				localContext.TracingService.Trace("Check for AVS Validation.");
				AvsInfo avsCheck = new AvsInfo();
				if (entity.Contains("msnfp_ccbrandcode"))
				{
					if (((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060004)
					{
						if (entity2.Contains("msnfp_avsvalidation"))
						{
							if ((bool)entity2["msnfp_avsvalidation"])
							{
								localContext.TracingService.Trace("AVS Validation = True");
								if (!paymentRecord.Contains("msnfp_customerid"))
								{
									localContext.TracingService.Trace("No Donor. Exiting plugin.");
									setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
									throw new ArgumentNullException("msnfp_customerid");
								}
								try
								{
									localContext.TracingService.Trace("Entering address information for AVS validation.");
									avsCheck = AssignAVSValidationFieldsFromPaymentMethod(paymentRecord, entity, avsCheck, localContext, service);
									resPurchaseCC.SetAvsInfo(avsCheck);
								}
								catch
								{
									localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
									setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
									throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString());
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
						localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value);
					}
				}
				else
				{
					localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
				}
				HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
				httpsPostRequest.SetProcCountryCode(procCountryCode);
				if (entity2.Contains("msnfp_testmode"))
				{
					if ((bool)entity2["msnfp_testmode"])
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
				httpsPostRequest.SetTransaction(resPurchaseCC);
				httpsPostRequest.SetStatusCheck(statusCheck);
				localContext.TracingService.Trace("Sending request.");
				httpsPostRequest.Send();
				localContext.TracingService.Trace("Request sent successfully.");
				try
				{
					Receipt receipt = httpsPostRequest.GetReceipt();
					string str = "";
					localContext.TracingService.Trace("---------Moneris Response---------");
					localContext.TracingService.Trace("DataKey = " + receipt.GetDataKey());
					localContext.TracingService.Trace("ReceiptId = " + receipt.GetReceiptId());
					localContext.TracingService.Trace("ReferenceNum = " + receipt.GetReferenceNum());
					localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
					localContext.TracingService.Trace("AuthCode = " + receipt.GetAuthCode());
					localContext.TracingService.Trace("Message = " + receipt.GetMessage());
					localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
					localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
					localContext.TracingService.Trace("TransType = " + receipt.GetTransType());
					localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
					localContext.TracingService.Trace("TransAmount = " + receipt.GetTransAmount());
					localContext.TracingService.Trace("CardType = " + receipt.GetCardType());
					localContext.TracingService.Trace("TxnNumber = " + receipt.GetTxnNumber());
					localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
					localContext.TracingService.Trace("ResSuccess = " + receipt.GetResSuccess());
					localContext.TracingService.Trace("PaymentType = " + receipt.GetPaymentType());
					localContext.TracingService.Trace("IsVisaDebit = " + receipt.GetIsVisaDebit());
					localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
					localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
					localContext.TracingService.Trace("---------Customer---------");
					localContext.TracingService.Trace("Cust ID = " + receipt.GetResDataCustId());
					localContext.TracingService.Trace("Phone = " + receipt.GetResDataPhone());
					localContext.TracingService.Trace("Email = " + receipt.GetResDataEmail());
					localContext.TracingService.Trace("Note = " + receipt.GetResDataNote());
					localContext.TracingService.Trace("Exp Date (YYMM) = " + receipt.GetResDataExpdate());
					localContext.TracingService.Trace("Crypt Type = " + receipt.GetResDataCryptType());
					localContext.TracingService.Trace("Avs Street Number = " + receipt.GetResDataAvsStreetNumber());
					localContext.TracingService.Trace("Avs Street Name = " + receipt.GetResDataAvsStreetName());
					localContext.TracingService.Trace("Avs Zipcode = " + receipt.GetResDataAvsZipcode());
					localContext.TracingService.Trace("---------End Customer---------");
					localContext.TracingService.Trace("---------End Moneris Response---------");
					str = str + "---------Moneris Response---------" + Environment.NewLine;
					str = str + "DataKey = " + receipt.GetDataKey() + Environment.NewLine;
					str = str + "ReceiptId = " + receipt.GetReceiptId() + Environment.NewLine;
					str = str + "ReferenceNum = " + receipt.GetReferenceNum() + Environment.NewLine;
					str = str + "ResponseCode = " + receipt.GetResponseCode() + Environment.NewLine;
					str = str + "AuthCode = " + receipt.GetAuthCode() + Environment.NewLine;
					str = str + "Message = " + receipt.GetMessage() + Environment.NewLine;
					str = str + "TransDate = " + receipt.GetTransDate() + Environment.NewLine;
					str = str + "TransTime = " + receipt.GetTransTime() + Environment.NewLine;
					str = str + "TransType = " + receipt.GetTransType() + Environment.NewLine;
					str = str + "Complete = " + receipt.GetComplete() + Environment.NewLine;
					str = str + "TransAmount = " + receipt.GetTransAmount() + Environment.NewLine;
					str = str + "CardType = " + receipt.GetCardType() + Environment.NewLine;
					str = str + "TxnNumber = " + receipt.GetTxnNumber() + Environment.NewLine;
					str = str + "TimedOut = " + receipt.GetTimedOut() + Environment.NewLine;
					str = str + "ResSuccess = " + receipt.GetResSuccess() + Environment.NewLine;
					str = str + "PaymentType = " + receipt.GetPaymentType() + Environment.NewLine;
					str = str + "IsVisaDebit = " + receipt.GetIsVisaDebit() + Environment.NewLine;
					str = str + "Avs Response = " + receipt.GetAvsResultCode() + Environment.NewLine;
					str = str + "Cvd Response = " + receipt.GetCvdResultCode() + Environment.NewLine;
					str = str + "---------Customer---------" + Environment.NewLine;
					str = str + "Cust ID = " + receipt.GetResDataCustId() + Environment.NewLine;
					str = str + "Phone = " + receipt.GetResDataPhone() + Environment.NewLine;
					str = str + "Email = " + receipt.GetResDataEmail() + Environment.NewLine;
					str = str + "Note = " + receipt.GetResDataNote() + Environment.NewLine;
					str = str + "Exp Date (YYMM) = " + receipt.GetResDataExpdate() + Environment.NewLine;
					str = str + "Crypt Type = " + receipt.GetResDataCryptType() + Environment.NewLine;
					str = str + "Avs Street Number = " + receipt.GetResDataAvsStreetNumber() + Environment.NewLine;
					str = str + "Avs Street Name = " + receipt.GetResDataAvsStreetName() + Environment.NewLine;
					str = str + "Avs Zipcode = " + receipt.GetResDataAvsZipcode() + Environment.NewLine;
					str = str + "---------End Customer---------" + Environment.NewLine;
					str = str + "---------End Moneris Response---------" + Environment.NewLine;
					localContext.TracingService.Trace("Creating response record with response: " + receipt.GetMessage());
					if (receipt.GetResponseCode() != null)
					{
						if (int.TryParse(receipt.GetResponseCode(), out var result2))
						{
							if (result2 < 50)
							{
								setStatusCodeOnPayment(paymentRecord, 844060000, localContext, service);
							}
							else
							{
								setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
							}
						}
						else
						{
							localContext.TracingService.Trace("Error: Response code is not a number = " + receipt.GetResponseCode());
							setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
							removePaymentMethod(entity, localContext, service);
						}
					}
					Entity entity3 = new Entity("msnfp_response");
					entity3["msnfp_identifier"] = "Response for " + (string)paymentRecord["msnfp_name"];
					entity3["msnfp_response"] = str;
					entity3["msnfp_paymentid"] = new EntityReference("msnfp_payment", (Guid)paymentRecord["msnfp_paymentid"]);
					if (paymentRecord.Contains("msnfp_eventpackageid") && paymentRecord["msnfp_eventpackageid"] != null)
					{
						entity3["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)paymentRecord["msnfp_eventpackageid"]).Id);
					}
					Guid guid = service.Create(entity3);
					bool flag = true;
					ITracingService tracingService = localContext.TracingService;
					Guid guid2 = guid;
					tracingService.Trace("Response created (" + guid2.ToString() + "). Linking response record to payment.");
					paymentRecord["msnfp_responseid"] = new EntityReference("msnfp_response", guid);
					if (receipt.GetResponseCode() != null && int.TryParse(receipt.GetResponseCode(), out var result3))
					{
						if (result3 < 50)
						{
							localContext.TracingService.Trace("Setting msnfp_transactionidentifier = " + receipt.GetReferenceNum());
							localContext.TracingService.Trace("Setting msnfp_transactionnumber = " + receipt.GetTxnNumber());
							localContext.TracingService.Trace("Setting order_id = " + text2);
							paymentRecord["msnfp_transactionidentifier"] = receipt.GetReferenceNum();
							paymentRecord["msnfp_transactionnumber"] = receipt.GetTxnNumber();
							paymentRecord["msnfp_invoiceidentifier"] = text2;
							paymentRecord["msnfp_transactionresult"] = "Approved - " + result3;
							if (receipt.GetCardType() != null)
							{
								localContext.TracingService.Trace("Card Type Response Code = " + receipt.GetCardType());
								switch (receipt.GetCardType())
								{
								case "M":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060001);
									break;
								case "V":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060000);
									break;
								case "AX":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060004);
									break;
								case "NO":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060008);
									break;
								case "D":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060007);
									break;
								case "DC":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060005);
									break;
								case "C1":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060006);
									break;
								case "JCB":
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060006);
									break;
								default:
									paymentRecord["msnfp_ccbrandcodepayment"] = new OptionSetValue(844060010);
									break;
								}
							}
						}
						else if (result3 > 50)
						{
							paymentRecord["msnfp_transactionresult"] = "FAILED";
						}
					}
					try
					{
						entity = service.Retrieve("msnfp_paymentmethod", ((EntityReference)paymentRecord["msnfp_paymentmethodid"]).Id, new ColumnSet("msnfp_cclast4", "msnfp_ccexpmmyy", "msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode"));
						if (entity == null)
						{
							localContext.TracingService.Trace("Clear Payment Method lookup on this transaction.");
							paymentRecord["msnfp_paymentmethodid"] = null;
						}
					}
					catch (Exception ex)
					{
						localContext.TracingService.Trace("Could not find Payment Method. Clear Payment Method lookup on this transaction record.");
						localContext.TracingService.Trace(ex.ToString());
						paymentRecord["msnfp_paymentmethodid"] = null;
					}
					service.Update(paymentRecord);
				}
				catch (Exception ex2)
				{
					localContext.TracingService.Trace(ex2.ToString());
				}
			}
		}

		private Entity getPaymentMethodForPayment(Entity paymentRecord, LocalPluginContext localContext, IOrganizationService service)
		{
			if (paymentRecord.Contains("msnfp_paymentmethodid"))
			{
				return service.Retrieve("msnfp_paymentmethod", ((EntityReference)paymentRecord["msnfp_paymentmethodid"]).Id, new ColumnSet("msnfp_paymentmethodid", "msnfp_cclast4", "msnfp_ccexpmmyy", "msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode", "msnfp_authtoken", "msnfp_telephone1", "msnfp_billing_line1", "msnfp_billing_postalcode", "msnfp_emailaddress1", "msnfp_stripecustomerid", "msnfp_bankactnumber", "msnfp_bankactrtnumber"));
			}
			localContext.TracingService.Trace("No payment method (msnfp_paymentmethod) on this payment. Exiting plugin.");
			setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
			throw new ArgumentNullException("msnfp_paymentmethod");
		}

		private void setStatusCodeOnPayment(Entity paymentRecord, int statuscode, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------Attempting to change payment status.---------");
			if (paymentRecord == null)
			{
				localContext.TracingService.Trace("Payment does not exist.");
				return;
			}
			try
			{
				localContext.TracingService.Trace("Set statuscode to: " + statuscode + " for payment id: " + paymentRecord.Id.ToString());
				paymentRecord["statuscode"] = new OptionSetValue(statuscode);
				service.Update(paymentRecord);
				localContext.TracingService.Trace("Updated payment status successfully.");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("setStatusCodeOnPayment() Error: " + ex.ToString());
			}
		}

		private Entity getPaymentProcessorForPaymentMethod(Entity paymentMethod, Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service)
		{
			if (paymentMethod.Contains("msnfp_paymentprocessorid"))
			{
				return service.Retrieve("msnfp_paymentprocessor", ((EntityReference)paymentMethod["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_apikey", "msnfp_name", "msnfp_storeid", "msnfp_avsvalidation", "msnfp_cvdvalidation", "msnfp_testmode", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword"));
			}
			localContext.TracingService.Trace("No payment processor is assigned to this payment method. Exiting plugin.");
			removePaymentMethod(paymentMethod, localContext, service);
			setStatusCodeOnPayment(giftTransaction, 844060003, localContext, service);
			throw new ArgumentNullException("msnfp_paymentprocessorid");
		}

		private void removePaymentMethod(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------Attempting to delete payment method---------");
			if (paymentMethod == null)
			{
				localContext.TracingService.Trace("Payment Method does not exist, cannot remove.");
				return;
			}
			localContext.TracingService.Trace("Is Reusable Payment Method: " + (bool)paymentMethod["msnfp_isreusable"]);
			if (!(bool)paymentMethod["msnfp_isreusable"])
			{
				localContext.TracingService.Trace("Payment Method is Not Reusable.");
				try
				{
					localContext.TracingService.Trace("Deleting Payment Method Id: " + paymentMethod.Id.ToString());
					service.Delete("msnfp_paymentmethod", paymentMethod.Id);
					localContext.TracingService.Trace("Payment Method successfully removed. ");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("removePaymentMethod() Error: " + ex.ToString());
				}
			}
			else
			{
				localContext.TracingService.Trace("Payment Method is Reusable. Ignoring Delete.");
			}
		}

		private void MaskStripeCreditCard(LocalPluginContext localContext, Entity primaryCreditCard, string cardId, string cardBrand, string customerId)
		{
			localContext.TracingService.Trace("Inside the method MaskStripeCreditCard. ");
			string str = (string)(primaryCreditCard["msnfp_cclast4"] = primaryCreditCard["msnfp_cclast4"].ToString().Substring(primaryCreditCard["msnfp_cclast4"].ToString().Length - 4));
			if (cardBrand != null)
			{
				switch (cardBrand)
				{
				case "MasterCard":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060001);
					break;
				case "Visa":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060000);
					break;
				case "American Express":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060004);
					break;
				case "Discover":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060008);
					break;
				case "Diners Club":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060005);
					break;
				case "UnionPay":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060009);
					break;
				case "JCB":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
					break;
				default:
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060010);
					break;
				}
			}
			localContext.TracingService.Trace("CC Number : " + str);
			primaryCreditCard["msnfp_authtoken"] = cardId;
			primaryCreditCard["msnfp_stripecustomerid"] = customerId;
			localContext.OrganizationService.Update(primaryCreditCard);
			localContext.TracingService.Trace("credit card record updated...MaskStripeCreditCard");
		}

		private AvsInfo AssignAVSValidationFieldsFromPaymentMethod(Entity paymentRecord, Entity paymentMethod, AvsInfo avsCheck, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering AssignAVSValidationFieldsFromPaymentMethod().");
			try
			{
				if (!paymentMethod.Contains("msnfp_billing_line1") || !paymentMethod.Contains("msnfp_billing_postalcode"))
				{
					localContext.TracingService.Trace("Donor (" + ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName + ") with id = " + ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString() + " is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					throw new Exception("Donor (" + ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName + ") with id = " + ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString() + " is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
				}
				string[] array = ((string)paymentMethod["msnfp_billing_line1"]).Split(' ');
				if (array.Length <= 1)
				{
					localContext.TracingService.Trace("Could not split address for AVS Validation. Please ensure the Street 1 billing address on the payment method is in the form '123 Example Street'. Exiting plugin.");
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
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
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				throw new Exception("AssignAVSValidationFieldsFromPaymentMethod() Error: " + ex.ToString());
			}
			return avsCheck;
		}

		private void addMonerisVaultProfile(Entity paymentRecord, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering addMonerisVaultProfile().");
			Entity entity = null;
			Entity entity2 = null;
			localContext.TracingService.Trace("Gathering transaction data from target id.");
			entity = getPaymentMethodForPayment(paymentRecord, localContext, service);
			if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
			{
				localContext.TracingService.Trace("Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
				if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
				{
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				}
				return;
			}
			if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				removePaymentMethod(entity, localContext, service);
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				return;
			}
			entity2 = getPaymentProcessorForPaymentMethod(entity, paymentRecord, localContext, service);
			localContext.TracingService.Trace("Put gathered payment information into vault profile object.");
			string storeId = (string)entity2["msnfp_storeid"];
			string apiToken = (string)entity2["msnfp_apikey"];
			string pan = (string)entity["msnfp_cclast4"];
			string text = (string)entity["msnfp_ccexpmmyy"];
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
			string custId = ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString();
			if (entity.Contains("msnfp_telephone1"))
			{
				phone = (string)entity["msnfp_telephone1"];
			}
			if (entity.Contains("msnfp_emailaddress1"))
			{
				email = (string)entity["msnfp_emailaddress1"];
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
			if (entity.Contains("msnfp_ccbrandcode"))
			{
				if (((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060004)
				{
					if (entity2.Contains("msnfp_avsvalidation"))
					{
						if ((bool)entity2["msnfp_avsvalidation"])
						{
							localContext.TracingService.Trace("AVS Validation = True");
							if (!paymentRecord.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("No Donor. Exiting plugin.");
								setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
								throw new ArgumentNullException("msnfp_customerid");
							}
							try
							{
								localContext.TracingService.Trace("Entering address information for AVS validation.");
								avsCheck = AssignAVSValidationFieldsFromPaymentMethod(paymentRecord, entity, avsCheck, localContext, service);
								resAddCC.SetAvsInfo(avsCheck);
							}
							catch
							{
								localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
								setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
								throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)paymentRecord["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)paymentRecord["msnfp_customerid"]).Id.ToString());
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
					localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
			}
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			httpsPostRequest.SetProcCountryCode(procCountryCode);
			if (entity2.Contains("msnfp_testmode"))
			{
				if ((bool)entity2["msnfp_testmode"])
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
					entity["msnfp_authtoken"] = receipt.GetDataKey();
					if (receipt.GetDataKey().Length > 0)
					{
						entity["msnfp_cclast4"] = receipt.GetResDataMaskedPan();
						localContext.TracingService.Trace("Masked Card Number");
					}
					service.Update(entity);
					localContext.TracingService.Trace("Added token to payment method.");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error, could not assign data id to auth token. Data key: " + receipt.GetDataKey());
					localContext.TracingService.Trace("Error: " + ex.ToString());
					setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
					throw new ArgumentNullException("msnfp_authtoken");
				}
			}
			catch (Exception ex2)
			{
				localContext.TracingService.Trace("Error processing response from payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex2.ToString());
				setStatusCodeOnPayment(paymentRecord, 844060003, localContext, service);
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
		}

		private void UpdateEventPackageTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventPackageTotals---------");
			if (queriedEntityRecord.Contains("msnfp_eventpackageid"))
			{
				decimal num = default(decimal);
				Entity eventPackage = service.Retrieve("msnfp_eventpackage", ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id, new ColumnSet("msnfp_eventpackageid", "msnfp_amount", "msnfp_amount_paid", "msnfp_amount_balance"));
				if (((OptionSetValue)queriedEntityRecord["statuscode"]).Value == 844060004)
				{
					eventPackage["statuscode"] = new OptionSetValue(844060004);
				}
				List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_payment")
					where ((EntityReference)a["msnfp_eventpackageid"]).Id == eventPackage.Id && (((OptionSetValue)a["statuscode"]).Value == 844060000 || ((OptionSetValue)a["statuscode"]).Value == 844060004)
					select a).ToList();
				foreach (Entity item in list)
				{
					num += (item.Contains("msnfp_amount_balance") ? ((Money)item["msnfp_amount_balance"]).Value : 0m);
				}
				decimal d = (eventPackage.Contains("msnfp_amount") ? ((Money)eventPackage["msnfp_amount"]).Value : 0m);
				eventPackage["msnfp_amount_balance"] = new Money(d - num);
				eventPackage["msnfp_amount_paid"] = new Money(num);
				if (d == num)
				{
					eventPackage["statuscode"] = new OptionSetValue(844060000);
				}
				service.Update(eventPackage);
			}
			localContext.TracingService.Trace("---------Exiting UpdateEventPackageTotals---------");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Payment";
			string text2 = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_paymentid"].ToString());
				MSNFP_Payment mSNFP_Payment = new MSNFP_Payment();
				mSNFP_Payment.paymentid = (Guid)queriedEntityRecord["msnfp_paymentid"];
				if (queriedEntityRecord.Contains("msnfp_eventpackageid") && queriedEntityRecord["msnfp_eventpackageid"] != null)
				{
					mSNFP_Payment.eventpackageid = ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpackageid.");
				}
				else
				{
					mSNFP_Payment.eventpackageid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Payment.customerid = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_Payment.customerid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Payment.amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount");
				}
				else
				{
					mSNFP_Payment.amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_refunded") && queriedEntityRecord["msnfp_amount_refunded"] != null)
				{
					mSNFP_Payment.AmountRefunded = ((Money)queriedEntityRecord["msnfp_amount_refunded"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_refunded");
				}
				else
				{
					mSNFP_Payment.AmountRefunded = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_refunded.");
				}
				if (queriedEntityRecord.Contains("msnfp_name") && queriedEntityRecord["msnfp_name"] != null)
				{
					mSNFP_Payment.name = (string)queriedEntityRecord["msnfp_name"];
					localContext.TracingService.Trace("Got msnfp_name.");
				}
				else
				{
					mSNFP_Payment.name = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_name.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionfraudcode") && queriedEntityRecord["msnfp_transactionfraudcode"] != null)
				{
					mSNFP_Payment.transactionfraudcode = (string)queriedEntityRecord["msnfp_transactionfraudcode"];
					localContext.TracingService.Trace("Got msnfp_transactionfraudcode.");
				}
				else
				{
					mSNFP_Payment.transactionfraudcode = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionfraudcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionidentifier") && queriedEntityRecord["msnfp_transactionidentifier"] != null)
				{
					mSNFP_Payment.transactionidentifier = (string)queriedEntityRecord["msnfp_transactionidentifier"];
					localContext.TracingService.Trace("Got msnfp_transactionidentifier.");
				}
				else
				{
					mSNFP_Payment.transactionidentifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionresult") && queriedEntityRecord["msnfp_transactionresult"] != null)
				{
					mSNFP_Payment.transactionresult = (string)queriedEntityRecord["msnfp_transactionresult"];
					localContext.TracingService.Trace("Got msnfp_transactionresult.");
				}
				else
				{
					mSNFP_Payment.transactionresult = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionresult.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymenttype") && queriedEntityRecord["msnfp_paymenttype"] != null)
				{
					mSNFP_Payment.paymenttype = ((OptionSetValue)queriedEntityRecord["msnfp_paymenttype"]).Value;
					localContext.TracingService.Trace("Got msnfp_paymenttype.");
				}
				else
				{
					mSNFP_Payment.paymenttype = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymenttype.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentprocessorid") && queriedEntityRecord["msnfp_paymentprocessorid"] != null)
				{
					mSNFP_Payment.paymentprocessorid = ((EntityReference)queriedEntityRecord["msnfp_paymentprocessorid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentprocessorid.");
				}
				else
				{
					mSNFP_Payment.paymentprocessorid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentprocessorid.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentmethodid") && queriedEntityRecord["msnfp_paymentmethodid"] != null)
				{
					mSNFP_Payment.paymentmethodid = ((EntityReference)queriedEntityRecord["msnfp_paymentmethodid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentmethodid.");
				}
				else
				{
					mSNFP_Payment.paymentmethodid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentmethodid.");
				}
				if (queriedEntityRecord.Contains("msnfp_ccbrandcodepayment") && queriedEntityRecord["msnfp_ccbrandcodepayment"] != null)
				{
					mSNFP_Payment.ccbrandcodepayment = ((OptionSetValue)queriedEntityRecord["msnfp_ccbrandcodepayment"]).Value;
					localContext.TracingService.Trace("Got msnfp_ccbrandcodepayment.");
				}
				else
				{
					mSNFP_Payment.ccbrandcodepayment = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ccbrandcodepayment.");
				}
				if (queriedEntityRecord.Contains("msnfp_invoiceidentifier") && queriedEntityRecord["msnfp_invoiceidentifier"] != null)
				{
					mSNFP_Payment.invoiceidentifier = (string)queriedEntityRecord["msnfp_invoiceidentifier"];
					localContext.TracingService.Trace("Got msnfp_invoiceidentifier.");
				}
				else
				{
					mSNFP_Payment.invoiceidentifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_invoiceidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_responseid") && queriedEntityRecord["msnfp_responseid"] != null)
				{
					mSNFP_Payment.responseid = ((EntityReference)queriedEntityRecord["msnfp_responseid"]).Id;
					localContext.TracingService.Trace("Got msnfp_responseid.");
				}
				else
				{
					mSNFP_Payment.responseid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_responseid.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_balance") && queriedEntityRecord["msnfp_amount_balance"] != null)
				{
					mSNFP_Payment.AmountBalance = ((Money)queriedEntityRecord["msnfp_amount_balance"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_balance");
				}
				else
				{
					mSNFP_Payment.AmountBalance = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_balance.");
				}
				if (queriedEntityRecord.Contains("msnfp_configurationid") && queriedEntityRecord["msnfp_configurationid"] != null)
				{
					mSNFP_Payment.configurationid = ((EntityReference)queriedEntityRecord["msnfp_configurationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_configurationid.");
				}
				else
				{
					mSNFP_Payment.configurationid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_configurationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_daterefunded") && queriedEntityRecord["msnfp_daterefunded"] != null)
				{
					mSNFP_Payment.daterefunded = (DateTime)queriedEntityRecord["msnfp_daterefunded"];
					localContext.TracingService.Trace("Got msnfp_daterefunded");
				}
				else
				{
					mSNFP_Payment.daterefunded = null;
					localContext.TracingService.Trace("Did NOT find msnfp_daterefunded");
				}
				if (queriedEntityRecord.Contains("msnfp_chequenumber") && queriedEntityRecord["msnfp_chequenumber"] != null)
				{
					mSNFP_Payment.chequenumber = (string)queriedEntityRecord["msnfp_chequenumber"];
					localContext.TracingService.Trace("Got msnfp_chequenumber.");
				}
				else
				{
					mSNFP_Payment.chequenumber = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_chequenumber.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Payment.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Payment.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Payment.statuscode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Payment.statuscode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Payment.createdon = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Payment.createdon = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Payment.createdon = null;
				}
				mSNFP_Payment.syncdate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Payment.deleted = true;
					mSNFP_Payment.deleteddate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Payment.deleted = false;
					mSNFP_Payment.deleteddate = null;
				}
				localContext.TracingService.Trace("JSON object created");
				if (messageName == "Create")
				{
					text2 = text2 + text + "/Create" + text;
				}
				else if (messageName == "Update" || messageName == "Delete")
				{
					text2 = text2 + text + "/Update" + text;
				}
				MemoryStream memoryStream = new MemoryStream();
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Payment));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Payment);
				byte[] array = memoryStream.ToArray();
				memoryStream.Close();
				string @string = Encoding.UTF8.GetString(array, 0, array.Length);
				WebAPIClient webAPIClient = new WebAPIClient();
				webAPIClient.Headers[HttpRequestHeader.ContentType] = "application/json";
				webAPIClient.Headers["Padlock"] = (string)configurationRecord["msnfp_apipadlocktoken"];
				webAPIClient.Encoding = Encoding.UTF8;
				localContext.TracingService.Trace("---------Preparing JSON---------");
				localContext.TracingService.Trace("Converted to json API URL : " + text2);
				localContext.TracingService.Trace("JSON: " + @string);
				localContext.TracingService.Trace("---------End of Preparing JSON---------");
				localContext.TracingService.Trace("Sending data to Azure.");
				string str = webAPIClient.UploadString(text2, @string);
				localContext.TracingService.Trace("Got response.");
				localContext.TracingService.Trace("Response: " + str);
			}
			else
			{
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting plugin.");
			}
		}
	}
}
