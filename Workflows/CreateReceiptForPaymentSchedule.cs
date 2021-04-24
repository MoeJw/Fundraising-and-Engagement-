using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Plugins.PaymentProcesses;

namespace Workflows
{
	public class CreateReceiptForPaymentSchedule : CodeActivity
	{
		[ReferenceTarget("msnfp_receipt")]
		[Output("Receipt")]
		public OutArgument<EntityReference> Receipt
		{
			get;
			set;
		}

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
				extension.Trace("Entering CreateReceiptForPaymentSchedule");
				OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
				Guid primaryEntityId = extension2.PrimaryEntityId;
				Entity entity = organizationService.Retrieve(extension2.PrimaryEntityName, extension2.PrimaryEntityId, new ColumnSet("msnfp_configurationid", "msnfp_receiptpreferencecode", "msnfp_taxreceiptid", "msnfp_lastpaymentdate", "transactioncurrencyid", "msnfp_customerid", "ownerid"));
				EntityReference attributeValue = entity.GetAttributeValue<EntityReference>("msnfp_configurationid");
				EntityReference previousReceipt = GetPreviousReceipt(entity, extension);
				int year = DateTime.Now.Year;
				List<Entity> matchingTransactions = GetMatchingTransactions(primaryEntityId, year, extension, organizationService);
				if (matchingTransactions.Count <= 0)
				{
					extension.Trace("No Matching Transactions Found.");
					if (previousReceipt != null)
					{
						extension.Trace("Returning Previous Receipt");
						Receipt.Set(executionContext, previousReceipt);
					}
					extension.Trace("Exiting");
					return;
				}
				DateTime? lastPaymentDate = GetLastPaymentDate(entity, extension);
				int matchingTransactionCount = GetMatchingTransactionCount(matchingTransactions, extension);
				decimal matchingTransactionsMoneySum = GetMatchingTransactionsMoneySum(matchingTransactions, "msnfp_amount", extension);
				decimal matchingTransactionsMoneySum2 = GetMatchingTransactionsMoneySum(matchingTransactions, "msnfp_amount_receipted", extension);
				decimal matchingTransactionsMoneySum3 = GetMatchingTransactionsMoneySum(matchingTransactions, "msnfp_amount_membership", extension);
				decimal matchingTransactionsMoneySum4 = GetMatchingTransactionsMoneySum(matchingTransactions, "msnfp_amount_nonreceiptable", extension);
				Entity receiptStack = GetReceiptStack(attributeValue.Id, year, extension, organizationService);
				Guid guid = CreateNewReceiptRecord(receiptStack, matchingTransactionsMoneySum2, matchingTransactionsMoneySum3, matchingTransactionsMoneySum4, matchingTransactionCount, matchingTransactionsMoneySum, entity, lastPaymentDate, extension, organizationService);
				AssociateReceiptToPaymentSchedule(guid, entity, extension, organizationService);
				AssociateReceiptToMatchingTransactions(guid, matchingTransactions, extension, organizationService);
				extension.Trace("Done.");
				Receipt.Set(executionContext, new EntityReference("msnfp_receipt", guid));
			}
			catch (FaultException<OrganizationServiceFault> ex)
			{
				extension.Trace("Workflow Exception: {0}", ex.ToString());
				throw;
			}
		}

		private static Guid CreateNewReceiptRecord(Entity receiptStack, decimal totalAmountReceipted, decimal totalAmountMembership, decimal totalAmountNonReceiptable, int matchingTransactionCount, decimal totalAmount, Entity paymentSchedule, DateTime? lastDonationDate, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Generating new Receipt record");
			Entity entity = new Entity("msnfp_receipt");
			entity["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", receiptStack.Id);
			entity["msnfp_amount_receipted"] = new Money(totalAmountReceipted);
			entity["msnfp_amount_nonreceiptable"] = new Money(totalAmountMembership + totalAmountNonReceiptable);
			entity["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
			entity["msnfp_receiptissuedate"] = DateTime.Now;
			entity["msnfp_transactioncount"] = matchingTransactionCount;
			entity["msnfp_amount"] = new Money(totalAmount);
			entity["transactioncurrencyid"] = paymentSchedule.GetAttributeValue<EntityReference>("transactioncurrencyid");
			if (paymentSchedule.GetAttributeValue<EntityReference>("msnfp_customerid") != null)
			{
				entity["msnfp_customerid"] = paymentSchedule.GetAttributeValue<EntityReference>("msnfp_customerid");
			}
			entity["msnfp_lastdonationdate"] = lastDonationDate;
			entity["ownerid"] = paymentSchedule.GetAttributeValue<EntityReference>("ownerid");
			entity["statuscode"] = new OptionSetValue(1);
			entity["msnfp_generatedorprinted"] = Convert.ToDouble(1);
			tracingService.Trace("Saving new Receipt.");
			Guid guid = service.Create(entity);
			Guid guid2 = guid;
			tracingService.Trace("Receipt Created. Id:" + guid2.ToString());
			return guid;
		}

		private static EntityReference GetPreviousReceipt(Entity paymentSchedule, ITracingService tracingService)
		{
			tracingService.Trace("Looking for Previous Receipt (if any)");
			EntityReference attributeValue = paymentSchedule.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
			if (attributeValue != null)
			{
				tracingService.Trace("Previous Receipt Found. Id:" + attributeValue.Id.ToString());
			}
			else
			{
				tracingService.Trace("No Previous Receipt Found.");
			}
			return attributeValue;
		}

		private static List<Entity> GetMatchingTransactions(Guid paymentScheduleId, int currentYear, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Getting list of matching Transactions");
			List<Entity> list = new List<Entity>();
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_transaction");
			queryByAttribute.ColumnSet = new ColumnSet("msnfp_bookdate", "msnfp_amount", "msnfp_amount_receipted", "msnfp_amount_membership", "msnfp_amount_nonreceiptable");
			queryByAttribute.AddAttributeValue("msnfp_transaction_paymentscheduleid", paymentScheduleId);
			queryByAttribute.AddAttributeValue("statuscode", 844060000);
			queryByAttribute.AddAttributeValue("msnfp_taxreceiptid", null);
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				tracingService.Trace("Found " + entityCollection.Entities.Count + " Transactions. (Possible matches)");
				foreach (Entity entity in entityCollection.Entities)
				{
					tracingService.Trace("Transaction Id:" + entity.Id.ToString() + ", Book Date:" + entity.GetAttributeValue<DateTime>("msnfp_bookdate").ToString());
					if (entity.GetAttributeValue<DateTime>("msnfp_bookdate").Year == currentYear)
					{
						tracingService.Trace("Adding Transaction Id:" + entity.Id.ToString() + " to the list of Matching Transactions");
						list.Add(entity);
					}
				}
			}
			tracingService.Trace("Found " + list.Count + " Matching Transactions.");
			return list;
		}

		private static int GetMatchingTransactionCount(List<Entity> matchingTransactions, ITracingService tracingService)
		{
			return matchingTransactions.Count;
		}

		private DateTime? GetLastPaymentDate(Entity paymentSchedule, ITracingService tracingService)
		{
			DateTime? result = null;
			if (paymentSchedule.GetAttributeValue<DateTime>("msnfp_lastpaymentdate") != DateTime.MinValue)
			{
				result = paymentSchedule.GetAttributeValue<DateTime>("msnfp_lastpaymentdate");
			}
			return result;
		}

		private static decimal GetMatchingTransactionsMoneySum(List<Entity> matchingTransactions, string moneyFieldToSum, ITracingService tracingService)
		{
			decimal result = matchingTransactions.Sum((Entity t) => (t.GetAttributeValue<Money>(moneyFieldToSum) != null) ? t.GetAttributeValue<Money>(moneyFieldToSum).Value : 0m);
			tracingService.Trace("Sum of " + moneyFieldToSum + " field across all matching Transactions:" + result);
			return result;
		}

		private static Entity GetReceiptStack(Guid configurationId, int currentYear, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Getting Receipt Stack for year:" + currentYear);
			Entity entity = null;
			int optionsSetValueForLabel = Utilities.GetOptionsSetValueForLabel(service, "msnfp_receiptstack", "msnfp_receiptyear", currentYear.ToString());
			tracingService.Trace("receiptYearVal:" + optionsSetValueForLabel);
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_receiptstack");
			queryByAttribute.ColumnSet = new ColumnSet("msnfp_configurationid", "msnfp_receiptyear", "statecode");
			queryByAttribute.AddAttributeValue("msnfp_configurationid", configurationId);
			queryByAttribute.AddAttributeValue("msnfp_receiptyear", optionsSetValueForLabel);
			queryByAttribute.AddAttributeValue("statecode", 0);
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				entity = entityCollection.Entities[0];
			}
			if (entity != null)
			{
				tracingService.Trace("Found Receipt Stack Id:" + entity.Id.ToString());
			}
			else
			{
				tracingService.Trace("No Receipt Stack found.");
			}
			return entity;
		}

		private string GetReceiptIdentifier(Entity receiptStack, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Entering SetReceiptIdentifier().");
			string result = string.Empty;
			string empty = string.Empty;
			double num = 0.0;
			int num2 = 0;
			if (receiptStack != null)
			{
				tracingService.Trace("Obtaining prefix, current range and number range from Receipt Stack.");
				empty = receiptStack.GetAttributeValue<string>("msnfp_prefix");
				num = receiptStack.GetAttributeValue<double>("msnfp_currentrange");
				num2 = ((receiptStack.GetAttributeValue<OptionSetValue>("msnfp_numberrange") != null) ? receiptStack.GetAttributeValue<OptionSetValue>("msnfp_numberrange").Value : 0);
				switch (num2)
				{
				case 844060006:
					tracingService.Trace("Number range : 6 digit");
					result = empty + (num + 1.0).ToString().PadLeft(6, '0');
					break;
				case 844060008:
					tracingService.Trace("Number range : 8 digit");
					result = empty + (num + 1.0).ToString().PadLeft(8, '0');
					break;
				case 844060010:
					tracingService.Trace("Number range : 10 digit");
					result = empty + (num + 1.0).ToString().PadLeft(10, '0');
					break;
				default:
					tracingService.Trace("Receipt number range unknown. msnfp_numberrange: " + num2);
					break;
				}
				tracingService.Trace("Now update the receipt stacks current number by 1.");
				Entity entity = new Entity(receiptStack.LogicalName, receiptStack.Id);
				entity["msnfp_currentrange"] = num + 1.0;
				service.Update(entity);
				tracingService.Trace("Updated Receipt Stack current range to: " + (num + 1.0));
			}
			else
			{
				tracingService.Trace("No receipt stack found.");
			}
			tracingService.Trace("Exiting SetReceiptIdentifier().");
			return result;
		}

		private static void AssociateReceiptToMatchingTransactions(Guid receiptId, List<Entity> matchingTransactions, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Associating new Receipt to " + matchingTransactions.Count + " Matching Transactions");
			foreach (Entity matchingTransaction in matchingTransactions)
			{
				tracingService.Trace("Updating Transaction Id:" + matchingTransaction.Id.ToString());
				Entity entity = new Entity(matchingTransaction.LogicalName, matchingTransaction.Id);
				entity["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", receiptId);
				service.Update(entity);
			}
			tracingService.Trace("Done Associating new Receipt to Matching Transactions");
		}

		private static void AssociateReceiptToPaymentSchedule(Guid receiptId, Entity paymentSchedule, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Associating new Receipt to Payment Schedule");
			Entity entity = new Entity(paymentSchedule.LogicalName, paymentSchedule.Id);
			entity["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", receiptId);
			service.Update(entity);
			tracingService.Trace("Done Associating new Receipt to Payment Schedule");
		}
	}
}
