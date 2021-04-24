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
using Plugins.Common;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class DonorCommitmentCreate : PluginBase
	{
		private ITracingService tracingService;

		public DonorCommitmentCreate(string unsecure, string secure)
			: base(typeof(DonorCommitmentCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered DonorCommitmentCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			string empty = string.Empty;
			Guid empty2 = Guid.Empty;
			string messageName = pluginExecutionContext.MessageName;
			Entity entity = null;
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			Entity configurationRecordByUser = Plugins.PaymentProcesses.Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			tracingService = localContext.TracingService;
			ColumnSet columnSet = new ColumnSet("msnfp_customerid", "msnfp_totalamount", "msnfp_totalamount_paid", "msnfp_totalamount_balance", "msnfp_parentscheduleid", "msnfp_appealid", "msnfp_packageid", "msnfp_commitment_campaignid", "statecode", "statuscode", "msnfp_bookdate");
			localContext.TracingService.Trace("---------Entering DonorCommitmentCreate.cs Main Function---------");
			localContext.TracingService.Trace("Message Name: " + messageName);
			Plugins.PaymentProcesses.Utilities utilities = new Plugins.PaymentProcesses.Utilities();
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				Entity targetRecord = (Entity)pluginExecutionContext.InputParameters["Target"];
				entity = organizationService.Retrieve("msnfp_donorcommitment", targetRecord.Id, columnSet);
				if (messageName == "Create")
				{
					if (targetRecord.Contains("msnfp_name") && targetRecord["msnfp_name"] != null)
					{
						localContext.TracingService.Trace("Pledge's title: " + (string)targetRecord["msnfp_name"]);
						if (targetRecord.Contains("msnfp_commitment_campaignid"))
						{
							Entity entity2 = organizationService.Retrieve("campaign", ((EntityReference)targetRecord["msnfp_commitment_campaignid"]).Id, new ColumnSet("name"));
							if (entity2.Contains("name"))
							{
								string str = (string)entity2["name"];
								Entity entity3 = organizationService.Retrieve(targetRecord.LogicalName, targetRecord.Id, new ColumnSet("msnfp_name"));
								if (entity3.Contains("msnfp_name") && entity3["msnfp_name"] != null)
								{
									entity3["msnfp_name"] = entity3["msnfp_name"]?.ToString() + " - " + str;
									organizationService.Update(entity3);
								}
							}
						}
					}
					if (targetRecord.Contains("msnfp_customerid"))
					{
						localContext.TracingService.Trace("See if this donor has any pledge matches.");
						List<Entity> list = (from c in organizationServiceContext.CreateQuery("msnfp_pledgematch")
							where ((EntityReference)c["msnfp_customerfromid"]).Id == ((EntityReference)targetRecord["msnfp_customerid"]).Id
							select c).ToList();
						localContext.TracingService.Trace("Pledge Matches found: " + list.Count);
						if (list != null)
						{
							foreach (Entity item in list)
							{
								localContext.TracingService.Trace("Pledge Match ID to process next: " + item.Id.ToString());
								AddPledgeFromPledgeMatchRecord(targetRecord, item, localContext, organizationService, pluginExecutionContext);
							}
						}
					}
					if (targetRecord.Contains("msnfp_commitment_defaultdesignationid"))
					{
						CreateDesignationRecords(localContext, organizationService, targetRecord);
					}
					localContext.TracingService.Trace("Before checking if the Payment Schedule is present");
					if (entity.Contains("msnfp_parentscheduleid") && entity["msnfp_parentscheduleid"] != null)
					{
						localContext.TracingService.Trace("Before updating the Payment Schedule");
						utilities.UpdatePaymentScheduleBalance(organizationServiceContext, organizationService, entity, (EntityReference)entity["msnfp_parentscheduleid"], "create");
						localContext.TracingService.Trace("After updating the Payment Schedule");
					}
					if (!string.IsNullOrEmpty(configurationRecordByUser.GetAttributeValue<string>("msnfp_apipadlocktoken")))
					{
						AddOrUpdateThisRecordWithAzure(entity, configurationRecordByUser, messageName);
					}
					Plugins.PaymentProcesses.Utilities.UpdateHouseholdOnRecord(organizationService, entity, "msnfp_householdid", "msnfp_customerid");
				}
				if (messageName == "Update")
				{
					localContext.TracingService.Trace("Before checking if the Payment Schedule is present");
					if (targetRecord.Contains("statuscode") && targetRecord["statuscode"] != null && entity.GetAttributeValue<EntityReference>("msnfp_parentscheduleid") != null && (localContext.PluginExecutionContext.Depth < 2 || (localContext.PluginExecutionContext.ParentContext != null && localContext.PluginExecutionContext.ParentContext.PrimaryEntityName.ToLower() == "msnfp_donorcommitment")))
					{
						localContext.TracingService.Trace("Before updating the Payment Schedule");
						utilities.UpdatePaymentScheduleBalance(organizationServiceContext, organizationService, entity, (EntityReference)entity["msnfp_parentscheduleid"], "update");
						localContext.TracingService.Trace("After updating the Payment Schedule");
					}
					if (!string.IsNullOrEmpty(configurationRecordByUser.GetAttributeValue<string>("msnfp_apipadlocktoken")))
					{
						AddOrUpdateThisRecordWithAzure(entity, configurationRecordByUser, messageName);
					}
					Plugins.PaymentProcesses.Utilities.UpdateHouseholdOnRecord(organizationService, targetRecord, "msnfp_householdid", "msnfp_customerid");
				}
			}
			else if (pluginExecutionContext.InputParameters["Target"] is EntityReference && string.Compare(messageName, "delete", StringComparison.CurrentCultureIgnoreCase) == 0)
			{
				EntityReference entityReference = (EntityReference)pluginExecutionContext.InputParameters["Target"];
				entity = organizationService.Retrieve("msnfp_donorcommitment", entityReference.Id, columnSet);
			}
			if (entity != null)
			{
				AddOrUpdateThisRecordWithAzure(entity, configurationRecordByUser, messageName);
				Plugins.Common.Utilities.CallYearlyGivingServiceAsync(entity.Id, entity.LogicalName, configurationRecordByUser.Id, organizationService, localContext.TracingService);
			}
			localContext.TracingService.Trace("---------Exiting DonorCommitmentCreate.cs---------");
		}

		private void AddPledgeFromPledgeMatchRecord(Entity donorCommitmentRecord, Entity pledgeMatchRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Entering AddPledgeFromPledgeMatchRecord()---------");
			if (pledgeMatchRecord.Contains("msnfp_appliestocode"))
			{
				if (((OptionSetValue)pledgeMatchRecord["msnfp_appliestocode"]).Value != 844060001)
				{
					localContext.TracingService.Trace("msnfp_appliestocode applies to transaction create, value: " + ((OptionSetValue)pledgeMatchRecord["msnfp_appliestocode"]).Value);
					int num = (pledgeMatchRecord.Contains("msnfp_percentage") ? ((int)pledgeMatchRecord["msnfp_percentage"]) : 100);
					Money money = (Money)donorCommitmentRecord["msnfp_totalamount"];
					Money money2 = new Money();
					if (money.Value == 0m || num == 0)
					{
						money2.Value = 0m;
					}
					else
					{
						money2.Value = money.Value * (decimal)num / 100m;
					}
					localContext.TracingService.Trace("Commitment amount: " + money2.Value);
					try
					{
						Entity entity = new Entity("msnfp_donorcommitment");
						if (pledgeMatchRecord.Contains("msnfp_customertoid"))
						{
							entity["msnfp_customerid"] = pledgeMatchRecord["msnfp_customertoid"];
							string logicalName = ((EntityReference)pledgeMatchRecord["msnfp_customertoid"]).LogicalName;
							Guid id = ((EntityReference)pledgeMatchRecord["msnfp_customertoid"]).Id;
							Entity entity2 = null;
							if (logicalName == "contact")
							{
								entity2 = service.Retrieve(logicalName, id, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "emailaddress1"));
								entity["msnfp_firstname"] = (entity2.Contains("firstname") ? ((string)entity2["firstname"]) : string.Empty);
								entity["msnfp_lastname"] = (entity2.Contains("lastname") ? ((string)entity2["lastname"]) : string.Empty);
							}
							else if (logicalName == "account")
							{
								entity2 = service.Retrieve(logicalName, id, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "emailaddress1"));
							}
							entity["msnfp_billing_line1"] = (entity2.Contains("address1_line1") ? ((string)entity2["address1_line1"]) : string.Empty);
							entity["msnfp_billing_line2"] = (entity2.Contains("address1_line2") ? ((string)entity2["address1_line2"]) : string.Empty);
							entity["msnfp_billing_line3"] = (entity2.Contains("address1_line3") ? ((string)entity2["address1_line3"]) : string.Empty);
							entity["msnfp_billing_city"] = (entity2.Contains("address1_city") ? ((string)entity2["address1_city"]) : string.Empty);
							entity["msnfp_billing_stateorprovince"] = (entity2.Contains("address1_stateorprovince") ? ((string)entity2["address1_stateorprovince"]) : string.Empty);
							entity["msnfp_billing_country"] = (entity2.Contains("address1_country") ? ((string)entity2["address1_country"]) : string.Empty);
							entity["msnfp_billing_postalcode"] = (entity2.Contains("address1_postalcode") ? ((string)entity2["address1_postalcode"]) : string.Empty);
							entity["msnfp_telephone1"] = (entity2.Contains("telephone1") ? ((string)entity2["telephone1"]) : string.Empty);
							entity["msnfp_emailaddress1"] = (entity2.Contains("emailaddress1") ? ((string)entity2["emailaddress1"]) : string.Empty);
						}
						entity["msnfp_customerid"] = pledgeMatchRecord["msnfp_customertoid"];
						entity["msnfp_totalamount"] = money2;
						entity["msnfp_totalamount_paid"] = new Money(0m);
						entity["msnfp_totalamount_balance"] = money2;
						entity["msnfp_bookdate"] = (donorCommitmentRecord.Contains("msnfp_bookdate") ? donorCommitmentRecord["msnfp_bookdate"] : ((object)DateTime.Today));
						entity["createdby"] = new EntityReference("systemuser", context.InitiatingUserId);
						if (donorCommitmentRecord.Contains("msnfp_commitment_campaignid"))
						{
							localContext.TracingService.Trace("Campaign ID: " + ((EntityReference)donorCommitmentRecord["msnfp_commitment_campaignid"]).Id.ToString());
							entity["msnfp_commitment_campaignid"] = new EntityReference("campaign", ((EntityReference)donorCommitmentRecord["msnfp_commitment_campaignid"]).Id);
						}
						if (donorCommitmentRecord.Contains("msnfp_appealid"))
						{
							localContext.TracingService.Trace("Appeal ID: " + ((EntityReference)donorCommitmentRecord["msnfp_appealid"]).Id.ToString());
							entity["msnfp_appealid"] = new EntityReference("msnfp_appeal", ((EntityReference)donorCommitmentRecord["msnfp_appealid"]).Id);
						}
						if (donorCommitmentRecord.Contains("msnfp_packageid"))
						{
							localContext.TracingService.Trace("Package ID: " + ((EntityReference)donorCommitmentRecord["msnfp_packageid"]).Id.ToString());
							entity["msnfp_packageid"] = new EntityReference("msnfp_package", ((EntityReference)donorCommitmentRecord["msnfp_packageid"]).Id);
						}
						if (donorCommitmentRecord.Contains("msnfp_designationid"))
						{
							localContext.TracingService.Trace("msnfp_designationid ID: " + ((EntityReference)donorCommitmentRecord["msnfp_designationid"]).Id.ToString());
							entity["msnfp_designationid"] = new EntityReference("msnfp_designation", ((EntityReference)donorCommitmentRecord["msnfp_designationid"]).Id);
						}
						if (donorCommitmentRecord.Contains("msnfp_commitment_defaultdesignationid"))
						{
							localContext.TracingService.Trace("msnfp_commitment_defaultdesignationid ID: " + ((EntityReference)donorCommitmentRecord["msnfp_commitment_defaultdesignationid"]).Id.ToString());
							entity["msnfp_commitment_defaultdesignationid"] = new EntityReference("msnfp_designation", ((EntityReference)donorCommitmentRecord["msnfp_commitment_defaultdesignationid"]).Id);
						}
						entity["statuscode"] = new OptionSetValue(1);
						if (donorCommitmentRecord.Contains("msnfp_customerid"))
						{
							if (((EntityReference)donorCommitmentRecord["msnfp_customerid"]).LogicalName == "contact")
							{
								localContext.TracingService.Trace("Constituent is a contact: " + ((EntityReference)donorCommitmentRecord["msnfp_customerid"]).Id.ToString());
								entity["msnfp_constituentid"] = new EntityReference("contact", ((EntityReference)donorCommitmentRecord["msnfp_customerid"]).Id);
							}
							else if (((EntityReference)donorCommitmentRecord["msnfp_customerid"]).LogicalName == "account")
							{
								localContext.TracingService.Trace("Constituent is an account: " + ((EntityReference)donorCommitmentRecord["msnfp_customerid"]).Id.ToString());
								entity["msnfp_constituentid"] = new EntityReference("account", ((EntityReference)donorCommitmentRecord["msnfp_customerid"]).Id);
							}
						}
						if (donorCommitmentRecord.Contains("msnfp_configurationid"))
						{
							localContext.TracingService.Trace("Configuration record id: " + ((EntityReference)donorCommitmentRecord["msnfp_configurationid"]).Id.ToString());
							entity["msnfp_configurationid"] = new EntityReference("msnfp_configuration", ((EntityReference)donorCommitmentRecord["msnfp_configurationid"]).Id);
						}
						localContext.TracingService.Trace("Creating new donor commitment.");
						service.Create(entity);
						localContext.TracingService.Trace("New donor commitment created.");
					}
					catch (Exception ex)
					{
						localContext.TracingService.Trace("Error creating pledge match donor commitment: " + ex.Message);
					}
				}
				else
				{
					localContext.TracingService.Trace("This pledge match is not for Transactions (844060001), msnfp_appliestocode: " + ((OptionSetValue)pledgeMatchRecord["msnfp_appliestocode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("No msnfp_appliestocode value found.");
			}
			localContext.TracingService.Trace("---------Exiting AddPledgeFromPledgeMatchRecord()---------");
		}

		private static void CreateDesignationRecords(LocalPluginContext localContext, IOrganizationService service, Entity commitment)
		{
			localContext.TracingService.Trace("Creating Designated Credit");
			EntityReference attributeValue = commitment.GetAttributeValue<EntityReference>("msnfp_commitment_defaultdesignationid");
			Entity entity = service.Retrieve(attributeValue.LogicalName, attributeValue.Id, new ColumnSet("msnfp_name"));
			Money attributeValue2 = commitment.GetAttributeValue<Money>("msnfp_totalamount");
			DateTime attributeValue3 = commitment.GetAttributeValue<DateTime>("msnfp_bookdate");
			DateTime attributeValue4 = commitment.GetAttributeValue<DateTime>("msnfp_receiveddate");
			Entity entity2 = new Entity("msnfp_designatedcredit");
			entity2["msnfp_donorcommitmentid"] = commitment.ToEntityReference();
			entity2["msnfp_designatiedcredit_designationid"] = attributeValue;
			if (attributeValue2 != null && attributeValue2.Value > 0m)
			{
				entity2["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_name") + "-$" + attributeValue2.Value;
				entity2["msnfp_amount"] = attributeValue2;
			}
			else
			{
				entity2["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_name");
			}
			if (attributeValue3 != default(DateTime))
			{
				entity2["msnfp_bookdate"] = attributeValue3;
			}
			if (attributeValue4 != default(DateTime))
			{
				entity2["msnfp_receiveddate"] = attributeValue4;
			}
			service.Create(entity2);
			localContext.TracingService.Trace("Created Designated Credit");
			localContext.TracingService.Trace("Created Designation Plan");
			Entity entity3 = new Entity("msnfp_designationplan");
			entity3["msnfp_designationplan_campaignid"] = commitment.GetAttributeValue<EntityReference>("msnfp_commitment_campaignid");
			entity3["msnfp_customerid"] = commitment["msnfp_customerid"];
			entity3["msnfp_designationplan_donorcommitmentid"] = commitment.ToEntityReference();
			entity3["msnfp_designationplan_designationid"] = attributeValue;
			if (attributeValue2.Value > 0m)
			{
				entity3["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_name") + "-$" + attributeValue2.Value;
				entity3["msnfp_amountofpledgemax"] = attributeValue2;
			}
			else
			{
				entity3["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_name");
			}
			if (attributeValue3 != default(DateTime))
			{
				entity3["msnfp_bookdate"] = attributeValue3;
			}
			entity3["msnfp_designationplan_paymentscheduleid"] = commitment.GetAttributeValue<EntityReference>("msnfp_parentscheduleid");
			service.Create(entity3);
			localContext.TracingService.Trace("Created Designation Plan");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, string messageName)
		{
			tracingService.Trace("---------Send the Record to Azure---------");
			string text = "DonorCommitment";
			string text2 = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			tracingService.Trace("Got API URL: " + text2);
			if (!string.IsNullOrEmpty(text2))
			{
				tracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord.Id.ToString());
				MSNFP_DonorCommitment mSNFP_DonorCommitment = new MSNFP_DonorCommitment();
				mSNFP_DonorCommitment.DonorCommitmentId = queriedEntityRecord.Id;
				if (queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_appealid") != null)
				{
					mSNFP_DonorCommitment.AppealId = queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_appealid").Id;
					tracingService.Trace("Got msnfp_appealid.");
				}
				else
				{
					mSNFP_DonorCommitment.AppealId = null;
					tracingService.Trace("Did NOT find msnfp_appealid.");
				}
				if (queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_packageid") != null)
				{
					mSNFP_DonorCommitment.PackageId = queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_packageid").Id;
					tracingService.Trace("Got msnfp_packageid.");
				}
				else
				{
					mSNFP_DonorCommitment.PackageId = null;
					tracingService.Trace("Did NOT find msnfp_packageid.");
				}
				if (queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_commitment_campaignid") != null)
				{
					mSNFP_DonorCommitment.CampaignId = queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_commitment_campaignid").Id;
					tracingService.Trace("Got msnfp_commitment_campaignid.");
				}
				else
				{
					mSNFP_DonorCommitment.CampaignId = null;
					tracingService.Trace("Did NOT find msnfp_commitment_campaignid.");
				}
				if (queriedEntityRecord.GetAttributeValue<Money>("msnfp_totalamount") != null)
				{
					mSNFP_DonorCommitment.TotalAmount = queriedEntityRecord.GetAttributeValue<Money>("msnfp_totalamount").Value;
					tracingService.Trace("Got msnfp_totalamount.");
				}
				else
				{
					mSNFP_DonorCommitment.TotalAmount = null;
					tracingService.Trace("Did NOT find msnfp_totalamount.");
				}
				if (queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_customerid") != null)
				{
					mSNFP_DonorCommitment.CustomerId = queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_customerid").Id;
					if (queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_customerid").LogicalName == "contact")
					{
						mSNFP_DonorCommitment.CustomerIdType = 2;
					}
					else if (queriedEntityRecord.GetAttributeValue<EntityReference>("msnfp_customerid").LogicalName == "account")
					{
						mSNFP_DonorCommitment.CustomerIdType = 1;
					}
					tracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_DonorCommitment.CustomerId = null;
					mSNFP_DonorCommitment.CustomerIdType = null;
					tracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.GetAttributeValue<DateTime?>("msnfp_bookdate").HasValue)
				{
					mSNFP_DonorCommitment.BookDate = queriedEntityRecord.GetAttributeValue<DateTime>("msnfp_bookdate");
					tracingService.Trace("Got msnfp_bookdate.");
				}
				else
				{
					mSNFP_DonorCommitment.BookDate = null;
					tracingService.Trace("Did NOT find msnfp_bookdate.");
				}
				if (queriedEntityRecord.GetAttributeValue<Money>("msnfp_totalamount_balance") != null)
				{
					mSNFP_DonorCommitment.TotalAmountBalance = queriedEntityRecord.GetAttributeValue<Money>("msnfp_totalamount_balance").Value;
					tracingService.Trace("Got msnfp_totalamount_balance.");
				}
				else
				{
					mSNFP_DonorCommitment.TotalAmountBalance = null;
					tracingService.Trace("Did NOT find msnfp_totalamount_balance.");
				}
				if (messageName == "Create")
				{
					mSNFP_DonorCommitment.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_DonorCommitment.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				mSNFP_DonorCommitment.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_DonorCommitment.Deleted = true;
					mSNFP_DonorCommitment.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_DonorCommitment.Deleted = false;
					mSNFP_DonorCommitment.DeletedDate = null;
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_DonorCommitment.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					tracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_DonorCommitment.StateCode = null;
					tracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_DonorCommitment.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					tracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_DonorCommitment.StatusCode = null;
					tracingService.Trace("Did NOT find statuscode.");
				}
				tracingService.Trace("JSON object created");
				if (messageName == "Create")
				{
					text2 = text2 + text + "/Create" + text;
				}
				else if (messageName == "Update" || messageName == "Delete")
				{
					text2 = text2 + text + "/Update" + text;
				}
				MemoryStream memoryStream = new MemoryStream();
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_DonorCommitment));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_DonorCommitment);
				byte[] array = memoryStream.ToArray();
				memoryStream.Close();
				string @string = Encoding.UTF8.GetString(array, 0, array.Length);
				WebAPIClient webAPIClient = new WebAPIClient();
				webAPIClient.Headers[HttpRequestHeader.ContentType] = "application/json";
				webAPIClient.Headers["Padlock"] = (string)configurationRecord["msnfp_apipadlocktoken"];
				webAPIClient.Encoding = Encoding.UTF8;
				tracingService.Trace("---------Preparing JSON---------");
				tracingService.Trace("Converted to json API URL : " + text2);
				tracingService.Trace("JSON: " + @string);
				tracingService.Trace("---------End of Preparing JSON---------");
				tracingService.Trace("Sending data to Azure.");
				string text3 = webAPIClient.UploadString(text2, @string);
				tracingService.Trace("Got response.");
				tracingService.Trace("Response: " + text3);
				Plugins.PaymentProcesses.Utilities utilities = new Plugins.PaymentProcesses.Utilities();
				utilities.CheckAPIReturnJSONForErrors(text3, configurationRecord.GetAttributeValue<OptionSetValue>("msnfp_showapierrorresponses"), tracingService);
			}
			else
			{
				tracingService.Trace("No API URL or Enable Portal Pages. Exiting workflow.");
			}
		}
	}
}
