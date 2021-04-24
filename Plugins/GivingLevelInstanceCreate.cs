using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins
{
	public class GivingLevelInstanceCreate : PluginBase
	{
		public ITracingService tracingService
		{
			get;
			set;
		}

		public GivingLevelInstanceCreate(string unsecure, string secure)
			: base(typeof(GivingLevelInstanceCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			tracingService = localContext.TracingService;
			tracingService.Trace("---------Triggered GivingLevelInstanceCreate.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			if (pluginExecutionContext.Depth > 1)
			{
				tracingService.Trace($"Context depth >1 : {pluginExecutionContext.Depth}");
			}
			else if (pluginExecutionContext.InputParameters.Contains("Target") && pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				Entity entity = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (string.Equals(pluginExecutionContext.MessageName, "update", StringComparison.InvariantCultureIgnoreCase) && pluginExecutionContext.PostEntityImages.ContainsKey("postImage"))
				{
					entity = pluginExecutionContext.PostEntityImages["postImage"];
				}
				IOrganizationService organizationService = localContext.OrganizationService;
				ProcessGivingLevelInstance(organizationService, entity.ToEntityReference(), entity.GetAttributeValue<EntityReference>("msnfp_customerid"), entity.GetAttributeValue<bool>("msnfp_primary"));
			}
		}

		private void ProcessGivingLevelInstance(IOrganizationService service, EntityReference givingLevelInstanceRef, EntityReference customerReference, bool isPrimary)
		{
			if (customerReference != null && isPrimary)
			{
				Entity entity = service.Retrieve(customerReference.LogicalName, customerReference.Id, new ColumnSet("msnfp_givinglevelid"));
				EntityCollection entityCollection = service.RetrieveMultiple(new QueryExpression(givingLevelInstanceRef.LogicalName)
				{
					ColumnSet = new ColumnSet("msnfp_givinglevelinstanceid"),
					Criteria = 
					{
						Conditions = 
						{
							new ConditionExpression("msnfp_customerid", ConditionOperator.Equal, customerReference.Id),
							new ConditionExpression("msnfp_primary", ConditionOperator.Equal, true),
							new ConditionExpression("msnfp_givinglevelinstanceid", ConditionOperator.NotEqual, givingLevelInstanceRef.Id)
						}
					}
				});
				if (entity.GetAttributeValue<EntityReference>("msnfp_givinglevelid") == null || entity.GetAttributeValue<EntityReference>("msnfp_givinglevelid").Id != givingLevelInstanceRef.Id)
				{
					service.Update(new Entity(customerReference.LogicalName, customerReference.Id)
					{
						Attributes = 
						{
							new KeyValuePair<string, object>("msnfp_givinglevelid", givingLevelInstanceRef)
						}
					});
				}
				entityCollection.Entities.ToList().ForEach(delegate(Entity gi)
				{
					gi["msnfp_primary"] = false;
					service.Update(gi);
				});
			}
			else
			{
				tracingService.Trace("No processing done as either it is not primary or no customer on it");
			}
		}
	}
}
