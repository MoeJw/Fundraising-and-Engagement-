using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResIndRefundCC : Transaction
	{
		private static string[] xmlTags = new string[6]
		{
			"data_key",
			"order_id",
			"cust_id",
			"amount",
			"crypt_type",
			"dynamic_descriptor"
		};

		public ResIndRefundCC()
			: base(xmlTags)
		{
		}

		public ResIndRefundCC(Hashtable ResIndRefundCC)
			: base(ResIndRefundCC, xmlTags)
		{
		}

		public ResIndRefundCC(string data_key, string order_id, string amount, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public ResIndRefundCC(string data_key, string order_id, string cust_id, string amount, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_ind_refund_cc" : "us_res_ind_refund_cc");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
