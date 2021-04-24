using System;
using System.Activities;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Plugins.PaymentProcesses;

namespace Workflows
{
	public sealed class CreateReceiptOnTransactionCreate : CodeActivity
	{
		private bool IsTraced = false;

		protected override void Execute(CodeActivityContext executionContext)
		{
			if (executionContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			ITracingService extension = executionContext.GetExtension<ITracingService>();
			IWorkflowContext extension2 = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory extension3 = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService organizationService = extension3.CreateOrganizationService(null);
			try
			{
				OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
				Guid primaryEntityId = extension2.PrimaryEntityId;
				Entity gift = organizationService.Retrieve(extension2.PrimaryEntityName, extension2.PrimaryEntityId, new ColumnSet("msnfp_transactionid", "msnfp_taxreceiptid", "msnfp_customerid", "statuscode", "msnfp_bookdate", "msnfp_configurationid", "msnfp_amount_receipted", "msnfp_amount", "msnfp_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_thirdpartyreceipt", "transactioncurrencyid", "ownerid", "msnfp_receiptpreferencecode"));
				decimal num = default(decimal);
				decimal num2 = default(decimal);
				decimal num3 = default(decimal);
				extension.Trace("Entering CreateReceiptOnTransactionCreate.cs");
				if (IsTraced)
				{
					extension.Trace("Transaction Id : " + primaryEntityId.ToString());
				}
				if (gift.Contains("msnfp_thirdpartyreceipt"))
				{
					if (IsTraced)
					{
						extension.Trace("Transaction contains msnfp_thirdpartyreceipt");
					}
					Entity entity = new Entity("msnfp_receipt");
					entity["msnfp_receipt"] = (string)gift["msnfp_thirdpartyreceipt"];
					entity["msnfp_title"] = (string)gift["msnfp_thirdpartyreceipt"];
					entity["msnfp_receiptgeneration"] = new OptionSetValue(844060002);
					num = (gift.Contains("msnfp_amount_receipted") ? ((Money)gift["msnfp_amount_receipted"]).Value : 0m);
					num2 = (gift.Contains("msnfp_amount_membership") ? ((Money)gift["msnfp_amount_membership"]).Value : 0m);
					num3 = (gift.Contains("msnfp_amount_nonreceiptable") ? ((Money)gift["msnfp_amount_nonreceiptable"]).Value : 0m);
					if (IsTraced)
					{
						extension.Trace("got membership amount and non-receiptable amount.");
					}
					if (IsTraced)
					{
						extension.Trace("amount " + num + " Membership Amount: " + num2 + " Non-receiptable : " + num3);
					}
					entity["msnfp_amount_receipted"] = gift["msnfp_amount_receipted"];
					entity["msnfp_amount_nonreceiptable"] = new Money(num2 + num3);
					string logicalName = ((EntityReference)gift["ownerid"]).LogicalName;
					Guid id = ((EntityReference)gift["ownerid"]).Id;
					entity["ownerid"] = new EntityReference(logicalName, id);
					entity["statuscode"] = new OptionSetValue(1);
					Guid id2 = organizationService.Create(entity);
					if (IsTraced)
					{
						extension.Trace("receipt created based on third party receipt number.");
					}
					gift["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", id2);
					gift["msnfp_previousreceiptid"] = new EntityReference("msnfp_receipt", id2);
					organizationService.Update(gift);
					if (IsTraced)
					{
						extension.Trace("Transaction updated with the receipt created wtih third party receipt number");
					}
				}
				else if (gift.Contains("msnfp_receiptpreferencecode") && gift["msnfp_receiptpreferencecode"] != null)
				{
					if (IsTraced)
					{
						extension.Trace("Transaction contains receipt preference");
					}
					int value = ((OptionSetValue)gift["msnfp_receiptpreferencecode"]).Value;
					if (value != 844060000)
					{
						if (IsTraced)
						{
							extension.Trace("Transaction receipt preference is NOT 'DO NOT RECEIPT', value: " + value);
						}
						if (gift.Contains("msnfp_taxreceiptid"))
						{
							if (IsTraced)
							{
								extension.Trace("Transaction contains receipt");
							}
							Guid id3 = ((EntityReference)gift["msnfp_taxreceiptid"]).Id;
							Entity entity2 = organizationService.Retrieve("msnfp_receipt", id3, new ColumnSet("msnfp_generatedorprinted", "statuscode"));
							if (entity2 != null)
							{
								if (IsTraced)
								{
									extension.Trace("receipt retrieve");
								}
								double num4 = (entity2.Contains("msnfp_generatedorprinted") ? ((double)entity2["msnfp_generatedorprinted"]) : 0.0);
								entity2["msnfp_generatedorprinted"] = num4 + 1.0;
								if (gift.Contains("statuscode") && ((OptionSetValue)gift["statuscode"]).Value != 844060000 && ((OptionSetValue)entity2["statuscode"]).Value == 1)
								{
									entity2["statuscode"] = new OptionSetValue(844060002);
								}
								organizationService.Update(entity2);
								SetReceiptIdentifier(entity2.Id, extension, organizationService);
								if (IsTraced)
								{
									extension.Trace("receipt updated");
								}
							}
						}
						else
						{
							if (IsTraced)
							{
								extension.Trace("Transaction does not contains receipt");
							}
							if (((OptionSetValue)gift["statuscode"]).Value == 844060000)
							{
								if (IsTraced)
								{
									extension.Trace("Transaction is completed");
								}
								Entity entity3 = new Entity("msnfp_receipt");
								int num5 = (gift.Contains("msnfp_bookdate") ? ((DateTime)gift["msnfp_bookdate"]).Year : 0);
								if (IsTraced)
								{
									extension.Trace("Gift Year : " + num5);
								}
								if (num5 != 0)
								{
									int receiptYearValue2 = Utilities.GetOptionsSetValueForLabel(organizationService, "msnfp_receiptstack", "msnfp_receiptyear", num5.ToString());
									if (IsTraced)
									{
										extension.Trace("receipt year value: " + receiptYearValue2);
									}
									Entity entity4 = (from a in organizationServiceContext.CreateQuery("msnfp_receiptstack")
										where ((EntityReference)a["msnfp_configurationid"]).Id == ((EntityReference)gift["msnfp_configurationid"]).Id && a["msnfp_receiptyear"] == new OptionSetValue(receiptYearValue2)
										select a).FirstOrDefault();
									if (entity4 != null)
									{
										if (IsTraced)
										{
											extension.Trace("receipt stake available.");
										}
										entity3["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", entity4.Id);
										num = (gift.Contains("msnfp_amount_receipted") ? ((Money)gift["msnfp_amount_receipted"]).Value : 0m);
										num2 = (gift.Contains("msnfp_amount_membership") ? ((Money)gift["msnfp_amount_membership"]).Value : 0m);
										num3 = (gift.Contains("msnfp_amount_nonreceiptable") ? ((Money)gift["msnfp_amount_nonreceiptable"]).Value : 0m);
										if (IsTraced)
										{
											extension.Trace("got membership amount and non-receiptable amount.");
										}
										if (IsTraced)
										{
											extension.Trace("amount " + num + " Membership Amount: " + num2 + " Non-receiptable : " + num3);
										}
										entity3["msnfp_amount_receipted"] = gift["msnfp_amount_receipted"];
										entity3["msnfp_amount_nonreceiptable"] = new Money(num2 + num3);
										entity3["msnfp_generatedorprinted"] = Convert.ToDouble(1);
										entity3["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
										entity3["msnfp_receiptissuedate"] = DateTime.Now;
										entity3["msnfp_transactioncount"] = 1;
										entity3["msnfp_amount"] = gift["msnfp_amount"];
										if (gift.Contains("transactioncurrencyid"))
										{
											entity3["transactioncurrencyid"] = new EntityReference("transactioncurrency", ((EntityReference)gift["transactioncurrencyid"]).Id);
										}
										if (gift.Contains("msnfp_customerid"))
										{
											string logicalName2 = ((EntityReference)gift["msnfp_customerid"]).LogicalName;
											Guid id4 = ((EntityReference)gift["msnfp_customerid"]).Id;
											entity3["msnfp_customerid"] = new EntityReference(logicalName2, id4);
										}
										string logicalName3 = ((EntityReference)gift["ownerid"]).LogicalName;
										Guid id5 = ((EntityReference)gift["ownerid"]).Id;
										entity3["ownerid"] = new EntityReference(logicalName3, id5);
										entity3["statuscode"] = new OptionSetValue(1);
										Guid guid = organizationService.Create(entity3);
										bool flag = true;
										SetReceiptIdentifier(guid, extension, organizationService);
										if (IsTraced)
										{
											extension.Trace("receipt created");
										}
										gift["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", guid);
										gift["msnfp_previousreceiptid"] = new EntityReference("msnfp_receipt", guid);
										organizationService.Update(gift);
										if (IsTraced)
										{
											extension.Trace("Transaction Updated");
										}
									}
								}
							}
						}
					}
				}
				else
				{
					if (IsTraced)
					{
						extension.Trace("Transaction does not contains receipt preference");
					}
					if (!gift.Contains("msnfp_taxreceiptid"))
					{
						if (IsTraced)
						{
							extension.Trace("Transaction does not contains receipt");
						}
						if (((OptionSetValue)gift["statuscode"]).Value == 844060000)
						{
							if (IsTraced)
							{
								extension.Trace("Transaction is completed");
							}
							Entity entity5 = new Entity("msnfp_receipt");
							int num6 = (gift.Contains("msnfp_bookdate") ? ((DateTime)gift["msnfp_bookdate"]).Year : 0);
							if (IsTraced)
							{
								extension.Trace("Gift Year : " + num6);
							}
							if (num6 != 0)
							{
								int receiptYearValue = Utilities.GetOptionsSetValueForLabel(organizationService, "msnfp_receiptstack", "msnfp_receiptyear", num6.ToString());
								if (IsTraced)
								{
									extension.Trace("receipt year value: " + receiptYearValue);
								}
								Entity entity6 = (from a in organizationServiceContext.CreateQuery("msnfp_receiptstack")
									where ((EntityReference)a["msnfp_configurationid"]).Id == ((EntityReference)gift["msnfp_configurationid"]).Id && a["msnfp_receiptyear"] == new OptionSetValue(receiptYearValue)
									select a).FirstOrDefault();
								if (entity6 != null)
								{
									if (IsTraced)
									{
										extension.Trace("receipt stake available.");
									}
									entity5["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", entity6.Id);
									num = (gift.Contains("msnfp_amount_receipted") ? ((Money)gift["msnfp_amount_receipted"]).Value : 0m);
									num2 = (gift.Contains("msnfp_amount_membership") ? ((Money)gift["msnfp_amount_membership"]).Value : 0m);
									num3 = (gift.Contains("msnfp_amount_nonreceiptable") ? ((Money)gift["msnfp_amount_nonreceiptable"]).Value : 0m);
									if (IsTraced)
									{
										extension.Trace("got membership amount and non-receiptable amount.");
									}
									if (IsTraced)
									{
										extension.Trace("amount " + num + " Membership Amount: " + num2 + " Non-receiptable : " + num3);
									}
									entity5["msnfp_amount_receipted"] = gift["msnfp_amount_receipted"];
									entity5["msnfp_amount_nonreceiptable"] = new Money(num2 + num3);
									entity5["msnfp_generatedorprinted"] = Convert.ToDouble(1);
									entity5["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
									entity5["msnfp_receiptissuedate"] = DateTime.Now;
									entity5["msnfp_transactioncount"] = 1;
									entity5["msnfp_amount"] = gift["msnfp_amount"];
									if (gift.Contains("transactioncurrencyid"))
									{
										entity5["transactioncurrencyid"] = new EntityReference("transactioncurrency", ((EntityReference)gift["transactioncurrencyid"]).Id);
									}
									if (gift.Contains("msnfp_customerid"))
									{
										string logicalName4 = ((EntityReference)gift["msnfp_customerid"]).LogicalName;
										Guid id6 = ((EntityReference)gift["msnfp_customerid"]).Id;
										entity5["msnfp_customerid"] = new EntityReference(logicalName4, id6);
									}
									string logicalName5 = ((EntityReference)gift["ownerid"]).LogicalName;
									Guid id7 = ((EntityReference)gift["ownerid"]).Id;
									entity5["ownerid"] = new EntityReference(logicalName5, id7);
									entity5["statuscode"] = new OptionSetValue(1);
									Guid guid2 = organizationService.Create(entity5);
									bool flag2 = true;
									SetReceiptIdentifier(guid2, extension, organizationService);
									if (IsTraced)
									{
										extension.Trace("receipt created");
									}
									gift["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", guid2);
									gift["msnfp_previousreceiptid"] = new EntityReference("msnfp_receipt", guid2);
									organizationService.Update(gift);
									if (IsTraced)
									{
										extension.Trace("Transaction Updated");
									}
								}
							}
						}
					}
				}
			}
			catch (FaultException<OrganizationServiceFault> ex)
			{
				if (IsTraced)
				{
					extension.Trace("Workflow Exception: {0}", ex.ToString());
				}
				throw;
			}
			if (IsTraced)
			{
				throw new Exception("Tracing enabled. Set it to false to remove this message.");
			}
		}

		private void SetReceiptIdentifier(Guid receiptID, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Entering SetReceiptIdentifier().");
			Entity entity = null;
			Guid? guid = null;
			string text = string.Empty;
			string empty = string.Empty;
			double num = 0.0;
			int num2 = 0;
			ColumnSet columnSet = new ColumnSet("msnfp_receiptid", "msnfp_identifier", "msnfp_receiptstackid");
			Entity entity2 = service.Retrieve("msnfp_receipt", receiptID, columnSet);
			tracingService.Trace("Found receipt with id: " + receiptID.ToString());
			if (entity2.Contains("msnfp_identifier"))
			{
				tracingService.Trace("Found receipt identifier: " + (string)entity2["msnfp_identifier"]);
				if (entity2["msnfp_identifier"] != null || ((string)entity2["msnfp_identifier"]).Length > 0)
				{
					tracingService.Trace("Receipt already has identfier. Exiting SetReceiptIdentifier().");
					return;
				}
			}
			if (entity2.Contains("msnfp_receiptstackid"))
			{
				guid = entity2.GetAttributeValue<EntityReference>("msnfp_receiptstackid").Id;
				tracingService.Trace("Found receipt stack.");
			}
			else
			{
				tracingService.Trace("No receipt stack found.");
			}
			if (guid.HasValue)
			{
				Guid? guid2 = guid;
				tracingService.Trace("Locking Receipt Stack record Id:" + guid2.ToString());
				Entity entity3 = new Entity("msnfp_receiptstack", guid.Value);
				entity3["msnfp_locked"] = true;
				service.Update(entity3);
				tracingService.Trace("Receipt Stack Record Locked.");
				entity = service.Retrieve("msnfp_receiptstack", ((EntityReference)entity2["msnfp_receiptstackid"]).Id, new ColumnSet("msnfp_receiptstackid", "msnfp_prefix", "msnfp_currentrange", "msnfp_numberrange"));
				tracingService.Trace("Obtaining prefix, current range and number range.");
				empty = (entity.Contains("msnfp_prefix") ? ((string)entity["msnfp_prefix"]) : string.Empty);
				num = (entity.Contains("msnfp_currentrange") ? ((double)entity["msnfp_currentrange"]) : 0.0);
				num2 = (entity.Contains("msnfp_numberrange") ? ((OptionSetValue)entity["msnfp_numberrange"]).Value : 0);
				switch (num2)
				{
				case 844060006:
					tracingService.Trace("Number range : 6 digit");
					text = empty + (num + 1.0).ToString().PadLeft(6, '0');
					break;
				case 844060008:
					tracingService.Trace("Number range : 8 digit");
					text = empty + (num + 1.0).ToString().PadLeft(8, '0');
					break;
				case 844060010:
					tracingService.Trace("Number range : 10 digit");
					text = empty + (num + 1.0).ToString().PadLeft(10, '0');
					break;
				default:
					tracingService.Trace("Receipt number range unknown. msnfp_numberrange: " + num2);
					break;
				}
				tracingService.Trace("Receipt Number: " + text);
				entity2["msnfp_receiptnumber"] = text;
				entity2["msnfp_identifier"] = text;
				tracingService.Trace("Updating Receipt.");
				service.Update(entity2);
				tracingService.Trace("Receipt Updated");
				tracingService.Trace("Now update the receipt stacks current number by 1.");
				Entity entity4 = new Entity("msnfp_receiptstack", guid.Value);
				entity4["msnfp_currentrange"] = num + 1.0;
				entity4["msnfp_locked"] = false;
				service.Update(entity4);
				tracingService.Trace("Updated Receipt Stack current range to: " + (num + 1.0));
			}
			else
			{
				tracingService.Trace("No receipt stack found.");
			}
			tracingService.Trace("Exiting SetReceiptIdentifier().");
		}
	}
}
