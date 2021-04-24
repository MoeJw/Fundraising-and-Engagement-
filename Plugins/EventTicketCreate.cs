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
	public class EventTicketCreate : PluginBase
	{
		public EventTicketCreate(string unsecure, string secure)
			: base(typeof(EventTicketCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventTicketCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering EventTicketCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_eventticket", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						if (entity3.Contains("msnfp_tableticket") && (bool)entity3["msnfp_tableticket"])
						{
							CreateEventTableRecords(entity3, localContext, organizationService, pluginExecutionContext);
						}
						if (entity3.Contains("msnfp_quantity") && entity3["msnfp_quantity"] != null)
						{
							entity3["msnfp_sum_available"] = (int)entity3["msnfp_quantity"];
							organizationService.Update(entity3);
						}
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						if (entity3.Contains("msnfp_quantity") && entity3["msnfp_quantity"] != null)
						{
							UpdateEventTicketsAvailable(queriedEntityRecord, orgSvcContext, organizationService, localContext);
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_eventticket", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting EventTicketCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventticketid", "msnfp_amount", "msnfp_quantity", "msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_description", "msnfp_eventid", "msnfp_maxspots", "msnfp_registrationsperticket", "msnfp_sum_available", "msnfp_sum_sold", "msnfp_tickets", "msnfp_identifier", "msnfp_val_sold", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void UpdateEventTotalRegistration(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------Update Event Total Registration---------");
			if (!queriedEntityRecord.Contains("msnfp_eventid"))
			{
				return;
			}
			Entity eventRecord = service.Retrieve("msnfp_event", ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id, new ColumnSet("msnfp_eventid", "msnfp_sum_totalregistrations"));
			int num = 0;
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_eventticket")
				where ((EntityReference)a["msnfp_eventid"]).Id == eventRecord.Id && ((OptionSetValue)a["statuscode"]).Value != 844060000
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_sum_totalregistrations") && item["msnfp_sum_totalregistrations"] != null)
				{
					num += (int)item["msnfp_sum_totalregistrations"];
				}
			}
			eventRecord["msnfp_sum_totalregistrations"] = num;
			service.Update(eventRecord);
		}

		private void CreateEventTableRecords(Entity targetIncomingRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------CreateEventTableRecords---------");
			int num = 0;
			int num2 = 0;
			if (targetIncomingRecord.Contains("msnfp_registrationsperticket") && targetIncomingRecord["msnfp_registrationsperticket"] != null)
			{
				num = (int)targetIncomingRecord["msnfp_registrationsperticket"];
			}
			if (targetIncomingRecord.Contains("msnfp_quantity") && targetIncomingRecord["msnfp_quantity"] != null)
			{
				num2 = (int)targetIncomingRecord["msnfp_quantity"];
			}
			if (num2 <= 0)
			{
				return;
			}
			for (int i = 0; i < num2; i++)
			{
				Entity entity = new Entity("msnfp_eventtables");
				entity["msnfp_tablecapacity"] = num;
				entity["msnfp_eventticketid"] = new EntityReference("msnfp_eventticket", targetIncomingRecord.Id);
				if (targetIncomingRecord.Contains("msnfp_eventid") && targetIncomingRecord["msnfp_eventid"] != null)
				{
					entity["msnfp_eventid"] = new EntityReference("msnfp_event", ((EntityReference)targetIncomingRecord["msnfp_eventid"]).Id);
				}
				service.Create(entity);
			}
		}

		private void UpdateEventTicketsAvailable(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTicketsAvailable---------");
			Entity eventTicket = service.Retrieve("msnfp_eventticket", queriedEntityRecord.Id, new ColumnSet("msnfp_eventticketid", "msnfp_sum_available"));
			List<Entity> source = (from a in orgSvcContext.CreateQuery("msnfp_ticket")
				where ((EntityReference)a["msnfp_eventticketid"]).Id == eventTicket.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			if (queriedEntityRecord.Contains("msnfp_quantity") && queriedEntityRecord["msnfp_quantity"] != null)
			{
				eventTicket["msnfp_sum_available"] = (int)queriedEntityRecord["msnfp_quantity"] - source.Count();
			}
			service.Update(eventTicket);
		}

		private void UpdateEventTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTotals---------");
			int num = 0;
			decimal value = default(decimal);
			Entity entity = service.Retrieve("msnfp_event", ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id, new ColumnSet("msnfp_eventid", "msnfp_sum_tickets", "msnfp_count_tickets"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_eventticket")
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
			entity["msnfp_count_tickets"] = num;
			entity["msnfp_sum_tickets"] = new Money(value);
			decimal value2 = Utilities.CalculateEventTotalRevenue(entity, service, orgSvcContext, localContext.TracingService);
			entity["msnfp_sum_total"] = new Money(value2);
			service.Update(entity);
			localContext.TracingService.Trace("Event Record Updated");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "EventTicket";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_eventticketid"].ToString());
				MSNFP_EventTicket mSNFP_EventTicket = new MSNFP_EventTicket();
				mSNFP_EventTicket.EvenTicketId = (Guid)queriedEntityRecord["msnfp_eventticketid"];
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_EventTicket.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount");
				}
				else
				{
					mSNFP_EventTicket.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_EventTicket.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_EventTicket.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_EventTicket.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_EventTicket.AmountNonReceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_EventTicket.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_EventTicket.AmountTax = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_description") && queriedEntityRecord["msnfp_description"] != null)
				{
					mSNFP_EventTicket.Description = (string)queriedEntityRecord["msnfp_description"];
					localContext.TracingService.Trace("Got msnfp_description.");
				}
				else
				{
					mSNFP_EventTicket.Description = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_description.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_EventTicket.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_EventTicket.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_maxspots") && queriedEntityRecord["msnfp_maxspots"] != null)
				{
					mSNFP_EventTicket.MaxSpots = (int)queriedEntityRecord["msnfp_maxspots"];
					localContext.TracingService.Trace("Got msnfp_maxspots.");
				}
				else
				{
					mSNFP_EventTicket.MaxSpots = null;
					localContext.TracingService.Trace("Did NOT find msnfp_maxspots.");
				}
				if (queriedEntityRecord.Contains("msnfp_registrationsperticket") && queriedEntityRecord["msnfp_registrationsperticket"] != null)
				{
					mSNFP_EventTicket.RegistrationsPerTicket = (int)queriedEntityRecord["msnfp_registrationsperticket"];
					localContext.TracingService.Trace("Got msnfp_registrationsperticket.");
				}
				else
				{
					mSNFP_EventTicket.RegistrationsPerTicket = null;
					localContext.TracingService.Trace("Did NOT find msnfp_registrationsperticket.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_available") && queriedEntityRecord["msnfp_sum_available"] != null)
				{
					mSNFP_EventTicket.SumAvailable = (int)queriedEntityRecord["msnfp_sum_available"];
					localContext.TracingService.Trace("Got msnfp_sum_available.");
				}
				else
				{
					mSNFP_EventTicket.SumAvailable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_available.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_sold") && queriedEntityRecord["msnfp_sum_sold"] != null)
				{
					mSNFP_EventTicket.SumSold = (int)queriedEntityRecord["msnfp_sum_sold"];
					localContext.TracingService.Trace("Got msnfp_sum_sold.");
				}
				else
				{
					mSNFP_EventTicket.SumSold = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_sold.");
				}
				if (queriedEntityRecord.Contains("msnfp_tickets") && queriedEntityRecord["msnfp_tickets"] != null)
				{
					mSNFP_EventTicket.Tickets = (int)queriedEntityRecord["msnfp_tickets"];
					localContext.TracingService.Trace("Got msnfp_tickets.");
				}
				else
				{
					mSNFP_EventTicket.Tickets = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tickets.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_EventTicket.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_EventTicket.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_sold") && queriedEntityRecord["msnfp_val_sold"] != null)
				{
					mSNFP_EventTicket.ValTickets = ((Money)queriedEntityRecord["msnfp_val_sold"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_sold.");
				}
				else
				{
					mSNFP_EventTicket.ValTickets = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_sold.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_EventTicket.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_EventTicket.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_EventTicket.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_EventTicket.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_EventTicket.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_EventTicket.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_EventTicket.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_EventTicket.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_EventTicket.CreatedOn = null;
				}
				mSNFP_EventTicket.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_EventTicket.Deleted = true;
					mSNFP_EventTicket.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_EventTicket.Deleted = false;
					mSNFP_EventTicket.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_EventTicket));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_EventTicket);
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
