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
	public class MembershipOrderCreate : PluginBase
	{
		public MembershipOrderCreate(string unsecure, string secure)
			: base(typeof(MembershipOrderCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered MembershipOrderCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering MembershipOrderCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_membershiporder", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_membershiporder", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting MembershipOrderCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_membershiporderid", "msnfp_identifier", "msnfp_frommembershipid", "msnfp_order", "msnfp_orderdate", "msnfp_tomembershipgroupid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "MembershipOrder";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_membershiporderid"].ToString());
				MSNFP_MembershipOrder mSNFP_MembershipOrder = new MSNFP_MembershipOrder();
				mSNFP_MembershipOrder.MembershipOrderId = (Guid)queriedEntityRecord["msnfp_membershiporderid"];
				mSNFP_MembershipOrder.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_MembershipOrder.Identifier);
				if (queriedEntityRecord.Contains("msnfp_frommembershipid") && queriedEntityRecord["msnfp_frommembershipid"] != null)
				{
					mSNFP_MembershipOrder.FromMembershipCategoryId = ((EntityReference)queriedEntityRecord["msnfp_frommembershipid"]).Id;
					localContext.TracingService.Trace("Got msnfp_frommembershipid.");
				}
				else
				{
					mSNFP_MembershipOrder.FromMembershipCategoryId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_frommembershipid.");
				}
				if (queriedEntityRecord.Contains("msnfp_order") && queriedEntityRecord["msnfp_order"] != null)
				{
					mSNFP_MembershipOrder.Order = (int)queriedEntityRecord["msnfp_order"];
					localContext.TracingService.Trace("Got msnfp_order.");
				}
				else
				{
					mSNFP_MembershipOrder.Order = null;
					localContext.TracingService.Trace("Did NOT find msnfp_order.");
				}
				if (queriedEntityRecord.Contains("msnfp_orderdate") && queriedEntityRecord["msnfp_orderdate"] != null)
				{
					mSNFP_MembershipOrder.OrderDate = (DateTime)queriedEntityRecord["msnfp_orderdate"];
					localContext.TracingService.Trace("Got msnfp_orderdate.");
				}
				else
				{
					mSNFP_MembershipOrder.OrderDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_orderdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_tomembershipgroupid") && queriedEntityRecord["msnfp_tomembershipgroupid"] != null)
				{
					mSNFP_MembershipOrder.ToMembershipGroupId = ((EntityReference)queriedEntityRecord["msnfp_tomembershipgroupid"]).Id;
					localContext.TracingService.Trace("Got msnfp_tomembershipgroupid.");
				}
				else
				{
					mSNFP_MembershipOrder.ToMembershipGroupId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tomembershipgroupid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_MembershipOrder.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_MembershipOrder.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_MembershipOrder.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_MembershipOrder.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_MembershipOrder.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_MembershipOrder.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_MembershipOrder.CreatedOn = null;
				}
				mSNFP_MembershipOrder.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_MembershipOrder.Deleted = true;
					mSNFP_MembershipOrder.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_MembershipOrder.Deleted = false;
					mSNFP_MembershipOrder.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_MembershipOrder));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_MembershipOrder);
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
