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
	public class BankRunScheduleCreate : PluginBase
	{
		public BankRunScheduleCreate(string unsecure, string secure)
			: base(typeof(BankRunScheduleCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered BankRunScheduleCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering BankRunScheduleCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_bankrunschedule", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_bankrunschedule", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting BankRunScheduleCreate.cs---------");
		}

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_bankrunscheduleid", "msnfp_identifier", "msnfp_bankrunid", "msnfp_paymentscheduleid", "statuscode", "statecode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "BankRunSchedule";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_bankrunid"].ToString());
				MSNFP_BankRunSchedule mSNFP_BankRunSchedule = new MSNFP_BankRunSchedule();
				mSNFP_BankRunSchedule.BankRunScheduleId = (Guid)queriedEntityRecord["msnfp_bankrunscheduleid"];
				mSNFP_BankRunSchedule.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Identifier: " + mSNFP_BankRunSchedule.Identifier);
				if (queriedEntityRecord.Contains("msnfp_paymentscheduleid") && queriedEntityRecord["msnfp_paymentscheduleid"] != null)
				{
					mSNFP_BankRunSchedule.PaymentScheduleId = ((EntityReference)queriedEntityRecord["msnfp_paymentscheduleid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentscheduleid");
				}
				else
				{
					mSNFP_BankRunSchedule.PaymentScheduleId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentscheduleid");
				}
				if (queriedEntityRecord.Contains("msnfp_bankrunid") && queriedEntityRecord["msnfp_bankrunid"] != null)
				{
					mSNFP_BankRunSchedule.BankRunId = ((EntityReference)queriedEntityRecord["msnfp_bankrunid"]).Id;
					localContext.TracingService.Trace("Got msnfp_bankrunid");
				}
				else
				{
					mSNFP_BankRunSchedule.BankRunId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bankrunid");
				}
				if (messageName == "Create")
				{
					mSNFP_BankRunSchedule.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_BankRunSchedule.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_BankRunSchedule.CreatedOn = null;
				}
				if (messageName == "Delete")
				{
					mSNFP_BankRunSchedule.Deleted = true;
					mSNFP_BankRunSchedule.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_BankRunSchedule.Deleted = false;
					mSNFP_BankRunSchedule.DeletedDate = null;
				}
				mSNFP_BankRunSchedule.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
				mSNFP_BankRunSchedule.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_BankRunSchedule));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_BankRunSchedule);
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
