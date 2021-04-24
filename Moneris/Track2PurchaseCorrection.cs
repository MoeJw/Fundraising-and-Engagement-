using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Track2PurchaseCorrection : Transaction
	{
		private static string[] xmlTags = new string[4]
		{
			"order_id",
			"txn_number",
			"cust_id",
			"dynamic_descriptor"
		};

		public Track2PurchaseCorrection()
			: base(xmlTags)
		{
		}

		public Track2PurchaseCorrection(Hashtable track2purchasecorrection)
			: base(track2purchasecorrection, xmlTags)
		{
		}

		public Track2PurchaseCorrection(string order_id, string txn_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "track2_purchasecorrection" : "us_track2_purchasecorrection");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("<track2></track2>");
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
