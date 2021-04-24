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
	public class ReceiptStackCreate : PluginBase
	{
		public ReceiptStackCreate(string unsecure, string secure)
			: base(typeof(ReceiptStackCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered ReceiptStackCreate.cs---------");
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
			entity = Utilities.GetConfigurationRecordByMessageName(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering ReceiptStackCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_receiptstack", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_receiptstack", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting ReceiptStackCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_receiptstackid", "msnfp_configurationid", "msnfp_currentrange", "msnfp_numberrange", "msnfp_prefix", "msnfp_receiptyear", "msnfp_startingrange", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "ReceiptStack";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_receiptstackid"].ToString());
				MSNFP_ReceiptStack mSNFP_ReceiptStack = new MSNFP_ReceiptStack();
				mSNFP_ReceiptStack.ReceiptStackId = (Guid)queriedEntityRecord["msnfp_receiptstackid"];
				mSNFP_ReceiptStack.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_ReceiptStack.Identifier);
				if (queriedEntityRecord.Contains("msnfp_configurationid") && queriedEntityRecord["msnfp_configurationid"] != null)
				{
					mSNFP_ReceiptStack.ConfigurationId = ((EntityReference)queriedEntityRecord["msnfp_configurationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_configurationid.");
				}
				else
				{
					mSNFP_ReceiptStack.ConfigurationId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_configurationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_currentrange") && queriedEntityRecord["msnfp_currentrange"] != null)
				{
					mSNFP_ReceiptStack.CurrentRange = (double)queriedEntityRecord["msnfp_currentrange"];
					localContext.TracingService.Trace("Got msnfp_currentrange.");
				}
				else
				{
					mSNFP_ReceiptStack.CurrentRange = null;
					localContext.TracingService.Trace("Did NOT find msnfp_currentrange.");
				}
				if (queriedEntityRecord.Contains("msnfp_numberrange") && queriedEntityRecord["msnfp_numberrange"] != null)
				{
					mSNFP_ReceiptStack.NumberRange = ((OptionSetValue)queriedEntityRecord["msnfp_numberrange"]).Value;
					localContext.TracingService.Trace("Got msnfp_numberrange.");
				}
				else
				{
					mSNFP_ReceiptStack.NumberRange = null;
					localContext.TracingService.Trace("Did NOT find msnfp_numberrange.");
				}
				if (queriedEntityRecord.Contains("msnfp_prefix") && queriedEntityRecord["msnfp_prefix"] != null)
				{
					mSNFP_ReceiptStack.Prefix = (string)queriedEntityRecord["msnfp_prefix"];
					localContext.TracingService.Trace("Got msnfp_prefix.");
				}
				else
				{
					mSNFP_ReceiptStack.Prefix = null;
					localContext.TracingService.Trace("Did NOT find msnfp_prefix.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptyear") && queriedEntityRecord["msnfp_receiptyear"] != null)
				{
					mSNFP_ReceiptStack.ReceiptYear = ((OptionSetValue)queriedEntityRecord["msnfp_receiptyear"]).Value;
					localContext.TracingService.Trace("Got msnfp_receiptyear.");
				}
				else
				{
					mSNFP_ReceiptStack.ReceiptYear = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptyear.");
				}
				if (queriedEntityRecord.Contains("msnfp_startingrange") && queriedEntityRecord["msnfp_startingrange"] != null)
				{
					mSNFP_ReceiptStack.StartingRange = (double)queriedEntityRecord["msnfp_startingrange"];
					localContext.TracingService.Trace("Got msnfp_startingrange.");
				}
				else
				{
					mSNFP_ReceiptStack.StartingRange = null;
					localContext.TracingService.Trace("Did NOT find msnfp_startingrange.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_ReceiptStack.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_ReceiptStack.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_ReceiptStack.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_ReceiptStack.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_ReceiptStack.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_ReceiptStack.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_ReceiptStack.CreatedOn = null;
				}
				mSNFP_ReceiptStack.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_ReceiptStack.Deleted = true;
					mSNFP_ReceiptStack.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_ReceiptStack.Deleted = false;
					mSNFP_ReceiptStack.DeletedDate = null;
				}
				mSNFP_ReceiptStack.Configuration = null;
				mSNFP_ReceiptStack.Receipt = new HashSet<MSNFP_Receipt>();
				mSNFP_ReceiptStack.ReceiptLog = new HashSet<MSNFP_ReceiptLog>();
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_ReceiptStack));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_ReceiptStack);
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
	}
}
