using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using FundraisingandEngagement.StripeIntegration.Helpers;
using FundraisingandEngagement.StripeWebPayment.Model;
using FundraisingandEngagement.StripeWebPayment.Service;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moneris;
using Plugins.AzureModels;
using Plugins.Common;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class TransactionGiftCreate : PluginBase
	{
		public static Guid ContactGivingLevelWorkflowId = Guid.Parse("EAAE076C-DB57-4979-A479-CC17B83CE705");

		public static Guid AccountGivingLevelWorkflowId = Guid.Parse("810C634A-2F4C-45B7-BFCC-C4FAAE315970");

		public TransactionGiftCreate(string unsecure, string secure)
			: base(typeof(TransactionGiftCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered TransactionGiftCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			string empty = string.Empty;
			Guid empty2 = Guid.Empty;
			string messageName = pluginExecutionContext.MessageName;
			if (pluginExecutionContext.Depth > 1 && !CheckExecutionPipeLine(pluginExecutionContext))
			{
				localContext.TracingService.Trace("Context.depth = " + pluginExecutionContext.Depth + ". Exiting Plugin.");
				for (IPluginExecutionContext pluginExecutionContext2 = pluginExecutionContext; pluginExecutionContext2 != null; pluginExecutionContext2 = pluginExecutionContext2.ParentContext)
				{
					localContext.TracingService.Trace(pluginExecutionContext2.PrimaryEntityName + ", " + pluginExecutionContext2.Depth);
				}
				return;
			}
			localContext.TracingService.Trace("Context.depth = " + pluginExecutionContext.Depth);
			Entity entity = null;
			Entity entity2 = null;
			entity2 = Plugins.PaymentProcesses.Utilities.GetConfigurationRecordByMessageName(pluginExecutionContext, organizationService, localContext.TracingService);
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity3 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			Plugins.PaymentProcesses.Utilities utilities = new Plugins.PaymentProcesses.Utilities();
			if (entity3 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			localContext.TracingService.Trace("---------Entering TransactionGiftCreate.cs Main Function---------");
			localContext.TracingService.Trace("Message Name: " + messageName);
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				entity = (Entity)pluginExecutionContext.InputParameters["Target"];
				Guid empty3 = Guid.Empty;
				ColumnSet columnSet = ReturnTransactionColumnSet();
				Entity giftTransaction=null;
				try
				{

				   giftTransaction = organizationService.Retrieve("msnfp_transaction", entity.Id, columnSet);

				}
				catch (Exception e) {
					localContext.TracingService.Trace("Transaction identifier (msnfp_name): " + (string)giftTransaction["msnfp_name"]);
				}
				if (giftTransaction == null)
				{
					//throw new ArgumentNullException("msnfp_transactionid");
				}
				if (giftTransaction.Contains("msnfp_name") && giftTransaction.Contains("statuscode"))
				{
					localContext.TracingService.Trace("Transaction identifier (msnfp_name): " + (string)giftTransaction["msnfp_name"]);
					localContext.TracingService.Trace("Transaction status reason (statuscode): " + ((OptionSetValue)giftTransaction["statuscode"]).Value);
				}
				if (messageName == "Create" && (!entity.Contains("msnfp_depositdate") || !entity.GetAttributeValue<DateTime?>("msnfp_depositdate").HasValue))
				{
					localContext.TracingService.Trace("contains msnfp_depositdate");
					Entity entity4 = new Entity(entity.LogicalName, entity.Id);
					Entity entity5 = entity;
					Entity entity6 = entity;
					object obj2 = (entity4["msnfp_receiveddate"] = DateTime.Now);
					object obj4 = (entity4["msnfp_depositdate"] = obj2);
					object obj7 = (entity5["msnfp_depositdate"] = (entity6["msnfp_receiveddate"] = obj4));
					organizationService.Update(entity4);
				}
				if (giftTransaction.Contains("msnfp_transaction_paymentmethodid"))
				{
					Entity entity7 = organizationService.Retrieve("msnfp_paymentmethod", ((EntityReference)giftTransaction["msnfp_transaction_paymentmethodid"]).Id, new ColumnSet("msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode", "msnfp_customerid"));
					if (entity7 != null)
					{
						localContext.TracingService.Trace("Obtained payment method for this transaction.");
						Entity entity8 = null;
						if (entity7.Contains("msnfp_paymentprocessorid"))
						{
							localContext.TracingService.Trace("Getting payment processor for transaction.");
							entity8 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)entity7["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentgatewaytype"));
							if (entity8.Contains("msnfp_paymentgatewaytype"))
							{
								localContext.TracingService.Trace("Obtained payment gateway for this transaction.");
								if (giftTransaction.Contains("msnfp_chargeoncreate") && !giftTransaction.Contains("msnfp_transactionidentifier") && messageName == "Create")
								{
									localContext.TracingService.Trace(string.Format("Is reusable {0}", entity7.GetAttributeValue<bool>("msnfp_isreusable")));
									if ((bool)giftTransaction["msnfp_chargeoncreate"] && ((OptionSetValue)giftTransaction["statuscode"]).Value == 844060000)
									{
										if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060000)
										{
											localContext.TracingService.Trace("Trying to process using processMonerisVaultTransaction");
											processMonerisVaultTransaction(giftTransaction, localContext, organizationService);
										}
										else if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060001)
										{
											localContext.TracingService.Trace("Trying to process using processStripeTransaction");
											processStripeTransaction(entity2, giftTransaction, localContext, organizationService, !entity7.GetAttributeValue<bool>("msnfp_isreusable"));
										}
										else if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060002)
										{
											localContext.TracingService.Trace("Trying to process using ProcessiATSTransaction");
											ProcessiATSTransaction(entity2, giftTransaction, localContext, organizationService, !entity7.GetAttributeValue<bool>("msnfp_isreusable"));
										}
										else
										{
											localContext.TracingService.Trace("((OptionSetValue)paymentProcessor[msnfp_paymentgatewaytype]).Value" + ((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value);
										}
										removePaymentMethod(entity7, localContext, organizationService);
									}
									else if (giftTransaction.Contains("msnfp_chargeoncreate"))
									{
										localContext.TracingService.Trace("msnfp_chargeoncreate = " + (bool)giftTransaction["msnfp_chargeoncreate"]);
									}
								}
								else if (!string.IsNullOrEmpty(giftTransaction.GetAttributeValue<string>("msnfp_transactionresult")))
								{
									localContext.TracingService.Trace("Transaction Result = " + (string)giftTransaction["msnfp_transactionresult"]);
									localContext.TracingService.Trace("Status Code = " + ((OptionSetValue)giftTransaction["statuscode"]).Value);
									if (giftTransaction.GetAttributeValue<OptionSetValue>("statuscode").Value == 844060000 && messageName.ToLower() == "update" && (giftTransaction.GetAttributeValue<string>("msnfp_transactionresult").ToLower().Contains("failed") || giftTransaction.GetAttributeValue<string>("msnfp_transactionresult").ToLower().Contains("declined") || giftTransaction.GetAttributeValue<string>("msnfp_transactionresult").ToLower().Contains("reject")))
									{
										localContext.TracingService.Trace("Attempting to retry failed transaction");
										if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060000)
										{
											localContext.TracingService.Trace("Trying to process using processMonerisVaultTransaction");
											processMonerisVaultTransaction(giftTransaction, localContext, organizationService);
										}
										else if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060001)
										{
											localContext.TracingService.Trace("Trying to process using processStripeTransaction");
											processStripeTransaction(entity2, giftTransaction, localContext, organizationService, !entity7.GetAttributeValue<bool>("msnfp_isreusable"));
										}
										else if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060002)
										{
											localContext.TracingService.Trace("Trying to process using ProcessiATSTransaction");
											ProcessiATSTransaction(entity2, giftTransaction, localContext, organizationService, !entity7.GetAttributeValue<bool>("msnfp_isreusable"));
										}
										else
										{
											localContext.TracingService.Trace("((OptionSetValue)paymentProcessor[msnfp_paymentgatewaytype]).Value" + ((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value);
										}
										removePaymentMethod(entity7, localContext, organizationService);
									}
								}
								else
								{
									if (giftTransaction.Contains("msnfp_chargeoncreate"))
									{
										localContext.TracingService.Trace("msnfp_chargeoncreate = " + (bool)giftTransaction["msnfp_chargeoncreate"]);
									}
									if (giftTransaction.Contains("msnfp_transactionidentifier"))
									{
										localContext.TracingService.Trace("msnfp_transactionidentifier = " + (string)giftTransaction["msnfp_transactionidentifier"]);
									}
									localContext.TracingService.Trace("No payment processed.");
								}
							}
						}
						else
						{
							localContext.TracingService.Trace("There is no payment processor. No payment processed.");
						}
					}
				}
				if (messageName == "Create")
				{
					localContext.TracingService.Trace("11111111111111111.");

				localContext.TracingService.Trace("11111111111111111."+ entity["msnfp_donorcommitmentid"].ToString());
					PopultateDonorFieldsOnTransactionAndPaymentMethod(ref giftTransaction, empty3, localContext, organizationService);
					if (entity.Contains("msnfp_donorcommitmentid"))
					{
						utilities.UpdateDonorCommitmentBalance(organizationServiceContext, organizationService, (EntityReference)entity["msnfp_donorcommitmentid"], 0);
					}
					CreateDesignatedCredit(localContext, organizationService, entity);
					Guid? guid = CreateTributeRecord(localContext, entity, organizationService);
					if (guid.HasValue)
					{
						entity["msnfp_tributeid"] = new EntityReference("msnfp_tributeormemory", guid.Value);
					}
					UpdateEventPackageDonationTotals(entity, organizationServiceContext, organizationService, localContext);
					UpdateEventTotals(entity, organizationServiceContext, organizationService, localContext);
					ReceiptUtilities.UpdateReceipt(pluginExecutionContext, organizationService, localContext.TracingService);
					CreateAutoSoftCredits(entity, entity2, organizationService, localContext.TracingService);
					AddOrUpdateThisRecordWithAzure(entity, entity2, localContext, organizationService, pluginExecutionContext);
					if (giftTransaction.Contains("msnfp_customerid"))
					{
						localContext.TracingService.Trace("See if this donor has any pledge matches.");
						List<Entity> list = (from c in organizationServiceContext.CreateQuery("msnfp_pledgematch")
							where ((EntityReference)c["msnfp_customerfromid"]).Id == ((EntityReference)giftTransaction["msnfp_customerid"]).Id
							select c).ToList();
						localContext.TracingService.Trace("Pledge Matches found: " + list.Count);
						if (list != null)
						{
							foreach (Entity item in list)
							{
								localContext.TracingService.Trace("Pledge Match ID to process next: " + item.Id.ToString());
								AddPledgeFromPledgeMatchRecord(giftTransaction, item, localContext, organizationService, pluginExecutionContext);
							}
						}
					}
				}
				else if (messageName == "Update")
				{
					if (!giftTransaction.Contains("msnfp_depositdate") || !giftTransaction.GetAttributeValue<DateTime?>("msnfp_depositdate").HasValue)
					{
						DateTime now = DateTime.Now;
						giftTransaction["msnfp_depositdate"] = now;
						localContext.TracingService.Trace("contains depositdate 2");
						Entity entity9 = new Entity(entity.LogicalName, entity.Id);
						entity9["msnfp_depositdate"] = now;
						organizationService.Update(entity9);
					}
					UpdateEventPackageDonationTotals(giftTransaction, organizationServiceContext, organizationService, localContext);
					UpdateEventTotals(giftTransaction, organizationServiceContext, organizationService, localContext);
					ReceiptUtilities.UpdateReceipt(pluginExecutionContext, organizationService, localContext.TracingService);
					AddOrUpdateThisRecordWithAzure(giftTransaction, entity2, localContext, organizationService, pluginExecutionContext);
					localContext.TracingService.Trace("Update the transaction receipt (if appicable).");
					UpdateTransactionReceiptStatus(giftTransaction.Id, localContext, organizationService);
				}
				if (giftTransaction.Attributes.ContainsKey("statuscode") && giftTransaction.GetAttributeValue<OptionSetValue>("statuscode").Value == 844060000 && giftTransaction.Attributes.ContainsKey("msnfp_customerid"))
				{
					localContext.TracingService.Trace("Transaction completed - initiate Giving level logic..");
					ProcessGivingLevelInstance(organizationService, giftTransaction.GetAttributeValue<EntityReference>("msnfp_customerid"), pluginExecutionContext, localContext.TracingService);
				}
				if (giftTransaction.Contains("msnfp_transaction_paymentscheduleid"))
				{
					AttemptToSetPledgeScheduleToCompleted(((EntityReference)giftTransaction["msnfp_transaction_paymentscheduleid"]).Id, ((OptionSetValue)giftTransaction["statuscode"]).Value, localContext, organizationService, organizationServiceContext);
					if (messageName == "Create")
					{
						SetParentPaymentScheduleCardTypeToChilds(giftTransaction, organizationService, organizationServiceContext, localContext);
					}
				}
				if (giftTransaction != null && giftTransaction.Contains("msnfp_customerid") && giftTransaction["msnfp_customerid"] != null)
				{
					PopulateMostRecentGiftDataToDonor(organizationService, organizationServiceContext, localContext, giftTransaction, messageName);
				}
				if (giftTransaction.Contains("msnfp_membershipinstanceid"))
				{
					UpdateCustomerPrimaryMembership(giftTransaction, localContext, organizationService, organizationServiceContext);
				}
				Plugins.PaymentProcesses.Utilities.UpdateHouseholdOnRecord(organizationService, giftTransaction, "msnfp_householdid", "msnfp_customerid");
			}
			if (messageName == "Delete")
			{
				ReceiptUtilities.UpdateReceipt(pluginExecutionContext, organizationService, localContext.TracingService);
				ColumnSet columnSet2 = ReturnTransactionColumnSet();
				entity = organizationService.Retrieve("msnfp_transaction", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, columnSet2);
				AddOrUpdateThisRecordWithAzure(entity, entity2, localContext, organizationService, pluginExecutionContext);
				if (entity != null && entity.Contains("msnfp_customerid") && entity["msnfp_customerid"] != null)
				{
					PopulateMostRecentGiftDataToDonor(organizationService, organizationServiceContext, localContext, entity, messageName);
				}
			}
			if (entity != null)
			{
				Plugins.Common.Utilities.CallYearlyGivingServiceAsync(entity.Id, entity.LogicalName, entity2.Id, organizationService, localContext.TracingService);
			}
			localContext.TracingService.Trace("---------Exiting TransactionGiftCreate.cs---------");
		}

		private void CreateAutoSoftCredits(Entity targetTransaction, Entity config, IOrganizationService service, ITracingService tracingService)
		{
			bool attributeValue = service.Retrieve(config.LogicalName, config.Id, new ColumnSet("msnfp_autocreate_softcredit")).GetAttributeValue<bool>("msnfp_autocreate_softcredit");
			int value = targetTransaction.GetAttributeValue<OptionSetValue>("msnfp_typecode").Value;
			if (attributeValue && value == 844060000)
			{
				ColumnSet donationColumnnsToCopy = ReturnTransactionColumnSet();
				if (targetTransaction.GetAttributeValue<EntityReference>("msnfp_relatedconstituentid") != null)
				{
					Entity entity = AutoSoftCredit.CreateSoftCredit(targetTransaction, donationColumnnsToCopy, targetTransaction.GetAttributeValue<EntityReference>("msnfp_relatedconstituentid"), service, tracingService);
					service.Create(entity);
				}
				if (targetTransaction.GetAttributeValue<EntityReference>("msnfp_solicitorid") != null)
				{
					Entity entity2 = AutoSoftCredit.CreateSoftCredit(targetTransaction, donationColumnnsToCopy, targetTransaction.GetAttributeValue<EntityReference>("msnfp_solicitorid"), service, tracingService);
					service.Create(entity2);
				}
			}
		}

		private bool CheckExecutionPipeLine(IPluginExecutionContext context)
		{
			bool flag = context.ParentContext != null;
			bool flag2 = context.ParentContext.PrimaryEntityName == "msnfp_donationimport" || (context.ParentContext.ParentContext != null && context.ParentContext.ParentContext.PrimaryEntityName == "msnfp_donationimport");
			bool flag3 = string.Compare(context.MessageName, "create", StringComparison.CurrentCultureIgnoreCase) == 0 && context.ParentContext.PrimaryEntityName == "msnfp_transaction";
			bool flag4 = context.ParentContext.ParentContext != null && context.ParentContext.ParentContext.PrimaryEntityName == "msnfp_refund";
			return flag && (flag2 || flag3 || flag4);
		}

		private ColumnSet ReturnTransactionColumnSet()
		{
			return new ColumnSet("msnfp_transactionid", "msnfp_name", "msnfp_transaction_paymentmethodid", "msnfp_amount", "statuscode", "msnfp_chargeoncreate", "msnfp_customerid", "msnfp_transactionidentifier", "msnfp_transactionnumber", "msnfp_paymenttypecode", "msnfp_firstname", "msnfp_lastname", "msnfp_billing_line1", "msnfp_billing_line2", "msnfp_billing_line3", "msnfp_billing_city", "msnfp_billing_stateorprovince", "msnfp_billing_country", "msnfp_billing_postalcode", "msnfp_chequenumber", "msnfp_chequewiredate", "msnfp_transactionresult", "msnfp_relatedconstituentid", "msnfp_transactionfraudcode", "msnfp_amount_receipted", "msnfp_bookdate", "msnfp_daterefunded", "msnfp_originatingcampaignid", "msnfp_appealid", "msnfp_anonymous", "msnfp_dataentrysource", "msnfp_telephone1", "msnfp_telephone2", "msnfp_emailaddress1", "msnfp_organizationname", "msnfp_transactiondescription", "msnfp_appraiser", "msnfp_transaction_paymentscheduleid", "msnfp_packageid", "msnfp_invoiceidentifier", "msnfp_configurationid", "msnfp_eventid", "msnfp_eventpackageid", "msnfp_amount_membership", "msnfp_ref_amount_membership", "msnfp_amount_transfer", "msnfp_currentretry", "msnfp_nextfailedretry", "msnfp_receiveddate", "msnfp_lastfailedretry", "msnfp_validationdate", "msnfp_validationperformed", "msnfp_amount", "statuscode", "statecode", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_ref_amount_receipted", "msnfp_ref_amount", "msnfp_ref_amount_nonreceiptable", "msnfp_ref_amount_tax", "msnfp_ga_amount_claimed", "msnfp_ccbrandcode", "msnfp_ga_deliverycode", "msnfp_ga_returnid", "msnfp_ga_applicablecode", "msnfp_giftbatchid", "msnfp_membershipinstanceid", "msnfp_membershipcategoryid", "msnfp_mobilephone", "msnfp_taxreceiptid", "msnfp_receiptpreferencecode", "msnfp_donorcommitmentid", "msnfp_returneddate", "msnfp_thirdpartyreceipt", "msnfp_dataentryreference", "msnfp_tributecode", "msnfp_tributeacknowledgement", "msnfp_tributeid", "msnfp_tributemessage", "createdon", "msnfp_paymentprocessorid", "transactioncurrencyid", "msnfp_depositdate", "owningbusinessunit", "msnfp_designationid", "msnfp_typecode");
		}

		private void ProcessGivingLevelInstance(IOrganizationService service, EntityReference customerRef, IPluginExecutionContext context, ITracingService tracingService)
		{
			Entity configurationRecordByUser = Plugins.PaymentProcesses.Utilities.GetConfigurationRecordByUser(context, service, tracingService);
			if (!(configurationRecordByUser?.Attributes.ContainsKey("msnfp_givinglevelcalculation") ?? false))
			{
				return;
			}
			int value = configurationRecordByUser.GetAttributeValue<OptionSetValue>("msnfp_givinglevelcalculation").Value;
			DateTime? dateTime = null;
			DateTime? dateTime2 = null;
			switch (value)
			{
			case 844060000:
				dateTime = new DateTime(DateTime.UtcNow.Year, 1, 1);
				dateTime2 = new DateTime(DateTime.UtcNow.Year, 12, 31);
				break;
			case 844060001:
				dateTime = new DateTime(DateTime.UtcNow.Year - 1, 3, 15);
				dateTime2 = new DateTime(DateTime.UtcNow.Year, 3, 14);
				break;
			case 844060002:
				dateTime = new DateTime(DateTime.UtcNow.Year, 1, 1);
				dateTime2 = DateTime.UtcNow.Date;
				break;
			}
			tracingService.Trace($"Fetching giving instance for : contact {customerRef.Id}");
			FilterExpression filterExpression = new FilterExpression();
			filterExpression.AddCondition(new ConditionExpression("msnfp_customerid", ConditionOperator.Equal, customerRef.Id));
			filterExpression.AddCondition(new ConditionExpression("msnfp_primary", ConditionOperator.Equal, true));
			if (dateTime.HasValue && dateTime2.HasValue)
			{
				filterExpression.AddCondition(new ConditionExpression("createdon", ConditionOperator.OnOrAfter, dateTime.Value));
				filterExpression.AddCondition(new ConditionExpression("createdon", ConditionOperator.OnOrBefore, dateTime2.Value));
			}
			IEnumerable<Entity> source = service.RetrieveMultiple(new QueryExpression("msnfp_givinglevelinstance")
			{
				NoLock = true,
				ColumnSet = new ColumnSet("msnfp_givinglevelinstanceid"),
				Criteria = filterExpression
			}).Entities.AsEnumerable();
			tracingService.Trace($"RetrieveMultiple giving instance count {source.Count()}");
			List<OrganizationRequest> list = new List<OrganizationRequest>();
			list.AddRange(source.Select((Entity g) => new UpdateRequest
			{
				Target = new Entity
				{
					LogicalName = g.LogicalName,
					Id = g.Id,
					Attributes = 
					{
						new KeyValuePair<string, object>("msnfp_primary", false)
					}
				}
			}));
			ExecuteWorkflowRequest executeWorkflowRequest = new ExecuteWorkflowRequest();
			executeWorkflowRequest.EntityId = customerRef.Id;
			if (string.Equals(customerRef.LogicalName, "contact", StringComparison.OrdinalIgnoreCase))
			{
				executeWorkflowRequest.WorkflowId = ContactGivingLevelWorkflowId;
			}
			else
			{
				executeWorkflowRequest.WorkflowId = AccountGivingLevelWorkflowId;
			}
			ExecuteMultipleRequest executeMultipleRequest = new ExecuteMultipleRequest
			{
				Settings = new ExecuteMultipleSettings
				{
					ContinueOnError = false,
					ReturnResponses = true
				},
				Requests = new OrganizationRequestCollection()
			};
			list.Add(executeWorkflowRequest);
			while (list.Any())
			{
				IEnumerable<OrganizationRequest> items = list.Take(25);
				list = list.Skip(25).ToList();
				executeMultipleRequest.Requests.AddRange(items);
				ExecuteMultipleResponse executeMultipleResponse = (ExecuteMultipleResponse)service.Execute(executeMultipleRequest);
				executeMultipleRequest.Requests = new OrganizationRequestCollection();
				if (executeMultipleResponse.IsFaulted)
				{
					tracingService.Trace("An error has occurred : " + executeMultipleResponse.Responses.FirstOrDefault((ExecuteMultipleResponseItem w) => w.Fault != null).Fault.Message);
				}
			}
		}

		private void PopultateDonorFieldsOnTransactionAndPaymentMethod(ref Entity giftTransaction, Guid ownerId, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Validating donor - start.");
			localContext.TracingService.Trace("Validating donor - start."+giftTransaction.ToString());
			if (!giftTransaction.Contains("msnfp_customerid"))
			{
				Entity entity = null;
				Guid empty = Guid.Empty;
				bool flag = false;
				Entity entity2 = null;
				Guid guid = Guid.Empty;
				bool flag2 = false;
				bool flag3 = false;
				localContext.TracingService.Trace("Validating Organization Name.");
				string text = (giftTransaction.Contains("msnfp_organizationname") ? ((string)giftTransaction["msnfp_organizationname"]) : string.Empty);
				if (!string.IsNullOrEmpty(text))
				{
					localContext.TracingService.Trace("Organization Name: " + text + ".");
					ColumnSet columnSet = new ColumnSet("name");
					List<Entity> list = new List<Entity>();
					QueryExpression queryExpression = new QueryExpression("account");
					queryExpression.ColumnSet = columnSet;
					queryExpression.Criteria = new FilterExpression();
					queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
					queryExpression.Criteria.FilterOperator = LogicalOperator.And;
					FilterExpression filterExpression = queryExpression.Criteria.AddFilter(LogicalOperator.And);
					filterExpression.AddCondition("name", ConditionOperator.Equal, text);
					list = service.RetrieveMultiple(queryExpression).Entities.ToList();
					if (list.Count > 0)
					{
						localContext.TracingService.Trace("Account found.");
						entity = list.FirstOrDefault();
						giftTransaction["msnfp_customerid"] = new EntityReference("account", entity.Id);
						empty = entity.Id;
						flag = true;
						flag3 = true;
					}
					else
					{
						localContext.TracingService.Trace("No account found, creating new record.");
						entity = new Entity("account");
						entity["name"] = text;
						if (ownerId != Guid.Empty)
						{
							entity["ownerid"] = new EntityReference("team", ownerId);
							localContext.TracingService.Trace("account ownerid: " + ownerId.ToString());
						}
						else
						{
							localContext.TracingService.Trace("account ownerid not found");
						}
						empty = service.Create(entity);
						localContext.TracingService.Trace("Account created and set as Donor.");
						if (empty != Guid.Empty)
						{
							giftTransaction["msnfp_customerid"] = new EntityReference("account", empty);
							flag = true;
							flag3 = true;
						}
					}
				}
				localContext.TracingService.Trace("Account validation completed.");
				localContext.TracingService.Trace("Validating Contact.");
				string text2 = (giftTransaction.Contains("msnfp_firstname") ? ((string)giftTransaction["msnfp_firstname"]) : string.Empty);
				string text3 = (giftTransaction.Contains("msnfp_lastname") ? ((string)giftTransaction["msnfp_lastname"]) : string.Empty);
				if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3))
				{
					localContext.TracingService.Trace("First Name: " + text2 + " Last Name: " + text3 + ".");
					string text4 = (giftTransaction.Contains("msnfp_emailaddress1") ? ((string)giftTransaction["msnfp_emailaddress1"]) : string.Empty);
					string text5 = (giftTransaction.Contains("msnfp_firstname") ? ((string)giftTransaction["msnfp_firstname"]) : string.Empty);
					string text6 = (giftTransaction.Contains("msnfp_lastname") ? ((string)giftTransaction["msnfp_lastname"]) : string.Empty);
					string value = (giftTransaction.Contains("msnfp_billing_postalcode") ? ((string)giftTransaction["msnfp_billing_postalcode"]) : string.Empty);
					string value2 = (giftTransaction.Contains("msnfp_billing_city") ? ((string)giftTransaction["msnfp_billing_city"]) : string.Empty);
					string value3 = (giftTransaction.Contains("msnfp_billing_line1") ? ((string)giftTransaction["msnfp_billing_line1"]) : string.Empty);
					string value4 = (giftTransaction.Contains("msnfp_billing_line2") ? ((string)giftTransaction["msnfp_billing_line2"]) : string.Empty);
					string value5 = (giftTransaction.Contains("msnfp_billing_line3") ? ((string)giftTransaction["msnfp_billing_line3"]) : string.Empty);
					string value6 = (giftTransaction.Contains("msnfp_billing_stateorprovince") ? ((string)giftTransaction["msnfp_billing_stateorprovince"]) : string.Empty);
					string value7 = (giftTransaction.Contains("msnfp_billing_country") ? ((string)giftTransaction["msnfp_billing_country"]) : string.Empty);
					string value8 = (giftTransaction.Contains("msnfp_telephone1") ? ((string)giftTransaction["msnfp_telephone1"]) : string.Empty);
					string value9 = (giftTransaction.Contains("msnfp_telephone2") ? ((string)giftTransaction["msnfp_telephone2"]) : string.Empty);
					string value10 = (giftTransaction.Contains("msnfp_mobilephone") ? ((string)giftTransaction["msnfp_mobilephone"]) : string.Empty);
					ColumnSet columnSet2 = new ColumnSet("contactid", "firstname", "lastname", "middlename", "firstname", "birthdate", "emailaddress1", "emailaddress2", "emailaddress3", "telephone1", "mobilephone", "gendercode", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode");
					List<Entity> list2 = new List<Entity>();
					QueryExpression queryExpression2 = new QueryExpression("contact");
					queryExpression2.ColumnSet = columnSet2;
					queryExpression2.Criteria = new FilterExpression();
					queryExpression2.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
					queryExpression2.Criteria.FilterOperator = LogicalOperator.And;
					if (!string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(text5) && !string.IsNullOrEmpty(text6))
					{
						FilterExpression filterExpression2 = queryExpression2.Criteria.AddFilter(LogicalOperator.Or);
						filterExpression2.AddCondition("emailaddress1", ConditionOperator.Equal, text4);
						filterExpression2.AddCondition("emailaddress2", ConditionOperator.Equal, text4);
						filterExpression2.AddCondition("emailaddress3", ConditionOperator.Equal, text4);
						FilterExpression filterExpression3 = queryExpression2.Criteria.AddFilter(LogicalOperator.And);
						filterExpression3.AddCondition("firstname", ConditionOperator.BeginsWith, text5.Substring(0, 1));
						filterExpression3.AddCondition("lastname", ConditionOperator.BeginsWith, text6);
						list2 = service.RetrieveMultiple(queryExpression2).Entities.ToList();
					}
					if (list2.Count > 0)
					{
						localContext.TracingService.Trace("Contact by email found.");
						entity2 = list2.FirstOrDefault();
					}
					else
					{
						localContext.TracingService.Trace("No Contact found by email.");
						list2 = new List<Entity>();
						FilterExpression filterExpression4 = new FilterExpression();
						if (!string.IsNullOrEmpty(text6) && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(value2) && !string.IsNullOrEmpty(text5) && !string.IsNullOrEmpty(value3))
						{
							filterExpression4.Conditions.Add(new ConditionExpression("lastname", ConditionOperator.Equal, text6));
							filterExpression4.Conditions.Add(new ConditionExpression("address1_postalcode", ConditionOperator.Equal, value));
							filterExpression4.Conditions.Add(new ConditionExpression("address1_city", ConditionOperator.Equal, value2));
							filterExpression4.Conditions.Add(new ConditionExpression("address1_line1", ConditionOperator.Equal, value3));
							filterExpression4.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.BeginsWith, text5.Substring(0, 1)));
							filterExpression4.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
							filterExpression4.FilterOperator = LogicalOperator.And;
							queryExpression2.Criteria = filterExpression4;
							list2 = service.RetrieveMultiple(queryExpression2).Entities.ToList();
						}
						if (list2.Count > 0)
						{
							entity2 = list2.FirstOrDefault();
						}
						else
						{
							list2 = new List<Entity>();
							FilterExpression filterExpression5 = new FilterExpression();
							if (!string.IsNullOrEmpty(text6) && !string.IsNullOrEmpty(value3) && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(text5))
							{
								filterExpression5.Conditions.Add(new ConditionExpression("lastname", ConditionOperator.Equal, text6));
								filterExpression5.Conditions.Add(new ConditionExpression("address1_line1", ConditionOperator.Equal, value3));
								filterExpression5.Conditions.Add(new ConditionExpression("address1_postalcode", ConditionOperator.Equal, value));
								filterExpression5.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.BeginsWith, text5.Substring(0, 1)));
								filterExpression5.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
								filterExpression5.FilterOperator = LogicalOperator.And;
								queryExpression2.Criteria = filterExpression5;
								list2 = service.RetrieveMultiple(queryExpression2).Entities.ToList();
							}
							if (list2.Count > 0)
							{
								entity2 = list2.FirstOrDefault();
							}
						}
					}
					if (entity2 != null)
					{
						guid = entity2.Id;
						localContext.TracingService.Trace("Found customer based on search criteria and set as Donor.");
						flag2 = true;
					}
					else
					{
						entity2 = new Entity("contact");
						entity2["lastname"] = text6;
						entity2["firstname"] = text5;
						entity2["emailaddress1"] = text4;
						entity2["telephone1"] = value8;
						entity2["telephone2"] = value9;
						entity2["mobilephone"] = value10;
						entity2["address1_line1"] = value3;
						entity2["address1_line2"] = value4;
						entity2["address1_line3"] = value5;
						entity2["address1_city"] = value2;
						entity2["address1_stateorprovince"] = value6;
						entity2["address1_country"] = value7;
						entity2["address1_postalcode"] = value;
						if (ownerId != Guid.Empty)
						{
							entity2["ownerid"] = new EntityReference("team", ownerId);
							localContext.TracingService.Trace("contact ownerid: " + ownerId.ToString());
						}
						else
						{
							localContext.TracingService.Trace("contact ownerid not found");
						}
						guid = service.Create(entity2);
						flag2 = true;
					}
				}
				localContext.TracingService.Trace("Contact validation completed.");
				if (flag2 && guid != Guid.Empty)
				{
					localContext.TracingService.Trace("Assigning Contact.");
					if (flag)
					{
						if (!giftTransaction.Contains("msnfp_relatedconstituentid"))
						{
							giftTransaction["msnfp_relatedconstituentid"] = new EntityReference("contact", guid);
							flag3 = true;
						}
					}
					else
					{
						giftTransaction["msnfp_customerid"] = new EntityReference("contact", guid);
						flag3 = true;
					}
				}
				localContext.TracingService.Trace("Updating transaction record.");
				if (flag3)
				{
					localContext.TracingService.Trace("transactionUpdatedYN");
					service.Update(giftTransaction);
					ColumnSet columnSet3 = ReturnTransactionColumnSet();
					giftTransaction = service.Retrieve("msnfp_transaction", giftTransaction.Id, columnSet3);
					localContext.TracingService.Trace("Transaction record updated.");
				}
			}
			if (!giftTransaction.Contains("msnfp_transaction_paymentmethodid") || giftTransaction.GetAttributeValue<EntityReference>("msnfp_transaction_paymentmethodid") == null)
			{
				return;
			}
			Entity entity3 = service.Retrieve("msnfp_paymentmethod", giftTransaction.GetAttributeValue<EntityReference>("msnfp_transaction_paymentmethodid").Id, new ColumnSet("msnfp_customerid"));
			if ((entity3.Contains("msnfp_customerid") && entity3.GetAttributeValue<EntityReference>("msnfp_customerid") != null) || giftTransaction.GetAttributeValue<EntityReference>("msnfp_customerid") == null)
			{
				return;
			}
			localContext.TracingService.Trace("Copying Customer from Transaction to Payment Method");
			Entity entity4 = new Entity(entity3.LogicalName, entity3.Id);
			entity4["msnfp_customerid"] = giftTransaction["msnfp_customerid"];
			try
			{
				service.Update(entity4);
				localContext.TracingService.Trace("Copied Customer (id:" + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString() + ") from Transaction to Payment Method");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("Could not Copy Customer to Payment Method:");
				localContext.TracingService.Trace("Exception:" + ex.Message);
				if (ex.InnerException != null)
				{
					localContext.TracingService.Trace("Inner Exception:" + ex.InnerException.Message);
				}
			}
		}

		private static Guid? CreateTributeRecord(LocalPluginContext localContext, Entity targetTransaction, IOrganizationService service)
		{
			Guid? result = null;
			if (targetTransaction.GetAttributeValue<string>("msnfp_tributename") != null && targetTransaction.GetAttributeValue<OptionSetValue>("msnfp_tributecode") != null)
			{
				localContext.TracingService.Trace("Creating new Tribute Record.");
				string attributeValue = targetTransaction.GetAttributeValue<string>("msnfp_tributename");
				int value = targetTransaction.GetAttributeValue<OptionSetValue>("msnfp_tributecode").Value;
				localContext.TracingService.Trace("Tribute Name:" + attributeValue);
				localContext.TracingService.Trace("Tribute Code:" + value);
				Entity entity = new Entity("msnfp_tributeormemory");
				entity["msnfp_name"] = attributeValue;
				entity["msnfp_identifier"] = attributeValue;
				entity["msnfp_tributeormemorytypecode"] = new OptionSetValue(value);
				result = service.Create(entity);
				localContext.TracingService.Trace("Tribute Record Created.");
				localContext.TracingService.Trace("Updating Transaction with the Tribute'.");
				Entity entity2 = new Entity(targetTransaction.LogicalName, targetTransaction.Id);
				entity2["msnfp_tributeid"] = new EntityReference(entity.LogicalName, result.Value);
				service.Update(entity2);
				localContext.TracingService.Trace("Transaction Updated Initial Tribute");
			}
			return result;
		}

		private static void CreateDesignatedCredit(LocalPluginContext localContext, IOrganizationService service, Entity targetTransaction)
		{
			if (targetTransaction.Contains("msnfp_designationid") && targetTransaction["msnfp_designationid"] != null)
			{
				localContext.TracingService.Trace("Creating Designated Credit");
				EntityReference attributeValue = targetTransaction.GetAttributeValue<EntityReference>("msnfp_designationid");
				Entity entity = service.Retrieve(attributeValue.LogicalName, attributeValue.Id, new ColumnSet("msnfp_name"));
				localContext.TracingService.Trace("Primary Designation ID:" + attributeValue.Id.ToString());
				localContext.TracingService.Trace("Primary Designation Name:" + entity.GetAttributeValue<string>("msnfp_name"));
				Money money = ((targetTransaction.GetAttributeValue<Money>("msnfp_amount") != null) ? targetTransaction.GetAttributeValue<Money>("msnfp_amount") : new Money(0m));
				DateTime attributeValue2 = targetTransaction.GetAttributeValue<DateTime>("msnfp_bookdate");
				DateTime attributeValue3 = targetTransaction.GetAttributeValue<DateTime>("msnfp_receiveddate");
				Entity entity2 = new Entity("msnfp_designatedcredit");
				entity2["msnfp_transactionid"] = new EntityReference(targetTransaction.LogicalName, targetTransaction.Id);
				entity2["msnfp_designatiedcredit_designationid"] = new EntityReference(entity.LogicalName, entity.Id);
				if (money.Value > 0m)
				{
					entity2["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_name") + "-$" + money.Value;
					entity2["msnfp_amount"] = money;
				}
				else
				{
					entity2["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_name");
				}
				if (attributeValue2 != default(DateTime))
				{
					entity2["msnfp_bookdate"] = attributeValue2;
				}
				if (attributeValue3 != default(DateTime))
				{
					entity2["msnfp_receiveddate"] = attributeValue3;
				}
				service.Create(entity2);
				localContext.TracingService.Trace("Created Designated Credit");
			}
		}

		private void SetParentPaymentScheduleCardTypeToChilds(Entity giftTransaction, IOrganizationService service, OrganizationServiceContext orgSvcContext, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------Entering SetParentPaymentScheduleCardTypeToChilds()---------");
			if (giftTransaction.Contains("msnfp_transaction_paymentscheduleid") && giftTransaction["msnfp_transaction_paymentscheduleid"] != null && giftTransaction.Contains("msnfp_ccbrandcode") && giftTransaction["msnfp_ccbrandcode"] != null)
			{
				try
				{
					localContext.TracingService.Trace("Updating parent payment schedule (" + ((EntityReference)giftTransaction["msnfp_transaction_paymentscheduleid"]).Id.ToString() + ") card brand with transaction information.");
					ColumnSet columnSet = new ColumnSet("msnfp_paymentscheduleid", "msnfp_ccbrandcode");
					Entity entity = service.Retrieve("msnfp_paymentschedule", ((EntityReference)giftTransaction["msnfp_transaction_paymentscheduleid"]).Id, columnSet);
					localContext.TracingService.Trace("Card Brand Code = " + ((OptionSetValue)giftTransaction["msnfp_ccbrandcode"]).Value);
					entity["msnfp_ccbrandcode"] = (OptionSetValue)giftTransaction["msnfp_ccbrandcode"];
					service.Update(entity);
					localContext.TracingService.Trace("Update of Payment Schedule complete.");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error: " + ex.Message.ToString());
				}
			}
			localContext.TracingService.Trace("---------Exiting SetParentPaymentScheduleCardTypeToChilds()---------");
		}

		private void PopulateMostRecentGiftDataToDonor(IOrganizationService organizationService, OrganizationServiceContext orgSvcContext, LocalPluginContext localContext, Entity giftTransaction, string messageName)
		{
			localContext.TracingService.Trace("----- Populating The Most Recent Gift To the according Donor -----");
			EntityReference donor = giftTransaction.GetAttributeValue<EntityReference>("msnfp_customerid");
			if (donor == null)
			{
				return;
			}
			if (donor.LogicalName == "contact" || donor.LogicalName == "account")
			{
				Entity entity = organizationService.Retrieve(donor.LogicalName, donor.Id, new ColumnSet("msnfp_lasttransactionid"));
				if (string.Compare(messageName, "Delete", ignoreCase: true) == 0)
				{
					localContext.TracingService.Trace("Message is Delete. Locating the previous Transaction.");
					localContext.TracingService.Trace("Current TransactionId=" + giftTransaction.Id.ToString());
					Entity entity2 = (from c in orgSvcContext.CreateQuery("msnfp_transaction")
						where ((EntityReference)c["msnfp_customerid"]).Id == donor.Id && (Guid)c["msnfp_transactionid"] != giftTransaction.Id
						orderby c["msnfp_bookdate"] descending, c["createdon"] descending
						select c).FirstOrDefault();
					if (entity2 != null)
					{
						entity["msnfp_lasttransactionid"] = new EntityReference(entity2.LogicalName, entity2.Id);
						entity["msnfp_lasttransactiondate"] = (DateTime)entity2["msnfp_bookdate"];
						organizationService.Update(entity);
					}
				}
				else if (entity.Contains("msnfp_lasttransactionid") && entity["msnfp_lasttransactionid"] != null)
				{
					Entity entity3 = (from c in orgSvcContext.CreateQuery("msnfp_transaction")
						where ((EntityReference)c["msnfp_customerid"]).Id == donor.Id && (c.GetAttributeValue<OptionSetValue>("msnfp_typecode") == null || (c.GetAttributeValue<OptionSetValue>("msnfp_typecode") != null && c.GetAttributeValue<OptionSetValue>("msnfp_typecode").Value != 844060001))
						orderby c["msnfp_bookdate"] descending, c["createdon"] descending
						select c).FirstOrDefault();
					if (entity3 != null)
					{
						entity["msnfp_lasttransactionid"] = new EntityReference(entity3.LogicalName, entity3.Id);
						if (entity3.Attributes.ContainsKey("msnfp_bookdate"))
						{
							entity["msnfp_lasttransactiondate"] = entity3.GetAttributeValue<DateTime>("msnfp_bookdate");
						}
						organizationService.Update(entity);
					}
				}
				else if (giftTransaction.GetAttributeValue<OptionSetValue>("msnfp_typecode") == null || giftTransaction.GetAttributeValue<OptionSetValue>("msnfp_typecode").Value != 844060001)
				{
					entity["msnfp_lasttransactionid"] = new EntityReference(giftTransaction.LogicalName, giftTransaction.Id);
					if (giftTransaction.Attributes.ContainsKey("msnfp_bookdate"))
					{
						entity["msnfp_lasttransactiondate"] = giftTransaction.GetAttributeValue<DateTime>("msnfp_bookdate");
					}
					organizationService.Update(entity);
				}
			}
			localContext.TracingService.Trace("----- Finished Populating The Most Recent Gift To the according Donor -----");
		}

		private void processStripeTransaction(Entity configurationRecord, Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service, bool singleTransactionYN)
		{
			string empty = string.Empty;
			string text = string.Empty;
			Entity entity = null;
			string text2 = "";
			string empty2 = string.Empty;
			Guid empty3 = Guid.Empty;
			Entity entity2 = null;
			Entity entity3 = null;
			decimal d = default(decimal);
			bool flag = false;
			string text3 = Guid.NewGuid().ToString();
			string str = "";
			if (giftTransaction.Contains("transactioncurrencyid") && giftTransaction["transactioncurrencyid"] != null)
			{
				Entity entity4 = service.Retrieve("transactioncurrency", ((EntityReference)giftTransaction["transactioncurrencyid"]).Id, new ColumnSet("isocurrencycode"));
				if (entity4 != null)
				{
					text = (entity4.Contains("isocurrencycode") ? ((string)entity4["isocurrencycode"]) : string.Empty);
				}
			}
			int num = (configurationRecord.Contains("msnfp_sche_retryinterval") ? ((int)configurationRecord["msnfp_sche_retryinterval"]) : 0);
			try
			{
				StripeCustomer stripeCustomer = null;
				string text4 = null;
				BaseStipeRepository baseStipeRepository = new BaseStipeRepository();
				entity = getPaymentMethodForTransaction(giftTransaction, localContext, service);
				if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
				{
					localContext.TracingService.Trace("processStripeTransaction - Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
					if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
					{
						setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
					}
					return;
				}
				if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
				{
					localContext.TracingService.Trace("processStripeTransaction - Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
					removePaymentMethod(entity, localContext, service);
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
					return;
				}
				entity3 = getPaymentProcessorForPaymentMethod(entity, giftTransaction, localContext, service);
				string text5 = entity3["msnfp_stripeservicekey"].ToString();
				StripeConfiguration.SetApiKey(text5);
				if (giftTransaction.Contains("msnfp_customerid"))
				{
					empty2 = ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName;
					empty3 = ((EntityReference)giftTransaction["msnfp_customerid"]).Id;
					entity2 = ((!(empty2 == "account")) ? service.Retrieve("contact", empty3, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "mobilephone", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")) : service.Retrieve("account", empty3, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "telephone3", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")));
				}
				if (entity.Contains("msnfp_stripecustomerid") && entity["msnfp_stripecustomerid"] != null && entity.Contains("msnfp_authtoken") && entity["msnfp_authtoken"] != null)
				{
					localContext.TracingService.Trace("processStripeTransaction - Existing Card use");
					string customerId = entity["msnfp_stripecustomerid"].ToString();
					text4 = entity["msnfp_authtoken"].ToString();
					int? num2 = ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value;
					if (num2.HasValue)
					{
						text2 = num2 switch
						{
							844060001 => "MasterCard", 
							844060000 => "Visa", 
							844060004 => "American Express", 
							844060008 => "Discover", 
							844060005 => "Diners Club", 
							844060009 => "UnionPay", 
							844060006 => "JCB", 
							_ => "Unknown", 
						};
					}
					StripeConfiguration.SetApiKey(text5);
					StripeCustomerService stripeCustomerService = new StripeCustomerService();
					stripeCustomer = stripeCustomerService.Get(customerId);
				}
				else
				{
					localContext.TracingService.Trace("processStripeTransaction - New Card use");
					flag = true;
					string custName = ((entity2.LogicalName == "account") ? entity2["name"].ToString() : (entity2["firstname"].ToString() + entity2["lastname"].ToString()));
					string custEmail = (entity2.Contains("emailaddress1") ? entity2["emailaddress1"].ToString() : string.Empty);
					localContext.TracingService.Trace("processStripeTransaction - extracting customer info - done");
					stripeCustomer = new CustomerService().GetStripeCustomer(custName, custEmail, text5);
					localContext.TracingService.Trace("processStripeTransaction - obtained stripeCustomer");
					StripeTokenCreateOptions stripeTokenCreateOptions = new StripeTokenCreateOptions();
					string text6 = (entity.Contains("msnfp_ccexpmmyy") ? entity["msnfp_ccexpmmyy"].ToString() : string.Empty);
					stripeTokenCreateOptions.Card = new StripeCreditCardOptions
					{
						Number = entity["msnfp_cclast4"].ToString(),
						ExpirationYear = text6.Substring(text6.Length - 2),
						ExpirationMonth = text6.Substring(0, text6.Length - 2)
					};
					StripeTokenService stripeTokenService = new StripeTokenService();
					StripeToken stripeToken = stripeTokenService.Create(stripeTokenCreateOptions);
					StripeCard stripeCard = new StripeCard();
					stripeCard.SourceToken = stripeToken.Id;
					string url = $"https://api.stripe.com/v1/customers/{stripeCustomer.Id}/sources";
					StripeCard stripeCard2 = baseStipeRepository.Create(stripeCard, url, text5);
					if (string.IsNullOrEmpty(stripeCard2.Id))
					{
						throw new Exception("processStripeTransaction - Unable to add card to customer");
					}
					text4 = stripeCard2.Id;
					text2 = stripeCard2.Brand;
					localContext.TracingService.Trace("processStripeTransaction - Card Id");
					MaskStripeCreditCard(localContext, entity, text4, text2, stripeCustomer.Id);
				}
				if (giftTransaction.Contains("msnfp_amount"))
				{
					d = ((Money)giftTransaction["msnfp_amount"]).Value;
				}
				int amount = Convert.ToInt32((d * 100m).ToString().Split('.')[0]);
				StripeCharge stripeCharge = new StripeCharge();
				stripeCharge.Amount = amount;
				stripeCharge.Currency = (string.IsNullOrEmpty(text) ? "CAD" : text);
				stripeCharge.Customer = stripeCustomer;
				Source source = new Source();
				source.Id = text4;
				stripeCharge.Source = source;
				stripeCharge.Description = (giftTransaction.Contains("msnfp_invoiceidentifier") ? ((string)giftTransaction["msnfp_invoiceidentifier"]) : text3);
				StripeCharge stripeCharge2 = baseStipeRepository.Create(stripeCharge, "https://api.stripe.com/v1/charges", text5);
				Entity entity5 = new Entity("msnfp_response");
				entity5["msnfp_transactionid"] = new EntityReference("msnfp_transaction", (Guid)giftTransaction["msnfp_transactionid"]);
				entity5["msnfp_identifier"] = "Response for " + (string)giftTransaction["msnfp_name"];
				if (!string.IsNullOrEmpty(stripeCharge2.FailureMessage))
				{
					entity5["msnfp_response"] = "FAILED";
					giftTransaction["statuscode"] = new OptionSetValue(844060003);
					giftTransaction["msnfp_lastfailedretry"] = DateTime.Now.ToLocalTime().Date;
					giftTransaction["msnfp_currentretry"] = 0;
					giftTransaction["msnfp_nextfailedretry"] = DateTime.Now.AddDays(num).ToLocalTime().Date;
					giftTransaction["msnfp_transactionresult"] = "FAILED";
				}
				else
				{
					localContext.TracingService.Trace("processStripeOneTimeTransaction - stripePayment.Id : " + stripeCharge2.Id);
					if (stripeCharge2 != null)
					{
						localContext.TracingService.Trace("processStripeOneTimeTransaction - stripePayment.InvoiceId : " + stripeCharge2.InvoiceId);
						localContext.TracingService.Trace("processStripeOneTimeTransaction - stripePayment.Status : " + stripeCharge2.Status.ToString());
						if (giftTransaction.Contains("msnfp_parenttransactionid"))
						{
							entity5["msnfp_transactionid"] = new EntityReference("msnfp_transaction", ((EntityReference)giftTransaction["msnfp_parenttransactionid"]).Id);
						}
						if (stripeCharge2.Status.Equals("succeeded"))
						{
							str = str + "---------Start Stripe Response---------" + Environment.NewLine;
							str = str + "TransAmount = " + stripeCharge2.Status + Environment.NewLine;
							str = str + "TransAmount = " + d + Environment.NewLine;
							str = str + "Auth Token = " + text4 + Environment.NewLine;
							str += "---------End Stripe Response---------";
							localContext.TracingService.Trace("processStripeTransaction - Got successful response from Stripe payment gateway.");
							entity5["msnfp_response"] = str;
							giftTransaction["msnfp_transactionresult"] = stripeCharge2.Status;
							giftTransaction["msnfp_transactionidentifier"] = stripeCharge2.Id;
							giftTransaction["statuscode"] = new OptionSetValue(844060000);
							if (text2 != null)
							{
								localContext.TracingService.Trace("Card Type Response Code = " + text2);
								switch (text2)
								{
								case "MasterCard":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060001);
									break;
								case "Visa":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060000);
									break;
								case "American Express":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060004);
									break;
								case "Discover":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060008);
									break;
								case "Diners Club":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060005);
									break;
								case "UnionPay":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060009);
									break;
								case "JCB":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
									break;
								default:
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060010);
									break;
								}
							}
							if (entity2 != null)
							{
								entity2["msnfp_lasttransactionid"] = new EntityReference("msnfp_transaction", (Guid)giftTransaction["msnfp_transactionid"]);
								entity2["msnfp_lasttransactiondate"] = (giftTransaction.Contains("createdon") ? ((DateTime)giftTransaction["createdon"]) : DateTime.MinValue);
								service.Update(entity2);
							}
							localContext.TracingService.Trace("processStripeTransaction - Updated Transaction Record.");
						}
						else
						{
							localContext.TracingService.Trace("processStripeTransaction - Got failure response from payment gateway.");
							entity5["msnfp_response"] = stripeCharge2.StripeResponse.ToString();
							giftTransaction["statuscode"] = new OptionSetValue(844060003);
							localContext.TracingService.Trace("processStripeTransaction - Status code updated to failed");
							giftTransaction["msnfp_lastfailedretry"] = DateTime.Now.ToLocalTime().Date;
							giftTransaction["msnfp_currentretry"] = 0;
							giftTransaction["msnfp_nextfailedretry"] = DateTime.Now.AddDays(num).ToLocalTime().Date;
							giftTransaction["msnfp_transactionidentifier"] = stripeCharge2.Id;
							giftTransaction["msnfp_transactionresult"] = stripeCharge2.Status;
							localContext.TracingService.Trace("Gateway Response Message." + stripeCharge2.Status);
						}
					}
				}
				giftTransaction["msnfp_invoiceidentifier"] = text3;
				Guid id = service.Create(entity5);
				bool flag2 = true;
				giftTransaction["msnfp_responseid"] = new EntityReference("msnfp_response", id);
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("processStripeTransaction - error : " + ex.Message);
				giftTransaction["statuscode"] = new OptionSetValue(844060003);
				localContext.TracingService.Trace("processStripeTransaction - Status code updated to failed");
				giftTransaction["msnfp_lastfailedretry"] = DateTime.Now.ToLocalTime().Date;
				giftTransaction["msnfp_currentretry"] = 0;
				giftTransaction["msnfp_nextfailedretry"] = DateTime.Now.AddDays(num).ToLocalTime().Date;
				giftTransaction["msnfp_transactionresult"] = "FAILED";
				if (giftTransaction.Contains("msnfp_parenttransactionid") && giftTransaction["msnfp_parenttransactionid"] != null)
				{
					localContext.TracingService.Trace("processStripeTransaction - payment fails remove credit card from parent as well");
					Entity entity6 = service.Retrieve("msnfp_transaction", ((EntityReference)giftTransaction["msnfp_transactionid"]).Id, new ColumnSet("msnfp_transaction_paymentmethodid"));
					if (entity6 != null && entity6.Contains("msnfp_transaction_paymentmethodid") && entity6["msnfp_transaction_paymentmethodid"] != null)
					{
						entity6["msnfp_transaction_paymentmethodid"] = null;
						localContext.TracingService.Trace("msnfp_transaction_paymentmethodid");
						service.Update(entity6);
						localContext.TracingService.Trace("processStripeTransaction - parent gift updated. removed Credit card");
					}
				}
			}
			if (singleTransactionYN)
			{
				giftTransaction["msnfp_transaction_paymentmethodid"] = null;
				if (flag)
				{
					removePaymentMethod(entity, localContext, service);
				}
			}
			localContext.TracingService.Trace("stripe");
			service.Update(giftTransaction);
			localContext.TracingService.Trace("processStripeTransaction - Entity Updated.");
		}

		private void ProcessiATSTransaction(Entity configurationRecord, Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service, bool singleTransactionYN)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			Entity entity = null;
			string empty3 = string.Empty;
			Guid empty4 = Guid.Empty;
			Entity entity2 = null;
			Entity entity3 = null;
			decimal num = default(decimal);
			string text = Guid.NewGuid().ToString();
			string text2 = "";
			string agentCode = string.Empty;
			string password = string.Empty;
			XmlDocument xmlDocument = null;
			string text3 = null;
			bool flag = false;
			if (giftTransaction.Contains("transactioncurrencyid") && giftTransaction["transactioncurrencyid"] != null)
			{
				localContext.TracingService.Trace("Getting transaction currency.");
				Entity entity4 = service.Retrieve("transactioncurrency", ((EntityReference)giftTransaction["transactioncurrencyid"]).Id, new ColumnSet("isocurrencycode"));
				if (entity4 != null)
				{
					empty2 = (entity4.Contains("isocurrencycode") ? ((string)entity4["isocurrencycode"]) : string.Empty);
				}
			}
			int num2 = (configurationRecord.Contains("msnfp_sche_retryinterval") ? ((int)configurationRecord["msnfp_sche_retryinterval"]) : 0);
			try
			{
				entity = getPaymentMethodForTransaction(giftTransaction, localContext, service);
				localContext.TracingService.Trace("Payment method retrieved");
				entity3 = getPaymentProcessorForPaymentMethod(entity, giftTransaction, localContext, service);
				localContext.TracingService.Trace("Payment processor retrieved.");
				if (entity3 != null)
				{
					agentCode = entity3.GetAttributeValue<string>("msnfp_iatsagentcode");
					password = entity3.GetAttributeValue<string>("msnfp_iatspassword");
				}
				if (giftTransaction.Contains("msnfp_customerid"))
				{
					empty3 = ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName;
					empty4 = ((EntityReference)giftTransaction["msnfp_customerid"]).Id;
					entity2 = ((!(empty3 == "account")) ? service.Retrieve("contact", empty4, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "mobilephone", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")) : service.Retrieve("account", empty4, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "telephone2", "telephone3", "emailaddress1", "msnfp_lasttransactionid", "msnfp_lasttransactiondate", "msnfp_vip", "ownerid")));
				}
				if (giftTransaction.Contains("msnfp_amount"))
				{
					num = ((Money)giftTransaction["msnfp_amount"]).Value;
				}
				if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value == 844060000)
				{
					localContext.TracingService.Trace("iATS credit card payment.");
					if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
					{
						localContext.TracingService.Trace("processiATSTransaction - Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
						removePaymentMethod(entity, localContext, service);
						setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
						return;
					}
					if (entity.Contains("msnfp_authtoken"))
					{
						text3 = entity["msnfp_authtoken"] as string;
					}
					else
					{
						localContext.TracingService.Trace("Create new customer for iATS payment.");
						flag = true;
						string text4 = (entity.Contains("msnfp_ccexpmmyy") ? entity.GetAttributeValue<string>("msnfp_ccexpmmyy") : string.Empty);
						string str = text4.Substring(text4.Length - 2);
						string str2 = text4.Substring(0, text4.Length - 2);
						text4 = str2 + "/" + str;
						string creditCardNum = (entity.Contains("msnfp_cclast4") ? entity.GetAttributeValue<string>("msnfp_cclast4") : string.Empty);
						CreateCreditCardCustomerCode createCreditCardCustomerCode = new CreateCreditCardCustomerCode();
						createCreditCardCustomerCode.lastName = ((entity2.LogicalName == "contact") ? entity2.GetAttributeValue<string>("lastname") : string.Empty);
						createCreditCardCustomerCode.firstName = ((entity2.LogicalName == "account") ? entity2.GetAttributeValue<string>("name") : entity2.GetAttributeValue<string>("firstname"));
						createCreditCardCustomerCode.agentCode = agentCode;
						createCreditCardCustomerCode.password = password;
						createCreditCardCustomerCode.beginDate = DateTime.Today;
						createCreditCardCustomerCode.endDate = DateTime.Today.AddDays(1.0);
						createCreditCardCustomerCode.country = entity2.GetAttributeValue<string>("address1_country");
						createCreditCardCustomerCode.creditCardExpiry = text4;
						createCreditCardCustomerCode.creditCardNum = creditCardNum;
						createCreditCardCustomerCode.recurring = false;
						createCreditCardCustomerCode.address = entity2.GetAttributeValue<string>("address1_line1");
						createCreditCardCustomerCode.city = entity2.GetAttributeValue<string>("address1_city");
						createCreditCardCustomerCode.zipCode = entity2.GetAttributeValue<string>("address1_postalcode");
						createCreditCardCustomerCode.state = entity2.GetAttributeValue<string>("address1_stateorprovince");
						createCreditCardCustomerCode.email = entity2.GetAttributeValue<string>("emailaddress1");
						createCreditCardCustomerCode.creditCardCustomerName = ((entity2.LogicalName == "account") ? entity2.GetAttributeValue<string>("name") : (entity2.GetAttributeValue<string>("firstname") + " " + entity2.GetAttributeValue<string>("lastname")));
						XmlDocument xmlDocument2 = iATSProcess.CreateCreditCardCustomerCode(createCreditCardCustomerCode);
						localContext.TracingService.Trace(xmlDocument2.InnerXml);
						XmlNodeList elementsByTagName = xmlDocument2.GetElementsByTagName("AUTHORIZATIONRESULT");
						foreach (XmlNode item in elementsByTagName)
						{
							string innerText = item.InnerText;
							localContext.TracingService.Trace("Auth Result- " + item.InnerText);
							if (innerText.Contains("OK"))
							{
								text3 = xmlDocument2.GetElementsByTagName("CUSTOMERCODE")[0].InnerText;
							}
						}
						localContext.TracingService.Trace("Mask the credit card.");
						MaskStripeCreditCard(localContext, entity, text3, null, null);
					}
					if (!string.IsNullOrEmpty(text3))
					{
						localContext.TracingService.Trace("Payment Method is Credit Card.");
						ProcessCreditCardWithCustomerCode processCreditCardWithCustomerCode = new ProcessCreditCardWithCustomerCode();
						processCreditCardWithCustomerCode.agentCode = agentCode;
						processCreditCardWithCustomerCode.password = password;
						processCreditCardWithCustomerCode.customerCode = text3;
						processCreditCardWithCustomerCode.invoiceNum = (giftTransaction.Contains("msnfp_invoiceidentifier") ? ((string)giftTransaction["msnfp_invoiceidentifier"]) : text);
						processCreditCardWithCustomerCode.total = $"{num:0.00}";
						localContext.TracingService.Trace("Donation Amount : " + $"{num:0.00}");
						processCreditCardWithCustomerCode.comment = "Debited by Dynamics 365 on " + DateTime.Now.ToString();
						xmlDocument = iATSProcess.ProcessCreditCardWithCustomerCode(processCreditCardWithCustomerCode);
						localContext.TracingService.Trace("Process complete to Payment with Credit Card.");
					}
				}
				else if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value == 844060001)
				{
					localContext.TracingService.Trace("iATS Bank Account payment.");
					if (!entity.Contains("msnfp_bankactnumber"))
					{
						localContext.TracingService.Trace("processiATSTransaction - Not a completed bank account. Missing bank account number");
						removePaymentMethod(entity, localContext, service);
						setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
						return;
					}
					if (entity.Contains("msnfp_authtoken"))
					{
						text3 = entity["msnfp_authtoken"] as string;
					}
					else
					{
						CreateACHEFTCustomerCode createACHEFTCustomerCode = new CreateACHEFTCustomerCode();
						createACHEFTCustomerCode.lastName = ((entity2.LogicalName == "contact") ? entity2.GetAttributeValue<string>("lastname") : string.Empty);
						createACHEFTCustomerCode.firstName = ((entity2.LogicalName == "account") ? entity2.GetAttributeValue<string>("name") : entity2.GetAttributeValue<string>("firstname"));
						createACHEFTCustomerCode.agentCode = agentCode;
						createACHEFTCustomerCode.password = password;
						createACHEFTCustomerCode.beginDate = DateTime.Today;
						createACHEFTCustomerCode.endDate = DateTime.Today.AddDays(1.0);
						createACHEFTCustomerCode.country = entity2.GetAttributeValue<string>("address1_country");
						createACHEFTCustomerCode.accountNum = entity.GetAttributeValue<string>("msnfp_bankactnumber");
						createACHEFTCustomerCode.recurring = false;
						createACHEFTCustomerCode.address = entity2.GetAttributeValue<string>("address1_line1");
						createACHEFTCustomerCode.city = entity2.GetAttributeValue<string>("address1_city");
						createACHEFTCustomerCode.zipCode = entity2.GetAttributeValue<string>("address1_postalcode");
						createACHEFTCustomerCode.state = entity2.GetAttributeValue<string>("address1_stateorprovince");
						createACHEFTCustomerCode.email = entity2.GetAttributeValue<string>("emailaddress1");
						XmlDocument xmlDocument3 = iATSProcess.CreateACHEFTCustomerCode(createACHEFTCustomerCode);
						XmlNodeList elementsByTagName2 = xmlDocument3.GetElementsByTagName("AUTHORIZATIONRESULT");
						foreach (XmlNode item2 in elementsByTagName2)
						{
							string innerText2 = item2.InnerText;
							localContext.TracingService.Trace("Auth Result- " + item2.InnerText);
							if (innerText2.Contains("OK"))
							{
								text3 = xmlDocument3.GetElementsByTagName("CUSTOMERCODE")[0].InnerText;
							}
						}
					}
					if (!string.IsNullOrEmpty(text3))
					{
						localContext.TracingService.Trace("Payment Method is Bank Account.");
						ProcessACHEFTWithCustomerCode processACHEFTWithCustomerCode = new ProcessACHEFTWithCustomerCode();
						processACHEFTWithCustomerCode.agentCode = agentCode;
						processACHEFTWithCustomerCode.password = password;
						processACHEFTWithCustomerCode.customerCode = text3;
						processACHEFTWithCustomerCode.invoiceNum = (giftTransaction.Contains("msnfp_invoiceidentifier") ? ((string)giftTransaction["msnfp_invoiceidentifier"]) : text);
						processACHEFTWithCustomerCode.total = $"{num:0.00}";
						localContext.TracingService.Trace("Donation Amount : " + $"{num:0.00}");
						processACHEFTWithCustomerCode.comment = "Debited by Dynamics 365 on " + DateTime.Now.ToString();
						xmlDocument = iATSProcess.ProcessACHEFTWithCustomerCode(processACHEFTWithCustomerCode);
						localContext.TracingService.Trace("Process complete to Payment with bank account.");
					}
				}
				if (xmlDocument != null)
				{
					XmlNodeList elementsByTagName3 = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT");
					Entity entity5 = new Entity("msnfp_response");
					entity5["msnfp_transactionid"] = new EntityReference("msnfp_transaction", (Guid)giftTransaction["msnfp_transactionid"]);
					entity5["msnfp_identifier"] = "Response for " + (string)giftTransaction["msnfp_name"];
					foreach (XmlNode item3 in elementsByTagName3)
					{
						entity5["msnfp_response"] = item3.InnerText;
						string innerText3 = item3.InnerText;
						if (innerText3.Contains("OK"))
						{
							localContext.TracingService.Trace("Got successful response from iATS payment gateway.");
							if (giftTransaction.Contains("msnfp_parenttransactionid"))
							{
								entity5["msnfp_transactionid"] = new EntityReference("msnfp_transaction", ((EntityReference)giftTransaction["msnfp_parenttransactionid"]).Id);
							}
							text2 = text2 + "---------Start iATS Response---------" + Environment.NewLine;
							text2 = text2 + "TransStatus = " + item3.InnerText + Environment.NewLine;
							text2 = text2 + "TransAmount = " + num + Environment.NewLine;
							text2 = text2 + "Auth Token = " + text3 + Environment.NewLine;
							text2 += "---------End iATS Response---------";
							localContext.TracingService.Trace("processiATSTransaction - Got successful response from iATS payment gateway.");
							entity5["msnfp_response"] = text2;
							giftTransaction["msnfp_transactionresult"] = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
							giftTransaction["msnfp_transactionidentifier"] = xmlDocument.GetElementsByTagName("TRANSACTIONID")[0].InnerText;
							giftTransaction["statuscode"] = new OptionSetValue(844060000);
							if (entity2 != null)
							{
								entity2["msnfp_lasttransactionid"] = new EntityReference("msnfp_transaction", (Guid)giftTransaction["msnfp_transactionid"]);
								entity2["msnfp_lasttransactiondate"] = (giftTransaction.Contains("createdon") ? ((DateTime)giftTransaction["createdon"]) : DateTime.MinValue);
								service.Update(entity2);
							}
							localContext.TracingService.Trace("processStripeTransaction - Updated Transaction Record.");
						}
						else
						{
							localContext.TracingService.Trace("Got failure response from iATS payment gateway.");
							giftTransaction["statuscode"] = new OptionSetValue(844060003);
							localContext.TracingService.Trace("Status code updated to failed");
							giftTransaction["msnfp_lastfailedretry"] = DateTime.Now.ToLocalTime().Date;
							giftTransaction["msnfp_currentretry"] = 0;
							giftTransaction["msnfp_nextfailedretry"] = DateTime.Now.AddDays(num2).ToLocalTime().Date;
							giftTransaction["msnfp_transactionresult"] = "FAILED";
							giftTransaction["msnfp_transactionidentifier"] = xmlDocument.GetElementsByTagName("TRANSACTIONID")[0].InnerText;
							giftTransaction["msnfp_transactionresult"] = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
							localContext.TracingService.Trace("Gateway Response Message." + xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText);
						}
					}
					giftTransaction["msnfp_invoiceidentifier"] = text;
					Guid id = service.Create(entity5);
					bool flag2 = true;
					giftTransaction["msnfp_responseid"] = new EntityReference("msnfp_response", id);
				}
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("processiATSTransaction - error : " + ex.Message);
				giftTransaction["statuscode"] = new OptionSetValue(844060003);
				localContext.TracingService.Trace("processiATSTransaction - Status code updated to failed");
				giftTransaction["msnfp_lastfailedretry"] = DateTime.Now.ToLocalTime().Date;
				giftTransaction["msnfp_currentretry"] = 0;
				giftTransaction["msnfp_nextfailedretry"] = DateTime.Now.AddDays(num2).ToLocalTime().Date;
				giftTransaction["msnfp_transactionresult"] = "FAILED";
				if (giftTransaction.Contains("msnfp_parenttransactionid") && giftTransaction["msnfp_parenttransactionid"] != null)
				{
					localContext.TracingService.Trace("processiATSTransaction - payment fails remove credit card from parent as well");
					Entity entity6 = service.Retrieve("msnfp_transaction", ((EntityReference)giftTransaction["msnfp_transactionid"]).Id, new ColumnSet("msnfp_transaction_paymentmethodid"));
					if (entity6 != null && entity6.Contains("msnfp_transaction_paymentmethodid") && entity6["msnfp_transaction_paymentmethodid"] != null)
					{
						entity6["msnfp_transaction_paymentmethodid"] = null;
						localContext.TracingService.Trace("msnfp_transaction_paymentmethodid 2");
						service.Update(entity6);
						localContext.TracingService.Trace("processiATSTransaction - parent gift updated. removed Credit card");
					}
				}
			}
			if (singleTransactionYN)
			{
				giftTransaction["msnfp_transaction_paymentmethodid"] = null;
				if (flag)
				{
					removePaymentMethod(entity, localContext, service);
				}
			}
			localContext.TracingService.Trace("iats");
			service.Update(giftTransaction);
			localContext.TracingService.Trace("processiATSTransaction - Entity Updated.");
		}

		private string processMonerisOneTimeTransaction(Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service)
		{
			string result = "";
			localContext.TracingService.Trace("Entering processMonerisOneTimeTransaction().");
			Entity entity = null;
			Entity entity2 = null;
			localContext.TracingService.Trace("Gathering transaction data from target id.");
			entity = getPaymentMethodForTransaction(giftTransaction, localContext, service);
			if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
			{
				localContext.TracingService.Trace("Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
				if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
				{
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				}
				return result;
			}
			if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				removePaymentMethod(entity, localContext, service);
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				return result;
			}
			entity2 = getPaymentProcessorForPaymentMethod(entity, giftTransaction, localContext, service);
			localContext.TracingService.Trace("Put gathered payment information into purchase object.");
			string text = Guid.NewGuid().ToString();
			string storeId = (string)entity2["msnfp_storeid"];
			string apiToken = (string)entity2["msnfp_apikey"];
			string amount = ((Money)giftTransaction["msnfp_amount"]).Value.ToString();
			string pan = (string)entity["msnfp_cclast4"];
			string text2 = (string)entity["msnfp_ccexpmmyy"];
			string cryptType = "7";
			string procCountryCode = "CA";
			bool statusCheck = false;
			string str = text2.Substring(0, 2);
			string str2 = text2.Substring(2, 2);
			localContext.TracingService.Trace("Old Expiry format (MMYY):" + text2);
			text2 = str2 + str;
			localContext.TracingService.Trace("Moneris Expiry format (YYMM):" + text2);
			localContext.TracingService.Trace("Creating Moneris purchase object.");
			Purchase purchase = new Purchase();
			purchase.SetOrderId(text);
			purchase.SetAmount(amount);
			purchase.SetPan(pan);
			purchase.SetExpDate(text2);
			purchase.SetCryptType(cryptType);
			purchase.SetDynamicDescriptor("2134565");
			localContext.TracingService.Trace("Check for AVS Validation.");
			AvsInfo avsCheck = new AvsInfo();
			if (entity.Contains("msnfp_ccbrandcode"))
			{
				if (((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060004)
				{
					if (entity2.Contains("msnfp_avsvalidation"))
					{
						if ((bool)entity2["msnfp_avsvalidation"])
						{
							localContext.TracingService.Trace("AVS Validation = True");
							if (!giftTransaction.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("No Donor. Exiting plugin.");
								setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
								throw new ArgumentNullException("msnfp_customerid");
							}
							try
							{
								localContext.TracingService.Trace("Entering address information for AVS validation.");
								avsCheck = AssignAVSValidationFieldsFromPaymentMethod(giftTransaction, entity, avsCheck, localContext, service);
								purchase.SetAvsInfo(avsCheck);
							}
							catch
							{
								localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
								setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
								throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString());
							}
						}
						else
						{
							localContext.TracingService.Trace("AVS Validation = False");
						}
					}
				}
				else
				{
					localContext.TracingService.Trace("Could not do AVS Validation as the card type is not supported. AVS is only supported by Visa(844060000 credit,844060002 debit), MasterCard(844060001 credit,844060003 debit), Discover(844060008) and American Express(844060004).");
					localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
			}
			localContext.TracingService.Trace("Creating HttpsPostRequest object.");
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			try
			{
				httpsPostRequest.SetProcCountryCode(procCountryCode);
				if (entity2.Contains("msnfp_testmode"))
				{
					if ((bool)entity2["msnfp_testmode"])
					{
						localContext.TracingService.Trace("Test Mode is Enabled.");
						httpsPostRequest.SetTestMode(state: true);
					}
					else
					{
						localContext.TracingService.Trace("Test Mode is Disabled.");
						httpsPostRequest.SetTestMode(state: false);
					}
				}
				else
				{
					localContext.TracingService.Trace("Test Mode not set. Defaulting to test mode enabled.");
					httpsPostRequest.SetTestMode(state: true);
				}
				httpsPostRequest.SetStoreId(storeId);
				httpsPostRequest.SetApiToken(apiToken);
				httpsPostRequest.SetTransaction(purchase);
				httpsPostRequest.SetStatusCheck(statusCheck);
				localContext.TracingService.Trace("Sending Moneris HttpsPostRequest.");
				httpsPostRequest.Send();
				localContext.TracingService.Trace("HttpsPostRequest sent successfully!");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("HttpsPostRequest Error: " + ex.ToString());
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				removePaymentMethod(entity, localContext, service);
				return result;
			}
			try
			{
				Receipt receipt = httpsPostRequest.GetReceipt();
				string str3 = "";
				localContext.TracingService.Trace("---------Moneris Response---------");
				localContext.TracingService.Trace("CardType = " + receipt.GetCardType());
				localContext.TracingService.Trace("TransAmount = " + receipt.GetTransAmount());
				localContext.TracingService.Trace("TxnNumber = " + receipt.GetTxnNumber());
				localContext.TracingService.Trace("ReceiptId = " + receipt.GetReceiptId());
				localContext.TracingService.Trace("TransType = " + receipt.GetTransType());
				localContext.TracingService.Trace("ReferenceNum = " + receipt.GetReferenceNum());
				localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
				localContext.TracingService.Trace("ISO = " + receipt.GetISO());
				localContext.TracingService.Trace("BankTotals = " + receipt.GetBankTotals());
				localContext.TracingService.Trace("Message = " + receipt.GetMessage());
				localContext.TracingService.Trace("AuthCode = " + receipt.GetAuthCode());
				localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
				localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
				localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
				localContext.TracingService.Trace("Ticket = " + receipt.GetTicket());
				localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
				localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
				localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
				localContext.TracingService.Trace("ITD Response = " + receipt.GetITDResponse());
				localContext.TracingService.Trace("IsVisaDebit = " + receipt.GetIsVisaDebit());
				localContext.TracingService.Trace("---------End Moneris Response---------");
				str3 = str3 + "---------Moneris Response---------" + Environment.NewLine;
				str3 = str3 + "CardType = " + receipt.GetCardType() + Environment.NewLine;
				str3 = str3 + "TransAmount = " + receipt.GetTransAmount() + Environment.NewLine;
				str3 = str3 + "TxnNumber = " + receipt.GetTxnNumber() + Environment.NewLine;
				str3 = str3 + "ReceiptId = " + receipt.GetReceiptId() + Environment.NewLine;
				str3 = str3 + "TransType = " + receipt.GetTransType() + Environment.NewLine;
				str3 = str3 + "ReferenceNum = " + receipt.GetReferenceNum() + Environment.NewLine;
				str3 = str3 + "ResponseCode = " + receipt.GetResponseCode() + Environment.NewLine;
				str3 = str3 + "ISO = " + receipt.GetISO() + Environment.NewLine;
				str3 = str3 + "BankTotals = " + receipt.GetBankTotals() + Environment.NewLine;
				str3 = str3 + "Message = " + receipt.GetMessage() + Environment.NewLine;
				str3 = str3 + "AuthCode = " + receipt.GetAuthCode() + Environment.NewLine;
				str3 = str3 + "Complete = " + receipt.GetComplete() + Environment.NewLine;
				str3 = str3 + "TransDate = " + receipt.GetTransDate() + Environment.NewLine;
				str3 = str3 + "TransTime = " + receipt.GetTransTime() + Environment.NewLine;
				str3 = str3 + "Ticket = " + receipt.GetTicket() + Environment.NewLine;
				str3 = str3 + "TimedOut = " + receipt.GetTimedOut() + Environment.NewLine;
				str3 = str3 + "Avs Response = " + receipt.GetAvsResultCode() + Environment.NewLine;
				str3 = str3 + "Cvd Response = " + receipt.GetCvdResultCode() + Environment.NewLine;
				str3 = str3 + "ITD Response = " + receipt.GetITDResponse() + Environment.NewLine;
				str3 = str3 + "IsVisaDebit = " + receipt.GetIsVisaDebit() + Environment.NewLine;
				str3 += "---------End Moneris Response---------";
				if (receipt.GetResponseCode() != null)
				{
					if (!int.TryParse(receipt.GetResponseCode(), out var result2))
					{
						localContext.TracingService.Trace("Error: Response code is not a number = " + receipt.GetResponseCode());
						setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
						removePaymentMethod(entity, localContext, service);
						return result;
					}
					if (result2 < 50)
					{
						setStatusCodeOnTransaction(giftTransaction, 844060000, localContext, service);
						removePaymentMethod(entity, localContext, service);
					}
					else
					{
						setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
						removePaymentMethod(entity, localContext, service);
					}
				}
				localContext.TracingService.Trace("Creating response record with response: " + receipt.GetMessage());
				Entity entity3 = new Entity("msnfp_response");
				entity3["msnfp_identifier"] = "Response for " + (string)giftTransaction["msnfp_name"];
				entity3["msnfp_response"] = str3;
				entity3["msnfp_transactionid"] = new EntityReference("msnfp_transaction", (Guid)giftTransaction["msnfp_transactionid"]);
				Guid guid = service.Create(entity3);
				bool flag = true;
				ITracingService tracingService = localContext.TracingService;
				Guid guid2 = guid;
				tracingService.Trace("Response created (" + guid2.ToString() + "). Linking response record to transaction.");
				giftTransaction["msnfp_responseid"] = new EntityReference("msnfp_response", guid);
				if (receipt.GetResponseCode() != null && int.TryParse(receipt.GetResponseCode(), out var result3))
				{
					if (result3 < 50)
					{
						localContext.TracingService.Trace("Setting msnfp_transactionidentifier = " + receipt.GetReferenceNum());
						localContext.TracingService.Trace("Setting msnfp_transactionnumber = " + receipt.GetTxnNumber());
						localContext.TracingService.Trace("Setting order_id = " + text);
						giftTransaction["msnfp_transactionidentifier"] = receipt.GetReferenceNum();
						giftTransaction["msnfp_transactionnumber"] = receipt.GetTxnNumber();
						giftTransaction["msnfp_invoiceidentifier"] = text;
						giftTransaction["msnfp_transactionresult"] = "Approved - " + result3;
						if (receipt.GetCardType() != null)
						{
							localContext.TracingService.Trace("Card Type Response Code = " + receipt.GetCardType());
							switch (receipt.GetCardType())
							{
							case "M":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060001);
								break;
							case "V":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060000);
								break;
							case "AX":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060004);
								break;
							case "NO":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060008);
								break;
							case "D":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060007);
								break;
							case "DC":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060005);
								break;
							case "C1":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
								break;
							case "JCB":
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
								break;
							default:
								giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060010);
								break;
							}
						}
					}
					else
					{
						giftTransaction["msnfp_transactionresult"] = "Declined - " + result3;
					}
				}
				try
				{
					entity = service.Retrieve("msnfp_paymentmethod", ((EntityReference)giftTransaction["msnfp_transaction_paymentmethodid"]).Id, new ColumnSet("msnfp_cclast4", "msnfp_ccexpmmyy", "msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode"));
					if (entity == null)
					{
						localContext.TracingService.Trace("Clear Payment Method lookup on this transaction.");
						giftTransaction["msnfp_transaction_paymentmethodid"] = null;
					}
				}
				catch (Exception)
				{
					localContext.TracingService.Trace("Could not find Payment Method. Clear Payment Method lookup on this transaction record.");
					giftTransaction["msnfp_transaction_paymentmethodid"] = null;
				}
				localContext.TracingService.Trace("paymentmethod 3");
				service.Update(giftTransaction);
				localContext.TracingService.Trace("Setting return response code: " + receipt.GetResponseCode());
				result = receipt.GetResponseCode();
			}
			catch (Exception ex3)
			{
				localContext.TracingService.Trace("Receipt Error: " + ex3.ToString());
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				removePaymentMethod(entity, localContext, service);
			}
			return result;
		}

		private void processMonerisVaultTransaction(Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering processMonerisVaultTransaction().");
			Entity entity = null;
			Entity entity2 = null;
			localContext.TracingService.Trace("Gathering transaction data from target id.");
			entity = getPaymentMethodForTransaction(giftTransaction, localContext, service);
			if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
			{
				localContext.TracingService.Trace("Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
				if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
				{
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				}
				return;
			}
			if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				removePaymentMethod(entity, localContext, service);
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				return;
			}
			entity2 = getPaymentProcessorForPaymentMethod(entity, giftTransaction, localContext, service);
			if (!entity.Contains("msnfp_authtoken") || entity["msnfp_authtoken"] == null)
			{
				localContext.TracingService.Trace("No data id found for customer. Attempting to process the payment and if successful create a new Moneris Vault profile with this transaction.");
				string text = processMonerisOneTimeTransaction(giftTransaction, localContext, service);
				if (int.TryParse(text, out var result))
				{
					if (result < 50)
					{
						localContext.TracingService.Trace("Response was Approved. Now add to vault.");
						addMonerisVaultProfile(giftTransaction, localContext, service);
					}
					else
					{
						localContext.TracingService.Trace("Response code: " + text + ". Please check payment details. Exiting plugin.");
						setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
					}
				}
			}
			else
			{
				if (!entity.Contains("msnfp_authtoken"))
				{
					return;
				}
				localContext.TracingService.Trace("Data id found for customer.");
				string custId = ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString();
				localContext.TracingService.Trace("Put gathered payment information into purchase object.");
				string text2 = Guid.NewGuid().ToString();
				string storeId = (string)entity2["msnfp_storeid"];
				string apiToken = (string)entity2["msnfp_apikey"];
				string amount = ((Money)giftTransaction["msnfp_amount"]).Value.ToString();
				string procCountryCode = "CA";
				bool statusCheck = false;
				string dataKey = (string)entity["msnfp_authtoken"];
				string cryptType = "7";
				string dynamicDescriptor = "Created in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
				localContext.TracingService.Trace("Creating ResPurchaseCC object.");
				ResPurchaseCC resPurchaseCC = new ResPurchaseCC();
				resPurchaseCC.SetDataKey(dataKey);
				resPurchaseCC.SetOrderId(text2);
				resPurchaseCC.SetCustId(custId);
				resPurchaseCC.SetAmount(amount);
				resPurchaseCC.SetCryptType(cryptType);
				resPurchaseCC.SetDynamicDescriptor(dynamicDescriptor);
				localContext.TracingService.Trace("Check for AVS Validation.");
				AvsInfo avsCheck = new AvsInfo();
				if (entity.Contains("msnfp_ccbrandcode"))
				{
					if (((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060004)
					{
						if (entity2.Contains("msnfp_avsvalidation"))
						{
							if ((bool)entity2["msnfp_avsvalidation"])
							{
								localContext.TracingService.Trace("AVS Validation = True");
								if (!giftTransaction.Contains("msnfp_customerid"))
								{
									localContext.TracingService.Trace("No Donor. Exiting plugin.");
									setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
									throw new ArgumentNullException("msnfp_customerid");
								}
								try
								{
									localContext.TracingService.Trace("Entering address information for AVS validation.");
									avsCheck = AssignAVSValidationFieldsFromPaymentMethod(giftTransaction, entity, avsCheck, localContext, service);
									resPurchaseCC.SetAvsInfo(avsCheck);
								}
								catch
								{
									localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
									setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
									throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString());
								}
							}
							else
							{
								localContext.TracingService.Trace("AVS Validation = False");
							}
						}
					}
					else
					{
						localContext.TracingService.Trace("Could not do AVS Validation as the card type is not supported. AVS is only supported by Visa(844060000 credit,844060002 debit), MasterCard(844060001 credit,844060003 debit), Discover(844060008) and American Express(844060004).");
						localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value);
					}
				}
				else
				{
					localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
				}
				HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
				httpsPostRequest.SetProcCountryCode(procCountryCode);
				if (entity2.Contains("msnfp_testmode"))
				{
					if ((bool)entity2["msnfp_testmode"])
					{
						localContext.TracingService.Trace("Test Mode is Enabled.");
						httpsPostRequest.SetTestMode(state: true);
					}
					else
					{
						localContext.TracingService.Trace("Test Mode is Disabled.");
						httpsPostRequest.SetTestMode(state: false);
					}
				}
				else
				{
					localContext.TracingService.Trace("Test Mode not set. Defaulting to test mode enabled.");
					httpsPostRequest.SetTestMode(state: true);
				}
				httpsPostRequest.SetStoreId(storeId);
				httpsPostRequest.SetApiToken(apiToken);
				httpsPostRequest.SetTransaction(resPurchaseCC);
				httpsPostRequest.SetStatusCheck(statusCheck);
				localContext.TracingService.Trace("Sending request.");
				httpsPostRequest.Send();
				localContext.TracingService.Trace("Request sent successfully.");
				try
				{
					Receipt receipt = httpsPostRequest.GetReceipt();
					string str = "";
					localContext.TracingService.Trace("---------Moneris Response---------");
					localContext.TracingService.Trace("DataKey = " + receipt.GetDataKey());
					localContext.TracingService.Trace("ReceiptId = " + receipt.GetReceiptId());
					localContext.TracingService.Trace("ReferenceNum = " + receipt.GetReferenceNum());
					localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
					localContext.TracingService.Trace("AuthCode = " + receipt.GetAuthCode());
					localContext.TracingService.Trace("Message = " + receipt.GetMessage());
					localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
					localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
					localContext.TracingService.Trace("TransType = " + receipt.GetTransType());
					localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
					localContext.TracingService.Trace("TransAmount = " + receipt.GetTransAmount());
					localContext.TracingService.Trace("CardType = " + receipt.GetCardType());
					localContext.TracingService.Trace("TxnNumber = " + receipt.GetTxnNumber());
					localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
					localContext.TracingService.Trace("ResSuccess = " + receipt.GetResSuccess());
					localContext.TracingService.Trace("PaymentType = " + receipt.GetPaymentType());
					localContext.TracingService.Trace("IsVisaDebit = " + receipt.GetIsVisaDebit());
					localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
					localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
					localContext.TracingService.Trace("---------Customer---------");
					localContext.TracingService.Trace("Cust ID = " + receipt.GetResDataCustId());
					localContext.TracingService.Trace("Phone = " + receipt.GetResDataPhone());
					localContext.TracingService.Trace("Email = " + receipt.GetResDataEmail());
					localContext.TracingService.Trace("Note = " + receipt.GetResDataNote());
					localContext.TracingService.Trace("Exp Date (YYMM) = " + receipt.GetResDataExpdate());
					localContext.TracingService.Trace("Crypt Type = " + receipt.GetResDataCryptType());
					localContext.TracingService.Trace("Avs Street Number = " + receipt.GetResDataAvsStreetNumber());
					localContext.TracingService.Trace("Avs Street Name = " + receipt.GetResDataAvsStreetName());
					localContext.TracingService.Trace("Avs Zipcode = " + receipt.GetResDataAvsZipcode());
					localContext.TracingService.Trace("---------End Customer---------");
					localContext.TracingService.Trace("---------End Moneris Response---------");
					str = str + "---------Moneris Response---------" + Environment.NewLine;
					str = str + "DataKey = " + receipt.GetDataKey() + Environment.NewLine;
					str = str + "ReceiptId = " + receipt.GetReceiptId() + Environment.NewLine;
					str = str + "ReferenceNum = " + receipt.GetReferenceNum() + Environment.NewLine;
					str = str + "ResponseCode = " + receipt.GetResponseCode() + Environment.NewLine;
					str = str + "AuthCode = " + receipt.GetAuthCode() + Environment.NewLine;
					str = str + "Message = " + receipt.GetMessage() + Environment.NewLine;
					str = str + "TransDate = " + receipt.GetTransDate() + Environment.NewLine;
					str = str + "TransTime = " + receipt.GetTransTime() + Environment.NewLine;
					str = str + "TransType = " + receipt.GetTransType() + Environment.NewLine;
					str = str + "Complete = " + receipt.GetComplete() + Environment.NewLine;
					str = str + "TransAmount = " + receipt.GetTransAmount() + Environment.NewLine;
					str = str + "CardType = " + receipt.GetCardType() + Environment.NewLine;
					str = str + "TxnNumber = " + receipt.GetTxnNumber() + Environment.NewLine;
					str = str + "TimedOut = " + receipt.GetTimedOut() + Environment.NewLine;
					str = str + "ResSuccess = " + receipt.GetResSuccess() + Environment.NewLine;
					str = str + "PaymentType = " + receipt.GetPaymentType() + Environment.NewLine;
					str = str + "IsVisaDebit = " + receipt.GetIsVisaDebit() + Environment.NewLine;
					str = str + "Avs Response = " + receipt.GetAvsResultCode() + Environment.NewLine;
					str = str + "Cvd Response = " + receipt.GetCvdResultCode() + Environment.NewLine;
					str = str + "---------Customer---------" + Environment.NewLine;
					str = str + "Cust ID = " + receipt.GetResDataCustId() + Environment.NewLine;
					str = str + "Phone = " + receipt.GetResDataPhone() + Environment.NewLine;
					str = str + "Email = " + receipt.GetResDataEmail() + Environment.NewLine;
					str = str + "Note = " + receipt.GetResDataNote() + Environment.NewLine;
					str = str + "Masked Pan = " + receipt.GetResDataMaskedPan() + Environment.NewLine;
					str = str + "Exp Date (YYMM) = " + receipt.GetResDataExpdate() + Environment.NewLine;
					str = str + "Crypt Type = " + receipt.GetResDataCryptType() + Environment.NewLine;
					str = str + "Avs Street Number = " + receipt.GetResDataAvsStreetNumber() + Environment.NewLine;
					str = str + "Avs Street Name = " + receipt.GetResDataAvsStreetName() + Environment.NewLine;
					str = str + "Avs Zipcode = " + receipt.GetResDataAvsZipcode() + Environment.NewLine;
					str = str + "---------End Customer---------" + Environment.NewLine;
					str = str + "---------End Moneris Response---------" + Environment.NewLine;
					localContext.TracingService.Trace("Creating response record with response: " + receipt.GetMessage());
					if (receipt.GetResponseCode() != null)
					{
						if (int.TryParse(receipt.GetResponseCode(), out var result2))
						{
							if (result2 < 50)
							{
								setStatusCodeOnTransaction(giftTransaction, 844060000, localContext, service);
							}
							else
							{
								setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
							}
						}
						else
						{
							localContext.TracingService.Trace("Error: Response code is not a number = " + receipt.GetResponseCode());
							setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
							removePaymentMethod(entity, localContext, service);
						}
					}
					Entity entity3 = new Entity("msnfp_response");
					entity3["msnfp_identifier"] = "Response for " + (string)giftTransaction["msnfp_name"];
					entity3["msnfp_response"] = str;
					entity3["msnfp_transactionid"] = new EntityReference("msnfp_transaction", (Guid)giftTransaction["msnfp_transactionid"]);
					Guid guid = service.Create(entity3);
					bool flag = true;
					ITracingService tracingService = localContext.TracingService;
					Guid guid2 = guid;
					tracingService.Trace("Response created (" + guid2.ToString() + "). Linking response record to transaction.");
					giftTransaction["msnfp_responseid"] = new EntityReference("msnfp_response", guid);
					if (receipt.GetResponseCode() != null && int.TryParse(receipt.GetResponseCode(), out var result3))
					{
						if (result3 < 50)
						{
							localContext.TracingService.Trace("Setting msnfp_transactionidentifier = " + receipt.GetReferenceNum());
							localContext.TracingService.Trace("Setting msnfp_transactionnumber = " + receipt.GetTxnNumber());
							localContext.TracingService.Trace("Setting order_id = " + text2);
							giftTransaction["msnfp_transactionidentifier"] = receipt.GetReferenceNum();
							giftTransaction["msnfp_transactionnumber"] = receipt.GetTxnNumber();
							giftTransaction["msnfp_invoiceidentifier"] = text2;
							giftTransaction["msnfp_transactionresult"] = "Approved - " + result3;
							if (receipt.GetCardType() != null)
							{
								localContext.TracingService.Trace("Card Type Response Code = " + receipt.GetCardType());
								switch (receipt.GetCardType())
								{
								case "M":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060001);
									break;
								case "V":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060000);
									break;
								case "AX":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060004);
									break;
								case "NO":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060008);
									break;
								case "D":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060007);
									break;
								case "DC":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060005);
									break;
								case "C1":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
									break;
								case "JCB":
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
									break;
								default:
									giftTransaction["msnfp_ccbrandcode"] = new OptionSetValue(844060010);
									break;
								}
							}
						}
						else if (result3 > 50)
						{
							giftTransaction["msnfp_transactionresult"] = "FAILED";
						}
					}
					try
					{
						if (entity == null || (entity != null && !entity.GetAttributeValue<bool>("msnfp_isreusable")))
						{
							localContext.TracingService.Trace("Clear Payment Method lookup on this transaction.");
							giftTransaction["msnfp_transaction_paymentmethodid"] = null;
						}
					}
					catch (Exception ex)
					{
						localContext.TracingService.Trace("Could not find Payment Method. Clear Payment Method lookup on this transaction record.");
						localContext.TracingService.Trace(ex.ToString());
						giftTransaction["msnfp_transaction_paymentmethodid"] = null;
					}
					localContext.TracingService.Trace("payment method 4");
					service.Update(giftTransaction);
				}
				catch (Exception ex2)
				{
					localContext.TracingService.Trace(ex2.ToString());
				}
			}
		}

		private void addMonerisVaultProfile(Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering addMonerisVaultProfile().");
			Entity entity = null;
			Entity entity2 = null;
			localContext.TracingService.Trace("Gathering transaction data from target id.");
			entity = getPaymentMethodForTransaction(giftTransaction, localContext, service);
			if (entity.Contains("msnfp_type") && ((OptionSetValue)entity["msnfp_type"]).Value != 844060000)
			{
				localContext.TracingService.Trace("Not a credit card (844060000). Payment method msnfp_type = " + ((OptionSetValue)entity["msnfp_type"]).Value);
				if (((OptionSetValue)entity["msnfp_type"]).Value != 844060001)
				{
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				}
				return;
			}
			if (!entity.Contains("msnfp_cclast4") || !entity.Contains("msnfp_ccexpmmyy"))
			{
				localContext.TracingService.Trace("Not a completed credit card. Missing msnfp_cclast4 or msnfp_ccexpmmyy.");
				removePaymentMethod(entity, localContext, service);
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				return;
			}
			entity2 = getPaymentProcessorForPaymentMethod(entity, giftTransaction, localContext, service);
			localContext.TracingService.Trace("Put gathered payment information into vault profile object.");
			string storeId = (string)entity2["msnfp_storeid"];
			string apiToken = (string)entity2["msnfp_apikey"];
			string pan = (string)entity["msnfp_cclast4"];
			string text = (string)entity["msnfp_ccexpmmyy"];
			string cryptType = "7";
			string procCountryCode = "CA";
			bool statusCheck = false;
			string str = text.Substring(0, 2);
			string str2 = text.Substring(2, 2);
			localContext.TracingService.Trace("Old Expiry format (MMYY):" + text);
			text = str2 + str;
			localContext.TracingService.Trace("Moneris Expiry format (YYMM):" + text);
			string phone = "";
			string email = "";
			string note = "Created in Dynamics 365 on " + DateTime.UtcNow.ToString() + "(UTC)";
			string custId = ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString();
			if (entity.Contains("msnfp_telephone1"))
			{
				phone = (string)entity["msnfp_telephone1"];
			}
			if (entity.Contains("msnfp_emailaddress1"))
			{
				email = (string)entity["msnfp_emailaddress1"];
			}
			ResAddCC resAddCC = new ResAddCC();
			resAddCC.SetPan(pan);
			resAddCC.SetExpDate(text);
			resAddCC.SetCryptType(cryptType);
			resAddCC.SetCustId(custId);
			resAddCC.SetPhone(phone);
			resAddCC.SetEmail(email);
			resAddCC.SetNote(note);
			resAddCC.SetGetCardType("true");
			AvsInfo avsCheck = new AvsInfo();
			localContext.TracingService.Trace("Check for AVS Validation.");
			if (entity.Contains("msnfp_ccbrandcode"))
			{
				if (((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060000 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060002 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060001 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060003 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060008 || ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value == 844060004)
				{
					if (entity2.Contains("msnfp_avsvalidation"))
					{
						if ((bool)entity2["msnfp_avsvalidation"])
						{
							localContext.TracingService.Trace("AVS Validation = True");
							if (!giftTransaction.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("No Donor. Exiting plugin.");
								setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
								throw new ArgumentNullException("msnfp_customerid");
							}
							try
							{
								localContext.TracingService.Trace("Entering address information for AVS validation.");
								avsCheck = AssignAVSValidationFieldsFromPaymentMethod(giftTransaction, entity, avsCheck, localContext, service);
								resAddCC.SetAvsInfo(avsCheck);
							}
							catch
							{
								localContext.TracingService.Trace("Error with AVSValidation. Exiting plugin.");
								setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
								throw new Exception("Unable to set AVSValidation fields in setStatusCodeOnTransaction(). Please ensure the address fields are valid for the customer (" + ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName + ") with id: " + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString());
							}
						}
						else
						{
							localContext.TracingService.Trace("AVS Validation = False");
						}
					}
				}
				else
				{
					localContext.TracingService.Trace("Could not do AVS Validation as the card type is not supported. AVS is only supported by Visa(844060000 credit,844060002 debit), MasterCard(844060001 credit,844060003 debit), Discover(844060008) and American Express(844060004).");
					localContext.TracingService.Trace("Payment Card type: " + ((OptionSetValue)entity["msnfp_ccbrandcode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not do AVS Validation as the card type is not assigned.");
			}
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			httpsPostRequest.SetProcCountryCode(procCountryCode);
			if (entity2.Contains("msnfp_testmode"))
			{
				if ((bool)entity2["msnfp_testmode"])
				{
					localContext.TracingService.Trace("Test Mode is Enabled.");
					httpsPostRequest.SetTestMode(state: true);
				}
				else
				{
					localContext.TracingService.Trace("Test Mode is Disabled.");
					httpsPostRequest.SetTestMode(state: false);
				}
			}
			else
			{
				localContext.TracingService.Trace("Test Mode not set. Defaulting to test mode enabled.");
				httpsPostRequest.SetTestMode(state: true);
			}
			httpsPostRequest.SetStoreId(storeId);
			httpsPostRequest.SetApiToken(apiToken);
			httpsPostRequest.SetTransaction(resAddCC);
			httpsPostRequest.SetStatusCheck(statusCheck);
			localContext.TracingService.Trace("Attempting to create the new user profile in the Moneris Vault.");
			httpsPostRequest.Send();
			try
			{
				Receipt receipt = httpsPostRequest.GetReceipt();
				localContext.TracingService.Trace("---------Moneris Response---------");
				localContext.TracingService.Trace("DataKey = " + receipt.GetDataKey());
				localContext.TracingService.Trace("ResponseCode = " + receipt.GetResponseCode());
				localContext.TracingService.Trace("Message = " + receipt.GetMessage());
				localContext.TracingService.Trace("TransDate = " + receipt.GetTransDate());
				localContext.TracingService.Trace("TransTime = " + receipt.GetTransTime());
				localContext.TracingService.Trace("Complete = " + receipt.GetComplete());
				localContext.TracingService.Trace("TimedOut = " + receipt.GetTimedOut());
				localContext.TracingService.Trace("ResSuccess = " + receipt.GetResSuccess());
				localContext.TracingService.Trace("PaymentType = " + receipt.GetPaymentType());
				localContext.TracingService.Trace("Cust ID = " + receipt.GetResDataCustId());
				localContext.TracingService.Trace("Phone = " + receipt.GetResDataPhone());
				localContext.TracingService.Trace("Email = " + receipt.GetResDataEmail());
				localContext.TracingService.Trace("Note = " + receipt.GetResDataNote());
				localContext.TracingService.Trace("MaskedPan = " + receipt.GetResDataMaskedPan());
				localContext.TracingService.Trace("Exp Date = " + receipt.GetResDataExpdate());
				localContext.TracingService.Trace("Crypt Type = " + receipt.GetResDataCryptType());
				localContext.TracingService.Trace("Avs Response = " + receipt.GetAvsResultCode());
				localContext.TracingService.Trace("Cvd Response = " + receipt.GetCvdResultCode());
				localContext.TracingService.Trace("Avs Street Number = " + receipt.GetResDataAvsStreetNumber());
				localContext.TracingService.Trace("Avs Street Name = " + receipt.GetResDataAvsStreetName());
				localContext.TracingService.Trace("Avs Zipcode = " + receipt.GetResDataAvsZipcode());
				localContext.TracingService.Trace("---------End Moneris Response---------");
				try
				{
					entity["msnfp_authtoken"] = receipt.GetDataKey();
					if (receipt.GetDataKey().Length > 0)
					{
						entity["msnfp_cclast4"] = receipt.GetResDataMaskedPan();
						localContext.TracingService.Trace("Masked Card Number and CVV");
					}
					service.Update(entity);
					localContext.TracingService.Trace("Added token to payment method.");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("Error, could not assign data id to auth token. Data key: " + receipt.GetDataKey());
					localContext.TracingService.Trace("Error: " + ex.ToString());
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
					throw new ArgumentNullException("msnfp_authtoken");
				}
			}
			catch (Exception ex2)
			{
				localContext.TracingService.Trace("Error processing response from payment gateway. Exiting plugin.");
				localContext.TracingService.Trace("Error: " + ex2.ToString());
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				throw new Exception("Error processing response from payment gateway. Please check donor information to make sure it is correctly inputted.");
			}
		}

		private void AddOrUpdateThisRecordWithAzure(Entity giftTransaction, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Update the Transaction Record On Azure---------");
			string messageName = context.MessageName;
			string text = configurationRecord.GetAttributeValue<string>("msnfp_azure_webapiurl");
			localContext.TracingService.Trace("Got API URL: " + text);
			if (!(text != string.Empty))
			{
				return;
			}
			localContext.TracingService.Trace("Getting Latest Info for Record: " + giftTransaction["msnfp_transactionid"].ToString());
			MSNFP_Transaction mSNFP_Transaction = new MSNFP_Transaction();
			mSNFP_Transaction.TransactionId = (Guid)giftTransaction["msnfp_transactionid"];
			mSNFP_Transaction.Name = (giftTransaction.Contains("msnfp_name") ? ((string)giftTransaction["msnfp_name"]) : string.Empty);
			localContext.TracingService.Trace("Title: " + mSNFP_Transaction.Name);
			if (giftTransaction.Contains("msnfp_customerid") && giftTransaction["msnfp_customerid"] != null)
			{
				mSNFP_Transaction.CustomerId = ((EntityReference)giftTransaction["msnfp_customerid"]).Id;
				if (((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName.ToLower() == "contact")
				{
					mSNFP_Transaction.CustomerIdType = 2;
				}
				else if (((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName.ToLower() == "account")
				{
					mSNFP_Transaction.CustomerIdType = 1;
				}
				localContext.TracingService.Trace("Got msnfp_customerid.");
			}
			else
			{
				mSNFP_Transaction.CustomerId = null;
				mSNFP_Transaction.CustomerIdType = null;
				localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
			}
			if (giftTransaction.Contains("msnfp_firstname") && giftTransaction["msnfp_firstname"] != null)
			{
				mSNFP_Transaction.FirstName = (string)giftTransaction["msnfp_firstname"];
				localContext.TracingService.Trace("Got msnfp_firstname.");
			}
			else
			{
				mSNFP_Transaction.FirstName = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_firstname.");
			}
			if (giftTransaction.Contains("msnfp_lastname") && giftTransaction["msnfp_lastname"] != null)
			{
				mSNFP_Transaction.LastName = (string)giftTransaction["msnfp_lastname"];
				localContext.TracingService.Trace("Got msnfp_lastname.");
			}
			else
			{
				mSNFP_Transaction.LastName = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_lastname.");
			}
			if (giftTransaction.Contains("msnfp_billing_line1") && giftTransaction["msnfp_billing_line1"] != null)
			{
				mSNFP_Transaction.BillingLine1 = (string)giftTransaction["msnfp_billing_line1"];
				localContext.TracingService.Trace("Got msnfp_billing_line1.");
			}
			else
			{
				mSNFP_Transaction.BillingLine1 = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_line1.");
			}
			if (giftTransaction.Contains("msnfp_billing_line2") && giftTransaction["msnfp_billing_line2"] != null)
			{
				mSNFP_Transaction.BillingLine2 = (string)giftTransaction["msnfp_billing_line2"];
				localContext.TracingService.Trace("Got msnfp_billing_line2.");
			}
			else
			{
				mSNFP_Transaction.BillingLine2 = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_line2.");
			}
			if (giftTransaction.Contains("msnfp_billing_line3") && giftTransaction["msnfp_billing_line3"] != null)
			{
				mSNFP_Transaction.BillingLine3 = (string)giftTransaction["msnfp_billing_line3"];
				localContext.TracingService.Trace("Got msnfp_billing_line3.");
			}
			else
			{
				mSNFP_Transaction.BillingLine3 = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_line3.");
			}
			if (giftTransaction.Contains("msnfp_billing_city") && giftTransaction["msnfp_billing_city"] != null)
			{
				mSNFP_Transaction.BillingCity = (string)giftTransaction["msnfp_billing_city"];
				localContext.TracingService.Trace("Got msnfp_billing_city.");
			}
			else
			{
				mSNFP_Transaction.BillingCity = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_city.");
			}
			if (giftTransaction.Contains("msnfp_billing_stateorprovince") && giftTransaction["msnfp_billing_stateorprovince"] != null)
			{
				mSNFP_Transaction.BillingStateorProvince = (string)giftTransaction["msnfp_billing_stateorprovince"];
				localContext.TracingService.Trace("Got msnfp_billing_stateorprovince.");
			}
			else
			{
				mSNFP_Transaction.BillingStateorProvince = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_stateorprovince.");
			}
			if (giftTransaction.Contains("msnfp_billing_country") && giftTransaction["msnfp_billing_country"] != null)
			{
				mSNFP_Transaction.BillingCountry = (string)giftTransaction["msnfp_billing_country"];
				localContext.TracingService.Trace("Got msnfp_billing_country");
			}
			else
			{
				mSNFP_Transaction.BillingCountry = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_country.");
			}
			if (giftTransaction.Contains("msnfp_billing_postalcode") && giftTransaction["msnfp_billing_postalcode"] != null)
			{
				mSNFP_Transaction.BillingPostalCode = (string)giftTransaction["msnfp_billing_postalcode"];
				localContext.TracingService.Trace("Got msnfp_billing_postalcode");
			}
			else
			{
				mSNFP_Transaction.BillingPostalCode = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_billing_postalcode.");
			}
			if (giftTransaction.Contains("msnfp_chequenumber") && giftTransaction["msnfp_chequenumber"] != null)
			{
				mSNFP_Transaction.ChequeNumber = (string)giftTransaction["msnfp_chequenumber"];
				localContext.TracingService.Trace("Got msnfp_chequenumber");
			}
			else
			{
				mSNFP_Transaction.ChequeNumber = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_chequenumber.");
			}
			if (giftTransaction.Contains("msnfp_chequewiredate") && giftTransaction["msnfp_chequewiredate"] != null)
			{
				mSNFP_Transaction.ChequeWireDate = (DateTime)giftTransaction["msnfp_chequewiredate"];
				localContext.TracingService.Trace("Got msnfp_chequewiredate");
			}
			else
			{
				mSNFP_Transaction.ChequeWireDate = null;
				localContext.TracingService.Trace("Did NOT find msnfp_chequewiredate.");
			}
			if (giftTransaction.Contains("msnfp_transactionidentifier") && giftTransaction["msnfp_transactionidentifier"] != null)
			{
				mSNFP_Transaction.TransactionIdentifier = (string)giftTransaction["msnfp_transactionidentifier"];
				localContext.TracingService.Trace("Got msnfp_transactionidentifier.");
			}
			else
			{
				mSNFP_Transaction.TransactionIdentifier = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_transactionidentifier.");
			}
			if (giftTransaction.Contains("msnfp_transactionnumber") && giftTransaction["msnfp_transactionnumber"] != null)
			{
				mSNFP_Transaction.TransactionNumber = (string)giftTransaction["msnfp_transactionnumber"];
				localContext.TracingService.Trace("Got msnfp_transactionnumber.");
			}
			else
			{
				mSNFP_Transaction.TransactionNumber = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_transactionnumber.");
			}
			if (giftTransaction.Contains("msnfp_transactionresult") && giftTransaction["msnfp_transactionresult"] != null)
			{
				mSNFP_Transaction.TransactionResult = (string)giftTransaction["msnfp_transactionresult"];
				localContext.TracingService.Trace("Got msnfp_transactionresult.");
			}
			else
			{
				mSNFP_Transaction.TransactionResult = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_transactionresult.");
			}
			if (giftTransaction.Contains("msnfp_dataentryreference") && giftTransaction["msnfp_dataentryreference"] != null)
			{
				mSNFP_Transaction.DataEntryReference = (string)giftTransaction["msnfp_dataentryreference"];
				localContext.TracingService.Trace("Got msnfp_dataentryreference.");
			}
			else
			{
				mSNFP_Transaction.DataEntryReference = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_dataentryreference.");
			}
			if (giftTransaction.Contains("msnfp_relatedconstituentid") && giftTransaction["msnfp_relatedconstituentid"] != null)
			{
				mSNFP_Transaction.ConstituentId = ((EntityReference)giftTransaction["msnfp_relatedconstituentid"]).Id;
				localContext.TracingService.Trace("Got msnfp_relatedconstituentid.");
			}
			else
			{
				mSNFP_Transaction.ConstituentId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_relatedconstituentid.");
			}
			if (giftTransaction.Contains("msnfp_membershipcategoryid") && giftTransaction["msnfp_membershipcategoryid"] != null)
			{
				mSNFP_Transaction.MembershipId = ((EntityReference)giftTransaction["msnfp_membershipcategoryid"]).Id;
				localContext.TracingService.Trace("Got msnfp_membershipcategoryid.");
			}
			else
			{
				mSNFP_Transaction.MembershipId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_membershipcategoryid.");
			}
			if (giftTransaction.Contains("msnfp_membershipinstanceid") && giftTransaction["msnfp_membershipinstanceid"] != null)
			{
				mSNFP_Transaction.MembershipInstanceId = ((EntityReference)giftTransaction["msnfp_membershipinstanceid"]).Id;
				localContext.TracingService.Trace("Got msnfp_membershipinstanceid.");
			}
			else
			{
				mSNFP_Transaction.MembershipInstanceId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_membershipinstanceid.");
			}
			if (giftTransaction.Contains("msnfp_transactionfraudcode") && giftTransaction["msnfp_transactionfraudcode"] != null)
			{
				mSNFP_Transaction.TransactionFraudCode = (string)giftTransaction["msnfp_transactionfraudcode"];
				localContext.TracingService.Trace("Got msnfp_transactionfraudcode.");
			}
			else
			{
				mSNFP_Transaction.TransactionFraudCode = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_transactionfraudcode.");
			}
			if (giftTransaction.Contains("msnfp_transaction_paymentmethodid") && giftTransaction["msnfp_transaction_paymentmethodid"] != null)
			{
				mSNFP_Transaction.TransactionPaymentMethodId = ((EntityReference)giftTransaction["msnfp_transaction_paymentmethodid"]).Id;
				localContext.TracingService.Trace("Got msnfp_transaction_paymentmethodid");
			}
			else
			{
				mSNFP_Transaction.TransactionPaymentMethodId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_transaction_paymentmethodid");
			}
			if (giftTransaction.Contains("msnfp_amount_receipted") && giftTransaction["msnfp_amount_receipted"] != null)
			{
				mSNFP_Transaction.AmountReceipted = ((Money)giftTransaction["msnfp_amount_receipted"]).Value;
				localContext.TracingService.Trace("Got msnfp_amount_receipted");
			}
			else
			{
				localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted");
			}
			if (giftTransaction.Contains("msnfp_amount_nonreceiptable") && giftTransaction["msnfp_amount_nonreceiptable"] != null)
			{
				mSNFP_Transaction.AmountNonReceiptable = ((Money)giftTransaction["msnfp_amount_nonreceiptable"]).Value;
				localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable");
			}
			else
			{
				mSNFP_Transaction.AmountNonReceiptable = null;
			}
			if (giftTransaction.Contains("msnfp_amount_tax") && giftTransaction["msnfp_amount_tax"] != null)
			{
				mSNFP_Transaction.AmountTax = ((Money)giftTransaction["msnfp_amount_tax"]).Value;
				localContext.TracingService.Trace("Got msnfp_amount_tax");
			}
			else
			{
				mSNFP_Transaction.AmountTax = null;
				localContext.TracingService.Trace("Did NOT find msnfp_amount_tax");
			}
			if (giftTransaction.Contains("msnfp_bookdate") && giftTransaction["msnfp_bookdate"] != null)
			{
				mSNFP_Transaction.BookDate = (DateTime)giftTransaction["msnfp_bookdate"];
				localContext.TracingService.Trace("Got msnfp_bookdate");
			}
			else
			{
				mSNFP_Transaction.BookDate = null;
				localContext.TracingService.Trace("Did NOT find msnfp_bookdate");
			}
			if (giftTransaction.Contains("msnfp_daterefunded") && giftTransaction["msnfp_daterefunded"] != null)
			{
				mSNFP_Transaction.DateRefunded = (DateTime)giftTransaction["msnfp_daterefunded"];
				localContext.TracingService.Trace("Got msnfp_daterefunded");
			}
			else
			{
				mSNFP_Transaction.DateRefunded = null;
				localContext.TracingService.Trace("Did NOT find msnfp_daterefunded");
			}
			if (giftTransaction.Contains("msnfp_originatingcampaignid") && giftTransaction["msnfp_originatingcampaignid"] != null)
			{
				mSNFP_Transaction.OriginatingCampaignId = ((EntityReference)giftTransaction["msnfp_originatingcampaignid"]).Id;
				localContext.TracingService.Trace("Got msnfp_originatingcampaignid");
			}
			else
			{
				mSNFP_Transaction.OriginatingCampaignId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_originatingcampaignid");
			}
			if (giftTransaction.Contains("msnfp_appealid") && giftTransaction["msnfp_appealid"] != null)
			{
				mSNFP_Transaction.AppealId = ((EntityReference)giftTransaction["msnfp_appealid"]).Id;
				localContext.TracingService.Trace("Got msnfp_appealid");
			}
			else
			{
				mSNFP_Transaction.AppealId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_appealid");
			}
			if (giftTransaction.Contains("msnfp_anonymous") && giftTransaction["msnfp_anonymous"] != null)
			{
				mSNFP_Transaction.Anonymous = ((OptionSetValue)giftTransaction["msnfp_anonymous"]).Value;
				localContext.TracingService.Trace("Got msnfp_anonymous: " + mSNFP_Transaction.Anonymous);
			}
			else
			{
				mSNFP_Transaction.Anonymous = null;
				localContext.TracingService.Trace("Did NOT find msnfp_anonymous");
			}
			if (giftTransaction.Contains("msnfp_dataentrysource") && giftTransaction["msnfp_dataentrysource"] != null)
			{
				mSNFP_Transaction.DataEntrySource = ((OptionSetValue)giftTransaction["msnfp_dataentrysource"]).Value;
				localContext.TracingService.Trace("Got msnfp_dataentrysource");
			}
			else
			{
				localContext.TracingService.Trace("Did NOT find msnfp_dataentrysource");
			}
			if (giftTransaction.Contains("msnfp_paymenttypecode") && giftTransaction["msnfp_paymenttypecode"] != null)
			{
				mSNFP_Transaction.PaymentTypeCode = ((OptionSetValue)giftTransaction["msnfp_paymenttypecode"]).Value;
				localContext.TracingService.Trace("Got msnfp_paymenttypecode");
			}
			else
			{
				mSNFP_Transaction.PaymentTypeCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_paymenttypecode");
			}
			if (giftTransaction.Contains("msnfp_ccbrandcode") && giftTransaction["msnfp_ccbrandcode"] != null)
			{
				mSNFP_Transaction.CcBrandCode = ((OptionSetValue)giftTransaction["msnfp_ccbrandcode"]).Value;
				localContext.TracingService.Trace("Got msnfp_ccbrandcode");
			}
			else
			{
				mSNFP_Transaction.CcBrandCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ccbrandcode");
			}
			if (giftTransaction.Contains("msnfp_ga_deliverycode") && giftTransaction["msnfp_ga_deliverycode"] != null)
			{
				mSNFP_Transaction.GaDeliveryCode = ((OptionSetValue)giftTransaction["msnfp_ga_deliverycode"]).Value;
				localContext.TracingService.Trace("Got msnfp_ga_deliverycode");
			}
			else
			{
				mSNFP_Transaction.GaDeliveryCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ga_deliverycode");
			}
			if (giftTransaction.Contains("msnfp_tributecode") && giftTransaction["msnfp_tributecode"] != null)
			{
				mSNFP_Transaction.TributeCode = ((OptionSetValue)giftTransaction["msnfp_tributecode"]).Value;
				localContext.TracingService.Trace("Got msnfp_tributecode");
			}
			else
			{
				mSNFP_Transaction.TributeCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_tributecode");
			}
			if (giftTransaction.Contains("msnfp_tributename") && giftTransaction["msnfp_tributename"] != null)
			{
				mSNFP_Transaction.TributeName = (string)giftTransaction["msnfp_tributename"];
				localContext.TracingService.Trace("Got msnfp_tributename");
			}
			else
			{
				mSNFP_Transaction.TributeName = null;
				localContext.TracingService.Trace("Did NOT find msnfp_tributename");
			}
			if (giftTransaction.Contains("msnfp_ga_applicablecode") && giftTransaction["msnfp_ga_applicablecode"] != null)
			{
				mSNFP_Transaction.GaApplicableCode = ((OptionSetValue)giftTransaction["msnfp_ga_applicablecode"]).Value;
				localContext.TracingService.Trace("Got msnfp_ga_applicablecode");
			}
			else
			{
				mSNFP_Transaction.GaApplicableCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ga_applicablecode");
			}
			if (giftTransaction.Contains("msnfp_receiptpreferencecode") && giftTransaction["msnfp_receiptpreferencecode"] != null)
			{
				mSNFP_Transaction.ReceiptPreferenceCode = ((OptionSetValue)giftTransaction["msnfp_receiptpreferencecode"]).Value;
				localContext.TracingService.Trace("Got msnfp_receiptpreferencecode");
			}
			else
			{
				mSNFP_Transaction.ReceiptPreferenceCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_receiptpreferencecode");
			}
			if (giftTransaction.Contains("msnfp_ga_returnid") && giftTransaction["msnfp_ga_returnid"] != null)
			{
				mSNFP_Transaction.GaReturnId = ((EntityReference)giftTransaction["msnfp_ga_returnid"]).Id;
				localContext.TracingService.Trace("Got msnfp_ga_returnid");
			}
			else
			{
				mSNFP_Transaction.GaReturnId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ga_returnid");
			}
			if (giftTransaction.Contains("msnfp_donorcommitmentid") && giftTransaction["msnfp_donorcommitmentid"] != null)
			{
				mSNFP_Transaction.DonorCommitmentId = ((EntityReference)giftTransaction["msnfp_donorcommitmentid"]).Id;
				localContext.TracingService.Trace("Got msnfp_donorcommitmentid");
			}
			else
			{
				mSNFP_Transaction.DonorCommitmentId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_donorcommitmentid");
			}
			if (giftTransaction.Contains("msnfp_giftbatchid") && giftTransaction["msnfp_giftbatchid"] != null)
			{
				mSNFP_Transaction.GiftBatchId = ((EntityReference)giftTransaction["msnfp_giftbatchid"]).Id;
				localContext.TracingService.Trace("Got msnfp_giftbatchid");
			}
			else
			{
				mSNFP_Transaction.GiftBatchId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_giftbatchid");
			}
			if (giftTransaction.Contains("msnfp_telephone1") && giftTransaction["msnfp_telephone1"] != null)
			{
				mSNFP_Transaction.Telephone1 = (string)giftTransaction["msnfp_telephone1"];
				localContext.TracingService.Trace("Got msnfp_telephone1");
			}
			else
			{
				mSNFP_Transaction.Telephone1 = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_telephone1");
			}
			if (giftTransaction.Contains("msnfp_telephone2") && giftTransaction["msnfp_telephone2"] != null)
			{
				mSNFP_Transaction.Telephone2 = (string)giftTransaction["msnfp_telephone2"];
				localContext.TracingService.Trace("Got msnfp_telephone2");
			}
			else
			{
				mSNFP_Transaction.Telephone2 = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_telephone2");
			}
			if (giftTransaction.Contains("msnfp_tributeacknowledgement") && giftTransaction["msnfp_tributeacknowledgement"] != null)
			{
				mSNFP_Transaction.TributeAcknowledgement = (string)giftTransaction["msnfp_tributeacknowledgement"];
				localContext.TracingService.Trace("Got msnfp_tributeacknowledgement");
			}
			else
			{
				mSNFP_Transaction.TributeAcknowledgement = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_tributeacknowledgement");
			}
			if (giftTransaction.Contains("msnfp_mobilephone") && giftTransaction["msnfp_mobilephone"] != null)
			{
				mSNFP_Transaction.MobilePhone = (string)giftTransaction["msnfp_mobilephone"];
				localContext.TracingService.Trace("Got msnfp_mobilephone");
			}
			else
			{
				mSNFP_Transaction.MobilePhone = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_mobilephone");
			}
			if (giftTransaction.Contains("msnfp_thirdpartyreceipt") && giftTransaction["msnfp_thirdpartyreceipt"] != null)
			{
				mSNFP_Transaction.ThirdPartyReceipt = (string)giftTransaction["msnfp_thirdpartyreceipt"];
				localContext.TracingService.Trace("Got msnfp_thirdpartyreceipt");
			}
			else
			{
				mSNFP_Transaction.ThirdPartyReceipt = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_thirdpartyreceipt");
			}
			if (giftTransaction.Contains("msnfp_emailaddress1") && giftTransaction["msnfp_emailaddress1"] != null)
			{
				mSNFP_Transaction.Emailaddress1 = (string)giftTransaction["msnfp_emailaddress1"];
				localContext.TracingService.Trace("Got msnfp_emailaddress1");
			}
			else
			{
				mSNFP_Transaction.Emailaddress1 = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_emailaddress1");
			}
			if (giftTransaction.Contains("msnfp_organizationname") && giftTransaction["msnfp_organizationname"] != null)
			{
				mSNFP_Transaction.OrganizationName = (string)giftTransaction["msnfp_organizationname"];
				localContext.TracingService.Trace("Got msnfp_organizationname");
			}
			else
			{
				mSNFP_Transaction.OrganizationName = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_organizationname");
			}
			if (giftTransaction.Contains("msnfp_transactiondescription") && giftTransaction["msnfp_transactiondescription"] != null)
			{
				mSNFP_Transaction.TransactionDescription = (string)giftTransaction["msnfp_transactiondescription"];
				localContext.TracingService.Trace("Got msnfp_transactiondescription");
			}
			else
			{
				mSNFP_Transaction.TransactionDescription = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_transactiondescription");
			}
			if (giftTransaction.Contains("msnfp_appraiser") && giftTransaction["msnfp_appraiser"] != null)
			{
				mSNFP_Transaction.Appraiser = (string)giftTransaction["msnfp_appraiser"];
				localContext.TracingService.Trace("Got msnfp_appraiser");
			}
			else
			{
				mSNFP_Transaction.Appraiser = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_appraiser");
			}
			if (giftTransaction.Contains("msnfp_transaction_paymentscheduleid") && giftTransaction["msnfp_transaction_paymentscheduleid"] != null)
			{
				mSNFP_Transaction.TransactionPaymentScheduleId = ((EntityReference)giftTransaction["msnfp_transaction_paymentscheduleid"]).Id;
				localContext.TracingService.Trace("Got msnfp_transaction_paymentscheduleid");
			}
			else
			{
				mSNFP_Transaction.TransactionPaymentScheduleId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_transaction_paymentscheduleid");
			}
			if (giftTransaction.Contains("msnfp_paymentprocessorid") && giftTransaction["msnfp_paymentprocessorid"] != null)
			{
				mSNFP_Transaction.PaymentProcessorId = ((EntityReference)giftTransaction["msnfp_paymentprocessorid"]).Id;
				localContext.TracingService.Trace("Got msnfp_paymentprocessorid");
			}
			else
			{
				mSNFP_Transaction.PaymentProcessorId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_paymentprocessorid");
			}
			if (giftTransaction.Contains("msnfp_packageid") && giftTransaction["msnfp_packageid"] != null)
			{
				mSNFP_Transaction.PackageId = ((EntityReference)giftTransaction["msnfp_packageid"]).Id;
				localContext.TracingService.Trace("Got msnfp_packageid");
			}
			else
			{
				mSNFP_Transaction.PackageId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_packageid");
			}
			if (giftTransaction.Contains("msnfp_taxreceiptid") && giftTransaction["msnfp_taxreceiptid"] != null)
			{
				mSNFP_Transaction.TaxReceiptId = ((EntityReference)giftTransaction["msnfp_taxreceiptid"]).Id;
				localContext.TracingService.Trace("Got msnfp_taxreceiptid");
			}
			else
			{
				mSNFP_Transaction.TaxReceiptId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_taxreceiptid");
			}
			if (giftTransaction.Contains("msnfp_tributeid") && giftTransaction["msnfp_tributeid"] != null)
			{
				mSNFP_Transaction.TributeId = ((EntityReference)giftTransaction["msnfp_tributeid"]).Id;
				localContext.TracingService.Trace("Got msnfp_tributeid");
			}
			else
			{
				mSNFP_Transaction.TributeId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_tributeid");
			}
			if (giftTransaction.Contains("msnfp_invoiceidentifier") && giftTransaction["msnfp_invoiceidentifier"] != null)
			{
				mSNFP_Transaction.InvoiceIdentifier = (string)giftTransaction["msnfp_invoiceidentifier"];
				localContext.TracingService.Trace("Got msnfp_invoiceidentifier");
			}
			else
			{
				mSNFP_Transaction.InvoiceIdentifier = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_invoiceidentifier");
			}
			if (giftTransaction.Contains("msnfp_tributemessage") && giftTransaction["msnfp_tributemessage"] != null)
			{
				mSNFP_Transaction.TributeMessage = (string)giftTransaction["msnfp_tributemessage"];
				localContext.TracingService.Trace("Got msnfp_tributemessage");
			}
			else
			{
				mSNFP_Transaction.TributeMessage = string.Empty;
				localContext.TracingService.Trace("Did NOT find msnfp_tributemessage");
			}
			if (giftTransaction.Contains("msnfp_configurationid") && giftTransaction["msnfp_configurationid"] != null)
			{
				mSNFP_Transaction.ConfigurationId = ((EntityReference)giftTransaction["msnfp_configurationid"]).Id;
				localContext.TracingService.Trace("Got msnfp_configurationid");
			}
			else
			{
				mSNFP_Transaction.ConfigurationId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_configurationid");
			}
			if (giftTransaction.Contains("msnfp_eventid") && giftTransaction["msnfp_eventid"] != null)
			{
				mSNFP_Transaction.EventId = ((EntityReference)giftTransaction["msnfp_eventid"]).Id;
				localContext.TracingService.Trace("Got msnfp_eventid");
			}
			else
			{
				mSNFP_Transaction.EventId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_eventid");
			}
			if (giftTransaction.Contains("msnfp_eventpackageid") && giftTransaction["msnfp_eventpackageid"] != null)
			{
				mSNFP_Transaction.EventPackageId = ((EntityReference)giftTransaction["msnfp_eventpackageid"]).Id;
				localContext.TracingService.Trace("Got msnfp_eventpackageid");
			}
			else
			{
				mSNFP_Transaction.EventPackageId = null;
				localContext.TracingService.Trace("Did NOT find msnfp_eventpackageid");
			}
			if (giftTransaction.Contains("msnfp_amount_membership") && giftTransaction["msnfp_amount_membership"] != null)
			{
				mSNFP_Transaction.AmountMembership = ((Money)giftTransaction["msnfp_amount_membership"]).Value;
				localContext.TracingService.Trace("Got msnfp_amount_membership");
			}
			if (giftTransaction.Contains("msnfp_ref_amount_membership") && giftTransaction["msnfp_ref_amount_membership"] != null)
			{
				mSNFP_Transaction.RefAmountMembership = ((Money)giftTransaction["msnfp_ref_amount_membership"]).Value;
				localContext.TracingService.Trace("Got msnfp_ref_amount_membership");
			}
			if (giftTransaction.Contains("msnfp_ref_amount_nonreceiptable") && giftTransaction["msnfp_ref_amount_nonreceiptable"] != null)
			{
				mSNFP_Transaction.RefAmountNonreceiptable = ((Money)giftTransaction["msnfp_ref_amount_nonreceiptable"]).Value;
				localContext.TracingService.Trace("Got msnfp_ref_amount_nonreceiptable");
			}
			else
			{
				mSNFP_Transaction.RefAmountNonreceiptable = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_nonreceiptable");
			}
			if (giftTransaction.Contains("msnfp_ref_amount_tax") && giftTransaction["msnfp_ref_amount_tax"] != null)
			{
				mSNFP_Transaction.RefAmountTax = ((Money)giftTransaction["msnfp_ref_amount_tax"]).Value;
				localContext.TracingService.Trace("Got msnfp_ref_amount_tax");
			}
			else
			{
				mSNFP_Transaction.RefAmountTax = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_tax");
			}
			if (giftTransaction.Contains("msnfp_ref_amount") && giftTransaction["msnfp_ref_amount"] != null)
			{
				mSNFP_Transaction.RefAmount = ((Money)giftTransaction["msnfp_ref_amount"]).Value;
				localContext.TracingService.Trace("Got msnfp_ref_amount");
			}
			else
			{
				mSNFP_Transaction.RefAmount = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ref_amount");
			}
			if (giftTransaction.Contains("msnfp_amount_transfer") && giftTransaction["msnfp_amount_transfer"] != null)
			{
				mSNFP_Transaction.AmountTransfer = ((Money)giftTransaction["msnfp_amount_transfer"]).Value;
				localContext.TracingService.Trace("Got msnfp_amount_transfer");
			}
			else
			{
				mSNFP_Transaction.AmountTransfer = null;
				localContext.TracingService.Trace("Did NOT find msnfp_amount_transfer");
			}
			if (giftTransaction.Contains("msnfp_ga_amount_claimed") && giftTransaction["msnfp_ga_amount_claimed"] != null)
			{
				mSNFP_Transaction.GaAmountClaimed = ((Money)giftTransaction["msnfp_ga_amount_claimed"]).Value;
				localContext.TracingService.Trace("Got msnfp_ga_amount_claimed");
			}
			else
			{
				mSNFP_Transaction.GaAmountClaimed = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ga_amount_claimed");
			}
			if (giftTransaction.Contains("msnfp_currentretry") && giftTransaction["msnfp_currentretry"] != null)
			{
				mSNFP_Transaction.CurrentRetry = (int)giftTransaction["msnfp_currentretry"];
				localContext.TracingService.Trace("Got msnfp_currentretry");
			}
			if (giftTransaction.Contains("msnfp_nextfailedretry") && giftTransaction["msnfp_nextfailedretry"] != null)
			{
				mSNFP_Transaction.NextFailedRetry = (DateTime)giftTransaction["msnfp_nextfailedretry"];
				localContext.TracingService.Trace("Got msnfp_nextfailedretry");
			}
			else
			{
				mSNFP_Transaction.NextFailedRetry = null;
				localContext.TracingService.Trace("Did NOT find msnfp_nextfailedretry");
			}
			if (giftTransaction.Contains("msnfp_returneddate") && giftTransaction["msnfp_returneddate"] != null)
			{
				mSNFP_Transaction.ReturnedDate = (DateTime)giftTransaction["msnfp_returneddate"];
				localContext.TracingService.Trace("Got msnfp_returneddate");
			}
			else
			{
				mSNFP_Transaction.ReturnedDate = null;
				localContext.TracingService.Trace("Did NOT find msnfp_returneddate");
			}
			if (giftTransaction.Contains("msnfp_receiveddate") && giftTransaction["msnfp_receiveddate"] != null)
			{
				mSNFP_Transaction.ReceivedDate = (DateTime)giftTransaction["msnfp_receiveddate"];
				localContext.TracingService.Trace("Got msnfp_receiveddate");
			}
			else
			{
				mSNFP_Transaction.ReceivedDate = null;
				localContext.TracingService.Trace("Did NOT find msnfp_receiveddate");
			}
			if (giftTransaction.Contains("msnfp_lastfailedretry") && giftTransaction["msnfp_lastfailedretry"] != null)
			{
				mSNFP_Transaction.LastFailedRetry = (DateTime)giftTransaction["msnfp_lastfailedretry"];
				localContext.TracingService.Trace("Got msnfp_lastfailedretry");
			}
			else
			{
				mSNFP_Transaction.LastFailedRetry = null;
				localContext.TracingService.Trace("Did NOT find msnfp_lastfailedretry");
			}
			if (giftTransaction.Contains("msnfp_validationdate") && giftTransaction["msnfp_validationdate"] != null)
			{
				mSNFP_Transaction.ValidationDate = (DateTime)giftTransaction["msnfp_validationdate"];
				localContext.TracingService.Trace("Got msnfp_ValidationDate");
			}
			else
			{
				mSNFP_Transaction.ValidationDate = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ValidationDate");
			}
			if (giftTransaction.Contains("msnfp_validationperformed") && giftTransaction["msnfp_validationperformed"] != null)
			{
				mSNFP_Transaction.ValidationPerformed = (bool)giftTransaction["msnfp_validationperformed"];
				localContext.TracingService.Trace("Got msnfp_validationperformed");
			}
			else
			{
				mSNFP_Transaction.ValidationPerformed = false;
				localContext.TracingService.Trace("Did NOT find msnfp_validationperformed");
			}
			if (giftTransaction.Contains("msnfp_chargeoncreate") && giftTransaction["msnfp_chargeoncreate"] != null)
			{
				mSNFP_Transaction.ChargeonCreate = (bool)giftTransaction["msnfp_chargeoncreate"];
				localContext.TracingService.Trace("Got msnfp_chargeoncreate");
			}
			else
			{
				mSNFP_Transaction.ChargeonCreate = false;
				localContext.TracingService.Trace("Did NOT find msnfp_chargeoncreate");
			}
			if (giftTransaction.Contains("msnfp_amount") && giftTransaction["msnfp_amount"] != null)
			{
				mSNFP_Transaction.Amount = ((Money)giftTransaction["msnfp_amount"]).Value;
				localContext.TracingService.Trace("Got msnfp_amount (total header)");
			}
			if (giftTransaction.Contains("msnfp_ref_amount_receipted") && giftTransaction["msnfp_ref_amount_receipted"] != null)
			{
				mSNFP_Transaction.RefAmountReceipted = ((Money)giftTransaction["msnfp_ref_amount_receipted"]).Value;
				localContext.TracingService.Trace("Got msnfp_ref_amount_receipted");
			}
			else
			{
				mSNFP_Transaction.RefAmountReceipted = null;
				localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_receipted");
			}
			if (giftTransaction.Contains("transactioncurrencyid") && giftTransaction["transactioncurrencyid"] != null)
			{
				mSNFP_Transaction.TransactionCurrencyId = ((EntityReference)giftTransaction["transactioncurrencyid"]).Id;
				localContext.TracingService.Trace("Got TransactionCurrencyId.");
			}
			else
			{
				mSNFP_Transaction.TransactionCurrencyId = null;
				localContext.TracingService.Trace("Did NOT find TransactionCurrencyId.");
			}
			if (giftTransaction.Contains("owningbusinessunit") && giftTransaction["owningbusinessunit"] != null)
			{
				mSNFP_Transaction.OwningBusinessUnitId = ((EntityReference)giftTransaction["owningbusinessunit"]).Id;
				localContext.TracingService.Trace("Got OwningBusinessUnitId.");
			}
			else
			{
				mSNFP_Transaction.OwningBusinessUnitId = null;
				localContext.TracingService.Trace("Did NOT find OwningBusinessUnitId.");
			}
			if (giftTransaction.Contains("msnfp_depositdate") && giftTransaction["msnfp_depositdate"] != null)
			{
				mSNFP_Transaction.DepositDate = (DateTime)giftTransaction["msnfp_depositdate"];
				localContext.TracingService.Trace("Got DepositDate.");
			}
			else
			{
				mSNFP_Transaction.DepositDate = null;
				localContext.TracingService.Trace("Did NOT find DepositDate");
			}
			if (giftTransaction.Contains("msnfp_typecode") && giftTransaction["msnfp_typecode"] != null)
			{
				mSNFP_Transaction.TypeCode = ((OptionSetValue)giftTransaction["msnfp_typecode"]).Value;
				localContext.TracingService.Trace("Got msnfp_typecode");
			}
			else
			{
				mSNFP_Transaction.TypeCode = null;
				localContext.TracingService.Trace("Did NOT find msnfp_typecode");
			}
			if (messageName == "Create")
			{
				mSNFP_Transaction.CreatedOn = DateTime.UtcNow;
			}
			else if (giftTransaction.Contains("createdon") && giftTransaction["createdon"] != null)
			{
				mSNFP_Transaction.CreatedOn = (DateTime)giftTransaction["createdon"];
			}
			else
			{
				mSNFP_Transaction.CreatedOn = null;
			}
			mSNFP_Transaction.Response = new HashSet<MSNFP_Response>();
			mSNFP_Transaction.Refund = new HashSet<MSNFP_Refund>();
			mSNFP_Transaction.StatusCode = ((OptionSetValue)giftTransaction["statuscode"]).Value;
			mSNFP_Transaction.StateCode = ((OptionSetValue)giftTransaction["statecode"]).Value;
			if (messageName == "Delete")
			{
				mSNFP_Transaction.Deleted = true;
				mSNFP_Transaction.DeletedDate = DateTime.UtcNow;
			}
			else
			{
				mSNFP_Transaction.Deleted = false;
				mSNFP_Transaction.DeletedDate = null;
			}
			mSNFP_Transaction.SyncDate = DateTime.UtcNow;
			localContext.TracingService.Trace("JSON object created");
			if (messageName == "Create")
			{
				text += "Transaction/CreateTransaction";
			}
			else if (messageName == "Update" || messageName == "Delete")
			{
				text += "Transaction/UpdateTransaction";
			}
			MemoryStream memoryStream = new MemoryStream();
			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Transaction));
			dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Transaction);
			byte[] array = memoryStream.ToArray();
			memoryStream.Close();
			string @string = Encoding.UTF8.GetString(array, 0, array.Length);
			WebAPIClient webAPIClient = new WebAPIClient();
			webAPIClient.Headers[HttpRequestHeader.ContentType] = "application/json";
			webAPIClient.Headers["Padlock"] = (string)configurationRecord["msnfp_apipadlocktoken"];
			webAPIClient.Encoding = Encoding.UTF8;
			localContext.TracingService.Trace("---------Preparing JSON---------");
			localContext.TracingService.Trace("Converted to json API URL : " + text);
			localContext.TracingService.Trace("JSON: " + @string);
			localContext.TracingService.Trace("---------End of Preparing JSON---------");
			localContext.TracingService.Trace("Sending data to Azure.");
			string str = webAPIClient.UploadString(text, @string);
			localContext.TracingService.Trace("Got response.");
			localContext.TracingService.Trace("Response: " + str);
		}

		private void UpdateEventPackageDonationTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventPackageDonationTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventpackageid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity eventPackage = service.Retrieve("msnfp_eventpackage", ((EntityReference)queriedEntityRecord["msnfp_eventpackageid"]).Id, new ColumnSet("msnfp_eventpackageid", "msnfp_amount"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_transaction")
				where ((EntityReference)a["msnfp_eventpackageid"]).Id == eventPackage.Id && ((OptionSetValue)a["statuscode"]).Value == 844060000
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
				{
					value += ((Money)item["msnfp_amount"]).Value;
				}
			}
			eventPackage["msnfp_sum_donations"] = list.Count();
			eventPackage["msnfp_val_donations"] = new Money(value);
			service.Update(eventPackage);
		}

		private void UpdateEventTotals(Entity queriedEntityRecord, OrganizationServiceContext orgSvcContext, IOrganizationService service, LocalPluginContext localContext)
		{
			localContext.TracingService.Trace("---------UpdateEventTotals---------");
			if (!queriedEntityRecord.Contains("msnfp_eventid"))
			{
				return;
			}
			decimal value = default(decimal);
			Entity parentEvent = service.Retrieve("msnfp_event", ((EntityReference)queriedEntityRecord["msnfp_eventid"]).Id, new ColumnSet("msnfp_eventid", "msnfp_sum_donations", "msnfp_count_donations"));
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_transaction")
				where ((EntityReference)a["msnfp_eventid"]).Id == parentEvent.Id && ((OptionSetValue)a["statuscode"]).Value == 844060000
				select a).ToList();
			if (list.Count > 0)
			{
				foreach (Entity item in list)
				{
					if (item.Contains("msnfp_amount") && item["msnfp_amount"] != null)
					{
						value += ((Money)item["msnfp_amount"]).Value;
					}
				}
			}
			parentEvent["msnfp_count_donations"] = list.Count();
			parentEvent["msnfp_sum_donations"] = new Money(value);
			decimal value2 = Plugins.PaymentProcesses.Utilities.CalculateEventTotalRevenue(parentEvent, service, orgSvcContext, localContext.TracingService);
			parentEvent["msnfp_sum_total"] = new Money(value2);
			service.Update(parentEvent);
		}

		private void AddPledgeFromPledgeMatchRecord(Entity giftRecord, Entity pledgeMatchRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Entering AddPledgeFromPledgeMatchRecord()---------");
			if (pledgeMatchRecord.Contains("msnfp_appliestocode"))
			{
				if (((OptionSetValue)pledgeMatchRecord["msnfp_appliestocode"]).Value != 844060000)
				{
					localContext.TracingService.Trace("msnfp_appliestocode applies to transaction create, value: " + ((OptionSetValue)pledgeMatchRecord["msnfp_appliestocode"]).Value);
					int num = (pledgeMatchRecord.Contains("msnfp_percentage") ? ((int)pledgeMatchRecord["msnfp_percentage"]) : 100);
					Money money = (Money)giftRecord["msnfp_amount_receipted"];
					Money money2 = new Money();
					if (money.Value == 0m || num == 0)
					{
						money2.Value = 0m;
					}
					else
					{
						money2.Value = money.Value * (decimal)num / 100m;
					}
					localContext.TracingService.Trace("Commitment amount: " + money2.Value);
					Entity entity = new Entity("msnfp_donorcommitment");
					if (pledgeMatchRecord.Contains("msnfp_customertoid"))
					{
						entity["msnfp_customerid"] = pledgeMatchRecord["msnfp_customertoid"];
						string logicalName = ((EntityReference)pledgeMatchRecord["msnfp_customertoid"]).LogicalName;
						Guid id = ((EntityReference)pledgeMatchRecord["msnfp_customertoid"]).Id;
						Entity entity2 = null;
						if (logicalName == "contact")
						{
							entity2 = service.Retrieve(logicalName, id, new ColumnSet("contactid", "firstname", "lastname", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "emailaddress1"));
							entity["msnfp_firstname"] = (entity2.Contains("firstname") ? ((string)entity2["firstname"]) : string.Empty);
							entity["msnfp_lastname"] = (entity2.Contains("lastname") ? ((string)entity2["lastname"]) : string.Empty);
						}
						else if (logicalName == "account")
						{
							entity2 = service.Retrieve(logicalName, id, new ColumnSet("accountid", "name", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "telephone1", "emailaddress1"));
						}
						entity["msnfp_billing_line1"] = (entity2.Contains("address1_line1") ? ((string)entity2["address1_line1"]) : string.Empty);
						entity["msnfp_billing_line2"] = (entity2.Contains("address1_line2") ? ((string)entity2["address1_line2"]) : string.Empty);
						entity["msnfp_billing_line3"] = (entity2.Contains("address1_line3") ? ((string)entity2["address1_line3"]) : string.Empty);
						entity["msnfp_billing_city"] = (entity2.Contains("address1_city") ? ((string)entity2["address1_city"]) : string.Empty);
						entity["msnfp_billing_stateorprovince"] = (entity2.Contains("address1_stateorprovince") ? ((string)entity2["address1_stateorprovince"]) : string.Empty);
						entity["msnfp_billing_country"] = (entity2.Contains("address1_country") ? ((string)entity2["address1_country"]) : string.Empty);
						entity["msnfp_billing_postalcode"] = (entity2.Contains("address1_postalcode") ? ((string)entity2["address1_postalcode"]) : string.Empty);
						entity["msnfp_telephone1"] = (entity2.Contains("telephone1") ? ((string)entity2["telephone1"]) : string.Empty);
						entity["msnfp_emailaddress1"] = (entity2.Contains("emailaddress1") ? ((string)entity2["emailaddress1"]) : string.Empty);
					}
					entity["msnfp_customerid"] = pledgeMatchRecord["msnfp_customertoid"];
					entity["msnfp_totalamount"] = money2;
					entity["msnfp_totalamount_paid"] = new Money(0m);
					entity["msnfp_totalamount_balance"] = money2;
					entity["msnfp_bookdate"] = (giftRecord.Contains("msnfp_bookdate") ? giftRecord["msnfp_bookdate"] : ((object)DateTime.Today));
					entity["createdby"] = new EntityReference("systemuser", context.InitiatingUserId);
					if (giftRecord.Contains("msnfp_originatingcampaignid"))
					{
						localContext.TracingService.Trace("Campaign ID: " + ((EntityReference)giftRecord["msnfp_originatingcampaignid"]).Id.ToString());
						entity["msnfp_commitment_campaignid"] = new EntityReference("campaign", ((EntityReference)giftRecord["msnfp_originatingcampaignid"]).Id);
					}
					if (giftRecord.Contains("msnfp_appealid"))
					{
						localContext.TracingService.Trace("Appeal ID: " + ((EntityReference)giftRecord["msnfp_appealid"]).Id.ToString());
						entity["msnfp_appealid"] = new EntityReference("msnfp_appeal", ((EntityReference)giftRecord["msnfp_appealid"]).Id);
					}
					if (giftRecord.Contains("msnfp_packageid"))
					{
						localContext.TracingService.Trace("Package ID: " + ((EntityReference)giftRecord["msnfp_packageid"]).Id.ToString());
						entity["msnfp_packageid"] = new EntityReference("msnfp_package", ((EntityReference)giftRecord["msnfp_packageid"]).Id);
					}
					if (giftRecord.Contains("msnfp_designationid"))
					{
						localContext.TracingService.Trace("msnfp_designationid ID: " + ((EntityReference)giftRecord["msnfp_designationid"]).Id.ToString());
						entity["msnfp_designationid"] = new EntityReference("msnfp_designation", ((EntityReference)giftRecord["msnfp_designationid"]).Id);
						localContext.TracingService.Trace("msnfp_commitment_defaultdesignationid ID: " + ((EntityReference)giftRecord["msnfp_designationid"]).Id.ToString());
						entity["msnfp_commitment_defaultdesignationid"] = new EntityReference("msnfp_designation", ((EntityReference)giftRecord["msnfp_designationid"]).Id);
					}
					entity["statuscode"] = new OptionSetValue(1);
					if (giftRecord.Contains("msnfp_customerid") && ((EntityReference)giftRecord["msnfp_customerid"]).LogicalName == "contact")
					{
						localContext.TracingService.Trace("Constituent is a contact: " + ((EntityReference)giftRecord["msnfp_customerid"]).Id.ToString());
						entity["msnfp_constituentid"] = new EntityReference("contact", ((EntityReference)giftRecord["msnfp_customerid"]).Id);
					}
					if (giftRecord.Contains("msnfp_configurationid"))
					{
						localContext.TracingService.Trace("Configuration record id: " + ((EntityReference)giftRecord["msnfp_configurationid"]).Id.ToString());
						entity["msnfp_configurationid"] = new EntityReference("msnfp_configuration", ((EntityReference)giftRecord["msnfp_configurationid"]).Id);
					}
					localContext.TracingService.Trace("Creating new donor commitment.");
					service.Create(entity);
					localContext.TracingService.Trace("New donor commitment created.");
				}
				else
				{
					localContext.TracingService.Trace("This pledge match is not for Transactions, msnfp_appliestocode: " + ((OptionSetValue)pledgeMatchRecord["msnfp_appliestocode"]).Value);
				}
			}
			else
			{
				localContext.TracingService.Trace("No msnfp_appliestocode value found.");
			}
			localContext.TracingService.Trace("---------Exiting AddPledgeFromPledgeMatchRecord()---------");
		}

		private void AttemptToSetPledgeScheduleToCompleted(Guid pledgeScheduleId, int statusCode, LocalPluginContext localContext, IOrganizationService service, OrganizationServiceContext orgSvcContext)
		{
			localContext.TracingService.Trace("---------Entering AttemptToSetPledgeScheduleToCompleted()---------");
			localContext.TracingService.Trace("Transaction status code: " + statusCode);
			ColumnSet columnSet = new ColumnSet("msnfp_paymentscheduleid", "msnfp_name", "msnfp_scheduletypecode", "statuscode", "msnfp_totalamount_balance");
			Entity entity = service.Retrieve("msnfp_paymentschedule", pledgeScheduleId, columnSet);
			if (entity.Contains("msnfp_scheduletypecode"))
			{
				if (((OptionSetValue)entity["msnfp_scheduletypecode"]).Value == 844060005)
				{
					localContext.TracingService.Trace("Parent Schedule is of type Pledge Schedule, checking to see if the parent is indeed complete.");
					List<Guid> list = (from g in orgSvcContext.CreateQuery("msnfp_donorcommitment")
						where ((EntityReference)g["msnfp_parentscheduleid"]).Id == pledgeScheduleId && g["statuscode"] != new OptionSetValue(844060000)
						select g.Id).ToList();
					if (list.Count == 0 && statusCode == 844060000)
					{
						localContext.TracingService.Trace("Count is." + list.Count);
						localContext.TracingService.Trace("Balance  is." + ((Money)entity["msnfp_totalamount_balance"]).Value);
						if (entity.Contains("msnfp_totalamount_balance") && ((Money)entity["msnfp_totalamount_balance"]).Value <= 0m)
						{
							localContext.TracingService.Trace("There are no active donor commitments for this pledge schedule. Setting status to Completed.");
							entity["statuscode"] = new OptionSetValue(844060000);
							service.Update(entity);
							localContext.TracingService.Trace("Updated parent pledge schedule to Completed.");
						}
					}
					else
					{
						ITracingService tracingService = localContext.TracingService;
						string[] obj = new string[5]
						{
							"There are ",
							list.Count.ToString(),
							" active donor commitments for this pledge schedule (",
							null,
							null
						};
						Guid guid = pledgeScheduleId;
						obj[3] = guid.ToString();
						obj[4] = "). Cannot set to completed yet.";
						tracingService.Trace(string.Concat(obj));
					}
					localContext.TracingService.Trace("---------Exiting AttemptToSetPledgeScheduleToCompleted()---------");
				}
				else
				{
					localContext.TracingService.Trace("Payment schedule id: " + pledgeScheduleId.ToString() + " is not a pledge schedule. Exiting function.");
				}
			}
			else
			{
				localContext.TracingService.Trace("Could not find msnfp_scheduletypecode on payment schedule id: " + pledgeScheduleId.ToString() + ". Exiting function.");
			}
		}

		private void UpdateCustomerPrimaryMembership(Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service, OrganizationServiceContext orgSvcContext)
		{
			localContext.TracingService.Trace("---------Entering UpdateCustomerPrimaryMembership()---------");
			if (giftTransaction.Contains("msnfp_membershipinstanceid") && giftTransaction.Contains("msnfp_customerid"))
			{
				ColumnSet columnSet = new ColumnSet("msnfp_membershipid", "msnfp_primary", "msnfp_customer");
				Entity entity = service.Retrieve("msnfp_membership", ((EntityReference)giftTransaction["msnfp_membershipinstanceid"]).Id, columnSet);
				localContext.TracingService.Trace("Membership Instance: " + ((Guid)entity["msnfp_membershipid"]).ToString());
				if (entity.Contains("msnfp_primary") && ((EntityReference)giftTransaction["msnfp_customerid"]).Id != Guid.Empty && (bool)entity["msnfp_primary"])
				{
					localContext.TracingService.Trace("Updating Primary Membership for Customer: " + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString());
					List<Guid> list = (from s in orgSvcContext.CreateQuery("msnfp_membership")
						where ((EntityReference)s["msnfp_customer"]).Id == ((EntityReference)giftTransaction["msnfp_customerid"]).Id && (byte)s["msnfp_primary"] != 0 == true
						select s.Id).ToList();
					Entity entity2 = null;
					if (((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						if (!entity.Contains("msnfp_customer"))
						{
							entity["msnfp_customer"] = new EntityReference("contact", ((EntityReference)giftTransaction["msnfp_customerid"]).Id);
							service.Update(entity);
						}
						ColumnSet columnSet2 = new ColumnSet("contactid", "msnfp_primarymembershipid");
						entity2 = service.Retrieve("contact", ((EntityReference)giftTransaction["msnfp_customerid"]).Id, columnSet2);
						entity2["msnfp_primarymembershipid"] = new EntityReference("msnfp_membership", ((EntityReference)giftTransaction["msnfp_membershipinstanceid"]).Id);
						service.Update(entity2);
						localContext.TracingService.Trace("Update Complete");
						localContext.TracingService.Trace("Updating " + list.Count + " additional membership instance records to not primary.");
						foreach (Guid item in list)
						{
							ColumnSet columnSet3 = new ColumnSet("msnfp_membershipid", "msnfp_primary");
							Entity entity3 = service.Retrieve("msnfp_membership", item, columnSet3);
							entity3["msnfp_primary"] = false;
							service.Update(entity3);
						}
					}
					else if (((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						if (!entity.Contains("msnfp_customer"))
						{
							entity["msnfp_customer"] = new EntityReference("account", ((EntityReference)giftTransaction["msnfp_customerid"]).Id);
							service.Update(entity);
						}
						ColumnSet columnSet4 = new ColumnSet("accountid", "msnfp_primarymembershipid");
						entity2 = service.Retrieve("account", ((EntityReference)giftTransaction["msnfp_customerid"]).Id, columnSet4);
						entity2["msnfp_primarymembershipid"] = new EntityReference("msnfp_membership", ((EntityReference)giftTransaction["msnfp_membershipinstanceid"]).Id);
						service.Update(entity2);
						localContext.TracingService.Trace("Update Complete");
						localContext.TracingService.Trace("Updating " + list.Count + " additional membership instance records to not primary.");
						foreach (Guid item2 in list)
						{
							ColumnSet columnSet5 = new ColumnSet("msnfp_membershipid", "msnfp_primary");
							Entity entity4 = service.Retrieve("msnfp_membership", item2, columnSet5);
							entity4["msnfp_primary"] = false;
							service.Update(entity4);
						}
					}
				}
			}
			localContext.TracingService.Trace("---------Exiting UpdateCustomerPrimaryMembership()---------");
		}

		private void removePaymentMethod(Entity paymentMethod, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------Attempting to delete payment method---------");
			if (paymentMethod == null)
			{
				localContext.TracingService.Trace("Payment Method does not exist, cannot remove.");
				return;
			}
			localContext.TracingService.Trace("Is Reusable Payment Method: " + (bool)paymentMethod["msnfp_isreusable"]);
			if (!(bool)paymentMethod["msnfp_isreusable"])
			{
				localContext.TracingService.Trace("Payment Method is Not Reusable.");
				try
				{
					localContext.TracingService.Trace("Deleting Payment Method Id: " + paymentMethod.Id.ToString());
					service.Delete("msnfp_paymentmethod", paymentMethod.Id);
					localContext.TracingService.Trace("Payment Method successfully removed. ");
				}
				catch (Exception ex)
				{
					localContext.TracingService.Trace("removePaymentMethod() Error: " + ex.ToString());
				}
			}
			else
			{
				localContext.TracingService.Trace("Payment Method is Reusable. Ignoring Delete.");
			}
		}

		private void MaskStripeCreditCard(LocalPluginContext localContext, Entity primaryCreditCard, string cardId, string cardBrand, string customerId)
		{
			localContext.TracingService.Trace("Inside the method MaskStripeCreditCard. ");
			string str = (string)(primaryCreditCard["msnfp_cclast4"] = primaryCreditCard["msnfp_cclast4"].ToString().Substring(primaryCreditCard["msnfp_cclast4"].ToString().Length - 4));
			if (cardBrand != null)
			{
				switch (cardBrand)
				{
				case "MasterCard":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060001);
					break;
				case "Visa":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060000);
					break;
				case "American Express":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060004);
					break;
				case "Discover":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060008);
					break;
				case "Diners Club":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060005);
					break;
				case "UnionPay":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060009);
					break;
				case "JCB":
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060006);
					break;
				default:
					primaryCreditCard["msnfp_ccbrandcode"] = new OptionSetValue(844060010);
					break;
				}
			}
			localContext.TracingService.Trace("CC Number : " + str);
			primaryCreditCard["msnfp_authtoken"] = cardId;
			primaryCreditCard["msnfp_stripecustomerid"] = customerId;
			localContext.OrganizationService.Update(primaryCreditCard);
			localContext.TracingService.Trace("credit card record updated...MaskStripeCreditCard");
		}

		private void setStatusCodeOnTransaction(Entity giftTransaction, int statuscode, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------Attempting to change transaction status.---------");
			if (giftTransaction == null)
			{
				localContext.TracingService.Trace("Transaction does not exist.");
				return;
			}
			try
			{
				localContext.TracingService.Trace("Set statuscode to: " + statuscode + " for transaction id: " + giftTransaction.Id.ToString());
				giftTransaction["statuscode"] = new OptionSetValue(statuscode);
				service.Update(giftTransaction);
				localContext.TracingService.Trace("Updated transaction status successfully.");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("setStatusCodeOnTransaction() Error: " + ex.ToString());
			}
		}

		private Entity getPaymentMethodForTransaction(Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service)
		{
			if (giftTransaction.Contains("msnfp_transaction_paymentmethodid"))
			{
				return service.Retrieve("msnfp_paymentmethod", ((EntityReference)giftTransaction["msnfp_transaction_paymentmethodid"]).Id, new ColumnSet("msnfp_paymentmethodid", "msnfp_cclast4", "msnfp_ccexpmmyy", "msnfp_paymentprocessorid", "msnfp_type", "msnfp_isreusable", "msnfp_ccbrandcode", "msnfp_authtoken", "msnfp_telephone1", "msnfp_billing_line1", "msnfp_billing_postalcode", "msnfp_emailaddress1", "msnfp_stripecustomerid"));
			}
			localContext.TracingService.Trace("No payment method (msnfp_transaction_paymentmethodid) on this transaction. Exiting plugin.");
			setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
			throw new ArgumentNullException("msnfp_transaction_paymentmethodid");
		}

		private Entity getPaymentProcessorForPaymentMethod(Entity paymentMethod, Entity giftTransaction, LocalPluginContext localContext, IOrganizationService service)
		{
			if (paymentMethod.Contains("msnfp_paymentprocessorid"))
			{
				return service.Retrieve("msnfp_paymentprocessor", ((EntityReference)paymentMethod["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_apikey", "msnfp_name", "msnfp_storeid", "msnfp_avsvalidation", "msnfp_cvdvalidation", "msnfp_testmode", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword"));
			}
			localContext.TracingService.Trace("No payment processor is assigned to this payment method. Exiting plugin.");
			removePaymentMethod(paymentMethod, localContext, service);
			setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
			throw new ArgumentNullException("msnfp_paymentprocessorid");
		}

		private AvsInfo AssignAVSValidationFieldsFromPaymentMethod(Entity giftTransaction, Entity paymentMethod, AvsInfo avsCheck, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering AssignAVSValidationFieldsFromPaymentMethod().");
			try
			{
				if (!paymentMethod.Contains("msnfp_billing_line1") || !paymentMethod.Contains("msnfp_billing_postalcode"))
				{
					localContext.TracingService.Trace("Donor (" + ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName + ") with id = " + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString() + " is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
					throw new Exception("Donor (" + ((EntityReference)giftTransaction["msnfp_customerid"]).LogicalName + ") with id = " + ((EntityReference)giftTransaction["msnfp_customerid"]).Id.ToString() + " is missing either msnfp_billing_line1 or msnfp_billing_postalcode fields on their payment method. Exiting plugin.");
				}
				string[] array = ((string)paymentMethod["msnfp_billing_line1"]).Split(' ');
				if (array.Length <= 1)
				{
					localContext.TracingService.Trace("Could not split address for AVS Validation. Please ensure the Street 1 billing address on the payment method is in the form '123 Example Street'. Exiting plugin.");
					setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
					throw new ArgumentNullException("msnfp_billing_line1");
				}
				string text = (string)paymentMethod["msnfp_billing_line1"];
				localContext.TracingService.Trace("Unformatted Street Name: " + text);
				text = text.Replace(array[0], "").Trim(' ');
				localContext.TracingService.Trace("Formatted Street Name: " + text);
				localContext.TracingService.Trace("Formatted Street Number: " + array[0]);
				avsCheck.SetAvsStreetNumber(array[0]);
				avsCheck.SetAvsStreetName(text);
				avsCheck.SetAvsZipCode((string)paymentMethod["msnfp_billing_postalcode"]);
				if (paymentMethod.Contains("msnfp_emailaddress1"))
				{
					avsCheck.SetAvsEmail((string)paymentMethod["msnfp_emailaddress1"]);
				}
				avsCheck.SetAvsShipMethod("G");
				if (paymentMethod.Contains("msnfp_telephone1"))
				{
					avsCheck.SetAvsCustPhone((string)paymentMethod["msnfp_telephone1"]);
				}
				localContext.TracingService.Trace("Updated AVS Check variable successfully.");
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("AssignAVSValidationFieldsFromPaymentMethod() Error: " + ex.ToString());
				setStatusCodeOnTransaction(giftTransaction, 844060003, localContext, service);
				throw new Exception("AssignAVSValidationFieldsFromPaymentMethod() Error: " + ex.ToString());
			}
			return avsCheck;
		}

		private void UpdateTransactionReceiptStatus(Guid transactionID, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering UpdateTransactionReceiptStatus().");
			ColumnSet columnSet = new ColumnSet("msnfp_transactionid", "msnfp_taxreceiptid", "statuscode");
			Entity entity = service.Retrieve("msnfp_transaction", transactionID, columnSet);
			if (entity.Contains("msnfp_taxreceiptid") && entity.Contains("statuscode"))
			{
				localContext.TracingService.Trace("Transaction has a receipt.");
				ColumnSet columnSet2 = new ColumnSet("msnfp_receiptid", "statuscode");
				Entity entity2 = service.Retrieve("msnfp_receipt", ((EntityReference)entity["msnfp_taxreceiptid"]).Id, columnSet2);
				if (entity2.Contains("statuscode"))
				{
					localContext.TracingService.Trace("Obtained Receipt, checking Transaction status reason.");
					if (((OptionSetValue)entity["statuscode"]).Value == 844060000)
					{
						localContext.TracingService.Trace("Gift is Completed. Setting Receipt to Issued.");
						entity2["statuscode"] = new OptionSetValue(1);
					}
					else if (((OptionSetValue)entity["statuscode"]).Value == 844060003)
					{
						localContext.TracingService.Trace("Gift is set to Failed. Setting Receipt to Void (Payment Failed).");
						entity2["statuscode"] = new OptionSetValue(844060002);
					}
					localContext.TracingService.Trace("Saving receipt.");
					service.Update(entity2);
					localContext.TracingService.Trace("Saving complete.");
				}
			}
			else
			{
				localContext.TracingService.Trace("No receipt found.");
			}
			localContext.TracingService.Trace("Exiting UpdateTransactionReceiptStatus().");
		}
	}
}
