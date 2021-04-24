using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class IndependentRefund : Transaction
	{
		private static string[] xmlTags = new string[11]
		{
			"order_id",
			"cust_id",
			"amount",
			"pan",
			"expdate",
			"crypt_type",
			"commcard_invoice",
			"commcard_tax_amount",
			"dynamic_descriptor",
			"mcp_amount",
			"mcp_currency_code"
		};

		public IndependentRefund()
			: base(xmlTags)
		{
		}

		public IndependentRefund(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public IndependentRefund(string order_id, string amount, string pan, string expdate, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public IndependentRefund(string order_id, string cust_id, string amount, string pan, string expdate, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
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

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetCommcardInvoice(string commcard_invoice)
		{
			transactionParams.Add("commcard_invoice", commcard_invoice);
		}

		public void SetCommcardTaxAmount(string commcard_tax_amount)
		{
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
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
			string str = ((!country.Equals("US")) ? "ind_refund" : "us_ind_refund");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
