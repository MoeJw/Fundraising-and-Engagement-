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
	public class EventProductCreate : PluginBase
	{
		public EventProductCreate(string unsecure, string secure)
			: base(typeof(EventProductCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered EventProductCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(organizationService);
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
				localContext.TracingService.Trace("---------Entering EventProductCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_eventproduct", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						if (entity3.Contains("msnfp_quantity") && entity3["msnfp_quantity"] != null)
						{
							entity3["msnfp_sum_available"] = (int)entity3["msnfp_quantity"];
							organizationService.Update(entity3);
						}
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						UpdateEventTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_eventproduct", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting EventProductCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventproductid", "msnfp_amount", "msnfp_description", "msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_eventid", "msnfp_maxproducts", "msnfp_sum_available", "msnfp_quantity", "msnfp_restrictperregistration", "msnfp_amount_tax", "msnfp_val_sold", "msnfp_identifier", "msnfp_sum_sold", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void UpdateEventTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTotals---------");
			int num = 0;
			decimal value = default(decimal);
			Entity entity = service.Retrieve("msnfp_event", ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id, new ColumnSet("msnfp_eventid", "msnfp_sum_products", "msnfp_count_products"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_eventproduct")
				where ((EntityReference)a["msnfp_eventid"]).Id == ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id && ((OptionSetValue)a["statecode"]).Value == 0 && ((OptionSetValue)a["statuscode"]).Value != 844060000
				select a).ToList();
			if (list.Count > 0)
			{
				foreach (Entity item in list)
				{
					if (item.Contains("msnfp_sum_sold"))
					{
						num += (int)item["msnfp_sum_sold"];
					}
					if (item.Contains("msnfp_val_sold"))
					{
						value += ((Money)item["msnfp_val_sold"]).Value;
					}
				}
			}
			entity["msnfp_count_products"] = num;
			entity["msnfp_sum_products"] = new Money(value);
			decimal value2 = Utilities.CalculateEventTotalRevenue(entity, service, orgSvcContext, localContext.TracingService);
			entity["msnfp_sum_total"] = new Money(value2);
			service.Update(entity);
			localContext.TracingService.Trace("Event Record Updated");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "EventProduct";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_eventproductid"].ToString());
				MSNFP_EventProduct mSNFP_EventProduct = new MSNFP_EventProduct();
				mSNFP_EventProduct.EventProductId = (Guid)queriedEntityRecord["msnfp_eventproductid"];
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_EventProduct.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount");
				}
				else
				{
					mSNFP_EventProduct.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_description") && queriedEntityRecord["msnfp_description"] != null)
				{
					mSNFP_EventProduct.Description = (string)queriedEntityRecord["msnfp_description"];
					localContext.TracingService.Trace("Got msnfp_description.");
				}
				else
				{
					mSNFP_EventProduct.Description = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_description.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_EventProduct.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_EventProduct.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_EventProduct.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_EventProduct.AmountNonReceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_EventProduct.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_EventProduct.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_maxproducts") && queriedEntityRecord["msnfp_maxproducts"] != null)
				{
					mSNFP_EventProduct.MaxProducts = (int)queriedEntityRecord["msnfp_maxproducts"];
					localContext.TracingService.Trace("Got msnfp_maxproducts.");
				}
				else
				{
					mSNFP_EventProduct.MaxProducts = null;
					localContext.TracingService.Trace("Did NOT find msnfp_maxproducts.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_available") && queriedEntityRecord["msnfp_sum_available"] != null)
				{
					mSNFP_EventProduct.ValAvailable = (int)queriedEntityRecord["msnfp_sum_available"];
					localContext.TracingService.Trace("Got msnfp_sum_available.");
				}
				else
				{
					mSNFP_EventProduct.ValAvailable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_available.");
				}
				if (queriedEntityRecord.Contains("msnfp_quantity") && queriedEntityRecord["msnfp_quantity"] != null)
				{
					mSNFP_EventProduct.Quantity = (int)queriedEntityRecord["msnfp_quantity"];
					localContext.TracingService.Trace("Got msnfp_quantity.");
				}
				else
				{
					mSNFP_EventProduct.Quantity = null;
					localContext.TracingService.Trace("Did NOT find msnfp_quantity.");
				}
				if (queriedEntityRecord.Contains("msnfp_restrictperregistration") && queriedEntityRecord["msnfp_restrictperregistration"] != null)
				{
					mSNFP_EventProduct.RestrictPerRegistration = (bool)queriedEntityRecord["msnfp_restrictperregistration"];
					localContext.TracingService.Trace("Got msnfp_restrictperregistration.");
				}
				else
				{
					mSNFP_EventProduct.RestrictPerRegistration = null;
					localContext.TracingService.Trace("Did NOT find msnfp_restrictperregistration.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_sold") && queriedEntityRecord["msnfp_sum_sold"] != null)
				{
					mSNFP_EventProduct.ValSold = (int)queriedEntityRecord["msnfp_sum_sold"];
					localContext.TracingService.Trace("Got msnfp_sum_sold.");
				}
				else
				{
					mSNFP_EventProduct.ValSold = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_sold.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_EventProduct.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_EventProduct.AmountTax = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_val_sold") && queriedEntityRecord["msnfp_val_sold"] != null)
				{
					mSNFP_EventProduct.SumSold = ((Money)queriedEntityRecord["msnfp_val_sold"]).Value;
					localContext.TracingService.Trace("Got msnfp_val_sold.");
				}
				else
				{
					mSNFP_EventProduct.SumSold = null;
					localContext.TracingService.Trace("Did NOT find msnfp_val_sold.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_EventProduct.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_EventProduct.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_sum_sold") && queriedEntityRecord["msnfp_sum_sold"] != null)
				{
					mSNFP_EventProduct.ValSold = (int)queriedEntityRecord["msnfp_sum_sold"];
					localContext.TracingService.Trace("Got msnfp_sum_sold.");
				}
				else
				{
					mSNFP_EventProduct.ValSold = null;
					localContext.TracingService.Trace("Did NOT find msnfp_sum_sold.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_EventProduct.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_EventProduct.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_EventProduct.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_EventProduct.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_EventProduct.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_EventProduct.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_EventProduct.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_EventProduct.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_EventProduct.CreatedOn = null;
				}
				mSNFP_EventProduct.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_EventProduct.Deleted = true;
					mSNFP_EventProduct.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_EventProduct.Deleted = false;
					mSNFP_EventProduct.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_EventProduct));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_EventProduct);
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
