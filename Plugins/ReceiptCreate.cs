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
	public class ReceiptCreate : PluginBase
	{
		public ReceiptCreate(string unsecure, string secure)
			: base(typeof(ReceiptCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered ReceiptCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			if (pluginExecutionContext.Depth > 2)
			{
				localContext.TracingService.Trace("Context.depth > 2. Exiting Plugin.");
				return;
			}
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
				localContext.TracingService.Trace("---------Entering ReceiptCreate.cs Main Function---------");
				localContext.TracingService.Trace("Message Name: " + messageName);
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (messageName == "Update")
				{
					queriedEntityRecord = organizationService.Retrieve("msnfp_receipt", entity3.Id, GetColumnSet());
				}
				if (entity3 != null)
				{
					SetReceiptIdentifier(entity3.Id, localContext, organizationService, messageName);
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
					localContext.TracingService.Trace("Target record not found. Exiting workflow.");
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_receipt", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting ReceiptCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_receiptid", "msnfp_identifier", "msnfp_customerid", "msnfp_expectedtaxcredit", "msnfp_generatedorprinted", "msnfp_lastdonationdate", "msnfp_amount_nonreceiptable", "msnfp_transactioncount", "msnfp_preferredlanguagecode", "msnfp_receiptnumber", "msnfp_receiptgeneration", "msnfp_receiptissuedate", "msnfp_receiptstackid", "msnfp_receiptstatus", "msnfp_amount_receipted", "msnfp_paymentscheduleid", "msnfp_replacesreceiptid", "msnfp_amount", "transactioncurrencyid", "msnfp_printed", "msnfp_deliverycode", "msnfp_emaildeliverystatuscode", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Receipt";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_receiptid"].ToString());
				MSNFP_Receipt mSNFP_Receipt = new MSNFP_Receipt();
				mSNFP_Receipt.ReceiptId = (Guid)queriedEntityRecord["msnfp_receiptid"];
				mSNFP_Receipt.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_Receipt.Identifier);
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Receipt.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_Receipt.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_Receipt.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_Receipt.CustomerId = null;
					mSNFP_Receipt.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_expectedtaxcredit") && queriedEntityRecord["msnfp_expectedtaxcredit"] != null)
				{
					mSNFP_Receipt.ExpectedTaxCredit = ((Money)queriedEntityRecord["msnfp_expectedtaxcredit"]).Value;
					localContext.TracingService.Trace("Got msnfp_expectedtaxcredit.");
				}
				else
				{
					mSNFP_Receipt.ExpectedTaxCredit = null;
					localContext.TracingService.Trace("Did NOT find msnfp_expectedtaxcredit.");
				}
				if (queriedEntityRecord.Contains("msnfp_generatedorprinted") && queriedEntityRecord["msnfp_generatedorprinted"] != null)
				{
					mSNFP_Receipt.GeneratedorPrinted = (double)queriedEntityRecord["msnfp_generatedorprinted"];
					localContext.TracingService.Trace("Got msnfp_generatedorprinted.");
				}
				else
				{
					mSNFP_Receipt.GeneratedorPrinted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_generatedorprinted.");
				}
				if (queriedEntityRecord.Contains("msnfp_lastdonationdate") && queriedEntityRecord["msnfp_lastdonationdate"] != null)
				{
					mSNFP_Receipt.LastDonationDate = (DateTime)queriedEntityRecord["msnfp_lastdonationdate"];
					localContext.TracingService.Trace("Got msnfp_lastdonationdate.");
				}
				else
				{
					mSNFP_Receipt.LastDonationDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_lastdonationdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_Receipt.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Receipt.AmountNonReceiptable = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactioncount") && queriedEntityRecord["msnfp_transactioncount"] != null)
				{
					mSNFP_Receipt.TransactionCount = (int)queriedEntityRecord["msnfp_transactioncount"];
					localContext.TracingService.Trace("Got msnfp_transactioncount.");
				}
				else
				{
					mSNFP_Receipt.TransactionCount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactioncount.");
				}
				if (queriedEntityRecord.Contains("msnfp_preferredlanguagecode") && queriedEntityRecord["msnfp_preferredlanguagecode"] != null)
				{
					mSNFP_Receipt.PreferredLanguageCode = ((OptionSetValue)queriedEntityRecord["msnfp_preferredlanguagecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_preferredlanguagecode.");
				}
				else
				{
					mSNFP_Receipt.PreferredLanguageCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_preferredlanguagecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptnumber") && queriedEntityRecord["msnfp_receiptnumber"] != null)
				{
					mSNFP_Receipt.ReceiptNumber = (string)queriedEntityRecord["msnfp_receiptnumber"];
					localContext.TracingService.Trace("Got msnfp_receiptnumber.");
				}
				else
				{
					mSNFP_Receipt.ReceiptNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptnumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptgeneration") && queriedEntityRecord["msnfp_receiptgeneration"] != null)
				{
					mSNFP_Receipt.ReceiptGeneration = ((OptionSetValue)queriedEntityRecord["msnfp_receiptgeneration"]).Value;
					localContext.TracingService.Trace("Got msnfp_receiptgeneration.");
				}
				else
				{
					mSNFP_Receipt.ReceiptGeneration = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptgeneration.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptissuedate") && queriedEntityRecord["msnfp_receiptissuedate"] != null)
				{
					mSNFP_Receipt.ReceiptIssueDate = (DateTime)queriedEntityRecord["msnfp_receiptissuedate"];
					localContext.TracingService.Trace("Got msnfp_receiptissuedate.");
				}
				else
				{
					mSNFP_Receipt.ReceiptIssueDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptissuedate.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptstackid") && queriedEntityRecord["msnfp_receiptstackid"] != null)
				{
					mSNFP_Receipt.ReceiptStackId = ((EntityReference)queriedEntityRecord["msnfp_receiptstackid"]).Id;
					localContext.TracingService.Trace("Got msnfp_receiptstackid.");
				}
				else
				{
					mSNFP_Receipt.ReceiptStackId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptstackid.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiptstatus") && queriedEntityRecord["msnfp_receiptstatus"] != null)
				{
					mSNFP_Receipt.ReceiptStatus = (string)queriedEntityRecord["msnfp_receiptstatus"];
					localContext.TracingService.Trace("Got msnfp_receiptstatus.");
				}
				else
				{
					mSNFP_Receipt.ReceiptStatus = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiptstatus.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_Receipt.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_Receipt.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentscheduleid") && queriedEntityRecord["msnfp_paymentscheduleid"] != null)
				{
					mSNFP_Receipt.PaymentScheduleId = ((EntityReference)queriedEntityRecord["msnfp_paymentscheduleid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentscheduleid.");
				}
				else
				{
					mSNFP_Receipt.PaymentScheduleId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentscheduleid.");
				}
				if (queriedEntityRecord.Contains("msnfp_replacesreceiptid") && queriedEntityRecord["msnfp_replacesreceiptid"] != null)
				{
					mSNFP_Receipt.ReplacesReceiptId = ((EntityReference)queriedEntityRecord["msnfp_replacesreceiptid"]).Id;
					localContext.TracingService.Trace("Got msnfp_replacesreceiptid.");
				}
				else
				{
					mSNFP_Receipt.ReplacesReceiptId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_replacesreceiptid.");
				}
				if (queriedEntityRecord.Contains("msnfp_replacesreceiptid") && queriedEntityRecord["msnfp_replacesreceiptid"] != null)
				{
					mSNFP_Receipt.ReplacesReceiptId = ((EntityReference)queriedEntityRecord["msnfp_replacesreceiptid"]).Id;
					localContext.TracingService.Trace("Got msnfp_replacesreceiptid.");
				}
				else
				{
					mSNFP_Receipt.ReplacesReceiptId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_replacesreceiptid.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Receipt.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_Receipt.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_Receipt.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got TransactionCurrencyId.");
				}
				else
				{
					mSNFP_Receipt.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find TransactionCurrencyId.");
				}
				if (queriedEntityRecord.Contains("msnfp_printed") && queriedEntityRecord["msnfp_printed"] != null)
				{
					mSNFP_Receipt.Printed = (DateTime)queriedEntityRecord["msnfp_printed"];
					localContext.TracingService.Trace("Got Printed.");
				}
				else
				{
					mSNFP_Receipt.Printed = null;
					localContext.TracingService.Trace("Did NOT find Printed.");
				}
				if (queriedEntityRecord.Contains("msnfp_deliverycode") && queriedEntityRecord["msnfp_deliverycode"] != null)
				{
					mSNFP_Receipt.DeliveryCode = ((OptionSetValue)queriedEntityRecord["msnfp_deliverycode"]).Value;
					localContext.TracingService.Trace("Got Delivery.");
				}
				else
				{
					mSNFP_Receipt.DeliveryCode = null;
					localContext.TracingService.Trace("Did NOT find Delivery.");
				}
				if (queriedEntityRecord.Contains("msnfp_emaildeliverystatuscode") && queriedEntityRecord["msnfp_emaildeliverystatuscode"] != null)
				{
					mSNFP_Receipt.EmailDeliveryStatusCode = ((OptionSetValue)queriedEntityRecord["msnfp_emaildeliverystatuscode"]).Value;
					localContext.TracingService.Trace("Got EmailDeliveryStatus.");
				}
				else
				{
					mSNFP_Receipt.EmailDeliveryStatusCode = null;
					localContext.TracingService.Trace("Did NOT find EmailDeliveryStatus.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Receipt.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Receipt.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Receipt.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Receipt.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Receipt.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Receipt.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Receipt.CreatedOn = null;
				}
				mSNFP_Receipt.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Receipt.Deleted = true;
					mSNFP_Receipt.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Receipt.Deleted = false;
					mSNFP_Receipt.DeletedDate = null;
				}
				mSNFP_Receipt.PaymentSchedule = null;
				mSNFP_Receipt.ReceiptStack = null;
				mSNFP_Receipt.ReplacesReceipt = null;
				mSNFP_Receipt.InverseReplacesReceipt = new HashSet<MSNFP_Receipt>();
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
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Receipt));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Receipt);
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
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting workflow.");
			}
		}

		private void SetReceiptIdentifier(Guid receiptID, LocalPluginContext localContext, IOrganizationService service, string messageName)
		{
			localContext.TracingService.Trace("Entering SetReceiptIdentifier().");
			Entity entity = null;
			Guid? guid = null;
			string text = string.Empty;
			string empty = string.Empty;
			double num = 0.0;
			int num2 = 0;
			ColumnSet columnSet = new ColumnSet("msnfp_receiptid", "msnfp_identifier", "msnfp_receiptstackid", "msnfp_receiptstatus");
			Entity entity2 = service.Retrieve("msnfp_receipt", receiptID, columnSet);
			localContext.TracingService.Trace("Found receipt with id: " + receiptID.ToString());
			if (entity2.Contains("msnfp_identifier") && messageName != "Create")
			{
				localContext.TracingService.Trace("Found receipt identifier: " + (string)entity2["msnfp_identifier"]);
				if (entity2["msnfp_identifier"] != null || ((string)entity2["msnfp_identifier"]).Length > 0)
				{
					localContext.TracingService.Trace("Receipt already has identfier. Exiting SetReceiptIdentifier().");
					return;
				}
			}
			if (entity2.Contains("msnfp_receiptstackid"))
			{
				guid = entity2.GetAttributeValue<EntityReference>("msnfp_receiptstackid").Id;
				localContext.TracingService.Trace("Found receipt stack.");
			}
			else
			{
				localContext.TracingService.Trace("No receipt stack found.");
			}
			if (guid.HasValue)
			{
				ITracingService tracingService = localContext.TracingService;
				Guid? guid2 = guid;
				tracingService.Trace("Locking Receipt Stack record Id:" + guid2.ToString());
				Entity entity3 = new Entity("msnfp_receiptstack", guid.Value);
				entity3["msnfp_locked"] = true;
				service.Update(entity3);
				localContext.TracingService.Trace("Receipt Stack record locked");
				entity = service.Retrieve("msnfp_receiptstack", ((EntityReference)entity2["msnfp_receiptstackid"]).Id, new ColumnSet("msnfp_receiptstackid", "msnfp_prefix", "msnfp_currentrange", "msnfp_numberrange"));
				localContext.TracingService.Trace("Obtaining prefix, current range and number range.");
				empty = (entity.Contains("msnfp_prefix") ? ((string)entity["msnfp_prefix"]) : string.Empty);
				num = (entity.Contains("msnfp_currentrange") ? ((double)entity["msnfp_currentrange"]) : 0.0);
				num2 = (entity.Contains("msnfp_numberrange") ? ((OptionSetValue)entity["msnfp_numberrange"]).Value : 0);
				switch (num2)
				{
				case 844060000:
					localContext.TracingService.Trace("Number range : 6 digit");
					text = empty + (num + 1.0).ToString().PadLeft(6, '0');
					break;
				case 844060001:
					localContext.TracingService.Trace("Number range : 8 digit");
					text = empty + (num + 1.0).ToString().PadLeft(8, '0');
					break;
				case 844060002:
					localContext.TracingService.Trace("Number range : 10 digit");
					text = empty + (num + 1.0).ToString().PadLeft(10, '0');
					break;
				default:
					localContext.TracingService.Trace("Receipt number range unknown. msnfp_numberrange: " + num2);
					break;
				}
				localContext.TracingService.Trace("Receipt Number: " + text);
				Entity entity4 = new Entity(entity2.LogicalName, entity2.Id);
				entity4["msnfp_receiptnumber"] = text;
				entity4["msnfp_identifier"] = text;
				if (messageName == "Create" && !entity2.Contains("msnfp_receiptstatus"))
				{
					entity4["msnfp_receiptstatus"] = "Issued";
				}
				localContext.TracingService.Trace("Updating Receipt.");
				service.Update(entity4);
				localContext.TracingService.Trace("Receipt Updated");
				localContext.TracingService.Trace("Now update the receipt stacks current number by 1.");
				Entity entity5 = new Entity("msnfp_receiptstack", guid.Value);
				entity5["msnfp_currentrange"] = num + 1.0;
				entity5["msnfp_locked"] = false;
				service.Update(entity5);
				localContext.TracingService.Trace("Updated Receipt Stack current range to: " + (num + 1.0));
			}
			else
			{
				localContext.TracingService.Trace("No receipt stack found.");
			}
			localContext.TracingService.Trace("Exiting SetReceiptIdentifier().");
		}
	}
}
