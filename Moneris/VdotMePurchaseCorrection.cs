using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class VdotMePurchaseCorrection : Transaction
	{
		private static string[] xmlTags = new string[6]
		{
			"order_id",
			"txn_number",
			"crypt_type",
			"ship_indicator",
			"cust_id",
			"dynamic_descriptor"
		};

		public VdotMePurchaseCorrection()
			: base(xmlTags)
		{
		}

		public VdotMePurchaseCorrection(Hashtable purchasecorrection)
			: base(purchasecorrection, xmlTags)
		{
		}

		public VdotMePurchaseCorrection(string order_id, string txn_num, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_num);
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

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetShipIndicator(string ship_indicator)
		{
			transactionParams.Add("ship_indicator", ship_indicator);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "vdotme_purchasecorrection" : "us_vdotme_purchasecorrection");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
