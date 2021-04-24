using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using FundraisingandEngagement.StripeWebPayment.Model;
using FundraisingandEngagement.StripeWebPayment.Service;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Moneris;
using Plugins.AzureModels;
using Plugins.PaymentProcesses;

namespace Plugins
{
	public class RefundCreate : PluginBase
	{
		public RefundCreate(string unsecure, string secure)
			: base(typeof(RefundCreate))
		{
		}

		protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
		{
			if (localContext == null)
			{
				throw new ArgumentNullException("localContext");
			}
			localContext.TracingService.Trace("---------Triggered RefundCreate.cs ---------");
			IPluginExecutionContext pluginExecutionContext = localContext.PluginExecutionContext;
			IOrganizationService organizationService = localContext.OrganizationService;
			OrganizationServiceContext organizationServiceContext = new OrganizationServiceContext(organizationService);
			Entity queriedEntityRecord = null;
			Entity entity = null;
			string messageName = pluginExecutionContext.MessageName;
			Guid initiatingUserId = pluginExecutionContext.InitiatingUserId;
			Entity entity2 = organizationService.Retrieve("systemuser", initiatingUserId, new ColumnSet("msnfp_configurationid"));
			if (entity2 == null)
			{
				throw new Exception("No user id found. Please ensure the user is valid. Exiting plugin.");
			}
			if (pluginExecutionContext.Depth > 1)
			{
				localContext.TracingService.Trace("Context.depth > 1. Exiting Plugin.");
				return;
			}
			entity = Utilities.GetConfigurationRecordByUser(pluginExecutionContext, organizationService, localContext.TracingService);
			if (!pluginExecutionContext.InputParameters.Contains("Target"))
			{
				return;
			}
			if (pluginExecutionContext.InputParameters["Target"] is Entity)
			{
				localContext.TracingService.Trace("---------Entering RefundCreate.cs Main Function---------");
				Entity entity3 = (Entity)pluginExecutionContext.InputParameters["Target"];
				if (entity3.Contains("msnfp_transactionid"))
				{
					localContext.TracingService.Trace("Refund has an associated Transaction: " + ((EntityReference)entity3["msnfp_transactionid"]).Id.ToString());
					Guid id = ((EntityReference)entity3["msnfp_transactionid"]).Id;
					ColumnSet columnSet = new ColumnSet("msnfp_transactionid", "msnfp_customerid", "msnfp_amount", "msnfp_amount_receipted", "msnfp_paymenttypecode", "msnfp_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_amount_tax", "msnfp_ref_amount", "msnfp_ref_amount_receipted", "msnfp_ref_amount_membership", "msnfp_ref_amount_nonreceiptable", "msnfp_ref_amount_tax", "msnfp_configurationid", "msnfp_transactionidentifier", "msnfp_transaction_paymentmethodid", "modifiedby", "statuscode", "msnfp_daterefunded", "msnfp_transactionresult", "msnfp_paymentprocessorid", "msnfp_invoiceidentifier", "msnfp_transactionnumber", "transactioncurrencyid");
					Entity gift = organizationService.Retrieve("msnfp_transaction", id, columnSet);
					if (entity3.Contains("msnfp_refundtypecode"))
					{
						Entity entity4 = null;
						if (gift.Contains("msnfp_configurationid"))
						{
							localContext.TracingService.Trace("Gift contains Configuration.");
							entity4 = (from c in organizationServiceContext.CreateQuery("msnfp_configuration")
								where (Guid)c["msnfp_configurationid"] == ((EntityReference)gift["msnfp_configurationid"]).Id
								orderby c["createdon"] descending
								select c).FirstOrDefault();
							localContext.TracingService.Trace("Configuration retrieved");
						}
						else
						{
							entity4 = entity;
						}
						if (entity3.Contains("msnfp_refundtypecode") && (((OptionSetValue)entity3["msnfp_refundtypecode"]).Value == 844060002 || ((OptionSetValue)entity3["msnfp_refundtypecode"]).Value == 844060003))
						{
							localContext.TracingService.Trace("Refund type Credit Card or Bank Account. Refund Type: " + ((OptionSetValue)entity3["msnfp_refundtypecode"]).Value);
							if (gift.Contains("msnfp_paymentprocessorid"))
							{
								localContext.TracingService.Trace("Getting the payment processor from the transaction record.");
								Entity entity5 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)gift["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentprocessorid", "msnfp_apikey", "msnfp_storeid", "msnfp_testmode", "msnfp_paymentgatewaytype", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword"));
								localContext.TracingService.Trace("Retrieved payment processor: " + ((Guid)entity5["msnfp_paymentprocessorid"]).ToString());
								if (((OptionSetValue)entity5["msnfp_paymentgatewaytype"]).Value == 844060000)
								{
									localContext.TracingService.Trace("Payment gateway Moneris.");
									RefundMonerisTransaction(gift, entity5, entity3, entity4, localContext, organizationService);
								}
								else if (((OptionSetValue)entity5["msnfp_paymentgatewaytype"]).Value == 844060001)
								{
									localContext.TracingService.Trace("Payment gateway Stripe.");
									RefundStripeTransaction(gift, entity5, entity3, entity4, localContext, organizationService);
								}
								else
								{
									if (((OptionSetValue)entity5["msnfp_paymentgatewaytype"]).Value != 844060002)
									{
										localContext.TracingService.Trace("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
										throw new Exception("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
									}
									localContext.TracingService.Trace("Payment gateway iATS.");
									RefundIatsTransaction(gift, entity5, entity3, entity4, localContext, organizationService);
								}
							}
							else
							{
								localContext.TracingService.Trace("No payment processor found on this transaction. Attempting to use Configuration Payment Processor.");
								if (!entity4.Contains("msnfp_paymentprocessorid"))
								{
									localContext.TracingService.Trace("No payment processor found on this transaction or in the configuration. Failing refund. Aborting Plugin.");
									throw new Exception("Refund Failed: No payment processor found on this transaction or in the configuration. Please set the payment processor/gateway on the transaction or configuration, save and try the refund process again.");
								}
								localContext.TracingService.Trace("Getting the payment processor from the configuration record.");
								Entity entity6 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)entity4["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentprocessorid", "msnfp_apikey", "msnfp_storeid", "msnfp_testmode", "msnfp_paymentgatewaytype", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword"));
								localContext.TracingService.Trace("Retrieved payment processor: " + ((Guid)entity6["msnfp_paymentprocessorid"]).ToString());
								if (((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value == 844060000)
								{
									localContext.TracingService.Trace("Payment gateway Moneris.");
									RefundMonerisTransaction(gift, entity6, entity3, entity4, localContext, organizationService);
								}
								else if (((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value == 844060001)
								{
									localContext.TracingService.Trace("Payment gateway Stripe.");
									RefundStripeTransaction(gift, entity6, entity3, entity4, localContext, organizationService);
								}
								else
								{
									if (((OptionSetValue)entity6["msnfp_paymentgatewaytype"]).Value != 844060002)
									{
										localContext.TracingService.Trace("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
										throw new Exception("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
									}
									localContext.TracingService.Trace("Payment gateway iATS.");
									RefundIatsTransaction(gift, entity6, entity3, entity4, localContext, organizationService);
								}
							}
						}
						else
						{
							localContext.TracingService.Trace("Refund is not credit/debit, so we don't use a payment gateway and update just the refund/transaction records.");
							SetTransactionAsRefunded(gift, entity3, localContext, organizationService);
						}
					}
					else
					{
						localContext.TracingService.Trace("Warning: No refund type code. This should not occur on create.");
					}
				}
				else if (entity3.Contains("msnfp_paymentid"))
				{
					localContext.TracingService.Trace("Refund has an associated Payment: " + ((EntityReference)entity3["msnfp_paymentid"]).Id.ToString());
					Guid id2 = ((EntityReference)entity3["msnfp_paymentid"]).Id;
					ColumnSet columnSet2 = new ColumnSet("msnfp_paymentid", "msnfp_customerid", "msnfp_eventpackageid", "msnfp_amount", "msnfp_amount_balance", "msnfp_paymenttype", "msnfp_amount_refunded", "msnfp_configurationid", "msnfp_transactionidentifier", "msnfp_paymentmethodid", "modifiedby", "statuscode", "msnfp_daterefunded", "msnfp_transactionresult", "msnfp_paymentprocessorid", "msnfp_invoiceidentifier", "msnfp_transactionnumber", "transactioncurrencyid");
					Entity payment = organizationService.Retrieve("msnfp_payment", id2, columnSet2);
					if (entity3.Contains("msnfp_refundtypecode"))
					{
						Entity entity7 = null;
						if (payment.Contains("msnfp_configurationid"))
						{
							localContext.TracingService.Trace("Payment contains Configuration.");
							entity7 = (from c in organizationServiceContext.CreateQuery("msnfp_configuration")
								where (Guid)c["msnfp_configurationid"] == ((EntityReference)payment["msnfp_configurationid"]).Id
								orderby c["createdon"] descending
								select c).FirstOrDefault();
							localContext.TracingService.Trace("Configuration retrieved");
						}
						else
						{
							entity7 = entity;
						}
						if (entity3.Contains("msnfp_refundtypecode") && (((OptionSetValue)entity3["msnfp_refundtypecode"]).Value == 844060002 || ((OptionSetValue)entity3["msnfp_refundtypecode"]).Value == 844060003))
						{
							localContext.TracingService.Trace("Refund type Credit Card or Bank Account. Refund Type: " + ((OptionSetValue)entity3["msnfp_refundtypecode"]).Value);
							if (payment.Contains("msnfp_paymentprocessorid"))
							{
								localContext.TracingService.Trace("Getting the payment processor from the payment record.");
								Entity entity8 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)payment["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentprocessorid", "msnfp_apikey", "msnfp_storeid", "msnfp_testmode", "msnfp_paymentgatewaytype", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword"));
								localContext.TracingService.Trace("Retrieved payment processor: " + ((Guid)entity8["msnfp_paymentprocessorid"]).ToString());
								if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060000)
								{
									localContext.TracingService.Trace("Payment gateway Moneris.");
									RefundMonerisTransaction(payment, entity8, entity3, entity7, localContext, organizationService);
								}
								else if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value == 844060001)
								{
									localContext.TracingService.Trace("Payment gateway Stripe.");
									RefundStripeTransaction(payment, entity8, entity3, entity7, localContext, organizationService);
								}
								else
								{
									if (((OptionSetValue)entity8["msnfp_paymentgatewaytype"]).Value != 844060002)
									{
										localContext.TracingService.Trace("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
										throw new Exception("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
									}
									localContext.TracingService.Trace("Payment gateway iATS.");
									RefundIatsTransaction(payment, entity8, entity3, entity7, localContext, organizationService);
								}
							}
							else
							{
								localContext.TracingService.Trace("No payment processor found on this transaction. Attempting to use Configuration Payment Processor.");
								if (!entity7.Contains("msnfp_paymentprocessorid"))
								{
									localContext.TracingService.Trace("No payment processor found on this transaction or in the configuration. Failing refund. Aborting Plugin.");
									throw new Exception("Refund Failed: No payment processor found on this transaction or in the configuration. Please set the payment processor/gateway on the transaction or configuration, save and try the refund process again.");
								}
								localContext.TracingService.Trace("Getting the payment processor from the configuration record.");
								Entity entity9 = organizationService.Retrieve("msnfp_paymentprocessor", ((EntityReference)entity7["msnfp_paymentprocessorid"]).Id, new ColumnSet("msnfp_paymentprocessorid", "msnfp_apikey", "msnfp_storeid", "msnfp_testmode", "msnfp_paymentgatewaytype", "msnfp_stripeservicekey", "msnfp_iatsagentcode", "msnfp_iatspassword"));
								localContext.TracingService.Trace("Retrieved payment processor: " + ((Guid)entity9["msnfp_paymentprocessorid"]).ToString());
								if (((OptionSetValue)entity9["msnfp_paymentgatewaytype"]).Value == 844060000)
								{
									localContext.TracingService.Trace("Payment gateway Moneris.");
									RefundMonerisTransaction(payment, entity9, entity3, entity7, localContext, organizationService);
								}
								else if (((OptionSetValue)entity9["msnfp_paymentgatewaytype"]).Value == 844060001)
								{
									localContext.TracingService.Trace("Payment gateway Stripe.");
									RefundStripeTransaction(payment, entity9, entity3, entity7, localContext, organizationService);
								}
								else
								{
									if (((OptionSetValue)entity9["msnfp_paymentgatewaytype"]).Value != 844060002)
									{
										localContext.TracingService.Trace("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
										throw new Exception("Refund Failed: No matching msnfp_paymentgatewaytype. Please refer to the payment processor and ensure a gateway type is selected, save and try the refund process again.");
									}
									localContext.TracingService.Trace("Payment gateway iATS.");
									RefundIatsTransaction(payment, entity9, entity3, entity7, localContext, organizationService);
								}
							}
						}
						else
						{
							localContext.TracingService.Trace("Refund is not credit/debit, so we don't use a payment gateway and update just the refund/transaction records.");
							SetTransactionAsRefundedPayment(payment, entity3, localContext, organizationService);
						}
					}
					else
					{
						localContext.TracingService.Trace("Warning: No refund type code. This should not occur on create.");
					}
				}
				if (messageName == "Update" || messageName == "Create")
				{
					if (messageName == "Update")
					{
						queriedEntityRecord = organizationService.Retrieve("msnfp_refund", entity3.Id, GetColumnSet());
					}
					if (entity3 != null)
					{
						if (messageName == "Create")
						{
							AddOrUpdateThisRecordWithAzure(entity3, entity, localContext, organizationService, pluginExecutionContext);
						}
						else if (messageName == "Update")
						{
							AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
						}
					}
					else
					{
						localContext.TracingService.Trace("Target record not found.");
					}
				}
			}
			if (messageName == "Delete")
			{
				queriedEntityRecord = organizationService.Retrieve("msnfp_refund", ((EntityReference)pluginExecutionContext.InputParameters["Target"]).Id, GetColumnSet());
				AddOrUpdateThisRecordWithAzure(queriedEntityRecord, entity, localContext, organizationService, pluginExecutionContext);
			}
			localContext.TracingService.Trace("---------Exiting RefundCreate.cs---------");
		}

		private ColumnSet GetColumnSet()
		{
			return new ColumnSet("msnfp_refundid", "msnfp_identifier", "msnfp_customerid", "msnfp_amount_receipted", "msnfp_amount_membership", "msnfp_ref_amount_membership", "msnfp_amount_nonreceiptable", "msnfp_ref_amount_nonreceiptable", "msnfp_ref_amount_receipted", "msnfp_amount_tax", "msnfp_ref_amount_tax", "msnfp_chequenumber", "msnfp_transactionid", "msnfp_paymentid", "msnfp_bookdate", "msnfp_receiveddate", "msnfp_refundtypecode", "msnfp_amount", "msnfp_ref_amount", "msnfp_transactionidentifier", "msnfp_transactiionresult", "transactioncurrencyid", "statecode", "statuscode", "createdon");
		}

		private void AddOrUpdateThisRecordWithAzure(Entity queriedEntityRecord, Entity configurationRecord, LocalPluginContext localContext, IOrganizationService service, IPluginExecutionContext context)
		{
			localContext.TracingService.Trace("---------Send the Record to Azure---------");
			string messageName = context.MessageName;
			string text = "Refund";
			string text2 = Utilities.GetAzureWebAPIURL(service, context);
			localContext.TracingService.Trace("Got API URL: " + text2);
			if (text2 != string.Empty)
			{
				localContext.TracingService.Trace("Getting Latest Info for Record: " + queriedEntityRecord["msnfp_refundid"].ToString());
				MSNFP_Refund mSNFP_Refund = new MSNFP_Refund();
				mSNFP_Refund.RefundId = (Guid)queriedEntityRecord["msnfp_refundid"];
				mSNFP_Refund.Identifier = (queriedEntityRecord.Contains("msnfp_identifier") ? ((string)queriedEntityRecord["msnfp_identifier"]) : string.Empty);
				localContext.TracingService.Trace("Title: " + mSNFP_Refund.Identifier);
				if (queriedEntityRecord.Contains("msnfp_customerid") && queriedEntityRecord["msnfp_customerid"] != null)
				{
					mSNFP_Refund.CustomerId = ((EntityReference)queriedEntityRecord["msnfp_customerid"]).Id;
					if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "contact")
					{
						mSNFP_Refund.CustomerIdType = 2;
					}
					else if (((EntityReference)queriedEntityRecord["msnfp_customerid"]).LogicalName.ToLower() == "account")
					{
						mSNFP_Refund.CustomerIdType = 1;
					}
					localContext.TracingService.Trace("Got msnfp_customerid.");
				}
				else
				{
					mSNFP_Refund.CustomerId = null;
					mSNFP_Refund.CustomerIdType = null;
					localContext.TracingService.Trace("Did NOT find msnfp_customerid.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_receipted") && queriedEntityRecord["msnfp_amount_receipted"] != null)
				{
					mSNFP_Refund.AmountReceipted = ((Money)queriedEntityRecord["msnfp_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_receipted.");
				}
				else
				{
					mSNFP_Refund.AmountReceipted = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_membership") && queriedEntityRecord["msnfp_amount_membership"] != null)
				{
					mSNFP_Refund.AmountMembership = ((Money)queriedEntityRecord["msnfp_amount_membership"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_membership.");
				}
				else
				{
					mSNFP_Refund.AmountMembership = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_membership.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_membership") && queriedEntityRecord["msnfp_ref_amount_membership"] != null)
				{
					mSNFP_Refund.RefAmountMembership = ((Money)queriedEntityRecord["msnfp_ref_amount_membership"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_membership.");
				}
				else
				{
					mSNFP_Refund.RefAmountMembership = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_membership.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_nonreceiptable") && queriedEntityRecord["msnfp_amount_nonreceiptable"] != null)
				{
					mSNFP_Refund.AmountNonReceiptable = ((Money)queriedEntityRecord["msnfp_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Refund.AmountNonReceiptable = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_nonreceiptable") && queriedEntityRecord["msnfp_ref_amount_nonreceiptable"] != null)
				{
					mSNFP_Refund.RefAmountNonreceiptable = ((Money)queriedEntityRecord["msnfp_ref_amount_nonreceiptable"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_nonreceiptable.");
				}
				else
				{
					mSNFP_Refund.RefAmountNonreceiptable = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_nonreceiptable.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_receipted") && queriedEntityRecord["msnfp_ref_amount_receipted"] != null)
				{
					mSNFP_Refund.RefAmountReceipted = ((Money)queriedEntityRecord["msnfp_ref_amount_receipted"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_receipted.");
				}
				else
				{
					mSNFP_Refund.RefAmountReceipted = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_receipted.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount_tax") && queriedEntityRecord["msnfp_amount_tax"] != null)
				{
					mSNFP_Refund.AmountTax = ((Money)queriedEntityRecord["msnfp_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount_tax.");
				}
				else
				{
					mSNFP_Refund.AmountTax = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount_tax") && queriedEntityRecord["msnfp_ref_amount_tax"] != null)
				{
					mSNFP_Refund.RefAmountTax = ((Money)queriedEntityRecord["msnfp_ref_amount_tax"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount_tax.");
				}
				else
				{
					mSNFP_Refund.RefAmountTax = default(decimal);
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount_tax.");
				}
				if (queriedEntityRecord.Contains("msnfp_chequenumber") && queriedEntityRecord["msnfp_chequenumber"] != null)
				{
					mSNFP_Refund.ChequeNumber = (string)queriedEntityRecord["msnfp_chequenumber"];
					localContext.TracingService.Trace("Got msnfp_chequenumber.");
				}
				else
				{
					mSNFP_Refund.ChequeNumber = null;
					localContext.TracingService.Trace("Did NOT find msnfp_chequenumber.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionid") && queriedEntityRecord["msnfp_transactionid"] != null)
				{
					mSNFP_Refund.TransactionId = ((EntityReference)queriedEntityRecord["msnfp_transactionid"]).Id;
					localContext.TracingService.Trace("Got msnfp_transactionid.");
				}
				else
				{
					mSNFP_Refund.TransactionId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionid.");
				}
				if (queriedEntityRecord.Contains("msnfp_paymentid") && queriedEntityRecord["msnfp_paymentid"] != null)
				{
					mSNFP_Refund.PaymentId = ((EntityReference)queriedEntityRecord["msnfp_paymentid"]).Id;
					localContext.TracingService.Trace("Got msnfp_paymentid.");
				}
				else
				{
					mSNFP_Refund.PaymentId = null;
					localContext.TracingService.Trace("Did NOT find msnfp_paymentid.");
				}
				if (queriedEntityRecord.Contains("msnfp_bookdate") && queriedEntityRecord["msnfp_bookdate"] != null)
				{
					mSNFP_Refund.BookDate = (DateTime)queriedEntityRecord["msnfp_bookdate"];
					localContext.TracingService.Trace("Got msnfp_bookdate.");
				}
				else
				{
					mSNFP_Refund.BookDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_bookdate.");
				}
				if (queriedEntityRecord.Contains("msnfp_receiveddate") && queriedEntityRecord["msnfp_receiveddate"] != null)
				{
					mSNFP_Refund.ReceivedDate = (DateTime)queriedEntityRecord["msnfp_receiveddate"];
					localContext.TracingService.Trace("Got msnfp_receiveddate.");
				}
				else
				{
					mSNFP_Refund.ReceivedDate = null;
					localContext.TracingService.Trace("Did NOT find msnfp_receiveddate.");
				}
				if (queriedEntityRecord.Contains("msnfp_refundtypecode") && queriedEntityRecord["msnfp_refundtypecode"] != null)
				{
					mSNFP_Refund.RefundTypeCode = ((OptionSetValue)queriedEntityRecord["msnfp_refundtypecode"]).Value;
					localContext.TracingService.Trace("Got msnfp_refundtypecode.");
				}
				else
				{
					mSNFP_Refund.RefundTypeCode = null;
					localContext.TracingService.Trace("Did NOT find msnfp_refundtypecode.");
				}
				if (queriedEntityRecord.Contains("msnfp_amount") && queriedEntityRecord["msnfp_amount"] != null)
				{
					mSNFP_Refund.Amount = ((Money)queriedEntityRecord["msnfp_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_amount.");
				}
				else
				{
					mSNFP_Refund.Amount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_ref_amount") && queriedEntityRecord["msnfp_ref_amount"] != null)
				{
					mSNFP_Refund.RefAmount = ((Money)queriedEntityRecord["msnfp_ref_amount"]).Value;
					localContext.TracingService.Trace("Got msnfp_ref_amount.");
				}
				else
				{
					mSNFP_Refund.RefAmount = null;
					localContext.TracingService.Trace("Did NOT find msnfp_ref_amount.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactionidentifier") && queriedEntityRecord["msnfp_transactionidentifier"] != null)
				{
					mSNFP_Refund.TransactionIdentifier = (string)queriedEntityRecord["msnfp_transactionidentifier"];
					localContext.TracingService.Trace("Got msnfp_transactionidentifier.");
				}
				else
				{
					mSNFP_Refund.TransactionIdentifier = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactionidentifier.");
				}
				if (queriedEntityRecord.Contains("msnfp_transactiionresult") && queriedEntityRecord["msnfp_transactiionresult"] != null)
				{
					mSNFP_Refund.TransactionResult = (string)queriedEntityRecord["msnfp_transactiionresult"];
					localContext.TracingService.Trace("Got msnfp_transactiionresult.");
				}
				else
				{
					mSNFP_Refund.TransactionResult = null;
					localContext.TracingService.Trace("Did NOT find msnfp_transactiionresult.");
				}
				if (queriedEntityRecord.Contains("transactioncurrencyid") && queriedEntityRecord["transactioncurrencyid"] != null)
				{
					mSNFP_Refund.TransactionCurrencyId = ((EntityReference)queriedEntityRecord["transactioncurrencyid"]).Id;
					localContext.TracingService.Trace("Got TransactionCurrencyId.");
				}
				else
				{
					mSNFP_Refund.TransactionCurrencyId = null;
					localContext.TracingService.Trace("Did NOT find TransactionCurrencyId.");
				}
				if (queriedEntityRecord.Contains("statecode") && queriedEntityRecord["statecode"] != null)
				{
					mSNFP_Refund.StateCode = ((OptionSetValue)queriedEntityRecord["statecode"]).Value;
					localContext.TracingService.Trace("Got statecode.");
				}
				else
				{
					mSNFP_Refund.StateCode = null;
					localContext.TracingService.Trace("Did NOT find statecode.");
				}
				if (queriedEntityRecord.Contains("statuscode") && queriedEntityRecord["statuscode"] != null)
				{
					mSNFP_Refund.StatusCode = ((OptionSetValue)queriedEntityRecord["statuscode"]).Value;
					localContext.TracingService.Trace("Got statuscode.");
				}
				else
				{
					mSNFP_Refund.StatusCode = null;
					localContext.TracingService.Trace("Did NOT find statuscode.");
				}
				if (messageName == "Create")
				{
					mSNFP_Refund.CreatedOn = DateTime.UtcNow;
				}
				else if (queriedEntityRecord.Contains("createdon") && queriedEntityRecord["createdon"] != null)
				{
					mSNFP_Refund.CreatedOn = (DateTime)queriedEntityRecord["createdon"];
				}
				else
				{
					mSNFP_Refund.CreatedOn = null;
				}
				mSNFP_Refund.SyncDate = DateTime.UtcNow;
				if (messageName == "Delete")
				{
					mSNFP_Refund.Deleted = true;
					mSNFP_Refund.DeletedDate = DateTime.UtcNow;
				}
				else
				{
					mSNFP_Refund.Deleted = false;
					mSNFP_Refund.DeletedDate = null;
				}
				localContext.TracingService.Trace("JSON object created");
				if (messageName == "Create")
				{
					text2 = text2 + text + "/Create" + text;
				}
				else if (messageName == "Update" || messageName == "Delete")
				{
					text2 = text2 + text + "/Update" + text;
				}
				MemoryStream memoryStream = new MemoryStream();
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MSNFP_Refund));
				dataContractJsonSerializer.WriteObject(memoryStream, mSNFP_Refund);
				byte[] array = memoryStream.ToArray();
				memoryStream.Close();
				string @string = Encoding.UTF8.GetString(array, 0, array.Length);
				WebAPIClient webAPIClient = new WebAPIClient();
				webAPIClient.Headers[HttpRequestHeader.ContentType] = "application/json";
				webAPIClient.Headers["Padlock"] = (string)configurationRecord["msnfp_apipadlocktoken"];
				webAPIClient.Encoding = Encoding.UTF8;
				localContext.TracingService.Trace("---------Preparing JSON---------");
				localContext.TracingService.Trace("Converted to json API URL : " + text2);
				localContext.TracingService.Trace("JSON: " + @string);
				localContext.TracingService.Trace("---------End of Preparing JSON---------");
				localContext.TracingService.Trace("Sending data to Azure.");
				string str = webAPIClient.UploadString(text2, @string);
				localContext.TracingService.Trace("Got response.");
				localContext.TracingService.Trace("Response: " + str);
			}
			else
			{
				localContext.TracingService.Trace("No API URL or Enable Portal Pages. Exiting workflow.");
			}
		}

		private void RefundIatsTransaction(Entity refundEntity, Entity paymentProcessor, Entity primaryRefund, Entity config, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("iATS refund starts");
			string agentCode = string.Empty;
			string password = string.Empty;
			try
			{
				CreditCardDetail creditCardDetail = new CreditCardDetail();
				int num = 0;
				if (refundEntity.LogicalName == "msnfp_transaction")
				{
					num = (refundEntity.Contains("msnfp_paymenttypecode") ? ((OptionSetValue)refundEntity["msnfp_paymenttypecode"]).Value : 0);
				}
				else if (refundEntity.LogicalName == "msnfp_payment")
				{
					num = (refundEntity.Contains("msnfp_paymenttype") ? ((OptionSetValue)refundEntity["msnfp_paymenttype"]).Value : 0);
				}
				localContext.TracingService.Trace("giftType : " + num);
				creditCardDetail.Identifier = (refundEntity.Contains("msnfp_transactionidentifier") ? ((string)refundEntity["msnfp_transactionidentifier"]) : string.Empty);
				localContext.TracingService.Trace("Transaction Identifier : " + creditCardDetail.Identifier);
				creditCardDetail.Amount = string.Format("{0:0.00}", primaryRefund.Contains("msnfp_ref_amount") ? (((Money)primaryRefund["msnfp_ref_amount"]).Value * -1m) : 0m);
				localContext.TracingService.Trace("Refund Amount : " + creditCardDetail.Amount);
				if (paymentProcessor != null)
				{
					agentCode = paymentProcessor.GetAttributeValue<string>("msnfp_iatsagentcode");
					password = paymentProcessor.GetAttributeValue<string>("msnfp_iatspassword");
				}
				if (num == 844060002)
				{
					localContext.TracingService.Trace("Credit Card");
					Entity entity = new Entity("msnfp_response");
					if (refundEntity.LogicalName == "msnfp_transaction")
					{
						entity["msnfp_transactionid"] = new EntityReference("msnfp_transaction", refundEntity.Id);
					}
					else if (refundEntity.LogicalName == "msnfp_payment")
					{
						entity["msnfp_paymentid"] = new EntityReference("msnfp_payment", refundEntity.Id);
					}
					if (refundEntity.Contains("msnfp_eventpackageid") && refundEntity["msnfp_eventpackageid"] != null)
					{
						entity["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)refundEntity["msnfp_eventpackageid"]).Id);
					}
					localContext.TracingService.Trace("Refund Started");
					ProcessCreditCardRefundWithTransactionId processCreditCardRefundWithTransactionId = new ProcessCreditCardRefundWithTransactionId();
					processCreditCardRefundWithTransactionId.agentCode = agentCode;
					processCreditCardRefundWithTransactionId.password = password;
					processCreditCardRefundWithTransactionId.transactionId = creditCardDetail.Identifier;
					processCreditCardRefundWithTransactionId.total = creditCardDetail.Amount;
					XmlDocument xmlDocument = iATSProcess.ProcessCreditCardRefundWithTransactionId(processCreditCardRefundWithTransactionId);
					XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT");
					foreach (XmlNode item in elementsByTagName)
					{
						string innerText = item.InnerText;
						if (innerText.Contains("OK"))
						{
							localContext.TracingService.Trace("Refund Success");
							entity["msnfp_response"] = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
							if (refundEntity.LogicalName == "msnfp_transaction")
							{
								SetTransactionAsRefunded(refundEntity, primaryRefund, localContext, service);
							}
							else if (refundEntity.LogicalName == "msnfp_payment")
							{
								SetTransactionAsRefundedPayment(refundEntity, primaryRefund, localContext, service);
							}
						}
						else
						{
							entity["msnfp_response"] = xmlDocument.GetElementsByTagName("AUTHORIZATIONRESULT")[0].InnerText;
							localContext.TracingService.Trace("iATS refund process failure.");
							refundEntity["msnfp_transactionresult"] = "Refund Failed.";
						}
					}
					service.Create(entity);
				}
				service.Update(refundEntity);
				localContext.TracingService.Trace("Updated successfully");
			}
			catch (Exception exception)
			{
				throw new InvalidPluginExecutionException("Error in Refund", exception);
			}
		}

		private void RefundStripeTransaction(Entity refundEntity, Entity paymentProcessor, Entity primaryRefund, Entity config, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Start");
			try
			{
				CreditCardDetail creditCardDetail = new CreditCardDetail();
				int num = 0;
				if (refundEntity.LogicalName == "msnfp_transaction")
				{
					num = (refundEntity.Contains("msnfp_paymenttypecode") ? ((OptionSetValue)refundEntity["msnfp_paymenttypecode"]).Value : 0);
				}
				else if (refundEntity.LogicalName == "msnfp_payment")
				{
					num = (refundEntity.Contains("msnfp_paymenttype") ? ((OptionSetValue)refundEntity["msnfp_paymenttype"]).Value : 0);
				}
				localContext.TracingService.Trace("giftType : " + num);
				creditCardDetail.Identifier = (refundEntity.Contains("msnfp_transactionidentifier") ? ((string)refundEntity["msnfp_transactionidentifier"]) : string.Empty);
				localContext.TracingService.Trace("Transaction Identifier : " + creditCardDetail.Identifier);
				creditCardDetail.Amount = string.Format("{0:0.00}", primaryRefund.Contains("msnfp_ref_amount") ? ((Money)primaryRefund["msnfp_ref_amount"]).Value : 0m);
				localContext.TracingService.Trace("Transaction Amount : " + creditCardDetail.Amount);
				string text = (Convert.ToDecimal(creditCardDetail.Amount) * 100m).ToString().Split('.')[0];
				localContext.TracingService.Trace("Amount to be refunded : " + text);
				if (num == 844060002)
				{
					localContext.TracingService.Trace("Credit Card");
					Entity entity = new Entity("msnfp_response");
					if (refundEntity.LogicalName == "msnfp_transaction")
					{
						entity["msnfp_transactionid"] = new EntityReference("msnfp_transaction", refundEntity.Id);
					}
					else if (refundEntity.LogicalName == "msnfp_payment")
					{
						entity["msnfp_paymentid"] = new EntityReference("msnfp_payment", refundEntity.Id);
						if (refundEntity.Contains("msnfp_eventpackageid") && refundEntity["msnfp_eventpackageid"] != null)
						{
							entity["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)refundEntity["msnfp_eventpackageid"]).Id);
						}
					}
					localContext.TracingService.Trace("Refund Started");
					string apiKey = paymentProcessor["msnfp_stripeservicekey"].ToString();
					StripeConfiguration.SetApiKey(apiKey);
					StripeRefundCreateOptions stripeRefundCreateOptions = new StripeRefundCreateOptions
					{
						Amount = Convert.ToInt32(text),
						Reason = "requested_by_customer"
					};
					localContext.TracingService.Trace("Transaction Amount : " + stripeRefundCreateOptions.Amount);
					StripeRefundService stripeRefundService = new StripeRefundService();
					StripeRefund stripeRefund = stripeRefundService.Create(creditCardDetail.Identifier, stripeRefundCreateOptions);
					if (stripeRefund != null)
					{
						localContext.TracingService.Trace("Find Stripe RefundResponse : " + stripeRefund.Status);
						if (stripeRefund.Status == "succeeded")
						{
							localContext.TracingService.Trace("Refund Success");
							entity["msnfp_response"] = stripeRefund.Status;
							if (refundEntity.LogicalName == "msnfp_transaction")
							{
								SetTransactionAsRefunded(refundEntity, primaryRefund, localContext, service);
							}
							else if (refundEntity.LogicalName == "msnfp_payment")
							{
								SetTransactionAsRefundedPayment(refundEntity, primaryRefund, localContext, service);
							}
						}
						else
						{
							entity["msnfp_response"] = stripeRefund.FailureReason;
							localContext.TracingService.Trace("Stripe refund process failure.");
							refundEntity["msnfp_transactionresult"] = "Refund Failed.";
						}
						service.Create(entity);
					}
				}
				service.Update(refundEntity);
				localContext.TracingService.Trace("Updated successfully");
			}
			catch (Exception exception)
			{
				throw new InvalidPluginExecutionException("Error in Refund", exception);
			}
		}

		private void RefundMonerisTransaction(Entity refundEntity, Entity paymentProcessor, Entity primaryRefund, Entity config, LocalPluginContext localContext, IOrganizationService service)
		{
			localContext.TracingService.Trace("Entering RefundMonerisTransaction().");
			CreditCardDetail creditCardDetail = new CreditCardDetail();
			if (!refundEntity.Contains("msnfp_invoiceidentifier") || !refundEntity.Contains("msnfp_transactionnumber"))
			{
				return;
			}
			localContext.TracingService.Trace("Gift contains payment method.");
			creditCardDetail.Identifier = (refundEntity.Contains("msnfp_invoiceidentifier") ? ((string)refundEntity["msnfp_invoiceidentifier"]) : string.Empty);
			creditCardDetail.TxnNumber = (refundEntity.Contains("msnfp_transactionnumber") ? ((string)refundEntity["msnfp_transactionnumber"]) : string.Empty);
			localContext.TracingService.Trace("Transaction Identifier: " + creditCardDetail.Identifier);
			creditCardDetail.Amount = string.Format("{0:0.00}", primaryRefund.Contains("msnfp_ref_amount") ? ((Money)primaryRefund["msnfp_ref_amount"]).Value : 0m);
			localContext.TracingService.Trace("Transaction Amount : " + string.Format("{0:0.00}", ((Money)primaryRefund["msnfp_ref_amount"]).Value));
			creditCardDetail.CryptType = Constants.CRYPTTYPE;
			localContext.TracingService.Trace("Attempting to refund Moneris purchase.");
			Receipt receipt = MonerisRefundPurchase(creditCardDetail, config, paymentProcessor, localContext);
			StringBuilder stringBuilder = new StringBuilder();
			if (receipt != null)
			{
				int num = 0;
				if (receipt.GetResponseCode() != "null")
				{
					num = Convert.ToInt32(receipt.GetResponseCode());
				}
				Entity entity = new Entity("msnfp_response");
				stringBuilder.Append("Response Code : " + receipt.GetResponseCode());
				stringBuilder.AppendLine();
				stringBuilder.Append("Response Message : " + receipt.GetMessage());
				stringBuilder.AppendLine();
				stringBuilder.Append("Response Complete : " + receipt.GetComplete());
				entity["msnfp_response"] = stringBuilder.ToString();
				if (refundEntity.LogicalName == "msnfp_transaction")
				{
					entity["msnfp_transactionid"] = new EntityReference("msnfp_transaction", refundEntity.Id);
				}
				else if (refundEntity.LogicalName == "msnfp_payment")
				{
					entity["msnfp_paymentid"] = new EntityReference("msnfp_payment", refundEntity.Id);
					if (refundEntity.Contains("msnfp_eventpackageid") && refundEntity["msnfp_eventpackageid"] != null)
					{
						entity["msnfp_eventpackageid"] = new EntityReference("msnfp_eventpackage", ((EntityReference)refundEntity["msnfp_eventpackageid"]).Id);
					}
				}
				service.Create(entity);
				localContext.TracingService.Trace("Process Successful.");
				localContext.TracingService.Trace("responseCode.ToString() == " + num);
				localContext.TracingService.Trace("receipt.GetResponseCode() == " + receipt.GetResponseCode());
				localContext.TracingService.Trace("receipt.GetMessage() == " + receipt.GetMessage());
				localContext.TracingService.Trace("receipt.GetComplete() == " + receipt.GetComplete());
				if (num != 0 && num < 50)
				{
					if (refundEntity.LogicalName == "msnfp_transaction")
					{
						SetTransactionAsRefunded(refundEntity, primaryRefund, localContext, service);
					}
					else if (refundEntity.LogicalName == "msnfp_payment")
					{
						SetTransactionAsRefundedPayment(refundEntity, primaryRefund, localContext, service);
					}
					if (receipt.GetReferenceNum() != null)
					{
						primaryRefund["msnfp_transactionidentifier"] = receipt.GetReferenceNum();
						localContext.TracingService.Trace("Reference Number : " + receipt.GetReferenceNum());
						service.Update(primaryRefund);
						localContext.TracingService.Trace("Refund Updated.");
					}
				}
				else if (num == 0 || num > 49)
				{
					refundEntity["msnfp_transactionresult"] = "Refund Payment Failed: " + num;
					localContext.TracingService.Trace("Updated Transaction Record's transaction result with 'Refund Payment Failed'. Response Code: " + num);
				}
			}
			else
			{
				refundEntity["msnfp_transactionresult"] = "Refund Failed.";
				localContext.TracingService.Trace("Receipt is null - MonerisRefundPurchase() did not complete successfully.");
			}
			service.Update(refundEntity);
			localContext.TracingService.Trace("Updated gift values.");
		}

		private void SetTransactionAsRefunded(Entity gift, Entity primaryRefund, LocalPluginContext localContext, IOrganizationService service)
		{
			gift["msnfp_transactionresult"] = "Refunded";
			gift["msnfp_daterefunded"] = DateTime.UtcNow;
			localContext.TracingService.Trace("Getting latest amounts from the refund.");
			localContext.TracingService.Trace("Now assign amount fields.");
			localContext.TracingService.Trace("Primary Refund Amounts");
			decimal d = (primaryRefund.Contains("msnfp_amount_receipted") ? ((Money)primaryRefund["msnfp_amount_receipted"]).Value : 0m);
			localContext.TracingService.Trace("amount = " + d);
			decimal d2 = (primaryRefund.Contains("msnfp_amount_membership") ? ((Money)primaryRefund["msnfp_amount_membership"]).Value : 0m);
			localContext.TracingService.Trace("amountMembership = " + d2);
			decimal d3 = (primaryRefund.Contains("msnfp_amount_nonreceiptable") ? ((Money)primaryRefund["msnfp_amount_nonreceiptable"]).Value : 0m);
			localContext.TracingService.Trace("amountNonReceiptable = " + d3);
			decimal d4 = (primaryRefund.Contains("msnfp_amount_tax") ? ((Money)primaryRefund["msnfp_amount_tax"]).Value : 0m);
			localContext.TracingService.Trace("amountTax = " + d4);
			localContext.TracingService.Trace("Primary Refund Amount Receipted");
			decimal d5 = (primaryRefund.Contains("msnfp_ref_amount_receipted") ? ((Money)primaryRefund["msnfp_ref_amount_receipted"]).Value : 0m);
			localContext.TracingService.Trace("amountRefund = " + d5);
			decimal d6 = (primaryRefund.Contains("msnfp_ref_amount_membership") ? ((Money)primaryRefund["msnfp_ref_amount_membership"]).Value : 0m);
			localContext.TracingService.Trace("amountMembershipRefund = " + d6);
			decimal d7 = (primaryRefund.Contains("msnfp_ref_amount_nonreceiptable") ? ((Money)primaryRefund["msnfp_ref_amount_nonreceiptable"]).Value : 0m);
			localContext.TracingService.Trace("amountNonReceiptableRefund = " + d7);
			decimal d8 = (primaryRefund.Contains("msnfp_ref_amount_tax") ? ((Money)primaryRefund["msnfp_ref_amount_tax"]).Value : 0m);
			localContext.TracingService.Trace("amountTaxRefund = " + d8);
			decimal d9 = (primaryRefund.Contains("msnfp_ref_amount") ? ((Money)primaryRefund["msnfp_ref_amount"]).Value : 0m);
			localContext.TracingService.Trace("Getting amounts from the transaction.");
			decimal d10 = (gift.Contains("msnfp_ref_amount_receipted") ? ((Money)gift["msnfp_ref_amount_receipted"]).Value : 0m);
			localContext.TracingService.Trace("giftAmountRefunded = " + d10);
			decimal d11 = (gift.Contains("msnfp_ref_amount_membership") ? ((Money)gift["msnfp_ref_amount_membership"]).Value : 0m);
			localContext.TracingService.Trace("giftAmountMembershipRefunded = " + d11);
			decimal d12 = (gift.Contains("msnfp_ref_amount_nonreceiptable") ? ((Money)gift["msnfp_ref_amount_nonreceiptable"]).Value : 0m);
			localContext.TracingService.Trace("giftAmountNonReceiptableRefunded = " + d12);
			decimal d13 = (gift.Contains("msnfp_ref_amount_tax") ? ((Money)gift["msnfp_ref_amount_tax"]).Value : 0m);
			localContext.TracingService.Trace("giftAmountTaxRefunded = " + d13);
			decimal d14 = (gift.Contains("msnfp_ref_amount") ? ((Money)gift["msnfp_ref_amount"]).Value : 0m);
			localContext.TracingService.Trace("giftTotalRefunded = " + d14);
			localContext.TracingService.Trace("Calculating the new amounts for the transaction (amount = amounts - refund amount) and setting refund fields.");
			gift["msnfp_amount_receipted"] = new Money(d - d5);
			gift["msnfp_amount_membership"] = new Money(d2 - d6);
			gift["msnfp_amount_nonreceiptable"] = new Money(d3 - d7);
			gift["msnfp_amount_tax"] = new Money(d4 - d8);
			gift["msnfp_ref_amount_receipted"] = new Money(d10 + d5);
			gift["msnfp_ref_amount_membership"] = new Money(d11 + d6);
			gift["msnfp_ref_amount_nonreceiptable"] = new Money(d12 + d7);
			gift["msnfp_ref_amount"] = new Money(d14 + d9);
			gift["msnfp_ref_amount_tax"] = new Money(d13 + d8);
			localContext.TracingService.Trace("Getting amounts from the transaction.");
			gift["statuscode"] = new OptionSetValue(844060004);
			if (primaryRefund.Contains("msnfp_refunddate"))
			{
				gift["msnfp_daterefunded"] = primaryRefund["msnfp_refunddate"];
			}
			else
			{
				gift["msnfp_daterefunded"] = DateTime.UtcNow;
			}
			service.Update(gift);
		}

		private void SetTransactionAsRefundedPayment(Entity payment, Entity primaryRefund, LocalPluginContext localContext, IOrganizationService service)
		{
			payment["msnfp_transactionresult"] = "Refunded";
			payment["msnfp_daterefunded"] = DateTime.UtcNow;
			localContext.TracingService.Trace("Getting latest amounts from the refund.");
			localContext.TracingService.Trace("Now assign amount fields.");
			decimal d = (primaryRefund.Contains("msnfp_ref_amount") ? ((Money)primaryRefund["msnfp_ref_amount"]).Value : 0m);
			localContext.TracingService.Trace("Getting amounts from the transaction.");
			decimal d2 = (payment.Contains("msnfp_amount_refunded") ? ((Money)payment["msnfp_amount_refunded"]).Value : 0m);
			localContext.TracingService.Trace("paymentTotalRefunded = " + d2);
			decimal d3 = (payment.Contains("msnfp_amount") ? ((Money)payment["msnfp_amount"]).Value : 0m);
			localContext.TracingService.Trace("paymentTotalAmount = " + d3);
			localContext.TracingService.Trace("Calculating the new amounts for the transaction (amount = amounts - refund amount) and setting refund fields.");
			payment["msnfp_amount_refunded"] = new Money(d2 + d);
			payment["msnfp_amount_balance"] = new Money(d3 - (d2 + d));
			localContext.TracingService.Trace("Getting amounts from the transaction.");
			payment["statuscode"] = new OptionSetValue(844060004);
			if (primaryRefund.Contains("msnfp_refunddate"))
			{
				payment["msnfp_daterefunded"] = primaryRefund["msnfp_refunddate"];
			}
			else
			{
				payment["msnfp_daterefunded"] = DateTime.UtcNow;
			}
			service.Update(payment);
		}

		private Receipt MonerisRefundPurchase(CreditCardDetail CCD, Entity config, Entity paymentProcessor, LocalPluginContext localContext)
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			localContext.TracingService.Trace("---------Entering RefundPurchase() ---------");
			string text = (paymentProcessor.Contains("msnfp_storeid") ? ((string)paymentProcessor["msnfp_storeid"]) : "store5");
			string text2 = (paymentProcessor.Contains("msnfp_apikey") ? ((string)paymentProcessor["msnfp_apikey"]) : "yesguy");
			bool testMode = paymentProcessor.Contains("msnfp_testmode") && (bool)paymentProcessor["msnfp_testmode"];
			localContext.TracingService.Trace("msnfp_storeid: " + text);
			localContext.TracingService.Trace("msnfp_apikey: " + text2);
			localContext.TracingService.Trace("msnfp_testmode: " + testMode);
			localContext.TracingService.Trace("CCD.TxnNumber: " + CCD.TxnNumber);
			localContext.TracingService.Trace("CCD.Identifier: " + CCD.Identifier);
			localContext.TracingService.Trace("CCD.Amount: " + CCD.Amount);
			Refund refund = new Refund();
			refund.SetTxnNumber(CCD.TxnNumber);
			refund.SetOrderId(CCD.Identifier);
			refund.SetAmount(CCD.Amount);
			refund.SetCryptType(CCD.CryptType);
			HttpsPostRequest httpsPostRequest = new HttpsPostRequest();
			httpsPostRequest.SetTestMode(testMode);
			httpsPostRequest.SetStoreId(text);
			httpsPostRequest.SetApiToken(text2);
			httpsPostRequest.SetTransaction(refund);
			httpsPostRequest.Send();
			Receipt result = null;
			try
			{
				localContext.TracingService.Trace("Obtained Receipt.");
				result = httpsPostRequest.GetReceipt();
			}
			catch (Exception ex)
			{
				localContext.TracingService.Trace("Error: " + ex.ToString());
				Console.WriteLine(ex);
			}
			return result;
		}
	}
}
