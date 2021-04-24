using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;

namespace Plugins.PaymentProcesses
{
	public class Utilities
	{
		public enum HouseholdRelationshipType
		{
			PrimaryHouseholdMember = 844060000,
			Member,
			Minor,
			Deceased
		}

		public enum AccountType
		{
			Household = 844060000,
			Organization
		}

		public enum DonationImportStatusReason
		{
			Ready = 844060000,
			Failed,
			Active,
			Inactive
		}

		public enum ContactAddressTypeCode
		{
			Primary = 844060000,
			Home,
			Business,
			Seasonal,
			AlternateHome,
			AlternateBusiness
		}

		public enum GivingLevelCalculation
		{
			CurrentCalendar = 844060000,
			CurrentFiscalYear,
			YearToDate
		}

		private enum ShowAPIErrorMessages
		{
			Yes = 844060000,
			No
		}

		public static string GetAzureWebAPIURL(IOrganizationService service, IPluginExecutionContext context)
		{
			string result = string.Empty;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(service);
			Guid initiatingUserId = context.InitiatingUserId;
			Entity user = service.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (user.Contains("msnfp_configurationid"))
			{
				Entity entity = (from c in organizationServiceContext.CreateQuery("msnfp_configuration")
					where (Guid)c["msnfp_configurationid"] == ((EntityReference)user["msnfp_configurationid"]).Id
					orderby c["createdon"] descending
					select c).FirstOrDefault();
				if (entity != null)
				{
					result = (entity.Contains("msnfp_azure_webapiurl") ? ((string)entity["msnfp_azure_webapiurl"]) : string.Empty);
				}
			}
			return result;
		}

		internal void RecalculateGiftBatch(EntityReference giftBatchRef, IOrganizationService service, ITracingService tracingService)
		{
			Entity entity = service.Retrieve(giftBatchRef.LogicalName, giftBatchRef.Id, new ColumnSet("statuscode"));
			tracingService.Trace("Gift Batch Id:" + giftBatchRef.Id.ToString() + ", statuscode:" + entity.GetAttributeValue<OptionSetValue>("statuscode").Value);
			if (entity.GetAttributeValue<OptionSetValue>("statuscode").Value != 1)
			{
				return;
			}
			int num = 0;
			decimal num2 = default(decimal);
			decimal num3 = default(decimal);
			decimal num4 = default(decimal);
			decimal num5 = default(decimal);
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_transaction");
			queryByAttribute.AddAttributeValue("msnfp_giftbatchid", giftBatchRef.Id);
			queryByAttribute.AddAttributeValue("statecode", 0);
			queryByAttribute.ColumnSet = new ColumnSet("msnfp_amount", "msnfp_amount_nonreceiptable", "msnfp_amount_receipted", "msnfp_amount_membership");
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities.Count > 0)
			{
				num = entityCollection.Entities.Count;
				tracingService.Trace($"Found {num} Transactions associated with the Gift Batch.");
				num2 = entityCollection.Entities.Where((Entity w) => w.GetAttributeValue<Money>("msnfp_amount") != null).Sum((Entity t) => t.GetAttributeValue<Money>("msnfp_amount").Value);
				tracingService.Trace($"Transaction Total Amount {num2}");
				num3 = entityCollection.Entities.Where((Entity w) => w.GetAttributeValue<Money>("msnfp_amount_nonreceiptable") != null).Sum((Entity t) => t.GetAttributeValue<Money>("msnfp_amount_nonreceiptable").Value);
				tracingService.Trace($"Transaction Total Non-Receiptable Amount {num3}");
				num4 = entityCollection.Entities.Where((Entity w) => w.GetAttributeValue<Money>("msnfp_amount_receipted") != null).Sum((Entity t) => t.GetAttributeValue<Money>("msnfp_amount_receipted").Value);
				tracingService.Trace($"Transaction Total Receipted Amount {num4}");
				num5 = entityCollection.Entities.Where((Entity w) => w.GetAttributeValue<Money>("msnfp_amount_membership") != null).Sum((Entity t) => t.GetAttributeValue<Money>("msnfp_amount_membership").Value);
				tracingService.Trace($"Transaction Total Membership Amount {num5}");
			}
			Entity entity2 = new Entity(giftBatchRef.LogicalName, giftBatchRef.Id);
			entity2["msnfp_tally_gifts"] = num;
			entity2["msnfp_tally_amount"] = num2;
			entity2["msnfp_tally_amount_nonreceiptable"] = num3;
			entity2["msnfp_tally_amount_receipted"] = num4;
			entity2["msnfp_tally_amount_membership"] = num5;
			tracingService.Trace("Updating Gift Batch.");
			service.Update(entity2);
			tracingService.Trace("Done.");
		}

