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
	public class EventSponsorCreate : PluginBase
	{
		private const string PostImageAlias = "msnfp_eventsponsor";

		public EventSponsorCreate(string unsecure, string secure)
			: base(typeof(EventSponsorCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventSponsorCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering EventSponsorCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_eventsponsor", entity3.Id, GetColumnSet());
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
					localContext.TracingService.Trace("Target record not found. Exiting workflow.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_eventsponsor", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting EventSponsorCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventsponsorid", "msnfp_largeimage", "msnfp_order", "msnfp_orderdate", "msnfp_sponsortitle", "msnfp_identifier", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "EventSponsor";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_eventsponsorid"].ToString());
				MSNFP_EventSponsor mSNFP_EventSponsor = new MSNFP_EventSponsor();
				mSNFP_EventSponsor.EventSponsorId = (Guid)queriedEntityRecord["msnfp_eventsponsorid"];
				if (queriedEntityRecord.Contains("msnfp_largeimage") && queriedEntityRecord["msnfp_largeimage"] != null)
				{
					mSNFP_EventSponsor.LargeImage = (string)queriedEntityRecord["msnfp_largeimage"];
					localContext.TracingService.Trace("Got msnfp_largeimage.");
				}
				else
				{
					mSNFP_EventSponsor.LargeImage = null;
					localContext.TracingService.Trace("Did NOT find msnfp_largeimage.");
				}
				if (queriedEntityRecord.Contains("msnfp_order") && queriedEntityRecord["msnfp_order"] != null)
				{
					mSNFP_EventSponsor.Order = (int)queriedEntityRecord["msnfp_order"];
					localContext.TracingService.Trace("Got msnfp_order.");
				}
				else
				{
					mSNFP_EventSponsor.Order = null;
					localContext.TracingService.Trace("Did NOT find msnfp_order.");
				}
				if (queriedEntityRecord.Contains("msnfp_orderdate") && queriedEntityRecord["msnfp_orderdate"] != null)
				{
					mSNFP_EventSponsor.OrderDate = (DateTime)queriedEntityRecord["msnfp_orderdate"];
					localContext.TracingService.Trace("Got msnfp_orderdate.");
				}
				else
				{
					mSNFP_EventSponsor.OrderDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_orderdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_sponsortitle") && queriedEntityRecord["msnfp_sponsortitle"] != null)
				{
					mSNFP_EventSponsor.SponsorTitle = (string)queriedEntityRecord["msnfp_sponsortitle"];
					localContext.TracingService.Trace("Got msnfp_sponsortitle.");
				}
				else
				{
					mSNFP_EventSponsor.SponsorTitle = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sponsortitle.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_EventSponsor.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_EventSponsor.Identifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_EventSponsor.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_EventSponsor.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_EventSponsor.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_EventSponsor.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_EventSponsor.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_EventSponsor.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_EventSponsor.CreatedOn = null;
				}
				mSNFP_EventSponsor.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_EventSponsor.Deleted = true;
					mSNFP_EventSponsor.DeletedDate = DateTime.UtcNow;
					localContext.TracingService.Trace("Setting Deleted Date to:" + mSNFP_EventSponsor.DeletedDate.ToString());
				}
				else
				{
					mSNFP_EventSponsor.Deleted = false;
					mSNFP_EventSponsor.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_EventSponsor));
				localContext.TracingService.Trace("Attempt to create JSON via serialization.");
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_EventSponsor);
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
