using System;
using Microsoft.Xrm.Sdk;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class UpdateGiftBatchFromTransaction : PluginBase
	{
		private readonly string preImageAlias = "transaction";

		private readonly string postImageAlias = "transaction";

		public UpdateGiftBatchFromTransaction(string unsecure, string secure)
			: base(typeof(UpdateGiftBatchFromTransaction))
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
			Entity entity = ((pluginExecutionContext.PreEntityImages != null && pluginExecutionContext.PreEntityImages.Contains(preImageAlias)) ? pluginExecutionContext.PreEntityImages[preImageAlias] : null);
			Entity entity2 = ((pluginExecutionContext.PostEntityImages != null && pluginExecutionContext.PostEntityImages.Contains(postImageAlias)) ? pluginExecutionContext.PostEntityImages[postImageAlias] : null);
			localContext.TracingService.Trace("got Transaction record as Primary entity.");
			Utilities utilities = new Utilities();
			if (string.Equals(pluginExecutionContext.MessageName, "update", StringComparison.CurrentCultureIgnoreCase))
			{
				if (entity.GetAttributeValue<EntityReference>("msnfp_giftbatchid") != entity2.GetAttributeValue<EntityReference>("msnfp_giftbatchid") || entity.GetAttributeValue<OptionSetValue>("statecode") != entity2.GetAttributeValue<OptionSetValue>("statecode"))
				{
					if (entity.GetAttributeValue<EntityReference>("msnfp_giftbatchid") != null)
					{
						utilities.RecalculateGiftBatch(entity.GetAttributeValue<EntityReference>("msnfp_giftbatchid"), organizationService, localContext.TracingService);
					}
					if (entity2.GetAttributeValue<EntityReference>("msnfp_giftbatchid") != null)
					{
						utilities.RecalculateGiftBatch(entity2.GetAttributeValue<EntityReference>("msnfp_giftbatchid"), organizationService, localContext.TracingService);
					}
				}
			}
			else if (string.Equals(pluginExecutionContext.MessageName, "delete", StringComparison.CurrentCultureIgnoreCase) && entity.GetAttributeValue<EntityReference>("msnfp_giftbatchid") != null)
			{
				utilities.RecalculateGiftBatch(entity.GetAttributeValue<EntityReference>("msnfp_giftbatchid"), organizationService, localContext.TracingService);
			}
		}
	}
}
