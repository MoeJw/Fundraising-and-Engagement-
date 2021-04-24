using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins.Common
{
	public static class AutoSoftCredit
	{
		public static Entity CreateSoftCredit(Entity originalDonation, ColumnSet donationColumnnsToCopy, EntityReference softCreditCustomer, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Creating a Soft Credit.");
			tracingService.Trace("Original Donation:" + originalDonation.Id.ToString());
			Entity entity = service.Retrieve(originalDonation.LogicalName, originalDonation.Id, donationColumnnsToCopy);
			entity.Id = Guid.Empty;
			entity.Attributes.Remove("msnfp_transactionid");
			entity["msnfp_typecode"] = new OptionSetValue(844060001);
			entity["msnfp_customerid"] = softCreditCustomer;
			entity["msnfp_relatedcustomerid"] = originalDonation.GetAttributeValue<EntityReference>("msnfp_customerid");
			tracingService.Trace("Soft Credit customer:" + softCreditCustomer.Id.ToString());
			entity["msnfp_parenttransactionid"] = new EntityReference(originalDonation.LogicalName, originalDonation.Id);
			tracingService.Trace("Soft Credit Parent Transaction:" + originalDonation.Id.ToString());
			entity["msnfp_relatedconstituentid"] = null;
			entity["msnfp_solicitorid"] = null;
			return entity;
		}
	}
}
