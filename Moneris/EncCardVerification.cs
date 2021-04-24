using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class EncCardVerification : Transaction
	{
		private AvsInfo avsInfo;

		private CvdInfo cvdInfo;

		private static string[] xmlTags = new string[5]
		{
			"order_id",
			"cust_id",
			"enc_track2",
			"device_type",
			"crypt_type"
		};

		public EncCardVerification()
			: base(xmlTags)
		{
		}

		public EncCardVerification(Hashtable encpreauth)
			: base(encpreauth, xmlTags)
		{
		}

		public EncCardVerification(string order_id, string enc_track2, string device_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
		}

		public EncCardVerification(string order_id, string cust_id, string enc_track2, string device_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
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

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
		}

		public void SetCvdInfo(CvdInfo cvd)
		{
			cvdInfo = cvd;
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "enc_card_verification" : "us_enc_card_verification");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (avsInfo != null)
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			if (cvdInfo != null)
			{
				stringBuilder.Append(cvdInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
