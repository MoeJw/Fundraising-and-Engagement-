using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugins.AzureModels;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class ContactCreate : PluginBase
	{
		public ContactCreate(string unsecure, string secure)
			: base(typeof(ContactCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered ContactCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			if (pluginExecutionContext.Depth > 1 && !CheckExecutionPipeLine(pluginExecutionContext))
			{
				localContext.TracingService.Trace("Context.depth > 1 => Exiting Plugin. context.Depth: " + pluginExecutionContext.Depth);
				localContext.TracingService.Trace("Parent context: " + pluginExecutionContext.ParentContext.PrimaryEntityName);
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
					if (entity.LogicalName.ToLower() != "contact")
					{
						throw new InvalidPluginExecutionException("The target entity is NOT CONTACT. Exiting plugin.");
					}
				}
				else
				{
					if (!(messageName == "Delete"))
					{
						throw new InvalidPluginExecutionException("The Target is NOT an Entity. Exiting plugin.");
					}
					Entity queriedEntityRecord = organizationService.Retrieve("contact", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumns());
					if (configurationRecordByUser.Contains("msnfp_apipadlocktoken") && configurationRecordByUser["msnfp_apipadlocktoken"] != null)
					{
						AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
					}
				}
				if (messageName != "Delete")
				{
					Entity retreivedContact = organizationService.Retrieve(entity.LogicalName, entity.Id, GetColumns());
					if (messageName == "Create" && retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid") != null && retreivedContact.GetAttributeValue<EntityReference>("parentcustomerid") != null)
					{
						localContext.TracingService.Trace("Contact has a msnfp_householdid and parentcustomerid");
						localContext.TracingService.Trace("msnfp_householdid = " + ((EntityReference)retreivedContact["msnfp_householdid"]).Id.ToString());
						localContext.TracingService.Trace("parentcustomerid = " + ((EntityReference)retreivedContact["parentcustomerid"]).Id.ToString());
						if (((EntityReference)retreivedContact["msnfp_householdid"]).Id.Equals(((EntityReference)retreivedContact["parentcustomerid"]).Id))
						{
							localContext.TracingService.Trace("They are the same, so remove company.");
							retreivedContact["parentcustomerid"] = null;
							UpdateEntity(organizationService, retreivedContact);
						}
					}
					SetUpHousehold(organizationService, configurationRecordByUser.GetAttributeValue<string>("msnfp_householdsequence"), ref retreivedContact, entity, pluginExecutionContext, localContext.TracingService);
					if (configurationRecordByUser.Contains("msnfp_apipadlocktoken") && configurationRecordByUser["msnfp_apipadlocktoken"] != null)
					{
						AddOrUpdateThisRecordWithAzure(retreivedContact, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
					}
					else
					{
						localContext.TracingService.Trace("No Padlock Token found to synchronize the record to Azure. AddOrUpdateThisRecordWithAzure() failed.");
					}
				}
				localContext.TracingService.Trace("---------Exitting ContactCreate.cs---------");
				return;
			}
			throw new ArgumentNullException("Target");
		}

		private bool CheckExecutionPipeLine(IPluginExecutionContext context)
		{
			return context.ParentContext != null && (context.ParentContext.PrimaryEntityName == "account" || context.ParentContext.PrimaryEntityName == "contact" || (context.ParentContext != null && context.ParentContext.ParentContext != null && context.ParentContext.ParentContext.PrimaryEntityName == "msnfp_donationimport"));
		}

		private bool CheckExecutionPipelineForDonationImport(IPluginExecutionContext context)
		{
			return context.ParentContext != null && context.ParentContext != null && context.ParentContext.ParentContext != null && context.ParentContext.ParentContext.PrimaryEntityName == "msnfp_donationimport";
		}

		private Entity GetDonationImportFromExecutionPipeLine(IPluginExecutionContext context, IOrganizationService service)
		{
			Entity entity = null;
			if (CheckExecutionPipelineForDonationImport(context))
			{
				entity = ((context.ParentContext.ParentContext.SharedVariables.Where((KeyValuePair<string, object> s) => s.Key == "DonationImport").Count() > 0) ? ((Entity)context.ParentContext.ParentContext.SharedVariables.FirstOrDefault((KeyValuePair<string, object> s) => s.Key == "DonationImport").Value) : null);
				if (entity == null)
				{
					entity = service.Retrieve("msnfp_donationimport", context.ParentContext.ParentContext.PrimaryEntityId, new ColumnSet("msnfp_createhousehold", "msnfp_donationimportid"));
				}
			}
			return entity;
		}

		private ColumnSet GetColumns()
		{
			return new ColumnSet("contactid", "address1_addressid", "address1_addresstypecode", "address1_city", "address1_country", "address1_county", "address1_latitude", "address1_line1", "address1_line2", "address1_line3", "address1_longitude", "address1_name", "address1_postalcode", "address1_postofficebox", "address1_stateorprovince", "address2_addressid", "address2_addresstypecode", "address2_city", "address2_country", "address2_county", "address2_latitude", "address2_line1", "address2_line2", "address2_line3", "address2_longitude", "address2_name", "address2_postalcode", "address2_postofficebox", "address2_stateorprovince", "address3_addressid", "address3_addresstypecode", "address3_city", "address3_country", "address3_county", "address3_latitude", "address3_line1", "address3_line2", "address3_line3", "address3_longitude", "address3_name", "address3_postalcode", "address3_postofficebox", "address3_stateorprovince", "msnfp_birthday", "donotbulkemail", "donotbulkpostalmail", "donotemail", "donotfax", "donotphone", "donotpostalmail", "emailaddress1", "emailaddress2", "emailaddress3", "firstname", "fullname", "gendercode", "jobtitle", "lastname", "masterid", "owningbusinessunit", "msnfp_age", "msnfp_anonymity", "msnfp_count_lifetimetransactions", "msnfp_givinglevelid", "msnfp_lasteventpackagedate", "msnfp_lasteventpackageid", "msnfp_lasttransactiondate", "msnfp_lasttransactionid", "msnfp_preferredlanguagecode", "msnfp_primarymembershipid", "msnfp_receiptpreferencecode", "msnfp_sum_lifetimetransactions", "msnfp_telephone1typecode", "msnfp_telephone2typecode", "msnfp_telephone3typecode", "msnfp_upcomingbirthday", "msnfp_vip", "merged", "middlename", "mobilephone", "parentcustomerid", "salutation", "suffix", "telephone1", "telephone2", "telephone3", "transactioncurrencyid", "statecode", "statuscode", "createdon", "msnfp_householdrelationship", "msnfp_householdid", "msnfp_deceased", "msnfp_year0_giving", "msnfp_year1_giving", "msnfp_year2_giving", "msnfp_year3_giving", "msnfp_year4_giving", "msnfp_lifetimegivingsum");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Contact";
			string text2 = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["contactid"].ToString());
				Contact contact = new Contact();
				contact.ContactId = (Guid)queriedEntityRecord["contactid"];
				if (queriedEntityRecord.Contains("address1_addressid") && queriedEntityRecord["address1_addressid"] != null)
				{
					contact.Address1_AddressId = (Guid)queriedEntityRecord["address1_addressid"];
					localContext.TracingService.Trace("Got address1_addressid.");
				}
				else
				{
					contact.Address1_AddressId = null;
					localContext.TracingService.Trace("Did NOT find address1_addressid.");
				}
				if (queriedEntityRecord.Contains("address1_addresstypecode") && queriedEntityRecord["address1_addresstypecode"] != null)
				{
					contact.Address1_AddressTypeCode = ((OptionSetValue)queriedEntityRecord["address1_addresstypecode"]).Value;
					localContext.TracingService.Trace("Got address1_addresstypecode.");
				}
				else
				{
					contact.Address1_AddressTypeCode = null;
					localContext.TracingService.Trace("Did NOT find address1_addresstypecode.");
				}
				if (queriedEntityRecord.Contains("address1_city") && queriedEntityRecord["address1_city"] != null)
				{
					contact.Address1_City = (string)queriedEntityRecord["address1_city"];
					localContext.TracingService.Trace("Got address1_city.");
				}
				else
				{
					contact.Address1_City = null;
					localContext.TracingService.Trace("Did NOT find address1_city.");
				}
				if (queriedEntityRecord.Contains("address1_country") && queriedEntityRecord["address1_country"] != null)
				{
					contact.Address1_Country = (string)queriedEntityRecord["address1_country"];
					localContext.TracingService.Trace("Got address1_country.");
				}
				else
				{
					contact.Address1_Country = null;
					localContext.TracingService.Trace("Did NOT find address1_country.");
				}
				if (queriedEntityRecord.Contains("address1_county") && queriedEntityRecord["address1_county"] != null)
				{
					contact.Address1_County = (string)queriedEntityRecord["address1_county"];
					localContext.TracingService.Trace("Got address1_county.");
				}
				else
				{
					contact.Address1_County = null;
					localContext.TracingService.Trace("Did NOT find address1_county.");
				}
				if (queriedEntityRecord.Contains("address1_latitude") && queriedEntityRecord["address1_latitude"] != null)
				{
					contact.Address1_Latitude = (float)queriedEntityRecord["address1_latitude"];
					localContext.TracingService.Trace("Got address1_latitude.");
				}
				else
				{
					contact.Address1_Latitude = null;
					localContext.TracingService.Trace("Did NOT find address1_latitude.");
				}
				if (queriedEntityRecord.Contains("address1_line1") && queriedEntityRecord["address1_line1"] != null)
				{
					contact.Address1_Line1 = (string)queriedEntityRecord["address1_line1"];
					localContext.TracingService.Trace("Got address1_line1.");
				}
				else
				{
					contact.Address1_Line1 = null;
					localContext.TracingService.Trace("Did NOT find address1_line1.");
				}
				if (queriedEntityRecord.Contains("address1_line2") && queriedEntityRecord["address1_line2"] != null)
				{
					contact.Address1_Line2 = (string)queriedEntityRecord["address1_line2"];
					localContext.TracingService.Trace("Got address1_line2.");
				}
				else
				{
					contact.Address1_Line2 = null;
					localContext.TracingService.Trace("Did NOT find address1_line2.");
				}
				if (queriedEntityRecord.Contains("address1_line3") && queriedEntityRecord["address1_line3"] != null)
				{
					contact.Address1_Line3 = (string)queriedEntityRecord["address1_line3"];
					localContext.TracingService.Trace("Got address1_line3.");
				}
				else
				{
					contact.Address1_Line3 = null;
					localContext.TracingService.Trace("Did NOT find address1_line3.");
				}
				if (queriedEntityRecord.Contains("address1_longitude") && queriedEntityRecord["address1_longitude"] != null)
				{
					contact.Address1_Longitude = (float)queriedEntityRecord["address1_longitude"];
					localContext.TracingService.Trace("Got address1_longitude.");
				}
				else
				{
					contact.Address1_Longitude = null;
					localContext.TracingService.Trace("Did NOT find address1_longitude.");
				}
				if (queriedEntityRecord.Contains("address1_name") && queriedEntityRecord["address1_name"] != null)
				{
					contact.Address1_Name = (string)queriedEntityRecord["address1_name"];
					localContext.TracingService.Trace("Got address1_name.");
				}
				else
				{
					contact.Address1_Name = null;
					localContext.TracingService.Trace("Did NOT find address1_name.");
				}
				if (queriedEntityRecord.Contains("address1_postalcode") && queriedEntityRecord["address1_postalcode"] != null)
				{
					contact.Address1_PostalCode = (string)queriedEntityRecord["address1_postalcode"];
					localContext.TracingService.Trace("Got address1_postalcode.");
				}
				else
				{
					contact.Address1_PostalCode = null;
					localContext.TracingService.Trace("Did NOT find address1_postalcode.");
				}
				if (queriedEntityRecord.Contains("address1_postofficebox") && queriedEntityRecord["address1_postofficebox"] != null)
				{
					contact.Address1_PostOfficeBox = (string)queriedEntityRecord["address1_postofficebox"];
					localContext.TracingService.Trace("Got address1_postofficebox.");
				}
				else
				{
					contact.Address1_PostOfficeBox = null;
					localContext.TracingService.Trace("Did NOT find address1_postofficebox.");
				}
				if (queriedEntityRecord.Contains("address1_stateorprovince") && queriedEntityRecord["address1_stateorprovince"] != null)
				{
					contact.Address1_StateOrProvince = (string)queriedEntityRecord["address1_stateorprovince"];
					localContext.TracingService.Trace("Got address1_stateorprovince.");
				}
				else
				{
					contact.Address1_StateOrProvince = null;
					localContext.TracingService.Trace("Did NOT find address1_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("address2_addressid") && queriedEntityRecord["address2_addressid"] != null)
				{
					contact.Address2_AddressId = (Guid)queriedEntityRecord["address2_addressid"];
					localContext.TracingService.Trace("Got address2_addressid.");
				}
				else
				{
					contact.Address2_AddressId = null;
					localContext.TracingService.Trace("Did NOT find address1_addressid.");
				}
				if (queriedEntityRecord.Contains("address2_addresstypecode") && queriedEntityRecord["address2_addresstypecode"] != null)
				{
					contact.Address2_AddressTypeCode = ((OptionSetValue)queriedEntityRecord["address2_addresstypecode"]).Value;
					localContext.TracingService.Trace("Got address2_addresstypecode.");
				}
				else
				{
					contact.Address2_AddressTypeCode = null;
					localContext.TracingService.Trace("Did NOT find address2_addresstypecode.");
				}
				if (queriedEntityRecord.Contains("address2_city") && queriedEntityRecord["address2_city"] != null)
				{
					contact.Address2_City = (string)queriedEntityRecord["address2_city"];
					localContext.TracingService.Trace("Got address2_city.");
				}
				else
				{
					contact.Address2_City = null;
					localContext.TracingService.Trace("Did NOT find address2_city.");
				}
				if (queriedEntityRecord.Contains("address2_country") && queriedEntityRecord["address2_country"] != null)
				{
					contact.Address2_Country = (string)queriedEntityRecord["address2_country"];
					localContext.TracingService.Trace("Got address2_country.");
				}
				else
				{
					contact.Address2_Country = null;
					localContext.TracingService.Trace("Did NOT find address2_country.");
				}
				if (queriedEntityRecord.Contains("address2_county") && queriedEntityRecord["address2_county"] != null)
				{
					contact.Address2_County = (string)queriedEntityRecord["address2_county"];
					localContext.TracingService.Trace("Got address2_county.");
				}
				else
				{
					contact.Address2_County = null;
					localContext.TracingService.Trace("Did NOT find address2_county.");
				}
				if (queriedEntityRecord.Contains("address2_latitude") && queriedEntityRecord["address2_latitude"] != null)
				{
					contact.Address2_Latitude = (float)queriedEntityRecord["address2_latitude"];
					localContext.TracingService.Trace("Got address2_latitude.");
				}
				else
				{
					contact.Address2_Latitude = null;
					localContext.TracingService.Trace("Did NOT find address2_latitude.");
				}
				if (queriedEntityRecord.Contains("address2_line1") && queriedEntityRecord["address2_line1"] != null)
				{
					contact.Address2_Line1 = (string)queriedEntityRecord["address2_line1"];
					localContext.TracingService.Trace("Got address2_line1.");
				}
				else
				{
					contact.Address2_Line1 = null;
					localContext.TracingService.Trace("Did NOT find address2_line1.");
				}
				if (queriedEntityRecord.Contains("address2_line2") && queriedEntityRecord["address2_line2"] != null)
				{
					contact.Address2_Line2 = (string)queriedEntityRecord["address2_line2"];
					localContext.TracingService.Trace("Got address2_line2.");
				}
				else
				{
					contact.Address2_Line2 = null;
					localContext.TracingService.Trace("Did NOT find address2_line2.");
				}
				if (queriedEntityRecord.Contains("address2_line3") && queriedEntityRecord["address2_line3"] != null)
				{
					contact.Address2_Line3 = (string)queriedEntityRecord["address2_line3"];
					localContext.TracingService.Trace("Got address2_line3.");
				}
				else
				{
					contact.Address2_Line3 = null;
					localContext.TracingService.Trace("Did NOT find address2_line3.");
				}
				if (queriedEntityRecord.Contains("address2_longitude") && queriedEntityRecord["address2_longitude"] != null)
				{
					contact.Address2_Longitude = (float)queriedEntityRecord["address2_longitude"];
					localContext.TracingService.Trace("Got address2_longitude.");
				}
				else
				{
					contact.Address2_Longitude = null;
					localContext.TracingService.Trace("Did NOT find address2_longitude.");
				}
				if (queriedEntityRecord.Contains("address2_name") && queriedEntityRecord["address2_name"] != null)
				{
					contact.Address2_Name = (string)queriedEntityRecord["address2_name"];
					localContext.TracingService.Trace("Got address2_name.");
				}
				else
				{
					contact.Address2_Name = null;
					localContext.TracingService.Trace("Did NOT find address2_name.");
				}
				if (queriedEntityRecord.Contains("address2_postalcode") && queriedEntityRecord["address2_postalcode"] != null)
				{
					contact.Address2_PostalCode = (string)queriedEntityRecord["address2_postalcode"];
					localContext.TracingService.Trace("Got address2_postalcode.");
				}
				else
				{
					contact.Address2_PostalCode = null;
					localContext.TracingService.Trace("Did NOT find address2_postalcode.");
				}
				if (queriedEntityRecord.Contains("address2_postofficebox") && queriedEntityRecord["address2_postofficebox"] != null)
				{
					contact.Address2_PostOfficeBox = (string)queriedEntityRecord["address2_postofficebox"];
					localContext.TracingService.Trace("Got address2_postofficebox.");
				}
				else
				{
					contact.Address2_PostOfficeBox = null;
					localContext.TracingService.Trace("Did NOT find address2_postofficebox.");
				}
				if (queriedEntityRecord.Contains("address2_stateorprovince") && queriedEntityRecord["address2_stateorprovince"] != null)
				{
					contact.Address2_StateOrProvince = (string)queriedEntityRecord["address2_stateorprovince"];
					localContext.TracingService.Trace("Got address2_stateorprovince.");
				}
				else
				{
					contact.Address2_StateOrProvince = null;
					localContext.TracingService.Trace("Did NOT find address2_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("address3_addressid") && queriedEntityRecord["address3_addressid"] != null)
				{
					contact.Address3_AddressId = (Guid)queriedEntityRecord["address3_addressid"];
					localContext.TracingService.Trace("Got address3_addressid.");
				}
				else
				{
					contact.Address3_AddressId = null;
					localContext.TracingService.Trace("Did NOT find address3_addressid.");
				}
				if (queriedEntityRecord.Contains("address3_addresstypecode") && queriedEntityRecord["address3_addresstypecode"] != null)
				{
					contact.Address3_AddressTypeCode = ((OptionSetValue)queriedEntityRecord["address3_addresstypecode"]).Value;
					localContext.TracingService.Trace("Got address3_addresstypecode.");
				}
				else
				{
					contact.Address3_AddressTypeCode = null;
					localContext.TracingService.Trace("Did NOT find address3_addresstypecode.");
				}
				if (queriedEntityRecord.Contains("address3_city") && queriedEntityRecord["address3_city"] != null)
				{
					contact.Address3_City = (string)queriedEntityRecord["address3_city"];
					localContext.TracingService.Trace("Got address3_city.");
				}
				else
				{
					contact.Address3_City = null;
					localContext.TracingService.Trace("Did NOT find address3_city.");
				}
				if (queriedEntityRecord.Contains("address3_country") && queriedEntityRecord["address3_country"] != null)
				{
					contact.Address3_Country = (string)queriedEntityRecord["address3_country"];
					localContext.TracingService.Trace("Got address3_country.");
				}
				else
				{
					contact.Address3_Country = null;
					localContext.TracingService.Trace("Did NOT find address3_country.");
				}
				if (queriedEntityRecord.Contains("address3_county") && queriedEntityRecord["address3_county"] != null)
				{
					contact.Address3_County = (string)queriedEntityRecord["address3_county"];
					localContext.TracingService.Trace("Got address3_county.");
				}
				else
				{
					contact.Address3_County = null;
					localContext.TracingService.Trace("Did NOT find address3_county.");
				}
				if (queriedEntityRecord.Contains("address3_latitude") && queriedEntityRecord["address3_latitude"] != null)
				{
					contact.Address3_Latitude = (float)queriedEntityRecord["address3_latitude"];
					localContext.TracingService.Trace("Got address3_latitude.");
				}
				else
				{
					contact.Address3_Latitude = null;
					localContext.TracingService.Trace("Did NOT find address3_latitude.");
				}
				if (queriedEntityRecord.Contains("address3_line1") && queriedEntityRecord["address3_line1"] != null)
				{
					contact.Address3_Line1 = (string)queriedEntityRecord["address3_line1"];
					localContext.TracingService.Trace("Got address3_line1.");
				}
				else
				{
					contact.Address3_Line1 = null;
					localContext.TracingService.Trace("Did NOT find address3_line1.");
				}
				if (queriedEntityRecord.Contains("address3_line2") && queriedEntityRecord["address3_line2"] != null)
				{
					contact.Address3_Line2 = (string)queriedEntityRecord["address3_line2"];
					localContext.TracingService.Trace("Got address3_line2.");
				}
				else
				{
					contact.Address3_Line2 = null;
					localContext.TracingService.Trace("Did NOT find address3_line2.");
				}
				if (queriedEntityRecord.Contains("address3_line3") && queriedEntityRecord["address3_line3"] != null)
				{
					contact.Address3_Line3 = (string)queriedEntityRecord["address3_line3"];
					localContext.TracingService.Trace("Got address3_line3.");
				}
				else
				{
					contact.Address3_Line3 = null;
					localContext.TracingService.Trace("Did NOT find address3_line3.");
				}
				if (queriedEntityRecord.Contains("address3_longitude") && queriedEntityRecord["address3_longitude"] != null)
				{
					contact.Address3_Longitude = (float)queriedEntityRecord["address3_longitude"];
					localContext.TracingService.Trace("Got address3_longitude.");
				}
				else
				{
					contact.Address3_Longitude = null;
					localContext.TracingService.Trace("Did NOT find address3_longitude.");
				}
				if (queriedEntityRecord.Contains("address3_name") && queriedEntityRecord["address3_name"] != null)
				{
					contact.Address3_Name = (string)queriedEntityRecord["address3_name"];
					localContext.TracingService.Trace("Got address3_name.");
				}
				else
				{
					contact.Address3_Name = null;
					localContext.TracingService.Trace("Did NOT find address3_name.");
				}
				if (queriedEntityRecord.Contains("address3_postalcode") && queriedEntityRecord["address3_postalcode"] != null)
				{
					contact.Address3_PostalCode = (string)queriedEntityRecord["address3_postalcode"];
					localContext.TracingService.Trace("Got address3_postalcode.");
				}
				else
				{
					contact.Address3_PostalCode = null;
					localContext.TracingService.Trace("Did NOT find address3_postalcode.");
				}
				if (queriedEntityRecord.Contains("address3_postofficebox") && queriedEntityRecord["address3_postofficebox"] != null)
				{
					contact.Address3_PostOfficeBox = (string)queriedEntityRecord["address3_postofficebox"];
					localContext.TracingService.Trace("Got address3_postofficebox.");
				}
				else
				{
					contact.Address3_PostOfficeBox = null;
					localContext.TracingService.Trace("Did NOT find address3_postofficebox.");
				}
				if (queriedEntityRecord.Contains("address3_stateorprovince") && queriedEntityRecord["address3_stateorprovince"] != null)
				{
					contact.Address3_StateOrProvince = (string)queriedEntityRecord["address3_stateorprovince"];
					localContext.TracingService.Trace("Got address3_stateorprovince.");
				}
				else
				{
					contact.Address3_StateOrProvince = null;
					localContext.TracingService.Trace("Did NOT find address3_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("msnfp_birthday") && queriedEntityRecord["msnfp_birthday"] != null)
				{
					contact.BirthDate = (DateTime)queriedEntityRecord["msnfp_birthday"];
					localContext.TracingService.Trace("Got msnfp_birthday.");
				}
				else
				{
					contact.BirthDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_birthday.");
				}
				if (queriedEntityRecord.Contains("donotbulkemail") && queriedEntityRecord["donotbulkemail"] != null)
				{
					contact.DoNotBulkEMail = (bool)queriedEntityRecord["donotbulkemail"];
					localContext.TracingService.Trace("Got donotbulkemail.");
				}
				else
				{
					contact.DoNotBulkEMail = null;
					localContext.TracingService.Trace("Did NOT find donotbulkemail.");
				}
				if (queriedEntityRecord.Contains("donotbulkpostalmail") && queriedEntityRecord["donotbulkpostalmail"] != null)
				{
					contact.DoNotBulkPostalMail = (bool)queriedEntityRecord["donotbulkpostalmail"];
					localContext.TracingService.Trace("Got donotbulkpostalmail.");
				}
				else
				{
					contact.DoNotBulkPostalMail = null;
					localContext.TracingService.Trace("Did NOT find donotbulkpostalmail.");
				}
				if (queriedEntityRecord.Contains("donotemail") && queriedEntityRecord["donotemail"] != null)
				{
					contact.DoNotEmail = (bool)queriedEntityRecord["donotemail"];
					localContext.TracingService.Trace("Got donotemail.");
				}
				else
				{
					contact.DoNotEmail = null;
					localContext.TracingService.Trace("Did NOT find donotemail.");
				}
				if (queriedEntityRecord.Contains("donotfax") && queriedEntityRecord["donotfax"] != null)
				{
					contact.DoNotFax = (bool)queriedEntityRecord["donotfax"];
					localContext.TracingService.Trace("Got donotfax.");
				}
				else
				{
					contact.DoNotFax = null;
					localContext.TracingService.Trace("Did NOT find donotfax.");
				}
				if (queriedEntityRecord.Contains("donotphone") && queriedEntityRecord["donotphone"] != null)
				{
					contact.DoNotPhone = (bool)queriedEntityRecord["donotphone"];
					localContext.TracingService.Trace("Got donotphone.");
				}
				else
				{
					contact.DoNotPhone = null;
					localContext.TracingService.Trace("Did NOT find donotphone.");
				}
				if (queriedEntityRecord.Contains("donotpostalmail") && queriedEntityRecord["donotpostalmail"] != null)
				{
					contact.DoNotPostalMail = (bool)queriedEntityRecord["donotpostalmail"];
					localContext.TracingService.Trace("Got donotpostalmail.");
				}
				else
				{
					contact.DoNotPostalMail = null;
					localContext.TracingService.Trace("Did NOT find donotpostalmail.");
				}
				if (queriedEntityRecord.Contains("emailaddress1") && queriedEntityRecord["emailaddress1"] != null)
				{
					contact.EmailAddress1 = (string)queriedEntityRecord["emailaddress1"];
					localContext.TracingService.Trace("Got emailaddress1.");
				}
				else
				{
					contact.EmailAddress1 = null;
					localContext.TracingService.Trace("Did NOT find emailaddress1.");
				}
				if (queriedEntityRecord.Contains("emailaddress2") && queriedEntityRecord["emailaddress2"] != null)
				{
					contact.EmailAddress2 = (string)queriedEntityRecord["emailaddress2"];
					localContext.TracingService.Trace("Got emailaddress2.");
				}
				else
				{
					contact.EmailAddress2 = null;
					localContext.TracingService.Trace("Did NOT find emailaddress2.");
				}
				if (queriedEntityRecord.Contains("emailaddress3") && queriedEntityRecord["emailaddress3"] != null)
				{
					contact.EmailAddress3 = (string)queriedEntityRecord["emailaddress3"];
					localContext.TracingService.Trace("Got emailaddress3.");
				}
				else
				{
					contact.EmailAddress3 = null;
					localContext.TracingService.Trace("Did NOT find emailaddress3.");
				}
				if (queriedEntityRecord.Contains("firstname") && queriedEntityRecord["firstname"] != null)
				{
					contact.FirstName = (string)queriedEntityRecord["firstname"];
					localContext.TracingService.Trace("Got firstname.");
				}
				else
				{
					contact.FirstName = null;
					localContext.TracingService.Trace("Did NOT find firstname.");
				}
				if (queriedEntityRecord.Contains("fullname") && queriedEntityRecord["fullname"] != null)
				{
					contact.FullName = (string)queriedEntityRecord["fullname"];
					localContext.TracingService.Trace("Got fullname.");
				}
				else
				{
					contact.FullName = null;
					localContext.TracingService.Trace("Did NOT find fullname.");
				}
				if (queriedEntityRecord.Contains("gendercode") && queriedEntityRecord["gendercode"] != null)
				{
					contact.GenderCode = ((OptionSetValue)queriedEntityRecord["gendercode"]).Value;
					localContext.TracingService.Trace("Got gendercode.");
				}
				else
				{
					contact.GenderCode = null;
					localContext.TracingService.Trace("Did NOT find gendercode.");
				}
				if (queriedEntityRecord.Contains("jobtitle") && queriedEntityRecord["jobtitle"] != null)
				{
					contact.JobTitle = (string)queriedEntityRecord["jobtitle"];
					localContext.TracingService.Trace("Got jobtitle.");
				}
				else
				{
					contact.JobTitle = null;
					localContext.TracingService.Trace("Did NOT find jobtitle.");
				}
				if (queriedEntityRecord.Contains("lastname") && queriedEntityRecord["lastname"] != null)
				{
					contact.LastName = (string)queriedEntityRecord["lastname"];
					localContext.TracingService.Trace("Got lastname.");
				}
				else
				{
					contact.LastName = null;
					localContext.TracingService.Trace("Did NOT find lastname.");
				}
				if (queriedEntityRecord.Contains("masterid") && queriedEntityRecord["masterid"] != null)
				{
					contact.MasterId = ((EntityReference)queriedEntityRecord["masterid"]).Id;
					localContext.TracingService.Trace("Got masterid.");
				}
				else
				{
					contact.MasterId = null;
					localContext.TracingService.Trace("Did NOT find masterid.");
				}
				if (queriedEntityRecord.Contains("owningbusinessunit") && queriedEntityRecord["owningbusinessunit"] != null)
				{
					contact.OwningBusinessUnitId = ((EntityReference)queriedEntityRecord["owningbusinessunit"]).Id;
					localContext.TracingService.Trace("Got owningbusinessunit.");
				}
				else
				{
					contact.OwningBusinessUnitId = null;
					localContext.TracingService.Trace("Did NOT find owningbusinessunit.");
				}
				if (queriedEntityRecord.Contains("msnfp_age") && queriedEntityRecord["msnfp_age"] != null)
				{
					contact.msnfp_Age = (int)queriedEntityRecord["msnfp_age"];
					localContext.TracingService.Trace("Got msnfp_age.");
				}
				else
				{
					contact.msnfp_Age = null;
					localContext.TracingService.Trace("Did NOT find msnfp_age.");
				}
				if (queriedEntityRecord.Contains("msnfp_anonymity") && queriedEntityRecord["msnfp_anonymity"] != null)
				{
					contact.msnfp_Anonymity = ((OptionSetValue)queriedEntityRecord["msnfp_anonymity"]).Value;
					localContext.TracingService.Trace("Got msnfp_anonymity.");
				}
				else
				{
					contact.msnfp_Anonymity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_anonymity.");
				}
				if (queriedEntityRecord.Contains("msnfp_count_lifetimetransactions") && queriedEntityRecord["msnfp_count_lifetimetransactions"] != null)
				{
					contact.msnfp_Count_LifetimeTransactions = (int)queriedEntityRecord["msnfp_count_lifetimetransactions"];
					localContext.TracingService.Trace("Got msnfp_count_lifetimetransactions.");
				}
				else
				{
					contact.msnfp_Count_LifetimeTransactions = null;
					localContext.TracingService.Trace("Did NOT find msnfp_count_lifetimetransactions.");
				}
				if (queriedEntityRecord.Contains("msnfp_givinglevelid") && queriedEntityRecord["msnfp_givinglevelid"] != null)
				{
					contact.msnfp_GivingLevelId = ((EntityReference)queriedEntityRecord["msnfp_givinglevelid"]).Id;
					localContext.TracingService.Trace("Got msnfp_givinglevelid.");
				}
				else
				{
					contact.msnfp_GivingLevelId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_givinglevelid.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasteventpackagedate") && queriedEntityRecord["msnfp_lasteventpackagedate"] != null)
				{
					contact.msnfp_LastEventPackageDate = (DateTime)queriedEntityRecord["msnfp_lasteventpackagedate"];
					localContext.TracingService.Trace("Got msnfp_lasteventpackagedate.");
				}
				else
				{
					contact.msnfp_LastEventPackageDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasteventpackagedate.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasteventpackageid") && queriedEntityRecord["msnfp_lasteventpackageid"] != null)
				{
					contact.msnfp_LastEventPackageId = ((EntityReference)queriedEntityRecord["msnfp_lasteventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_lasteventpackageid.");
				}
				else
				{
					contact.msnfp_LastEventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasteventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasttransactiondate") && queriedEntityRecord["msnfp_lasttransactiondate"] != null)
				{
					contact.msnfp_LastTransactionDate = (DateTime)queriedEntityRecord["msnfp_lasttransactiondate"];
					localContext.TracingService.Trace("Got msnfp_lasttransactiondate.");
				}
				else
				{
					contact.msnfp_LastTransactionDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasttransactiondate.");
				}
				if (queriedEntityRecord.Contains("msnfp_lasttransactionid") && queriedEntityRecord["msnfp_lasttransactionid"] != null)
				{
					contact.msnfp_LastTransactionId = ((EntityReference)queriedEntityRecord["msnfp_lasttransactionid"]).Id;
					localContext.TracingService.Trace("Got msnfp_lasttransactionid.");
				}
				else
				{
					contact.msnfp_LastTransactionId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lasttransactionid.");
				}
				if (queriedEntityRecord.Contains("msnfp_preferredlanguagecode") && queriedEntityRecord["msnfp_preferredlanguagecode"] != null)
				{
					contact.msnfp_PreferredLanguageCode = ((OptionSetValue)queriedEntityRecord["msnfp_preferredlanguagecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_preferredlanguagecode.");
				}
				else
				{
					contact.msnfp_PreferredLanguageCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_preferredlanguagecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_primarymembershipid") && queriedEntityRecord["msnfp_primarymembershipid"] != null)
				{
					contact.msnfp_PrimaryMembershipId = ((EntityReference)queriedEntityRecord["msnfp_primarymembershipid"]).Id;
					localContext.TracingService.Trace("Got msnfp_primarymembershipid.");
				}
				else
				{
					contact.msnfp_PrimaryMembershipId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_primarymembershipid.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptpreferencecode") && queriedEntityRecord["msnfp_receiptpreferencecode"] != null)
				{
					contact.msnfp_ReceiptPreferenceCode = ((OptionSetValue)queriedEntityRecord["msnfp_receiptpreferencecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_receiptpreferencecode.");
				}
				else
				{
					contact.msnfp_ReceiptPreferenceCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptpreferencecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_lifetimetransactions") && queriedEntityRecord["msnfp_sum_lifetimetransactions"] != null)
				{
					contact.msnfp_Sum_LifetimeTransactions = ((Money)queriedEntityRecord["msnfp_sum_lifetimetransactions"]).Value;
					localContext.TracingService.Trace("Got msnfp_primarymembershipid.");
				}
				else
				{
					contact.msnfp_Sum_LifetimeTransactions = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_lifetimetransactions.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone1typecode") && queriedEntityRecord["msnfp_telephone1typecode"] != null)
				{
					contact.msnfp_Telephone1TypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_telephone1typecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_telephone1typecode.");
				}
				else
				{
					contact.msnfp_Telephone1TypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone1typecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone2typecode") && queriedEntityRecord["msnfp_telephone2typecode"] != null)
				{
					contact.msnfp_Telephone2TypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_telephone2typecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_telephone2typecode.");
				}
				else
				{
					contact.msnfp_Telephone2TypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone2typecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone3typecode") && queriedEntityRecord["msnfp_telephone3typecode"] != null)
				{
					contact.msnfp_Telephone3TypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_telephone3typecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_telephone3typecode.");
				}
				else
				{
					contact.msnfp_Telephone3TypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone3typecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_upcomingbirthday") && queriedEntityRecord["msnfp_upcomingbirthday"] != null)
				{
					contact.msnfp_UpcomingBirthday = (DateTime)queriedEntityRecord["msnfp_upcomingbirthday"];
					localContext.TracingService.Trace("Got msnfp_upcomingbirthday.");
				}
				else
				{
					contact.msnfp_UpcomingBirthday = null;
					localContext.TracingService.Trace("Did NOT find msnfp_upcomingbirthday.");
				}
				if (queriedEntityRecord.Contains("msnfp_vip") && queriedEntityRecord["msnfp_vip"] != null)
				{
					contact.msnfp_Vip = (bool)queriedEntityRecord["msnfp_vip"];
					localContext.TracingService.Trace("Got msnfp_vip.");
				}
				else
				{
					contact.msnfp_Vip = null;
					localContext.TracingService.Trace("Did NOT find msnfp_vip.");
				}
				if (queriedEntityRecord.Contains("merged") && queriedEntityRecord["merged"] != null)
				{
					contact.Merged = (bool)queriedEntityRecord["merged"];
					localContext.TracingService.Trace("Got merged.");
				}
				else
				{
					contact.Merged = null;
					localContext.TracingService.Trace("Did NOT find merged.");
				}
				if (queriedEntityRecord.Contains("middlename") && queriedEntityRecord["middlename"] != null)
				{
					contact.MiddleName = (string)queriedEntityRecord["middlename"];
					localContext.TracingService.Trace("Got middlename.");
				}
				else
				{
					contact.MiddleName = null;
					localContext.TracingService.Trace("Did NOT find middlename.");
				}
				if (queriedEntityRecord.Contains("mobilephone") && queriedEntityRecord["mobilephone"] != null)
				{
					contact.MobilePhone = (string)queriedEntityRecord["mobilephone"];
					localContext.TracingService.Trace("Got mobilephone.");
				}
				else
				{
					contact.MobilePhone = null;
					localContext.TracingService.Trace("Did NOT find mobilephone.");
				}
				if (queriedEntityRecord.Contains("parentcustomerid") && queriedEntityRecord["parentcustomerid"] != null)
				{
					contact.ParentCustomerId = ((EntityReference)queriedEntityRecord["parentcustomerid"]).Id;
					if (((EntityReference)queriedEntityRecord["parentcustomerid"]).LogicalName.ToLower() == "contact")
					{
						contact.ParentCustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["parentcustomerid"]).LogicalName.ToLower() == "account")
					{
						contact.ParentCustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got parentcustomerid.");
				}
				else
				{
					contact.ParentCustomerId = null;
					contact.ParentCustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find parentcustomerid.");
				}
				if (queriedEntityRecord.Contains("salutation") && queriedEntityRecord["salutation"] != null)
				{
					contact.Salutation = (string)queriedEntityRecord["salutation"];
					localContext.TracingService.Trace("Got salutation.");
				}
				else
				{
					contact.Salutation = null;
					localContext.TracingService.Trace("Did NOT find salutation.");
				}
				if (queriedEntityRecord.Contains("suffix") && queriedEntityRecord["suffix"] != null)
				{
					contact.Suffix = (string)queriedEntityRecord["suffix"];
					localContext.TracingService.Trace("Got suffix.");
				}
				else
				{
					contact.Suffix = null;
					localContext.TracingService.Trace("Did NOT find suffix.");
				}
				if (queriedEntityRecord.Contains("telephone1") && queriedEntityRecord["telephone1"] != null)
				{
					contact.Telephone1 = (string)queriedEntityRecord["telephone1"];
					localContext.TracingService.Trace("Got telephone1.");
				}
				else
				{
					contact.Telephone1 = null;
					localContext.TracingService.Trace("Did NOT find telephone1.");
				}
				if (queriedEntityRecord.Contains("telephone2") && queriedEntityRecord["telephone2"] != null)
				{
					contact.Telephone2 = (string)queriedEntityRecord["telephone2"];
					localContext.TracingService.Trace("Got telephone2.");
				}
				else
				{
					contact.Telephone2 = null;
					localContext.TracingService.Trace("Did NOT find telephone2.");
				}
				if (queriedEntityRecord.Contains("telephone3") && queriedEntityRecord["telephone3"] != null)
				{
					contact.Telephone3 = (string)queriedEntityRecord["telephone3"];
					localContext.TracingService.Trace("Got telephone3.");
				}
				else
				{
					contact.Telephone3 = null;
					localContext.TracingService.Trace("Did NOT find telephone3.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					contact.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got TransactionCurrencyId.");
				}
				else
				{
					contact.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find TransactionCurrencyId.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					contact.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					contact.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					contact.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					contact.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (queriedEntityRecord.Contains("msnfp_householdid") && queriedEntityRecord["msnfp_householdid"] != null)
				{
					contact.msnfp_householdid = ((EntityReference)queriedEntityRecord["msnfp_householdid"]).Id;
					localContext.TracingService.Trace("Got msnfp_householdid.");
				}
				else
				{
					contact.msnfp_householdid = null;
					localContext.TracingService.Trace("Did NOT find msnfp_householdid.");
				}
				if (queriedEntityRecord.Contains("msnfp_year0_giving") && queriedEntityRecord["msnfp_year0_giving"] != null)
				{
					contact.msnfp_year0_giving = ((Money)queriedEntityRecord["msnfp_year0_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year0_giving.");
				}
				else
				{
					contact.msnfp_year0_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year0_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year1_giving") && queriedEntityRecord["msnfp_year1_giving"] != null)
				{
					contact.msnfp_year1_giving = ((Money)queriedEntityRecord["msnfp_year1_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year1_giving.");
				}
				else
				{
					contact.msnfp_year1_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year1_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year2_giving") && queriedEntityRecord["msnfp_year2_giving"] != null)
				{
					contact.msnfp_year2_giving = ((Money)queriedEntityRecord["msnfp_year2_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year2_giving.");
				}
				else
				{
					contact.msnfp_year2_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year2_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year3_giving") && queriedEntityRecord["msnfp_year3_giving"] != null)
				{
					contact.msnfp_year3_giving = ((Money)queriedEntityRecord["msnfp_year3_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year3_giving.");
				}
				else
				{
					contact.msnfp_year3_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year3_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_year4_giving") && queriedEntityRecord["msnfp_year4_giving"] != null)
				{
					contact.msnfp_year4_giving = ((Money)queriedEntityRecord["msnfp_year4_giving"]).Value;
					localContext.TracingService.Trace("Got msnfp_year4_giving.");
				}
				else
				{
					contact.msnfp_year4_giving = null;
					localContext.TracingService.Trace("Did NOT find msnfp_year4_giving.");
				}
				if (queriedEntityRecord.Contains("msnfp_lifetimegivingsum") && queriedEntityRecord["msnfp_lifetimegivingsum"] != null)
				{
					contact.msnfp_lifetimegivingsum = ((Money)queriedEntityRecord["msnfp_lifetimegivingsum"]).Value;
					localContext.TracingService.Trace("Got msnfp_lifetimegivingsum.");
				}
				else
				{
					contact.msnfp_lifetimegivingsum = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lifetimegivingsum.");
				}
				if (messageName == "Create")
				{
					contact.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					contact.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					contact.CreatedOn = null;
				}
				contact.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					contact.Deleted = true;
					contact.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					contact.Deleted = false;
					contact.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Contact));
				dataContractJsonSerializer.WriteObject(memoryStream, contact);
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

		private void SetUpHousehold(IOrganizationService service, string houseHoldSequence, ref Entity retreivedContact, Entity target, IPluginExecutionContext context, ITracingService tracingService)
		{
			Entity entity = new Entity(retreivedContact.LogicalName, retreivedContact.Id);
			bool attributeValue = target.GetAttributeValue<bool>("msnfp_deceased");
			if (target.GetAttributeValue<EntityReference>("msnfp_householdid") != null && target.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") == null)
			{
				EntityCollection members = GetMembers(service, target.GetAttributeValue<EntityReference>("msnfp_householdid").Id, 844060000, target.Id);
				if (members.Entities.Count > 0)
				{
					if (retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") != null && retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship").Value == 844060000)
					{
						UpdateEntity(service, new Entity(target.LogicalName, target.Id)
						{
							Attributes = 
							{
								new KeyValuePair<string, object>("msnfp_householdrelationship", new OptionSetValue(844060001))
							}
						});
					}
				}
				else if (retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") == null)
				{
					UpdateEntity(service, new Entity(target.LogicalName, target.Id)
					{
						Attributes = 
						{
							new KeyValuePair<string, object>("msnfp_householdrelationship", new OptionSetValue(844060000))
						}
					});
					tracingService.Trace("SetUpHousehold");
					service.Update(new Entity("account", target.GetAttributeValue<EntityReference>("msnfp_householdid").Id)
					{
						Attributes = 
						{
							new KeyValuePair<string, object>("primarycontactid", new EntityReference("contact", target.Id))
						}
					});
				}
				return;
			}
			Entity donationImportFromExecutionPipeLine = GetDonationImportFromExecutionPipeLine(context, service);
			if (attributeValue)
			{
				if (retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") != null && retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship").Value != 844060003)
				{
					entity["msnfp_householdrelationship"] = new OptionSetValue(844060003);
				}
				if (retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid") != null)
				{
					ResetHouseholdPrimaryContact(service, retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid"), retreivedContact.Id, tracingService);
				}
			}
			else if (retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") != null)
			{
				switch (retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship").Value)
				{
				case 844060000:
					if (retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid") == null && (donationImportFromExecutionPipeLine == null || (donationImportFromExecutionPipeLine?.GetAttributeValue<bool>("msnfp_createhousehold") ?? false)))
					{
						EntityReference value = (EntityReference)(entity["msnfp_householdid"] = Utilities.CreateHouseholdFromContact(service, retreivedContact.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship"), retreivedContact));
						retreivedContact["msnfp_householdid"] = value;
						if (donationImportFromExecutionPipeLine != null)
						{
							Entity entity2 = new Entity(donationImportFromExecutionPipeLine.LogicalName, donationImportFromExecutionPipeLine.Id);
							entity2["msnfp_householdid"] = value;
							UpdateEntity(service, entity2);
						}
					}
					else
					{
						UpdateHouseholdPrimaryMembers(service, retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid"), 844060000, retreivedContact.Id, tracingService);
					}
					if (retreivedContact.GetAttributeValue<bool>("msnfp_deceased"))
					{
						entity["msnfp_deceased"] = false;
					}
					break;
				case 844060003:
					if (retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid") != null)
					{
						ResetHouseholdPrimaryContact(service, retreivedContact.GetAttributeValue<EntityReference>("msnfp_householdid"), retreivedContact.Id, tracingService);
						if (!retreivedContact.GetAttributeValue<bool>("msnfp_deceased"))
						{
							entity["msnfp_deceased"] = true;
						}
					}
					break;
				case 844060001:
				case 844060002:
					if (retreivedContact.GetAttributeValue<bool>("msnfp_deceased"))
					{
						entity["msnfp_deceased"] = false;
					}
					break;
				}
			}
			UpdateEntity(service, entity);
		}

		private static void UpdateEntity(IOrganizationService service, Entity updatedTarget)
		{
			if (updatedTarget.Attributes.Any((KeyValuePair<string, object> a) => a.Key.Contains("msnfp")))
			{
				service.Update(updatedTarget);
			}
		}

		private static void ResetHouseholdPrimaryContact(IOrganizationService service, EntityReference householdReference, Guid contactId, ITracingService tracingService)
		{
			if (householdReference != null)
			{
				Entity entity = service.Retrieve(householdReference.LogicalName, householdReference.Id, new ColumnSet("primarycontactid"));
				if (entity.GetAttributeValue<EntityReference>("primarycontactid") != null && entity.GetAttributeValue<EntityReference>("primarycontactid").Id == contactId)
				{
					tracingService.Trace("ResetHouseholdPrimaryContact");
					entity["primarycontactid"] = null;
					service.Update(entity);
				}
			}
		}

		private void UpdateHouseholdPrimaryMembers(IOrganizationService service, EntityReference houseHoldRef, int relationshiptType, Guid currentContactGuid, ITracingService tracingService)
		{
			if (houseHoldRef == null)
			{
				return;
			}
			EntityCollection members = GetMembers(service, houseHoldRef.Id, relationshiptType, currentContactGuid);
			if (relationshiptType == 844060000)
			{
				members.Entities.ToList().ForEach(delegate(Entity c)
				{
					Entity entity = new Entity("contact")
					{
						["msnfp_householdrelationship"] = new OptionSetValue(844060001),
						Id = c.Id
					};
					service.Update(entity);
				});
				tracingService.Trace("UpdateHouseholdPrimaryMembers");
				service.Update(new Entity(houseHoldRef.LogicalName, houseHoldRef.Id)
				{
					Attributes = 
					{
						new KeyValuePair<string, object>("primarycontactid", new EntityReference("contact", currentContactGuid))
					}
				});
			}
		}

		private static EntityCollection GetMembers(IOrganizationService service, Guid houseHoldId, int relationshiptType, Guid currentContactGuid)
		{
			QueryExpression queryExpression = new QueryExpression("contact");
			queryExpression.NoLock = false;
			queryExpression.ColumnSet = new ColumnSet("contactid");
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_householdid", ConditionOperator.Equal, houseHoldId));
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_householdrelationship", ConditionOperator.Equal, relationshiptType));
			queryExpression.Criteria.AddCondition(new ConditionExpression("contactid", ConditionOperator.NotEqual, currentContactGuid));
			return service.RetrieveMultiple(queryExpression);
		}
	}
}
