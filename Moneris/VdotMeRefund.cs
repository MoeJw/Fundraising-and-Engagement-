using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class VdotMeRefund : Transaction
	{
		private static string[] xmlTags = new string[6]
		{
			"order_id",
			"txn_number",
			"amount",
			"crypt_type",
			"cust_id",
			"dynamic_descriptor"
		};

		public VdotMeRefund()
			: base(xmlTags)
		{
		}

		public VdotMeRefund(Hashtable refund)
			: base(refund, xmlTags)
		{
		}

		public VdotMeRefund(string order_id, string txn_number, string amount, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("amount", amount);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
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
			string str = ((!country.Equals("US")) ? "vdotme_refund" : "us_vdotme_refund");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
