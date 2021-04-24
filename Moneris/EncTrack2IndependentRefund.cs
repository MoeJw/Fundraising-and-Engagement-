using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class EncTrack2IndependentRefund : Transaction
	{
		private static string[] xmlTags = new string[7]
		{
			"order_id",
			"cust_id",
			"amount",
			"enc_track2",
			"pos_code",
			"device_type",
			"dynamic_descriptor"
		};

		public EncTrack2IndependentRefund()
			: base(xmlTags)
		{
		}

		public EncTrack2IndependentRefund(string order_id, string amount, string enc_track2, string pos_code, string device_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("pos_code", pos_code);
			transactionParams.Add("device_type", device_type);
		}

		public EncTrack2IndependentRefund(string order_id, string cust_id, string amount, string enc_track2, string pos_code, string device_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("pos_code", pos_code);
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

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetEncTrack2(string enc_track2)
		{
			transactionParams.Add("enc_track2", enc_track2);
		}

		public void SetPosCode(string pos_code)
		{
			transactionParams.Add("pos_code", pos_code);
		}

		public void SetDeviceType(string device_type)
		{
			transactionParams.Add("device_type", device_type);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "enc_track2_ind_refund" : "us_enc_track2_ind_refund");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
