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
	public class TributeOrMemoryCreate : PluginBase
	{
		private const string PostImageAlias = "msnfp_tributeormemory";

		public TributeOrMemoryCreate(string unsecure, string secure)
			: base(typeof(TributeOrMemoryCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered TributeOrMemoryCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering TributeOrMemoryCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_tributeormemory", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_tributeormemory", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting TributeOrMemoryCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_tributeormemoryid", "msnfp_identifier", "msnfp_name", "msnfp_tributeormemorytypecode", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "TributeOrMemory";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_tributeormemoryid"].ToString());
				MSNFP_TributeOrMemory mSNFP_TributeOrMemory = new MSNFP_TributeOrMemory();
				mSNFP_TributeOrMemory.TributeOrMemoryId = (Guid)queriedEntityRecord["msnfp_tributeormemoryid"];
				if (queriedEntityRecord.Contains("msnfp_name") && queriedEntityRecord["msnfp_name"] != null)
				{
					mSNFP_TributeOrMemory.Name = (string)queriedEntityRecord["msnfp_name"];
					localContext.TracingService.Trace("Got msnfp_name.");
				}
				else
				{
					mSNFP_TributeOrMemory.Name = null;
					localContext.TracingService.Trace("Did NOT find msnfp_name.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_TributeOrMemory.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_TributeOrMemory.Identifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_tributeormemorytypecode") && queriedEntityRecord["msnfp_tributeormemorytypecode"] != null)
				{
					mSNFP_TributeOrMemory.TributeOrMemoryTypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_tributeormemorytypecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_tributeormemorytypecode.");
				}
				else
				{
					mSNFP_TributeOrMemory.TributeOrMemoryTypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tributeormemorytypecode.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_TributeOrMemory.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_TributeOrMemory.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_TributeOrMemory.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_TributeOrMemory.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_TributeOrMemory.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_TributeOrMemory.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_TributeOrMemory.CreatedOn = null;
				}
				mSNFP_TributeOrMemory.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_TributeOrMemory.Deleted = true;
					mSNFP_TributeOrMemory.DeletedDate = DateTime.UtcNow;
					localContext.TracingService.Trace("Setting Deleted Date to:" + mSNFP_TributeOrMemory.DeletedDate.ToString());
				}
				else
				{
					mSNFP_TributeOrMemory.Deleted = false;
					mSNFP_TributeOrMemory.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_TributeOrMemory));
				localContext.TracingService.Trace("Attempt to create JSON via serialization.");
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_TributeOrMemory);
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
