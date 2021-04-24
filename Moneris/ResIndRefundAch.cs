using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class ResIndRefundAch : Transaction
	{
		private static string[] xmlTags = new string[4]
		{
			"data_key",
			"order_id",
			"cust_id",
			"amount"
		};

		public ResIndRefundAch()
			: base(xmlTags)
		{
		}

		public ResIndRefundAch(Hashtable USResIndRefundAch)
			: base(USResIndRefundAch, xmlTags)
		{
		}

		public ResIndRefundAch(string data_key, string order_id, string amount)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
		}

		public ResIndRefundAch(string data_key, string order_id, string cust_id, string amount)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "res_ind_refund_ach" : "us_res_ind_refund_ach");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
