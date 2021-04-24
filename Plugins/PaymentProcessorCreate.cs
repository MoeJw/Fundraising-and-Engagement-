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
	public class PaymentProcessorCreate : PluginBase
	{
		public PaymentProcessorCreate(string unsecure, string secure)
			: base(typeof(PaymentProcessorCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered PaymentProcessorCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering PaymentProcessorCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_paymentprocessor", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting PaymentProcessorCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_paymentprocessorid", "msnfp_name", "msnfp_apikey", "msnfp_name", "msnfp_identifier", "msnfp_paymentgatewaytype", "msnfp_storeid", "msnfp_testmode", "statecode", "statuscode", "msnfp_bankrunfileformat", "msnfp_scotiabankcustomernumber", "msnfp_originatorshortname", "msnfp_originatorlongname", "msnfp_bmooriginatorid", "msnfp_abaremittername", "msnfp_abausername", "msnfp_abausernumber", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "PaymentProcessor";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_paymentprocessorid"].ToString());
				MSNFP_PaymentProcessor mSNFP_PaymentProcessor = new MSNFP_PaymentProcessor();
				mSNFP_PaymentProcessor.PaymentProcessorId = (Guid)queriedEntityRecord["msnfp_paymentprocessorid"];
				mSNFP_PaymentProcessor.Name = (queriedEntityRecord.Contains("msnfp_name") ? ((string)queriedEntityRecord["msnfp_name"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_PaymentProcessor.Name);
				if (queriedEntityRecord.Contains("msnfp_apikey") && queriedEntityRecord["msnfp_apikey"] != null)
				{
					mSNFP_PaymentProcessor.MonerisApiKey = (string)queriedEntityRecord["msnfp_apikey"];
					localContext.TracingService.Trace("Got msnfp_apikey.");
				}
				else
				{
					mSNFP_PaymentProcessor.MonerisApiKey = null;
					localContext.TracingService.Trace("Did NOT find msnfp_apikey.");
				}
				if (queriedEntityRecord.Contains("msnfp_name") && queriedEntityRecord["msnfp_name"] != null)
				{
					mSNFP_PaymentProcessor.Name = (string)queriedEntityRecord["msnfp_name"];
					localContext.TracingService.Trace("Got msnfp_name.");
				}
				else
				{
					mSNFP_PaymentProcessor.Name = null;
					localContext.TracingService.Trace("Did NOT find msnfp_name.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_PaymentProcessor.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_PaymentProcessor.Identifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentgatewaytype") && queriedEntityRecord["msnfp_paymentgatewaytype"] != null)
				{
					mSNFP_PaymentProcessor.PaymentGatewayType = ((OptionSetValue)queriedEntityRecord["msnfp_paymentgatewaytype"]).Value;
					localContext.TracingService.Trace("Got msnfp_paymentgatewaytype.");
				}
				else
				{
					mSNFP_PaymentProcessor.PaymentGatewayType = 844060000;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentgatewaytype.");
				}
				if (queriedEntityRecord.Contains("msnfp_storeid") && queriedEntityRecord["msnfp_storeid"] != null)
				{
					mSNFP_PaymentProcessor.MonerisStoreId = (string)queriedEntityRecord["msnfp_storeid"];
					localContext.TracingService.Trace("Got msnfp_storeid.");
				}
				else
				{
					mSNFP_PaymentProcessor.MonerisStoreId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_storeid.");
				}
				if (queriedEntityRecord.Contains("msnfp_testmode") && queriedEntityRecord["msnfp_testmode"] != null)
				{
					mSNFP_PaymentProcessor.MonerisTestMode = (bool)queriedEntityRecord["msnfp_testmode"];
					localContext.TracingService.Trace("Got msnfp_testmode.");
				}
				else
				{
					mSNFP_PaymentProcessor.MonerisTestMode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_testmode.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_PaymentProcessor.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_PaymentProcessor.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_PaymentProcessor.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_PaymentProcessor.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (queriedEntityRecord.Contains("msnfp_bankrunfileformat") && queriedEntityRecord["msnfp_bankrunfileformat"] != null)
				{
					mSNFP_PaymentProcessor.BankRunFileFormat = ((OptionSetValue)queriedEntityRecord["msnfp_bankrunfileformat"]).Value;
					localContext.TracingService.Trace("Got msnfp_bankrunfileformat.");
				}
				else
				{
					mSNFP_PaymentProcessor.BankRunFileFormat = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bankrunfileformat.");
				}
				if (queriedEntityRecord.Contains("msnfp_scotiabankcustomernumber") && queriedEntityRecord["msnfp_scotiabankcustomernumber"] != null)
				{
					mSNFP_PaymentProcessor.ScotiabankCustomerNumber = (string)queriedEntityRecord["msnfp_scotiabankcustomernumber"];
					localContext.TracingService.Trace("Got msnfp_scotiabankcustomernumber.");
				}
				else
				{
					mSNFP_PaymentProcessor.ScotiabankCustomerNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_scotiabankcustomernumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_originatorshortname") && queriedEntityRecord["msnfp_originatorshortname"] != null)
				{
					mSNFP_PaymentProcessor.OriginatorShortName = (string)queriedEntityRecord["msnfp_originatorshortname"];
					localContext.TracingService.Trace("Got msnfp_originatorshortname.");
				}
				else
				{
					mSNFP_PaymentProcessor.OriginatorShortName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_originatorshortname.");
				}
				if (queriedEntityRecord.Contains("msnfp_originatorlongname") && queriedEntityRecord["msnfp_originatorlongname"] != null)
				{
					mSNFP_PaymentProcessor.OriginatorLongName = (string)queriedEntityRecord["msnfp_originatorlongname"];
					localContext.TracingService.Trace("Got msnfp_originatorlongname.");
				}
				else
				{
					mSNFP_PaymentProcessor.OriginatorLongName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_originatorlongname.");
				}
				if (queriedEntityRecord.Contains("msnfp_bmooriginatorid") && queriedEntityRecord["msnfp_bmooriginatorid"] != null)
				{
					mSNFP_PaymentProcessor.BmoOriginatorId = (string)queriedEntityRecord["msnfp_bmooriginatorid"];
					localContext.TracingService.Trace("Got msnfp_bmooriginatorid.");
				}
				else
				{
					mSNFP_PaymentProcessor.BmoOriginatorId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bmooriginatorid.");
				}
				if (queriedEntityRecord.Contains("msnfp_abaremittername") && queriedEntityRecord["msnfp_abaremittername"] != null)
				{
					mSNFP_PaymentProcessor.AbaRemitterName = (string)queriedEntityRecord["msnfp_abaremittername"];
					localContext.TracingService.Trace("Got msnfp_abaremittername.");
				}
				else
				{
					mSNFP_PaymentProcessor.AbaRemitterName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bmooriginatorid.");
				}
				if (queriedEntityRecord.Contains("msnfp_abausername") && queriedEntityRecord["msnfp_abausername"] != null)
				{
					mSNFP_PaymentProcessor.AbaUserName = (string)queriedEntityRecord["msnfp_abausername"];
					localContext.TracingService.Trace("Got msnfp_abausername.");
				}
				else
				{
					mSNFP_PaymentProcessor.AbaUserName = null;
					localContext.TracingService.Trace("Did NOT find msnfp_abausername.");
				}
				if (queriedEntityRecord.Contains("msnfp_abausernumber") && queriedEntityRecord["msnfp_abausernumber"] != null)
				{
					mSNFP_PaymentProcessor.AbaUserNumber = (string)queriedEntityRecord["msnfp_abausernumber"];
					localContext.TracingService.Trace("Got msnfp_abausernumber.");
				}
				else
				{
					mSNFP_PaymentProcessor.AbaUserNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_abausernumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_iatsagentcode") && queriedEntityRecord["msnfp_iatsagentcode"] != null)
				{
					mSNFP_PaymentProcessor.IatsAgentCode = (string)queriedEntityRecord["msnfp_iatsagentcode"];
					localContext.TracingService.Trace("Got msnfp_iatsagentcode.");
				}
				else
				{
					mSNFP_PaymentProcessor.IatsAgentCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_iatsagentcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_iatspassword") && queriedEntityRecord["msnfp_iatspassword"] != null)
				{
					mSNFP_PaymentProcessor.IatsPassword = (string)queriedEntityRecord["msnfp_iatspassword"];
					localContext.TracingService.Trace("Got msnfp_iatspassword.");
				}
				else
				{
					mSNFP_PaymentProcessor.IatsPassword = null;
					localContext.TracingService.Trace("Did NOT find msnfp_iatspassword.");
				}
				if (messageName == "Create")
				{
					mSNFP_PaymentProcessor.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_PaymentProcessor.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_PaymentProcessor.CreatedOn = null;
				}
				mSNFP_PaymentProcessor.Updated = null;
				mSNFP_PaymentProcessor.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_PaymentProcessor.Deleted = true;
					mSNFP_PaymentProcessor.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_PaymentProcessor.Deleted = false;
					mSNFP_PaymentProcessor.DeletedDate = null;
				}
				mSNFP_PaymentProcessor.Configuration = new HashSet<MSNFP_Configuration>();
				mSNFP_PaymentProcessor.PaymentMethod = new HashSet<MSNFP_PaymentMethod>();
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_PaymentProcessor));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_PaymentProcessor);
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
