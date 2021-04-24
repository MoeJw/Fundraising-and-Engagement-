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
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class SponsorshipCreate : PluginBase
	{
		private const string PostImageAlias = "msnfp_sponsorship";

		public SponsorshipCreate(string unsecure, string secure)
			: base(typeof(SponsorshipCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered SponsorshipCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(organizationService);
			Entity queriedEntityRecord = null;
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
				localContext.TracingService.Trace("---------Entering SponsorshipCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_sponsorship", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						UpdateEventPackageSponsorshipTotals(entity3, orgSvcContext, organizationService, localContext);
						UpdateEventSponsorshipTotals(entity3, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						UpdateEventPackageSponsorshipTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
						UpdateEventSponsorshipTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting workflow.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_sponsorship", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting SponsorshipCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_sponsorshipid", "msnfp_customerid", "msnfp_eventid", "msnfp_eventpackageid", "msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_amount", "msnfp_date", "msnfp_description", "msnfp_name", "msnfp_identifier", "statecode", "statuscode", "createdon", "msnfp_eventpackageid", "msnfp_eventsponsorshipid");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Sponsorship";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_sponsorshipid"].ToString());
				MSNFP_Sponsorship mSNFP_Sponsorship = new MSNFP_Sponsorship();
				mSNFP_Sponsorship.SponsorshipId = (Guid)queriedEntityRecord["msnfp_sponsorshipid"];
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Sponsorship.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_Sponsorship.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_Sponsorship.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_Sponsorship.CustomerId = null;
					mSNFP_Sponsorship.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_Sponsorship.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_Sponsorship.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventpackageid") && queriedEntityRecord["msnfp_eventpackageid"] != null)
				{
					mSNFP_Sponsorship.EventPackageId = ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpackageid.");
				}
				else
				{
					mSNFP_Sponsorship.EventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventsponsorshipid") && queriedEntityRecord["msnfp_eventsponsorshipid"] != null)
				{
					mSNFP_Sponsorship.EventSponsorshipId = ((EntityReference)queriedEntityRecord["msnfp_eventsponsorshipid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventsponsorshipid.");
				}
				else
				{
					mSNFP_Sponsorship.EventSponsorshipId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventsponsorshipid.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_Sponsorship.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_Sponsorship.AmountReceipted = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_Sponsorship.AmountNonreceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Sponsorship.AmountNonreceiptable = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_Sponsorship.AmountNonreceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Sponsorship.AmountNonreceiptable = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_Sponsorship.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_Sponsorship.AmountTax = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Sponsorship.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_Sponsorship.Amount = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_date") && queriedEntityRecord["msnfp_date"] != null)
				{
					mSNFP_Sponsorship.Date = (DateTime)queriedEntityRecord["msnfp_date"];
					localContext.TracingService.Trace("Got msnfp_date.");
				}
				else
				{
					mSNFP_Sponsorship.Date = null;
					localContext.TracingService.Trace("Did NOT find msnfp_date.");
				}
				if (queriedEntityRecord.Contains("msnfp_description") && queriedEntityRecord["msnfp_description"] != null)
				{
					mSNFP_Sponsorship.Description = (string)queriedEntityRecord["msnfp_description"];
					localContext.TracingService.Trace("Got msnfp_description.");
				}
				else
				{
					mSNFP_Sponsorship.Description = null;
					localContext.TracingService.Trace("Did NOT find msnfp_description.");
				}
				if (queriedEntityRecord.Contains("msnfp_name") && queriedEntityRecord["msnfp_name"] != null)
				{
					mSNFP_Sponsorship.Name = (string)queriedEntityRecord["msnfp_name"];
					localContext.TracingService.Trace("Got msnfp_name.");
				}
				else
				{
					mSNFP_Sponsorship.Name = null;
					localContext.TracingService.Trace("Did NOT find msnfp_name.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_Sponsorship.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_Sponsorship.Identifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Sponsorship.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Sponsorship.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Sponsorship.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Sponsorship.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Sponsorship.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Sponsorship.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Sponsorship.CreatedOn = null;
				}
				mSNFP_Sponsorship.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Sponsorship.Deleted = true;
					mSNFP_Sponsorship.DeletedDate = DateTime.UtcNow;
					localContext.TracingService.Trace("Setting Deleted Date to:" + mSNFP_Sponsorship.DeletedDate.ToString());
				}
				else
				{
					mSNFP_Sponsorship.Deleted = false;
					mSNFP_Sponsorship.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Sponsorship));
				localContext.TracingService.Trace("Attempt to create JSON via serialization.");
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Sponsorship);
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
				string text3 = webAPIClient.UploadString(text2, @string);
				localContext.TracingService.Trace("Got response.");
				localContext.TracingService.Trace("Response: " + text3);
				Utilities utilities = new Utilities();
				utilities.CheckAPIReturnJSONForErrors(text3, configurationRecord.GetAttributeValue<OptionSetValue>("msnfp_showapierrorresponses"), localContext.TracingService);
			}
			else
			{
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting workflow.");
			}
		}

		private void UpdateEventPackageSponsorshipTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventPackageSponsorshipTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventpackageid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventPackage = service.Retrieve("msnfp_eventpackage", ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id, new ColumnSet("msnfp_eventpackageid", "msnfp_amount"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_sponsorship")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == eventPackage.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			localContext.TracingService.Trace("eventPackage.Id: " + eventPackage.Id.ToString());
			localContext.TracingService.Trace("sponsorshipList.Count(): " + list.Count());
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventPackage["msnfp_sum_sponsorships"] = list.Count();
			eventPackage["msnfp_val_sponsorships"] = new Money(value);
			service.Update(eventPackage);
		}

		private void UpdateEventSponsorshipTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventSponsorshipTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventsponsorshipid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventSponsorship = service.Retrieve("msnfp_eventsponsorship", ((EntityReference)queriedEntityRecord["msnfp_eventsponsorshipid"]).Id, new ColumnSet("msnfp_eventsponsorshipid", "msnfp_amount", "msnfp_quantity"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_sponsorship")
				where ((EntityReference)a["msnfp_eventsponsorshipid"]).Id == eventSponsorship.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventSponsorship["msnfp_sum_sold"] = list.Count();
			eventSponsorship["msnfp_val_sold"] = new Money(value);
			if (eventSponsorship.Contains("msnfp_quantity") && eventSponsorship["msnfp_quantity"] != null)
			{
				eventSponsorship["msnfp_sum_available"] = (int)eventSponsorship["msnfp_quantity"] - list.Count();
			}
			service.Update(eventSponsorship);
		}
	}
}
