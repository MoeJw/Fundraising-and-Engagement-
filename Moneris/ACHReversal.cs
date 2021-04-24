using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ACHReversal : Transaction
	{
		private static string[] xmlTags = new string[2]
		{
			"order_id",
			"txn_number"
		};

		public ACHReversal()
			: base(xmlTags)
		{
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
			string str = ((!country.Equals("US")) ? "ach_reversal" : "us_ach_reversal");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
