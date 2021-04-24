using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class ContactUpdate : PluginBase
	{
		private readonly string preImageAlias = "contact";

		private readonly string postImageAlias = "contact";

		public ContactUpdate(string unsecure, string secure)
			: base(typeof(ContactUpdate))
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
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			Entity entity = ((pluginExecutionContext.PreEntityImages != null && pluginExecutionContext.PreEntityImages.Contains(preImageAlias)) ? pluginExecutionContext.PreEntityImages[preImageAlias] : null);
			Entity entity2 = ((pluginExecutionContext.PostEntityImages != null && pluginExecutionContext.PostEntityImages.Contains(postImageAlias)) ? pluginExecutionContext.PostEntityImages[postImageAlias] : null);
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			localContext.TracingService.Trace("got Contact record as Primary entity.");
			Utilities utilities = new Utilities();
			localContext.TracingService.Trace("Address Changed.");
			Entity entity3 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity3 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			Entity configurationRecordByUser = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
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
			if (entity.Contains("address3_line1") && entity.Contains("address3_postalcode"))
			{
				string a = (entity.Contains("address3_line1") ? ((string)entity["address3_line1"]) : string.Empty);
				string b = (entity2.Contains("address3_line1") ? ((string)entity2["address3_line1"]) : string.Empty);
				string a2 = (entity.Contains("address3_postalcode") ? ((string)entity["address3_postalcode"]) : string.Empty);
				string b2 = (entity2.Contains("address3_postalcode") ? ((string)entity2["address3_postalcode"]) : string.Empty);
				if (a != b && a2 != b2)
				{
					utilities.CreateAddressChange(organizationService, entity, entity2, 3, localContext.TracingService);
				}
			}
		}
	}
}
