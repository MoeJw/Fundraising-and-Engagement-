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
	public class RegistrationPreferenceCreate : PluginBase
	{
		public RegistrationPreferenceCreate(string unsecure, string secure)
			: base(typeof(RegistrationPreferenceCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered RegistrationPreferenceCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			if (pluginExecutionContext.Depth > 2)
			{
				localContext.TracingService.Trace("Context.depth > 2. Exiting Plugin.");
				return;
			}
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
				localContext.TracingService.Trace("---------Entering RegistrationPreferenceCreate.cs Main Function---------");
				localContext.TracingService.Trace("Message Name: " + messageName);
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_registrationpreference", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_registrationpreference", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting RegistrationPreferenceCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_registrationpreferenceid", "msnfp_registrationid", "msnfp_eventpreference", "msnfp_eventid", "msnfp_other", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "RegistrationPreference";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_registrationpreferenceid"].ToString());
				MSNFP_RegistrationPreference mSNFP_RegistrationPreference = new MSNFP_RegistrationPreference();
				mSNFP_RegistrationPreference.registrationpreferenceid = (Guid)queriedEntityRecord["msnfp_registrationpreferenceid"];
				if (queriedEntityRecord.Contains("msnfp_registrationid") && queriedEntityRecord["msnfp_registrationid"] != null)
				{
					mSNFP_RegistrationPreference.registrationid = ((EntityReference)queriedEntityRecord["msnfp_registrationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_registrationid");
				}
				else
				{
					mSNFP_RegistrationPreference.registrationid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_registrationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventpreference") && queriedEntityRecord["msnfp_eventpreference"] != null)
				{
					mSNFP_RegistrationPreference.eventpreference = ((EntityReference)queriedEntityRecord["msnfp_eventpreference"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpreference");
				}
				else
				{
					mSNFP_RegistrationPreference.eventpreference = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpreference.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_RegistrationPreference.eventid = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid");
				}
				else
				{
					mSNFP_RegistrationPreference.eventid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_other") && queriedEntityRecord["msnfp_other"] != null)
				{
					mSNFP_RegistrationPreference.other = (string)queriedEntityRecord["msnfp_other"];
					localContext.TracingService.Trace("Got msnfp_other.");
				}
				else
				{
					mSNFP_RegistrationPreference.other = null;
					localContext.TracingService.Trace("Did NOT find msnfp_other.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_RegistrationPreference.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_RegistrationPreference.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_RegistrationPreference.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_RegistrationPreference.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_RegistrationPreference.createdon = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_RegistrationPreference.createdon = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_RegistrationPreference.createdon = null;
				}
				mSNFP_RegistrationPreference.syncdate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_RegistrationPreference.deleted = true;
					mSNFP_RegistrationPreference.deleteddate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_RegistrationPreference.deleted = false;
					mSNFP_RegistrationPreference.deleteddate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_RegistrationPreference));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_RegistrationPreference);
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
