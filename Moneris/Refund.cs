using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class Refund : Transaction
	{
		private static string[] xmlTags = new string[8]
		{
			"order_id",
			"amount",
			"txn_number",
			"crypt_type",
			"cust_id",
			"dynamic_descriptor",
			"mcp_amount",
			"mcp_currency_code"
		};

		public Refund()
			: base(xmlTags)
		{
		}

		public Refund(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public Refund(string order_id, string amount, string txn_number, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetOrigOrderId(string orig_order_id)
		{
			transactionParams.Add("orig_order_id", orig_order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetMCPAmount(string mcp_amount)
		{
			transactionParams.Add("mcp_amount", mcp_amount);
		}

		public void SetMCPCurrencyCode(string mcp_currency_code)
		{
			transactionParams.Add("mcp_currency_code", mcp_currency_code);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "refund" : "us_refund");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
