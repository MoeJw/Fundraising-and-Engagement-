using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins
{
	public class UpdateReceiptOnAssociation : PluginBase
	{
		public UpdateReceiptOnAssociation(string unsecure, string secure)
			: base(typeof(UpdateReceiptOnAssociation))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered UpdateReceiptOnAssociation.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			Entity entity = null;
			string messageName = pluginExecutionContext.MessageName;
			string text = string.Empty;
			Entity entity2 = null;
			if (pluginExecutionContext.Depth > 1)
			{
				localContext.TracingService.Trace("Context depth > 1. Exiting.");
			}
			else
			{
				if (!pluginExecutionContext.InputParameters.Contains("Target"))
				{
					return;
				}
				if (pluginExecutionContext.InputParameters["Target"] is EntityReference)
				{
					localContext.TracingService.Trace("Message Name: " + messageName);
					if (pluginExecutionContext.MessageName.ToLower() == "associate" || pluginExecutionContext.MessageName.ToLower() == "disassociate")
					{
						if (pluginExecutionContext.InputParameters.Contains("Relationship"))
						{
							text = ((Relationship)pluginExecutionContext.InputParameters["Relationship"]).SchemaName;
							localContext.TracingService.Trace("Relationship found: " + text);
						}
						if (text.ToLower() != "msnfp_receipt_msnfp_transaction")
						{
							localContext.TracingService.Trace("Not correct relationship (triggered by " + text + ", looking for msnfp_receipt_msnfp_transaction). Exiting.");
							return;
						}
						localContext.TracingService.Trace("---------Entering UpdateReceiptOnAssociation.cs Main Function---------");
						EntityReference entityReference = (EntityReference)pluginExecutionContext.InputParameters["Target"];
						localContext.TracingService.Trace("targetIncomingRecord Logical Name: " + entityReference.LogicalName.ToString());
						Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
						Entity entity3 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
						if (entity3 == null)
						{
							throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
						}
						if (entityReference.LogicalName.ToLower().ToString() == "msnfp_receipt")
						{
							localContext.TracingService.Trace("Get associated transaction record from the incoming receipt entity.");
							UpdateReceiptAmountForThisTransaction(entityReference.Id, localContext, organizationService, pluginExecutionContext);
						}
						else if (entityReference.LogicalName.ToLower().ToString() == "msnfp_transaction")
						{
							localContext.TracingService.Trace("Get associated receipt record from the incoming transaction entity.");
							ColumnSet columnSet = new ColumnSet("msnfp_transactionid", "msnfp_amount_receipted", "msnfp_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_amount", "transactioncurrencyid", "msnfp_customerid", "ownerid", "msnfp_taxreceiptid", "msnfp_previousreceiptid");
							entity = organizationService.Retrieve("msnfp_transaction", entityReference.Id, columnSet);
							if (entity.Contains("msnfp_taxreceiptid"))
							{
								localContext.TracingService.Trace("Transaction contains receipt with id: " + ((EntityReference)entity["msnfp_taxreceiptid"]).Id.ToString());
								UpdateReceiptAmountForThisTransaction(((EntityReference)entity["msnfp_taxreceiptid"]).Id, localContext, organizationService, pluginExecutionContext);
							}
							else
							{
								localContext.TracingService.Trace("No receipt found. Attempting to use msnfp_previousreceiptid.");
								if (entity.Contains("msnfp_previousreceiptid"))
								{
									localContext.TracingService.Trace("Transaction contains previous receipt with id: " + ((EntityReference)entity["msnfp_previousreceiptid"]).Id.ToString());
									UpdateReceiptAmountForThisTransaction(((EntityReference)entity["msnfp_previousreceiptid"]).Id, localContext, organizationService, pluginExecutionContext);
								}
								else
								{
									localContext.TracingService.Trace("No msnfp_previousreceiptid found. Exiting workflow.");
								}
							}
						}
					}
				}
				else if (pluginExecutionContext.InputParameters["Target"] is Entity)
				{
					localContext.TracingService.Trace("Message Name: " + messageName);
					if (pluginExecutionContext.MessageName.ToLower() == "update")
					{
						localContext.TracingService.Trace("---------Entering UpdateReceiptOnAssociation.cs Main Function---------");
						entity2 = (Entity)pluginExecutionContext.InputParameters["Target"];
						localContext.TracingService.Trace("transactionRecordOnUpdate Logical Name: " + entity2.LogicalName.ToString());
						Guid initiatingUserId2 = pluginExecutionContext.InitiatingUserId;
						Entity entity4 = organizationService.Retrieve("systemuser", initiatingUserId2, new ColumnSet("msnfp_configurationid"));
						if (entity4 == null)
						{
							throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
						}
						if (entity2.LogicalName.ToLower().ToString() == "msnfp_transaction")
						{
							localContext.TracingService.Trace("Get associated receipt record from the incoming transaction entity (transactionRecordOnUpdate).");
							ColumnSet columnSet2 = new ColumnSet("msnfp_transactionid", "msnfp_amount_receipted", "msnfp_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_amount", "transactioncurrencyid", "msnfp_customerid", "ownerid", "msnfp_taxreceiptid", "msnfp_previousreceiptid");
							entity = organizationService.Retrieve("msnfp_transaction", pluginExecutionContext.PrimaryEntityId, columnSet2);
							localContext.TracingService.Trace("Retrieved Transaction.");
							if (entity.Contains("msnfp_taxreceiptid"))
							{
								localContext.TracingService.Trace("Retrieved receipt.");
								localContext.TracingService.Trace("Transaction contains receipt with id: " + ((EntityReference)entity["msnfp_taxreceiptid"]).Id.ToString());
								if (entity.Contains("msnfp_previousreceiptid"))
								{
									if (((EntityReference)entity["msnfp_previousreceiptid"]).Id != ((EntityReference)entity["msnfp_taxreceiptid"]).Id)
									{
										localContext.TracingService.Trace("Old Receipt and New Receipt are different. Updating both.");
										Guid id = ((EntityReference)entity["msnfp_previousreceiptid"]).Id;
										Guid id2 = ((EntityReference)entity["msnfp_taxreceiptid"]).Id;
										ITracingService tracingService = localContext.TracingService;
										Guid guid = id;
										tracingService.Trace("Updating Old Receipt ID: " + guid.ToString());
										UpdateReceiptAmountForThisTransaction(id, localContext, organizationService, pluginExecutionContext);
										ITracingService tracingService2 = localContext.TracingService;
										guid = id2;
										tracingService2.Trace("Finished Updating Old Receipt. Now Update the New Receipt ID: " + guid.ToString());
										UpdateReceiptAmountForThisTransaction(id2, localContext, organizationService, pluginExecutionContext);
									}
									else
									{
										UpdateReceiptAmountForThisTransaction(((EntityReference)entity["msnfp_taxreceiptid"]).Id, localContext, organizationService, pluginExecutionContext);
									}
								}
								else
								{
									UpdateReceiptAmountForThisTransaction(((EntityReference)entity["msnfp_taxreceiptid"]).Id, localContext, organizationService, pluginExecutionContext);
								}
							}
							else
							{
								localContext.TracingService.Trace("No receipt found. Attempting to use msnfp_previousreceiptid.");
								if (entity.Contains("msnfp_previousreceiptid"))
								{
									localContext.TracingService.Trace("Transaction contains previous receipt with id: " + ((EntityReference)entity["msnfp_previousreceiptid"]).Id.ToString());
									UpdateReceiptAmountForThisTransaction(((EntityReference)entity["msnfp_previousreceiptid"]).Id, localContext, organizationService, pluginExecutionContext);
								}
								else
								{
									localContext.TracingService.Trace("No msnfp_previousreceiptid found. Exiting workflow.");
								}
							}
						}
					}
				}
				localContext.TracingService.Trace("---------Exiting UpdateReceiptOnAssociation.cs---------");
			}
		}

		private void UpdateReceiptAmountForThisTransaction(Guid receiptRecordID, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("Entering UpdateReceiptAmountForThisTransaction()");
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(service);
			ColumnSet columnSet = new ColumnSet("msnfp_receiptid", "createdon", "msnfp_customerid", "msnfp_expectedtaxcredit", "msnfp_generatedorprinted", "msnfp_lastdonationdate", "msnfp_amount_nonreceiptable", "msnfp_transactioncount", "msnfp_preferredlanguagecode", "msnfp_receiptnumber", "msnfp_receiptgeneration", "msnfp_receiptissuedate", "msnfp_receiptstackid", "msnfp_receiptstatus", "msnfp_amount_receipted", "msnfp_paymentscheduleid", "msnfp_replacesreceiptid", "msnfp_identifier", "msnfp_amount");
			Entity entity = service.Retrieve("msnfp_receipt", receiptRecordID, columnSet);
			localContext.TracingService.Trace("Old msnfp_amount_receipted: " + ((Money)entity["msnfp_amount_receipted"]).Value);
			entity["msnfp_amount_receipted"] = new Money(0m);
			localContext.TracingService.Trace("Old msnfp_amount_nonreceiptable: " + ((Money)entity["msnfp_amount_nonreceiptable"]).Value);
			entity["msnfp_amount_nonreceiptable"] = new Money(0m);
			localContext.TracingService.Trace("Old msnfp_amount: " + ((Money)entity["msnfp_amount"]).Value);
			entity["msnfp_amount"] = new Money(0m);
			entity["msnfp_transactioncount"] = 0;
			List<Entity> list = (from t in organizationServiceContext.CreateQuery("msnfp_transaction")
				where ((EntityReference)t["msnfp_taxreceiptid"]).Id == receiptRecordID
				select t).ToList();
			foreach (Entity item in list)
			{
				decimal num = default(decimal);
				decimal num2 = default(decimal);
				decimal num3 = default(decimal);
				decimal num4 = default(decimal);
				localContext.TracingService.Trace("------------------");
				localContext.TracingService.Trace("Processing Transaction ID: " + ((Guid)item["msnfp_transactionid"]).ToString());
				num = (item.Contains("msnfp_amount") ? ((Money)item["msnfp_amount"]).Value : 0m);
				num2 = (item.Contains("msnfp_amount_receipted") ? ((Money)item["msnfp_amount_receipted"]).Value : 0m);
				num3 = (item.Contains("msnfp_amount_membership") ? ((Money)item["msnfp_amount_membership"]).Value : 0m);
				num4 = (item.Contains("msnfp_amount_nonreceiptable") ? ((Money)item["msnfp_amount_nonreceiptable"]).Value : 0m);
				localContext.TracingService.Trace("Got membership amount and non-receiptable amount.");
				localContext.TracingService.Trace("Amount Receipted " + num2 + " Membership Amount: " + num3 + " Non-receiptable : " + num4);
				localContext.TracingService.Trace("Old msnfp_amount_receipted: " + ((Money)entity["msnfp_amount_receipted"]).Value);
				entity["msnfp_amount_receipted"] = new Money(((Money)entity["msnfp_amount_receipted"]).Value + new Money(num2).Value);
				localContext.TracingService.Trace("New msnfp_amount_receipted: " + ((Money)entity["msnfp_amount_receipted"]).Value);
				localContext.TracingService.Trace("Old msnfp_amount_nonreceiptable: " + ((Money)entity["msnfp_amount_nonreceiptable"]).Value);
				entity["msnfp_amount_nonreceiptable"] = new Money(((Money)entity["msnfp_amount_nonreceiptable"]).Value + new Money(num3 + num4).Value);
				localContext.TracingService.Trace("New msnfp_amount_nonreceiptable: " + ((Money)entity["msnfp_amount_nonreceiptable"]).Value);
				localContext.TracingService.Trace("Old msnfp_amount: " + ((Money)entity["msnfp_amount"]).Value);
				entity["msnfp_amount"] = new Money(((Money)entity["msnfp_amount"]).Value + new Money(num).Value);
				localContext.TracingService.Trace("New msnfp_amount: " + ((Money)entity["msnfp_amount"]).Value);
				entity["msnfp_generatedorprinted"] = Convert.ToDouble(1);
				entity["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
				entity["msnfp_receiptissuedate"] = DateTime.Now;
				localContext.TracingService.Trace("Getting transaction count.");
				localContext.TracingService.Trace("Old msnfp_transactioncount: " + (int)entity["msnfp_transactioncount"]);
				entity["msnfp_transactioncount"] = (int)entity["msnfp_transactioncount"] + 1;
				localContext.TracingService.Trace("New msnfp_transactioncount: " + (int)entity["msnfp_transactioncount"]);
				if (item.Contains("transactioncurrencyid"))
				{
					entity["transactioncurrencyid"] = new EntityReference("transactioncurrency", ((EntityReference)item["transactioncurrencyid"]).Id);
				}
				if (item.Contains("msnfp_customerid"))
				{
					string logicalName = ((EntityReference)item["msnfp_customerid"]).LogicalName;
					Guid id = ((EntityReference)item["msnfp_customerid"]).Id;
					entity["msnfp_customerid"] = new EntityReference(logicalName, id);
				}
				string logicalName2 = ((EntityReference)item["ownerid"]).LogicalName;
				Guid id2 = ((EntityReference)item["ownerid"]).Id;
				entity["ownerid"] = new EntityReference(logicalName2, id2);
				entity["statuscode"] = new OptionSetValue(1);
				if (item.Contains("msnfp_taxreceiptid"))
				{
					localContext.TracingService.Trace("Replace old receipt with this one.");
					if (item.Contains("msnfp_previousreceiptid"))
					{
						localContext.TracingService.Trace("Old Previous Receipt ID: " + ((EntityReference)item["msnfp_previousreceiptid"]).Id.ToString());
					}
					item["msnfp_previousreceiptid"] = new EntityReference("msnfp_receipt", ((EntityReference)item["msnfp_taxreceiptid"]).Id);
					localContext.TracingService.Trace("Updated Previous Receipt ID: " + ((EntityReference)item["msnfp_previousreceiptid"]).Id.ToString());
					localContext.TracingService.Trace("Saving Transaction.");
					if (!organizationServiceContext.IsAttached(item))
					{
						organizationServiceContext.Attach(item);
					}
					organizationServiceContext.UpdateObject(item);
					organizationServiceContext.SaveChanges();
					localContext.TracingService.Trace("Transaction Updated.");
				}
				localContext.TracingService.Trace("------------------");
			}
			localContext.TracingService.Trace("Saving Receipt.");
			service.Update(entity);
			localContext.TracingService.Trace("Receipt updated.");
			localContext.TracingService.Trace("Exiting UpdateReceiptAmountForThisTransaction()");
		}
	}
}
