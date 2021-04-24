using System;
using System.Collections.Generic;
using System.IO;
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
	public class ConfigurationCreate : PluginBase
	{
		public ConfigurationCreate(string unsecure, string secure)
			: base(typeof(ConfigurationCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			Utilities utilities = new Utilities();
			localContext.TracingService.Trace("Configuration Create.");
			string messageName = pluginExecutionContext.MessageName;
			Entity entity = null;
			string empty = string.Empty;
			if (!pluginExecutionContext.InputParameters.Contains("Target") || (!(pluginExecutionContext.InputParameters["Target"] is Entity) && !(pluginExecutionContext.InputParameters["Target"] is EntityReference)))
			{
				return;
			}
			localContext.TracingService.Trace("Triggered By: " + messageName);
			if (messageName == "Create")
			{
				entity = (Entity)pluginExecutionContext.InputParameters["Target"];
			}
			else if (messageName == "Update" || messageName == "Delete")
			{
				entity = organizationService.Retrieve("msnfp_configuration", pluginExecutionContext.PrimaryEntityId, GetColumnSet());
				localContext.TracingService.Trace("Obtained values");
			}
			if (entity == null || (!(messageName == "Create") && !(messageName == "Update") && !(messageName == "Delete")))
			{
				return;
			}
			localContext.TracingService.Trace("Configuration is Primary entity. Plugin Called On: " + messageName);
			MSNFP_Configuration mSNFP_Configuration = new MSNFP_Configuration();
			mSNFP_Configuration.ConfigurationId = (Guid)entity["msnfp_configurationid"];
			mSNFP_Configuration.Identifier = (entity.Contains("msnfp_identifier") ? ((string)entity["msnfp_identifier"]) : string.Empty);
			localContext.TracingService.Trace("Obtaining JSON Fields to send to API.");
			mSNFP_Configuration.AzureWebApiUrl = (entity.Contains("msnfp_azure_webapiurl") ? ((string)entity["msnfp_azure_webapiurl"]) : string.Empty);
			mSNFP_Configuration.AddressAuth1 = (entity.Contains("msnfp_addressauth1") ? ((string)entity["msnfp_addressauth1"]) : string.Empty);
			mSNFP_Configuration.AddressAuth2 = (entity.Contains("msnfp_addressauth2") ? ((string)entity["msnfp_addressauth2"]) : string.Empty);
			mSNFP_Configuration.BankRunPregeneratedBy = ((!entity.Contains("msnfp_bankrun_pregeneratedby")) ? 1 : ((int)entity["msnfp_bankrun_pregeneratedby"]));
			mSNFP_Configuration.CharityTitle = (entity.Contains("msnfp_charitytitle") ? ((string)entity["msnfp_charitytitle"]) : string.Empty);
			mSNFP_Configuration.ScheMaxRetries = (entity.Contains("msnfp_sche_maxretries") ? ((int)entity["msnfp_sche_maxretries"]) : 0);
			if (entity.Contains("msnfp_sche_recurrencestart"))
			{
				mSNFP_Configuration.ScheRecurrenceStart = ((OptionSetValue)entity["msnfp_sche_recurrencestart"]).Value;
			}
			else
			{
				mSNFP_Configuration.ScheRecurrenceStart = null;
			}
			mSNFP_Configuration.ScheRetryinterval = ((!entity.Contains("msnfp_sche_retryinterval")) ? 1 : ((int)entity["msnfp_sche_retryinterval"]));
			localContext.TracingService.Trace("Get Entity References.");
			if (entity.Contains("msnfp_teamownerid") && entity["msnfp_teamownerid"] != null)
			{
				mSNFP_Configuration.TeamOwnerId = ((EntityReference)entity["msnfp_teamownerid"]).Id;
			}
			else
			{
				mSNFP_Configuration.TeamOwnerId = null;
			}
			if (entity.Contains("msnfp_paymentprocessorid") && entity["msnfp_paymentprocessorid"] != null)
			{
				mSNFP_Configuration.PaymentProcessorId = ((EntityReference)entity["msnfp_paymentprocessorid"]).Id;
			}
			else
			{
				mSNFP_Configuration.PaymentProcessorId = null;
			}
			if (entity.Contains("msnfp_defaultconfiguration") && entity["msnfp_defaultconfiguration"] != null)
			{
				mSNFP_Configuration.DefaultConfiguration = (bool)entity["msnfp_defaultconfiguration"];
				localContext.TracingService.Trace("Got Default Configuration.");
			}
			else
			{
				mSNFP_Configuration.DefaultConfiguration = null;
				localContext.TracingService.Trace("Did NOT find Default Configuration.");
			}
			if (entity.Contains("statecode") && entity["statecode"] != null)
			{
				mSNFP_Configuration.StateCode = ((OptionSetValue)entity["statecode"]).Value;
				localContext.TracingService.Trace("Got statecode.");
			}
			else
			{
				mSNFP_Configuration.StateCode = null;
				localContext.TracingService.Trace("Did NOT find statecode.");
			}
			if (entity.Contains("statuscode") && entity["statuscode"] != null)
			{
				mSNFP_Configuration.StatusCode = ((OptionSetValue)entity["statuscode"]).Value;
				localContext.TracingService.Trace("Got statuscode.");
			}
			else
			{
				mSNFP_Configuration.StatusCode = null;
				localContext.TracingService.Trace("Did NOT find statuscode.");
			}
			localContext.TracingService.Trace("Set remaining JSON fields to null as they are not used here.");
			if (messageName == "Create")
			{
				mSNFP_Configuration.CreatedOn = DateTime.UtcNow;
			}
			else if (entity.Contains("createdon") && entity["createdon"] != null)
			{
				mSNFP_Configuration.CreatedOn = (DateTime)entity["createdon"];
			}
			else
			{
				mSNFP_Configuration.CreatedOn = null;
			}
			mSNFP_Configuration.SyncDate = DateTime.UtcNow;
			if (messageName == "Delete")
			{
				mSNFP_Configuration.Deleted = true;
				mSNFP_Configuration.DeletedDate = DateTime.UtcNow;
			}
			else
			{
				mSNFP_Configuration.Deleted = false;
				mSNFP_Configuration.DeletedDate = null;
			}
			mSNFP_Configuration.PaymentProcessor = null;
			mSNFP_Configuration.Event = new HashSet<MSNFP_Event>();
			mSNFP_Configuration.EventPackage = new HashSet<MSNFP_EventPackage>();
			mSNFP_Configuration.PaymentSchedule = new HashSet<MSNFP_PaymentSchedule>();
			mSNFP_Configuration.ReceiptStack = new HashSet<MSNFP_ReceiptStack>();
			mSNFP_Configuration.Transaction = new HashSet<MSNFP_Transaction>();
			localContext.TracingService.Trace("Gathered fields.");
			empty = mSNFP_Configuration.AzureWebApiUrl;
			localContext.TracingService.Trace("API URL: " + empty);
			if (empty.Length > 0)
			{
				localContext.TracingService.Trace("Syncing data to Azure.");
				if (messageName == "Update" || messageName == "Delete")
				{
					empty += "Configuration/UpdateConfiguration";
				}
				else if (messageName == "Create")
				{
					empty += "Configuration/CreateConfiguration";
				}
				MemoryStream memoryStream = new MemoryStream();
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Configuration));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Configuration);
				byte[] array = memoryStream.ToArray();
				memoryStream.Close();
				string @string = Encoding.UTF8.GetString(array, 0, array.Length);
				localContext.TracingService.Trace("---------Preparing JSON---------");
				localContext.TracingService.Trace("Converted to json API URL : " + empty);
				localContext.TracingService.Trace("JSON: " + @string);
				localContext.TracingService.Trace("---------End of Preparing JSON---------");
				localContext.TracingService.Trace("Sending data to Azure.");
				WebAPIClient webAPIClient = new WebAPIClient();
				webAPIClient.Headers[HttpRequestHeader.ContentType] = "application/json";
				if (entity.Contains("msnfp_apipadlocktoken"))
				{
					if ((string)entity["msnfp_apipadlocktoken"] == null)
					{
						return;
					}
					webAPIClient.Headers["Padlock"] = (string)entity["msnfp_apipadlocktoken"];
					webAPIClient.Encoding = Encoding.UTF8;
					string text = webAPIClient.UploadString(empty, @string);
					localContext.TracingService.Trace("Got response.");
					localContext.TracingService.Trace("Response: " + text);
					Utilities utilities2 = new Utilities();
					utilities2.CheckAPIReturnJSONForErrors(text, entity.GetAttributeValue<OptionSetValue>("msnfp_showapierrorresponses"), localContext.TracingService);
					EntityCollection entityCollection = organizationService.RetrieveMultiple(new QueryExpression
					{
						EntityName = "transactioncurrency",
						ColumnSet = new ColumnSet("transactioncurrencyid")
					});
					foreach (Entity entity2 in entityCollection.Entities)
					{
						OrganizationRequest organizationRequest = new OrganizationRequest("msnfp_ActionSyncCurrency");
						organizationRequest["Target"] = entity2.ToEntityReference();
						organizationService.Execute(organizationRequest);
						localContext.TracingService.Trace($"currency {entity2.Id} updated.");
					}
				}
				else
				{
					localContext.TracingService.Trace("No padlock found, did not send data to Azure.");
				}
			}
			else
			{
				localContext.TracingService.Trace("API URL is null or Portal Syncing is turned off. Exiting Plugin.");
			}
		}

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("statuscode", "statecode", "msnfp_defaultconfiguration", "msnfp_configurationid", "msnfp_identifier", "msnfp_azure_webapiurl", "msnfp_bankrun_pregeneratedby", "msnfp_charitytitle", "msnfp_sche_maxretries", "msnfp_sche_retryinterval", "msnfp_teamownerid", "msnfp_sche_recurrencestart", "msnfp_paymentprocessorid", "createdon", "msnfp_apipadlocktoken");
		}
	}
}
