using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.AzureModels;
using Plugins.Common;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class EventPackageCreate : PluginBase
	{
		public EventPackageCreate(string unsecure, string secure)
			: base(typeof(EventPackageCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventPackageCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(organizationService);
			Entity entity = null;
			string messageName = pluginExecutionContext.MessageName;
			Entity entity2 = null;
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity3 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity3 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			entity2 = Plugins.PaymentProcesses.Utilities.GetConfigurationRecordByMessageName(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			ReceiptUtilities.UpdateReceipt(pluginExecutionContext, organizationService, localContext.TracingService);
			UpdateReceiptStatusReason(pluginExecutionContext, organizationService, localContext.TracingService);
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering EventPackageCreate.cs Main Function---------");
				Entity entity4 = (Entity)pluginExecutionContext.InputParameters["Target"];
				entity = organizationService.Retrieve("msnfp_eventpackage", entity4.Id, GetColumnSet());
				if (entity4 != null)
				{
					if (entity4.Contains("msnfp_amount_balance"))
					{
						decimal value = ((Money)entity4["msnfp_amount_balance"]).Value;
						if (value == 0m)
						{
							updatedRelatedRecordsToPaid(entity4, localContext, organizationService);
						}
					}
					if (messageName == "Create")
					{
						if (entity2 != null)
						{
							Entity entity5 = new Entity(entity4.LogicalName, entity4.Id);
							entity5["msnfp_configurationid"] = entity2.ToEntityReference();
							organizationService.Update(entity5);
						}
						AddOrUpdateThisRecordWithAzure(entity4, entity2, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						UpdateEventTotals(entity, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(entity, entity2, localContext, organizationService, pluginExecutionContext);
					}
					Plugins.PaymentProcesses.Utilities.UpdateHouseholdOnRecord(organizationService, entity4, "msnfp_householdid", "msnfp_customerid");
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting plugin.");
				}
				if (entity != null && entity.Contains("msnfp_customerid") && entity["msnfp_customerid"] != null)
				{
					PopulateMostRecentEventPackageDataToDonor(organizationService, orgSvcContext, localContext, entity, messageName);
				}
			}
			if (messageName == "Delete")
			{
				entity = organizationService.Retrieve("msnfp_eventpackage", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(entity, entity2, localContext, organizationService, pluginExecutionContext);
				if (entity != null && entity.Contains("msnfp_customerid") && entity["msnfp_customerid"] != null)
				{
					PopulateMostRecentEventPackageDataToDonor(organizationService, orgSvcContext, localContext, entity, messageName);
				}
			}
			if (entity != null)
			{
				Plugins.Common.Utilities.CallYearlyGivingServiceAsync(entity.Id, entity.LogicalName, entity2.Id, organizationService, localContext.TracingService);
			}
			localContext.TracingService.Trace("---------Exiting EventPackageCreate.cs---------");
		}

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventpackageid", "msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_amount", "msnfp_ref_amount_receipted", "msnfp_ref_amount_nonreceiptable", "msnfp_ref_amount_tax", "msnfp_ref_amount", "msnfp_firstname", "msnfp_lastname", "msnfp_emailaddress1", "msnfp_telephone1", "msnfp_telephone2", "msnfp_billing_city", "msnfp_billing_country", "msnfp_billing_line1", "msnfp_billing_line2", "msnfp_billing_line3", "msnfp_billing_postalcode", "msnfp_billing_stateorprovince", "msnfp_campaignid", "msnfp_packageid", "msnfp_appealid", "msnfp_eventid", "msnfp_chequenumber", "msnfp_chequewiredate", "msnfp_configurationid", "msnfp_constituentid", "msnfp_customerid", "msnfp_date", "msnfp_daterefunded", "msnfp_dataentrysource", "msnfp_identifier", "msnfp_ccbrandcode", "msnfp_organizationname", "msnfp_paymentmethodid", "msnfp_dataentryreference", "msnfp_invoiceidentifier", "msnfp_transactionfraudcode", "msnfp_transactionidentifier", "msnfp_transactionresult", "msnfp_thirdpartyreceipt", "msnfp_sum_donations", "msnfp_sum_products", "msnfp_sum_sponsorships", "msnfp_sum_tickets", "msnfp_sum_registrations", "msnfp_val_donations", "msnfp_val_products", "msnfp_val_sponsorships", "msnfp_val_tickets", "transactioncurrencyid", "msnfp_amount_balance", "createdon", "statuscode", "statecode");
		}

		private void UpdateReceiptStatusReason(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
		{
			if (context.MessageName != "Update")
			{
				return;
			}
			Entity entity = (Entity)context.InputParameters["Target"];
			OptionSetValue attributeValue = entity.GetAttributeValue<OptionSetValue>("statuscode");
			if (attributeValue == null)
			{
				return;
			}
			Entity entity2 = context.PostEntityImages["postImage"];
			EntityReference attributeValue2 = entity2.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
			if (attributeValue2 != null)
			{
				Entity entity3 = new Entity("msnfp_receipt", attributeValue2.Id);
				if (attributeValue.Value == 844060000)
				{
					tracingService.Trace("Event Package Status Reason is Complete. Setting Receipt Status Reason to Issued.");
					entity3["statuscode"] = new OptionSetValue(1);
					service.Update(entity3);
				}
				else if (attributeValue.Value == 844060003)
				{
					tracingService.Trace("Event Package Status Reason is Failed. Setting Receipt Status Reason to Void(Failed).");
					entity3["statuscode"] = new OptionSetValue(844060002);
					service.Update(entity3);
				}
			}
		}

		private void PopulateMostRecentEventPackageDataToDonor(IOrganizationService organizationService, OrganizationServiceContext orgSvcContext, LocalPluginContext localContext, Entity eventPackageRecord, string messageName)
		{
			localContext.TracingService.Trace("----- Populating The Most Recent Event Registration To the according Donor -----");
			EntityReference donor = (EntityReference)eventPackageRecord.Attributes["msnfp_customerid"];
			if (donor.LogicalName == "contact" || donor.LogicalName == "account")
			{
				Entity entity = organizationService.Retrieve(donor.LogicalName, donor.Id, new ColumnSet("msnfp_lasteventpackageid"));
				if (string.Compare(messageName, "Delete", ignoreCase: true) == 0)
				{
					localContext.TracingService.Trace("Message is Delete. Locating the previous Event Package.");
					localContext.TracingService.Trace("Current EventPackageId=" + eventPackageRecord.Id.ToString());
					Entity entity2 = (from c in orgSvcContext.CreateQuery("msnfp_eventpackage")
						where ((EntityReference)c["msnfp_customerid"]).Id == donor.Id && (Guid)c["msnfp_eventpackageid"] != eventPackageRecord.Id
						orderby c["createdon"] descending
						select c).FirstOrDefault();
					if (entity2 != null)
					{
						entity["msnfp_lasteventpackageid"] = new EntityReference(entity2.LogicalName, entity2.Id);
						entity["msnfp_lasteventpackagedate"] = (DateTime)entity2["createdon"];
						organizationService.Update(entity);
					}
				}
				else if (entity.Contains("msnfp_lasteventpackageid") && entity["msnfp_lasteventpackageid"] != null)
				{
					Entity entity3 = (from c in orgSvcContext.CreateQuery("msnfp_eventpackage")
						where ((EntityReference)c["msnfp_customerid"]).Id == donor.Id
						orderby c["createdon"] descending
						select c).First();
					entity["msnfp_lasteventpackageid"] = new EntityReference(entity3.LogicalName, entity3.Id);
					entity["msnfp_lasteventpackagedate"] = (DateTime)entity3["createdon"];
					organizationService.Update(entity);
				}
				else
				{
					entity["msnfp_lasteventpackageid"] = new EntityReference(eventPackageRecord.LogicalName, eventPackageRecord.Id);
					entity["msnfp_lasteventpackagedate"] = (DateTime)eventPackageRecord["createdon"];
					organizationService.Update(entity);
				}
			}
			localContext.TracingService.Trace("----- Finished Populating The Most Recent Event Registration To the according Donor -----");
		}

		private void updatedRelatedRecordsToPaid(Entity primaryEventPackage, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------updatedRelatedRecordsToPaid---------");
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(service);
			List<Entity> list = (from a in organizationServiceContext.CreateQuery("msnfp_registration")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == primaryEventPackage.Id && ((OptionSetValue)a["statuscode"]).Value == 1
				select a).ToList();
			localContext.TracingService.Trace("Got registrationList");
			Entity entity = null;
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_registrationid"))
				{
					localContext.TracingService.Trace("Got msnfp_registrationid");
					entity = service.Retrieve("msnfp_registration", item.Id, new ColumnSet("msnfp_registrationid", "statuscode"));
					entity["statuscode"] = new OptionSetValue(844060000);
					localContext.TracingService.Trace("Record updated to Paid");
					service.Update(entity);
					localContext.TracingService.Trace("Record updated for ticketRegistrations");
				}
			}
			List<Entity> list2 = (from a in organizationServiceContext.CreateQuery("msnfp_product")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == primaryEventPackage.Id && ((OptionSetValue)a["statuscode"]).Value == 1
				select a).ToList();
			localContext.TracingService.Trace("Got productList");
			Entity entity2 = null;
			foreach (Entity item2 in list2)
			{
				if (item2.Contains("msnfp_productid"))
				{
					localContext.TracingService.Trace("Got msnfp_productid");
					entity2 = service.Retrieve("msnfp_product", item2.Id, new ColumnSet("msnfp_productid", "statuscode"));
					entity2["statuscode"] = new OptionSetValue(844060000);
					localContext.TracingService.Trace("Record updated for Paid");
					service.Update(entity2);
					localContext.TracingService.Trace("Record updated for product");
				}
			}
			List<Entity> list3 = (from a in organizationServiceContext.CreateQuery("msnfp_sponsorship")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == primaryEventPackage.Id && ((OptionSetValue)a["statuscode"]).Value == 844060003
				select a).ToList();
			localContext.TracingService.Trace("Got sponsorshipList");
			Entity entity3 = null;
			foreach (Entity item3 in list3)
			{
				if (item3.Contains("msnfp_sponsorshipid"))
				{
					localContext.TracingService.Trace("Got msnfp_sponsorshipid");
					entity3 = service.Retrieve("msnfp_sponsorship", item3.Id, new ColumnSet("msnfp_sponsorshipid", "statuscode"));
					entity3["statuscode"] = new OptionSetValue(844060000);
					localContext.TracingService.Trace("Record updated for Paid");
					service.Update(entity3);
					localContext.TracingService.Trace("Record updated for sponsorship");
				}
			}
		}

		private void UpdateEventTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTotals---------");
			decimal value = default(decimal);
			Entity entity = service.Retrieve("msnfp_event", ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id, new ColumnSet("msnfp_eventid", "msnfp_sum_packages", "msnfp_count_packages"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_eventpackage")
				where ((EntityReference)a["msnfp_eventid"]).Id == ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id && ((OptionSetValue)a["statecode"]).Value == 0 && ((OptionSetValue)a["statuscode"]).Value == 844060000
				select a).ToList();
			if (list.Count > 0)
			{
				foreach (Entity item in list)
				{
					if (item.Contains("msnfp_val_sponsorships") && item["msnfp_val_sponsorships"] != null)
					{
						value += ((Money)item["msnfp_val_sponsorships"]).Value;
					}
					if (item.Contains("msnfp_val_tickets") && item["msnfp_val_tickets"] != null)
					{
						value += ((Money)item["msnfp_val_tickets"]).Value;
					}
					if (item.Contains("msnfp_val_products") && item["msnfp_val_products"] != null)
					{
						value += ((Money)item["msnfp_val_products"]).Value;
					}
					if (item.Contains("msnfp_val_donations") && item["msnfp_val_donations"] != null)
					{
						value += ((Money)item["msnfp_val_donations"]).Value;
					}
				}
			}
			entity["msnfp_count_packages"] = list.Count;
			entity["msnfp_sum_packages"] = new Money(value);
			service.Update(entity);
			localContext.TracingService.Trace("Event Record Updated");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "EventPackage";
			string text2 = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_eventpackageid"].ToString());
				MSNFP_EventPackage mSNFP_EventPackage = new MSNFP_EventPackage();
				mSNFP_EventPackage.EventPackageId = (Guid)queriedEntityRecord["msnfp_eventpackageid"];
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_EventPackage.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted");
				}
				else
				{
					mSNFP_EventPackage.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_EventPackage.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable");
				}
				else
				{
					mSNFP_EventPackage.AmountNonReceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_EventPackage.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax");
				}
				else
				{
					mSNFP_EventPackage.AmountTax = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_EventPackage.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount");
				}
				else
				{
					mSNFP_EventPackage.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_receipted") && queriedEntityRecord["msnfp_ref_amount_receipted"] != null)
				{
					mSNFP_EventPackage.RefAmountReceipted = ((Money)queriedEntityRecord["msnfp_ref_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_receipted");
				}
				else
				{
					mSNFP_EventPackage.RefAmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_nonreceiptable") && queriedEntityRecord["msnfp_ref_amount_nonreceiptable"] != null)
				{
					mSNFP_EventPackage.RefAmountNonreceiptable = ((Money)queriedEntityRecord["msnfp_ref_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_nonreceiptable");
				}
				else
				{
					mSNFP_EventPackage.RefAmountNonreceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_tax") && queriedEntityRecord["msnfp_ref_amount_tax"] != null)
				{
					mSNFP_EventPackage.RefAmountTax = ((Money)queriedEntityRecord["msnfp_ref_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_tax");
				}
				else
				{
					mSNFP_EventPackage.RefAmountTax = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount") && queriedEntityRecord["msnfp_ref_amount"] != null)
				{
					mSNFP_EventPackage.RefAmount = ((Money)queriedEntityRecord["msnfp_ref_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount");
				}
				else
				{
					mSNFP_EventPackage.RefAmount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_firstname") && queriedEntityRecord["msnfp_firstname"] != null)
				{
					mSNFP_EventPackage.FirstName = (string)queriedEntityRecord["msnfp_firstname"];
					localContext.TracingService.Trace("Got msnfp_firstname");
				}
				else
				{
					mSNFP_EventPackage.FirstName = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_firstname.");
				}
				if (queriedEntityRecord.Contains("msnfp_lastname") && queriedEntityRecord["msnfp_lastname"] != null)
				{
					mSNFP_EventPackage.LastName = (string)queriedEntityRecord["msnfp_lastname"];
					localContext.TracingService.Trace("Got msnfp_lastname");
				}
				else
				{
					mSNFP_EventPackage.LastName = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_lastname.");
				}
				if (queriedEntityRecord.Contains("msnfp_emailaddress1") && queriedEntityRecord["msnfp_emailaddress1"] != null)
				{
					mSNFP_EventPackage.Emailaddress1 = (string)queriedEntityRecord["msnfp_emailaddress1"];
					localContext.TracingService.Trace("Got msnfp_emailaddress1");
				}
				else
				{
					mSNFP_EventPackage.Emailaddress1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_emailaddress1.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone1") && queriedEntityRecord["msnfp_telephone1"] != null)
				{
					mSNFP_EventPackage.Telephone1 = (string)queriedEntityRecord["msnfp_telephone1"];
					localContext.TracingService.Trace("Got msnfp_telephone1");
				}
				else
				{
					mSNFP_EventPackage.Telephone1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone1.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone2") && queriedEntityRecord["msnfp_telephone2"] != null)
				{
					mSNFP_EventPackage.Telephone2 = (string)queriedEntityRecord["msnfp_telephone2"];
					localContext.TracingService.Trace("Got msnfp_telephone2");
				}
				else
				{
					mSNFP_EventPackage.Telephone2 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone2.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_city") && queriedEntityRecord["msnfp_billing_city"] != null)
				{
					mSNFP_EventPackage.BillingCity = (string)queriedEntityRecord["msnfp_billing_city"];
					localContext.TracingService.Trace("Got msnfp_billing_city");
				}
				else
				{
					mSNFP_EventPackage.BillingCity = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_city.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_country") && queriedEntityRecord["msnfp_billing_country"] != null)
				{
					mSNFP_EventPackage.BillingCountry = (string)queriedEntityRecord["msnfp_billing_country"];
					localContext.TracingService.Trace("Got msnfp_billing_country");
				}
				else
				{
					mSNFP_EventPackage.BillingCountry = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_country.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line1") && queriedEntityRecord["msnfp_billing_line1"] != null)
				{
					mSNFP_EventPackage.BillingLine1 = (string)queriedEntityRecord["msnfp_billing_line1"];
					localContext.TracingService.Trace("Got msnfp_billing_line1");
				}
				else
				{
					mSNFP_EventPackage.BillingLine1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line1.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line2") && queriedEntityRecord["msnfp_billing_line2"] != null)
				{
					mSNFP_EventPackage.BillingLine2 = (string)queriedEntityRecord["msnfp_billing_line2"];
					localContext.TracingService.Trace("Got msnfp_billing_line2");
				}
				else
				{
					mSNFP_EventPackage.BillingLine2 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line2.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line3") && queriedEntityRecord["msnfp_billing_line3"] != null)
				{
					mSNFP_EventPackage.BillingLine3 = (string)queriedEntityRecord["msnfp_billing_line3"];
					localContext.TracingService.Trace("Got msnfp_billing_line3");
				}
				else
				{
					mSNFP_EventPackage.BillingLine3 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line3.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_postalcode") && queriedEntityRecord["msnfp_billing_postalcode"] != null)
				{
					mSNFP_EventPackage.BillingPostalCode = (string)queriedEntityRecord["msnfp_billing_postalcode"];
					localContext.TracingService.Trace("Got msnfp_billing_postalcode");
				}
				else
				{
					mSNFP_EventPackage.BillingPostalCode = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_postalcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_stateorprovince") && queriedEntityRecord["msnfp_billing_stateorprovince"] != null)
				{
					mSNFP_EventPackage.BillingStateorProvince = (string)queriedEntityRecord["msnfp_billing_stateorprovince"];
					localContext.TracingService.Trace("Got msnfp_billing_stateorprovince");
				}
				else
				{
					mSNFP_EventPackage.BillingStateorProvince = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("msnfp_campaignid") && queriedEntityRecord["msnfp_campaignid"] != null)
				{
					mSNFP_EventPackage.CampaignId = ((EntityReference)queriedEntityRecord["msnfp_campaignid"]).Id;
					localContext.TracingService.Trace("Got msnfp_campaignid");
				}
				else
				{
					mSNFP_EventPackage.CampaignId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_campaignid.");
				}
				if (queriedEntityRecord.Contains("msnfp_packageid") && queriedEntityRecord["msnfp_packageid"] != null)
				{
					mSNFP_EventPackage.PackageId = ((EntityReference)queriedEntityRecord["msnfp_packageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_packageid");
				}
				else
				{
					mSNFP_EventPackage.PackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_packageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_appealid") && queriedEntityRecord["msnfp_appealid"] != null)
				{
					mSNFP_EventPackage.Appealid = ((EntityReference)queriedEntityRecord["msnfp_appealid"]).Id;
					localContext.TracingService.Trace("Got msnfp_appealid");
				}
				else
				{
					mSNFP_EventPackage.Appealid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_appealid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_EventPackage.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid");
				}
				else
				{
					mSNFP_EventPackage.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_chequenumber") && queriedEntityRecord["msnfp_chequenumber"] != null)
				{
					mSNFP_EventPackage.ChequeNumber = (string)queriedEntityRecord["msnfp_chequenumber"];
					localContext.TracingService.Trace("Got msnfp_chequenumber");
				}
				else
				{
					mSNFP_EventPackage.ChequeNumber = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_chequenumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_chequewiredate") && queriedEntityRecord["msnfp_chequewiredate"] != null)
				{
					mSNFP_EventPackage.ChequeWireDate = (DateTime)queriedEntityRecord["msnfp_chequewiredate"];
					localContext.TracingService.Trace("Got msnfp_chequewiredate");
				}
				else
				{
					mSNFP_EventPackage.ChequeWireDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_chequewiredate.");
				}
				if (queriedEntityRecord.Contains("msnfp_configurationid") && queriedEntityRecord["msnfp_configurationid"] != null)
				{
					mSNFP_EventPackage.ConfigurationId = ((EntityReference)queriedEntityRecord["msnfp_configurationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_configurationid");
				}
				else
				{
					mSNFP_EventPackage.ConfigurationId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_configurationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_constituentid") && queriedEntityRecord["msnfp_constituentid"] != null)
				{
					mSNFP_EventPackage.ConstituentId = ((EntityReference)queriedEntityRecord["msnfp_constituentid"]).Id;
					localContext.TracingService.Trace("Got msnfp_constituentid");
				}
				else
				{
					mSNFP_EventPackage.ConstituentId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_constituentid.");
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_EventPackage.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_EventPackage.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_EventPackage.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid");
				}
				else
				{
					mSNFP_EventPackage.CustomerId = null;
					mSNFP_EventPackage.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_date") && queriedEntityRecord["msnfp_date"] != null)
				{
					mSNFP_EventPackage.Date = (DateTime)queriedEntityRecord["msnfp_date"];
					localContext.TracingService.Trace("Got msnfp_date");
				}
				else
				{
					mSNFP_EventPackage.Date = null;
					localContext.TracingService.Trace("Did NOT find msnfp_date.");
				}
				if (queriedEntityRecord.Contains("msnfp_daterefunded") && queriedEntityRecord["msnfp_daterefunded"] != null)
				{
					mSNFP_EventPackage.DateRefunded = (DateTime)queriedEntityRecord["msnfp_daterefunded"];
					localContext.TracingService.Trace("Got msnfp_daterefunded");
				}
				else
				{
					mSNFP_EventPackage.DateRefunded = null;
					localContext.TracingService.Trace("Did NOT find msnfp_daterefunded.");
				}
				if (queriedEntityRecord.Contains("msnfp_dataentrysource") && queriedEntityRecord["msnfp_dataentrysource"] != null)
				{
					mSNFP_EventPackage.DataEntrySource = ((OptionSetValue)queriedEntityRecord["msnfp_dataentrysource"]).Value;
					localContext.TracingService.Trace("Got msnfp_dataentrysource");
				}
				else
				{
					mSNFP_EventPackage.DataEntrySource = null;
					localContext.TracingService.Trace("Did NOT find msnfp_dataentrysource.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_EventPackage.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier");
				}
				else
				{
					mSNFP_EventPackage.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_ccbrandcode") && queriedEntityRecord["msnfp_ccbrandcode"] != null)
				{
					mSNFP_EventPackage.CcBrandCode = ((OptionSetValue)queriedEntityRecord["msnfp_ccbrandcode"]).Value;
					localContext.TracingService.Trace("Got msnfp_ccbrandcode");
				}
				else
				{
					mSNFP_EventPackage.CcBrandCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ccbrandcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_organizationname") && queriedEntityRecord["msnfp_organizationname"] != null)
				{
					mSNFP_EventPackage.OrganizationName = (string)queriedEntityRecord["msnfp_organizationname"];
					localContext.TracingService.Trace("Got msnfp_organizationname");
				}
				else
				{
					mSNFP_EventPackage.OrganizationName = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_organizationname.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentmethodid") && queriedEntityRecord["msnfp_paymentmethodid"] != null)
				{
					mSNFP_EventPackage.PaymentmethodId = ((EntityReference)queriedEntityRecord["msnfp_paymentmethodid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentmethodid");
				}
				else
				{
					mSNFP_EventPackage.PaymentmethodId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentmethodid.");
				}
				if (queriedEntityRecord.Contains("msnfp_dataentryreference") && queriedEntityRecord["msnfp_dataentryreference"] != null)
				{
					mSNFP_EventPackage.DataEntryReference = (string)queriedEntityRecord["msnfp_dataentryreference"];
					localContext.TracingService.Trace("Got msnfp_dataentryreference");
				}
				else
				{
					mSNFP_EventPackage.DataEntryReference = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_dataentryreference.");
				}
				if (queriedEntityRecord.Contains("msnfp_invoiceidentifier") && queriedEntityRecord["msnfp_invoiceidentifier"] != null)
				{
					mSNFP_EventPackage.InvoiceIdentifier = (string)queriedEntityRecord["msnfp_invoiceidentifier"];
					localContext.TracingService.Trace("Got msnfp_invoiceidentifier");
				}
				else
				{
					mSNFP_EventPackage.InvoiceIdentifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_invoiceidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionfraudcode") && queriedEntityRecord["msnfp_transactionfraudcode"] != null)
				{
					mSNFP_EventPackage.TransactionFraudCode = (string)queriedEntityRecord["msnfp_transactionfraudcode"];
					localContext.TracingService.Trace("Got msnfp_transactionfraudcode");
				}
				else
				{
					mSNFP_EventPackage.TransactionFraudCode = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionfraudcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionidentifier") && queriedEntityRecord["msnfp_transactionidentifier"] != null)
				{
					mSNFP_EventPackage.TransactionIdentifier = (string)queriedEntityRecord["msnfp_transactionidentifier"];
					localContext.TracingService.Trace("Got msnfp_transactionidentifier");
				}
				else
				{
					mSNFP_EventPackage.TransactionIdentifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionresult") && queriedEntityRecord["msnfp_transactionresult"] != null)
				{
					mSNFP_EventPackage.TransactionResult = (string)queriedEntityRecord["msnfp_transactionresult"];
					localContext.TracingService.Trace("Got msnfp_transactionresult");
				}
				else
				{
					mSNFP_EventPackage.TransactionResult = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionresult.");
				}
				if (queriedEntityRecord.Contains("msnfp_thirdpartyreceipt") && queriedEntityRecord["msnfp_thirdpartyreceipt"] != null)
				{
					mSNFP_EventPackage.ThirdPartyReceipt = (string)queriedEntityRecord["msnfp_thirdpartyreceipt"];
					localContext.TracingService.Trace("Got msnfp_thirdpartyreceipt");
				}
				else
				{
					mSNFP_EventPackage.ThirdPartyReceipt = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_thirdpartyreceipt.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_donations") && queriedEntityRecord["msnfp_sum_donations"] != null)
				{
					mSNFP_EventPackage.SumDonations = (int)queriedEntityRecord["msnfp_sum_donations"];
					localContext.TracingService.Trace("Got msnfp_sum_donations");
				}
				else
				{
					mSNFP_EventPackage.SumDonations = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_donations.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_products") && queriedEntityRecord["msnfp_sum_products"] != null)
				{
					mSNFP_EventPackage.SumProducts = (int)queriedEntityRecord["msnfp_sum_products"];
					localContext.TracingService.Trace("Got msnfp_sum_products");
				}
				else
				{
					mSNFP_EventPackage.SumProducts = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_products.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_sponsorships") && queriedEntityRecord["msnfp_sum_sponsorships"] != null)
				{
					mSNFP_EventPackage.SumSponsorships = (int)queriedEntityRecord["msnfp_sum_sponsorships"];
					localContext.TracingService.Trace("Got msnfp_sum_sponsorships");
				}
				else
				{
					mSNFP_EventPackage.SumSponsorships = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_sponsorships.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_tickets") && queriedEntityRecord["msnfp_sum_tickets"] != null)
				{
					mSNFP_EventPackage.SumTickets = (int)queriedEntityRecord["msnfp_sum_tickets"];
					localContext.TracingService.Trace("Got msnfp_sum_tickets");
				}
				else
				{
					mSNFP_EventPackage.SumTickets = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_tickets.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_registrations") && queriedEntityRecord["msnfp_sum_registrations"] != null)
				{
					mSNFP_EventPackage.SumRegistrations = (int)queriedEntityRecord["msnfp_sum_registrations"];
					localContext.TracingService.Trace("Got msnfp_sum_registrations");
				}
				else
				{
					mSNFP_EventPackage.SumRegistrations = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_registrations.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_donations") && queriedEntityRecord["msnfp_val_donations"] != null)
				{
					mSNFP_EventPackage.ValDonations = ((Money)queriedEntityRecord["msnfp_val_donations"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_donations");
				}
				else
				{
					mSNFP_EventPackage.ValDonations = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_donations.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_products") && queriedEntityRecord["msnfp_val_products"] != null)
				{
					mSNFP_EventPackage.ValProducts = ((Money)queriedEntityRecord["msnfp_val_products"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_products");
				}
				else
				{
					mSNFP_EventPackage.ValProducts = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_products.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_sponsorships") && queriedEntityRecord["msnfp_val_sponsorships"] != null)
				{
					mSNFP_EventPackage.ValSponsorships = ((Money)queriedEntityRecord["msnfp_val_sponsorships"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_sponsorships");
				}
				else
				{
					mSNFP_EventPackage.ValSponsorships = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_sponsorships.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_tickets") && queriedEntityRecord["msnfp_val_tickets"] != null)
				{
					mSNFP_EventPackage.ValTickets = ((Money)queriedEntityRecord["msnfp_val_tickets"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_tickets");
				}
				else
				{
					mSNFP_EventPackage.ValTickets = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_tickets.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_EventPackage.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_EventPackage.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_EventPackage.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_EventPackage.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_EventPackage.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_EventPackage.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_EventPackage.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_EventPackage.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_EventPackage.CreatedOn = null;
				}
				mSNFP_EventPackage.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_EventPackage.Deleted = true;
					mSNFP_EventPackage.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_EventPackage.Deleted = false;
					mSNFP_EventPackage.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_EventPackage));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_EventPackage);
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
