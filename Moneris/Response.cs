using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace Moneris
{
	[ComVisible(true)]
	public class Response
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

		public Response(Stream aStream)
		{
			inStream = aStream;
			xtr = new XmlTextReader(inStream);
			doParse();
			xtr.Close();
		}

		public Response(string errorMessage)
		{
			string s = "<?xml version=\"1.0\"?><response><receipt><ReceiptId>Global Error Receipt</ReceiptId><ReferenceNum>null</ReferenceNum><ResponseCode>null</ResponseCode><ISO>null</ISO> <AuthCode>null</AuthCode><TransTime>null</TransTime><TransDate>null</TransDate><TransType>null</TransType><Complete>false</Complete><Message>" + errorMessage + "</Message><TransAmount>null</TransAmount><CardType>null</CardType><TransID>null</TransID><TimedOut>null</TimedOut></receipt></response>";
			xtr = new XmlTextReader(new StringReader(s));
			doParse();
			xtr.Close();
		}

		public Receipt GetReceipt()
		{
			Receipt receipt = new Receipt();
			receipt.setTermIdHash(termIDHash);
			receipt.SetCardHash(cardHash);
			receipt.SetPurchaseHash(purchaseHash);
			receipt.SetRefundHash(refundHash);
			receipt.SetCorrectionHash(correctionHash);
			receipt.SetResponseDataHash(responseDataHash);
			receipt.SetResDataHash(resDataHash);
			receipt.SetDataKeyHash(dataKeyHash);
			receipt.SetRuleDataHash(ruleDataHash);
			receipt.SetVmeDataHash(vmeDataHash);
			receipt.SetPaypassInfoHash(paypassInfoHash);
			receipt.SetDataKeyStack(dataKeyStack);
			receipt.SetRuleStack(ruleStack);
			receipt.SetIsMPI(isMPI);
			return receipt;
		}

		private void doParse()
		{
			while (xtr.Read())
			{
				switch (xtr.NodeType)
				{
				case XmlNodeType.Element:
					beginHandler(xtr.Name);
					break;
				case XmlNodeType.Text:
					textHandler(xtr.Value);
					break;
				case XmlNodeType.EndElement:
					endHandler(xtr.Name);
					break;
				}
			}
		}

		private void beginHandler(string tag)
		{
			currentTag = tag;
			if (tag.Equals("ResolveData"))
			{
				isResolveData = true;
				resDataHash = new Hashtable();
			}
			if (tag.Equals("Rule"))
			{
				isRule = true;
			}
			if (tag.Equals("BankTotals"))
			{
				isBatchTotals = true;
				purchaseHash = new Hashtable();
				refundHash = new Hashtable();
				correctionHash = new Hashtable();
			}
			if (isBatchTotals)
			{
				if (currentTag.Equals("Purchase"))
				{
					currentTxnType = "Purchase";
				}
				else if (currentTag.Equals("Refund"))
				{
					currentTxnType = "Refund";
				}
				else if (currentTag.Equals("Correction"))
				{
					currentTxnType = "Correction";
				}
			}
			if (tag.Equals("PayPassInfo"))
			{
				isPayPassInfo = true;
			}
			if (tag.Equals("VDotMeInfo"))
			{
				isVdotMe = true;
			}
			if (isVdotMe)
			{
				string b = "paymentInstrument";
				if (currentTag == b)
				{
					inBilling = true;
				}
				if (currentTag == "partialShippingAddress")
				{
					inPartialShipping = true;
				}
				if (currentTag == "shippingAddress")
				{
					inShipping = true;
				}
			}
			if (tag.Equals("MpiResponse"))
			{
				isMPI = true;
			}
		}

		private void endHandler(string tag)
		{
			string value = "paymentInstrument";
			if (tag.Equals("BankTotals"))
			{
				isBatchTotals = false;
			}
			if (tag.Equals("ResolveData"))
			{
				isResolveData = false;
			}
			if (tag.Equals("Rule"))
			{
				isRule = false;
			}
			if (tag.Equals("VDotMeInfo"))
			{
				isVdotMe = false;
			}
			if (tag.Equals(value))
			{
				inBilling = false;
			}
			if (tag.Equals("partialShippingAddress"))
			{
				inPartialShipping = false;
			}
			if (tag.Equals("shippingAddress"))
			{
				inShipping = false;
			}
			if (tag.Equals("PayPassInfo"))
			{
				isPayPassInfo = false;
			}
		}

		private void textHandler(string data)
		{
			if (isBatchTotals)
			{
				if (currentTag.Equals("term_id"))
				{
					currentTermID = data;
					cardHash.Add(currentTermID, new Stack());
					purchaseHash.Add(currentTermID, new Hashtable());
					refundHash.Add(currentTermID, new Hashtable());
					correctionHash.Add(currentTermID, new Hashtable());
				}
				else if (currentTag.Equals("closed"))
				{
					termIDHash.Add(currentTermID, data);
				}
				else if (currentTag.Equals("CardType"))
				{
					((Stack)cardHash[currentTermID]).Push(data);
					currentCardType = data;
					((Hashtable)purchaseHash[currentTermID])[currentCardType] = new Hashtable();
					((Hashtable)refundHash[currentTermID])[currentCardType] = new Hashtable();
					((Hashtable)correctionHash[currentTermID])[currentCardType] = new Hashtable();
				}
				else if (currentTag.Equals("Amount"))
				{
					if (currentTxnType.Equals("Purchase"))
					{
						((Hashtable)((Hashtable)purchaseHash[currentTermID])[currentCardType])["Amount"] = data;
					}
					else if (currentTxnType.Equals("Refund"))
					{
						((Hashtable)((Hashtable)refundHash[currentTermID])[currentCardType])["Amount"] = data;
					}
					else if (currentTxnType.Equals("Correction"))
					{
						((Hashtable)((Hashtable)correctionHash[currentTermID])[currentCardType])["Amount"] = data;
					}
				}
				else if (currentTag.Equals("Count"))
				{
					if (currentTxnType.Equals("Purchase"))
					{
						((Hashtable)((Hashtable)purchaseHash[currentTermID])[currentCardType])["Count"] = data;
					}
					else if (currentTxnType.Equals("Refund"))
					{
						((Hashtable)((Hashtable)refundHash[currentTermID])[currentCardType])["Count"] = data;
					}
					else if (currentTxnType.Equals("Correction"))
					{
						((Hashtable)((Hashtable)correctionHash[currentTermID])[currentCardType])["Count"] = data;
					}
				}
			}
			else if (isResolveData && !data.Equals("null"))
			{
				if (currentTag.Equals("data_key"))
				{
					currentDataKey = data;
					dataKeyHash.Add(currentDataKey, new Hashtable());
					dataKeyStack.Push(currentDataKey);
				}
				else
				{
					((Hashtable)dataKeyHash[currentDataKey])[currentTag] = data;
				}
				resDataHash[currentTag] = data;
			}
			else
			{
				responseDataHash[currentTag] = data;
				if (currentTag.Equals("DataKey") && !data.Equals("null"))
				{
					currentDataKey = data;
					dataKeyHash.Add(currentDataKey, new Hashtable());
					dataKeyStack.Push(currentDataKey);
				}
			}
			if (isPayPassInfo && !data.Equals("null"))
			{
				paypassInfoHash.Add(currentTag, data);
			}
			if (isVdotMe && !data.Equals("null"))
			{
				if (inShipping)
				{
					parseShipping(data);
				}
				else if (inPartialShipping)
				{
					parsePartialShipping(data);
				}
				else if (inBilling)
				{
					parseBilling(data);
				}
				else
				{
					vmeDataHash.Add(currentTag, data);
				}
			}
			if (isRule && !data.Equals("null"))
			{
				if (currentTag.Equals("RuleName"))
				{
					currentRule = data;
					ruleDataHash.Add(currentRule, new Hashtable());
					ruleStack.Push(currentRule);
				}
				else
				{
					((Hashtable)ruleDataHash[currentRule])[currentTag] = data;
				}
			}
		}

		private void parseBilling(string data)
		{
			string key = "billing" + uCaseFirst(currentTag);
			vmeDataHash.Add(key, data);
		}

		private void parsePartialShipping(string data)
		{
			string key = "partialShipping" + uCaseFirst(currentTag);
			vmeDataHash.Add(key, data);
		}

		private void parseShipping(string data)
		{
			string key = "shipping" + uCaseFirst(currentTag);
			vmeDataHash.Add(key, data);
		}

		private static string uCaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return "";
			}
			return char.ToUpper(s[0]) + s.Substring(1);
		}
	}
}
