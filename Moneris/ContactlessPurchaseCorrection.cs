using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class ContactlessPurchaseCorrection : Transaction
	{
		private static string[] xmlTags = new string[2]
		{
			"order_id",
			"txn_number"
		};

		public ContactlessPurchaseCorrection()
			: base(xmlTags)
		{
		}

		public ContactlessPurchaseCorrection(Hashtable contactlesspurchasecorrection)
			: base(contactlesspurchasecorrection, xmlTags)
		{
		}

		public ContactlessPurchaseCorrection(string order_id, string txn_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "contactless_purchasecorrection" : "us_contactless_purchasecorrection");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
