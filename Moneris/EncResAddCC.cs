using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class EncResAddCC : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[9]
		{
			"enc_track2",
			"device_type",
			"crypt_type",
			"cust_id",
			"avs_address",
			"avs_zipcode",
			"phone",
			"email",
			"note"
		};

		public EncResAddCC()
			: base(xmlTags)
		{
		}

		public EncResAddCC(Hashtable resaddcc)
			: base(resaddcc, xmlTags)
		{
		}

		public EncResAddCC(string enc_track2, string device_type, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetEncTrack2(string enc_track2)
		{
			transactionParams.Add("enc_track2", enc_track2);
		}

		public void SetDeviceType(string device_type)
		{
			transactionParams.Add("device_type", device_type);
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
			string str = ((!country.Equals("US")) ? "enc_res_add_cc" : "us_enc_res_add_cc");
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
