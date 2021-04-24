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
	public class TicketPageCreate : PluginBase
	{
		public TicketPageCreate(string unsecure, string secure)
			: base(typeof(TicketPageCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered TicketPageCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering TicketPageCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_ticket", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						UpdateEventPackageTicketTotals(entity3, orgSvcContext, organizationService, localContext);
						UpdateEventTicketTotals(entity3, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						UpdateEventPackageTicketTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
						UpdateEventTicketTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_ticket", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting TicketPageCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventpackageid", "msnfp_eventticketid", "msnfp_ticketid", "msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_amount", "msnfp_customerid", "msnfp_groupnotes", "msnfp_date", "msnfp_eventid", "msnfp_eventpackageid", "msnfp_registrationsperticket", "msnfp_eventticketid", "msnfp_identifier", "msnfp_name", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Ticket";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_ticketid"].ToString());
				MSNFP_Ticket mSNFP_Ticket = new MSNFP_Ticket();
				mSNFP_Ticket.TicketId = (Guid)queriedEntityRecord["msnfp_ticketid"];
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_Ticket.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted");
				}
				else
				{
					mSNFP_Ticket.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_Ticket.AmountNonreceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Ticket.AmountNonreceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_Ticket.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_Ticket.AmountTax = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Ticket.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_Ticket.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Ticket.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_Ticket.CustomerId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_groupnotes") && queriedEntityRecord["msnfp_groupnotes"] != null)
				{
					mSNFP_Ticket.GroupNotes = (string)queriedEntityRecord["msnfp_groupnotes"];
					localContext.TracingService.Trace("Got msnfp_groupnotes.");
				}
				else
				{
					mSNFP_Ticket.GroupNotes = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_groupnotes.");
				}
				if (queriedEntityRecord.Contains("msnfp_date") && queriedEntityRecord["msnfp_date"] != null)
				{
					mSNFP_Ticket.Date = (DateTime)queriedEntityRecord["msnfp_date"];
					localContext.TracingService.Trace("Got msnfp_date.");
				}
				else
				{
					mSNFP_Ticket.Date = null;
					localContext.TracingService.Trace("Did NOT find msnfp_date.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_Ticket.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_Ticket.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventpackageid") && queriedEntityRecord["msnfp_eventpackageid"] != null)
				{
					mSNFP_Ticket.EventPackageId = ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpackageid.");
				}
				else
				{
					mSNFP_Ticket.EventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_registrationsperticket") && queriedEntityRecord["msnfp_registrationsperticket"] != null)
				{
					mSNFP_Ticket.RegistrationsPerTicket = (int)queriedEntityRecord["msnfp_registrationsperticket"];
					localContext.TracingService.Trace("Got msnfp_registrationsperticket.");
				}
				else
				{
					mSNFP_Ticket.RegistrationsPerTicket = null;
					localContext.TracingService.Trace("Did NOT find msnfp_registrationsperticket.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventticketid") && queriedEntityRecord["msnfp_eventticketid"] != null)
				{
					mSNFP_Ticket.EventTicketId = ((EntityReference)queriedEntityRecord["msnfp_eventticketid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventticketid.");
				}
				else
				{
					mSNFP_Ticket.EventTicketId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventticketid.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_Ticket.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_Ticket.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_name") && queriedEntityRecord["msnfp_name"] != null)
				{
					mSNFP_Ticket.Name = (string)queriedEntityRecord["msnfp_name"];
					localContext.TracingService.Trace("Got msnfp_name.");
				}
				else
				{
					mSNFP_Ticket.Name = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_name.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_Ticket.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_Ticket.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Ticket.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Ticket.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Ticket.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Ticket.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Ticket.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Ticket.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Ticket.CreatedOn = null;
				}
				mSNFP_Ticket.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Ticket.Deleted = true;
					mSNFP_Ticket.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Ticket.Deleted = false;
					mSNFP_Ticket.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Ticket));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Ticket);
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

		private void UpdateEventPackageTicketTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventPackageTicketTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventpackageid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventPackage = service.Retrieve("msnfp_eventpackage", ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id, new ColumnSet("msnfp_eventpackageid", "msnfp_amount"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_ticket")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == eventPackage.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventPackage["msnfp_sum_tickets"] = list.Count();
			eventPackage["msnfp_val_tickets"] = new Money(value);
			service.Update(eventPackage);
		}

		private void UpdateEventTicketTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTicketTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventticketid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventTicket = service.Retrieve("msnfp_eventticket", ((EntityReference)queriedEntityRecord["msnfp_eventticketid"]).Id, new ColumnSet("msnfp_eventticketid", "msnfp_amount", "msnfp_quantity"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_ticket")
				where ((EntityReference)a["msnfp_eventticketid"]).Id == eventTicket.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventTicket["msnfp_sum_sold"] = list.Count();
			eventTicket["msnfp_val_sold"] = new Money(value);
			if (eventTicket.Contains("msnfp_quantity") && eventTicket["msnfp_quantity"] != null)
			{
				eventTicket["msnfp_sum_available"] = (int)eventTicket["msnfp_quantity"] - list.Count();
			}
			service.Update(eventTicket);
		}
	}
}
