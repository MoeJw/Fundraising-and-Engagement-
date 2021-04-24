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
	public class RegistrationPageCreate : PluginBase
	{
		public RegistrationPageCreate(string unsecure, string secure)
			: base(typeof(RegistrationPageCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered RegistrationPageCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering RegistrationPageCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_registration", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_registration", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting RegistrationPageCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_registrationid", "msnfp_firstname", "msnfp_lastname", "msnfp_email", "msnfp_telephone", "msnfp_address_line1", "msnfp_address_line2", "msnfp_address_city", "msnfp_address_province", "msnfp_address_postalcode", "msnfp_address_country", "msnfp_tableid", "msnfp_team", "msnfp_customerid", "msnfp_date", "msnfp_eventid", "msnfp_eventpackageid", "msnfp_ticketid", "msnfp_groupnotes", "msnfp_eventticketid", "msnfp_identifier", "msnfp_emailaddress1", "msnfp_telephone1", "msnfp_billing_city", "msnfp_billing_country", "msnfp_billing_line1", "msnfp_billing_line2", "msnfp_billing_line3", "msnfp_billing_postalcode", "msnfp_billing_stateorprovince", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Registration";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_registrationid"].ToString());
				MSNFP_Registration mSNFP_Registration = new MSNFP_Registration();
				mSNFP_Registration.RegistrationId = (Guid)queriedEntityRecord["msnfp_registrationid"];
				if (queriedEntityRecord.Contains("msnfp_firstname") && queriedEntityRecord["msnfp_firstname"] != null)
				{
					mSNFP_Registration.FirstName = (string)queriedEntityRecord["msnfp_firstname"];
					localContext.TracingService.Trace("Got msnfp_firstname.");
				}
				else
				{
					mSNFP_Registration.FirstName = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_firstname.");
				}
				if (queriedEntityRecord.Contains("msnfp_lastname") && queriedEntityRecord["msnfp_lastname"] != null)
				{
					mSNFP_Registration.LastName = (string)queriedEntityRecord["msnfp_lastname"];
					localContext.TracingService.Trace("Got msnfp_lastname.");
				}
				else
				{
					mSNFP_Registration.LastName = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_lastname.");
				}
				if (queriedEntityRecord.Contains("msnfp_email") && queriedEntityRecord["msnfp_email"] != null)
				{
					mSNFP_Registration.Email = (string)queriedEntityRecord["msnfp_email"];
					localContext.TracingService.Trace("Got msnfp_email.");
				}
				else
				{
					mSNFP_Registration.Email = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_email.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone") && queriedEntityRecord["msnfp_telephone"] != null)
				{
					mSNFP_Registration.Telephone = (string)queriedEntityRecord["msnfp_telephone"];
					localContext.TracingService.Trace("Got msnfp_telephone.");
				}
				else
				{
					mSNFP_Registration.Telephone = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone.");
				}
				if (queriedEntityRecord.Contains("msnfp_address_line1") && queriedEntityRecord["msnfp_address_line1"] != null)
				{
					mSNFP_Registration.Address_Line1 = (string)queriedEntityRecord["msnfp_address_line1"];
					localContext.TracingService.Trace("Got msnfp_address_line1.");
				}
				else
				{
					mSNFP_Registration.Address_Line1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_address_line1.");
				}
				if (queriedEntityRecord.Contains("msnfp_address_line2") && queriedEntityRecord["msnfp_address_line2"] != null)
				{
					mSNFP_Registration.Address_Line2 = (string)queriedEntityRecord["msnfp_address_line2"];
					localContext.TracingService.Trace("Got msnfp_address_line2.");
				}
				else
				{
					mSNFP_Registration.Address_Line2 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_address_line2.");
				}
				if (queriedEntityRecord.Contains("msnfp_address_city") && queriedEntityRecord["msnfp_address_city"] != null)
				{
					mSNFP_Registration.Address_City = (string)queriedEntityRecord["msnfp_address_city"];
					localContext.TracingService.Trace("Got msnfp_address_city.");
				}
				else
				{
					mSNFP_Registration.Address_City = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_address_city.");
				}
				if (queriedEntityRecord.Contains("msnfp_address_province") && queriedEntityRecord["msnfp_address_province"] != null)
				{
					mSNFP_Registration.Address_Province = (string)queriedEntityRecord["msnfp_address_province"];
					localContext.TracingService.Trace("Got msnfp_address_province.");
				}
				else
				{
					mSNFP_Registration.Address_Province = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_address_province.");
				}
				if (queriedEntityRecord.Contains("msnfp_address_postalcode") && queriedEntityRecord["msnfp_address_postalcode"] != null)
				{
					mSNFP_Registration.Address_PostalCode = (string)queriedEntityRecord["msnfp_address_postalcode"];
					localContext.TracingService.Trace("Got msnfp_address_postalcode.");
				}
				else
				{
					mSNFP_Registration.Address_PostalCode = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_address_postalcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_address_country") && queriedEntityRecord["msnfp_address_country"] != null)
				{
					mSNFP_Registration.Address_Country = (string)queriedEntityRecord["msnfp_address_country"];
					localContext.TracingService.Trace("Got msnfp_address_country.");
				}
				else
				{
					mSNFP_Registration.Address_Country = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_address_country.");
				}
				if (queriedEntityRecord.Contains("msnfp_tableid") && queriedEntityRecord["msnfp_tableid"] != null)
				{
					mSNFP_Registration.TableId = ((EntityReference)queriedEntityRecord["msnfp_tableid"]).Id;
					localContext.TracingService.Trace("Got msnfp_tableid");
				}
				else
				{
					mSNFP_Registration.TableId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_tableid.");
				}
				if (queriedEntityRecord.Contains("msnfp_team") && queriedEntityRecord["msnfp_team"] != null)
				{
					mSNFP_Registration.Team = (string)queriedEntityRecord["msnfp_team"];
					localContext.TracingService.Trace("Got msnfp_team.");
				}
				else
				{
					mSNFP_Registration.Team = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_team.");
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Registration.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_Registration.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_Registration.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid");
				}
				else
				{
					mSNFP_Registration.CustomerId = null;
					mSNFP_Registration.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_date") && queriedEntityRecord["msnfp_date"] != null)
				{
					mSNFP_Registration.Date = (DateTime)queriedEntityRecord["msnfp_date"];
					localContext.TracingService.Trace("Got msnfp_date.");
				}
				else
				{
					mSNFP_Registration.Date = null;
					localContext.TracingService.Trace("Did NOT find msnfp_date.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_Registration.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid .");
				}
				else
				{
					mSNFP_Registration.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventpackageid") && queriedEntityRecord["msnfp_eventpackageid"] != null)
				{
					mSNFP_Registration.EventPackageId = ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpackageid.");
				}
				else
				{
					mSNFP_Registration.EventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_ticketid") && queriedEntityRecord["msnfp_ticketid"] != null)
				{
					mSNFP_Registration.TicketId = ((EntityReference)queriedEntityRecord["msnfp_ticketid"]).Id;
					localContext.TracingService.Trace("Got msnfp_ticketid.");
				}
				else
				{
					mSNFP_Registration.TicketId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ticketid.");
				}
				if (queriedEntityRecord.Contains("msnfp_groupnotes") && queriedEntityRecord["msnfp_groupnotes"] != null)
				{
					mSNFP_Registration.GroupNotes = (string)queriedEntityRecord["msnfp_groupnotes"];
					localContext.TracingService.Trace("Got msnfp_groupnotes.");
				}
				else
				{
					mSNFP_Registration.GroupNotes = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_groupnotes.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventticketid") && queriedEntityRecord["msnfp_eventticketid"] != null)
				{
					mSNFP_Registration.EventTicketId = ((EntityReference)queriedEntityRecord["msnfp_eventticketid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventticketid.");
				}
				else
				{
					mSNFP_Registration.EventTicketId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventticketid.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_Registration.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_Registration.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_emailaddress1") && queriedEntityRecord["msnfp_emailaddress1"] != null)
				{
					mSNFP_Registration.msnfp_Emailaddress1 = (string)queriedEntityRecord["msnfp_emailaddress1"];
					localContext.TracingService.Trace("Got msnfp_emailaddress1.");
				}
				else
				{
					mSNFP_Registration.msnfp_Emailaddress1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_emailaddress1.");
				}
				if (queriedEntityRecord.Contains("msnfp_telephone1") && queriedEntityRecord["msnfp_telephone1"] != null)
				{
					mSNFP_Registration.msnfp_Telephone1 = (string)queriedEntityRecord["msnfp_telephone1"];
					localContext.TracingService.Trace("Got msnfp_telephone1.");
				}
				else
				{
					mSNFP_Registration.msnfp_Telephone1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_telephone1.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_city") && queriedEntityRecord["msnfp_billing_city"] != null)
				{
					mSNFP_Registration.msnfp_Billing_City = (string)queriedEntityRecord["msnfp_billing_city"];
					localContext.TracingService.Trace("Got msnfp_billing_city.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_City = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_city.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_country") && queriedEntityRecord["msnfp_billing_country"] != null)
				{
					mSNFP_Registration.msnfp_Billing_Country = (string)queriedEntityRecord["msnfp_billing_country"];
					localContext.TracingService.Trace("Got msnfp_billing_country.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_Country = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_country.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line1") && queriedEntityRecord["msnfp_billing_line1"] != null)
				{
					mSNFP_Registration.msnfp_Billing_Line1 = (string)queriedEntityRecord["msnfp_billing_line1"];
					localContext.TracingService.Trace("Got msnfp_billing_line1.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_Line1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line1.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line2") && queriedEntityRecord["msnfp_billing_line2"] != null)
				{
					mSNFP_Registration.msnfp_Billing_Line2 = (string)queriedEntityRecord["msnfp_billing_line2"];
					localContext.TracingService.Trace("Got msnfp_billing_line2.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_Line2 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line2.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_line3") && queriedEntityRecord["msnfp_billing_line3"] != null)
				{
					mSNFP_Registration.msnfp_Billing_Line3 = (string)queriedEntityRecord["msnfp_billing_line3"];
					localContext.TracingService.Trace("Got msnfp_billing_line3.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_Line3 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_line3.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_postalcode") && queriedEntityRecord["msnfp_billing_postalcode"] != null)
				{
					mSNFP_Registration.msnfp_Billing_Postalcode = (string)queriedEntityRecord["msnfp_billing_postalcode"];
					localContext.TracingService.Trace("Got msnfp_billing_postalcode.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_Postalcode = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_postalcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_billing_stateorprovince") && queriedEntityRecord["msnfp_billing_stateorprovince"] != null)
				{
					mSNFP_Registration.msnfp_Billing_StateorProvince = (string)queriedEntityRecord["msnfp_billing_stateorprovince"];
					localContext.TracingService.Trace("Got msnfp_billing_stateorprovince.");
				}
				else
				{
					mSNFP_Registration.msnfp_Billing_StateorProvince = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_billing_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Registration.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Registration.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Registration.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Registration.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Registration.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Registration.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Registration.CreatedOn = null;
				}
				mSNFP_Registration.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Registration.Deleted = true;
					mSNFP_Registration.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Registration.Deleted = false;
					mSNFP_Registration.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Registration));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Registration);
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
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting plugin.");
			}
		}
	}
}
