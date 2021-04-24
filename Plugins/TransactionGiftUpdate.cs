using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class TransactionGiftUpdate : PluginBase
	{
		private readonly string preImageAlias = "transaction";

		private readonly string postImageAlias = "transaction";

		public ITracingService tracingService
		{
			get;
			set;
		}

		public TransactionGiftUpdate(string unsecure, string secure)
			: base(typeof(TransactionGiftUpdate))
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
			OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(organizationService);
			tracingService = localContext.TracingService;
			Entity entity = ((pluginExecutionContext.PreEntityImages != null && pluginExecutionContext.PreEntityImages.Contains(preImageAlias)) ? pluginExecutionContext.PreEntityImages[preImageAlias] : null);
			Entity entity2 = ((pluginExecutionContext.PostEntityImages != null && pluginExecutionContext.PostEntityImages.Contains(postImageAlias)) ? pluginExecutionContext.PostEntityImages[postImageAlias] : null);
			Entity entity3 = (pluginExecutionContext.InputParameters.Contains("Target") ? ((Entity)pluginExecutionContext.InputParameters["Target"]) : null);
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			localContext.TracingService.Trace("got Transaction record as Primary entity.");
			Utilities utilities = new Utilities();
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			Entity entity4 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity4 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			localContext.TracingService.Trace("Retrieving values.");
			if (!entity2.Contains("msnfp_configurationid") || entity2.GetAttributeValue<EntityReference>("msnfp_configurationid") == null)
			{
				throw new Exception("No configuration record found on this record (" + entity2.LogicalName + ", id:" + entity2.Id.ToString() + "). Please ensure the record has a configuration record attached.");
			}
			if (entity2.Contains("msnfp_donorcommitmentid"))
			{
				empty = (entity2.Contains("msnfp_donorcommitmentid") ? ((EntityReference)entity2["msnfp_donorcommitmentid"]).Id : Guid.Empty);
				localContext.TracingService.Trace("Transaction: newDonorCommitmentId: " + empty.ToString());
				localContext.TracingService.Trace("Transaction: Updating donor commitment");
				utilities.UpdateDonorCommitmentBalance(orgSvcContext, organizationService, (EntityReference)entity2["msnfp_donorcommitmentid"], 0);
			}
			if (entity.Contains("msnfp_donorcommitmentid"))
			{
				empty2 = (entity.Contains("msnfp_donorcommitmentid") ? ((EntityReference)entity["msnfp_donorcommitmentid"]).Id : Guid.Empty);
				localContext.TracingService.Trace("Transaction: prevDonorCommitmentId: " + empty2.ToString());
				localContext.TracingService.Trace("Transaction: Updating donor commitment");
				utilities.UpdateDonorCommitmentBalance(orgSvcContext, organizationService, (EntityReference)entity["msnfp_donorcommitmentid"], 1);
			}
			if (entity2.Contains("statuscode"))
			{
				localContext.TracingService.Trace("Transaction: statuscode found");
				if ((((OptionSetValue)entity2["statuscode"]).Value == 844060003 || ((OptionSetValue)entity2["statuscode"]).Value == 844060004) && entity.Contains("msnfp_donorcommitmentid"))
				{
					utilities.UpdateDonorCommitmentBalance(orgSvcContext, organizationService, (EntityReference)entity["msnfp_donorcommitmentid"], 1);
				}
			}
		}
	}
}
