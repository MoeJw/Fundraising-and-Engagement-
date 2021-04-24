using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResAddPinless : Transaction
	{
		private static string[] xmlTags = new string[8]
		{
			"pan",
			"presentation_type",
			"p_account_number",
			"cust_id",
			"phone",
			"email",
			"note",
			"expdate"
		};

		public ResAddPinless()
			: base(xmlTags)
		{
		}

		public ResAddPinless(Hashtable usresaddpinless)
			: base(usresaddpinless, xmlTags)
		{
		}

		public ResAddPinless(string pan, string presentation_type, string p_account_number)
			: base(xmlTags)
		{
			transactionParams.Add("pan", pan);
			transactionParams.Add("presentation_type", presentation_type);
			transactionParams.Add("p_account_number", p_account_number);
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetPresentationType(string presentation_type)
		{
			transactionParams.Add("presentation_type", presentation_type);
		}

		public void SetPAccountNumber(string p_account_number)
		{
			transactionParams.Add("p_account_number", p_account_number);
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

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_add_pinless" : "us_res_add_pinless");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
