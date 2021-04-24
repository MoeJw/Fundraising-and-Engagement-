using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ContactlessPurchase : Transaction
	{
		private static string[] xmlTags = new string[9]
		{
			"order_id",
			"cust_id",
			"amount",
			"pan",
			"expdate",
			"track2",
			"commcard_invoice",
			"commcard_tax_amount",
			"dynamic_descriptor"
		};

		public ContactlessPurchase()
			: base(xmlTags)
		{
		}

		public ContactlessPurchase(Hashtable contactlesspurchase)
			: base(contactlesspurchase, xmlTags)
		{
		}

		public ContactlessPurchase(string order_id, string amount, string pan, string expdate, string commcard_invoice, string commcard_tax_amount)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("commcard_invoice", commcard_invoice);
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
		}

		public ContactlessPurchase(string order_id, string cust_id, string amount, string pan, string expdate, string commcard_invoice, string commcard_tax_amount)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("commcard_invoice", commcard_invoice);
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
		}

		public void SetTrack2(string track2)
		{
			transactionParams.Add("track2", track2);
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

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetCommcardInvoice(string commcard_invoice)
		{
			transactionParams.Add("commcard_invoice", commcard_invoice);
		}

		public void SetCommcardTaxAmount(string commcard_tax_amount)
		{
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
		}

		public void SetPosCode(string poscode)
		{
			transactionParams.Add("poscode", poscode);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "contactless_purchase" : "us_contactless_purchase");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
