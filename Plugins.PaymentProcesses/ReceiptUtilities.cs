using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins.PaymentProcesses
{
	public class ReceiptUtilities
	{
		public static void UpdateReceipt(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Updating associated receipt (if necessary).");
			tracingService.Trace("Preimages:" + context.PreEntityImages.Count);
			tracingService.Trace("Postimages:" + context.PostEntityImages.Count);
			Entity entity = null;
			EntityReference entityReference = null;
			string messageName = context.MessageName;
			Entity entity2 = null;
			if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
			{
				tracingService.Trace("Target Incoming Record is an Entity (needed for Create and some Updates).");
				entity2 = (Entity)context.InputParameters["Target"];
			}
			Entity entity3 = null;
			if (context.PreEntityImages.Contains("preImage"))
			{
				entity3 = context.PreEntityImages["preImage"];
				tracingService.Trace("Found preImage");
				if (entity3.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") != null)
				{
					tracingService.Trace("preImage contains tax receipt id:" + entity3.GetAttributeValue<EntityReference>("msnfp_taxreceiptid").Id.ToString());
				}
			}
			Entity entity4 = null;
			if (context.PostEntityImages.Contains("postImage"))
			{
				entity4 = context.PostEntityImages["postImage"];
				tracingService.Trace("Found postImage");
				if (entity4.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") != null)
				{
					tracingService.Trace("postImage contains tax receipt id:" + entity4.GetAttributeValue<EntityReference>("msnfp_taxreceiptid").Id.ToString());
				}
			}
			if (messageName == "Create" && entity2.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") != null)
			{
				tracingService.Trace("Updating Receipt on on Create");
				entityReference = entity2.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
				entity = UpdateReceiptFields(entityReference, service, tracingService, entity2, includeRelatedRecordInCalculations: true);
			}
			else if (messageName == "Update")
			{
				tracingService.Trace("Checking on record Update...");
				if (entity2.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") != null)
				{
					tracingService.Trace("Updating Receipt after receipt was added to record");
					entityReference = entity2.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
					entity = UpdateReceiptFields(entityReference, service, tracingService, entity2, includeRelatedRecordInCalculations: true);
				}
				else if (entity2.GetAttributeValue<OptionSetValue>("statecode") != null && entity2.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
				{
					tracingService.Trace("Updating Receipt after record was activated");
					entityReference = entity4.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
					entity = UpdateReceiptFields(entityReference, service, tracingService, entity4, includeRelatedRecordInCalculations: true);
				}
				else if (entity2.GetAttributeValue<OptionSetValue>("statecode") != null && entity2.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
				{
					tracingService.Trace("Updating Receipt after record was inactivated");
					entityReference = entity4.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
					entity = UpdateReceiptFields(entityReference, service, tracingService, entity4, includeRelatedRecordInCalculations: false);
				}
				else if (entity3.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") != null && entity4.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") == null)
				{
					tracingService.Trace("Updating Receipt after receipt was removed from record");
					entityReference = entity3.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
					entity = UpdateReceiptFields(entityReference, service, tracingService, entity3, includeRelatedRecordInCalculations: false);
				}
				else
				{
					tracingService.Trace("No changes needed on this update.");
				}
			}
			else if (messageName == "Delete")
			{
				tracingService.Trace("Updating Receipt after record was deleted");
				entity3 = context.PreEntityImages["preImage"];
				if (entity3.GetAttributeValue<EntityReference>("msnfp_taxreceiptid") != null)
				{
					entityReference = entity3.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
					entity = UpdateReceiptFields(entityReference, service, tracingService, entity3, includeRelatedRecordInCalculations: false);
				}
			}
		}

		private static Entity UpdateReceiptFields(EntityReference incomingReceiptRef, IOrganizationService service, ITracingService tracingService, Entity relatedRecord, bool includeRelatedRecordInCalculations)
		{
			if (incomingReceiptRef == null)
			{
				return null;
			}
			Entity entity = service.Retrieve(incomingReceiptRef.LogicalName, incomingReceiptRef.Id, new ColumnSet("msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount", "msnfp_eventcount", "msnfp_transactioncount"));
			tracingService.Trace("Updating receipt:" + entity.Id.ToString());
			Entity entity2 = null;
			if (!includeRelatedRecordInCalculations)
			{
				entity2 = relatedRecord;
				tracingService.Trace("Record to Exclude from Calculations:");
				tracingService.Trace(entity2.Id.ToString() + ", " + entity2.LogicalName);
			}
			Entity entity3 = new Entity(entity.LogicalName, entity.Id);
			bool flag = false;
			List<Entity> relatedTransactions = GetRelatedTransactions(entity.Id, service, tracingService, entity2);
			List<Entity> relatedEventPackages = GetRelatedEventPackages(entity.Id, service, tracingService, entity2);
			tracingService.Trace("updating msnfp_amount_receipted");
			Money money = new Money(GetSumOfMoneyFields(relatedTransactions, "msnfp_amount_receipted", tracingService) + GetSumOfMoneyFields(relatedEventPackages, "msnfp_amount_receipted", tracingService));
			tracingService.Trace("New Value:" + money.Value);
			entity3["msnfp_amount_receipted"] = money;
			flag = true;
			tracingService.Trace("updating msnfp_amount_nonreceiptable");
			Money money2 = new Money(GetSumOfMoneyFields(relatedTransactions, "msnfp_amount_membership", tracingService) + GetSumOfMoneyFields(relatedTransactions, "msnfp_amount_nonreceiptable", tracingService) + GetSumOfMoneyFields(relatedEventPackages, "msnfp_amount_nonreceiptable", tracingService));
			tracingService.Trace("New Value:" + money2.Value);
			entity3["msnfp_amount_nonreceiptable"] = money2;
			flag = true;
			tracingService.Trace("updating msnfp_amount");
			Money money3 = new Money(GetSumOfMoneyFields(relatedTransactions, "msnfp_amount", tracingService) + GetSumOfMoneyFields(relatedEventPackages, "msnfp_amount", tracingService));
			tracingService.Trace("New Value:" + money3.Value);
			entity3["msnfp_amount"] = money3;
			flag = true;
			tracingService.Trace("updating msnfp_eventcount");
			tracingService.Trace("New Value:" + relatedEventPackages.Count);
			entity3["msnfp_eventcount"] = relatedEventPackages.Count;
			if (true)
			{
				service.Update(entity3);
				return entity3;
			}
			return null;
		}

		private static Entity UpdateMoneyFieldsOnRemoval(EntityReference incomingReceiptRef, IOrganizationService service, ITracingService tracingService)
		{
			throw new NotImplementedException();
		}

		private static Entity CopyUpdatedMoneyFieldsToTargetRecord(Entity receiptWithUpdatedMoneyFields, Entity targetRecord, ITracingService tracingService)
		{
			tracingService.Trace("Copying updated money fields to target record");
			List<string> list = new List<string>
			{
				"msnfp_amount_receipted",
				"msnfp_amount_nonreceiptable",
				"msnfp_amount"
			};
			foreach (string item in list)
			{
				if (MoneyFieldHasNonZeroValue(receiptWithUpdatedMoneyFields, item))
				{
					tracingService.Trace("Copying " + item + " to incomingTargetRecord");
					tracingService.Trace("Value:" + receiptWithUpdatedMoneyFields.GetAttributeValue<Money>(item).Value);
					targetRecord[item] = receiptWithUpdatedMoneyFields.GetAttributeValue<Money>(item);
				}
			}
			if (receiptWithUpdatedMoneyFields.Contains("msnfp_eventcount") && receiptWithUpdatedMoneyFields.GetAttributeValue<int>("msnfp_eventcount") != 0)
			{
				tracingService.Trace("Copying msnfp_eventcount to incomingTargetRecord");
				targetRecord["msnfp_eventcount"] = receiptWithUpdatedMoneyFields.GetAttributeValue<int>("msnfp_eventcount");
			}
			return targetRecord;
		}

		private static List<Entity> GetRelatedTransactions(Guid receiptId, IOrganizationService service, ITracingService tracingService, Entity recordToExclude = null)
		{
			tracingService.Trace("Looking for related Transactions");
			List<Entity> list = new List<Entity>();
			QueryExpression queryExpression = new QueryExpression("msnfp_transaction");
			queryExpression.ColumnSet = new ColumnSet("msnfp_amount_receipted", "msnfp_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_amount");
			queryExpression.Criteria.AddCondition("msnfp_taxreceiptid", ConditionOperator.Equal, receiptId);
			queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
			if (recordToExclude != null && recordToExclude.LogicalName == "msnfp_transaction")
			{
				queryExpression.Criteria.AddCondition("msnfp_transactionid", ConditionOperator.NotEqual, recordToExclude.Id);
			}
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			if (entityCollection != null)
			{
				list = entityCollection.Entities.ToList();
				tracingService.Trace("Found " + list.Count + " related Transactions");
			}
			return list;
		}

		private static List<Entity> GetRelatedEventPackages(Guid receiptId, IOrganizationService service, ITracingService tracingService, Entity recordToExclude = null)
		{
			tracingService.Trace("Looking for related Event Packages");
			List<Entity> list = new List<Entity>();
			QueryExpression queryExpression = new QueryExpression("msnfp_eventpackage");
			queryExpression.ColumnSet = new ColumnSet("msnfp_amount_receipted", "msnfp_amount_nonreceiptable", "msnfp_amount");
			queryExpression.Criteria.AddCondition("msnfp_taxreceiptid", ConditionOperator.Equal, receiptId);
			queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
			if (recordToExclude != null && recordToExclude.LogicalName == "msnfp_eventpackage")
			{
				queryExpression.Criteria.AddCondition("msnfp_eventpackageid", ConditionOperator.NotEqual, recordToExclude.Id);
			}
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			if (entityCollection != null)
			{
				list = entityCollection.Entities.ToList();
				tracingService.Trace("Found " + list.Count + " related Event Packages");
			}
			return list;
		}

		private static bool MoneyFieldHasNonZeroValue(Entity entity, string moneyFieldName)
		{
			bool result = false;
			if (entity.Contains(moneyFieldName))
			{
				Money attributeValue = entity.GetAttributeValue<Money>(moneyFieldName);
				if (attributeValue != null && attributeValue.Value != 0m)
				{
					result = true;
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		private static bool MoneyFieldIsNullOrZero(Entity entity, string moneyFieldName)
		{
			return !MoneyFieldHasNonZeroValue(entity, moneyFieldName);
		}

		private static decimal GetSumOfMoneyFields(List<Entity> records, string moneyFieldToSum, ITracingService tracingService)
		{
			string text = ((records.Count > 0) ? records.First().LogicalName : "");
			decimal result = records.Sum((Entity t) => (t.GetAttributeValue<Money>(moneyFieldToSum) != null) ? t.GetAttributeValue<Money>(moneyFieldToSum).Value : 0m);
			tracingService.Trace("Sum of " + moneyFieldToSum + " field across all matching " + text + ":" + result);
			return result;
		}
	}
}
