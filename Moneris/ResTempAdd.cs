using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResTempAdd : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[9]
		{
			"pan",
			"expdate",
			"crypt_type",
			"duration",
			"cust_id",
			"phone",
			"email",
			"note",
			"data_key_format"
		};

		public ResTempAdd()
			: base(xmlTags)
		{
		}

		public ResTempAdd(Hashtable restempadd)
			: base(restempadd, xmlTags)
		{
		}

		public ResTempAdd(string pan, string expdate, string crypt_type, string duration)
			: base(xmlTags)
		{
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("crypt_type", crypt_type);
			transactionParams.Add("duration", duration);
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

		public void SetDuration(string duration)
		{
			transactionParams.Add("duration", duration);
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

		public void SetDataKeyFormat(string data_key_format)
		{
			transactionParams.Add("data_key_format", data_key_format);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_temp_add" : "us_res_temp_add");
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
