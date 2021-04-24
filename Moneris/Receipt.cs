using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Moneris
{
	[ComVisible(true)]
	public class Receipt
	{
		private Stream inStream = null;

		private XmlTextReader xtr = null;

		private string currentTag = null;

		private string currentTermID = null;

		private string currentCardType = null;

		private string currentTxnType = null;

		private string currentDataKey = null;

		private string currentRule = null;

		private bool isBatchTotals = false;

		private bool isResolveData = false;

		private bool isRule = false;

		private bool isVdotMe = false;

		private bool isPayPassInfo = false;

		private bool isMPI = false;

		private bool inBilling = false;

		private bool inShipping = false;

		private bool inPartialShipping = false;

		private Hashtable termIDHash = new Hashtable();

		private Hashtable cardHash = new Hashtable();

		private Hashtable purchaseHash;

		private Hashtable refundHash;

		private Hashtable correctionHash;

		private Hashtable responseDataHash = new Hashtable();

		private Hashtable resDataHash = new Hashtable();

		private Hashtable dataKeyHash = new Hashtable();

		private Hashtable ruleDataHash = new Hashtable();

		private Hashtable vmeDataHash = new Hashtable();

		private Hashtable paypassInfoHash = new Hashtable();

		private Stack dataKeyStack = new Stack();

		private Stack ruleStack = new Stack();

		public void setTermIdHash(Hashtable value)
		{
			termIDHash = value;
		}

		public void SetCardHash(Hashtable value)
		{
			cardHash = value;
		}

		public void SetPurchaseHash(Hashtable value)
		{
			purchaseHash = value;
		}

		public void SetRefundHash(Hashtable value)
		{
			refundHash = value;
		}

		public void SetCorrectionHash(Hashtable value)
		{
			correctionHash = value;
		}

		public void SetResponseDataHash(Hashtable value)
		{
			responseDataHash = value;
		}

		public void SetResDataHash(Hashtable value)
		{
			resDataHash = value;
		}

		public void SetDataKeyHash(Hashtable value)
		{
			dataKeyHash = value;
		}

		public void SetRuleDataHash(Hashtable value)
		{
			ruleDataHash = value;
		}

		public void SetVmeDataHash(Hashtable value)
		{
			vmeDataHash = value;
		}

		public void SetPaypassInfoHash(Hashtable value)
		{
			paypassInfoHash = value;
		}

		public void SetDataKeyStack(Stack value)
		{
			dataKeyStack = value;
		}

		public void SetRuleStack(Stack value)
		{
			ruleStack = value;
		}

		public void SetIsMPI(bool value)
		{
			isMPI = value;
		}

		public Hashtable GetResult()
		{
			return responseDataHash;
		}

		public string[] GetRules()
		{
			int count = ruleStack.Count;
			string[] array = new string[count];
			IEnumerator enumerator = ruleStack.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num++] = (string)enumerator.Current;
			}
			return array;
		}

		public string GetPurchaseAmount(string ecr_no, string card_type)
		{
			string text = (string)((Hashtable)((Hashtable)purchaseHash[ecr_no])[card_type])["Amount"];
			return (text == null) ? "0" : text;
		}

		public string GetPurchaseCount(string ecr_no, string card_type)
		{
			string text = (string)((Hashtable)((Hashtable)purchaseHash[ecr_no])[card_type])["Count"];
			return (text == null) ? "0" : text;
		}

		public string GetRefundAmount(string ecr_no, string card_type)
		{
			string text = (string)((Hashtable)((Hashtable)refundHash[ecr_no])[card_type])["Amount"];
			return (text == null) ? "0" : text;
		}

		public string GetRefundCount(string ecr_no, string card_type)
		{
			string text = (string)((Hashtable)((Hashtable)refundHash[ecr_no])[card_type])["Count"];
			return (text == null) ? "0" : text;
		}

		public string GetCorrectionAmount(string ecr_no, string card_type)
		{
			string text = (string)((Hashtable)((Hashtable)correctionHash[ecr_no])[card_type])["Amount"];
			return (text == null) ? "0" : text;
		}

		public string GetCorrectionCount(string ecr_no, string card_type)
		{
			string text = (string)((Hashtable)((Hashtable)correctionHash[ecr_no])[card_type])["Count"];
			return (text == null) ? "0" : text;
		}

		public string GetTerminalStatus(string ecr_no)
		{
			return (string)termIDHash[ecr_no];
		}

		public string[] GetTerminalIDs()
		{
			int count = termIDHash.Count;
			string[] array = new string[count];
			IDictionaryEnumerator enumerator = termIDHash.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num++] = (string)enumerator.Key;
			}
			return array;
		}

		public string[] GetCreditCards(string ecr_no)
		{
			Stack stack = (Stack)cardHash[ecr_no];
			int count = stack.Count;
			string[] array = new string[count];
			IEnumerator enumerator = stack.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num++] = (string)enumerator.Current;
			}
			return array;
		}

		public string GetITDResponse()
		{
			return (string)responseDataHash["ITDResponse"];
		}

		public string GetCardType()
		{
			return (string)responseDataHash["CardType"];
		}

		public string GetTransAmount()
		{
			return (string)responseDataHash["TransAmount"];
		}

		public string GetTxnNumber()
		{
			return (string)responseDataHash["TransID"];
		}

		public string GetReceiptId()
		{
			return (string)responseDataHash["ReceiptId"];
		}

		public string GetTransType()
		{
			return (string)responseDataHash["TransType"];
		}

		public string GetReferenceNum()
		{
			return (string)responseDataHash["ReferenceNum"];
		}

		public string GetResponseCode()
		{
			return (string)responseDataHash["ResponseCode"];
		}

		public string GetISO()
		{
			return (string)responseDataHash["ISO"];
		}

		public string GetBankTotals()
		{
			return (string)responseDataHash["BankTotals"];
		}

		public string GetMessage()
		{
			return (string)responseDataHash["Message"];
		}

		public string GetRecurSuccess()
		{
			return (string)responseDataHash["RecurSuccess"];
		}

		public string GetAuthCode()
		{
			return (string)responseDataHash["AuthCode"];
		}

		public string GetComplete()
		{
			return (string)responseDataHash["Complete"];
		}

		public string GetTransDate()
		{
			return (string)responseDataHash["TransDate"];
		}

		public string GetTransTime()
		{
			return (string)responseDataHash["TransTime"];
		}

		public string GetTicket()
		{
			return (string)responseDataHash["Ticket"];
		}

		public string GetTimedOut()
		{
			return (string)responseDataHash["TimedOut"];
		}

		public string GetIsVisaDebit()
		{
			return (string)responseDataHash["IsVisaDebit"];
		}

		public string GetAvsResultCode()
		{
			return (string)responseDataHash["AvsResultCode"];
		}

		public string GetCvdResultCode()
		{
			return (string)responseDataHash["CvdResultCode"];
		}

		public string GetMCPAmount()
		{
			return (string)responseDataHash["MCPAmount"];
		}

		public string GetMCPCurrencyCode()
		{
			return (string)responseDataHash["MCPCurrencyCode"];
		}

		public string GetRecurUpdateSuccess()
		{
			return (string)responseDataHash["RecurUpdateSuccess"];
		}

		public string GetNextRecurDate()
		{
			return (string)responseDataHash["NextRecurDate"];
		}

		public string GetCorporateCard()
		{
			return (string)responseDataHash["CorporateCard"];
		}

		public string GetRecurEndDate()
		{
			return (string)responseDataHash["RecurEndDate"];
		}

		public string GetDataKey()
		{
			return (string)responseDataHash["DataKey"];
		}

		public string GetResSuccess()
		{
			return (string)responseDataHash["ResSuccess"];
		}

		public string GetPaymentType()
		{
			return (string)responseDataHash["PaymentType"];
		}

		public string GetCavvResultCode()
		{
			return (string)responseDataHash["CavvResultCode"];
		}

		public string GetStatusCode()
		{
			return (string)responseDataHash["status_code"];
		}

		public string GetStatusMessage()
		{
			return (string)responseDataHash["status_message"];
		}

		public string GetMaskedPan()
		{
			return (string)responseDataHash["MaskedPan"];
		}

		public string GetCfSuccess()
		{
			return (string)responseDataHash["CfSuccess"];
		}

		public string GetCfStatus()
		{
			return (string)responseDataHash["CfStatus"];
		}

		public string GetFeeAmount()
		{
			return (string)responseDataHash["FeeAmount"];
		}

		public string GetFeeRate()
		{
			return (string)responseDataHash["FeeRate"];
		}

		public string GetFeeType()
		{
			return (string)responseDataHash["FeeType"];
		}

		public string GetExpNote(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["note"];
		}

		public string GetExpMaskedPan(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["masked_pan"];
		}

		public string GetExpPresentationType(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["presentation_type"];
		}

		public string GetExpPAccountNumber(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["p_account_number"];
		}

		public string GetExpExpdate(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["expdate"];
		}

		public string GetRuleName(string ruleName)
		{
			return (string)((Hashtable)ruleDataHash[ruleName])["RuleName"];
		}

		public string GetRuleCode(string ruleName)
		{
			return (string)((Hashtable)ruleDataHash[ruleName])["RuleCode"];
		}

		public string GetRuleMessageEn(string ruleName)
		{
			return (string)((Hashtable)ruleDataHash[ruleName])["RuleMessageEn"];
		}

		public string GetRuleMessageFr(string ruleName)
		{
			return (string)((Hashtable)ruleDataHash[ruleName])["RuleMessageFr"];
		}

		public string GetResCustId()
		{
			return (string)resDataHash["cust_id"];
		}

		public string GetResSec()
		{
			return (string)resDataHash["sec"];
		}

		public string GetResCustFirstName()
		{
			return (string)resDataHash["cust_first_name"];
		}

		public string GetResCustLastName()
		{
			return (string)resDataHash["cust_last_name"];
		}

		public string GetResCustAddress1()
		{
			return (string)resDataHash["cust_address1"];
		}

		public string GetResCustAddress2()
		{
			return (string)resDataHash["cust_address2"];
		}

		public string GetResCustCity()
		{
			return (string)resDataHash["cust_city"];
		}

		public string GetResCustState()
		{
			return (string)resDataHash["cust_state"];
		}

		public string GetResCustZip()
		{
			return (string)resDataHash["cust_zip"];
		}

		public string GetResRoutingNum()
		{
			return (string)resDataHash["routing_num"];
		}

		public string GetResAccountNum()
		{
			return (string)resDataHash["account_num"];
		}

		public string GetResMaskedAccountNum()
		{
			return (string)resDataHash["masked_account_num"];
		}

		public string GetResCheckNum()
		{
			return (string)resDataHash["check_num"];
		}

		public string GetResAccountType()
		{
			return (string)resDataHash["account_type"];
		}

		public string GetResDataCustId()
		{
			return (string)resDataHash["cust_id"];
		}

		public string GetResPhone()
		{
			return (string)resDataHash["phone"];
		}

		public string GetResEmail()
		{
			return (string)resDataHash["email"];
		}

		public string GetResNote()
		{
			return (string)resDataHash["note"];
		}

		public string GetResDataPhone()
		{
			return (string)resDataHash["phone"];
		}

		public string GetResDataEmail()
		{
			return (string)resDataHash["email"];
		}

		public string GetResDataNote()
		{
			return (string)resDataHash["note"];
		}

		public string GetResDataPan()
		{
			return (string)resDataHash["pan"];
		}

		public string GetResDataMaskedPan()
		{
			return (string)resDataHash["masked_pan"];
		}

		public string GetResDataExpdate()
		{
			return (string)resDataHash["expdate"];
		}

		public string GetResDataCryptType()
		{
			return (string)resDataHash["crypt_type"];
		}

		public string GetResDataAvsStreetNumber()
		{
			return (string)resDataHash["avs_street_number"];
		}

		public string GetResDataAvsStreetName()
		{
			return (string)resDataHash["avs_street_name"];
		}

		public string GetResDataAvsZipcode()
		{
			return (string)resDataHash["avs_zipcode"];
		}

		public string GetResDataPresentationType()
		{
			return (string)resDataHash["presentation_type"];
		}

		public string GetResDataPAccountNumber()
		{
			return (string)resDataHash["p_account_number"];
		}

		public string GetResMaskedPan()
		{
			return (string)resDataHash["masked_pan"];
		}

		public string GetResExpdate()
		{
			return (string)resDataHash["expdate"];
		}

		public string GetResExpDate()
		{
			return GetResExpdate();
		}

		public string GetResCryptType()
		{
			return (string)resDataHash["crypt_type"];
		}

		public string GetResAvsStreetNumber()
		{
			return (string)resDataHash["avs_street_number"];
		}

		public string GetResAvsStreetName()
		{
			return (string)resDataHash["avs_street_name"];
		}

		public string GetResAvsZipcode()
		{
			return (string)resDataHash["avs_zipcode"];
		}

		public string GetResDataSec()
		{
			return (string)resDataHash["sec"];
		}

		public string GetResDataCustFirstName()
		{
			return (string)resDataHash["cust_first_name"];
		}

		public string GetResDataCustLastName()
		{
			return (string)resDataHash["cust_last_name"];
		}

		public string GetResDataCustAddress1()
		{
			return (string)resDataHash["cust_address1"];
		}

		public string GetResDataCustAddress2()
		{
			return (string)resDataHash["cust_address2"];
		}

		public string GetResDataCustCity()
		{
			return (string)resDataHash["cust_city"];
		}

		public string GetResDataCustState()
		{
			return (string)resDataHash["cust_state"];
		}

		public string GetResDataCustZip()
		{
			return (string)resDataHash["cust_zip"];
		}

		public string GetResDataRoutingNum()
		{
			return (string)resDataHash["routing_num"];
		}

		public string GetResDataAccountNum()
		{
			return (string)resDataHash["account_num"];
		}

		public string GetResDataMaskedAccountNum()
		{
			return (string)resDataHash["masked_account_num"];
		}

		public string GetResDataCheckNum()
		{
			return (string)resDataHash["check_num"];
		}

		public string GetResDataAccountType()
		{
			return (string)resDataHash["account_type"];
		}

		public string GetResDataDataKey()
		{
			return (string)resDataHash["data_key"];
		}

		public string[] GetDataKeys()
		{
			int count = dataKeyStack.Count;
			string[] array = new string[count];
			IEnumerator enumerator = dataKeyStack.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num++] = (string)enumerator.Current;
			}
			return array;
		}

		public string GetCardLevelResult()
		{
			return "";
		}

		public string GetExpPaymentType(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["payment_type"];
		}

		public string GetExpCustId(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["cust_id"];
		}

		public string GetExpPhone(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["phone"];
		}

		public string GetExpEmail(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["email"];
		}

		public string GetExpCryptType(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["crypt_type"];
		}

		public string GetExpAvsStreetNumber(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["avs_street_number"];
		}

		public string GetExpAvsStreetName(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["avs_street_name"];
		}

		public string GetExpAvsZipCode(string dataKey)
		{
			return (string)((Hashtable)dataKeyHash[dataKey])["avs_zipcode"];
		}

		public string GetMpiType()
		{
			if (isMPI)
			{
				return (string)responseDataHash["type"];
			}
			return (string)responseDataHash["MpiType"];
		}

		public string GetMpiSuccess()
		{
			if (isMPI)
			{
				return (string)responseDataHash["success"];
			}
			return (string)responseDataHash["MpiSuccess"];
		}

		public string GetMpiMessage()
		{
			if (isMPI)
			{
				return (string)responseDataHash["message"];
			}
			return (string)responseDataHash["MpiMessage"];
		}

		public string GetMpiPaReq()
		{
			if (isMPI)
			{
				return (string)responseDataHash["PaReq"];
			}
			return (string)responseDataHash["MpiPaReq"];
		}

		public string GetMpiTermUrl()
		{
			if (isMPI)
			{
				return (string)responseDataHash["TermUrl"];
			}
			return (string)responseDataHash["MpiTermUrl"];
		}

		public string GetMpiMD()
		{
			if (isMPI)
			{
				return (string)responseDataHash["MD"];
			}
			return (string)responseDataHash["MpiMD"];
		}

		public string GetMpiACSUrl()
		{
			if (isMPI)
			{
				return (string)responseDataHash["ACSUrl"];
			}
			return (string)responseDataHash["MpiACSUrl"];
		}

		public string GetMpiCavv()
		{
			if (isMPI)
			{
				return (string)responseDataHash["cavv"];
			}
			return (string)responseDataHash["MpiCavv"];
		}

		public string GetMpiPAResVerified()
		{
			if (isMPI)
			{
				return (string)responseDataHash["PAResVerified"];
			}
			return (string)responseDataHash["MpiPAResVerified"];
		}

		public string GetMpiEci()
		{
			if (isMPI)
			{
				return (string)responseDataHash["eci"];
			}
			return (string)responseDataHash["eci"];
		}

		public string GetInLineForm()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<html><head><title>Title for Page</title></head>\n");
			stringBuilder.Append("<SCRIPT LANGUAGE=\"Javascript\">\n");
			stringBuilder.Append("<!--\n");
			stringBuilder.Append("function OnLoadEvent()\n");
			stringBuilder.Append("{\n");
			stringBuilder.Append("document.downloadForm.submit();\n");
			stringBuilder.Append("}\n");
			stringBuilder.Append("-->\n");
			stringBuilder.Append("</SCRIPT>\n");
			stringBuilder.Append("<body onload=\"OnLoadEvent()\">\n");
			stringBuilder.Append("<form name=\"downloadForm\" action=\"" + GetMpiACSUrl() + "\" method=\"POST\">\n");
			stringBuilder.Append("<noscript>\n");
			stringBuilder.Append("<br>\n");
			stringBuilder.Append("<br>\n");
			stringBuilder.Append("<center>\n");
			stringBuilder.Append("<h1>Processing your 3-D Secure Transaction</h1>\n");
			stringBuilder.Append("<h2>\n");
			stringBuilder.Append("JavaScript is currently disabled or is not supported\n");
			stringBuilder.Append("by your browser.<br>\n");
			stringBuilder.Append("<h3>Please click on the Submit button to continue\n");
			stringBuilder.Append("the processing of your 3-D secure\n");
			stringBuilder.Append("transaction.</h3>");
			stringBuilder.Append("<input type=\"submit\" value=\"Submit\">\n");
			stringBuilder.Append("</center>\n");
			stringBuilder.Append("</noscript>\n");
			stringBuilder.Append("<input type=\"hidden\" name=\"PaReq\" value=\"" + GetMpiPaReq() + "\">\n");
			stringBuilder.Append("<input type=\"hidden\" name=\"MD\" value=\"" + GetMpiMD() + "\">\n");
			stringBuilder.Append("<input type=\"hidden\" name=\"TermUrl\" value=\"" + GetMpiTermUrl() + "\">\n");
			stringBuilder.Append("</form>\n");
			stringBuilder.Append("</body>\n");
			stringBuilder.Append("</html>\n");
			return stringBuilder.ToString();
		}

		public string GetCurrencyCode()
		{
			return (string)vmeDataHash["currencyCode"];
		}

		public string GetPaymentTotal()
		{
			return (string)vmeDataHash["total"];
		}

		public string GetUserFirstName()
		{
			return (string)vmeDataHash["userFirstName"];
		}

		public string GetUserLastName()
		{
			return (string)vmeDataHash["userLastName"];
		}

		public string GetUserName()
		{
			return (string)vmeDataHash["userName"];
		}

		public string GetUserEmail()
		{
			return (string)vmeDataHash["userEmail"];
		}

		public string GetEncUserId()
		{
			return (string)vmeDataHash["encUserId"];
		}

		public string GetCreationTimeStamp()
		{
			return (string)vmeDataHash["creationTimeStamp"];
		}

		public string GetNameOnCard()
		{
			return (string)vmeDataHash["billingNameOnCard"];
		}

		public string GetExpirationDateMonth()
		{
			return (string)vmeDataHash["billingMonth"];
		}

		public string GetExpirationDateYear()
		{
			return (string)vmeDataHash["billingYear"];
		}

		public string GetLastFourDigits()
		{
			return (string)vmeDataHash["billingLastFourDigits"];
		}

		public string GetBinSixDigits()
		{
			return (string)vmeDataHash["billingBinSixDigits"];
		}

		public string GetCardBrand()
		{
			return (string)vmeDataHash["billingCardBrand"];
		}

		public string GetVdotMeCardType()
		{
			return (string)vmeDataHash["billingCardType"];
		}

		public string GetPersonName()
		{
			return (string)vmeDataHash["billingPersonName"];
		}

		public string GetBillingAddressLine1()
		{
			return (string)vmeDataHash["billingLine1"];
		}

		public string GetBillingCity()
		{
			return (string)vmeDataHash["billingCity"];
		}

		public string GetBillingStateProvinceCode()
		{
			return (string)vmeDataHash["billingStateProvinceCode"];
		}

		public string GetBillingPostalCode()
		{
			return (string)vmeDataHash["billingPostalCode"];
		}

		public string GetBillingCountryCode()
		{
			return (string)vmeDataHash["billingCountryCode"];
		}

		public string GetBillingPhone()
		{
			return (string)vmeDataHash["billingPhone"];
		}

		public string GetBillingVerificationStatus()
		{
			return (string)vmeDataHash["billingVerificationStatus"];
		}

		public string GetBillingId()
		{
			return (string)vmeDataHash["billingId"];
		}

		public string GetPartialShippingCountryCode()
		{
			return (string)vmeDataHash["partialShippingCountryCode"];
		}

		public string GetPartialShippingPostalCode()
		{
			return (string)vmeDataHash["partialShippingPostalCode"];
		}

		public string GetShippingPersonName()
		{
			return (string)vmeDataHash["shippingPersonName"];
		}

		public string GetShipAddressLine1()
		{
			return (string)vmeDataHash["shippingLine1"];
		}

		public string GetShippingCity()
		{
			return (string)vmeDataHash["shippingCity"];
		}

		public string GetShippingStateProvinceCode()
		{
			return (string)vmeDataHash["shippingStateProvinceCode"];
		}

		public string GetShippingPostalCode()
		{
			return (string)vmeDataHash["shippingPostalCode"];
		}

		public string GetShippingCountryCode()
		{
			return (string)vmeDataHash["shippingCountryCode"];
		}

		public string GetShippingPhone()
		{
			return (string)vmeDataHash["shippingPhone"];
		}

		public string GetShippingVerificationStatus()
		{
			return (string)vmeDataHash["shippingVerificationStatus"];
		}

		public string GetShippingId()
		{
			return (string)vmeDataHash["shippingId"];
		}

		public string GetShippingDefault()
		{
			return (string)vmeDataHash["shippingDefault"];
		}

		public string GetIsExpired()
		{
			return (string)vmeDataHash["billingExpired"];
		}

		public string GetBaseImageFileName()
		{
			return (string)vmeDataHash["billingBaseImageFileName"];
		}

		public string GetHeight()
		{
			return (string)vmeDataHash["billingHeight"];
		}

		public string GetWidth()
		{
			return (string)vmeDataHash["billingWidth"];
		}

		public string GetIssuerBid()
		{
			return (string)vmeDataHash["issuerBid"];
		}

		public string GetRiskAdvice()
		{
			return (string)vmeDataHash["advice"];
		}

		public string GetRiskScore()
		{
			return (string)vmeDataHash["score"];
		}

		public string GetAvsResponseCode()
		{
			return (string)vmeDataHash["avsResponseCode"];
		}

		public string GetCvvResponseCode()
		{
			return (string)vmeDataHash["cvvResponseCode"];
		}

		public string GetMPRequestToken()
		{
			return (string)responseDataHash["MPRequestToken"];
		}

		public string GetMPRedirectUrl()
		{
			return (string)responseDataHash["MPRedirectUrl"];
		}

		public string GetCardBrandId()
		{
			return (string)paypassInfoHash["CardBrandId"];
		}

		public string GetCardBrandName()
		{
			return (string)paypassInfoHash["CardBrandName"];
		}

		public string GetCardBillingAddressCity()
		{
			return (string)paypassInfoHash["CardBillingAddressCity"];
		}

		public string GetCardBillingAddressCountry()
		{
			return (string)paypassInfoHash["CardBillingAddressCountry"];
		}

		public string GetCardBillingAddressCountrySubdivision()
		{
			return (string)paypassInfoHash["CardBillingAddressCountrySubdivision"];
		}

		public string GetCardBillingAddressLine1()
		{
			return (string)paypassInfoHash["CardBillingAddressLine1"];
		}

		public string GetCardBillingAddressLine2()
		{
			return (string)paypassInfoHash["CardBillingAddressLine2"];
		}

		public string GetCardBillingAddressPostalCode()
		{
			return (string)paypassInfoHash["CardBillingAddressPostalCode"];
		}

		public string GetCardCardHolderName()
		{
			return (string)paypassInfoHash["CardCardHolderName"];
		}

		public string GetCardExpiryMonth()
		{
			return (string)paypassInfoHash["CardExpiryMonth"];
		}

		public string GetCardExpiryYear()
		{
			return (string)paypassInfoHash["CardExpiryYear"];
		}

		public string GetTransactionId()
		{
			return (string)paypassInfoHash["TransactionId"];
		}

		public string GetContactEmailAddress()
		{
			return (string)paypassInfoHash["ContactEmailAddress"];
		}

		public string GetContactFirstName()
		{
			return (string)paypassInfoHash["ContactFirstName"];
		}

		public string GetContactLastName()
		{
			return (string)paypassInfoHash["ContactLastName"];
		}

		public string GetContactPhoneNumber()
		{
			return (string)paypassInfoHash["ContactPhoneNumber"];
		}

		public string GetShippingAddressCity()
		{
			return (string)paypassInfoHash["ShippingAddressCity"];
		}

		public string GetShippingAddressCountry()
		{
			return (string)paypassInfoHash["ShippingAddressCountry"];
		}

		public string GetShippingAddressCountrySubdivision()
		{
			return (string)paypassInfoHash["ShippingAddressCountrySubdivision"];
		}

		public string GetShippingAddressLine1()
		{
			return (string)paypassInfoHash["ShippingAddressLine1"];
		}

		public string GetShippingAddressLine2()
		{
			return (string)paypassInfoHash["ShippingAddressLine2"];
		}

		public string GetShippingAddressPostalCode()
		{
			return (string)paypassInfoHash["ShippingAddressPostalCode"];
		}

		public string GetShippingAddressRecipientName()
		{
			return (string)paypassInfoHash["ShippingAddressRecipientName"];
		}

		public string GetShippingAddressRecipientPhoneNumber()
		{
			return (string)paypassInfoHash["ShippingAddressRecipientPhoneNumber"];
		}

		public string GetPayPassWalletIndicator()
		{
			return (string)paypassInfoHash["PayPassWalletIndicator"];
		}

		public string GetAuthenticationOptionsAuthenticateMethod()
		{
			return (string)paypassInfoHash["AuthenticationOptionsAuthenticateMethod"];
		}

		public string GetAuthenticationOptionsCardEnrollmentMethod()
		{
			return (string)paypassInfoHash["AuthenticationOptionsCardEnrollmentMethod"];
		}

		public string GetCardAccountNumber()
		{
			return (string)paypassInfoHash["CardAccountNumber"];
		}
	}
}
