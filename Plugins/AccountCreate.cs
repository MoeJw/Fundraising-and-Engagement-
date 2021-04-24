using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Permissions;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.AzureModels;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class AccountCreate : PluginBase
	{
		public ITracingService tracer
		{
			get;
			set;
		}

		public AccountCreate(string unsecure, string secure)
			: base(typeof(AccountCreate))
		{
		}
		public static void DemandSecurityPermissions()
		{
			Console.WriteLine("\nExecuting DemandSecurityPermissions.\n");
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.Assertion);
			
				Console.WriteLine("Demanding SecurityPermissionFlag.Assertion");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.Assertion succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.Assertion failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.ControlAppDomain);
				Console.WriteLine("Demanding SecurityPermissionFlag.ControlAppDomain");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlAppDomain succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlAppDomain failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.ControlDomainPolicy);
				Console.WriteLine("Demanding SecurityPermissionFlag.ControlDomainPolicy");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlDomainPolicy succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlDomainPolicy failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
				Console.WriteLine("Demanding SecurityPermissionFlag.ControlEvidence");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlEvidence succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlEvidence failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.ControlPolicy);
				Console.WriteLine("Demanding SecurityPermissionFlag.ControlPolicy");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlPolicy succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlPolicy failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.ControlPrincipal);
				Console.WriteLine("Demanding SecurityPermissionFlag.ControlPrincipal");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlPrincipal succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlPrincipal failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.ControlThread);
				Console.WriteLine("Demanding SecurityPermissionFlag.ControlThread");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlThread succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.ControlThread failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.Execution);
				Console.WriteLine("Demanding SecurityPermissionFlag.Execution");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.Execution succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.Execution failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.Infrastructure);
				Console.WriteLine("Demanding SecurityPermissionFlag.Infrastructure");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.Infrastructure succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.Infrastructure failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration);
				Console.WriteLine("Demanding SecurityPermissionFlag.RemotingConfiguration");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.RemotingConfiguration succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.RemotingConfiguration failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
				Console.WriteLine("Demanding SecurityPermissionFlag.SerializationFormatter");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.SerializationFormatter succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.SerializationFormatter failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.SkipVerification);
				Console.WriteLine("Demanding SecurityPermissionFlag.SkipVerification");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.SkipVerification succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.SkipVerification failed: " + e.Message);
			}
			try
			{
				SecurityPermission sp =
					new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
				Console.WriteLine("Demanding SecurityPermissionFlag.UnmanagedCode");
				sp.Demand();
				Console.WriteLine("Demand for SecurityPermissionFlag.UnmanagedCode succeeded.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Demand for SecurityPermissionFlag.UnmanagedCode failed: " + e.Message);
			}
		}
		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			tracer = localContext.TracingService;
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered AccountCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			if (string.Compare(pluginExecutionContext.MessageName, "create", StringComparison.CurrentCultureIgnoreCase) != 0 && pluginExecutionContext.Depth > 1 && !CheckExecutionPipeLine(pluginExecutionContext))
			{
				localContext.TracingService.Trace("Context.depth > 1 => Exiting Plugin. context.Depth: " + pluginExecutionContext.Depth);
				if (pluginExecutionContext.ParentContext != null)
				{
					localContext.TracingService.Trace("Parent context " + pluginExecutionContext.ParentContext.PrimaryEntityName);
				}
				if (pluginExecutionContext.ParentContext != null && pluginExecutionContext.ParentContext.ParentContext != null)
				{
					localContext.TracingService.Trace("Parent context (Parent) " + pluginExecutionContext.ParentContext.ParentContext.PrimaryEntityName);
				}
				return;
			}
			string messageName = pluginExecutionContext.MessageName;
			if (messageName != "Create" && messageName != "Update" && messageName != "Delete")
			{
				throw new InvalidPluginExecutionException("The plugin was triggered NOT in CREATE NOR UPDATE NOR DELETE mode. Exiting plugin.");
			}
			Entity configurationRecordByUser = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			Entity entity = null;
			if (pluginExecutionContext.InputParameters.Contains("Target"))
			{
				if (pluginExecutionContext.InputParameters["Target"] is Entity)
				{
					entity = (Entity)pluginExecutionContext.InputParameters["Target"];
					if (entity == null)
					{
						throw new InvalidPluginExecutionException("'Target' is null. Exiting plugin.");
					}
					localContext.TracingService.Trace("Target entity name: " + entity.LogicalName);
					if (entity.LogicalName.ToLower() != "account")
					{
						throw new InvalidPluginExecutionException("The target entity is NOT ACCOUNT. Exiting plugin.");
					}
				}
				else
				{
					if (!(messageName == "Delete"))
					{
						throw new InvalidPluginExecutionException("The Target is NOT an Entity. Exiting plugin.");
					}
					Entity queriedEntityRecord = organizationService.Retrieve("account", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetAccountFields());
					if (configurationRecordByUser.Contains("msnfp_apipadlocktoken") && configurationRecordByUser["msnfp_apipadlocktoken"] != null)
					{
						AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
					}
				}
				if (messageName != "Delete")
				{
					Entity entity2 = organizationService.Retrieve(entity.LogicalName, entity.Id, GetAccountFields());
					if (entity2.GetAttributeValue<OptionSetValue>("msnfp_accounttype") != null && entity2.GetAttributeValue<OptionSetValue>("msnfp_accounttype").Value == 844060000)
					{
						if (messageName.ToLower() == "create")
						{
							string value = Utilities.GenerateHouseHoldSequence(organizationService, entity2, configurationRecordByUser);
							if (!string.IsNullOrEmpty(value))
							{
								entity["name"] = value;
							}
						}
						UpdateHouseholdPrimaryMembers(organizationService, entity2.Id, entity2.GetAttributeValue<EntityReference>("primarycontactid"));
					}
					if (configurationRecordByUser.Contains("msnfp_apipadlocktoken") && configurationRecordByUser["msnfp_apipadlocktoken"] != null)
					{
						AddOrUpdateThisRecordWithAzure(entity2, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
					}
					else
					{
						localContext.TracingService.Trace("No Padlock Token found to synchronize the record to Azure. AddOrUpdateThisRecordWithAzure() failed.");
					}
				}
				localContext.TracingService.Trace("---------Exitting AccountCreate.cs---------");
				return;
			}
			throw new ArgumentNullException("Target");
		}

		private bool CheckExecutionPipeLine(IPluginExecutionContext context)
		{
			return context.ParentContext != null && context.ParentContext.ParentContext != null && (context.ParentContext.ParentContext.PrimaryEntityName == "msnfp_donationimport" || context.ParentContext.ParentContext.PrimaryEntityName == "contact");
		}

		private ColumnSet GetAccountFields()
		{
			return new ColumnSet("accountid", "address1_addressid", "address1_addresstypecode", "address1_city", "address1_country", "address1_county", "address1_latitude", "address1_line1", "address1_line2", "address1_line3", "address1_longitude", "address1_name", "address1_postalcode", "address1_postofficebox", "address1_stateorprovince", "address2_addressid", "address2_addresstypecode", "address2_city", "address2_country", "address2_county", "address2_latitude", "address2_line1", "address2_line2", "address2_line3", "address2_longitude", "address2_name", "address2_postalcode", "address2_postofficebox", "address2_stateorprovince", "donotbulkemail", "donotbulkpostalmail", "donotemail", "donotfax", "donotphone", "donotpostalmail", "emailaddress1", "emailaddress2", "emailaddress3", "masterid", "owningbusinessunit", "msnfp_anonymity", "msnfp_count_lifetimetransactions", "msnfp_givinglevelid", "msnfp_lasteventpackagedate", "msnfp_lasteventpackageid", "msnfp_lasttransactiondate", "msnfp_lasttransactionid", "msnfp_preferredlanguagecode", "msnfp_primarymembershipid", "msnfp_receiptpreferencecode", "msnfp_sum_lifetimetransactions", "msnfp_telephone1typecode", "msnfp_telephone2typecode", "msnfp_telephone3typecode", "msnfp_vip", "merged", "name", "parentaccountid", "telephone1", "telephone2", "telephone3", "websiteurl", "transactioncurrencyid", "statecode", "statuscode", "createdon", "msnfp_accounttype", "msnfp_year0_giving", "msnfp_year1_giving", "msnfp_year2_giving", "msnfp_year3_giving", "msnfp_year4_giving", "primarycontactid", "msnfp_lifetimegivingsum");
		}

		private void UpdateHouseholdPrimaryMembers(IOrganizationService service, Guid houseHoldId, EntityReference primaryContactRef)
		{
			if (primaryContactRef == null)
			{
				return;
			}
			QueryExpression queryExpression = new QueryExpression("contact");
			queryExpression.NoLock = true;
			queryExpression.ColumnSet = new ColumnSet("contactid");
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_householdid", ConditionOperator.Equal, houseHoldId));
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_householdrelationship", ConditionOperator.Equal, 844060000));
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			entityCollection.Entities.ToList().ForEach(delegate(Entity c)
			{
				Entity entity3 = new Entity("contact");
				if (c.Id != primaryContactRef.Id)
				{
					entity3["msnfp_householdrelationship"] = new OptionSetValue(844060001);
					entity3.Id = c.Id;
					service.Update(entity3);
				}
			});
			Entity entity = service.Retrieve(primaryContactRef.LogicalName, primaryContactRef.Id, new ColumnSet("msnfp_householdrelationship"));
			if (entityCollection.Entities.Any((Entity a) => a.Id != primaryContactRef.Id) || entity.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") == null || (entity.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") != null && entity.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship").Value != 844060000))
			{
				Entity entity2 = new Entity("contact");
				entity2["msnfp_householdrelationship"] = new OptionSetValue(844060000);
				entity2.Id = primaryContactRef.Id;
				service.Update(entity2);
			}
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			DemandSecurityPermissions();
			FileIOPermission f = new FileIOPermission(PermissionState.None);
			f.AllLocalFiles = FileIOPermissionAccess.Read;
			try
			{
				f.Demand();
			}
			catch (Exception s)
			{
				Console.WriteLine(s.Message);
			}
			try { 
	     	var _stt = new Sandboxer();
			
			}
			catch (Exception e) {

				Console.WriteLine("" + e);
			
			}

			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Account";
			string text2 = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["accountid"].ToString());
				Account account = new Account();
				account.AccountId = (Guid)queriedEntityRecord["accountid"];
				if (queriedEntityRecord.Contains("address1_addressid") && queriedEntityRecord["address1_addressid"] != null)
				{
					account.Address1_AddressId = (Guid)queriedEntityRecord["address1_addressid"];
					localContext.TracingService.Trace("Got address1_addressid.");
				}
				else
				{
					account.Address1_AddressId = null;
					localContext.TracingService.Trace("Did NOT find address1_addressid.");
				}
				if (queriedEntityRecord.Contains("address1_addresstypecode") && queriedEntityRecord["address1_addresstypecode"] != null)
				{
					account.Address1_AddressTypeCode = ((OptionSetValue)queriedEntityRecord["address1_addresstypecode"]).Value;
					localContext.TracingService.Trace("Got address1_addresstypecode.");
				}
				else
				{
					account.Address1_AddressTypeCode = null;
					localContext.TracingService.Trace("Did NOT find address1_addresstypecode.");
				}
				if (queriedEntityRecord.Contains("address1_city") && queriedEntityRecord["address1_city"] != null)
				{
					account.Address1_City = (string)queriedEntityRecord["address1_city"];
					localContext.TracingService.Trace("Got address1_city.");
				}
				else
				{
					account.Address1_City = null;
					localContext.TracingService.Trace("Did NOT find address1_city.");
				}
				if (queriedEntityRecord.Contains("address1_country") && queriedEntityRecord["address1_country"] != null)
				{
					account.Address1_Country = (string)queriedEntityRecord["address1_country"];
					localContext.TracingService.Trace("Got address1_country.");
				}
				else
				{
					account.Address1_Country = null;
					localContext.TracingService.Trace("Did NOT find address1_country.");
				}
				if (queriedEntityRecord.Contains("address1_county") && queriedEntityRecord["address1_county"] != null)
				{
					account.Address1_County = (string)queriedEntityRecord["address1_county"];
					localContext.TracingService.Trace("Got address1_county.");
				}
				else
				{
					account.Address1_County = null;
					localContext.TracingService.Trace("Did NOT find address1_county.");
				}
				if (queriedEntityRecord.Contains("address1_latitude") && queriedEntityRecord["address1_latitude"] != null)
				{
					account.Address1_Latitude = (float)queriedEntityRecord["address1_latitude"];
					localContext.TracingService.Trace("Got address1_latitude.");
				}
				else
				{
					account.Address1_Latitude = null;
					localContext.TracingService.Trace("Did NOT find address1_latitude.");
				}
				if (queriedEntityRecord.Contains("address1_line1") && queriedEntityRecord["address1_line1"] != null)
				{
					account.Address1_Line1 = (string)queriedEntityRecord["address1_line1"];
					localContext.TracingService.Trace("Got address1_line1.");
				}
				else
				{
					account.Address1_Line1 = null;
					localContext.TracingService.Trace("Did NOT find address1_line1.");
				}
				if (queriedEntityRecord.Contains("address1_line2") && queriedEntityRecord["address1_line2"] != null)
				{
					account.Address1_Line2 = (string)queriedEntityRecord["address1_line2"];
					localContext.TracingService.Trace("Got address1_line2.");
				}
				else
				{
					account.Address1_Line2 = null;
					localContext.TracingService.Trace("Did NOT find address1_line2.");
				}
				if (queriedEntityRecord.Contains("address1_line3") && queriedEntityRecord["address1_line3"] != null)
				{
					account.Address1_Line3 = (string)queriedEntityRecord["address1_line3"];
					localContext.TracingService.Trace("Got address1_line3.");
				}
				else
				{
					account.Address1_Line3 = null;
					localContext.TracingService.Trace("Did NOT find address1_line3.");
				}
				if (queriedEntityRecord.Contains("address1_longitude") && queriedEntityRecord["address1_longitude"] != null)
				{
					account.Address1_Longitude = (float)queriedEntityRecord["address1_longitude"];
					localContext.TracingService.Trace("Got address1_longitude.");
				}
				else
				{
					account.Address1_Longitude = null;
					localContext.TracingService.Trace("Did NOT find address1_longitude.");
				}
				if (queriedEntityRecord.Contains("address1_name") && queriedEntityRecord["address1_name"] != null)
				{
					account.Address1_Name = (string)queriedEntityRecord["address1_name"];
					localContext.TracingService.Trace("Got address1_name.");
				}
				else
				{
					account.Address1_Name = null;
					localContext.TracingService.Trace("Did NOT find address1_name.");
				}
				if (queriedEntityRecord.Contains("address1_postalcode") && queriedEntityRecord["address1_postalcode"] != null)
				{
					account.Address1_PostalCode = (string)queriedEntityRecord["address1_postalcode"];
					localContext.TracingService.Trace("Got address1_postalcode.");
				}
				else
				{
					account.Address1_PostalCode = null;
					localContext.TracingService.Trace("Did NOT find address1_postalcode.");
				}
				if (queriedEntityRecord.Contains("address1_postofficebox") && queriedEntityRecord["address1_postofficebox"] != null)
				{
					account.Address1_PostOfficeBox = (string)queriedEntityRecord["address1_postofficebox"];
					localContext.TracingService.Trace("Got address1_postofficebox.");
				}
				else
				{
					account.Address1_PostOfficeBox = null;
					localContext.TracingService.Trace("Did NOT find address1_postofficebox.");
				}
				if (queriedEntityRecord.Contains("address1_stateorprovince") && queriedEntityRecord["address1_stateorprovince"] != null)
				{
					account.Address1_StateOrProvince = (string)queriedEntityRecord["address1_stateorprovince"];
					localContext.TracingService.Trace("Got address1_stateorprovince.");
				}
				else
				{
					account.Address1_StateOrProvince = null;
					localContext.TracingService.Trace("Did NOT find address1_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("address2_addressid") && queriedEntityRecord["address2_addressid"] != null)
				{
					account.Address2_AddressId = (Guid)queriedEntityRecord["address2_addressid"];
					localContext.TracingService.Trace("Got address2_addressid.");
				}
				else
				{
					account.Address2_AddressId = null;
					localContext.TracingService.Trace("Did NOT find address1_addressid.");
				}
				if (queriedEntityRecord.Contains("address2_addresstypecode") && queriedEntityRecord["address2_addresstypecode"] != null)
				{
					account.Address2_AddressTypeCode = ((OptionSetValue)queriedEntityRecord["address2_addresstypecode"]).Value;
					localContext.TracingService.Trace("Got address2_addresstypecode.");
				}
				else
				{
					account.Address2_AddressTypeCode = null;
					localContext.TracingService.Trace("Did NOT find address2_addresstypecode.");
				}
				if (queriedEntityRecord.Contains("address2_city") && queriedEntityRecord["address2_city"] != null)
				{
					account.Address2_City = (string)queriedEntityRecord["address2_city"];
					localContext.TracingService.Trace("Got address2_city.");
				}
				else
				{
					account.Address2_City = null;
					localContext.TracingService.Trace("Did NOT find address2_city.");
				}
				if (queriedEntityRecord.Contains("address2_country") && queriedEntityRecord["address2_country"] != null)
				{
					account.Address2_Country = (string)queriedEntityRecord["address2_country"];
					localContext.TracingService.Trace("Got address2_country.");
				}
				else
				{
					account.Address2_Country = null;
					localContext.TracingService.Trace("Did NOT find address2_country.");
				}
				if (queriedEntityRecord.Contains("address2_county") && queriedEntityRecord["address2_county"] != null)
				{
					account.Address2_County = (string)queriedEntityRecord["address2_county"];
					localContext.TracingService.Trace("Got address2_county.");
				}
				else
				{
					account.Address2_County = null;
					localContext.TracingService.Trace("Did NOT find address2_county.");
				}
				if (queriedEntityRecord.Contains("address2_latitude") && queriedEntityRecord["address2_latitude"] != null)
				{
					account.Address2_Latitude = (float)queriedEntityRecord["address2_latitude"];
					localContext.TracingService.Trace("Got address2_latitude.");
				}
				else
				{
					account.Address2_Latitude = null;
					localContext.TracingService.Trace("Did NOT find address2_latitude.");
				}
				if (queriedEntityRecord.Contains("address2_line1") && queriedEntityRecord["address2_line1"] != null)
				{
					account.Address2_Line1 = (string)queriedEntityRecord["address2_line1"];
					localContext.TracingService.Trace("Got address2_line1.");
				}
				else
				{
					account.Address2_Line1 = null;
					localContext.TracingService.Trace("Did NOT find address2_line1.");
				}
				if (queriedEntityRecord.Contains("address2_line2") && queriedEntityRecord["address2_line2"] != null)
				{
					account.Address2_Line2 = (string)queriedEntityRecord["address2_line2"];
					localContext.TracingService.Trace("Got address2_line2.");
				}
				else
				{
					account.Address2_Line2 = null;
					localContext.TracingService.Trace("Did NOT find address2_line2.");
				}
				if (queriedEntityRecord.Contains("address2_line3") && queriedEntityRecord["address2_line3"] != null)
				{
					account.Address2_Line3 = (string)queriedEntityRecord["address2_line3"];
					localContext.TracingService.Trace("Got address2_line3.");
				}
				else
				{
					account.Address2_Line3 = null;
					localContext.TracingService.Trace("Did NOT find address2_line3.");
				}
				if (queriedEntityRecord.Contains("address2_longitude") && queriedEntityRecord["address2_longitude"] != null)
				{
					account.Address2_Longitude = (float)queriedEntityRecord["address2_longitude"];
					localContext.TracingService.Trace("Got address2_longitude.");
				}
				else
				{
					account.Address2_Longitude = null;
					localContext.TracingService.Trace("Did NOT find address2_longitude.");
				}
				if (queriedEntityRecord.Contains("address2_name") && queriedEntityRecord["address2_name"] != null)
				{
					account.Address2_Name = (string)queriedEntityRecord["address2_name"];
					localContext.TracingService.Trace("Got address2_name.");
				}
				else
				{
					account.Address2_Name = null;
					localContext.TracingService.Trace("Did NOT find address2_name.");
				}
				if (queriedEntityRecord.Contains("address2_postalcode") && queriedEntityRecord["address2_postalcode"] != null)
				{
					account.Address2_PostalCode = (string)queriedEntityRecord["address2_postalcode"];
					localContext.TracingService.Trace("Got address2_postalcode.");
				}
				else
				{
					account.Address2_PostalCode = null;
					localContext.TracingService.Trace("Did NOT find address2_postalcode.");
				}
				if (queriedEntityRecord.Contains("address2_postofficebox") && queriedEntityRecord["address2_postofficebox"] != null)
				{
					account.Address2_PostOfficeBox = (string)queriedEntityRecord["address2_postofficebox"];
					localContext.TracingService.Trace("Got address2_postofficebox.");
				}
				else
				{
					account.Address2_PostOfficeBox = null;
					localContext.TracingService.Trace("Did NOT find address2_postofficebox.");
				}
				if (queriedEntityRecord.Contains("address2_stateorprovince") && queriedEntityRecord["address2_stateorprovince"] != null)
				{
					account.Address2_StateOrProvince = (string)queriedEntityRecord["address2_stateorprovince"];
					localContext.TracingService.Trace("Got address2_stateorprovince.");
				}
				else
				{
					account.Address2_StateOrProvince = null;
					localContext.TracingService.Trace("Did NOT find address2_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("donotbulkemail") && queriedEntityRecord["donotbulkemail"] != null)
				{
					account.DoNotBulkEMail = (bool)queriedEntityRecord["donotbulkemail"];
					localContext.TracingService.Trace("Got donotbulkemail.");
				}
				else
				{
					account.DoNotBulkEMail = null;
					localContext.TracingService.Trace("Did NOT find donotbulkemail.");
				}
				if (queriedEntityRecord.Contains("donotbulkpostalmail") && queriedEntityRecord["donotbulkpostalmail"] != null)
				{
					account.DoNotBulkPostalMail = (bool)queriedEntityRecord["donotbulkpostalmail"];
					localContext.TracingService.Trace("Got donotbulkpostalmail.");
				}
				else
				{
					account.DoNotBulkPostalMail = null;
					localContext.TracingService.Trace("Did NOT find donotbulkpostalmail.");
				}
				if (queriedEntityRecord.Contains("donotemail") && queriedEntityRecord["donotemail"] != null)
				{
					account.DoNotEmail = (bool)queriedEntityRecord["donotemail"];
					localContext.TracingService.Trace("Got donotemail.");
				}
				else
				{
					account.DoNotEmail = null;
					localContext.TracingService.Trace("Did NOT find donotemail.");
				}
				if (queriedEntityRecord.Contains("donotfax") && queriedEntityRecord["donotfax"] != null)
				{
					account.DoNotFax = (bool)queriedEntityRecord["donotfax"];
					localContext.TracingService.Trace("Got donotfax.");
				}
				else
				{
					account.DoNotFax = null;
					localContext.TracingService.Trace("Did NOT find donotfax.");
				}
				if (queriedEntityRecord.Contains("donotphone") && queriedEntityRecord["donotphone"] != null)
				{
					account.DoNotPhone = (bool)queriedEntityRecord["donotphone"];
					localContext.TracingService.Trace("Got donotphone.");
				}
				else
				{
					account.DoNotPhone = null;
					localContext.TracingService.Trace("Did NOT find donotphone.");
				}
				if (queriedEntityRecord.Contains("donotpostalmail") && queriedEntityRecord["donotpostalmail"] != null)
				{
					account.DoNotPostalMail = (bool)queriedEntityRecord["donotpostalmail"];
					localContext.TracingService.Trace("Got donotpostalmail.");
				}
				else
				{
					account.DoNotPostalMail = null;
					localContext.TracingService.Trace("Did NOT find donotpostalmail.");
				}
				if (queriedEntityRecord.Contains("emailaddress1") && queriedEntityRecord["emailaddress1"] != null)
				{
					account.EmailAddress1 = (string)queriedEntityRecord["emailaddress1"];
					localContext.TracingService.Trace("Got emailaddress1.");
				}
				else
				{
					account.EmailAddress1 = null;
					localContext.TracingService.Trace("Did NOT find emailaddress1.");
				}
				if (queriedEntityRecord.Contains("emailaddress2") && queriedEntityRecord["emailaddress2"] != null)
				{
					account.EmailAddress2 = (string)queriedEntityRecord["emailaddress2"];
					localContext.TracingService.Trace("Got emailaddress2.");
				}
				else
				{
					account.EmailAddress2 = null;
					localContext.TracingService.Trace("Did NOT find emailaddress2.");
				}
				if (queriedEntityRecord.Contains("emailaddress3") && queriedEntityRecord["emailaddress3"] != null)
				{
					account.EmailAddress3 = (string)queriedEntityRecord["emailaddress3"];
					localContext.TracingService.Trace("Got emailaddress3.");
				}
				else
				{
					account.EmailAddress3 = null;
					localContext.TracingService.Trace("Did NOT find emailaddress3.");
				}
				if (queriedEntityRecord.Contains("masterid") && queriedEntityRecord["masterid"] != null)
				{
					account.MasterId = ((EntityReference)queriedEntityRecord["masterid"]).Id;
					localContext.TracingService.Trace("Got masterid.");
				}
				else
				{
					account.MasterId = null;
					localContext.TracingService.Trace("Did NOT find masterid.");
				}
				if (queriedEntityRecord.Contains("owningbusinessunit") && queriedEntityRecord["owningbusinessunit"] != null)
				{
					account.OwningBusinessUnitId = ((EntityReference)queriedEntityRecord["owningbusinessunit"]).Id;
					localContext.TracingService.Trace("Got owningbusinessunit.");
				}
				else
				{
					account.OwningBusinessUnitId = null;
					localContext.TracingService.Trace("Did NOT find owningbusinessunit.");
				}
				if (queriedEntityRecord.Contains("msnfp_anonymity") && queriedEntityRecord["msnfp_anonymity"] != null)
				{
					account.msnfp_Anonymity = ((OptionSetValue)queriedEntityRecord["msnfp_anonymity"]).Value;
					localContext.TracingService.Trace("Got msnfp_anonymity.");
				}
				else
				{
					account.msnfp_Anonymity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_anonymity.");
				}
				if (queriedEntityRecord.Contains("msnfp_count_lifetimetransactions") && queriedEntityRecord["msnfp_count_lifetimetransactions"] != null)
				{
					account.msnfp_Count_LifetimeTransactions = (int)queriedEntityRecord["msnfp_count_lifetimetransactions"];
					localContext.TracingService.Trace("Got msnfp_count_lifetimetransactions.");
				}
				else
				{
					account.msnfp_Count_LifetimeTransactions = null;
					localContext.TracingService.Trace("Did NOT find msnfp_count_lifetimetransactions.");
				}
				if (queriedEntityRecord.Contains("msnfp_givinglevelid") && queriedEntityRecord["msnfp_givinglevelid"] != null)
				{
					account.msnfp_GivingLevelId = ((EntityReference)queriedEntityRecord["msnfp_givinglevelid"]).Id;
					localContext.TracingService.Trace("Got msnfp_givinglevelid.");
				}
				else
				{
					account.msnfp_GivingLevelId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_givinglevelid.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasteventpackagedate") && queriedEntityRecord["msnfp_lasteventpackagedate"] != null)
				{
					account.msnfp_LastEventPackageDate = (DateTime)queriedEntityRecord["msnfp_lasteventpackagedate"];
					localContext.TracingService.Trace("Got msnfp_lasteventpackagedate.");
				}
				else
				{
					account.msnfp_LastEventPackageDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasteventpackagedate.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasteventpackageid") && queriedEntityRecord["msnfp_lasteventpackageid"] != null)
				{
					account.msnfp_LastEventPackageId = ((EntityReference)queriedEntityRecord["msnfp_lasteventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_lasteventpackageid.");
				}
				else
				{
					account.msnfp_LastEventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasteventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasttransactiondate") && queriedEntityRecord["msnfp_lasttransactiondate"] != null)
				{
					account.msnfp_LastTransactionDate = (DateTime)queriedEntityRecord["msnfp_lasttransactiondate"];
					localContext.TracingService.Trace("Got msnfp_lasttransactiondate.");
				}
				else
				{
					account.msnfp_LastTransactionDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasttransactiondate.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasttransactionid") && queriedEntityRecord["msnfp_lasttransactionid"] != null)
				{
					account.msnfp_LastTransactionId = ((EntityReference)queriedEntityRecord["msnfp_lasttransactionid"]).Id;
					localContext.TracingService.Trace("Got msnfp_lasttransactionid.");
				}
				else
				{
					account.msnfp_LastTransactionId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasttransactionid.");
				}
				if (queriedEntityRecord.Contains("msnfp_preferredlanguagecode") && queriedEntityRecord["msnfp_preferredlanguagecode"] != null)
				{
					account.msnfp_PreferredLanguageCode = ((OptionSetValue)queriedEntityRecord["msnfp_preferredlanguagecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_preferredlanguagecode.");
				}
				else
				{
					account.msnfp_PreferredLanguageCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_preferredlanguagecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_primarymembershipid") && queriedEntityRecord["msnfp_primarymembershipid"] != null)
				{
					account.msnfp_PrimaryMembershipId = ((EntityReference)queriedEntityRecord["msnfp_primarymembershipid"]).Id;
					localContext.TracingService.Trace("Got msnfp_primarymembershipid.");
				}
				else
				{
					account.msnfp_PrimaryMembershipId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_primarymembershipid.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptpreferencecode") && queriedEntityRecord["msnfp_receiptpreferencecode"] != null)
				{
					account.msnfp_ReceiptPreferenceCode = ((OptionSetValue)queriedEntityRecord["msnfp_receiptpreferencecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_receiptpreferencecode.");
				}
				else
				{
					account.msnfp_ReceiptPreferenceCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptpreferencecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_lifetimetransactions") && queriedEntityRecord["msnfp_sum_lifetimetransactions"] != null)
				{
					account.msnfp_Sum_LifetimeTransactions = ((Money)queriedEntityRecord["msnfp_sum_lifetimetransactions"]).Value;
					localContext.TracingService.Trace("Got msnfp_primarymembershipid.");
				}
				else
				{
					account.msnfp_Sum_LifetimeTransactions = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_lifetimetransactions.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone1typecode") && queriedEntityRecord["msnfp_telephone1typecode"] != null)
				{
					account.msnfp_Telephone1TypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_telephone1typecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_telephone1typecode.");
				}
				else
				{
					account.msnfp_Telephone1TypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone1typecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone2typecode") && queriedEntityRecord["msnfp_telephone2typecode"] != null)
				{
					account.msnfp_Telephone2TypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_telephone2typecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_telephone2typecode.");
				}
				else
				{
					account.msnfp_Telephone2TypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone2typecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone3typecode") && queriedEntityRecord["msnfp_telephone3typecode"] != null)
				{
					account.msnfp_Telephone3TypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_telephone3typecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_telephone3typecode.");
				}
				else
				{
					account.msnfp_Telephone3TypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone3typecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_vip") && queriedEntityRecord["msnfp_vip"] != null)
				{
					account.msnfp_Vip = (bool)queriedEntityRecord["msnfp_vip"];
					localContext.TracingService.Trace("Got msnfp_vip.");
				}
				else
				{
					account.msnfp_Vip = null;
					localContext.TracingService.Trace("Did NOT find msnfp_vip.");
				}
				if (queriedEntityRecord.Contains("merged") && queriedEntityRecord["merged"] != null)
				{
					account.Merged = (bool)queriedEntityRecord["merged"];
					localContext.TracingService.Trace("Got merged.");
				}
				else
				{
					account.Merged = null;
					localContext.TracingService.Trace("Did NOT find merged.");
				}
				if (queriedEntityRecord.Contains("name") && queriedEntityRecord["name"] != null)
				{
					account.Name = (string)queriedEntityRecord["name"];
					localContext.TracingService.Trace("Got name.");
				}
				else
				{
					account.Name = null;
					localContext.TracingService.Trace("Did NOT find name.");
				}
				if (queriedEntityRecord.Contains("parentaccountid") && queriedEntityRecord["parentaccountid"] != null)
				{
					account.ParentAccountId = ((EntityReference)queriedEntityRecord["parentaccountid"]).Id;
					localContext.TracingService.Trace("Got parentaccountid.");
				}
				else
				{
					account.ParentAccountId = null;
					localContext.TracingService.Trace("Did NOT find parentaccountid.");
				}
				if (queriedEntityRecord.Contains("telephone1") && queriedEntityRecord["telephone1"] != null)
				{
					account.Telephone1 = (string)queriedEntityRecord["telephone1"];
					localContext.TracingService.Trace("Got telephone1.");
				}
				else
				{
					account.Telephone1 = null;
					localContext.TracingService.Trace("Did NOT find telephone1.");
				}
				if (queriedEntityRecord.Contains("telephone2") && queriedEntityRecord["telephone2"] != null)
				{
					account.Telephone2 = (string)queriedEntityRecord["telephone2"];
					localContext.TracingService.Trace("Got telephone2.");
				}
				else
				{
					account.Telephone2 = null;
					localContext.TracingService.Trace("Did NOT find telephone2.");
				}
				if (queriedEntityRecord.Contains("telephone3") && queriedEntityRecord["telephone3"] != null)
				{
					account.Telephone3 = (string)queriedEntityRecord["telephone3"];
					localContext.TracingService.Trace("Got telephone3.");
				}
				else
				{
					account.Telephone3 = null;
					localContext.TracingService.Trace("Did NOT find telephone3.");
				}
				if (queriedEntityRecord.Contains("websiteurl") && queriedEntityRecord["websiteurl"] != null)
				{
					account.WebSiteURL = (string)queriedEntityRecord["websiteurl"];
					localContext.TracingService.Trace("Got websiteurl.");
				}
				else
				{
					account.WebSiteURL = null;
					localContext.TracingService.Trace("Did NOT find websiteurl.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					account.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got TransactionCurrencyId.");
				}
				else
				{
					account.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find TransactionCurrencyId.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					account.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					account.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					account.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					account.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (queriedEntityRecord.Contains("msnfp_accounttype") && queriedEntityRecord["msnfp_accounttype"] != null)
				{
					account.msnfp_accounttype = ((OptionSetValue)queriedEntityRecord["msnfp_accounttype"]).Value;
					localContext.TracingService.Trace("Got msnfp_accounttype.");
				}
				else
				{
					account.msnfp_accounttype = null;
					localContext.TracingService.Trace("Did NOT find msnfp_accounttype.");
				}
				if (queriedEntityRecord.Contains("msnfp_year0_giving") && queriedEntityRecord["msnfp_year0_giving"] != null)
				{
					account.msnfp_year0_giving = ((Money)queriedEntityRecord["msnfp_year0_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year0_giving.");
				}
				else
				{
					account.msnfp_year0_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year0_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year1_giving") && queriedEntityRecord["msnfp_year1_giving"] != null)
				{
					account.msnfp_year1_giving = ((Money)queriedEntityRecord["msnfp_year1_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year1_giving.");
				}
				else
				{
					account.msnfp_year1_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year1_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year2_giving") && queriedEntityRecord["msnfp_year2_giving"] != null)
				{
					account.msnfp_year2_giving = ((Money)queriedEntityRecord["msnfp_year2_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year2_giving.");
				}
				else
				{
					account.msnfp_year2_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year2_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year3_giving") && queriedEntityRecord["msnfp_year3_giving"] != null)
				{
					account.msnfp_year3_giving = ((Money)queriedEntityRecord["msnfp_year3_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year3_giving.");
				}
				else
				{
					account.msnfp_year3_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year3_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year4_giving") && queriedEntityRecord["msnfp_year4_giving"] != null)
				{
					account.msnfp_year4_giving = ((Money)queriedEntityRecord["msnfp_year4_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year4_giving.");
				}
				else
				{
					account.msnfp_year4_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year4_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_lifetimegivingsum") && queriedEntityRecord["msnfp_lifetimegivingsum"] != null)
				{
					account.msnfp_lifetimegivingsum = ((Money)queriedEntityRecord["msnfp_lifetimegivingsum"]).Value;
					localContext.TracingService.Trace("Got msnfp_lifetimegivingsum.");
				}
				else
				{
					account.msnfp_lifetimegivingsum = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lifetimegivingsum.");
				}
				if (messageName == "Create")
				{
					account.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					account.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					account.CreatedOn = null;
				}
				account.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					account.Deleted = true;
					account.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					account.Deleted = false;
					account.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Account));
				dataContractJsonSerializer.WriteObject(memoryStream, account);
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
                string text3="";
				try {
				
				
					text3 = webAPIClient.UploadString(text2, @string);
				
				
				}catch(Exception e)
				{
					localContext.TracingService.Trace("Got response."+e);

				}
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
