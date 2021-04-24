using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResUpdatePinless : Transaction
	{
		private static string[] xmlTags = new string[9]
		{
			"data_key",
			"cust_id",
			"phone",
			"email",
			"note",
			"pan",
			"expdate",
			"presentation_type",
			"p_account_number"
		};

		public ResUpdatePinless()
			: base(xmlTags)
		{
		}

		public ResUpdatePinless(Hashtable resupdatepinless)
			: base(resupdatepinless, xmlTags)
		{
		}

		public ResUpdatePinless(string data_key)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetIntendedUse(string intended_use)
		{
			transactionParams.Add("intended_use", intended_use);
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

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetPresentationType(string presentation_type)
		{
			transactionParams.Add("presentation_type", presentation_type);
		}

		public void SetPAccountNumber(string p_account_number)
		{
			transactionParams.Add("p_account_number", p_account_number);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_update_pinless" : "us_res_update_pinless");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
