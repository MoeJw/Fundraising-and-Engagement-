using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace Workflows
{
	public class UpdateGivingLevelOnContactOrAccount : CodeActivity
	{
		private ITracingService tracingService;

		protected override void Execute(CodeActivityContext executionContext)
		{
			if (executionContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			tracingService = executionContext.GetExtension<ITracingService>();
			tracingService.Trace("Executing UpdateGivingLevelOnContactOrAccount..");
			IWorkflowContext extension = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory extension2 = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService organizationService = extension2.CreateOrganizationService(null);
			tracingService.Trace($"Workflow context {extension == null}");
			EntityReference entityReference = new EntityReference(extension.PrimaryEntityName, extension.PrimaryEntityId);
			if (entityReference == null)
			{
				throw new InvalidPluginExecutionException("Target not found in workflwo..");
			}
			if (!(entityReference.LogicalName.ToLower() == "account") && !(entityReference.LogicalName.ToLower() == "contact"))
			{
				throw new InvalidPluginExecutionException("Valid primary entity values account / contact. Current entity : " + entityReference.LogicalName);
			}
			decimal completedTransactionsTotalAmount = GetCompletedTransactionsTotalAmount(organizationService, entityReference);
			if (completedTransactionsTotalAmount > 0m)
			{
				Entity givingLevel = GetGivingLevel(organizationService, completedTransactionsTotalAmount);
				Guid guid = CreateGivingLevelInstance(organizationService, givingLevel, entityReference);
				if (givingLevel != null && guid != Guid.Empty)
				{
					organizationService.Update(new Entity
					{
						Id = entityReference.Id,
						LogicalName = entityReference.LogicalName,
						Attributes = 
						{
							new KeyValuePair<string, object>("msnfp_givinglevelid", new EntityReference("msnfp_givinglevelinstance", guid))
						}
					});
				}
			}
		}

		private Guid CreateGivingLevelInstance(IOrganizationService service, Entity givingLevel, EntityReference targetCustomerReference)
		{
			if (givingLevel == null)
			{
				return Guid.Empty;
			}
			Entity entity = new Entity("msnfp_givinglevelinstance");
			entity["msnfp_givinglevelid"] = givingLevel.ToEntityReference();
			entity["msnfp_customerid"] = targetCustomerReference;
			entity["msnfp_primary"] = true;
			entity["msnfp_name"] = givingLevel.GetAttributeValue<string>("msnfp_identifier");
			entity["msnfp_identifier"] = givingLevel.GetAttributeValue<string>("msnfp_identifier");
			return service.Create(entity);
		}

		private Entity GetGivingLevel(IOrganizationService service, decimal? completedTransactionsAmount)
		{
			if (!completedTransactionsAmount.HasValue)
			{
				return null;
			}
			QueryExpression queryExpression = new QueryExpression("msnfp_givinglevel");
			queryExpression.ColumnSet = new ColumnSet("msnfp_givinglevelid", "msnfp_identifier");
			queryExpression.NoLock = true;
			queryExpression.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_amount_from", ConditionOperator.LessEqual, completedTransactionsAmount.Value));
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_amount_to", ConditionOperator.GreaterEqual, completedTransactionsAmount.Value));
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			return entityCollection.Entities.FirstOrDefault();
		}

		private decimal GetCompletedTransactionsTotalAmount(IOrganizationService service, EntityReference targetCustomerReference)
		{
			string text = $"<fetch no-lock='true' aggregate='true' >\r\n                                  <entity name='msnfp_transaction' >\r\n                                    <attribute name='msnfp_amount' alias='TotalAmount' aggregate='sum' />\r\n                                    <filter>\r\n                                      <condition attribute='statuscode' operator='eq' value='844060000' />\r\n                                      <condition attribute='msnfp_customerid' operator='eq' value='{targetCustomerReference.Id}' />\r\n                                    </filter>\r\n                                  </entity>\r\n                                </fetch>";
			Entity entity = service.RetrieveMultiple(new FetchExpression(text)).Entities.FirstOrDefault();
			tracingService.Trace(text ?? "");
			if (entity != null && entity.GetAttributeValue<AliasedValue>("TotalAmount") != null && entity.GetAttributeValue<AliasedValue>("TotalAmount").Value != null && (Money)entity.GetAttributeValue<AliasedValue>("TotalAmount").Value != null)
			{
				tracingService.Trace(string.Format("{0}", ((Money)entity.GetAttributeValue<AliasedValue>("TotalAmount").Value).Value));
				return ((Money)entity.GetAttributeValue<AliasedValue>("TotalAmount").Value).Value;
			}
			return 0m;
		}
	}
}
