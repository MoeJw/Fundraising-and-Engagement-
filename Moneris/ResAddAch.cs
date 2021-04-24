using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResAddAch : Transaction
	{
		private ACHInfo achInfo;

		private static string[] xmlTags = new string[4]
		{
			"cust_id",
			"phone",
			"email",
			"note"
		};

		public ResAddAch(Hashtable usresaddach)
			: base(usresaddach, xmlTags)
		{
		}

		public ResAddAch()
			: base(xmlTags)
		{
		}

		public ResAddAch(string cust_id)
			: base(xmlTags)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAchInfo(ACHInfo ach)
		{
			achInfo = ach;
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

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_add_ach" : "us_res_add_ach");
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
