using System;
using Microsoft.Xrm.Sdk;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class AccountUpdate : PluginBase
	{
		private readonly string preImageAlias = "account";

		private readonly string postImageAlias = "account";

		private ITracingService trace = null;

		public AccountUpdate(string unsecure, string secure)
			: base(typeof(AccountUpdate))
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
			localContext.TracingService.Trace("got Account record as Primary entity.");
			trace = localContext.TracingService;
			Utilities utilities = new Utilities();
			localContext.TracingService.Trace("Address Changed.");
			if (entity.Contains("address1_line1") && entity.Contains("address1_postalcode"))
			{
				string a = (entity.Contains("address1_line1") ? ((string)entity["address1_line1"]) : string.Empty);
				string b = (entity2.Contains("address1_line1") ? ((string)entity2["address1_line1"]) : string.Empty);
				string a2 = (entity.Contains("address1_postalcode") ? ((string)entity["address1_postalcode"]) : string.Empty);
				string b2 = (entity2.Contains("address1_postalcode") ? ((string)entity2["address1_postalcode"]) : string.Empty);
				if (a != b && a2 != b2)
				{
					utilities.CreateAddressChange(organizationService, entity, entity2, 1, localContext.TracingService);
				}
			}
			if (entity.Contains("address2_line1") && entity.Contains("address2_postalcode"))
			{
				string a = (entity.Contains("address2_line1") ? ((string)entity["address2_line1"]) : string.Empty);
				string b = (entity2.Contains("address2_line1") ? ((string)entity2["address2_line1"]) : string.Empty);
				string a2 = (entity.Contains("address2_postalcode") ? ((string)entity["address2_postalcode"]) : string.Empty);
				string b2 = (entity2.Contains("address2_postalcode") ? ((string)entity2["address2_postalcode"]) : string.Empty);
				if (a != b && a2 != b2)
				{
					utilities.CreateAddressChange(organizationService, entity, entity2, 2, localContext.TracingService);
				}
			}
		}
	}
}
