using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class VdotMeCompletion : Transaction
	{
		private static string[] xmlTags = new string[7]
		{
			"order_id",
			"txn_number",
			"comp_amount",
			"crypt_type",
			"ship_indicator",
			"cust_id",
			"dynamic_descriptor"
		};

		public VdotMeCompletion()
			: base(xmlTags)
		{
		}

		public VdotMeCompletion(Hashtable completion)
			: base(completion, xmlTags)
		{
		}

		public VdotMeCompletion(string order_id, string txn_number, string comp_amount, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("comp_amount", comp_amount);
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

		public void SetAmount(string comp_amount)
		{
			transactionParams.Add("comp_amount", comp_amount);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetShipIndicator(string ship_indicator)
		{
			transactionParams.Add("ship_indicator", ship_indicator);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "vdotme_completion" : "us_vdotme_completion");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
