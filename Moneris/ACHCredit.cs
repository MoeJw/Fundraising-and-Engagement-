using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ACHCredit : Transaction
	{
		private ACHInfo achinfo;

		private static string[] xmlTags = new string[4]
		{
			"order_id",
			"cust_id",
			"amount",
			"txn_number"
		};

		public ACHCredit()
			: base(xmlTags)
		{
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

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetAchInfo(ACHInfo ach_info)
		{
			achinfo = ach_info;
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "ach_credit" : "us_ach_credit");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (achinfo != null)
			{
				stringBuilder.Append(achinfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
