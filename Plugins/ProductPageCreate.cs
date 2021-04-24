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
	public class ProductPageCreate : PluginBase
	{
		public ProductPageCreate(string unsecure, string secure)
			: base(typeof(ProductPageCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered ProductPageCreate.cs---------");
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
				localContext.TracingService.Trace("---------Entering ProductPageCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_product", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					if (messageName == "Create")
					{
						UpdateEventPackageProductTotals(entity3, orgSvcContext, organizationService, localContext);
						UpdateEventProductTotals(entity3, orgSvcContext, organizationService, localContext);
						AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
					}
					else if (messageName == "Update")
					{
						UpdateEventPackageProductTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
						UpdateEventProductTotals(queriedEntityRecord, orgSvcContext, organizationService, localContext);
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
				queriedEntityRecord = organizationService.Retrieve("msnfp_product", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting ProductPageCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_eventpackageid", "msnfp_eventproductid", "msnfp_productid", "msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount", "msnfp_amount_tax", "msnfp_customerid", "msnfp_date", "msnfp_eventid", "msnfp_eventpackageid", "msnfp_eventproductid", "msnfp_identifier", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Product";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_productid"].ToString());
				MSNFP_Product mSNFP_Product = new MSNFP_Product();
				mSNFP_Product.ProductId = (Guid)queriedEntityRecord["msnfp_productid"];
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_Product.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted");
				}
				else
				{
					mSNFP_Product.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_Product.AmountNonreceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Product.AmountNonreceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Product.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_Product.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_Product.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_Product.AmountTax = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Product.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_Product.CustomerId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_date") && queriedEntityRecord["msnfp_date"] != null)
				{
					mSNFP_Product.Date = (DateTime)queriedEntityRecord["msnfp_date"];
					localContext.TracingService.Trace("Got msnfp_date.");
				}
				else
				{
					mSNFP_Product.Date = null;
					localContext.TracingService.Trace("Did NOT find msnfp_date.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventid") && queriedEntityRecord["msnfp_eventid"] != null)
				{
					mSNFP_Product.EventId = ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventid.");
				}
				else
				{
					mSNFP_Product.EventId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventpackageid") && queriedEntityRecord["msnfp_eventpackageid"] != null)
				{
					mSNFP_Product.EventPackageId = ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventpackageid.");
				}
				else
				{
					mSNFP_Product.EventPackageId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid.");
				}
				if (queriedEntityRecord.Contains("msnfp_eventproductid") && queriedEntityRecord["msnfp_eventproductid"] != null)
				{
					mSNFP_Product.EventProductId = ((EntityReference)queriedEntityRecord["msnfp_eventproductid"]).Id;
					localContext.TracingService.Trace("Got msnfp_eventproductid.");
				}
				else
				{
					mSNFP_Product.EventProductId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_eventproductid.");
				}
				if (queriedEntityRecord.Contains("msnfp_identifier") && queriedEntityRecord["msnfp_identifier"] != null)
				{
					mSNFP_Product.Identifier = (string)queriedEntityRecord["msnfp_identifier"];
					localContext.TracingService.Trace("Got msnfp_identifier.");
				}
				else
				{
					mSNFP_Product.Identifier = string.Empty;
					localContext.TracingService.Trace("Did NOT find msnfp_identifier.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_Product.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got transactioncurrencyid.");
				}
				else
				{
					mSNFP_Product.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find transactioncurrencyid.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Product.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Product.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Product.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Product.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Product.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Product.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Product.CreatedOn = null;
				}
				mSNFP_Product.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Product.Deleted = true;
					mSNFP_Product.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Product.Deleted = false;
					mSNFP_Product.DeletedDate = null;
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Product));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Product);
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

		private void UpdateEventPackageProductTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventPackageProductTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventpackageid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventPackage = service.Retrieve("msnfp_eventpackage", ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id, new ColumnSet("msnfp_eventpackageid", "msnfp_amount"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_product")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == eventPackage.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventPackage["msnfp_sum_products"] = list.Count();
			eventPackage["msnfp_val_products"] = new Money(value);
			service.Update(eventPackage);
		}

		private void UpdateEventProductTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventProductTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventproductid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventProduct = service.Retrieve("msnfp_eventproduct", ((EntityReference)queriedEntityRecord["msnfp_eventproductid"]).Id, new ColumnSet("msnfp_eventproductid", "msnfp_amount", "msnfp_quantity"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_product")
				where ((EntityReference)a["msnfp_eventproductid"]).Id == eventProduct.Id && ((OptionSetValue)a["statuscode"]).Value != 844060001
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventProduct["msnfp_sum_sold"] = list.Count();
			eventProduct["msnfp_val_sold"] = new Money(value);
			if (eventProduct.Contains("msnfp_quantity") && eventProduct["msnfp_quantity"] != null)
			{
				eventProduct["msnfp_sum_available"] = (int)eventProduct["msnfp_quantity"] - list.Count();
			}
			service.Update(eventProduct);
		}
	}
}
