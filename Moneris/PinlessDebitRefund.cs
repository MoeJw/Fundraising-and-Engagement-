using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class PinlessDebitRefund : Transaction
	{
		private static string[] xmlTags = new string[3]
		{
			"order_id",
			"amount",
			"txn_number"
		};

		public PinlessDebitRefund()
			: base(xmlTags)
		{
		}

		public PinlessDebitRefund(Hashtable pinless_debit_refund)
			: base(pinless_debit_refund, xmlTags)
		{
		}

		public PinlessDebitRefund(string order_id, string amount, string txn_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "us_pinless_debit_refund" : "us_pinless_debit_refund");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
