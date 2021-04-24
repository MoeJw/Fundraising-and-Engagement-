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
	public class EventSponsorshipPageCreate : PluginBase
	{
		public EventSponsorshipPageCreate(string unsecure, string secure)
			: base(typeof(EventSponsorshipPageCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventSponsorshipPageCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering EventSponsorshipPageCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_eventsponsorship", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						if (entity3.Contains("msnfp_quantity") && entity3["msnfp_quantity"] != null)
						{
							Entity entity4 = new Entity(entity3.LogicalName, entity3.Id);
							entity4["msnfp_sum_available"] = (int)entity3["msnfp_quantity"];
							organizationService.Update(entity4);
						}
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						if (entity3.Contains("msnfp_quantity") && entity3["msnfp_quantity"] != null)
						{
							UpdateEventSponsorshipsAvailable(queriedEntityRecord, orgSvcContext, organizationService, localContext);
						}
						UpdateEventTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting plugin.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_eventsponsorship", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting EventSponsorshipPageCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventsponsorshipid", "msnfp_advantage", "msnfp_amount", "msnfp_date", "msnfp_description", "msnfp_eventid", "msnfp_order", "msnfp_quantity", "msnfp_fromamount", "msnfp_sum_available", "msnfp_val_sold", "msnfp_identifier", "msnfp_sum_sold", "transactioncurrencyid", "msnfp_amount_nonreceiptable", "msnfp_amount_receipted", "statecode", "statuscode", "createdon");
		}

		private void UpdateEventSponsorshipsAvailable(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			Entity eventSponsorship = service.Retrieve("msnfp_eventsponsorship", queriedEntityRecord.Id, new ColumnSet("msnfp_eventsponsorshipid", "msnfp_sum_available"));
			Entity entity = new Entity(eventSponsorship.LogicalName, eventSponsorship.Id);
			List<Entity> source = (from a in orgSvcContext.CreateQuery("msnfp_sponsorship")
				where ((EntityReference)a["msnfp_eventsponsorshipid"]).Id == eventSponsorship.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			if (queriedEntityRecord.Contains("msnfp_quantity") && queriedEntityRecord["msnfp_quantity"] != null)
			{
				entity["msnfp_sum_available"] = (int)queriedEntityRecord["msnfp_quantity"] - source.Count();
			}
			service.Update(entity);
		}

		private void UpdateEventTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTotals---------");
			int num = 0;
			decimal value = default(decimal);
			Entity entity = service.Retrieve("msnfp_event", ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id, new ColumnSet("msnfp_eventid", "msnfp_sum_sponsorships", "msnfp_count_sponsorships"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_eventsponsorship")
				where ((EntityReference)a["msnfp_eventid"]).Id == ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id && ((OptionSetValue)a["statecode"]).Value == 0 && ((OptionSetValue)a["statuscode"]).Value != 844060000
				select a).ToList();
			if (list.Count > 0)
			{
				foreach (Entity item in list)
				{
					if (item.Contains("msnfp_sum_sold"))
					{
						num += (int)item["msnfp_sum_sold"];
					}
					if (item.Contains("msnfp_val_sold"))
					{
						value += ((Money)item["msnfp_val_sold"]).Value;
					}
				}
			}
			entity["msnfp_count_sponsorships"] = num;
			entity["msnfp_sum_sponsorships"] = new Money(value);
			decimal value2 = Utilities.CalculateEventTotalRevenue(entity, service, orgSvcContext, localContext.TracingService);
			entity["msnfp_sum_total"] = new Money(value2);
			service.Update(entity);
			localContext.TracingService.Trace("Event Record Updated");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "EventSponsorship";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_eventsponsorshipid"].ToString());
				MSNFP_EventSponsorship mSNFP_EventSponsorship = new MSNFP_EventSponsorship();
				mSNFP_EventSponsorship.EventSponsorshipId = (Guid)queriedEntityRecord["msnfp_eventsponsorshipid"];
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_EventSponsorship.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_EventSponsorship.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_EventSponsorship.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_EventSponsorship.AmountNonReceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_EventSponsorship.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_EventSponsorship.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_date") && queriedEntityRecord["msnfp_date"] != null)
				{
					mSNFP_EventSponsorship.Date = (DateTime)queriedEntityRecord["msnfp_date"];
					localContext.TracingService.Trace("Got msnfp_date.");
				}
				else
				{
					mSNFP_EventSponsorship.Date = null;
					localContext.TracingService.Trace("Did NOT find msnfp_date.");
				}
				if (queriedEntityRecord.Contains("msnfp_description") && queriedEntityRecord["msnfp_description"] != null)
				{
					mSNFP_EventSponsorship.Description = (string)queriedEntityRecord["msnfp_description"];
					localContext.TracingService.Trace("Got msnfp_description.");
				}
				else
				{
					mSNFP_EventSponsorship.Description = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_description.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_EventSponsorship.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_EventSponsorship.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_order") && queriedEntityRecord["msnfp_order"] != null)
				{
					mSNFP_EventSponsorship.Order = (int)queriedEntityRecord["msnfp_order"];
					localContext.TracingService.Trace("Got msnfp_order.");
				}
				else
				{
					mSNFP_EventSponsorship.Order = null;
					localContext.TracingService.Trace("Did NOT find msnfp_order.");
				}
				if (queriedEntityRecord.Contains("msnfp_quantity") && queriedEntityRecord["msnfp_quantity"] != null)
				{
					mSNFP_EventSponsorship.Quantity = (int)queriedEntityRecord["msnfp_quantity"];
					localContext.TracingService.Trace("Got msnfp_quantity.");
				}
				else
				{
					mSNFP_EventSponsorship.Quantity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_quantity.");
				}
				if (queriedEntityRecord.Contains("msnfp_fromamount") && queriedEntityRecord["msnfp_fromamount"] != null)
				{
					mSNFP_EventSponsorship.FromAmount = ((Money)queriedEntityRecord["msnfp_fromamount"]).Value;
					localContext.TracingService.Trace("Got msnfp_fromamount.");
				}
				else
				{
					mSNFP_EventSponsorship.FromAmount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_fromamount.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_sold") && queriedEntityRecord["msnfp_val_sold"] != null)
				{
					mSNFP_EventSponsorship.ValSold = ((Money)queriedEntityRecord["msnfp_val_sold"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_sold.");
				}
				else
				{
					mSNFP_EventSponsorship.ValSold = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_sold.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_EventSponsorship.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_EventSponsorship.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_sold") && queriedEntityRecord["msnfp_sum_sold"] != null)
				{
					mSNFP_EventSponsorship.SumSold = (int)queriedEntityRecord["msnfp_sum_sold"];
					localContext.TracingService.Trace("Got msnfp_sum_sold.");
				}
				else
				{
					mSNFP_EventSponsorship.SumSold = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_sold.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_EventSponsorship.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_EventSponsorship.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_TransactionCurrencyId.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_EventSponsorship.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_EventSponsorship.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_EventSponsorship.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_EventSponsorship.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_EventSponsorship.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_EventSponsorship.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_EventSponsorship.CreatedOn = null;
				}
				mSNFP_EventSponsorship.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_EventSponsorship.Deleted = true;
					mSNFP_EventSponsorship.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_EventSponsorship.Deleted = false;
					mSNFP_EventSponsorship.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_EventSponsorship));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_EventSponsorship);
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
