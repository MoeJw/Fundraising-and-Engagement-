using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class TributeOrMemoryUpdate : PluginBase
	{
		public TributeOrMemoryUpdate(string unsecure, string secure)
			: base(typeof(TributeOrMemoryUpdate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered TributeOrMemoryUpdate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			if (pluginExecutionContext.Depth > 1)
			{
				localContext.TracingService.Trace("Context.depth > 1 => Exiting Plugin. context.Depth: " + pluginExecutionContext.Depth);
				return;
			}
			string messageName = pluginExecutionContext.MessageName;
			Entity configurationRecordByUser = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			Entity entity = null;
			if (!pluginExecutionContext.InputParameters.Contains("Target") || !(pluginExecutionContext.InputParameters["Target"] is Entity))
			{
				return;
			}
			entity = (Entity)pluginExecutionContext.InputParameters["Target"];
			if (entity == null)
			{
				throw new InvalidPluginExecutionException("'Target' is null. Exiting plugin.");
			}
			if (entity.GetAttributeValue<EntityReference>("msnfp_duplicatetributeid") == null)
			{
				return;
			}
			EntityReference attributeValue = entity.GetAttributeValue<EntityReference>("msnfp_duplicatetributeid");
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_transaction");
			queryByAttribute.AddAttributeValue("msnfp_tributeid", attributeValue.Id);
			queryByAttribute.ColumnSet = new ColumnSet("msnfp_transactionid");
			EntityCollection entityCollection = organizationService.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				localContext.TracingService.Trace("Found " + entityCollection.Entities.Count + " Transactions for Duplicate Tribute.");
				foreach (Entity entity4 in entityCollection.Entities)
				{
					localContext.TracingService.Trace("Updating Transaction:" + entity4.Id.ToString());
					entity4["msnfp_tributeid"] = new EntityReference(entity.LogicalName, entity.Id);
					organizationService.Update(entity4);
					localContext.TracingService.Trace("Transaction Updated.");
				}
			}
			Entity entity2 = new Entity(attributeValue.LogicalName, attributeValue.Id);
			entity2["statecode"] = new OptionSetValue(1);
			entity2["statuscode"] = new OptionSetValue(2);
			organizationService.Update(entity2);
			Entity entity3 = new Entity(entity.LogicalName, entity.Id);
			entity3["msnfp_duplicatetributeid"] = null;
			organizationService.Update(entity3);
			localContext.TracingService.Trace("Cleared Duplicate Field");
		}
	}
}
