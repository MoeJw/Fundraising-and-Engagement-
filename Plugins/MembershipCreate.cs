using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	public class MembershipCreate : PluginBase
	{
		public MembershipCreate(string unsecure, string secure)
			: base(typeof(MembershipCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered MembershipCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			Entity entity = null;
			string messageName = pluginExecutionContext.MessageName;
			Entity entity2 = null;
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity3 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity3 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			entity2 = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering MembershipCreate.cs Main Function---------");
				Entity entity4 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					entity = organizationService.Retrieve("msnfp_membership", entity4.Id, GetColumnSet());
				}
				if (entity4 != null)
				{
					if (messageName == "Create")
					{
						AddOrUpdateThisRecordWithAzure(entity4, entity2, localContext, organizationService, pluginExecutionContext);
						UpdatePrimaryMembership(entity4, organizationService, localContext.TracingService);
					}
					else if (messageName == "Update")
					{
						AddOrUpdateThisRecordWithAzure(entity, entity2, localContext, organizationService, pluginExecutionContext);
						UpdatePrimaryMembership(entity, organizationService, localContext.TracingService);
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting plugin.");
				}
			}
			if (messageName == "Delete")
			{
				entity = organizationService.Retrieve("msnfp_membership", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(entity, entity2, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting MembershipCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_membershipid", "msnfp_name", "msnfp_customer", "msnfp_startdate", "msnfp_enddate", "msnfp_membershipcategoryid", "msnfp_primary", "statecode", "statuscode", "createdon");
		}

		private void UpdatePrimaryMembership(Entity target, IOrganizationService service, ITracingService tracer)
		{
			EntityReference msnfp_customer = target.GetAttributeValue<EntityReference>("msnfp_customer");
			if (msnfp_customer == null || target.GetAttributeValue<bool?>("msnfp_primary") != true)
			{
				return;
			}
			using OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(service);
			Entity entity = new Entity
			{
				LogicalName = msnfp_customer.LogicalName,
				Id = msnfp_customer.Id
			};
			entity["msnfp_primarymembershipid"] = target.ToEntityReference();
			service.Update(entity);
			IQueryable<Guid> queryable = from m in organizationServiceContext.CreateQuery("msnfp_membership")
				where (byte)m["msnfp_primary"] != 0 == true && (Guid)m["msnfp_membershipid"] != target.Id && (EntityReference)m["msnfp_customer"] != null && (EntityReference)m["msnfp_customer"] == msnfp_customer
				select m.Id;
			foreach (Guid item in queryable)
			{
				Entity entity2 = new Entity("msnfp_membership", item);
				entity2["msnfp_primary"] = false;
				service.Update(entity2);
			}
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Membership";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_membershipid"].ToString());
				MSNFP_Membership mSNFP_Membership = new MSNFP_Membership();
				mSNFP_Membership.MembershipId = (Guid)queriedEntityRecord["msnfp_membershipid"];
				mSNFP_Membership.Name = (queriedEntityRecord.Contains("msnfp_name") ? ((string)queriedEntityRecord["msnfp_name"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_Membership.Name);
				if (queriedEntityRecord.Contains("msnfp_customer") && queriedEntityRecord["msnfp_customer"] != null)
				{
					mSNFP_Membership.Customer = ((EntityReference)queriedEntityRecord["msnfp_customer"]).Id;
					localContext.TracingService.Trace("Got msnfp_customer.");
				}
				else
				{
					mSNFP_Membership.Customer = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customer.");
				}
				if (queriedEntityRecord.Contains("msnfp_startdate") && queriedEntityRecord["msnfp_startdate"] != null)
				{
					mSNFP_Membership.StartDate = (DateTime)queriedEntityRecord["msnfp_startdate"];
					localContext.TracingService.Trace("Got msnfp_startdate.");
				}
				else
				{
					mSNFP_Membership.StartDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_startdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_enddate") && queriedEntityRecord["msnfp_enddate"] != null)
				{
					mSNFP_Membership.EndDate = (DateTime)queriedEntityRecord["msnfp_enddate"];
					localContext.TracingService.Trace("Got msnfp_enddate.");
				}
				else
				{
					mSNFP_Membership.EndDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_enddate.");
				}
				if (queriedEntityRecord.Contains("msnfp_membershipcategoryid") && queriedEntityRecord["msnfp_membershipcategoryid"] != null)
				{
					mSNFP_Membership.MembershipCategoryId = ((EntityReference)queriedEntityRecord["msnfp_membershipcategoryid"]).Id;
					localContext.TracingService.Trace("Got msnfp_membershipcategoryid.");
				}
				else
				{
					mSNFP_Membership.MembershipCategoryId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_membershipcategoryid.");
				}
				if (queriedEntityRecord.Contains("msnfp_primary") && queriedEntityRecord["msnfp_primary"] != null)
				{
					mSNFP_Membership.Primary = (bool)queriedEntityRecord["msnfp_primary"];
					localContext.TracingService.Trace("Got msnfp_primary.");
				}
				else
				{
					mSNFP_Membership.Primary = null;
					localContext.TracingService.Trace("Did NOT find msnfp_primary.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Membership.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Membership.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Membership.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Membership.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Membership.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Membership.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Membership.CreatedOn = null;
				}
				mSNFP_Membership.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Membership.Deleted = true;
					mSNFP_Membership.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Membership.Deleted = false;
					mSNFP_Membership.DeletedDate = null;
				}
				mSNFP_Membership.MembershipCategory = null;
				mSNFP_Membership.MembershipOrder = new HashSet<MSNFP_MembershipOrder>();
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Membership));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Membership);
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
