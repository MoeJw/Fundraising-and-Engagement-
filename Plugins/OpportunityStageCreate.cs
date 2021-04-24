using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins
{
	public class OpportunityStageCreate : PluginBase
	{
		public OpportunityStageCreate(string unsecure, string secure)
			: base(typeof(OpportunityStageCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered CreateUpdateOpportunityStage.cs---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(organizationService);
			Entity entity = null;
			string messageName = pluginExecutionContext.MessageName;
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering CreateUpdateOpportunityStage.cs Main Function---------");
				Entity entity2 = (Entity)pluginExecutionContext.InputParameters["Target"];
				Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
				Entity entity3 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
				if (entity3 == null)
				{
					throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
				}
				ColumnSet columnSet = new ColumnSet("opportunityid", "stepname", "customerid", "statuscode", "statecode", "processid");
				entity = organizationService.Retrieve("opportunity", entity2.Id, columnSet);
				if (entity == null)
				{
					localContext.TracingService.Trace("The variable queriedEntityRecord is null. Cannot create opportunity stage without an associated opportunity. Exiting Plugin.");
					throw new ArgumentNullException("queriedEntityRecord");
				}
				if (GetLatestOpportunityStageForThisOpportunity(entity2.Id, localContext, orgSvcContext) == null)
				{
					localContext.TracingService.Trace("No opportunity stages found, creating first stage.");
					CreateBrandNewOpportunityStageForOpportunity(entity, localContext, orgSvcContext, organizationService);
				}
				else
				{
					bool flag = false;
					if (CompareThisStageToLastOpportunityStage(entity, localContext, orgSvcContext))
					{
						localContext.TracingService.Trace("Different stage detected, closing out last stage and creating new one.");
						CreateNextOpportunityStageForOpportunity(entity, localContext, orgSvcContext, organizationService);
					}
					else
					{
						localContext.TracingService.Trace("Same stage detected, updating stage information.");
						UpdateExistingOpportunityStage(entity, localContext, orgSvcContext);
					}
				}
			}
			localContext.TracingService.Trace("---------Exiting CreateUpdateOpportunityStage.cs---------");
		}

		private Entity GetLatestOpportunityStageForThisOpportunity(Guid opportunityId, LocalPluginContext localContext, OrganizationServiceContext orgSvcContext)
		{
			localContext.TracingService.Trace("---------Entering GetLatestOpportunityStageForThisOpportunity()---------");
			List<Entity> list = (from s in orgSvcContext.CreateQuery("msnfp_opportunitystage")
				where ((EntityReference)s["msnfp_opportunityid"]).Id == opportunityId && (DateTime)(DateTime?)s.GetAttributeValue<DateTime>("msnfp_finishedon") == null
				select s).ToList();
			localContext.TracingService.Trace("opportunityStages.Count = " + list.Count);
			if (list.Count > 0)
			{
				localContext.TracingService.Trace("Order list by created on");
				list = list.OrderByDescending((Entity s) => s["createdon"]).ToList();
				localContext.TracingService.Trace("Opportunity Stage Id = " + list[0].Id.ToString());
				localContext.TracingService.Trace("Opportunity Stage Stage Name = " + list[0]["msnfp_stagename"].ToString());
				localContext.TracingService.Trace("---------Exiting GetLatestOpportunityStageForThisOpportunity()---------");
				return list[0];
			}
			localContext.TracingService.Trace("No opportunity stage found. Exiting function.");
			localContext.TracingService.Trace("---------Exiting GetLatestOpportunityStageForThisOpportunity()---------");
			return null;
		}

		private bool CompareThisStageToLastOpportunityStage(Entity opportunityRecord, LocalPluginContext localContext, OrganizationServiceContext orgSvcContext)
		{
			localContext.TracingService.Trace("---------Entering CompareThisStageToLastOpportunityStage()---------");
			bool flag = false;
			List<Entity> list = (from s in orgSvcContext.CreateQuery("msnfp_opportunitystage")
				where ((EntityReference)s["msnfp_opportunityid"]).Id == opportunityRecord.Id && (DateTime)(DateTime?)s.GetAttributeValue<DateTime>("msnfp_finishedon") == null
				select s).ToList();
			localContext.TracingService.Trace("opportunityStages.Count = " + list.Count);
			flag = false;
			if (list.Count > 0)
			{
				string[] array = ((string)opportunityRecord["stepname"]).Split('-');
				string text = "";
				text = ((array.Length > 1) ? array[1] : ((string)opportunityRecord["stepname"]));
				localContext.TracingService.Trace("opportunityRecord[stepname] = " + text);
				localContext.TracingService.Trace("opportunityStages[0][msnfp_stagename] = " + list[0]["msnfp_stagename"].ToString());
				localContext.TracingService.Trace("Equal = " + (text == (string)list[0]["msnfp_stagename"]));
				flag = ((!(text == (string)list[0]["msnfp_stagename"])) ? true : false);
			}
			else
			{
				localContext.TracingService.Trace("No opportunity stages exist for this opportunity.");
				flag = true;
			}
			localContext.TracingService.Trace("Different Stage Detected = " + flag);
			localContext.TracingService.Trace("---------Exiting CompareThisStageToLastOpportunityStage()---------");
			return flag;
		}

		private Guid CreateBrandNewOpportunityStageForOpportunity(Entity opportunityRecord, LocalPluginContext localContext, OrganizationServiceContext orgSvcContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------Entering CreateBrandNewOpportunityStageForOpportunity()---------");
			if (opportunityRecord == null)
			{
				localContext.TracingService.Trace("The variable opportunityRecord is null. Cannot create opportunity stage without an associated opportunity. Exiting Plugin.");
				throw new ArgumentNullException("opportunityRecord");
			}
			Entity entity = new Entity("msnfp_opportunitystage");
			string[] array = (opportunityRecord.Contains("stepname") ? ((string)opportunityRecord["stepname"]).Split('-') : new string[0]);
			string text = "";
			text = ((array.Length <= 1 && opportunityRecord.Contains("stepname")) ? ((string)opportunityRecord["stepname"]) : ((array.Length != 2) ? "Qualify" : array[1]));
			localContext.TracingService.Trace("Pipeline Step Name: " + text);
			entity["msnfp_stagename"] = text;
			entity["msnfp_identifier"] = text + " - " + DateTime.Now.ToShortDateString();
			entity["msnfp_startedon"] = DateTime.UtcNow;
			entity["msnfp_appointments"] = 0;
			entity["msnfp_emails"] = 0;
			entity["msnfp_letters"] = 0;
			entity["msnfp_phonecalls"] = 0;
			entity["msnfp_tasks"] = 0;
			entity["msnfp_totalactivities"] = 0;
			entity["msnfp_opportunityid"] = new EntityReference("opportunity", opportunityRecord.Id);
			localContext.TracingService.Trace("Creating Opportunity Stage.");
			Guid result = service.Create(entity);
			localContext.TracingService.Trace("Opportunity Stage created with Id: " + result.ToString());
			localContext.TracingService.Trace("---------Exiting CreateBrandNewOpportunityStageForOpportunity()---------");
			return result;
		}

		private Guid CreateNextOpportunityStageForOpportunity(Entity opportunityRecord, LocalPluginContext localContext, OrganizationServiceContext orgSvcContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("---------Entering CreateNextOpportunityStageForOpportunity()---------");
			if (opportunityRecord == null)
			{
				localContext.TracingService.Trace("The variable opportunityRecord is null. Cannot create opportunity stage without an associated opportunity. Exiting Plugin.");
				throw new ArgumentNullException("opportunityRecord");
			}
			Entity entity = new Entity("msnfp_opportunitystage");
			Entity latestOpportunityStageForThisOpportunity = GetLatestOpportunityStageForThisOpportunity(opportunityRecord.Id, localContext, orgSvcContext);
			if (latestOpportunityStageForThisOpportunity == null)
			{
				localContext.TracingService.Trace("Error: There is no last opportunity stage but one was attempted to be retrieved. Exiting plugin.");
				throw new ArgumentNullException("lastOpportunityStage");
			}
			localContext.TracingService.Trace("Updating opportunity stage name: " + (string)latestOpportunityStageForThisOpportunity["msnfp_stagename"]);
			string[] array = ((string)opportunityRecord["stepname"]).Split('-');
			string text = "";
			text = ((array.Length > 1) ? array[1] : ((string)opportunityRecord["stepname"]));
			localContext.TracingService.Trace("Pipeline Step Name: " + text);
			if (text == (string)latestOpportunityStageForThisOpportunity["msnfp_stagename"])
			{
				localContext.TracingService.Trace("Stage names are both: " + text);
			}
			localContext.TracingService.Trace("Getting Appointments");
			DateTime started = (latestOpportunityStageForThisOpportunity.Attributes.ContainsKey("msnfp_startedon") ? latestOpportunityStageForThisOpportunity.GetAttributeValue<DateTime>("msnfp_startedon") : DateTime.Now);
			List<Guid> list = (from s in orgSvcContext.CreateQuery("appointment")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Appointments: " + list.Count);
			localContext.TracingService.Trace("Getting Emails");
			List<Guid> list2 = (from s in orgSvcContext.CreateQuery("email")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Emails: " + list2.Count);
			localContext.TracingService.Trace("Getting Letters");
			List<Guid> list3 = (from s in orgSvcContext.CreateQuery("letter")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Letters: " + list3.Count);
			localContext.TracingService.Trace("Getting Phonecalls");
			List<Guid> list4 = (from s in orgSvcContext.CreateQuery("phonecall")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Phonecalls: " + list4.Count);
			localContext.TracingService.Trace("Getting Tasks");
			List<Guid> list5 = (from s in orgSvcContext.CreateQuery("task")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Tasks: " + list5.Count);
			latestOpportunityStageForThisOpportunity["msnfp_finishedon"] = DateTime.UtcNow;
			if (latestOpportunityStageForThisOpportunity.Attributes.ContainsKey("msnfp_startedon"))
			{
				localContext.TracingService.Trace("Days in stage: " + Math.Round((DateTime.Now - (DateTime)latestOpportunityStageForThisOpportunity["msnfp_startedon"]).TotalDays, MidpointRounding.AwayFromZero));
				latestOpportunityStageForThisOpportunity["msnfp_daysinstage"] = (int)Math.Round((DateTime.Now - (DateTime)latestOpportunityStageForThisOpportunity["msnfp_startedon"]).TotalDays, MidpointRounding.AwayFromZero);
			}
			latestOpportunityStageForThisOpportunity["msnfp_appointments"] = list.Count;
			latestOpportunityStageForThisOpportunity["msnfp_emails"] = list2.Count;
			latestOpportunityStageForThisOpportunity["msnfp_letters"] = list3.Count;
			latestOpportunityStageForThisOpportunity["msnfp_phonecalls"] = list4.Count;
			latestOpportunityStageForThisOpportunity["msnfp_tasks"] = list5.Count;
			latestOpportunityStageForThisOpportunity["msnfp_totalactivities"] = list.Count + list2.Count + list3.Count + list4.Count + list5.Count;
			localContext.TracingService.Trace("Saving changes to previous stage: " + (string)latestOpportunityStageForThisOpportunity["msnfp_stagename"]);
			orgSvcContext.UpdateObject(latestOpportunityStageForThisOpportunity);
			orgSvcContext.SaveChanges();
			localContext.TracingService.Trace("Update complete. Previous Stage End Date: " + DateTime.Now.ToString());
			localContext.TracingService.Trace("Creating New Opportunity Stage with name: " + text);
			entity["msnfp_stagename"] = text;
			entity["msnfp_identifier"] = text + " - " + DateTime.Now.ToShortDateString();
			entity["msnfp_startedon"] = DateTime.Now;
			entity["msnfp_appointments"] = 0;
			entity["msnfp_emails"] = 0;
			entity["msnfp_letters"] = 0;
			entity["msnfp_phonecalls"] = 0;
			entity["msnfp_tasks"] = 0;
			entity["msnfp_totalactivities"] = 0;
			entity["msnfp_opportunityid"] = new EntityReference("opportunity", opportunityRecord.Id);
			Guid result = service.Create(entity);
			localContext.TracingService.Trace("Opportunity Stage created with Id: " + result.ToString());
			localContext.TracingService.Trace("---------Exiting CreateNextOpportunityStageForOpportunity()---------");
			return result;
		}

		private void UpdateExistingOpportunityStage(Entity opportunityRecord, LocalPluginContext localContext, OrganizationServiceContext orgSvcContext)
		{
			localContext.TracingService.Trace("---------Entering UpdateExistingOpportunityStage()---------");
			if (opportunityRecord == null)
			{
				localContext.TracingService.Trace("The variable opportunityRecord is null. Cannot update opportunity stage without an associated opportunity. Exiting Plugin.");
				throw new ArgumentNullException("opportunityRecord");
			}
			Entity latestOpportunityStageForThisOpportunity = GetLatestOpportunityStageForThisOpportunity(opportunityRecord.Id, localContext, orgSvcContext);
			if (latestOpportunityStageForThisOpportunity == null)
			{
				localContext.TracingService.Trace("The variable opportunityStage is null. Cannot update the opportunity stage as it is not found. Exiting Plugin.");
				throw new ArgumentNullException("opportunityStage");
			}
			DateTime started = (latestOpportunityStageForThisOpportunity.Attributes.ContainsKey("msnfp_startedon") ? latestOpportunityStageForThisOpportunity.GetAttributeValue<DateTime>("msnfp_startedon") : DateTime.Now);
			List<Guid> list = (from s in orgSvcContext.CreateQuery("appointment")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Appointments: " + list.Count);
			localContext.TracingService.Trace("Getting Emails");
			List<Guid> list2 = (from s in orgSvcContext.CreateQuery("email")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Emails: " + list2.Count);
			localContext.TracingService.Trace("Getting Letters");
			List<Guid> list3 = (from s in orgSvcContext.CreateQuery("letter")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Letters: " + list3.Count);
			localContext.TracingService.Trace("Getting Phonecalls");
			List<Guid> list4 = (from s in orgSvcContext.CreateQuery("phonecall")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Phonecalls: " + list4.Count);
			localContext.TracingService.Trace("Getting Tasks");
			List<Guid> list5 = (from s in orgSvcContext.CreateQuery("task")
				where ((EntityReference)s["regardingobjectid"]).Id == opportunityRecord.Id && (DateTime)s["createdon"] >= started
				select s.Id).ToList();
			localContext.TracingService.Trace("Got Tasks: " + list5.Count);
			if (latestOpportunityStageForThisOpportunity.Attributes.ContainsKey("msnfp_startedon"))
			{
				localContext.TracingService.Trace("Days in stage: " + Math.Round((DateTime.Now - (DateTime)latestOpportunityStageForThisOpportunity["msnfp_startedon"]).TotalDays, MidpointRounding.AwayFromZero));
				latestOpportunityStageForThisOpportunity["msnfp_daysinstage"] = (int)Math.Round((DateTime.Now - (DateTime)latestOpportunityStageForThisOpportunity["msnfp_startedon"]).TotalDays, MidpointRounding.AwayFromZero);
			}
			latestOpportunityStageForThisOpportunity["msnfp_appointments"] = list.Count;
			latestOpportunityStageForThisOpportunity["msnfp_emails"] = list2.Count;
			latestOpportunityStageForThisOpportunity["msnfp_letters"] = list3.Count;
			latestOpportunityStageForThisOpportunity["msnfp_phonecalls"] = list4.Count;
			latestOpportunityStageForThisOpportunity["msnfp_tasks"] = list5.Count;
			latestOpportunityStageForThisOpportunity["msnfp_totalactivities"] = list.Count + list2.Count + list3.Count + list4.Count + list5.Count;
			localContext.TracingService.Trace("Opportunity statecode: " + ((OptionSetValue)opportunityRecord["statecode"]).Value);
			if (((OptionSetValue)opportunityRecord["statecode"]).Value != 0)
			{
				localContext.TracingService.Trace("Setting finish date to now as the opportunity status is inactive.");
				latestOpportunityStageForThisOpportunity["msnfp_finishedon"] = DateTime.UtcNow;
			}
			localContext.TracingService.Trace("Updating Opportunity Stage.");
			orgSvcContext.UpdateObject(latestOpportunityStageForThisOpportunity);
			orgSvcContext.SaveChanges();
			localContext.TracingService.Trace("Updated Opportunity Stage.");
			localContext.TracingService.Trace("---------Exiting UpdateExistingOpportunityStage()---------");
		}
	}
}
