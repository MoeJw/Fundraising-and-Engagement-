using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class PaypassPurchase : Transaction
	{
		private static string[] xmlTags = new string[6]
		{
			"order_id",
			"cust_id",
			"amount",
			"mp_request_token",
			"crypt_type",
			"dynamic_descriptor"
		};

		public PaypassPurchase()
			: base(xmlTags)
		{
		}

		public PaypassPurchase(Hashtable paypass_purchase)
			: base(paypass_purchase, xmlTags)
		{
		}

		public PaypassPurchase(string order_id, string amount, string mp_request_token, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("mp_request_token", mp_request_token);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public PaypassPurchase(string order_id, string cust_id, string amount, string mp_request_token, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("mp_request_token", mp_request_token);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetMpRequestToken(string mp_request_token)
		{
			transactionParams.Add("mp_request_token", mp_request_token);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "paypass_purchase" : "us_paypass_purchase");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
