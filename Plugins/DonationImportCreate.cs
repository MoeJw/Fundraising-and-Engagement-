using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class DonationImportCreate : PluginBase
	{
		public DonationImportCreate(string unsecure, string secure)
			: base(typeof(DonationImportCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered DonationImportCreate.cs ---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			string messageName = pluginExecutionContext.MessageName;
			localContext.TracingService.Trace("messageName: " + messageName);
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			Entity configurationRecordByMessageName = Utilities.GetConfigurationRecordByMessageName(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering DonationImportCreate.cs Main Function---------");
				Entity entity2 = (Entity)pluginExecutionContext.InputParameters["Target"];
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (entity3 != null && entity3.Contains("msnfp_statusupdated") && !entity3.Contains("msnfp_transactionid"))
				{
					localContext.TracingService.Trace("Contains msnfp_statusupdated.");
					if (messageName == "Update")
					{
						entity3 = pluginExecutionContext.PostEntityImages["postImage"];
					}
					if (((OptionSetValue)entity3["msnfp_statusupdated"]).Value == 844060000)
					{
						localContext.TracingService.Trace("Checking Import Status..");
						if (entity3.GetAttributeValue<OptionSetValue>("msnfp_importresult") != null && entity3.GetAttributeValue<OptionSetValue>("msnfp_importresult").Value == 844060001)
						{
							organizationService.Update(new Entity(entity3.LogicalName, entity3.Id)
							{
								Attributes = 
								{
									new KeyValuePair<string, object>("msnfp_statusupdated", new OptionSetValue(844060001))
								}
							});
							return;
						}
						localContext.TracingService.Trace("Status Ready - processing donation import.");
						try
						{
							Entity entity4 = null;
							Guid empty = Guid.Empty;
							bool flag = false;
							bool flag2 = false;
							Entity entity5 = null;
							Entity entity6 = null;
							string empty2 = string.Empty;
							Guid empty3 = Guid.Empty;
							Guid empty4 = Guid.Empty;
							ColumnSet columnSet = null;
							bool flag3 = entity3.Contains("msnfp_createdonor") && (bool)entity3["msnfp_createdonor"];
							string empty5 = string.Empty;
							Guid empty6 = Guid.Empty;
							OptionSetValue attributeValue = entity3.GetAttributeValue<OptionSetValue>("msnfp_anonymous");
							bool flag4 = entity3.Contains("msnfp_donotsendmm") && (bool)entity3["msnfp_donotsendmm"];
							bool flag5 = entity3.Contains("msnfp_constituent_donotsendmm") && (bool)entity3["msnfp_constituent_donotsendmm"];
							if (entity3.Contains("msnfp_teamownerid") && entity3["msnfp_teamownerid"] != null)
							{
								empty5 = ((EntityReference)entity3["msnfp_teamownerid"]).LogicalName;
								empty6 = ((EntityReference)entity3["msnfp_teamownerid"]).Id;
							}
							else
							{
								empty5 = ((EntityReference)entity3["ownerid"]).LogicalName;
								empty6 = ((EntityReference)entity3["ownerid"]).Id;
							}
							KeyValuePair<string, object> keyValuePair = InitiateHouseholdProcess(organizationService, entity3, entity5?.ToEntityReference());
							if (keyValuePair.Key != "msnfp_householdid")
							{
								localContext.Trace("Added shared variable..");
								localContext.PluginExecutionContext.SharedVariables.Add("DonationImport", entity3);
							}
							else
							{
								entity3["msnfp_householdid"] = (EntityReference)keyValuePair.Value;
							}
							if (entity3.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("Donation Import contains Customer");
								empty2 = ((EntityReference)entity3["msnfp_customerid"]).LogicalName;
								empty3 = ((EntityReference)entity3["msnfp_customerid"]).Id;
								if (empty2 == "account")
								{
									entity5 = organizationService.Retrieve(empty2, empty3, new ColumnSet("accountid", "name"));
								}
								else if (empty2 == "contact")
								{
									columnSet = new ColumnSet("contactid", "salutation", "lastname", "fullname", "middlename", "firstname", "birthdate", "emailaddress1", "telephone1", "mobilephone", "gendercode", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode");
									entity5 = organizationService.Retrieve(empty2, empty3, columnSet);
									if (entity3.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") != null)
									{
										organizationService.Update(new Entity(entity5.LogicalName, entity5.Id)
										{
											Attributes = 
											{
												new KeyValuePair<string, object>("msnfp_householdrelationship", entity3.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship"))
											}
										});
									}
								}
								localContext.TracingService.Trace("Update Donation Import with Donor.");
							}
							else
							{
								localContext.TracingService.Trace("Donation Import does not have customer.");
								string text = (entity3.Contains("msnfp_organizationname") ? ((string)entity3["msnfp_organizationname"]) : string.Empty);
								string text2 = (entity3.Contains("msnfp_emailaddress1") ? ((string)entity3["msnfp_emailaddress1"]) : string.Empty);
								string text3 = (entity3.Contains("msnfp_firstname") ? ((string)entity3["msnfp_firstname"]) : string.Empty);
								string text4 = (entity3.Contains("msnfp_lastname") ? ((string)entity3["msnfp_lastname"]) : string.Empty);
								string value = (entity3.Contains("msnfp_billing_postalcode") ? ((string)entity3["msnfp_billing_postalcode"]) : string.Empty);
								string value2 = (entity3.Contains("msnfp_billing_city") ? ((string)entity3["msnfp_billing_city"]) : string.Empty);
								string value3 = (entity3.Contains("msnfp_billing_line1") ? ((string)entity3["msnfp_billing_line1"]) : string.Empty);
								localContext.TracingService.Trace("Validating Organization Name.");
								if (!string.IsNullOrEmpty(text))
								{
									localContext.TracingService.Trace("Organization Name: " + text + ".");
									ColumnSet columnSet2 = new ColumnSet("name");
									List<Entity> list = new List<Entity>();
									QueryExpression queryExpression = new QueryExpression("account");
									queryExpression.ColumnSet = columnSet2;
									queryExpression.Criteria = new FilterExpression();
									queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
									queryExpression.Criteria.FilterOperator = LogicalOperator.And;
									FilterExpression filterExpression = queryExpression.Criteria.AddFilter(LogicalOperator.And);
									filterExpression.AddCondition("name", ConditionOperator.Equal, text);
									list = organizationService.RetrieveMultiple(queryExpression).Entities.ToList();
									if (list.Count > 0)
									{
										localContext.TracingService.Trace("Account found.");
										entity4 = list.FirstOrDefault();
										entity3["msnfp_customerid"] = new EntityReference("account", entity4.Id);
										flag = true;
									}
									else if (flag3)
									{
										localContext.TracingService.Trace("No account found, creating new record.");
										entity4 = new Entity("account");
										entity4["name"] = text;
										entity4["donotbulkemail"] = entity3.Contains("msnfp_donotbulkemail") && (bool)entity3["msnfp_donotbulkemail"];
										entity4["donotemail"] = entity3.Contains("msnfp_donotemail") && (bool)entity3["msnfp_donotemail"];
										entity4["donotphone"] = entity3.Contains("msnfp_donotphone") && (bool)entity3["msnfp_donotphone"];
										entity4["donotpostalmail"] = entity3.Contains("msnfp_donotpostalmail") && (bool)entity3["msnfp_donotpostalmail"];
										if (flag4)
										{
											entity4["donotbulkpostalmail"] = false;
										}
										else
										{
											entity4["donotbulkpostalmail"] = true;
										}
										entity4["address1_addresstypecode"] = new OptionSetValue(3);
										entity4["msnfp_anonymity"] = attributeValue;
										if (entity3.Contains("msnfp_receiptpreferencecode"))
										{
											entity4["msnfp_receiptpreferencecode"] = (OptionSetValue)entity3["msnfp_receiptpreferencecode"];
										}
										if (empty5 != string.Empty && empty6 != Guid.Empty)
										{
											localContext.TracingService.Trace("Assigning Owner");
											entity4["ownerid"] = new EntityReference(empty5, empty6);
										}
										empty = organizationService.Create(entity4);
										ExecuteWorkflowRequest executeWorkflowRequest = new ExecuteWorkflowRequest();
										executeWorkflowRequest.EntityId = empty;
										executeWorkflowRequest.WorkflowId = Guid.Parse("810C634A-2F4C-45B7-BFCC-C4FAAE315970");
										organizationService.Execute(executeWorkflowRequest);
										localContext.TracingService.Trace("Account created and giving level calculated..");
										localContext.TracingService.Trace("Account created and set as Donor.");
										if (empty != Guid.Empty)
										{
											entity3["msnfp_customerid"] = new EntityReference("account", empty);
											flag = true;
										}
									}
								}
								localContext.TracingService.Trace("Account validation completed.");
								ColumnSet columnSet3 = new ColumnSet("contactid", "firstname", "lastname", "middlename", "firstname", "birthdate", "emailaddress1", "emailaddress2", "emailaddress3", "telephone1", "mobilephone", "gendercode", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode", "msnfp_householdid", "msnfp_householdrelationship");
								List<Entity> list2 = new List<Entity>();
								QueryExpression queryExpression2 = new QueryExpression("contact");
								queryExpression2.ColumnSet = columnSet3;
								queryExpression2.Criteria = new FilterExpression();
								queryExpression2.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
								queryExpression2.Criteria.FilterOperator = LogicalOperator.And;
								if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(text4))
								{
									FilterExpression filterExpression2 = queryExpression2.Criteria.AddFilter(LogicalOperator.Or);
									filterExpression2.AddCondition("emailaddress1", ConditionOperator.Equal, text2);
									filterExpression2.AddCondition("emailaddress2", ConditionOperator.Equal, text2);
									filterExpression2.AddCondition("emailaddress3", ConditionOperator.Equal, text2);
									FilterExpression filterExpression3 = queryExpression2.Criteria.AddFilter(LogicalOperator.And);
									filterExpression3.AddCondition("firstname", ConditionOperator.BeginsWith, text3.Substring(0, 1));
									filterExpression3.AddCondition("lastname", ConditionOperator.BeginsWith, text4);
									list2 = organizationService.RetrieveMultiple(queryExpression2).Entities.ToList();
								}
								if (list2.Count > 0)
								{
									localContext.TracingService.Trace("customer by email found.");
									entity5 = list2.FirstOrDefault();
								}
								else
								{
									localContext.TracingService.Trace("No customer found by email.");
									list2 = new List<Entity>();
									FilterExpression filterExpression4 = new FilterExpression();
									if (!string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(value2) && !string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(value3))
									{
										filterExpression4.Conditions.Add(new ConditionExpression("lastname", ConditionOperator.Equal, text4));
										filterExpression4.Conditions.Add(new ConditionExpression("address1_postalcode", ConditionOperator.Equal, value));
										filterExpression4.Conditions.Add(new ConditionExpression("address1_city", ConditionOperator.Equal, value2));
										filterExpression4.Conditions.Add(new ConditionExpression("address1_line1", ConditionOperator.Equal, value3));
										filterExpression4.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.BeginsWith, text3.Substring(0, 1)));
										filterExpression4.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
										filterExpression4.FilterOperator = LogicalOperator.And;
										queryExpression2.Criteria = filterExpression4;
										list2 = organizationService.RetrieveMultiple(queryExpression2).Entities.ToList();
									}
									if (list2.Count > 0)
									{
										entity5 = list2.FirstOrDefault();
									}
									else
									{
										list2 = new List<Entity>();
										FilterExpression filterExpression5 = new FilterExpression();
										if (!string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(value3) && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(text3))
										{
											filterExpression5.Conditions.Add(new ConditionExpression("lastname", ConditionOperator.Equal, text4));
											filterExpression5.Conditions.Add(new ConditionExpression("address1_line1", ConditionOperator.Equal, value3));
											filterExpression5.Conditions.Add(new ConditionExpression("address1_postalcode", ConditionOperator.Equal, value));
											filterExpression5.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.BeginsWith, text3.Substring(0, 1)));
											filterExpression5.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
											filterExpression5.FilterOperator = LogicalOperator.And;
											queryExpression2.Criteria = filterExpression5;
											list2 = organizationService.RetrieveMultiple(queryExpression2).Entities.ToList();
										}
										if (list2.Count > 0)
										{
											entity5 = list2.FirstOrDefault();
										}
									}
								}
								localContext.Trace($"Shared variable :{localContext.PluginExecutionContext.SharedVariables.Count}");
								if (entity5 != null)
								{
									localContext.TracingService.Trace("found customer based on search criteria and set to Customer System");
									if (flag)
									{
										entity3["msnfp_constituentid"] = new EntityReference(entity5.LogicalName, entity5.Id);
										flag2 = true;
									}
									else
									{
										entity3["msnfp_customerid"] = new EntityReference(entity5.LogicalName, entity5.Id);
									}
									if (entity5.GetAttributeValue<EntityReference>("msnfp_householdid") != null)
									{
										Entity entity7 = new Entity(entity5.LogicalName, entity5.Id);
										if (keyValuePair.Key == "msnfp_householdid")
										{
											entity7["msnfp_householdid"] = (EntityReference)keyValuePair.Value;
										}
										if (entity3.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") != null)
										{
											entity7["msnfp_householdrelationship"] = entity3.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship");
										}
										organizationService.Update(entity7);
									}
								}
								else
								{
									localContext.TracingService.Trace("No record found, going to create Contact");
									string value4 = (entity3.Contains("msnfp_firstname") ? ((string)entity3["msnfp_firstname"]) : string.Empty);
									string value5 = (entity3.Contains("msnfp_lastname") ? ((string)entity3["msnfp_lastname"]) : string.Empty);
									if (entity3.Contains("msnfp_createdonor") && flag3 && !string.IsNullOrEmpty(value4) && !string.IsNullOrEmpty(value5))
									{
										entity5 = new Entity("contact");
										entity5["msnfp_householdrelationship"] = (entity3.Attributes.ContainsKey("msnfp_householdrelationship") ? entity3.GetAttributeValue<OptionSetValue>("msnfp_householdrelationship") : null);
										entity5["msnfp_householdid"] = ((keyValuePair.Key == "msnfp_householdid") ? ((EntityReference)keyValuePair.Value) : null);
										entity5["salutation"] = (entity3.Contains("msnfp_salutation") ? ((string)entity3["msnfp_salutation"]) : string.Empty);
										entity5["lastname"] = (entity3.Contains("msnfp_lastname") ? ((string)entity3["msnfp_lastname"]) : string.Empty);
										entity5["middlename"] = (entity3.Contains("msnfp_middlename") ? ((string)entity3["msnfp_middlename"]) : string.Empty);
										entity5["firstname"] = (entity3.Contains("msnfp_firstname") ? ((string)entity3["msnfp_firstname"]) : string.Empty);
										if (entity3.Contains("msnfp_birthdate"))
										{
											entity5["birthdate"] = (DateTime)entity3["msnfp_birthdate"];
										}
										entity5["emailaddress1"] = (entity3.Contains("msnfp_emailaddress1") ? ((string)entity3["msnfp_emailaddress1"]) : string.Empty);
										entity5["telephone1"] = (entity3.Contains("msnfp_telephone1") ? ((string)entity3["msnfp_telephone1"]) : string.Empty);
										entity5["mobilephone"] = (entity3.Contains("msnfp_mobilephone") ? ((string)entity3["msnfp_mobilephone"]) : string.Empty);
										entity5["address1_line1"] = (entity3.Contains("msnfp_billing_line1") ? ((string)entity3["msnfp_billing_line1"]) : string.Empty);
										entity5["address1_line2"] = (entity3.Contains("msnfp_billing_line2") ? ((string)entity3["msnfp_billing_line2"]) : string.Empty);
										entity5["address1_line3"] = (entity3.Contains("msnfp_billing_line3") ? ((string)entity3["msnfp_billing_line3"]) : string.Empty);
										entity5["address1_city"] = (entity3.Contains("msnfp_billing_city") ? ((string)entity3["msnfp_billing_city"]) : string.Empty);
										entity5["address1_stateorprovince"] = (entity3.Contains("msnfp_billing_stateorprovince") ? ((string)entity3["msnfp_billing_stateorprovince"]) : string.Empty);
										entity5["address1_country"] = (entity3.Contains("msnfp_billing_country") ? ((string)entity3["msnfp_billing_country"]) : string.Empty);
										entity5["address1_postalcode"] = (entity3.Contains("msnfp_billing_postalcode") ? ((string)entity3["msnfp_billing_postalcode"]) : string.Empty);
										entity5["donotbulkemail"] = entity3.Contains("msnfp_donotbulkemail") && (bool)entity3["msnfp_donotbulkemail"];
										entity5["donotemail"] = entity3.Contains("msnfp_donotemail") && (bool)entity3["msnfp_donotemail"];
										entity5["donotphone"] = entity3.Contains("msnfp_donotphone") && (bool)entity3["msnfp_donotphone"];
										entity5["donotpostalmail"] = entity3.Contains("msnfp_donotpostalmail") && (bool)entity3["msnfp_donotpostalmail"];
										if (flag4)
										{
											entity5["donotbulkpostalmail"] = false;
										}
										else
										{
											entity5["donotbulkpostalmail"] = true;
										}
										entity5["address1_addresstypecode"] = new OptionSetValue(844060000);
										entity5["msnfp_anonymity"] = attributeValue;
										if (entity3.Contains("msnfp_receiptpreferencecode"))
										{
											entity5["msnfp_receiptpreferencecode"] = (OptionSetValue)entity3["msnfp_receiptpreferencecode"];
										}
										if (empty5 != string.Empty && empty6 != Guid.Empty)
										{
											entity5["ownerid"] = new EntityReference(empty5, empty6);
										}
										Guid guid = organizationService.Create(entity5);
										ExecuteWorkflowRequest executeWorkflowRequest2 = new ExecuteWorkflowRequest();
										executeWorkflowRequest2.EntityId = guid;
										executeWorkflowRequest2.WorkflowId = Guid.Parse("EAAE076C-DB57-4979-A479-CC17B83CE705");
										organizationService.Execute(executeWorkflowRequest2);
										localContext.TracingService.Trace("Contact created and giving level calculated..");
										localContext.TracingService.Trace("Contact created and set to Donor");
										if (flag)
										{
											entity3["msnfp_constituentid"] = new EntityReference("contact", guid);
											flag2 = true;
										}
										else
										{
											entity3["msnfp_customerid"] = new EntityReference("contact", guid);
										}
									}
								}
								localContext.TracingService.Trace("Donation Import updated");
							}
							if (!entity3.Contains("msnfp_constituentid") && !flag2)
							{
								localContext.TracingService.Trace("Donation Import does not have constituent");
								string text5 = (entity3.Contains("msnfp_constituent_firstname") ? ((string)entity3["msnfp_constituent_firstname"]) : string.Empty);
								string text6 = (entity3.Contains("msnfp_constituent_lastname") ? ((string)entity3["msnfp_constituent_lastname"]) : string.Empty);
								ColumnSet columnSet4 = new ColumnSet("contactid", "lastname", "middlename", "firstname", "birthdate", "emailaddress1", "emailaddress2", "emailaddress3", "telephone1", "mobilephone", "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince", "address1_country", "address1_postalcode");
								List<Entity> list3 = new List<Entity>();
								QueryExpression queryExpression3 = new QueryExpression("contact");
								queryExpression3.ColumnSet = columnSet4;
								queryExpression3.Criteria = new FilterExpression();
								queryExpression3.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
								if (!string.IsNullOrEmpty(text5) && !string.IsNullOrEmpty(text6))
								{
									FilterExpression filterExpression6 = new FilterExpression();
									filterExpression6.AddCondition("firstname", ConditionOperator.Equal, text5);
									filterExpression6.AddCondition("lastname", ConditionOperator.Equal, text6);
									filterExpression6.FilterOperator = LogicalOperator.And;
									queryExpression3.Criteria = filterExpression6;
									list3 = organizationService.RetrieveMultiple(queryExpression3).Entities.ToList();
									if (list3.Count > 0)
									{
										localContext.TracingService.Trace("constituent found by first name and last name.");
										entity6 = list3.FirstOrDefault();
									}
									if (entity6 != null)
									{
										localContext.TracingService.Trace("found constituent based on search criteria and set to Constituent");
										entity3["msnfp_constituentid"] = new EntityReference(entity6.LogicalName, entity6.Id);
									}
									else
									{
										localContext.TracingService.Trace("No constituent record found, going to create new Contact");
										if (entity3.Contains("msnfp_createconstituent") && (bool)entity3["msnfp_createconstituent"])
										{
											entity6 = new Entity("contact");
											entity6["salutation"] = (entity3.Contains("msnfp_constituent_salutation") ? ((string)entity3["msnfp_constituent_salutation"]) : string.Empty);
											entity6["lastname"] = (entity3.Contains("msnfp_constituent_lastname") ? ((string)entity3["msnfp_constituent_lastname"]) : string.Empty);
											entity6["middlename"] = (entity3.Contains("msnfp_constituent_middlename") ? ((string)entity3["msnfp_constituent_middlename"]) : string.Empty);
											entity6["firstname"] = (entity3.Contains("msnfp_constituent_firstname") ? ((string)entity3["msnfp_constituent_firstname"]) : string.Empty);
											entity6["emailaddress1"] = (entity3.Contains("msnfp_constituent_emailaddress1") ? ((string)entity3["msnfp_constituent_emailaddress1"]) : string.Empty);
											entity6["telephone1"] = (entity3.Contains("msnfp_constituent_telephone") ? ((string)entity3["msnfp_constituent_telephone"]) : string.Empty);
											entity6["mobilephone"] = (entity3.Contains("msnfp_constituent_mobilephone") ? ((string)entity3["msnfp_constituent_mobilephone"]) : string.Empty);
											entity6["address1_line1"] = (entity3.Contains("msnfp_constituent_billing_line1") ? ((string)entity3["msnfp_constituent_billing_line1"]) : string.Empty);
											entity6["address1_line2"] = (entity3.Contains("msnfp_constituent_billing_line2") ? ((string)entity3["msnfp_constituent_billing_line2"]) : string.Empty);
											entity6["address1_line3"] = (entity3.Contains("msnfp_constituent_billing_line3") ? ((string)entity3["msnfp_constituent_billing_line3"]) : string.Empty);
											entity6["address1_city"] = (entity3.Contains("msnfp_constituent_billing_city") ? ((string)entity3["msnfp_constituent_billing_city"]) : string.Empty);
											entity6["address1_stateorprovince"] = (entity3.Contains("msnfp_constituent_billing_stateorprovince") ? ((string)entity3["msnfp_constituent_billing_stateorprovince"]) : string.Empty);
											entity6["address1_country"] = (entity3.Contains("msnfp_constituent_billing_country") ? ((string)entity3["msnfp_constituent_billing_country"]) : string.Empty);
											entity6["address1_postalcode"] = (entity3.Contains("msnfp_constituent_billing_postalcode") ? ((string)entity3["msnfp_constituent_billing_postalcode"]) : string.Empty);
											entity6["donotbulkemail"] = entity3.Contains("msnfp_constituent_donotbulkemail") && (bool)entity3["msnfp_constituent_donotbulkemail"];
											entity6["donotemail"] = entity3.Contains("msnfp_constituent_donotemail") && (bool)entity3["msnfp_constituent_donotemail"];
											entity6["donotphone"] = entity3.Contains("msnfp_constituent_donotphone") && (bool)entity3["msnfp_constituent_donotphone"];
											entity6["donotpostalmail"] = entity3.Contains("msnfp_constituent_donotpostalmail") && (bool)entity3["msnfp_constituent_donotpostalmail"];
											if (flag5)
											{
												entity6["donotbulkpostalmail"] = false;
											}
											else
											{
												entity6["donotbulkpostalmail"] = true;
											}
											entity6["address1_addresstypecode"] = new OptionSetValue(844060000);
											entity6["msnfp_anonymity"] = attributeValue;
											if (entity3.Contains("msnfp_receiptpreferencecode"))
											{
												entity6["msnfp_receiptpreferencecode"] = (OptionSetValue)entity3["msnfp_receiptpreferencecode"];
											}
											if (empty5 != string.Empty && empty6 != Guid.Empty)
											{
												entity6["ownerid"] = new EntityReference(empty5, empty6);
											}
											Guid id = organizationService.Create(entity6);
											localContext.TracingService.Trace("constituent created and set to Constituent");
											entity3["msnfp_constituentid"] = new EntityReference("contact", id);
										}
									}
									localContext.TracingService.Trace("Donation Import updated with Constituent");
								}
							}
							Entity entity8 = new Entity("msnfp_transaction");
							if (entity3.Contains("msnfp_customerid"))
							{
								localContext.TracingService.Trace("Donation Import contains customer");
								string logicalName = ((EntityReference)entity3["msnfp_customerid"]).LogicalName;
								Guid id2 = ((EntityReference)entity3["msnfp_customerid"]).Id;
								entity8["msnfp_customerid"] = new EntityReference(logicalName, id2);
							}
							if (entity3.Contains("msnfp_constituentid"))
							{
								localContext.TracingService.Trace("Donation Import contains constituent");
								string logicalName2 = ((EntityReference)entity3["msnfp_constituentid"]).LogicalName;
								Guid id3 = ((EntityReference)entity3["msnfp_constituentid"]).Id;
								entity8["msnfp_relatedconstituentid"] = new EntityReference(logicalName2, id3);
							}
							if (entity3.Contains("msnfp_amount_receiptable"))
							{
								entity8["msnfp_amount_receipted"] = (Money)entity3["msnfp_amount_receiptable"];
							}
							if (entity3.Contains("msnfp_chequewiredate"))
							{
								entity8["msnfp_chequewiredate"] = (DateTime)entity3["msnfp_chequewiredate"];
							}
							if (entity3.Contains("msnfp_bookdate"))
							{
								entity8["msnfp_bookdate"] = (DateTime)entity3["msnfp_bookdate"];
							}
							if (entity3.Contains("msnfp_paymenttypecode"))
							{
								entity8["msnfp_paymenttypecode"] = (OptionSetValue)entity3["msnfp_paymenttypecode"];
							}
							if (entity3.Contains("msnfp_dataentrysource"))
							{
								entity8["msnfp_dataentrysource"] = (OptionSetValue)entity3["msnfp_dataentrysource"];
							}
							if (entity3.Contains("msnfp_amount_membership"))
							{
								entity8["msnfp_amount_membership"] = (Money)entity3["msnfp_amount_membership"];
							}
							if (entity3.Contains("msnfp_amount_nonreceiptable"))
							{
								entity8["msnfp_amount_nonreceiptable"] = (Money)entity3["msnfp_amount_nonreceiptable"];
							}
							if (entity3.Contains("msnfp_amount_tax"))
							{
								entity8["msnfp_amount_tax"] = (Money)entity3["msnfp_amount_tax"];
							}
							if (entity3.Contains("msnfp_ccbrandcode"))
							{
								entity8["msnfp_ccbrandcode"] = (OptionSetValue)entity3["msnfp_ccbrandcode"];
							}
							if (entity3.Contains("msnfp_appealid"))
							{
								string logicalName3 = ((EntityReference)entity3["msnfp_appealid"]).LogicalName;
								Guid id4 = ((EntityReference)entity3["msnfp_appealid"]).Id;
								entity8["msnfp_appealid"] = new EntityReference(logicalName3, id4);
							}
							if (entity3.Contains("msnfp_originatingcampaignid"))
							{
								localContext.TracingService.Trace("Donation Import contains campaign");
								string logicalName4 = ((EntityReference)entity3["msnfp_originatingcampaignid"]).LogicalName;
								Guid id5 = ((EntityReference)entity3["msnfp_originatingcampaignid"]).Id;
								entity8["msnfp_originatingcampaignid"] = new EntityReference(logicalName4, id5);
							}
							if (entity3.Contains("msnfp_amount"))
							{
								entity8["msnfp_amount"] = (Money)entity3["msnfp_amount"];
							}
							if (entity3.Contains("msnfp_packageid"))
							{
								localContext.TracingService.Trace("Donation Import contains package");
								string logicalName5 = ((EntityReference)entity3["msnfp_packageid"]).LogicalName;
								Guid id6 = ((EntityReference)entity3["msnfp_packageid"]).Id;
								entity8["msnfp_packageid"] = new EntityReference(logicalName5, id6);
							}
							if (entity3.Contains("msnfp_designationid"))
							{
								localContext.TracingService.Trace("Donation Import contains fund");
								string logicalName6 = ((EntityReference)entity3["msnfp_designationid"]).LogicalName;
								Guid id7 = ((EntityReference)entity3["msnfp_designationid"]).Id;
								entity8["msnfp_designationid"] = new EntityReference(logicalName6, id7);
							}
							entity8["msnfp_transactionidentifier"] = (entity3.Contains("msnfp_transactionidentifier") ? ((string)entity3["msnfp_transactionidentifier"]) : string.Empty);
							entity8["msnfp_dataentryreference"] = (entity3.Contains("msnfp_dataentryreference") ? ((string)entity3["msnfp_dataentryreference"]) : string.Empty);
							entity8["msnfp_chequenumber"] = (entity3.Contains("msnfp_chequenumber") ? ((string)entity3["msnfp_chequenumber"]) : string.Empty);
							if (entity3.Contains("msnfp_receiveddate"))
							{
								entity8["msnfp_depositdate"] = (DateTime)entity3["msnfp_receiveddate"];
							}
							entity8["msnfp_firstname"] = (entity3.Contains("msnfp_firstname") ? ((string)entity3["msnfp_firstname"]) : string.Empty);
							entity8["msnfp_lastname"] = (entity3.Contains("msnfp_lastname") ? ((string)entity3["msnfp_lastname"]) : string.Empty);
							entity8["msnfp_billing_line1"] = (entity3.Contains("msnfp_billing_line1") ? ((string)entity3["msnfp_billing_line1"]) : string.Empty);
							entity8["msnfp_billing_line2"] = (entity3.Contains("msnfp_billing_line2") ? ((string)entity3["msnfp_billing_line2"]) : string.Empty);
							entity8["msnfp_billing_line3"] = (entity3.Contains("msnfp_billing_line3") ? ((string)entity3["msnfp_billing_line3"]) : string.Empty);
							entity8["msnfp_billing_city"] = (entity3.Contains("msnfp_billing_city") ? ((string)entity3["msnfp_billing_city"]) : string.Empty);
							entity8["msnfp_billing_stateorprovince"] = (entity3.Contains("msnfp_billing_stateorprovince") ? ((string)entity3["msnfp_billing_stateorprovince"]) : string.Empty);
							entity8["msnfp_billing_postalcode"] = (entity3.Contains("msnfp_billing_postalcode") ? ((string)entity3["msnfp_billing_postalcode"]) : string.Empty);
							entity8["msnfp_billing_country"] = (entity3.Contains("msnfp_billing_country") ? ((string)entity3["msnfp_billing_country"]) : string.Empty);
							entity8["msnfp_emailaddress1"] = (entity3.Contains("msnfp_emailaddress1") ? ((string)entity3["msnfp_emailaddress1"]) : string.Empty);
							entity8["msnfp_telephone1"] = (entity3.Contains("msnfp_telephone1") ? ((string)entity3["msnfp_telephone1"]) : string.Empty);
							entity8["msnfp_anonymous"] = attributeValue;
							entity8["msnfp_chargeoncreate"] = false;
							if (empty5 != string.Empty && empty6 != Guid.Empty)
							{
								entity8["ownerid"] = new EntityReference(empty5, empty6);
							}
							entity8["statuscode"] = new OptionSetValue(844060000);
							entity8["msnfp_configurationid"] = entity3.GetAttributeValue<EntityReference>("msnfp_configurationid");
							Guid id8 = organizationService.Create(entity8);
							entity3["msnfp_transactionid"] = new EntityReference("msnfp_transaction", id8);
						}
						catch (Exception ex)
						{
							localContext.TracingService.Trace("error : " + ex.Message);
							entity3["msnfp_statusupdated"] = new OptionSetValue(844060001);
							localContext.TracingService.Trace("Status code updated to failed");
						}
						organizationService.Update(entity3);
					}
				}
			}
			localContext.TracingService.Trace("---------Exiting DonationImportCreate.cs---------");
		}

		private KeyValuePair<string, object> InitiateHouseholdProcess(IOrganizationService service, Entity donationImport, EntityReference contactRef)
		{
			if (donationImport.GetAttributeValue<EntityReference>("msnfp_householdid") != null)
			{
				return new KeyValuePair<string, object>("msnfp_householdid", donationImport.GetAttributeValue<EntityReference>("msnfp_householdid"));
			}
			EntityReference entityReference = Utilities.SearchHousehold(service, donationImport, contactRef);
			if (entityReference != null)
			{
				return new KeyValuePair<string, object>("msnfp_householdid", entityReference);
			}
			return new KeyValuePair<string, object>("DonationImport", donationImport);
		}
	}
}
