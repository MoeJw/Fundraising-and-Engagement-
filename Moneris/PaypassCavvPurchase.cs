using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class PaypassCavvPurchase : Transaction
	{
		private static string[] xmlTags = new string[7]
		{
			"order_id",
			"cavv",
			"cust_id",
			"amount",
			"mp_request_token",
			"crypt_type",
			"dynamic_descriptor"
		};

		public PaypassCavvPurchase()
			: base(xmlTags)
		{
		}

		public PaypassCavvPurchase(Hashtable paypass_cavv_purchase)
			: base(paypass_cavv_purchase, xmlTags)
		{
		}

		public PaypassCavvPurchase(string order_id, string cavv, string amount, string mp_request_token, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cavv", cavv);
			transactionParams.Add("amount", amount);
			transactionParams.Add("mp_request_token", mp_request_token);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public PaypassCavvPurchase(string order_id, string cavv, string cust_id, string amount, string mp_request_token, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cavv", cavv);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("mp_request_token", mp_request_token);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCavv(string cavv)
		{
			transactionParams.Add("cavv", cavv);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetMpRequestToken(string mp_request_token)
		{
			transactionParams.Add("mp_request_token", mp_request_token);
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
			string str = ((!country.Equals("US")) ? "paypass_cavv_purchase" : "us_paypass_cavv_purchase");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
