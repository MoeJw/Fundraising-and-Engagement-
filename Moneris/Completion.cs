using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class Completion : Transaction
	{
		private static string[] xmlTags = new string[11]
		{
			"order_id",
			"amount",
			"comp_amount",
			"txn_number",
			"crypt_type",
			"commcard_invoice",
			"commcard_tax_amount",
			"dynamic_descriptor",
			"cust_id",
			"mcp_amount",
			"mcp_currency_code"
		};

		public Completion()
			: base(xmlTags)
		{
		}

		public Completion(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public Completion(string order_id, string comp_amount, string txn_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("comp_amount", comp_amount);
			transactionParams.Add("txn_number", txn_number);
		}

		public Completion(string order_id, string comp_amount, string txn_number, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("comp_amount", comp_amount);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCompAmount(string comp_amount)
		{
			transactionParams.Add("comp_amount", comp_amount);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetCommcardInvoice(string commcard_invoice)
		{
			transactionParams.Add("commcard_invoice", commcard_invoice);
		}

		public void SetCommcardTaxAmount(string commcard_tax_amount)
		{
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
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
			string text = ((!country.Equals("US")) ? "completion" : "us_completion");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
