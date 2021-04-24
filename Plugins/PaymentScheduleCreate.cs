using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugins.AzureModels;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class PaymentScheduleCreate : PluginBase
	{
		public PaymentScheduleCreate(string unsecure, string secure)
			: base(typeof(PaymentScheduleCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered PaymentScheduleCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			Entity queriedEntityRecord = null;
			string messageName = pluginExecutionContext.MessageName;
			Entity configurationRecordByMessageName = Utilities.GetConfigurationRecordByMessageName(pluginExecutionContext, organizationService, localContext.TracingService);
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering PaymentScheduleCreate.cs Main Function---------");
				Entity entity2 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_paymentschedule", entity2.Id, GetPaymentScheduleColumns());
				}
				if (entity2 != null)
				{
					try
					{
						if (messageName == "Create")
						{
							if (!entity2.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("Validating donor - start.");
								queriedEntityRecord = organizationService.Retrieve("msnfp_paymentschedule", entity2.Id, GetPaymentScheduleColumns());
								AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByMessageName, localContext, organizationService, pluginExecutionContext);
							}
							else
							{
								AddOrUpdateThisRecordWithAzure(entity2, configurationRecordByMessageName, localContext, organizationService, pluginExecutionContext);
							}
						}
						else if (messageName == "Update")
						{
							AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByMessageName, localContext, organizationService, pluginExecutionContext);
						}
					}
					catch (Exception ex)
					{
						localContext.TracingService.Trace("An error has occured: " + ex.ToString());
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting workflow.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_paymentschedule", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetPaymentScheduleColumns());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByMessageName, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting PaymentScheduleCreate.cs---------");
		}

		private ColumnSet GetPaymentScheduleColumns()
		{
			return new ColumnSet("msnfp_paymentscheduleid", "msnfp_name", "createdon", "msnfp_amount_receipted", "msnfp_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_recurringamount", "msnfp_firstpaymentdate", "msnfp_frequencyinterval", "msnfp_frequency", "msnfp_frequencystartcode", "msnfp_nextpaymentdate", "msnfp_cancelationcode", "msnfp_cancellationnote", "msnfp_cancelledon", "msnfp_endondate", "msnfp_lastpaymentdate", "msnfp_scheduletypecode", "msnfp_anonymity", "msnfp_paymentmethodid", "msnfp_appealid", "msnfp_appraiser", "msnfp_billing_city", "msnfp_billing_country", "msnfp_billing_line1", "msnfp_billing_line2", "msnfp_billing_line3", "msnfp_billing_postalcode", "msnfp_billing_stateorprovince", "msnfp_originatingcampaignid", "msnfp_ccbrandcode", "msnfp_chargeoncreate", "msnfp_configurationid", "msnfp_constituentid", "msnfp_customerid", "msnfp_bookdate", "msnfp_ga_deliverycode", "msnfp_depositdate", "msnfp_emailaddress1", "msnfp_eventid", "msnfp_eventpackageid", "msnfp_firstname", "msnfp_giftbatchid", "msnfp_dataentrysource", "msnfp_paymenttypecode", "msnfp_lastname", "msnfp_membershipcategoryid", "msnfp_membershipinstanceid", "msnfp_mobilephone", "msnfp_organizationname", "msnfp_packageid", "msnfp_taxreceiptid", "msnfp_receiptpreferencecode", "msnfp_telephone1", "msnfp_telephone2", "msnfp_dataentryreference", "msnfp_invoiceidentifier", "msnfp_transactionfraudcode", "msnfp_transactionidentifier", "msnfp_transactionresult", "msnfp_tributecode", "msnfp_tributeacknowledgement", "msnfp_tributeid", "msnfp_tributemessage", "msnfp_paymentprocessorid", "msnfp_transactiondescription", "msnfp_designationid", "transactioncurrencyid", "statecode", "statuscode");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "PaymentSchedule";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_paymentscheduleid"].ToString());
				MSNFP_PaymentSchedule mSNFP_PaymentSchedule = new MSNFP_PaymentSchedule();
				mSNFP_PaymentSchedule.PaymentScheduleId = (Guid)queriedEntityRecord["msnfp_paymentscheduleid"];
				mSNFP_PaymentSchedule.Name = (queriedEntityRecord.Contains("msnfp_name") ? ((string)queriedEntityRecord["msnfp_name"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_PaymentSchedule.Name);
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_PaymentSchedule.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_PaymentSchedule.AmountReceipted = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_membership") && queriedEntityRecord["msnfp_amount_membership"] != null)
				{
					mSNFP_PaymentSchedule.AmountMembership = ((Money)queriedEntityRecord["msnfp_amount_membership"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_membership.");
				}
				else
				{
					mSNFP_PaymentSchedule.AmountMembership = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_membership.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_PaymentSchedule.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_PaymentSchedule.AmountNonReceiptable = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_PaymentSchedule.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_PaymentSchedule.AmountTax = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_recurringamount") && queriedEntityRecord["msnfp_recurringamount"] != null)
				{
					mSNFP_PaymentSchedule.RecurringAmount = ((Money)queriedEntityRecord["msnfp_recurringamount"]).Value;
					localContext.TracingService.Trace("Got msnfp_recurringamount.");
				}
				else
				{
					mSNFP_PaymentSchedule.RecurringAmount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_recurringamount.");
				}
				if (queriedEntityRecord.Contains("msnfp_firstpaymentdate") && queriedEntityRecord["msnfp_firstpaymentdate"] != null)
				{
					mSNFP_PaymentSchedule.FirstPaymentDate = (DateTime)queriedEntityRecord["msnfp_firstpaymentdate"];
					localContext.TracingService.Trace("Got msnfp_firstpaymentdate.");
				}
				else
				{
					mSNFP_PaymentSchedule.FirstPaymentDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_firstpaymentdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_frequencyinterval") && queriedEntityRecord["msnfp_frequencyinterval"] != null)
				{
					mSNFP_PaymentSchedule.FrequencyInterval = (int)queriedEntityRecord["msnfp_frequencyinterval"];
					localContext.TracingService.Trace("Got msnfp_frequencyinterval.");
				}
				else
				{
					mSNFP_PaymentSchedule.FrequencyInterval = null;
					localContext.TracingService.Trace("Did NOT find msnfp_frequencyinterval.");
				}
				if (queriedEntityRecord.Contains("msnfp_frequency") && queriedEntityRecord["msnfp_frequency"] != null)
				{
					mSNFP_PaymentSchedule.Frequency = ((OptionSetValue)queriedEntityRecord["msnfp_frequency"]).Value;
					localContext.TracingService.Trace("Got msnfp_frequency.");
				}
				else
				{
					mSNFP_PaymentSchedule.Frequency = null;
					localContext.TracingService.Trace("Did NOT find msnfp_frequency.");
				}
				if (queriedEntityRecord.Contains("msnfp_nextpaymentdate") && queriedEntityRecord["msnfp_nextpaymentdate"] != null)
				{
					mSNFP_PaymentSchedule.NextPaymentDate = (DateTime)queriedEntityRecord["msnfp_nextpaymentdate"];
					localContext.TracingService.Trace("Got msnfp_nextpaymentdate.");
				}
				else
				{
					mSNFP_PaymentSchedule.NextPaymentDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_nextpaymentdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_frequencystartcode") && queriedEntityRecord["msnfp_frequencystartcode"] != null)
				{
					mSNFP_PaymentSchedule.FrequencyStartCode = ((OptionSetValue)queriedEntityRecord["msnfp_frequencystartcode"]).Value;
					localContext.TracingService.Trace("Got msnfp_frequencystartcode.");
				}
				else
				{
					mSNFP_PaymentSchedule.FrequencyStartCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_frequencystartcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_cancelationcode") && queriedEntityRecord["msnfp_cancelationcode"] != null)
				{
					mSNFP_PaymentSchedule.CancelationCode = ((OptionSetValue)queriedEntityRecord["msnfp_cancelationcode"]).Value;
					localContext.TracingService.Trace("Got msnfp_cancelationcode.");
				}
				else
				{
					mSNFP_PaymentSchedule.CancelationCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_cancelationcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_cancellationnote") && queriedEntityRecord["msnfp_cancellationnote"] != null)
				{
					mSNFP_PaymentSchedule.CancellationNote = (string)queriedEntityRecord["msnfp_cancellationnote"];
					localContext.TracingService.Trace("Got msnfp_cancellationnote.");
				}
				else
				{
					mSNFP_PaymentSchedule.CancellationNote = null;
					localContext.TracingService.Trace("Did NOT find msnfp_cancellationnote.");
				}
				if (queriedEntityRecord.Contains("msnfp_cancelledon") && queriedEntityRecord["msnfp_cancelledon"] != null)
				{
					mSNFP_PaymentSchedule.CancelledOn = (DateTime)queriedEntityRecord["msnfp_cancelledon"];
					localContext.TracingService.Trace("Got msnfp_cancelledon.");
				}
				else
				{
					mSNFP_PaymentSchedule.CancelledOn = null;
					localContext.TracingService.Trace("Did NOT find msnfp_cancelledon.");
				}
				if (queriedEntityRecord.Contains("msnfp_endondate") && queriedEntityRecord["msnfp_endondate"] != null)
				{
					mSNFP_PaymentSchedule.EndonDate = (DateTime)queriedEntityRecord["msnfp_endondate"];
					localContext.TracingService.Trace("Got msnfp_endondate.");
				}
				else
				{
					mSNFP_PaymentSchedule.EndonDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_endondate.");
				}
				if (queriedEntityRecord.Contains("msnfp_lastpaymentdate") && queriedEntityRecord["msnfp_lastpaymentdate"] != null)
				{
					mSNFP_PaymentSchedule.LastPaymentDate = (DateTime)queriedEntityRecord["msnfp_lastpaymentdate"];
					localContext.TracingService.Trace("Got msnfp_lastpaymentdate.");
				}
				else
				{
					mSNFP_PaymentSchedule.LastPaymentDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lastpaymentdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_scheduletypecode") && queriedEntityRecord["msnfp_scheduletypecode"] != null)
				{
					mSNFP_PaymentSchedule.ScheduleTypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_scheduletypecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_scheduletypecode.");
				}
				else
				{
					mSNFP_PaymentSchedule.ScheduleTypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_scheduletypecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_anonymity") && queriedEntityRecord["msnfp_anonymity"] != null)
				{
					mSNFP_PaymentSchedule.Anonymity = ((OptionSetValue)queriedEntityRecord["msnfp_anonymity"]).Value;
					localContext.TracingService.Trace("Got msnfp_anonymity.");
				}
				else
				{
					mSNFP_PaymentSchedule.Anonymity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_anonymity.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentmethodid") && queriedEntityRecord["msnfp_paymentmethodid"] != null)
				{
					mSNFP_PaymentSchedule.PaymentMethodId = ((EntityReference)queriedEntityRecord["msnfp_paymentmethodid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentmethodid.");
				}
				else
				{
					mSNFP_PaymentSchedule.PaymentMethodId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentmethodid.");
				}
				if (queriedEntityRecord.Contains("msnfp_designationid") && queriedEntityRecord["msnfp_designationid"] != null)
				{
					mSNFP_PaymentSchedule.DesignationId = ((EntityReference)queriedEntityRecord["msnfp_designationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_designationid.");
				}
				else
				{
					mSNFP_PaymentSchedule.DesignationId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_designationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_appealid") && queriedEntityRecord["msnfp_appealid"] != null)
				{
					mSNFP_PaymentSchedule.AppealId = ((EntityReference)queriedEntityRecord["msnfp_appealid"]).Id;
					localContext.TracingService.Trace("Got msnfp_appealid.");
				}
				else
				{
					mSNFP_PaymentSchedule.AppealId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_appealid.");
				}
				if (queriedEntityRecord.Contains("msnfp_appraiser") && queriedEntityRecord["msnfp_appraiser"] != null)
				{
					mSNFP_PaymentSchedule.Appraiser = (string)queriedEntityRecord["msnfp_appraiser"];
					localContext.TracingService.Trace("Got msnfp_appraiser.");
				}
				else
				{
					mSNFP_PaymentSchedule.Appraiser = null;
					localContext.TracingService.Trace("Did NOT find msnfp_appraiser.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_city") && queriedEntityRecord["msnfp_billing_city"] != null)
				{
					mSNFP_PaymentSchedule.BillingCity = (string)queriedEntityRecord["msnfp_billing_city"];
					localContext.TracingService.Trace("Got msnfp_billing_city.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingCity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_city.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_country") && queriedEntityRecord["msnfp_billing_country"] != null)
				{
					mSNFP_PaymentSchedule.BillingCountry = (string)queriedEntityRecord["msnfp_billing_country"];
					localContext.TracingService.Trace("Got msnfp_billing_country.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingCountry = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_country.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line1") && queriedEntityRecord["msnfp_billing_line1"] != null)
				{
					mSNFP_PaymentSchedule.BillingLine1 = (string)queriedEntityRecord["msnfp_billing_line1"];
					localContext.TracingService.Trace("Got msnfp_billing_line1.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingLine1 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line1.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line2") && queriedEntityRecord["msnfp_billing_line2"] != null)
				{
					mSNFP_PaymentSchedule.BillingLine2 = (string)queriedEntityRecord["msnfp_billing_line2"];
					localContext.TracingService.Trace("Got msnfp_billing_line2.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingLine2 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line2.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line3") && queriedEntityRecord["msnfp_billing_line3"] != null)
				{
					mSNFP_PaymentSchedule.BillingLine3 = (string)queriedEntityRecord["msnfp_billing_line3"];
					localContext.TracingService.Trace("Got msnfp_billing_line3.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingLine3 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line3.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_postalcode") && queriedEntityRecord["msnfp_billing_postalcode"] != null)
				{
					mSNFP_PaymentSchedule.BillingPostalCode = (string)queriedEntityRecord["msnfp_billing_postalcode"];
					localContext.TracingService.Trace("Got msnfp_billing_postalcode.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingPostalCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_postalcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_stateorprovince") && queriedEntityRecord["msnfp_billing_stateorprovince"] != null)
				{
					mSNFP_PaymentSchedule.BillingStateorProvince = (string)queriedEntityRecord["msnfp_billing_stateorprovince"];
					localContext.TracingService.Trace("Got msnfp_billing_stateorprovince.");
				}
				else
				{
					mSNFP_PaymentSchedule.BillingStateorProvince = null;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("msnfp_originatingcampaignid") && queriedEntityRecord["msnfp_originatingcampaignid"] != null)
				{
					mSNFP_PaymentSchedule.OriginatingCampaignId = ((EntityReference)queriedEntityRecord["msnfp_originatingcampaignid"]).Id;
					localContext.TracingService.Trace("Got msnfp_originatingcampaignid.");
				}
				else
				{
					mSNFP_PaymentSchedule.OriginatingCampaignId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_originatingcampaignid.");
				}
				if (queriedEntityRecord.Contains("msnfp_ccbrandcode") && queriedEntityRecord["msnfp_ccbrandcode"] != null)
				{
					mSNFP_PaymentSchedule.CcBrandCode = ((OptionSetValue)queriedEntityRecord["msnfp_ccbrandcode"]).Value;
					localContext.TracingService.Trace("Got msnfp_ccbrandcode.");
				}
				else
				{
					mSNFP_PaymentSchedule.CcBrandCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ccbrandcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_chargeoncreate") && queriedEntityRecord["msnfp_chargeoncreate"] != null)
				{
					mSNFP_PaymentSchedule.ChargeonCreate = (bool)queriedEntityRecord["msnfp_chargeoncreate"];
					localContext.TracingService.Trace("Got msnfp_chargeoncreate.");
				}
				else
				{
					mSNFP_PaymentSchedule.ChargeonCreate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_chargeoncreate.");
				}
				if (queriedEntityRecord.Contains("msnfp_configurationid") && queriedEntityRecord["msnfp_configurationid"] != null)
				{
					mSNFP_PaymentSchedule.ConfigurationId = ((EntityReference)queriedEntityRecord["msnfp_configurationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_configurationid.");
				}
				else
				{
					mSNFP_PaymentSchedule.ConfigurationId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_configurationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_constituentid") && queriedEntityRecord["msnfp_constituentid"] != null)
				{
					mSNFP_PaymentSchedule.ConstituentId = ((EntityReference)queriedEntityRecord["msnfp_constituentid"]).Id;
					localContext.TracingService.Trace("Got msnfp_constituentid.");
				}
				else
				{
					mSNFP_PaymentSchedule.ConstituentId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_constituentid.");
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_PaymentSchedule.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_PaymentSchedule.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_PaymentSchedule.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_PaymentSchedule.CustomerId = null;
					mSNFP_PaymentSchedule.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_bookdate") && queriedEntityRecord["msnfp_bookdate"] != null)
				{
					mSNFP_PaymentSchedule.BookDate = (DateTime)queriedEntityRecord["msnfp_bookdate"];
					localContext.TracingService.Trace("Got msnfp_bookdate.");
				}
				else
				{
					mSNFP_PaymentSchedule.BookDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bookdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentprocessorid") && queriedEntityRecord["msnfp_paymentprocessorid"] != null)
				{
					mSNFP_PaymentSchedule.PaymentProcessorId = ((EntityReference)queriedEntityRecord["msnfp_paymentprocessorid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentprocessorid");
				}
				else
				{
					mSNFP_PaymentSchedule.PaymentProcessorId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentprocessorid");
				}
				if (queriedEntityRecord.Contains("msnfp_ga_deliverycode") && queriedEntityRecord["msnfp_ga_deliverycode"] != null)
				{
					mSNFP_PaymentSchedule.GaDeliveryCode = ((OptionSetValue)queriedEntityRecord["msnfp_ga_deliverycode"]).Value;
					localContext.TracingService.Trace("Got msnfp_ga_deliverycode.");
				}
				else
				{
					mSNFP_PaymentSchedule.GaDeliveryCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ga_deliverycode.");
				}
				if (queriedEntityRecord.Contains("msnfp_depositdate") && queriedEntityRecord["msnfp_depositdate"] != null)
				{
					mSNFP_PaymentSchedule.DepositDate = (DateTime)queriedEntityRecord["msnfp_depositdate"];
					localContext.TracingService.Trace("Got msnfp_depositdate.");
				}
				else
				{
					mSNFP_PaymentSchedule.DepositDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_depositdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_emailaddress1") && queriedEntityRecord["msnfp_emailaddress1"] != null)
				{
					mSNFP_PaymentSchedule.EmailAddress1 = (string)queriedEntityRecord["msnfp_emailaddress1"];
					localContext.TracingService.Trace("Got msnfp_emailaddress1.");
				}
				else
				{
					mSNFP_PaymentSchedule.EmailAddress1 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_emailaddress1.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_PaymentSchedule.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_PaymentSchedule.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventpackageid") && queriedEntityRecord["msnfp_eventpackageid"] != null)
				{
					mSNFP_PaymentSchedule.EventPackageId = ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpackageid.");
				}
				else
				{
					mSNFP_PaymentSchedule.EventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_firstname") && queriedEntityRecord["msnfp_firstname"] != null)
				{
					mSNFP_PaymentSchedule.FirstName = (string)queriedEntityRecord["msnfp_firstname"];
					localContext.TracingService.Trace("Got msnfp_firstname.");
				}
				else
				{
					mSNFP_PaymentSchedule.FirstName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_firstname.");
				}
				if (queriedEntityRecord.Contains("msnfp_giftbatchid") && queriedEntityRecord["msnfp_giftbatchid"] != null)
				{
					mSNFP_PaymentSchedule.GiftBatchId = ((EntityReference)queriedEntityRecord["msnfp_giftbatchid"]).Id;
					localContext.TracingService.Trace("Got msnfp_giftbatchid.");
				}
				else
				{
					mSNFP_PaymentSchedule.GiftBatchId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_giftbatchid.");
				}
				if (queriedEntityRecord.Contains("msnfp_dataentrysource") && queriedEntityRecord["msnfp_dataentrysource"] != null)
				{
					mSNFP_PaymentSchedule.DataEntrySource = ((OptionSetValue)queriedEntityRecord["msnfp_dataentrysource"]).Value;
					localContext.TracingService.Trace("Got msnfp_dataentrysource.");
				}
				else
				{
					mSNFP_PaymentSchedule.DataEntrySource = null;
					localContext.TracingService.Trace("Did NOT find msnfp_dataentrysource.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymenttypecode") && queriedEntityRecord["msnfp_paymenttypecode"] != null)
				{
					mSNFP_PaymentSchedule.PaymentTypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_paymenttypecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_paymenttypecode.");
				}
				else
				{
					mSNFP_PaymentSchedule.PaymentTypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymenttypecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_lastname") && queriedEntityRecord["msnfp_lastname"] != null)
				{
					mSNFP_PaymentSchedule.LastName = (string)queriedEntityRecord["msnfp_lastname"];
					localContext.TracingService.Trace("Got msnfp_lastname.");
				}
				else
				{
					mSNFP_PaymentSchedule.LastName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lastname.");
				}
				if (queriedEntityRecord.Contains("msnfp_membershipcategoryid") && queriedEntityRecord["msnfp_membershipcategoryid"] != null)
				{
					mSNFP_PaymentSchedule.MembershipCategoryId = ((EntityReference)queriedEntityRecord["msnfp_membershipcategoryid"]).Id;
					localContext.TracingService.Trace("Got msnfp_membershipcategoryid.");
				}
				else
				{
					mSNFP_PaymentSchedule.MembershipCategoryId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_membershipcategoryid.");
				}
				if (queriedEntityRecord.Contains("msnfp_membershipinstanceid") && queriedEntityRecord["msnfp_membershipinstanceid"] != null)
				{
					mSNFP_PaymentSchedule.MembershipId = ((EntityReference)queriedEntityRecord["msnfp_membershipinstanceid"]).Id;
					localContext.TracingService.Trace("Got msnfp_membershipinstanceid.");
				}
				else
				{
					mSNFP_PaymentSchedule.MembershipId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_membershipinstanceid.");
				}
				if (queriedEntityRecord.Contains("msnfp_mobilephone") && queriedEntityRecord["msnfp_mobilephone"] != null)
				{
					mSNFP_PaymentSchedule.MobilePhone = (string)queriedEntityRecord["msnfp_mobilephone"];
					localContext.TracingService.Trace("Got msnfp_mobilephone.");
				}
				else
				{
					mSNFP_PaymentSchedule.MobilePhone = null;
					localContext.TracingService.Trace("Did NOT find msnfp_mobilephone.");
				}
				if (queriedEntityRecord.Contains("msnfp_organizationname") && queriedEntityRecord["msnfp_organizationname"] != null)
				{
					mSNFP_PaymentSchedule.OrganizationName = (string)queriedEntityRecord["msnfp_organizationname"];
					localContext.TracingService.Trace("Got msnfp_organizationname.");
				}
				else
				{
					mSNFP_PaymentSchedule.OrganizationName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_organizationname.");
				}
				if (queriedEntityRecord.Contains("msnfp_packageid") && queriedEntityRecord["msnfp_packageid"] != null)
				{
					mSNFP_PaymentSchedule.PackageId = ((EntityReference)queriedEntityRecord["msnfp_packageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_packageid.");
				}
				else
				{
					mSNFP_PaymentSchedule.PackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_packageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_taxreceiptid") && queriedEntityRecord["msnfp_taxreceiptid"] != null)
				{
					mSNFP_PaymentSchedule.TaxReceiptId = ((EntityReference)queriedEntityRecord["msnfp_taxreceiptid"]).Id;
					localContext.TracingService.Trace("Got msnfp_taxreceiptid.");
				}
				else
				{
					mSNFP_PaymentSchedule.TaxReceiptId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_taxreceiptid.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptpreferencecode") && queriedEntityRecord["msnfp_receiptpreferencecode"] != null)
				{
					mSNFP_PaymentSchedule.ReceiptPreferenceCode = ((OptionSetValue)queriedEntityRecord["msnfp_receiptpreferencecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_receiptpreferencecode.");
				}
				else
				{
					mSNFP_PaymentSchedule.ReceiptPreferenceCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptpreferencecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone1") && queriedEntityRecord["msnfp_telephone1"] != null)
				{
					mSNFP_PaymentSchedule.Telephone1 = (string)queriedEntityRecord["msnfp_telephone1"];
					localContext.TracingService.Trace("Got msnfp_telephone1.");
				}
				else
				{
					mSNFP_PaymentSchedule.Telephone1 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone1.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone2") && queriedEntityRecord["msnfp_telephone2"] != null)
				{
					mSNFP_PaymentSchedule.Telephone2 = (string)queriedEntityRecord["msnfp_telephone2"];
					localContext.TracingService.Trace("Got msnfp_telephone2.");
				}
				else
				{
					mSNFP_PaymentSchedule.Telephone2 = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone2.");
				}
				if (queriedEntityRecord.Contains("msnfp_dataentryreference") && queriedEntityRecord["msnfp_dataentryreference"] != null)
				{
					mSNFP_PaymentSchedule.DataEntryReference = (string)queriedEntityRecord["msnfp_dataentryreference"];
					localContext.TracingService.Trace("Got msnfp_dataentryreference.");
				}
				else
				{
					mSNFP_PaymentSchedule.DataEntryReference = null;
					localContext.TracingService.Trace("Did NOT find msnfp_dataentryreference.");
				}
				if (queriedEntityRecord.Contains("msnfp_invoiceidentifier") && queriedEntityRecord["msnfp_invoiceidentifier"] != null)
				{
					mSNFP_PaymentSchedule.InvoiceIdentifier = (string)queriedEntityRecord["msnfp_invoiceidentifier"];
					localContext.TracingService.Trace("Got msnfp_invoiceidentifier.");
				}
				else
				{
					mSNFP_PaymentSchedule.InvoiceIdentifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_invoiceidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionfraudcode") && queriedEntityRecord["msnfp_transactionfraudcode"] != null)
				{
					mSNFP_PaymentSchedule.TransactionFraudCode = (string)queriedEntityRecord["msnfp_transactionfraudcode"];
					localContext.TracingService.Trace("Got msnfp_transactionfraudcode.");
				}
				else
				{
					mSNFP_PaymentSchedule.TransactionFraudCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionfraudcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionidentifier") && queriedEntityRecord["msnfp_transactionidentifier"] != null)
				{
					mSNFP_PaymentSchedule.TransactionIdentifier = (string)queriedEntityRecord["msnfp_transactionidentifier"];
					localContext.TracingService.Trace("Got msnfp_transactionidentifier.");
				}
				else
				{
					mSNFP_PaymentSchedule.TransactionIdentifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionresult") && queriedEntityRecord["msnfp_transactionresult"] != null)
				{
					mSNFP_PaymentSchedule.TransactionResult = (string)queriedEntityRecord["msnfp_transactionresult"];
					localContext.TracingService.Trace("Got msnfp_transactionresult.");
				}
				else
				{
					mSNFP_PaymentSchedule.TransactionResult = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionresult.");
				}
				if (queriedEntityRecord.Contains("msnfp_tributecode") && queriedEntityRecord["msnfp_tributecode"] != null)
				{
					mSNFP_PaymentSchedule.TributeCode = ((OptionSetValue)queriedEntityRecord["msnfp_tributecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_tributecode.");
				}
				else
				{
					mSNFP_PaymentSchedule.TributeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tributecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_tributeacknowledgement") && queriedEntityRecord["msnfp_tributeacknowledgement"] != null)
				{
					mSNFP_PaymentSchedule.TributeAcknowledgement = (string)queriedEntityRecord["msnfp_tributeacknowledgement"];
					localContext.TracingService.Trace("Got msnfp_tributeacknowledgement.");
				}
				else
				{
					mSNFP_PaymentSchedule.TributeAcknowledgement = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tributeacknowledgement.");
				}
				if (queriedEntityRecord.Contains("msnfp_tributeid") && queriedEntityRecord["msnfp_tributeid"] != null)
				{
					mSNFP_PaymentSchedule.TributeId = ((EntityReference)queriedEntityRecord["msnfp_tributeid"]).Id;
					localContext.TracingService.Trace("Got msnfp_tributeid.");
				}
				else
				{
					mSNFP_PaymentSchedule.TributeId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tributeid.");
				}
				if (queriedEntityRecord.Contains("msnfp_tributemessage") && queriedEntityRecord["msnfp_tributemessage"] != null)
				{
					mSNFP_PaymentSchedule.TributeMessage = (string)queriedEntityRecord["msnfp_tributemessage"];
					localContext.TracingService.Trace("Got msnfp_tributemessage.");
				}
				else
				{
					mSNFP_PaymentSchedule.TributeMessage = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tributemessage.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactiondescription") && queriedEntityRecord["msnfp_transactiondescription"] != null)
				{
					mSNFP_PaymentSchedule.TransactionDescription = (string)queriedEntityRecord["msnfp_transactiondescription"];
					localContext.TracingService.Trace("Got msnfp_transactiondescription.");
				}
				else
				{
					mSNFP_PaymentSchedule.TransactionDescription = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactiondescription.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_PaymentSchedule.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got TransactionCurrencyId.");
				}
				else
				{
					mSNFP_PaymentSchedule.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find TransactionCurrencyId.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_PaymentSchedule.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got StateCode");
				}
				else
				{
					mSNFP_PaymentSchedule.StateCode = null;
					localContext.TracingService.Trace("Did NOT find StateCode");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_PaymentSchedule.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got StatusCode");
				}
				else
				{
					mSNFP_PaymentSchedule.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find StatusCode");
				}
				if (messageName == "Create")
				{
					mSNFP_PaymentSchedule.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_PaymentSchedule.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_PaymentSchedule.CreatedOn = null;
				}
				mSNFP_PaymentSchedule.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_PaymentSchedule.Deleted = true;
					mSNFP_PaymentSchedule.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_PaymentSchedule.Deleted = false;
					mSNFP_PaymentSchedule.DeletedDate = null;
				}
				mSNFP_PaymentSchedule.Receipt = new HashSet<MSNFP_Receipt>();
				mSNFP_PaymentSchedule.Response = new HashSet<MSNFP_Response>();
				mSNFP_PaymentSchedule.Transaction = new HashSet<MSNFP_Transaction>();
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_PaymentSchedule));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_PaymentSchedule);
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
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting workflow.");
			}
		}
	}
}
