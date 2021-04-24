using System;
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
	public class EventDonationCreate : PluginBase
	{
		public EventDonationCreate(string unsecure, string secure)
			: base(typeof(EventDonationCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventDonationCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
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
				localContext.TracingService.Trace("---------Entering EventDonationCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_eventdonation", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_eventdonation", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting EventDonationCreate.cs---------");
		}

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventdonationid", "msnfp_amount", "msnfp_description", "msnfp_eventid", "msnfp_identifier", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "EventDonation";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				MSNFP_EventDonations mSNFP_EventDonations = new MSNFP_EventDonations();
				mSNFP_EventDonations.EventDonationId = (Guid)queriedEntityRecord["msnfp_eventdonationid"];
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_EventDonations.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount");
				}
				else
				{
					mSNFP_EventDonations.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_description") && queriedEntityRecord["msnfp_description"] != null)
				{
					mSNFP_EventDonations.Description = (string)queriedEntityRecord["msnfp_description"];
					localContext.TracingService.Trace("Got msnfp_description");
				}
				else
				{
					mSNFP_EventDonations.Description = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_description.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_EventDonations.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid");
				}
				else
				{
					mSNFP_EventDonations.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_EventDonations.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier");
				}
				else
				{
					mSNFP_EventDonations.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_EventDonations.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_EventDonations.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_EventDonations.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_EventDonations.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_EventDonations.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_EventDonations.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_EventDonations.CreatedOn = null;
				}
				mSNFP_EventDonations.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_EventDonations.Deleted = true;
					mSNFP_EventDonations.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_EventDonations.Deleted = false;
					mSNFP_EventDonations.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_EventDonations));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_EventDonations);
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
