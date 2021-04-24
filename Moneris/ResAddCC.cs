using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResAddCC : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[9]
		{
			"pan",
			"expdate",
			"crypt_type",
			"cust_id",
			"phone",
			"email",
			"note",
			"get_card_type",
			"data_key_format"
		};

		public ResAddCC()
			: base(xmlTags)
		{
		}

		public ResAddCC(Hashtable resaddcc)
			: base(resaddcc, xmlTags)
		{
		}

		public ResAddCC(string pan, string expdate, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetAvsAddress(string avs_address)
		{
			transactionParams.Add("avs_address", avs_address);
		}

		public void SetAvsZipCode(string avs_zipcode)
		{
			transactionParams.Add("avs_zipcode", avs_zipcode);
		}

		public void SetGetCardType(string get_card_type)
		{
			transactionParams.Add("get_card_type", get_card_type);
		}

		public void SetDataKeyFormat(string data_key_format)
		{
			transactionParams.Add("data_key_format", data_key_format);
		}

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
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
			string str = ((!country.Equals("US")) ? "res_add_cc" : "us_res_add_cc");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (avsInfo != null)
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
