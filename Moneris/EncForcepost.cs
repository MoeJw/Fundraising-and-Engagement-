using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class EncForcepost : Transaction
	{
		private static string[] xmlTags = new string[8]
		{
			"order_id",
			"cust_id",
			"amount",
			"enc_track2",
			"device_type",
			"auth_code",
			"crypt_type",
			"dynamic_descriptor"
		};

		public EncForcepost()
			: base(xmlTags)
		{
		}

		public EncForcepost(Hashtable encforcepost)
			: base(encforcepost, xmlTags)
		{
		}

		public EncForcepost(string order_id, string amount, string enc_track2, string device_type, string auth_code, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("auth_code", auth_code);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public EncForcepost(string order_id, string cust_id, string amount, string enc_track2, string device_type, string auth_code, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("auth_code", auth_code);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetEncTrack2(string enc_track2)
		{
			transactionParams.Add("enc_track2", enc_track2);
		}

		public void SetDeviceType(string device_type)
		{
			transactionParams.Add("device_type", device_type);
		}

		public void SetAuthCode(string auth_code)
		{
			transactionParams.Add("auth_code", auth_code);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "enc_forcepost" : "us_enc_forcepost");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
