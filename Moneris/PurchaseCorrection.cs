using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class PurchaseCorrection : Transaction
	{
		private static string[] xmlTags = new string[6]
		{
			"order_id",
			"txn_number",
			"crypt_type",
			"cust_id",
			"dynamic_descriptor",
			"mcp_currency_code"
		};

		public PurchaseCorrection()
			: base(xmlTags)
		{
		}

		public PurchaseCorrection(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public PurchaseCorrection(string order_id, string txn_number, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetCustomerID(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetMCPCurrencyCode(string mcp_currency_code)
		{
			transactionParams.Add("mcp_currency_code", mcp_currency_code);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "purchasecorrection" : "us_purchasecorrection");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