		public static string GetOptionsetText(Entity entity, IOrganizationService service, string optionsetName, int optionsetValue)
		{
			string result = string.Empty;
			try
			{
				RetrieveOptionSetRequest request = new RetrieveOptionSetRequest
				{
					Name = optionsetName
				};
				RetrieveOptionSetResponse retrieveOptionSetResponse = (RetrieveOptionSetResponse)service.Execute(request);
				OptionSetMetadata optionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;
				OptionMetadata[] array = optionSetMetadata.Options.ToArray();
				OptionMetadata[] array2 = array;
				foreach (OptionMetadata optionMetadata in array2)
				{
					if (optionMetadata.Value == optionsetValue)
					{
						result = optionMetadata.Label.UserLocalizedLabel.Label.ToString();
						break;
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return result;
		}

		public static string GetOptionSetValueLabel(string entityName, string fieldName, int optionSetValue, IOrganizationService service)
		{
			RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
			retrieveAttributeRequest.EntityLogicalName = entityName;
			retrieveAttributeRequest.LogicalName = fieldName;
			retrieveAttributeRequest.RetrieveAsIfPublished = false;
			RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
			EnumAttributeMetadata enumAttributeMetadata = (EnumAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
			return enumAttributeMetadata.OptionSet.Options.Where((OptionMetadata x) => x.Value == optionSetValue).FirstOrDefault().Label.UserLocalizedLabel.Label;
		}

		public Guid CreateVIPAlertTask(IOrganizationService service, Entity Cust, Entity primaryRecord, Entity owner)
		{
			Entity entity = new Entity("msnfp_vipalert");
			string str = string.Empty;
			if (Cust.LogicalName == "contact")
			{
				str = (string)Cust["firstname"] + " " + (string)Cust["lastname"];
			}
			else if (Cust.LogicalName == "account")
			{
				str = (string)Cust["name"];
			}
			entity["subject"] = "VIP Alert for " + str + " on the " + ((DateTime)primaryRecord["createdon"]).ToLocalTime().ToShortDateString();
			entity["description"] = "Constituent / Organization " + str + " has been marked as a VIP and therefore triggered this task";
			if (owner.LogicalName == "systemuser")
			{
				entity["ownerid"] = new EntityReference("systemuser", owner.Id);
			}
			else
			{
				entity["ownerid"] = new EntityReference("team", owner.Id);
			}
			entity["regardingobjectid"] = new EntityReference(primaryRecord.LogicalName, primaryRecord.Id);
			return service.Create(entity);
		}

		public Guid CreateVIPAlertEmail(IOrganizationService service, Entity Cust, Entity primaryRecord, Entity owner, Entity config)
		{
			Entity entity = new Entity("email");
			string text = string.Empty;
			string empty = string.Empty;
			string str = string.Empty;
			Guid guid = Guid.Empty;
			string text2 = (config.Contains("msnfp_organizationurl") ? ((string)config["msnfp_organizationurl"]) : string.Empty);
			if (!string.IsNullOrEmpty(text2))
			{
				if (Cust.LogicalName == "contact")
				{
					text = (string)Cust["firstname"] + " " + (string)Cust["lastname"];
					str = string.Format("<a href='" + text2 + "/main.aspx?etn=contact&id=%7b{0}%7d&newWindow=true&pagetype=entityrecord' target='_blank' style='cursor: pointer;'>{1}</a>", Cust.Id.ToString(), text);
				}
				else if (Cust.LogicalName == "account")
				{
					text = (string)Cust["name"];
					str = string.Format("<a href='" + text2 + "/main.aspx?etn=account&id=%7b{0}%7d&newWindow=true&pagetype=entityrecord' target='_blank' style='cursor: pointer;'>{1}</a>", Cust.Id.ToString(), text);
				}
				empty = "Constituent / Organization " + text + " has been marked as a VIP and therefore triggered this task<br/><br/>";
				empty = empty + "Link to the Constituent / Organization record: " + str;
				entity["activitytypecode"] = new OptionSetValue(4202);
				entity["subject"] = "VIP Alert for " + text + " on the " + ((DateTime)primaryRecord["createdon"]).ToLocalTime().ToShortDateString();
				entity["description"] = empty;
				EntityCollection entityCollection = new EntityCollection();
				Entity entity2 = new Entity("activityparty");
				entity2["partyid"] = new EntityReference("systemuser", owner.Id);
				entityCollection.Entities.Add(entity2);
				entity["to"] = entityCollection;
				entity["regardingobjectid"] = new EntityReference(primaryRecord.LogicalName, primaryRecord.Id);
				guid = service.Create(entity);
				SendEmailRequest request = new SendEmailRequest
				{
					EmailId = guid,
					TrackingToken = "",
					IssueSend = true
				};
				SendEmailResponse sendEmailResponse = (SendEmailResponse)service.Execute(request);
			}
			return guid;
		}

		public void UpdateDonorCommitmentBalance(OrganizationServiceContext orgSvcContext, IOrganizationService service, EntityReference donorCommitment, int type)
		{
			decimal d = default(decimal);
			decimal num = default(decimal);
			decimal num2 = default(decimal);
			Entity donorCommitmentRecord = service.Retrieve("msnfp_donorcommitment", donorCommitment.Id, new ColumnSet("msnfp_donorcommitmentid", "msnfp_totalamount", "msnfp_totalamount_balance", "msnfp_totalamount_paid"));
			if (donorCommitmentRecord == null)
			{
				return;
			}
			if (donorCommitmentRecord.Contains("msnfp_totalamount"))
			{
				d = ((Money)donorCommitmentRecord["msnfp_totalamount"]).Value;
			}
			List<Entity> list = (from a in orgSvcContext.CreateQuery("msnfp_transaction")
				where ((EntityReference)a["msnfp_donorcommitmentid"]).Id == donorCommitmentRecord.Id && (((OptionSetValue)a["statuscode"]).Value == 844060000 || ((OptionSetValue)a["statuscode"]).Value == 844060004)
				select a).ToList();
			foreach (Entity item in list)
			{
				if (item.Contains("msnfp_amount_receipted"))
				{
					num += ((Money)item["msnfp_amount_receipted"]).Value;
				}
				if (item.Contains("msnfp_amount_membership"))
				{
					num += ((Money)item["msnfp_amount_membership"]).Value;
				}
				if (item.Contains("msnfp_amount_nonreceiptable"))
				{
					num += ((Money)item["msnfp_amount_nonreceiptable"]).Value;
				}
				if (item.Contains("msnfp_amount_tax"))
				{
					num += ((Money)item["msnfp_amount_tax"]).Value;
				}
			}
			num2 = d - num;
			donorCommitmentRecord["msnfp_totalamount_balance"] = new Money(num2);
			donorCommitmentRecord["msnfp_totalamount_paid"] = new Money(num);
			if (num2 <= 0m)
			{
				donorCommitmentRecord["statuscode"] = new OptionSetValue(844060000);
			}
			else if (type == 1)
			{
				donorCommitmentRecord["statuscode"] = new OptionSetValue(1);
			}
			service.Update(donorCommitmentRecord);
		}

		public static void UpdateHouseholdOnRecord(IOrganizationService service, Entity record, string householdAttributeName, string customerAttributeName)
		{
			if (record.GetAttributeValue<EntityReference>(customerAttributeName) != null && string.Equals(record.GetAttributeValue<EntityReference>(customerAttributeName).LogicalName, "contact", StringComparison.OrdinalIgnoreCase))
			{
				Entity entity = service.Retrieve(record.GetAttributeValue<EntityReference>(customerAttributeName).LogicalName, record.GetAttributeValue<EntityReference>(customerAttributeName).Id, new ColumnSet(householdAttributeName));
				if (entity.GetAttributeValue<EntityReference>(householdAttributeName) != null)
				{
					Entity entity2 = new Entity(record.LogicalName, record.Id);
					entity2[householdAttributeName] = entity.GetAttributeValue<EntityReference>(householdAttributeName);
					service.Update(entity2);
				}
			}
		}

		public void UpdatePaymentScheduleBalance(OrganizationServiceContext orgSvcContext, IOrganizationService service, Entity donorCommitment, EntityReference paymentSchedule, string message)
		{
			QueryExpression queryExpression = new QueryExpression("msnfp_paymentschedule");
			queryExpression.NoLock = true;
			queryExpression.ColumnSet = new ColumnSet("msnfp_paymentscheduleid", "msnfp_totalamount_balance", "msnfp_totalamount_paid", "msnfp_totalamount");
			queryExpression.Criteria.AddCondition(new ConditionExpression("msnfp_paymentscheduleid", ConditionOperator.Equal, paymentSchedule.Id));
			queryExpression.LinkEntities.Add(new LinkEntity
			{
				EntityAlias = "donorCommitment",
				JoinOperator = JoinOperator.LeftOuter,
				Columns = new ColumnSet("msnfp_totalamount_paid", "msnfp_totalamount_balance", "msnfp_totalamount"),
				LinkCriteria = new FilterExpression
				{
					Conditions = 
					{
						new ConditionExpression("statuscode", ConditionOperator.NotEqual, 844060001)
					}
				},
				LinkToEntityName = "msnfp_donorcommitment",
				LinkToAttributeName = "msnfp_parentscheduleid",
				LinkFromAttributeName = "msnfp_paymentscheduleid",
				LinkFromEntityName = "msnfp_paymentschedule"
			});
			EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
			var anon = (from g in entityCollection.Entities
				group g by g.Id into s
				select new
				{
					TotalAmountBalance = s.FirstOrDefault().GetAttributeValue<Money>("msnfp_totalamount_balance"),
					TotalAmountPaid = s.FirstOrDefault().GetAttributeValue<Money>("msnfp_totalamount_paid"),
					TotalAmount = s.FirstOrDefault().GetAttributeValue<Money>("msnfp_totalamount"),
					TotalAmountBalanceDC = s.Where((Entity w) => w.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount_balance") != null && (Money)w.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount_balance").Value != null).Sum((Entity su) => ((Money)su.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount_balance").Value).Value),
					TotalAmountPaidDC = s.Where((Entity w) => w.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount_paid") != null && (Money)w.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount_paid").Value != null).Sum((Entity su) => ((Money)su.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount_paid").Value).Value),
					TotalAmountDC = s.Where((Entity w) => w.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount") != null && (Money)w.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount").Value != null).Sum((Entity su) => ((Money)su.GetAttributeValue<AliasedValue>("donorCommitment.msnfp_totalamount").Value).Value)
				}).FirstOrDefault();
			if (anon != null)
			{
				Entity entity = new Entity(paymentSchedule.LogicalName, paymentSchedule.Id);
				entity["msnfp_totalamount"] = new Money(anon.TotalAmountDC);
				entity["msnfp_totalamount_balance"] = new Money(anon.TotalAmountBalanceDC);
				entity["msnfp_totalamount_paid"] = new Money(anon.TotalAmountPaidDC);
				service.Update(entity);
			}
		}

		private bool ShowAPIErrorResponses(OptionSetValue showAPIErrorMessageOptionSet)
		{
			bool result = false;
			if (showAPIErrorMessageOptionSet == null)
			{
				return result;
			}
			return showAPIErrorMessageOptionSet.Value == 844060000;
		}

		public void CheckAPIReturnJSONForErrors(string returnedResult, OptionSetValue showAPIErrorMessage, ITracingService tracingService)
		{
			string empty = string.Empty;
			if (returnedResult.Length > 0)
			{
				JObject jObject = JObject.Parse(returnedResult);
				int num = (int)jObject["statusCode"];
				if (num == 200)
				{
					return;
				}
				empty = "An API syncing error has occured. Returned result: " + returnedResult;
			}
			else
			{
				empty = "An API syncing error has occured. Returned result contains no value. Please ensure the API URL in the configuration record is correct";
			}
			tracingService.Trace(empty);
			if (ShowAPIErrorResponses(showAPIErrorMessage))
			{
				throw new InvalidPluginExecutionException(empty);
			}
		}

		public void CreateAddressChange(IOrganizationService service, Entity prePrimaryRecord, Entity postPrimaryRecord, int Type, ITracingService tracingService)
		{
			tracingService.Trace("Entering Address Change method");
			tracingService.Trace("Record Type:" + postPrimaryRecord.LogicalName);
			tracingService.Trace("Address Change Type:" + Type);
			Entity entity = new Entity("msnfp_addresschange");
			int num = 0;
			int num2 = 0;
			switch (Type)
			{
			case 1:
				entity["msnfp_from_line1"] = (prePrimaryRecord.Contains("address1_line1") ? ((string)prePrimaryRecord["address1_line1"]) : string.Empty);
				entity["msnfp_from_line2"] = (prePrimaryRecord.Contains("address1_line2") ? ((string)prePrimaryRecord["address1_line2"]) : string.Empty);
				entity["msnfp_from_line3"] = (prePrimaryRecord.Contains("address1_line3") ? ((string)prePrimaryRecord["address1_line3"]) : string.Empty);
				entity["msnfp_from_city"] = (prePrimaryRecord.Contains("address1_city") ? ((string)prePrimaryRecord["address1_city"]) : string.Empty);
				entity["msnfp_from_postalcode"] = (prePrimaryRecord.Contains("address1_postalcode") ? ((string)prePrimaryRecord["address1_postalcode"]) : string.Empty);
				entity["msnfp_from_stateorprovince"] = (prePrimaryRecord.Contains("address1_stateorprovince") ? ((string)prePrimaryRecord["address1_stateorprovince"]) : string.Empty);
				entity["msnfp_from_country"] = (prePrimaryRecord.Contains("address1_country") ? ((string)prePrimaryRecord["address1_country"]) : string.Empty);
				entity["msnfp_to_line1"] = (postPrimaryRecord.Contains("address1_line1") ? ((string)postPrimaryRecord["address1_line1"]) : string.Empty);
				entity["msnfp_to_line2"] = (postPrimaryRecord.Contains("address1_line2") ? ((string)postPrimaryRecord["address1_line2"]) : string.Empty);
				entity["msnfp_to_line3"] = (postPrimaryRecord.Contains("address1_line3") ? ((string)postPrimaryRecord["address1_line3"]) : string.Empty);
				entity["msnfp_to_city"] = (postPrimaryRecord.Contains("address1_city") ? ((string)postPrimaryRecord["address1_city"]) : string.Empty);
				entity["msnfp_to_postalcode"] = (postPrimaryRecord.Contains("address1_postalcode") ? ((string)postPrimaryRecord["address1_postalcode"]) : string.Empty);
				entity["msnfp_to_stateorprovince"] = (postPrimaryRecord.Contains("address1_stateorprovince") ? ((string)postPrimaryRecord["address1_stateorprovince"]) : string.Empty);
				entity["msnfp_to_country"] = (postPrimaryRecord.Contains("address1_country") ? ((string)postPrimaryRecord["address1_country"]) : string.Empty);
				break;
			case 2:
				entity["msnfp_from_line1"] = (prePrimaryRecord.Contains("address2_line1") ? ((string)prePrimaryRecord["address2_line1"]) : string.Empty);
				entity["msnfp_from_line2"] = (prePrimaryRecord.Contains("address2_line2") ? ((string)prePrimaryRecord["address2_line2"]) : string.Empty);
				entity["msnfp_from_line3"] = (prePrimaryRecord.Contains("address2_line3") ? ((string)prePrimaryRecord["address2_line3"]) : string.Empty);
				entity["msnfp_from_city"] = (prePrimaryRecord.Contains("address2_city") ? ((string)prePrimaryRecord["address2_city"]) : string.Empty);
				entity["msnfp_from_postalcode"] = (prePrimaryRecord.Contains("address2_postalcode") ? ((string)prePrimaryRecord["address2_postalcode"]) : string.Empty);
				entity["msnfp_from_stateorprovince"] = (prePrimaryRecord.Contains("address2_stateorprovince") ? ((string)prePrimaryRecord["address2_stateorprovince"]) : string.Empty);
				entity["msnfp_from_country"] = (prePrimaryRecord.Contains("address2_country") ? ((string)prePrimaryRecord["address2_country"]) : string.Empty);
				entity["msnfp_to_line1"] = (postPrimaryRecord.Contains("address2_line1") ? ((string)postPrimaryRecord["address2_line1"]) : string.Empty);
				entity["msnfp_to_line2"] = (postPrimaryRecord.Contains("address2_line2") ? ((string)postPrimaryRecord["address2_line2"]) : string.Empty);
				entity["msnfp_to_line3"] = (postPrimaryRecord.Contains("address2_line3") ? ((string)postPrimaryRecord["address2_line3"]) : string.Empty);
				entity["msnfp_to_city"] = (postPrimaryRecord.Contains("address2_city") ? ((string)postPrimaryRecord["address2_city"]) : string.Empty);
				entity["msnfp_to_postalcode"] = (postPrimaryRecord.Contains("address2_postalcode") ? ((string)postPrimaryRecord["address2_postalcode"]) : string.Empty);
				entity["msnfp_to_stateorprovince"] = (postPrimaryRecord.Contains("address2_stateorprovince") ? ((string)postPrimaryRecord["address2_stateorprovince"]) : string.Empty);
				entity["msnfp_to_country"] = (postPrimaryRecord.Contains("address2_country") ? ((string)postPrimaryRecord["address2_country"]) : string.Empty);
				break;
			case 3:
				entity["msnfp_from_line1"] = (prePrimaryRecord.Contains("address3_line1") ? ((string)prePrimaryRecord["address3_line1"]) : string.Empty);
				entity["msnfp_from_line2"] = (prePrimaryRecord.Contains("address3_line2") ? ((string)prePrimaryRecord["address3_line2"]) : string.Empty);
				entity["msnfp_from_line3"] = (prePrimaryRecord.Contains("address3_line3") ? ((string)prePrimaryRecord["address3_line3"]) : string.Empty);
				entity["msnfp_from_city"] = (prePrimaryRecord.Contains("address3_city") ? ((string)prePrimaryRecord["address3_city"]) : string.Empty);
				entity["msnfp_from_postalcode"] = (prePrimaryRecord.Contains("address3_postalcode") ? ((string)prePrimaryRecord["address3_postalcode"]) : string.Empty);
				entity["msnfp_from_stateorprovince"] = (prePrimaryRecord.Contains("address3_stateorprovince") ? ((string)prePrimaryRecord["address3_stateorprovince"]) : string.Empty);
				entity["msnfp_from_country"] = (prePrimaryRecord.Contains("address3_country") ? ((string)prePrimaryRecord["address3_country"]) : string.Empty);
				entity["msnfp_to_line1"] = (postPrimaryRecord.Contains("address3_line1") ? ((string)postPrimaryRecord["address3_line1"]) : string.Empty);
				entity["msnfp_to_line2"] = (postPrimaryRecord.Contains("address3_line2") ? ((string)postPrimaryRecord["address3_line2"]) : string.Empty);
				entity["msnfp_to_line3"] = (postPrimaryRecord.Contains("address3_line3") ? ((string)postPrimaryRecord["address3_line3"]) : string.Empty);
				entity["msnfp_to_city"] = (postPrimaryRecord.Contains("address3_city") ? ((string)postPrimaryRecord["address3_city"]) : string.Empty);
				entity["msnfp_to_postalcode"] = (postPrimaryRecord.Contains("address3_postalcode") ? ((string)postPrimaryRecord["address3_postalcode"]) : string.Empty);
				entity["msnfp_to_stateorprovince"] = (postPrimaryRecord.Contains("address3_stateorprovince") ? ((string)postPrimaryRecord["address3_stateorprovince"]) : string.Empty);
				entity["msnfp_to_country"] = (postPrimaryRecord.Contains("address3_country") ? ((string)postPrimaryRecord["address3_country"]) : string.Empty);
				break;
			}
			tracingService.Trace("Copied Main Address Fields to Address Change Object.");
			string text = string.Empty;
			if (postPrimaryRecord.LogicalName == "contact")
			{
				if (postPrimaryRecord.Contains("firstname"))
				{
					text = (string)postPrimaryRecord["firstname"] + " ";
				}
				text += (string)postPrimaryRecord["lastname"];
				if (postPrimaryRecord.Contains("address1_addresstypecode") || postPrimaryRecord.Contains("address2_addresstypecode") || postPrimaryRecord.Contains("address3_addresstypecode"))
				{
					if (postPrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode") != null)
					{
						num = postPrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode").Value;
					}
					if (num == Constants.CONTACT_ADDRESS_TYPE_PRIMARY)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060000);
					}
					else if (num == Constants.CONTACT_ADDRESS_TYPE_HOME)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060001);
					}
					else if (num == Constants.CONTACT_ADDRESS_TYPE_BUSINESS)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060002);
					}
					else if (num == Constants.CONTACT_ADDRESS_TYPE_SEASONAL)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060003);
					}
					else if (num == Constants.CONTACT_ADDRESS_TYPE_ALTERNATEHOME)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060004);
					}
					else if (num == Constants.CONTACT_ADDRESS_TYPE_ALTERNATEBUSINESS)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060005);
					}
					tracingService.Trace("Got To Address Type Code for Contact");
				}
				if (prePrimaryRecord.Contains("address1_addresstypecode") || prePrimaryRecord.Contains("address2_addresstypecode") || prePrimaryRecord.Contains("address3_addresstypecode"))
				{
					if (prePrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode") != null)
					{
						num = prePrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode").Value;
					}
					if (num2 == Constants.CONTACT_ADDRESS_TYPE_PRIMARY)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060000);
					}
					else if (num2 == Constants.CONTACT_ADDRESS_TYPE_HOME)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060001);
					}
					else if (num2 == Constants.CONTACT_ADDRESS_TYPE_BUSINESS)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060002);
					}
					else if (num2 == Constants.CONTACT_ADDRESS_TYPE_SEASONAL)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060003);
					}
					else if (num2 == Constants.CONTACT_ADDRESS_TYPE_ALTERNATEHOME)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060004);
					}
					else if (num2 == Constants.CONTACT_ADDRESS_TYPE_ALTERNATEBUSINESS)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060005);
					}
					tracingService.Trace("Got From Address Type Code for Contact");
				}
			}
			else if (postPrimaryRecord.LogicalName == "account")
			{
				if (postPrimaryRecord.Contains("name"))
				{
					text = (string)postPrimaryRecord["name"];
				}
				if (postPrimaryRecord.Contains("address1_addresstypecode") || postPrimaryRecord.Contains("address2_addresstypecode"))
				{
					if (postPrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode") != null)
					{
						num = postPrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode").Value;
					}
					if (num == Constants.ACCOUNT_ADDRESS_TYPE_PRIMARY)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060000);
					}
					else if (num == Constants.ACCOUNT_ADDRESS_TYPE_BUSINESS)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060002);
					}
					else if (num == Constants.ACCOUNT_ADDRESS_TYPE_ALTERNATEBUSINESS)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060005);
					}
					else if (num == Constants.ACCOUNT_ADDRESS_TYPE_OTHER)
					{
						entity["msnfp_to_addresstypecode"] = new OptionSetValue(844060006);
					}
					tracingService.Trace("Got To Address Type Code for Account");
				}
				if (prePrimaryRecord.Contains("address1_addresstypecode") || prePrimaryRecord.Contains("address2_addresstypecode"))
				{
					if (prePrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode") != null)
					{
						num = prePrimaryRecord.GetAttributeValue<OptionSetValue>("address" + Type + "addresstypecode").Value;
					}
					if (num2 == Constants.ACCOUNT_ADDRESS_TYPE_PRIMARY)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060000);
					}
					else if (num2 == Constants.ACCOUNT_ADDRESS_TYPE_BUSINESS)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060002);
					}
					else if (num2 == Constants.ACCOUNT_ADDRESS_TYPE_ALTERNATEBUSINESS)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060005);
					}
					else if (num2 == Constants.ACCOUNT_ADDRESS_TYPE_OTHER)
					{
						entity["msnfp_from_addresstypecode"] = new OptionSetValue(844060006);
					}
					tracingService.Trace("Got From Address Type Code for Account");
				}
			}
			entity["subject"] = "Address Change for " + text + " on the " + DateTime.Now.ToShortDateString();
			entity["regardingobjectid"] = new EntityReference(prePrimaryRecord.LogicalName, prePrimaryRecord.Id);
			service.Create(entity);
			tracingService.Trace("Exiting Address Change method");
		}

		public Guid CreateAddressChangeAlertTask(IOrganizationService service, Entity prePrimaryRecord, Entity postPrimaryRecord, EntityCollection marketingList, Entity contactOwner)
		{
			Entity entity = new Entity("msnfp_addresschange");
			entity["msnfp_from_line1"] = (prePrimaryRecord.Contains("address1_line1") ? ((string)prePrimaryRecord["address1_line1"]) : string.Empty);
			entity["msnfp_from_line2"] = (prePrimaryRecord.Contains("address1_line2") ? ((string)prePrimaryRecord["address1_line2"]) : string.Empty);
			entity["msnfp_from_line3"] = (prePrimaryRecord.Contains("address1_line3") ? ((string)prePrimaryRecord["address1_line3"]) : string.Empty);
			entity["msnfp_from_city"] = (prePrimaryRecord.Contains("address1_city") ? ((string)prePrimaryRecord["address1_city"]) : string.Empty);
			entity["msnfp_from_postalcode"] = (prePrimaryRecord.Contains("address1_postalcode") ? ((string)prePrimaryRecord["address1_postalcode"]) : string.Empty);
			entity["msnfp_from_stateorprovince"] = (prePrimaryRecord.Contains("address1_stateorprovince") ? ((string)prePrimaryRecord["address1_stateorprovince"]) : string.Empty);
			entity["msnfp_from_country"] = (prePrimaryRecord.Contains("address1_country") ? ((string)prePrimaryRecord["address1_country"]) : string.Empty);
			entity["msnfp_to_line1"] = (postPrimaryRecord.Contains("address1_line1") ? ((string)postPrimaryRecord["address1_line1"]) : string.Empty);
			entity["msnfp_to_line2"] = (postPrimaryRecord.Contains("address1_line2") ? ((string)postPrimaryRecord["address1_line2"]) : string.Empty);
			entity["msnfp_to_line3"] = (postPrimaryRecord.Contains("address1_line3") ? ((string)postPrimaryRecord["address1_line3"]) : string.Empty);
			entity["msnfp_to_city"] = (postPrimaryRecord.Contains("address1_city") ? ((string)postPrimaryRecord["address1_city"]) : string.Empty);
			entity["msnfp_to_postalcode"] = (postPrimaryRecord.Contains("address1_postalcode") ? ((string)postPrimaryRecord["address1_postalcode"]) : string.Empty);
			entity["msnfp_to_stateorprovince"] = (postPrimaryRecord.Contains("address1_stateorprovince") ? ((string)postPrimaryRecord["address1_stateorprovince"]) : string.Empty);
			entity["msnfp_to_country"] = (postPrimaryRecord.Contains("address1_country") ? ((string)postPrimaryRecord["address1_country"]) : string.Empty);
			string text = string.Empty;
			if (postPrimaryRecord.LogicalName == "contact")
			{
				if (postPrimaryRecord.Contains("firstname"))
				{
					text = (string)postPrimaryRecord["firstname"] + " ";
				}
				text += (string)postPrimaryRecord["lastname"];
			}
			else if (postPrimaryRecord.LogicalName == "account" && postPrimaryRecord.Contains("name"))
			{
				text = (string)postPrimaryRecord["name"];
			}
			entity["subject"] = "Address Change for " + text + " on the " + DateTime.Now.ToShortDateString();
			StringBuilder stringBuilder = new StringBuilder();
			if (marketingList.Entities.Count > 0)
			{
				stringBuilder.Append("Related Marketing List : " + Environment.NewLine + Environment.NewLine);
				foreach (Entity entity3 in marketingList.Entities)
				{
					Entity entity2 = service.Retrieve("list", ((EntityReference)entity3["listid"]).Id, new ColumnSet("listid", "listname"));
					stringBuilder.Append((string)entity2["listname"]);
					stringBuilder.Append(Environment.NewLine + Environment.NewLine);
				}
			}
			entity["description"] = stringBuilder.ToString();
			if (contactOwner.LogicalName == "systemuser")
			{
				entity["ownerid"] = new EntityReference("systemuser", contactOwner.Id);
			}
			else
			{
				entity["ownerid"] = new EntityReference("team", contactOwner.Id);
			}
			entity["regardingobjectid"] = new EntityReference(prePrimaryRecord.LogicalName, prePrimaryRecord.Id);
			return service.Create(entity);
		}

		public Guid CreateAddressChangeAlertEmail(IOrganizationService service, Entity prePrimaryRecord, Entity postPrimaryRecord, EntityCollection marketingList, Entity contactOwner, Entity config)
		{
			Entity entity = new Entity("email");
			Guid guid = Guid.Empty;
			string text = string.Empty;
			string str = string.Empty;
			string text2 = (config.Contains("msnfp_organizationurl") ? ((string)config["msnfp_organizationurl"]) : string.Empty);
			if (!string.IsNullOrEmpty(text2))
			{
				if (postPrimaryRecord.LogicalName == "contact")
				{
					text = (string)postPrimaryRecord["firstname"] + " " + (string)postPrimaryRecord["lastname"];
					str = string.Format("<a href='" + text2 + "/main.aspx?etn=contact&id=%7b{0}%7d&newWindow=true&pagetype=entityrecord' target='_blank' style='cursor: pointer;'>{1}</a>", postPrimaryRecord.Id.ToString(), text);
				}
				else if (postPrimaryRecord.LogicalName == "account")
				{
					text = (string)postPrimaryRecord["name"];
					str = string.Format("<a href='" + text2 + "/main.aspx?etn=account&id=%7b{0}%7d&newWindow=true&pagetype=entityrecord' target='_blank' style='cursor: pointer;'>{1}</a>", postPrimaryRecord.Id.ToString(), text);
				}
				entity["activitytypecode"] = new OptionSetValue(4202);
				entity["subject"] = "Address Change for " + text + " on the " + DateTime.Now.ToShortDateString();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("<b>Record: </b>" + str);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append("<b>Address Prior : </b>");
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_line1") ? ((string)prePrimaryRecord["address1_line1"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_line2") ? ((string)prePrimaryRecord["address1_line2"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_line3") ? ((string)prePrimaryRecord["address1_line3"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_city") ? ((string)prePrimaryRecord["address1_city"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_stateorprovince") ? ((string)prePrimaryRecord["address1_stateorprovince"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_country") ? ((string)prePrimaryRecord["address1_country"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(prePrimaryRecord.Contains("address1_postalcode") ? ((string)prePrimaryRecord["address1_postalcode"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append("========================================");
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append("<b>Address Now : </b>");
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_line1") ? ((string)postPrimaryRecord["address1_line1"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_line2") ? ((string)postPrimaryRecord["address1_line2"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_line3") ? ((string)postPrimaryRecord["address1_line3"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_city") ? ((string)postPrimaryRecord["address1_city"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_stateorprovince") ? ((string)postPrimaryRecord["address1_stateorprovince"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_country") ? ((string)postPrimaryRecord["address1_country"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append(postPrimaryRecord.Contains("address1_postalcode") ? ((string)postPrimaryRecord["address1_postalcode"]) : string.Empty);
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append("========================================");
				stringBuilder.Append("<br/><br/>");
				stringBuilder.Append("<br/><br/>");
				if (marketingList.Entities.Count > 0)
				{
					stringBuilder.Append("Related Marketing List : ");
					stringBuilder.Append("<br/><br/>");
					foreach (Entity entity4 in marketingList.Entities)
					{
						Entity entity2 = service.Retrieve("list", ((EntityReference)entity4["listid"]).Id, new ColumnSet("listid", "listname"));
						stringBuilder.Append((string)entity2["listname"]);
						stringBuilder.Append("<br/><br/>");
					}
				}
				entity["description"] = stringBuilder.ToString();
				EntityCollection entityCollection = new EntityCollection();
				Entity entity3 = new Entity("activityparty");
				entity3["partyid"] = new EntityReference("systemuser", contactOwner.Id);
				entityCollection.Entities.Add(entity3);
				entity["to"] = entityCollection;
				entity["regardingobjectid"] = new EntityReference(prePrimaryRecord.LogicalName, prePrimaryRecord.Id);
				guid = service.Create(entity);
				SendEmailRequest request = new SendEmailRequest
				{
					EmailId = guid,
					TrackingToken = "",
					IssueSend = true
				};
				SendEmailResponse sendEmailResponse = (SendEmailResponse)service.Execute(request);
			}
			return guid;
		}

		public static string retrieveobjectTypeCode(string entitylogicalname, IOrganizationService service)
		{
			Entity entity = new Entity(entitylogicalname);
			RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest();
			retrieveEntityRequest.LogicalName = entity.LogicalName;
			retrieveEntityRequest.EntityFilters = EntityFilters.All;
			RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)service.Execute(retrieveEntityRequest);
			EntityMetadata entityMetadata = retrieveEntityResponse.EntityMetadata;
			return entityMetadata.ObjectTypeCode.ToString();
		}

		private string DecryptString(string Message, string Passphrase)
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] key = mD5CryptoServiceProvider.ComputeHash(uTF8Encoding.GetBytes(Passphrase));
			TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			tripleDESCryptoServiceProvider.Key = key;
			tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
			tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
			byte[] array = Convert.FromBase64String(Message);
			byte[] bytes;
			try
			{
				ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor();
				bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
			}
			finally
			{
				tripleDESCryptoServiceProvider.Clear();
				mD5CryptoServiceProvider.Clear();
			}
			return uTF8Encoding.GetString(bytes);
		}

		public static int GetOptionsSetValueForLabel(IOrganizationService service, string entityName, string attributeName, string selectedLabel)
		{
			RetrieveAttributeRequest request = new RetrieveAttributeRequest
			{
				EntityLogicalName = entityName,
				LogicalName = attributeName,
				RetrieveAsIfPublished = false
			};
			RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(request);
			PicklistAttributeMetadata picklistAttributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
			OptionMetadata[] array = picklistAttributeMetadata.OptionSet.Options.ToArray();
			int result = 0;
			OptionMetadata[] array2 = array;
			foreach (OptionMetadata optionMetadata in array2)
			{
				if (optionMetadata.Label.LocalizedLabels[0].Label.ToString().ToLower() == selectedLabel.ToLower())
				{
					result = optionMetadata.Value.Value;
					break;
				}
			}
			return result;
		}

		public static Entity GetConfigurationRecordByUser(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
		{
			string text = "msnfp_configurationid";
			Entity entity = null;
			ColumnSet columnSet = new ColumnSet("msnfp_configurationid", "msnfp_apipadlocktoken", "msnfp_sche_retryinterval", "msnfp_azure_webapiurl", "msnfp_householdsequence", "msnfp_givinglevelcalculation", "msnfp_showapierrorresponses");
			Entity entity2 = service.Retrieve("systemuser", context.InitiatingUserId, new ColumnSet(text));
			if (entity2 != null && entity2.GetAttributeValue<EntityReference>(text) != null)
			{
				tracingService.Trace("Retrieving User's Configuration record.");
				EntityReference attributeValue = entity2.GetAttributeValue<EntityReference>(text);
				tracingService.Trace("Configuration Id:" + attributeValue.Id.ToString());
				entity = service.Retrieve(attributeValue.LogicalName, attributeValue.Id, columnSet);
				tracingService.Trace("Got Configuration From User");
			}
			else
			{
				tracingService.Trace("User does not have a Configuration Record. Retrieving Default Configuration Record");
				QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_configuration");
				queryByAttribute.ColumnSet = columnSet;
				queryByAttribute.AddAttributeValue("msnfp_defaultconfiguration", true);
				queryByAttribute.AddOrder("modifiedon", OrderType.Descending);
				queryByAttribute.TopCount = 1;
				EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
				if (entityCollection == null || !((entityCollection.Entities != null) & (entityCollection.Entities.Count == 1)))
				{
					throw new Exception("User does not have a Configuration record and no Configuration record has been set as the Default.");
				}
				entity = entityCollection.Entities.First();
				tracingService.Trace("Default Configuration record Id:" + entity.Id.ToString());
			}
			return entity;
		}

		public static Entity GetConfigurationRecordByMessageName(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
		{
			string messageName = context.MessageName;
			string text = "msnfp_configurationid";
			Entity entity = null;
			ColumnSet columnSet = new ColumnSet("msnfp_configurationid", "msnfp_apipadlocktoken", "msnfp_sche_retryinterval", "msnfp_azure_webapiurl", "msnfp_showapierrorresponses");
			if (string.Compare(messageName, "Create", ignoreCase: true) == 0)
			{
				entity = GetConfigurationRecordByUser(context, service, tracingService);
			}
			else
			{
				if (string.Compare(messageName, "Update", ignoreCase: true) != 0 && string.Compare(messageName, "Delete", ignoreCase: true) != 0)
				{
					throw new Exception("Unexpected Message:" + messageName);
				}
				Guid id;
				if (context.InputParameters["Target"] is Entity)
				{
					Entity entity2 = (Entity)context.InputParameters["Target"];
					id = entity2.Id;
				}
				else
				{
					EntityReference entityReference = (EntityReference)context.InputParameters["Target"];
					id = entityReference.Id;
				}
				Entity entity3 = service.Retrieve(context.PrimaryEntityName, id, new ColumnSet(text));
				if (entity3 == null || entity3.GetAttributeValue<EntityReference>(text) == null)
				{
					Guid guid = id;
					throw new Exception("No configuration record found on this record (" + guid.ToString() + "). Please ensure the record has a configuration record attached.");
				}
				EntityReference attributeValue = entity3.GetAttributeValue<EntityReference>(text);
				entity = service.Retrieve(attributeValue.LogicalName, attributeValue.Id, columnSet);
				tracingService.Trace("Got Configuration From Target");
			}
			return entity;
		}

		public static EntityReference CreateHouseholdFromContact(IOrganizationService service, OptionSetValue householdType, Entity target)
		{
			EntityReference result = null;
			if (householdType.Value == 844060000)
			{
				Entity entity = new Entity("account");
				entity["primarycontactid"] = target.ToEntityReference();
				entity["msnfp_accounttype"] = new OptionSetValue(844060000);
				entity["name"] = target.GetAttributeValue<string>("lastname") + " Household";
				entity.Id = service.Create(entity);
				result = entity.ToEntityReference();
			}
			return result;
		}

		public static EntityReference SearchHousehold(IOrganizationService service, Entity donationImport, EntityReference contactRef)
		{
			FilterExpression filterExpression = new FilterExpression();
			filterExpression.AddCondition(new ConditionExpression("msnfp_accounttype", ConditionOperator.Equal, 844060000));
			if (donationImport.Attributes.ContainsKey("msnfp_billing_line1"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_line1", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_line1")));
			}
			if (donationImport.Attributes.ContainsKey("msnfp_billing_line2"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_line2", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_line2")));
			}
			if (donationImport.Attributes.ContainsKey("msnfp_billing_line3"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_line3", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_line3")));
			}
			if (donationImport.Attributes.ContainsKey("msnfp_billing_postalcode"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_postalcode", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_postalcode")));
			}
			if (donationImport.Attributes.ContainsKey("msnfp_billing_stateorprovince"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_stateorprovince", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_stateorprovince")));
			}
			if (donationImport.Attributes.ContainsKey("msnfp_billing_country"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_country", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_country")));
			}
			if (donationImport.Attributes.ContainsKey("msnfp_billing_city"))
			{
				filterExpression.AddCondition(new ConditionExpression("address1_city", ConditionOperator.Equal, donationImport.GetAttributeValue<string>("msnfp_billing_city")));
			}
			Entity entity = null;
			if (filterExpression.Conditions.Count > 1)
			{
				QueryExpression queryExpression = new QueryExpression();
				queryExpression.EntityName = "account";
				queryExpression.Criteria.Filters.Add(filterExpression);
				EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
				entity = entityCollection.Entities.FirstOrDefault();
			}
			return entity?.ToEntityReference();
		}

		public static string GenerateHouseHoldSequence(IOrganizationService service, Entity accountEntity, Entity configuration)
		{
			return string.Empty;
		}

		public static decimal CalculateEventTotalRevenue(Entity eventToUpdate, IOrganizationService service, OrganizationServiceContext orgSvcContext, ITracingService tracingService)
		{
			tracingService.Trace("Calculating Total Revenue for Event Id " + eventToUpdate.Id.ToString());
			decimal num = default(decimal);
			Entity entity = service.Retrieve(eventToUpdate.LogicalName, eventToUpdate.Id, new ColumnSet("msnfp_sum_donations", "msnfp_sum_products", "msnfp_sum_sponsorships", "msnfp_sum_tickets"));
			tracingService.Trace("Got totals for event.");
			IQueryable<decimal> queryable = from x in orgSvcContext.CreateQuery("msnfp_transaction")
				where x.GetAttributeValue<EntityReference>("msnfp_eventid").Id == eventToUpdate.Id && x.GetAttributeValue<OptionSetValue>("statuscode").Value == 844060000 && x.GetAttributeValue<Money>("msnfp_amount") != null
				select x.GetAttributeValue<Money>("msnfp_amount").Value;
			decimal d = default(decimal);
			foreach (decimal item in queryable)
			{
				d += item;
			}
			tracingService.Trace("totalDonations:" + d);
			IQueryable<decimal> queryable2 = from x in orgSvcContext.CreateQuery("msnfp_eventproduct")
				where x.GetAttributeValue<EntityReference>("msnfp_eventid").Id == eventToUpdate.Id && x.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && x.GetAttributeValue<OptionSetValue>("statuscode").Value != 844060000 && x.GetAttributeValue<Money>("msnfp_val_sold") != null
				select x.GetAttributeValue<Money>("msnfp_val_sold").Value;
			decimal d2 = default(decimal);
			foreach (decimal item2 in queryable2)
			{
				d2 += item2;
			}
			tracingService.Trace("totalProducts:" + d2);
			IQueryable<decimal> queryable3 = from x in orgSvcContext.CreateQuery("msnfp_eventsponsorship")
				where x.GetAttributeValue<EntityReference>("msnfp_eventid").Id == eventToUpdate.Id && x.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && x.GetAttributeValue<OptionSetValue>("statuscode").Value != 844060000 && x.GetAttributeValue<Money>("msnfp_val_sold") != null
				select x.GetAttributeValue<Money>("msnfp_val_sold").Value;
			decimal d3 = default(decimal);
			foreach (decimal item3 in queryable3)
			{
				d3 += item3;
			}
			tracingService.Trace("totalSponsorships:" + d3);
			IQueryable<decimal> queryable4 = from x in orgSvcContext.CreateQuery("msnfp_eventticket")
				where x.GetAttributeValue<EntityReference>("msnfp_eventid").Id == eventToUpdate.Id && x.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && x.GetAttributeValue<OptionSetValue>("statuscode").Value != 844060000 && x.GetAttributeValue<Money>("msnfp_val_sold") != null
				select x.GetAttributeValue<Money>("msnfp_val_sold").Value;
			decimal d4 = default(decimal);
			foreach (decimal item4 in queryable4)
			{
				d4 += item4;
			}
			tracingService.Trace("totalTickets:" + d4);
			num = d + d2 + d3 + d4;
			tracingService.Trace("totalRevenue:" + num);
			return num;
		}
	}
}
