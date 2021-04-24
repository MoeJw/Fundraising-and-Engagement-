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
	public class BankRunCreate : PluginBase
	{
		public BankRunCreate(string unsecure, string secure)
			: base(typeof(BankRunCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered BankRunCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering BankRunCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_bankrun", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_bankrun", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting BankRunCreate.cs---------");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "BankRun";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_bankrunid"].ToString());
				MSNFP_BankRun mSNFP_BankRun = new MSNFP_BankRun();
				mSNFP_BankRun.BankRunId = (Guid)queriedEntityRecord["msnfp_bankrunid"];
				mSNFP_BankRun.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Identifier: " + mSNFP_BankRun.Identifier);
				if (queriedEntityRecord.Contains("msnfp_startdate") && queriedEntityRecord["msnfp_startdate"] != null)
				{
					mSNFP_BankRun.StartDate = (DateTime)queriedEntityRecord["msnfp_startdate"];
					localContext.TracingService.Trace("Got msnfp_startdate.");
				}
				else
				{
					mSNFP_BankRun.StartDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_startdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_enddate") && queriedEntityRecord["msnfp_enddate"] != null)
				{
					mSNFP_BankRun.EndDate = (DateTime)queriedEntityRecord["msnfp_enddate"];
					localContext.TracingService.Trace("Got msnfp_enddate.");
				}
				else
				{
					mSNFP_BankRun.EndDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_enddate.");
				}
				if (queriedEntityRecord.Contains("msnfp_datetobeprocessed") && queriedEntityRecord["msnfp_datetobeprocessed"] != null)
				{
					mSNFP_BankRun.DateToBeProcessed = (DateTime)queriedEntityRecord["msnfp_datetobeprocessed"];
					localContext.TracingService.Trace("Got msnfp_datetobeprocessed.");
				}
				else
				{
					mSNFP_BankRun.DateToBeProcessed = null;
					localContext.TracingService.Trace("Did NOT find msnfp_datetobeprocessed.");
				}
				if (queriedEntityRecord.Contains("msnfp_bankrunstatus") && queriedEntityRecord["msnfp_bankrunstatus"] != null)
				{
					mSNFP_BankRun.BankRunStatus = ((OptionSetValue)queriedEntityRecord["msnfp_bankrunstatus"]).Value;
					localContext.TracingService.Trace("Got msnfp_bankrunstatus.");
				}
				else
				{
					mSNFP_BankRun.BankRunStatus = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bankrunstatus.");
				}
				if (queriedEntityRecord.Contains("msnfp_accounttocreditid") && queriedEntityRecord["msnfp_accounttocreditid"] != null)
				{
					mSNFP_BankRun.AccountToCreditId = ((EntityReference)queriedEntityRecord["msnfp_accounttocreditid"]).Id;
					localContext.TracingService.Trace("Got msnfp_accounttocreditid");
				}
				else
				{
					mSNFP_BankRun.AccountToCreditId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_accounttocreditid");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentprocessorid") && queriedEntityRecord["msnfp_paymentprocessorid"] != null)
				{
					mSNFP_BankRun.PaymentProcessorId = ((EntityReference)queriedEntityRecord["msnfp_paymentprocessorid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentprocessorid");
				}
				else
				{
					mSNFP_BankRun.PaymentProcessorId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentprocessorid");
				}
				if (queriedEntityRecord.Contains("msnfp_filecreationnumber") && queriedEntityRecord["msnfp_filecreationnumber"] != null)
				{
					mSNFP_BankRun.FileCreationNumber = (int)queriedEntityRecord["msnfp_filecreationnumber"];
					localContext.TracingService.Trace("Got msnfp_filecreationnumber");
				}
				else
				{
					mSNFP_BankRun.FileCreationNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_filecreationnumber");
				}
				if (messageName == "Create")
				{
					mSNFP_BankRun.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_BankRun.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_BankRun.CreatedOn = null;
				}
				if (messageName == "Delete")
				{
					mSNFP_BankRun.Deleted = true;
					mSNFP_BankRun.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_BankRun.Deleted = false;
					mSNFP_BankRun.DeletedDate = null;
				}
				mSNFP_BankRun.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
				mSNFP_BankRun.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_BankRun));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_BankRun);
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

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_bankrunid", "msnfp_identifier", "msnfp_startdate", "msnfp_enddate", "msnfp_datetobeprocessed", "msnfp_bankrunstatus", "msnfp_paymentprocessorid", "msnfp_accounttocreditid", "statuscode", "statecode", "createdon", "msnfp_filecreationnumber");
		}
	}
}
