using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResUpdateAch : Transaction
	{
		private ACHInfo achInfo;

		private Hashtable keyHashes = new Hashtable();

		private static string[] xmlTags = new string[5]
		{
			"data_key",
			"cust_id",
			"phone",
			"email",
			"note"
		};

		public ResUpdateAch()
			: base(xmlTags)
		{
		}

		public ResUpdateAch(Hashtable usresupdateach)
			: base(usresupdateach, xmlTags)
		{
		}

		public ResUpdateAch(string data_key)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetAchInfo(ACHInfo ach)
		{
			achInfo = ach;
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetPhone(string phone)
		{
			transactionParams.Add("phone", phone);
		}

		public void SetEmail(string email)
		{
			transactionParams.Add("email", email);
		}

		public void SetNote(string note)
		{
			transactionParams.Add("note", note);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_update_ach" : "us_res_update_ach");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (achInfo != null)
			{
				stringBuilder.Append(achInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
