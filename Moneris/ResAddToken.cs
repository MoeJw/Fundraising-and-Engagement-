using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResAddToken : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[10]
		{
			"data_key",
			"crypt_type",
			"avs_address",
			"avs_zipcode",
			"expdate",
			"cust_id",
			"phone",
			"email",
			"note",
			"data_key_format"
		};

		public ResAddToken()
			: base(xmlTags)
		{
		}

		public ResAddToken(Hashtable resaddtoken)
			: base(resaddtoken, xmlTags)
		{
		}

		public ResAddToken(string data_key, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
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

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetExpdate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
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

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDataKeyFormat(string note)
		{
			transactionParams.Add("data_key_format", note);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_add_token" : "us_res_add_token");
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
