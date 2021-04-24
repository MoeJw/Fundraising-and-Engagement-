using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Plugins.Common;

namespace Plugins
{
	public class RegistrationSecurityStatusCreate : CodeActivity
	{
		protected override void Execute(CodeActivityContext executionContext)
		{
			ITracingService extension = executionContext.GetExtension<ITracingService>();
			extension.Trace("Entered RegistrationSecurityStatusCreate Activity");
			IWorkflowContext extension2 = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory extension3 = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService organizationService = extension3.CreateOrganizationService(null);
			Guid primaryEntityId = extension2.PrimaryEntityId;
			string primaryEntityName = extension2.PrimaryEntityName;
			Entity entity = new Entity(primaryEntityName, primaryEntityId);
			string text2 = (string)(entity["msnfp_securitycode"] = (entity["msnfp_name"] = Utilities.RandomString(4)));
			organizationService.Update(entity);
		}
	}
}
