using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class ReceiptUpdate : PluginBase
	{
		public ReceiptUpdate(string unsecure, string secure)
			: base(typeof(ReceiptUpdate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			localContext.TracingService.Trace("---------Triggered ReceiptUpdate.cs---------");
			if (pluginExecutionContext.Depth > 2)
			{
				localContext.TracingService.Trace("Context.depth > 2. Exiting Plugin.");
				return;
			}
			Utilities utilities = new Utilities();
			localContext.TracingService.Trace("---------Entering ReceiptUpdate.cs Main Function---------");
			ColumnSet columnSet = new ColumnSet("msnfp_receiptid", "msnfp_generatedorprinted", "msnfp_amount_nonreceiptable", "msnfp_receiptnumber", "msnfp_receiptissuedate", "msnfp_amount_receipted", "msnfp_receiptgeneration", "msnfp_receiptstackid", "msnfp_receiptstatus", "msnfp_replacesreceiptid", "statuscode", "modifiedby", "msnfp_customerid", "msnfp_lastdonationdate");
			Entity primaryReceipt = organizationService.Retrieve("msnfp_receipt", pluginExecutionContext.PrimaryEntityId, columnSet);
			localContext.TracingService.Trace("Retrieved primary receipt. Id:" + primaryReceipt.Id.ToString() + ", Number" + primaryReceipt.GetAttributeValue<string>("msnfp_receiptnumber"));
			string optionSetValueLabel = Utilities.GetOptionSetValueLabel("msnfp_receipt", "statuscode", ((OptionSetValue)primaryReceipt["statuscode"]).Value, organizationService);
			string text = (primaryReceipt.Contains("msnfp_receiptnumber") ? ((string)primaryReceipt["msnfp_receiptnumber"]) : string.Empty);
			if (primaryReceipt.Contains("statuscode") && ((OptionSetValue)primaryReceipt["statuscode"]).Value == 844060000)
			{
				localContext.TracingService.Trace("Statuscode : Void");
				primaryReceipt["msnfp_receiptstatus"] = "Receipt Voided";
				Entity entity = new Entity("msnfp_receiptlog");
				if (primaryReceipt.Contains("msnfp_receiptstackid"))
				{
					entity["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", ((EntityReference)primaryReceipt["msnfp_receiptstackid"]).Id);
				}
				primaryReceipt["msnfp_identifier"] = text + " - " + optionSetValueLabel;
				entity["msnfp_receiptnumber"] = (primaryReceipt.Contains("msnfp_receiptnumber") ? ((string)primaryReceipt["msnfp_receiptnumber"]) : string.Empty);
				entity["msnfp_entryreason"] = "RECEIPT VOIDED";
				entity["msnfp_entryby"] = ((EntityReference)primaryReceipt["modifiedby"]).Name;
				if (pluginExecutionContext.Depth < 2)
				{
					organizationService.Create(entity);
					localContext.TracingService.Trace("Receipt log created as Void.");
				}
				else
				{
					localContext.TracingService.Trace("Receipt log not created due to context.depth >= 2.");
				}
			}
			else if (primaryReceipt.Contains("statuscode") && ((OptionSetValue)primaryReceipt["statuscode"]).Value == 844060001)
			{
				localContext.TracingService.Trace("Statuscode : Void (Reissued)");
				Entity entity2 = new Entity("msnfp_receipt");
				string text2 = string.Empty;
				string empty = string.Empty;
				double num = 0.0;
				int num2 = 0;
				Entity entity3 = new Entity("msnfp_receiptlog");
				List<Entity> list = (from g in organizationServiceContext.CreateQuery("msnfp_transaction")
					where ((EntityReference)g["msnfp_taxreceiptid"]).Id == primaryReceipt.Id && (((OptionSetValue)g["statuscode"]).Value == 844060000 || ((OptionSetValue)g["statuscode"]).Value == 844060004 || ((OptionSetValue)g["statuscode"]).Value == 1)
					select g).ToList();
				localContext.TracingService.Trace(list.Count + " Transactions");
				List<Entity> list2 = (from ev in organizationServiceContext.CreateQuery("msnfp_eventpackage")
					where ((EntityReference)ev["msnfp_taxreceiptid"]).Id == primaryReceipt.Id && ((OptionSetValue)ev["statuscode"]).Value == 844060000
					select ev).ToList();
				localContext.TracingService.Trace(list2.Count + " Event Packages");
				List<Entity> list3 = (from ps in organizationServiceContext.CreateQuery("msnfp_paymentschedule")
					where ((EntityReference)ps["msnfp_taxreceiptid"]).Id == primaryReceipt.Id && (((OptionSetValue)ps["statuscode"]).Value == 844060000 || ((OptionSetValue)ps["statuscode"]).Value == 844060004 || ((OptionSetValue)ps["statuscode"]).Value == 1)
					select ps).ToList();
				localContext.TracingService.Trace(list3.Count + " Payment Schedules");
				if (list.Count > 0 || list2.Count > 0)
				{
					localContext.TracingService.Trace("Got related transactions (" + list.Count + ") for this receipt.");
					localContext.TracingService.Trace("Got related event packages (" + list2.Count + ") for this receipt.");
					localContext.TracingService.Trace("Got related payment schedule(s) (" + list3.Count + ") for this receipt.");
					decimal value = default(decimal);
					decimal value2 = default(decimal);
					decimal d = default(decimal);
					decimal d2 = default(decimal);
					Entity entity4 = null;
					if (primaryReceipt.Contains("msnfp_receiptstackid"))
					{
						entity4 = organizationService.Retrieve("msnfp_receiptstack", ((EntityReference)primaryReceipt["msnfp_receiptstackid"]).Id, new ColumnSet("msnfp_receiptstackid", "msnfp_prefix", "msnfp_currentrange", "msnfp_numberrange"));
					}
					if (entity4 != null)
					{
						localContext.TracingService.Trace("Receipt stack available.");
						entity2["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", entity4.Id);
						empty = (entity4.Contains("msnfp_prefix") ? ((string)entity4["msnfp_prefix"]) : string.Empty);
						num = (entity4.Contains("msnfp_currentrange") ? ((double)entity4["msnfp_currentrange"]) : 0.0);
						switch (entity4.Contains("msnfp_numberrange") ? ((OptionSetValue)entity4["msnfp_numberrange"]).Value : 0)
						{
						case 844060006:
							localContext.TracingService.Trace("number range : 6 digit");
							text2 = empty + (num + 1.0).ToString().PadLeft(6, '0');
							break;
						case 844060008:
							localContext.TracingService.Trace("number range : 8 digit");
							text2 = empty + (num + 1.0).ToString().PadLeft(8, '0');
							break;
						case 844060010:
							localContext.TracingService.Trace("number range : 10 digit");
							text2 = empty + (num + 1.0).ToString().PadLeft(10, '0');
							break;
						}
						localContext.TracingService.Trace("receiptNumber : " + text2);
						entity3["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", entity4.Id);
						double value3 = (primaryReceipt.Contains("msnfp_generatedorprinted") ? ((double)primaryReceipt["msnfp_generatedorprinted"]) : 0.0);
						entity2["msnfp_generatedorprinted"] = Convert.ToDouble(value3);
						entity2["msnfp_receiptnumber"] = text2;
						entity2["msnfp_identifier"] = text2;
						entity2["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
						entity2["msnfp_replacesreceiptid"] = new EntityReference("msnfp_receipt", primaryReceipt.Id);
						if (list != null && list.Count > 0)
						{
							foreach (Entity item in list)
							{
								if (item.Contains("msnfp_amount_receipted"))
								{
									value2 += ((Money)item["msnfp_amount_receipted"]).Value;
								}
								if (item.Contains("msnfp_amount_membership"))
								{
									d += ((Money)item["msnfp_amount_membership"]).Value;
								}
								if (item.Contains("msnfp_amount_nonreceiptable"))
								{
									d2 += ((Money)item["msnfp_amount_nonreceiptable"]).Value;
								}
								if (item.Contains("msnfp_amount"))
								{
									value += ((Money)item["msnfp_amount"]).Value;
								}
							}
						}
						if (list2 != null && list2.Count > 0)
						{
							foreach (Entity item2 in list)
							{
								if (item2.Contains("msnfp_amount_receipted"))
								{
									value2 += ((Money)item2["msnfp_amount_receipted"]).Value;
								}
								if (item2.Contains("msnfp_amount_membership"))
								{
									d += ((Money)item2["msnfp_amount_membership"]).Value;
								}
								if (item2.Contains("msnfp_amount_nonreceiptable"))
								{
									d2 += ((Money)item2["msnfp_amount_nonreceiptable"]).Value;
								}
								if (item2.Contains("msnfp_amount"))
								{
									value += ((Money)item2["msnfp_amount"]).Value;
								}
							}
						}
						entity2["msnfp_amount_receipted"] = new Money(value2);
						entity2["msnfp_amount_nonreceiptable"] = new Money(d + d2);
						string str = string.Empty;
						if (primaryReceipt.Contains("msnfp_receiptissuedate"))
						{
							str = ((DateTime)primaryReceipt["msnfp_receiptissuedate"]).ToShortDateString();
						}
						entity2["msnfp_receiptstatus"] = "This receipt replaces receipt " + text + " issued on " + str;
						if (primaryReceipt.Contains("msnfp_lastdonationdate"))
						{
							entity2["msnfp_lastdonationdate"] = (DateTime)primaryReceipt["msnfp_lastdonationdate"];
						}
						entity2["msnfp_receiptissuedate"] = DateTime.Now;
						entity2["msnfp_transactioncount"] = list.Count;
						entity2["msnfp_eventcount"] = list2.Count;
						entity2["msnfp_amount"] = new Money(value);
						entity2["statuscode"] = new OptionSetValue(1);
						if (primaryReceipt.Contains("msnfp_customerid"))
						{
							string logicalName = ((EntityReference)primaryReceipt["msnfp_customerid"]).LogicalName;
							Guid id = ((EntityReference)primaryReceipt["msnfp_customerid"]).Id;
							entity2["msnfp_customerid"] = new EntityReference(logicalName, id);
						}
						Guid id2 = organizationService.Create(entity2);
						bool flag = true;
						localContext.TracingService.Trace("Receipt created successfully. Update the receipt stacks current number by 1.");
						entity4["msnfp_currentrange"] = num + 1.0;
						organizationService.Update(entity4);
						localContext.TracingService.Trace("Updated Receipt Stack current range to: " + (num + 1.0));
						localContext.TracingService.Trace("new receipt created");
						primaryReceipt["msnfp_identifier"] = text + " - Reissued";
						primaryReceipt["msnfp_receiptstatus"] = "Void";
						entity3["msnfp_receiptnumber"] = (primaryReceipt.Contains("msnfp_receiptnumber") ? ((string)primaryReceipt["msnfp_receiptnumber"]) : string.Empty);
						entity3["msnfp_entryreason"] = "RECEIPT REPLACED BY RECEIPT " + text2;
						entity3["msnfp_entryby"] = ((EntityReference)primaryReceipt["modifiedby"]).Name;
						organizationService.Create(entity3);
						localContext.TracingService.Trace("Receipt log created as Void (Reissued).");
						if (list != null)
						{
							foreach (Entity item3 in list)
							{
								Entity entity5 = organizationService.Retrieve("msnfp_transaction", item3.Id, new ColumnSet("msnfp_taxreceiptid"));
								if (entity5 != null)
								{
									entity5["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", id2);
									organizationService.Update(entity5);
									localContext.TracingService.Trace("Updated gift with new receipt.");
								}
							}
						}
						if (list2 != null)
						{
							foreach (Entity item4 in list2)
							{
								Entity entity6 = organizationService.Retrieve("msnfp_eventpackage", item4.Id, new ColumnSet("msnfp_taxreceiptid"));
								if (entity6 != null)
								{
									entity6["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", id2);
									organizationService.Update(entity6);
									localContext.TracingService.Trace("Updated event package with new receipt.");
								}
							}
						}
						foreach (Entity item5 in list3)
						{
							Entity entity7 = organizationService.Retrieve("msnfp_paymentschedule", item5.Id, new ColumnSet("msnfp_taxreceiptid"));
							if (entity7 != null)
							{
								entity7["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", id2);
								organizationService.Update(entity7);
								localContext.TracingService.Trace("Updated payment schedule with new receipt.");
							}
						}
					}
				}
			}
			else if (primaryReceipt.Contains("statuscode") && ((OptionSetValue)primaryReceipt["statuscode"]).Value == 844060002)
			{
				localContext.TracingService.Trace("statuscode : Void (Payment Failed)");
				primaryReceipt["msnfp_receiptstatus"] = "Receipt Voided";
				Entity entity8 = new Entity("msnfp_receiptlog");
				if (primaryReceipt.Contains("msnfp_receiptstackid"))
				{
					entity8["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", ((EntityReference)primaryReceipt["msnfp_receiptstackid"]).Id);
				}
				primaryReceipt["msnfp_identifier"] = text + " - " + optionSetValueLabel;
				entity8["msnfp_receiptnumber"] = (primaryReceipt.Contains("msnfp_receiptnumber") ? ((string)primaryReceipt["msnfp_receiptnumber"]) : string.Empty);
				entity8["msnfp_entryreason"] = "PAYMENT FAILED RECEIPT VOIDED";
				entity8["msnfp_entryby"] = ((EntityReference)primaryReceipt["modifiedby"]).Name;
				organizationService.Create(entity8);
				localContext.TracingService.Trace("receipt log created as Void (Payment Failed).");
			}
			organizationService.Update(primaryReceipt);
		}
	}
}
