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
	public class ReceiptLogCreate : PluginBase
	{
		private const string PostImageAlias = "msnfp_receiptlog";

		public ReceiptLogCreate(string unsecure, string secure)
			: base(typeof(ReceiptLogCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered ReceiptLogCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering ReceiptLogCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_receiptlog", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_receiptlog", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting ReceiptLogCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_receiptlogid", "msnfp_receiptstackid", "msnfp_entryby", "msnfp_entryreason", "msnfp_receiptnumber", "msnfp_identifier", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "ReceiptLog";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_receiptlogid"].ToString());
				MSNFP_ReceiptLog mSNFP_ReceiptLog = new MSNFP_ReceiptLog();
				mSNFP_ReceiptLog.ReceiptLogId = (Guid)queriedEntityRecord["msnfp_receiptlogid"];
				if (queriedEntityRecord.Contains("msnfp_receiptstackid") && queriedEntityRecord["msnfp_receiptstackid"] != null)
				{
					mSNFP_ReceiptLog.ReceiptStackId = ((EntityReference)queriedEntityRecord["msnfp_receiptstackid"]).Id;
					localContext.TracingService.Trace("Got msnfp_receiptstackid.");
				}
				else
				{
					mSNFP_ReceiptLog.ReceiptStackId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptstackid.");
				}
				if (queriedEntityRecord.Contains("msnfp_entryby") && queriedEntityRecord["msnfp_entryby"] != null)
				{
					mSNFP_ReceiptLog.EntryBy = (string)queriedEntityRecord["msnfp_entryby"];
					localContext.TracingService.Trace("Got msnfp_entryby.");
				}
				else
				{
					mSNFP_ReceiptLog.EntryBy = null;
					localContext.TracingService.Trace("Did NOT find msnfp_entryby.");
				}
				if (queriedEntityRecord.Contains("msnfp_entryreason") && queriedEntityRecord["msnfp_entryreason"] != null)
				{
					mSNFP_ReceiptLog.EntryReason = (string)queriedEntityRecord["msnfp_entryreason"];
					localContext.TracingService.Trace("Got msnfp_entryreason.");
				}
				else
				{
					mSNFP_ReceiptLog.EntryReason = null;
					localContext.TracingService.Trace("Did NOT find msnfp_entryreason.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptnumber") && queriedEntityRecord["msnfp_receiptnumber"] != null)
				{
					mSNFP_ReceiptLog.ReceiptNumber = (string)queriedEntityRecord["msnfp_receiptnumber"];
					localContext.TracingService.Trace("Got msnfp_receiptnumber.");
				}
				else
				{
					mSNFP_ReceiptLog.ReceiptNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptnumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_ReceiptLog.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_ReceiptLog.Identifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_ReceiptLog.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_ReceiptLog.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_ReceiptLog.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_ReceiptLog.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_ReceiptLog.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_ReceiptLog.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_ReceiptLog.CreatedOn = null;
				}
				mSNFP_ReceiptLog.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_ReceiptLog.Deleted = true;
					mSNFP_ReceiptLog.DeletedDate = DateTime.UtcNow;
					localContext.TracingService.Trace("Setting Deleted Date to:" + mSNFP_ReceiptLog.DeletedDate.ToString());
				}
				else
				{
					mSNFP_ReceiptLog.Deleted = false;
					mSNFP_ReceiptLog.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_ReceiptLog));
				localContext.TracingService.Trace("Attempt to create JSON via serialization.");
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_ReceiptLog);
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
