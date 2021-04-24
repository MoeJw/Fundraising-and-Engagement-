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
	public class GiftAidDeclarationCreate : PluginBase
	{
		private const string PostImageAlias = "msnfp_giftaiddeclaration";

		public GiftAidDeclarationCreate(string unsecure, string secure)
			: base(typeof(GiftAidDeclarationCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered GiftAidDeclarationCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering GiftAidDeclarationCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_giftaiddeclaration", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_giftaiddeclaration", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting GiftAidDeclarationCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_giftaiddeclarationid", "msnfp_customerid", "msnfp_declarationdate", "msnfp_declarationdelivered", "msnfp_giftaiddeclarationhtml", "msnfp_identifier", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "GiftAidDeclaration";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_giftaiddeclarationid"].ToString());
				MSNFP_GiftAidDeclaration mSNFP_GiftAidDeclaration = new MSNFP_GiftAidDeclaration();
				mSNFP_GiftAidDeclaration.GiftAidDeclarationId = (Guid)queriedEntityRecord["msnfp_giftaiddeclarationid"];
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_GiftAidDeclaration.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_GiftAidDeclaration.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_GiftAidDeclaration.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.CustomerId = null;
					mSNFP_GiftAidDeclaration.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_declarationdate") && queriedEntityRecord["msnfp_declarationdate"] != null)
				{
					mSNFP_GiftAidDeclaration.DeclarationDate = (DateTime)queriedEntityRecord["msnfp_declarationdate"];
					localContext.TracingService.Trace("Got msnfp_declarationdate.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.DeclarationDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_declarationdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_declarationdelivered") && queriedEntityRecord["msnfp_declarationdelivered"] != null)
				{
					mSNFP_GiftAidDeclaration.DeclarationDelivered = ((OptionSetValue)queriedEntityRecord["msnfp_declarationdelivered"]).Value;
					localContext.TracingService.Trace("Got msnfp_declarationdelivered.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.DeclarationDelivered = null;
					localContext.TracingService.Trace("Did NOT find msnfp_declarationdelivered.");
				}
				if (queriedEntityRecord.Contains("msnfp_giftaiddeclarationhtml") && queriedEntityRecord["msnfp_giftaiddeclarationhtml"] != null)
				{
					mSNFP_GiftAidDeclaration.GiftAidDeclarationHtml = (string)queriedEntityRecord["msnfp_giftaiddeclarationhtml"];
					localContext.TracingService.Trace("Got msnfp_giftaiddeclarationhtml.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.GiftAidDeclarationHtml = null;
					localContext.TracingService.Trace("Did NOT find msnfp_giftaiddeclarationhtml.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_GiftAidDeclaration.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.Identifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_GiftAidDeclaration.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_GiftAidDeclaration.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_GiftAidDeclaration.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_GiftAidDeclaration.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_GiftAidDeclaration.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_GiftAidDeclaration.CreatedOn = null;
				}
				mSNFP_GiftAidDeclaration.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_GiftAidDeclaration.Deleted = true;
					mSNFP_GiftAidDeclaration.DeletedDate = DateTime.UtcNow;
					localContext.TracingService.Trace("Setting Deleted Date to:" + mSNFP_GiftAidDeclaration.DeletedDate.ToString());
				}
				else
				{
					mSNFP_GiftAidDeclaration.Deleted = false;
					mSNFP_GiftAidDeclaration.DeletedDate = null;
				}
				if (messageName == "Create")
				{
					mSNFP_GiftAidDeclaration.Updated = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("msnfp_updated") && queriedEntityRecord["msnfp_updated"] != null)
				{
					mSNFP_GiftAidDeclaration.Updated = (DateTime)queriedEntityRecord["msnfp_updated"];
				}
				else
				{
					mSNFP_GiftAidDeclaration.Updated = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_GiftAidDeclaration));
				localContext.TracingService.Trace("Attempt to create JSON via serialization.");
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_GiftAidDeclaration);
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
