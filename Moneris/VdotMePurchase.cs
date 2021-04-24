using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class VdotMePurchase : Transaction
	{
		private static string[] xmlTags = new string[6]
		{
			"order_id",
			"cust_id",
			"amount",
			"callid",
			"crypt_type",
			"dynamic_descriptor"
		};

		public VdotMePurchase()
			: base(xmlTags)
		{
		}

		public VdotMePurchase(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public VdotMePurchase(string order_id, string amount, string call_id, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("callid", call_id);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public VdotMePurchase(string order_id, string cust_id, string amount, string call_id, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("callid", call_id);
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

		public void SetCallId(string callid)
		{
			transactionParams.Add("callid", callid);
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
			string str = ((!country.Equals("US")) ? "vdotme_purchase" : "us_vdotme_purchase");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
