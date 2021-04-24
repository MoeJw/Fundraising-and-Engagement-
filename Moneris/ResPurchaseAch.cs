using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResPurchaseAch : Transaction
	{
		private CustInfo custInfo = new CustInfo();

		private Recur recurInfo;

		private AvsInfo avsInfo;

		private CvdInfo cvdInfo;

		private static string[] xmlTags = new string[4]
		{
			"data_key",
			"order_id",
			"cust_id",
			"amount"
		};

		public ResPurchaseAch()
			: base(xmlTags)
		{
		}

		public ResPurchaseAch(string data_key, string order_id, string amount)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
		}

		public ResPurchaseAch(string data_key, string order_id, string cust_id, string amount)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
		}

		public ResPurchaseAch(string data_key, string order_id, string amount, CustInfo cust_info)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			custInfo = cust_info;
		}

		public ResPurchaseAch(string data_key, string order_id, string cust_id, string amount, CustInfo cust_info)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			custInfo = cust_info;
		}

		public void SetDataKey(string value)
		{
			transactionParams.Add("data_key", value);
		}

		public void SetOrderId(string value)
		{
			transactionParams.Add("order_id", value);
		}

		public void SetCustId(string value)
		{
			transactionParams.Add("cust_id", value);
		}

		public void SetAmount(string value)
		{
			transactionParams.Add("amount", value);
		}

		public void SetCryptType(string value)
		{
			transactionParams.Add("crypt_type", value);
		}

		public void SetHttpAccept(string value)
		{
			transactionParams.Add("httpAccept", value);
		}

		public void SetUserAgent(string value)
		{
			transactionParams.Add("userAgent", value);
		}

		public void SetCustInfo(CustInfo cust)
		{
			custInfo = cust;
		}

		public void SetRecur(Recur recur)
		{
			recurInfo = recur;
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_purchase_ach" : "us_res_purchase_ach");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (!custInfo.IsEmpty())
			{
				stringBuilder.Append(custInfo.toXML());
			}
			if (recurInfo != null)
			{
				stringBuilder.Append(recurInfo.toXML());
			}
			if (avsInfo != null)
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			if (cvdInfo != null)
			{
				stringBuilder.Append(cvdInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
