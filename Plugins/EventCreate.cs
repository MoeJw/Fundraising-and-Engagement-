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
	public class EventCreate : PluginBase
	{
		public EventCreate(string unsecure, string secure)
			: base(typeof(EventCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventCreate.cs---------");
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
			entity = Utilities.GetConfigurationRecordByMessageName(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering EventCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_event", entity3.Id, GetColumnSet());
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_event", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting EventCreate.cs---------");
		}

		private void DeleteEventOrdersOnListDeactivation(IOrganizationService service, EntityReference entRefEvent, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("Inside DeleteEventOrdersOnListDeactivation Method.");
			QueryExpression queryExpression = new QueryExpression("msnfp_eventorder");
			queryExpression.Criteria.AddCondition("msnfp_fromeventid", ConditionOperator.Equal, entRefEvent.Id);
			queryExpression.ColumnSet.AddColumns("msnfp_toeventlistid", "msnfp_fromeventlistid");
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			if (entityCollection.Entities.Count <= 0)
			{
				return;
			}
			localContext.TracingService.Trace("Delete Event Order count- " + entityCollection.Entities.Count);
			foreach (Entity entity in entityCollection.Entities)
			{
				service.Delete(entity.LogicalName, entity.Id);
				ReOrderEventOrder(service, (EntityReference)entity["msnfp_toeventlistid"], localContext);
			}
		}

		private void ReOrderEventOrder(IOrganizationService service, EntityReference entRefToEventList, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("ReOrder Event Order");
			QueryExpression queryExpression = new QueryExpression("msnfp_eventorder");
			queryExpression.ColumnSet.AddColumns("msnfp_order");
			queryExpression.Criteria.AddCondition("msnfp_toeventlistid", ConditionOperator.Equal, entRefToEventList.Id);
			queryExpression.AddOrder("msnfp_order", OrderType.Ascending);
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			int num = 1;
			if (entityCollection.Entities.Count <= 0)
			{
				return;
			}
			foreach (Entity entity in entityCollection.Entities)
			{
				if (entity.Contains("msnfp_order") && (int)entity["msnfp_order"] != num)
				{
					entity["msnfp_order"] = num;
					service.Update(entity);
				}
				num++;
			}
		}

		private static ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventid", "msnfp_goal", "msnfp_capacity", "msnfp_description", "msnfp_name", "msnfp_amount", "msnfp_eventtypecode", "msnfp_identifier", "msnfp_map_line1", "msnfp_map_line2", "msnfp_map_line3", "msnfp_map_city", "msnfp_stateorprovince", "msnfp_map_postalcode", "msnfp_map_country", "msnfp_proposedend", "msnfp_proposedstart", "msnfp_campaignid", "msnfp_appealid", "msnfp_packageid", "msnfp_designationid", "msnfp_configurationid", "msnfp_venueid", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Event";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_eventid"].ToString());
				MSNFP_Event mSNFP_Event = new MSNFP_Event
				{
					EventId = (Guid)queriedEntityRecord["msnfp_eventid"]
				};
				localContext.TracingService.Trace("Got myHttpWebResponse: -1");
				if (queriedEntityRecord.Contains("msnfp_goal") && queriedEntityRecord["msnfp_goal"] != null)
				{
					mSNFP_Event.Goal = ((Money)queriedEntityRecord["msnfp_goal"]).Value;
					localContext.TracingService.Trace("Got msnfp_goal");
				}
				else
				{
					mSNFP_Event.Goal = null;
					localContext.TracingService.Trace("Did NOT find msnfp_goal.");
				}
				if (queriedEntityRecord.Contains("msnfp_capacity") && queriedEntityRecord["msnfp_capacity"] != null)
				{
					mSNFP_Event.Capacity = (int)queriedEntityRecord["msnfp_capacity"];
					localContext.TracingService.Trace("Got msnfp_capacity");
				}
				else
				{
					mSNFP_Event.Capacity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_capacity.");
				}
				if (queriedEntityRecord.Contains("msnfp_description") && queriedEntityRecord["msnfp_description"] != null)
				{
					mSNFP_Event.Description = (string)queriedEntityRecord["msnfp_description"];
					localContext.TracingService.Trace("Got msnfp_description");
				}
				else
				{
					mSNFP_Event.Description = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_description.");
				}
				if (queriedEntityRecord.Contains("msnfp_name") && queriedEntityRecord["msnfp_name"] != null)
				{
					mSNFP_Event.Name = (string)queriedEntityRecord["msnfp_name"];
					localContext.TracingService.Trace("Got msnfp_name");
				}
				else
				{
					mSNFP_Event.Name = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_name.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Event.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount");
				}
				else
				{
					mSNFP_Event.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventtypecode") && queriedEntityRecord["msnfp_eventtypecode"] != null)
				{
					mSNFP_Event.EventTypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_eventtypecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_eventtypecode");
				}
				else
				{
					mSNFP_Event.EventTypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventtypecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_Event.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier");
				}
				else
				{
					mSNFP_Event.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_map_line1") && queriedEntityRecord["msnfp_map_line1"] != null)
				{
					mSNFP_Event.MapLine1 = (string)queriedEntityRecord["msnfp_map_line1"];
					localContext.TracingService.Trace("Got msnfp_map_line1");
				}
				else
				{
					mSNFP_Event.MapLine1 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_map_line1.");
				}
				if (queriedEntityRecord.Contains("msnfp_map_line2") && queriedEntityRecord["msnfp_map_line2"] != null)
				{
					mSNFP_Event.MapLine2 = (string)queriedEntityRecord["msnfp_map_line2"];
					localContext.TracingService.Trace("Got msnfp_map_line2");
				}
				else
				{
					mSNFP_Event.MapLine2 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_map_line2.");
				}
				if (queriedEntityRecord.Contains("msnfp_map_line3") && queriedEntityRecord["msnfp_map_line3"] != null)
				{
					mSNFP_Event.MapLine3 = (string)queriedEntityRecord["msnfp_map_line3"];
					localContext.TracingService.Trace("Got msnfp_map_line3");
				}
				else
				{
					mSNFP_Event.MapLine3 = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_map_line3.");
				}
				if (queriedEntityRecord.Contains("msnfp_map_city") && queriedEntityRecord["msnfp_map_city"] != null)
				{
					mSNFP_Event.MapCity = (string)queriedEntityRecord["msnfp_map_city"];
					localContext.TracingService.Trace("Got msnfp_map_city");
				}
				else
				{
					mSNFP_Event.MapCity = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_map_city.");
				}
				if (queriedEntityRecord.Contains("msnfp_stateorprovince") && queriedEntityRecord["msnfp_stateorprovince"] != null)
				{
					mSNFP_Event.MapStateOrProvince = (string)queriedEntityRecord["msnfp_stateorprovince"];
					localContext.TracingService.Trace("Got msnfp_stateorprovince");
				}
				else
				{
					mSNFP_Event.MapStateOrProvince = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_stateorprovince.");
				}
				if (queriedEntityRecord.Contains("msnfp_map_postalcode") && queriedEntityRecord["msnfp_map_postalcode"] != null)
				{
					mSNFP_Event.MapPostalCode = (string)queriedEntityRecord["msnfp_map_postalcode"];
					localContext.TracingService.Trace("Got msnfp_map_postalcode");
				}
				else
				{
					mSNFP_Event.MapPostalCode = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_map_postalcode.");
				}
				if (queriedEntityRecord.Contains("msnfp_map_country") && queriedEntityRecord["msnfp_map_country"] != null)
				{
					mSNFP_Event.MapCountry = (string)queriedEntityRecord["msnfp_map_country"];
					localContext.TracingService.Trace("Got msnfp_map_country");
				}
				else
				{
					mSNFP_Event.MapCountry = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_map_country.");
				}
				if (queriedEntityRecord.Contains("msnfp_proposedend") && queriedEntityRecord["msnfp_proposedend"] != null)
				{
					mSNFP_Event.ProposedEnd = (DateTime)queriedEntityRecord["msnfp_proposedend"];
					localContext.TracingService.Trace("Got msnfp_proposedend");
				}
				else
				{
					mSNFP_Event.ProposedEnd = null;
					localContext.TracingService.Trace("Did NOT find msnfp_proposedend.");
				}
				if (queriedEntityRecord.Contains("msnfp_proposedstart") && queriedEntityRecord["msnfp_proposedstart"] != null)
				{
					mSNFP_Event.ProposedStart = (DateTime)queriedEntityRecord["msnfp_proposedstart"];
					localContext.TracingService.Trace("Got msnfp_proposedstart");
				}
				else
				{
					mSNFP_Event.ProposedStart = null;
					localContext.TracingService.Trace("Did NOT find msnfp_proposedstart.");
				}
				if (queriedEntityRecord.Contains("msnfp_campaignid") && queriedEntityRecord["msnfp_campaignid"] != null)
				{
					mSNFP_Event.CampaignId = ((EntityReference)queriedEntityRecord["msnfp_campaignid"]).Id;
					localContext.TracingService.Trace("Got msnfp_campaignid");
				}
				else
				{
					mSNFP_Event.CampaignId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_campaignid.");
				}
				if (queriedEntityRecord.Contains("msnfp_appealid") && queriedEntityRecord["msnfp_appealid"] != null)
				{
					mSNFP_Event.AppealId = ((EntityReference)queriedEntityRecord["msnfp_appealid"]).Id;
					localContext.TracingService.Trace("Got msnfp_appealid");
				}
				else
				{
					mSNFP_Event.AppealId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_appealid.");
				}
				if (queriedEntityRecord.Contains("msnfp_packageid") && queriedEntityRecord["msnfp_packageid"] != null)
				{
					mSNFP_Event.PackageId = ((EntityReference)queriedEntityRecord["msnfp_packageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_packageid");
				}
				else
				{
					mSNFP_Event.PackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_packageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_designationid") && queriedEntityRecord["msnfp_designationid"] != null)
				{
					mSNFP_Event.DesignationId = ((EntityReference)queriedEntityRecord["msnfp_designationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_designationid");
				}
				else
				{
					mSNFP_Event.DesignationId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_designationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_configurationid") && queriedEntityRecord["msnfp_configurationid"] != null)
				{
					mSNFP_Event.ConfigurationId = ((EntityReference)queriedEntityRecord["msnfp_configurationid"]).Id;
					localContext.TracingService.Trace("Got msnfp_configurationid");
				}
				else
				{
					mSNFP_Event.ConfigurationId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_configurationid.");
				}
				if (queriedEntityRecord.Contains("msnfp_venueid") && queriedEntityRecord["msnfp_venueid"] != null)
				{
					mSNFP_Event.VenueId = ((EntityReference)queriedEntityRecord["msnfp_venueid"]).Id;
					localContext.TracingService.Trace("Got msnfp_venueid");
				}
				else
				{
					mSNFP_Event.VenueId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_venueid.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_Event.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_Event.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Event.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Event.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Event.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Event.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Event.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Event.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Event.CreatedOn = null;
				}
				mSNFP_Event.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Event.Deleted = true;
					mSNFP_Event.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Event.Deleted = false;
					mSNFP_Event.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Event));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Event);
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
