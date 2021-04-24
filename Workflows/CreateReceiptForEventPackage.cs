using System;
using System.Activities;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Plugins.PaymentProcesses;

namespace Workflows
{
	public class CreateReceiptForEventPackage : CodeActivity
	{
		[ReferenceTarget("msnfp_receipt")]
		[Output("Receipt")]
		public OutArgument<EntityReference> Receipt
		{
			get;
			set;
		}

		protected override void Execute(CodeActivityContext executionContext)
		{
			if (executionContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			ITracingService extension = executionContext.GetExtension<ITracingService>();
			IWorkflowContext extension2 = executionContext.GetExtension<IWorkflowContext>();
			IOrganizationServiceFactory extension3 = executionContext.GetExtension<IOrganizationServiceFactory>();
			IOrganizationService organizationService = extension3.CreateOrganizationService(null);
			try
			{
				extension.Trace("Entering CreateReceiptForEventPackage");
				OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
				Guid primaryEntityId = extension2.PrimaryEntityId;
				Entity entity = organizationService.Retrieve(extension2.PrimaryEntityName, extension2.PrimaryEntityId, new ColumnSet("msnfp_amount", "msnfp_amount_receipted", "msnfp_ref_amount_nonreceiptable", "msnfp_configurationid", "msnfp_taxreceiptid", "transactioncurrencyid", "msnfp_customerid", "ownerid", "statuscode"));
				EntityReference configuration = GetConfiguration(entity, extension2, organizationService, extension);
				EntityReference previousReceipt = GetPreviousReceipt(entity, extension);
				int year = DateTime.Now.Year;
				extension.Trace("currentYear:" + year);
				decimal totalAmount = ((entity.GetAttributeValue<Money>("msnfp_amount") != null) ? entity.GetAttributeValue<Money>("msnfp_amount").Value : 0m);
				extension.Trace("totalAmount:" + totalAmount);
				decimal totalAmountReceipted = ((entity.GetAttributeValue<Money>("msnfp_amount_receipted") != null) ? entity.GetAttributeValue<Money>("msnfp_amount_receipted").Value : 0m);
				extension.Trace("totalAmountReceipted:" + totalAmountReceipted);
				decimal totalAmountNonReceiptable = ((entity.GetAttributeValue<Money>("msnfp_ref_amount_nonreceiptable") != null) ? entity.GetAttributeValue<Money>("msnfp_ref_amount_nonreceiptable").Value : 0m);
				extension.Trace("totalAmountNonReceiptable:" + totalAmountNonReceiptable);
				Entity receiptStack = GetReceiptStack(configuration.Id, year, extension, organizationService);
				Guid guid = CreateNewReceiptRecord(receiptStack, totalAmountReceipted, 0m, totalAmountNonReceiptable, 0, totalAmount, entity, null, previousReceipt, extension, organizationService);
				AssociateReceiptToEventPackage(guid, entity, extension, organizationService);
				extension.Trace("Done.");
				Receipt.Set(executionContext, new EntityReference("msnfp_receipt", guid));
			}
			catch (FaultException<OrganizationServiceFault> ex)
			{
				extension.Trace("Workflow Exception: {0}", ex.ToString());
				throw;
			}
		}

		private static EntityReference GetConfiguration(Entity EventPackage, IWorkflowContext _context, IOrganizationService service, ITracingService tracingService)
		{
			EntityReference entityReference = null;
			tracingService.Trace("Getting Configuration from Event Package.");
			entityReference = EventPackage.GetAttributeValue<EntityReference>("msnfp_configurationid");
			if (entityReference == null)
			{
				tracingService.Trace("Getting Configuration from User.");
				Guid initiatingUserId = _context.InitiatingUserId;
				Entity entity = service.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
				if (entity == null)
				{
					throw new Exception("User Not Found. Aborting");
				}
				if (entity.GetAttributeValue<EntityReference>("msnfp_configurationid") == null)
				{
					throw new Exception("User does not have a Configuration record. Aborting.");
				}
				entityReference = entity.GetAttributeValue<EntityReference>("msnfp_configurationid");
			}
			tracingService.Trace("Got Configuration id:" + ((entityReference != null) ? entityReference.Id.ToString() : "nothing found."));
			return entityReference;
		}

		private static Guid CreateNewReceiptRecord(Entity receiptStack, decimal totalAmountReceipted, decimal totalAmountMembership, decimal totalAmountNonReceiptable, int matchingTransactionCount, decimal totalAmount, Entity eventPackage, DateTime? lastDonationDate, EntityReference previousReceiptRef, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Generating new Receipt record");
			Entity entity = new Entity("msnfp_receipt");
			entity["msnfp_receiptstackid"] = new EntityReference("msnfp_receiptstack", receiptStack.Id);
			entity["msnfp_amount_receipted"] = new Money(totalAmountReceipted);
			entity["msnfp_amount_nonreceiptable"] = new Money(totalAmountMembership + totalAmountNonReceiptable);
			entity["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
			entity["msnfp_receiptissuedate"] = DateTime.Now;
			entity["msnfp_transactioncount"] = matchingTransactionCount;
			entity["msnfp_amount"] = new Money(totalAmount);
			entity["transactioncurrencyid"] = eventPackage.GetAttributeValue<EntityReference>("transactioncurrencyid");
			if (eventPackage.GetAttributeValue<EntityReference>("msnfp_customerid") != null)
			{
				entity["msnfp_customerid"] = eventPackage.GetAttributeValue<EntityReference>("msnfp_customerid");
			}
			entity["msnfp_lastdonationdate"] = lastDonationDate;
			entity["ownerid"] = eventPackage.GetAttributeValue<EntityReference>("ownerid");
			OptionSetValue attributeValue = eventPackage.GetAttributeValue<OptionSetValue>("statuscode");
			if (attributeValue.Value == 844060000)
			{
				tracingService.Trace("Event Package is Completed. Setting Receipt Status to Issued.");
				entity["statuscode"] = new OptionSetValue(1);
			}
			else
			{
				tracingService.Trace("Event Package is NOT Completed. Setting Receipt Status to Void (Payment Failed).");
				entity["statuscode"] = new OptionSetValue(844060002);
			}
			entity["msnfp_receiptgeneration"] = new OptionSetValue(844060000);
			entity["msnfp_eventcount"] = 1;
			Guid guid;
			if (previousReceiptRef != null)
			{
				Entity entity2 = service.Retrieve(previousReceiptRef.LogicalName, previousReceiptRef.Id, new ColumnSet("msnfp_generatedorprinted"));
				entity["msnfp_generatedorprinted"] = entity2.GetAttributeValue<double>("msnfp_generatedorprinted") + Convert.ToDouble(1);
				tracingService.Trace("Updating Previous Receipt.");
				service.Update(entity);
				tracingService.Trace("Receipt Updated. Id:" + previousReceiptRef.Id.ToString());
				guid = previousReceiptRef.Id;
			}
			else
			{
				entity["msnfp_generatedorprinted"] = Convert.ToDouble(1);
				tracingService.Trace("Saving new Receipt.");
				guid = service.Create(entity);
				Guid guid2 = guid;
				tracingService.Trace("Receipt Created. Id:" + guid2.ToString());
			}
			return guid;
		}

		private static EntityReference GetPreviousReceipt(Entity eventPackage, ITracingService tracingService)
		{
			tracingService.Trace("Looking for Previous Receipt (if any)");
			EntityReference attributeValue = eventPackage.GetAttributeValue<EntityReference>("msnfp_taxreceiptid");
			if (attributeValue != null)
			{
				tracingService.Trace("Previous Receipt Found. Id:" + attributeValue.Id.ToString());
			}
			else
			{
				tracingService.Trace("No Previous Receipt Found.");
			}
			return attributeValue;
		}

		private static Entity GetReceiptStack(Guid configurationId, int currentYear, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Getting Receipt Stack for year:" + currentYear);
			Entity entity = null;
			int optionsSetValueForLabel = Utilities.GetOptionsSetValueForLabel(service, "msnfp_receiptstack", "msnfp_receiptyear", currentYear.ToString());
			tracingService.Trace("receiptYearVal:" + optionsSetValueForLabel);
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_receiptstack");
			queryByAttribute.ColumnSet = new ColumnSet("msnfp_configurationid", "msnfp_receiptyear", "statecode");
			queryByAttribute.AddAttributeValue("msnfp_configurationid", configurationId);
			queryByAttribute.AddAttributeValue("msnfp_receiptyear", optionsSetValueForLabel);
			queryByAttribute.AddAttributeValue("statecode", 0);
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				entity = entityCollection.Entities[0];
			}
			if (entity != null)
			{
				tracingService.Trace("Found Receipt Stack Id:" + entity.Id.ToString());
			}
			else
			{
				tracingService.Trace("No Receipt Stack found.");
			}
			return entity;
		}

		private string GetReceiptIdentifier(Entity receiptStack, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Entering GetReceiptIdentifier().");
			string result = string.Empty;
			string empty = string.Empty;
			double num = 0.0;
			int num2 = 0;
			if (receiptStack != null)
			{
				tracingService.Trace("Obtaining prefix, current range and number range from Receipt Stack.");
				empty = receiptStack.GetAttributeValue<string>("msnfp_prefix");
				num = receiptStack.GetAttributeValue<double>("msnfp_currentrange");
				num2 = ((receiptStack.GetAttributeValue<OptionSetValue>("msnfp_numberrange") != null) ? receiptStack.GetAttributeValue<OptionSetValue>("msnfp_numberrange").Value : 0);
				switch (num2)
				{
				case 844060006:
					tracingService.Trace("Number range : 6 digit");
					result = empty + (num + 1.0).ToString().PadLeft(6, '0');
					break;
				case 844060008:
					tracingService.Trace("Number range : 8 digit");
					result = empty + (num + 1.0).ToString().PadLeft(8, '0');
					break;
				case 844060010:
					tracingService.Trace("Number range : 10 digit");
					result = empty + (num + 1.0).ToString().PadLeft(10, '0');
					break;
				default:
					tracingService.Trace("Receipt number range unknown. msnfp_numberrange: " + num2);
					break;
				}
				tracingService.Trace("Now update the receipt stacks current number by 1.");
				Entity entity = new Entity(receiptStack.LogicalName, receiptStack.Id);
				entity["msnfp_currentrange"] = num + 1.0;
				service.Update(entity);
				tracingService.Trace("Updated Receipt Stack current range to: " + (num + 1.0));
			}
			else
			{
				tracingService.Trace("No receipt stack found.");
			}
			tracingService.Trace("Exiting SetReceiptIdentifier().");
			return result;
		}

		private static void AssociateReceiptToEventPackage(Guid receiptId, Entity eventPackage, ITracingService tracingService, IOrganizationService service)
		{
			tracingService.Trace("Associating new Receipt to Event Package");
			Entity entity = new Entity(eventPackage.LogicalName, eventPackage.Id);
			entity["msnfp_taxreceiptid"] = new EntityReference("msnfp_receipt", receiptId);
			service.Update(entity);
			tracingService.Trace("Done Associating new Receipt to Event Package");
		}
	}
}
