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
	public class MembershipCategoryCreate : PluginBase
	{
		public MembershipCategoryCreate(string unsecure, string secure)
			: base(typeof(MembershipCategoryCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered MembershipCategoryCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering MembershipCategoryCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_membershipcategory", entity3.Id, GetColumnSet());
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
					localContext.TracingService.Trace("Target record not found. Exiting plugin.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_membershipcategory", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting MembershipCategoryCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_membershipcategoryid", "msnfp_name", "msnfp_amount_membership", "msnfp_amount_tax", "msnfp_amount", "msnfp_goodwilldate", "msnfp_membershipduration", "msnfp_renewaldate", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "MembershipCategory";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_membershipcategoryid"].ToString());
				MSNFP_MembershipCategory mSNFP_MembershipCategory = new MSNFP_MembershipCategory();
				mSNFP_MembershipCategory.MembershipCategoryId = (Guid)queriedEntityRecord["msnfp_membershipcategoryid"];
				mSNFP_MembershipCategory.Name = (queriedEntityRecord.Contains("msnfp_name") ? ((string)queriedEntityRecord["msnfp_name"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_MembershipCategory.Name);
				if (queriedEntityRecord.Contains("msnfp_amount_membership") && queriedEntityRecord["msnfp_amount_membership"] != null)
				{
					mSNFP_MembershipCategory.AmountMembership = ((Money)queriedEntityRecord["msnfp_amount_membership"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_membership.");
				}
				else
				{
					mSNFP_MembershipCategory.AmountMembership = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_membership.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_MembershipCategory.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_MembershipCategory.AmountTax = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_MembershipCategory.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_MembershipCategory.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_goodwilldate") && queriedEntityRecord["msnfp_goodwilldate"] != null)
				{
					mSNFP_MembershipCategory.GoodWillDate = (DateTime)queriedEntityRecord["msnfp_goodwilldate"];
					localContext.TracingService.Trace("Got msnfp_goodwilldate.");
				}
				else
				{
					mSNFP_MembershipCategory.GoodWillDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_goodwilldate.");
				}
				if (queriedEntityRecord.Contains("msnfp_membershipduration") && queriedEntityRecord["msnfp_membershipduration"] != null)
				{
					mSNFP_MembershipCategory.MembershipDuration = ((OptionSetValue)queriedEntityRecord["msnfp_membershipduration"]).Value;
					localContext.TracingService.Trace("Got msnfp_membershipduration.");
				}
				else
				{
					mSNFP_MembershipCategory.MembershipDuration = null;
					localContext.TracingService.Trace("Did NOT find msnfp_membershipduration.");
				}
				if (queriedEntityRecord.Contains("msnfp_renewaldate") && queriedEntityRecord["msnfp_renewaldate"] != null)
				{
					mSNFP_MembershipCategory.RenewalDate = (DateTime)queriedEntityRecord["msnfp_renewaldate"];
					localContext.TracingService.Trace("Got msnfp_renewaldate.");
				}
				else
				{
					mSNFP_MembershipCategory.RenewalDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_renewaldate.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_MembershipCategory.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_MembershipCategory.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_MembershipCategory.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_MembershipCategory.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_MembershipCategory.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_MembershipCategory.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_MembershipCategory.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_MembershipCategory.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_MembershipCategory.CreatedOn = null;
				}
				mSNFP_MembershipCategory.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_MembershipCategory.Deleted = true;
					mSNFP_MembershipCategory.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_MembershipCategory.Deleted = false;
					mSNFP_MembershipCategory.DeletedDate = null;
				}
				mSNFP_MembershipCategory.Membership = new HashSet<MSNFP_Membership>();
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_MembershipCategory));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_MembershipCategory);
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
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting plugin.");
			}
		}
	}
}
