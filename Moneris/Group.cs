using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Group : Transaction
	{
		private static string[] xmlTags = new string[4]
		{
			"order_id",
			"txn_number",
			"group_ref_num",
			"group_type"
		};

		public Group()
			: base(xmlTags)
		{
		}

		public Group(Hashtable group)
			: base(group, xmlTags)
		{
		}

		public Group(string order_id, string txn_number, string group_ref_num, string group_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("group_ref_num", group_ref_num);
			transactionParams.Add("group_type", group_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetGroupRefNum(string group_ref_num)
		{
			transactionParams.Add("group_ref_num", group_ref_num);
		}

		public void SetGroupType(string group_type)
		{
			transactionParams.Add("group_type", group_type);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "group" : "us_group");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
