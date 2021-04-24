using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins
{
	public class DesignatedCreditCreate : PluginBase
	{
		public DesignatedCreditCreate(string unsecure, string secure)
			: base(typeof(DesignatedCreditCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered DesignatedCreditCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			string messageName = pluginExecutionContext.MessageName;
			if (pluginExecutionContext.Depth > 1)
			{
				localContext.TracingService.Trace("Context.depth > 1. Exiting Plugin.");
				return;
			}
			localContext.TracingService.Trace("Context.depth 0 = " + pluginExecutionContext.Depth);
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				Entity entity = (Entity)pluginExecutionContext.InputParameters["Target"];
				Entity entity2 = new Entity(entity.LogicalName);
				entity2.Id = entity.Id;
				EntityReference attributeValue = entity.GetAttributeValue<EntityReference>("msnfp_designatedcredit_designationid");
				if (attributeValue != null)
				{
					Entity entity3 = organizationService.Retrieve(attributeValue.LogicalName, attributeValue.Id, new ColumnSet("msnfp_name"));
					string attributeValue2 = entity3.GetAttributeValue<string>("msnfp_name");
					string text2 = (string)(entity2["msnfp_name"] = attributeValue2 + "-$" + entity.GetAttributeValue<Money>("msnfp_amount").Value);
					organizationService.Update(entity2);
				}
			}
		}
	}
}
