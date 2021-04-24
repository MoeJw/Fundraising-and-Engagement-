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
	public class TransactionCurrencyCreate : PluginBase
	{
		public TransactionCurrencyCreate(string unsecure, string secure)
			: base(typeof(TransactionCurrencyCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered TransactionCurrencyCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			Entity queriedEntityRecord = null;
			string messageName = pluginExecutionContext.MessageName;
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			Entity configurationRecordByUser = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			object obj = pluginExecutionContext.InputParameters["Target"];
			EntityReference entityReference = obj as EntityReference;
			object obj2 = ((entityReference != null) ? new Entity(entityReference.LogicalName, entityReference.Id) : obj);
			if (obj2 is Entity)
			{
				localContext.TracingService.Trace("---------Entering TransactionCurrencyCreate.cs Main Function---------");
				Entity entity2 = (Entity)obj2;
				if (messageName == "Update" || messageName == "msnfp_ActionSyncCurrency")
				{
					queriedEntityRecord = organizationService.Retrieve("transactioncurrency", entity2.Id, GetColumnSet());
				}
				if (entity2 != null)
				{
					if (messageName == "Create")
					{
						AddOrUpdateThisRecordWithAzure(entity2, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update" || messageName == "msnfp_ActionSyncCurrency")
					{
						AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
					}
				}
				else
				{
					localContext.TracingService.Trace("Target record not found. Exiting workflow.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("transactioncurrency", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, configurationRecordByUser, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting TransactionCurrencyCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("transactioncurrencyid", "currencyname", "currencysymbol", "isocurrencycode", "statuscode", "statecode", "exchangerate", "organizationid", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "TransactionCurrency";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["transactioncurrencyid"].ToString());
				TransactionCurrency transactionCurrency = new TransactionCurrency();
				transactionCurrency.TransactionCurrencyId = (Guid)queriedEntityRecord["transactioncurrencyid"];
				transactionCurrency.CurrencyName = (queriedEntityRecord.Contains("currencyname") ? ((string)queriedEntityRecord["currencyname"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + transactionCurrency.CurrencyName);
				if (queriedEntityRecord.Contains("currencysymbol") && queriedEntityRecord["currencysymbol"] != null)
				{
					transactionCurrency.CurrencySymbol = (string)queriedEntityRecord["currencysymbol"];
					localContext.TracingService.Trace("Got currencysymbol.");
				}
				else
				{
					transactionCurrency.CurrencySymbol = null;
					localContext.TracingService.Trace("Did NOT find currencysymbol.");
				}
				if (queriedEntityRecord.Contains("isocurrencycode") && queriedEntityRecord["isocurrencycode"] != null)
				{
					transactionCurrency.IsoCurrencyCode = (string)queriedEntityRecord["isocurrencycode"];
					localContext.TracingService.Trace("Got currenisocurrencycodecysymbol.");
				}
				else
				{
					transactionCurrency.IsoCurrencyCode = null;
					localContext.TracingService.Trace("Did NOT find isocurrencycode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					transactionCurrency.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					transactionCurrency.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					transactionCurrency.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					transactionCurrency.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("exchangerate") && queriedEntityRecord["exchangerate"] != null)
				{
					transactionCurrency.ExchangeRate = (decimal)queriedEntityRecord["exchangerate"];
					localContext.TracingService.Trace("Got exchangerate.");
				}
				else
				{
					transactionCurrency.ExchangeRate = null;
					localContext.TracingService.Trace("Did NOT find exchangerate.");
				}
				if (queriedEntityRecord.Contains("organizationid") && queriedEntityRecord["organizationid"] != null)
				{
					localContext.TracingService.Trace("Querying for base currency = " + ((EntityReference)queriedEntityRecord["organizationid"]).Id.ToString());
					ColumnSet columnSet = new ColumnSet("basecurrencyid");
					Entity entity = service.Retrieve("organization", ((EntityReference)queriedEntityRecord["organizationid"]).Id, columnSet);
					Guid id = ((EntityReference)entity["basecurrencyid"]).Id;
					localContext.TracingService.Trace("CurrencyId = " + id.ToString());
					if (id == (Guid)queriedEntityRecord["transactioncurrencyid"])
					{
						transactionCurrency.IsBase = true;
					}
					else
					{
						transactionCurrency.IsBase = false;
					}
					ITracingService tracingService = localContext.TracingService;
					Guid guid = id;
					tracingService.Trace("Got base currency = " + guid.ToString());
					localContext.TracingService.Trace("jsonDataObj.IsBase = " + transactionCurrency.IsBase);
				}
				else
				{
					transactionCurrency.IsBase = null;
					localContext.TracingService.Trace("Did NOT find isbase.");
				}
				if (messageName == "Create")
				{
					transactionCurrency.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					transactionCurrency.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					transactionCurrency.CreatedOn = null;
				}
				transactionCurrency.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					transactionCurrency.Deleted = true;
					transactionCurrency.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					transactionCurrency.Deleted = false;
					transactionCurrency.DeletedDate = null;
				}
				localContext.TracingService.Trace("JSON object created");
				int num;
				switch (messageName)
				{
				case "Create":
					text2 = text2 + text + "/Create" + text;
					break;
				default:
					num = ((messageName == "msnfp_ActionSyncCurrency") ? 1 : 0);
					goto IL_06e9;
				case "Update":
				case "Delete":
					{
						num = 1;
						goto IL_06e9;
					}
					IL_06e9:
					if (num != 0)
					{
						text2 = text2 + text + "/Update" + text;
					}
					break;
				}
				MemoryStream memoryStream = new MemoryStream();
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(TransactionCurrency));
				dataContractJsonSerializer.WriteObject(memoryStream, transactionCurrency);
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
