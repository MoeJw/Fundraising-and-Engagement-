using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class ForcePost : Transaction
	{
		private static string[] xmlTags = new string[8]
		{
			"order_id",
			"cust_id",
			"amount",
			"pan",
			"expdate",
			"auth_code",
			"crypt_type",
			"dynamic_descriptor"
		};

		public ForcePost()
			: base(xmlTags)
		{
		}

		public ForcePost(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public ForcePost(string order_id, string amount, string pan, string expdate, string auth_code, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("auth_code", auth_code);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public ForcePost(string order_id, string cust_id, string amount, string pan, string expdate, string auth_code, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
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

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
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
			string text = ((!country.Equals("US")) ? "forcepost" : "us_forcepost");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
