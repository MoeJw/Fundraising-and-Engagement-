using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class EncResUpdateCC : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[10]
		{
			"data_key",
			"enc_track2",
			"device_type",
			"cust_id",
			"phone",
			"email",
			"note",
			"crypt_type",
			"avs_address",
			"avs_zipcode"
		};

		public EncResUpdateCC()
			: base(xmlTags)
		{
		}

		public EncResUpdateCC(string data_key)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
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

		public void SetEncTrack2(string enc_track2)
		{
			transactionParams.Add("enc_track2", enc_track2);
		}

		public void SetDeviceType(string device_type)
		{
			transactionParams.Add("device_type", device_type);
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

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "enc_res_update_cc" : "us_enc_res_update_cc");
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
