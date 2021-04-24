using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins.Common
{
	public static class Utilities
	{
		public static string RandomString(int size)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Random random = new Random();
			for (int i = 0; i < size; i++)
			{
				char value = Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * random.NextDouble() + 65.0)));
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		public static byte[] GetWebResourceContent(string displayName, IOrganizationService service, ITracingService tracingService)
		{
			byte[] result = null;
			QueryByAttribute queryByAttribute = new QueryByAttribute("webresource");
			queryByAttribute.AddAttributeValue("displayname", displayName);
			queryByAttribute.ColumnSet = new ColumnSet("content");
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				string attributeValue = entityCollection.Entities[0].GetAttributeValue<string>("content");
				if (!string.IsNullOrEmpty(attributeValue))
				{
					result = Convert.FromBase64String(attributeValue);
				}
			}
			return result;
		}

		public static List<Guid> GetAllLabelsForCheckin(EntityReference checkinSecurityRef, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Creating All Labels for Checking Security:" + checkinSecurityRef.Id.ToString());
			List<Guid> list = new List<Guid>();
			Guid securityLabel = GetSecurityLabel(checkinSecurityRef, service, tracingService);
			list.Add(securityLabel);
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_individualcheckin");
			queryByAttribute.AddAttributeValue("msnfp_checkinsecurity", checkinSecurityRef.Id);
			queryByAttribute.AddAttributeValue("statecode", 0);
			queryByAttribute.AddAttributeValue("msnfp_checkedouton", null);
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				tracingService.Trace("found " + entityCollection.Entities.Count + " individual checkins for this security checkin record.");
				foreach (Entity entity in entityCollection.Entities)
				{
					EntityReference entityReference = entity.ToEntityReference();
					tracingService.Trace("Creating Name Label for " + entityReference.Id.ToString());
					list.Add(GetNameLabel(entityReference, service, tracingService));
					tracingService.Trace("Creating Bag Label for " + entityReference.Id.ToString());
					list.Add(GetBagLabel(entityReference, service, tracingService));
				}
			}
			return list;
		}

		public static void DeactivateOldLabels(EntityReference checkinSecurityRef, EntityReference individualCheckinRef, int labelType, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Deactivating old labels...");
			tracingService.Trace("Type:" + labelType);
			tracingService.Trace("checkinSecurityRef:" + ((checkinSecurityRef != null) ? checkinSecurityRef.Id.ToString() : ""));
			tracingService.Trace("individualCheckinRef:" + ((individualCheckinRef != null) ? individualCheckinRef.Id.ToString() : ""));
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_label");
			queryByAttribute.AddAttributeValue("msnfp_labeltype", labelType);
			queryByAttribute.AddAttributeValue("statecode", 0);
			if (checkinSecurityRef != null)
			{
				queryByAttribute.AddAttributeValue("msnfp_securitychecking", checkinSecurityRef.Id);
			}
			if (individualCheckinRef != null)
			{
				queryByAttribute.AddAttributeValue("msnfp_individualcheckin", individualCheckinRef.Id);
			}
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			if (entityCollection != null && entityCollection.Entities != null)
			{
				tracingService.Trace("Found " + entityCollection.Entities.Count + " labels to deactivate.");
				foreach (Entity entity2 in entityCollection.Entities)
				{
					Entity entity = new Entity("msnfp_label", entity2.Id);
					entity["statecode"] = new OptionSetValue(1);
					entity["statuscode"] = new OptionSetValue(2);
					service.Update(entity);
				}
			}
			tracingService.Trace("Deactivated old labels...");
		}

		public static Guid GetSecurityLabel(EntityReference checkinSecurityRef, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Creating Security Label");
			Entity entity = service.Retrieve(checkinSecurityRef.LogicalName, checkinSecurityRef.Id, new ColumnSet("msnfp_checkedinby", "msnfp_checkedinby", "msnfp_checkedinon"));
			tracingService.Trace("checkinSecurity record id:" + entity.Id.ToString());
			DeactivateOldLabels(checkinSecurityRef, null, 844060000, service, tracingService);
			QueryByAttribute queryByAttribute = new QueryByAttribute("msnfp_individualcheckin");
			queryByAttribute.AddAttributeValue("msnfp_checkinsecurity", checkinSecurityRef.Id);
			queryByAttribute.AddAttributeValue("statecode", 0);
			EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);
			int num = ((entityCollection != null && entityCollection.Entities != null) ? entityCollection.Entities.Count : 0);
			tracingService.Trace("Found " + num + " individual checkins.");
			EntityReference attributeValue = entity.GetAttributeValue<EntityReference>("msnfp_checkedinby");
			Entity entity2 = service.Retrieve(attributeValue.LogicalName, attributeValue.Id, new ColumnSet("fullname", "telephone1"));
			tracingService.Trace("Got Checked in by Id:" + entity2.Id.ToString());
			byte[] webResourceContent = GetWebResourceContent("msnfp_SampleBarCode", service, tracingService);
			tracingService.Trace("Got Entity Image Bytes. Size:" + webResourceContent.Length);
			Entity entity3 = new Entity("msnfp_label");
			entity3["msnfp_securitychecking"] = checkinSecurityRef;
			entity3["msnfp_labeltype"] = new OptionSetValue(844060000);
			entity3["msnfp_securitycode"] = entity.GetAttributeValue<string>("msnfp_securitycode");
			entity3["msnfp_name"] = entity.GetAttributeValue<string>("msnfp_securitycode");
			entity3["msnfp_checkedinby"] = entity2.GetAttributeValue<string>("fullname");
			entity3["msnfp_phone"] = entity2.GetAttributeValue<string>("telephone1");
			entity3["msnfp_checkedincount"] = num.ToString();
			entity3["msnfp_checkedindate"] = entity.GetAttributeValue<DateTime>("msnfp_checkedinon").ToString("f", CultureInfo.CurrentCulture);
			entity3["entityimage"] = webResourceContent;
			Guid guid = service.Create(entity3);
			Guid guid2 = guid;
			tracingService.Trace("Created Label Id:" + guid2.ToString());
			return guid;
		}

		public static Guid GetNameLabel(EntityReference individualCheckinRef, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Creating Name Label");
			Entity entity = service.Retrieve(individualCheckinRef.LogicalName, individualCheckinRef.Id, new ColumnSet("msnfp_checkinsecurity", "msnfp_contact", "msnfp_checkedinas", "msnfp_checkedinby", "msnfp_emergencyphone", "msnfp_checkedinon"));
			tracingService.Trace("individualCheckin id:" + entity.Id.ToString());
			EntityReference entityReference = entity.GetAttributeValue<EntityReference>("msnfp_checkinsecurity") ?? entity.GetAttributeValue<EntityReference>("msnfp_checkinsecurity");
			DeactivateOldLabels(entityReference, individualCheckinRef, 844060001, service, tracingService);
			Entity entity2 = null;
			EntityReference entityReference2 = ((entity.GetAttributeValue<EntityReference>("msnfp_contact") != null) ? entity.GetAttributeValue<EntityReference>("msnfp_contact") : null);
			if (entityReference2 != null)
			{
				entity2 = service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet("fullname", "msnfp_medicalnotes", "msnfp_emergencycontact", "msnfp_emergencyphone"));
				tracingService.Trace("Got checked in contact id:" + entity2.Id.ToString());
			}
			Entity entity3 = new Entity("msnfp_label");
			entity3["msnfp_individualcheckin"] = individualCheckinRef;
			entity3["msnfp_securitychecking"] = entityReference;
			entity3["msnfp_labeltype"] = new OptionSetValue(844060001);
			entity3["msnfp_checkedincontactname"] = ((entity2 != null) ? entity2.GetAttributeValue<string>("fullname") : "");
			string text = (string)(entity3["msnfp_checkedinas"] = GetOptionSetValueLabel(individualCheckinRef.LogicalName, "msnfp_checkedinas", entity.GetAttributeValue<OptionSetValue>("msnfp_checkedinas").Value, service));
			string text2 = (string)(entity3["msnfp_checkedinby"] = ((entity.GetAttributeValue<EntityReference>("msnfp_checkedinby") != null) ? entity.GetAttributeValue<EntityReference>("msnfp_checkedinby").Name : ""));
			entity3["msnfp_phone"] = entity.GetAttributeValue<string>("msnfp_emergencyphone");
			entity3["msnfp_checkedindate"] = entity.GetAttributeValue<DateTime>("msnfp_checkedinon").ToString("f", CultureInfo.CurrentCulture);
			entity3["msnfp_checkedinevents"] = GetCheckedInEventsForLabel(entity, service, tracingService);
			entity3["msnfp_medicalnotes"] = ((entity2 != null) ? entity2.GetAttributeValue<string>("msnfp_medicalnotes") : "");
			Guid guid = service.Create(entity3);
			Guid guid2 = guid;
			tracingService.Trace("Created Label Id:" + guid2.ToString());
			return guid;
		}

		public static Guid GetBagLabel(EntityReference individualCheckinRef, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Creating Bag Label");
			Entity entity = service.Retrieve(individualCheckinRef.LogicalName, individualCheckinRef.Id, new ColumnSet("msnfp_checkinsecurity", "msnfp_contact", "msnfp_checkedinas", "msnfp_checkedinby", "msnfp_emergencyphone", "msnfp_checkedinon"));
			tracingService.Trace("individualCheckin id:" + entity.Id.ToString());
			EntityReference entityReference = entity.GetAttributeValue<EntityReference>("msnfp_checkinsecurity") ?? entity.GetAttributeValue<EntityReference>("msnfp_checkinsecurity");
			DeactivateOldLabels(entityReference, individualCheckinRef, 844060002, service, tracingService);
			Entity entity2 = null;
			EntityReference entityReference2 = ((entity.GetAttributeValue<EntityReference>("msnfp_contact") != null) ? entity.GetAttributeValue<EntityReference>("msnfp_contact") : null);
			if (entityReference2 != null)
			{
				entity2 = service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet("fullname", "msnfp_medicalnotes", "msnfp_emergencycontact", "msnfp_emergencyphone"));
				tracingService.Trace("Got checked in contact id:" + entity2.Id.ToString());
			}
			Entity entity3 = new Entity("msnfp_label");
			entity3["msnfp_individualcheckin"] = individualCheckinRef;
			entity3["msnfp_securitychecking"] = entityReference;
			entity3["msnfp_labeltype"] = new OptionSetValue(844060002);
			entity3["msnfp_checkedincontactname"] = ((entity2 != null) ? entity2.GetAttributeValue<string>("fullname") : "");
			string text = (string)(entity3["msnfp_checkedinas"] = GetOptionSetValueLabel(individualCheckinRef.LogicalName, "msnfp_checkedinas", entity.GetAttributeValue<OptionSetValue>("msnfp_checkedinas").Value, service));
			string text2 = (string)(entity3["msnfp_checkedinby"] = ((entity.GetAttributeValue<EntityReference>("msnfp_checkedinby") != null) ? entity.GetAttributeValue<EntityReference>("msnfp_checkedinby").Name : ""));
			entity3["msnfp_phone"] = entity.GetAttributeValue<string>("msnfp_emergencyphone");
			entity3["msnfp_checkedindate"] = entity.GetAttributeValue<DateTime>("msnfp_checkedinon").ToString("f", CultureInfo.CurrentCulture);
			Guid guid = service.Create(entity3);
			Guid guid2 = guid;
			tracingService.Trace("Created Label Id:" + guid2.ToString());
			return guid;
		}

		private static string GetCheckedInEventsForLabel(Entity individualCheckin, IOrganizationService service, ITracingService tracingService)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string query = $"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>\r\n              <entity name='msnfp_checkinevent'>\r\n                <attribute name='msnfp_checkineventid' />\r\n                <attribute name='msnfp_name' />\r\n                <attribute name='createdon' />\r\n                <order attribute='msnfp_name' descending='false' />\r\n                <link-entity name='msnfp_msnfp_individualcheckin_msnfp_checkineven' from='msnfp_checkineventid' to='msnfp_checkineventid' visible='false' intersect='true'>\r\n                  <link-entity name='msnfp_individualcheckin' from='msnfp_individualcheckinid' to='msnfp_individualcheckinid' alias='ab'>\r\n                    <filter type='and'>\r\n                      <condition attribute='msnfp_individualcheckinid' operator='eq' uiname='' uitype='msnfp_individualcheckin' value='{individualCheckin.Id}' />\r\n                    </filter>\r\n                  </link-entity>\r\n                </link-entity>\r\n              </entity>\r\n            </fetch>";
			EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(query));
			if (entityCollection != null && entityCollection.Entities != null)
			{
				foreach (Entity entity in entityCollection.Entities)
				{
					stringBuilder.AppendLine(entity.GetAttributeValue<string>("msnfp_name") + "(" + entity.GetAttributeValue<DateTime>("msnfp_datestart").ToString("h:mmtt") + ")");
				}
			}
			return stringBuilder.ToString();
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

		public static async Task CallYearlyGivingServiceAsync(Guid entityId, string entityName, Guid configurationId, IOrganizationService service, ITracingService tracingService)
		{
			tracingService.Trace("Entering CallYearlyGivingServiceAsync");
			Entity configuration = service.Retrieve("msnfp_configuration", configurationId, new ColumnSet("msnfp_yearlygivingsecuritykey", "msnfp_bankrunfilewebjoburl"));
			if (configuration == null)
			{
				tracingService.Trace($"No Configuration record found with Id: {configurationId}");
				return;
			}
			if (string.IsNullOrEmpty(configuration.GetAttributeValue<string>("msnfp_yearlygivingsecuritykey")) || string.IsNullOrEmpty(configuration.GetAttributeValue<string>("msnfp_bankrunfilewebjoburl")))
			{
				tracingService.Trace("Missing URL or security key. Cannot access Background Services App.");
				return;
			}
			string[] obj = new string[7]
			{
				configuration.GetAttributeValue<string>("msnfp_bankrunfilewebjoburl"),
				"/api/yearlyGiving/",
				entityName,
				"/",
				null,
				null,
				null
			};
			Guid guid = entityId;
			obj[4] = guid.ToString();
			obj[5] = "?code=";
			obj[6] = configuration.GetAttributeValue<string>("msnfp_yearlygivingsecuritykey");
			string url = string.Concat(obj);
			tracingService.Trace("Got Background services URL");
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(url);
			string result = $"Response Status Code: {response.StatusCode}, Reason:{response.ReasonPhrase}, Content: {response.Content}";
			tracingService.Trace("Result: " + result);
		}
	}
}
