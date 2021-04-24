using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResMpiTxn : Transaction
	{
		private static string[] xmlTags = new string[8]
		{
			"data_key",
			"xid",
			"amount",
			"expdate",
			"MD",
			"merchantUrl",
			"accept",
			"userAgent"
		};

		public ResMpiTxn()
			: base(xmlTags)
		{
		}

		public ResMpiTxn(Hashtable resmpitxn)
			: base(resmpitxn, xmlTags)
		{
		}

		public ResMpiTxn(string data_key, string xid, string amount, string MD, string merchantUrl, string accept, string userAgent)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("xid", xid);
			transactionParams.Add("amount", amount);
			transactionParams.Add("MD", MD);
			transactionParams.Add("merchantUrl", merchantUrl);
			transactionParams.Add("accept", accept);
			transactionParams.Add("userAgent", userAgent);
		}

		public ResMpiTxn(string data_key, string xid, string amount, string MD, string merchantUrl, string accept, string userAgent, string expdate)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("xid", xid);
			transactionParams.Add("amount", amount);
			transactionParams.Add("MD", MD);
			transactionParams.Add("merchantUrl", merchantUrl);
			transactionParams.Add("accept", accept);
			transactionParams.Add("userAgent", userAgent);
			transactionParams.Add("expdate", expdate);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetXid(string xid)
		{
			transactionParams.Add("xid", xid);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetMD(string MD)
		{
			transactionParams.Add("MD", MD);
		}

		public void SetMerchantUrl(string merchantUrl)
		{
			transactionParams.Add("merchantUrl", merchantUrl);
		}

		public void SetAccept(string accept)
		{
			transactionParams.Add("accept", accept);
		}

		public void SetUserAgent(string userAgent)
		{
			transactionParams.Add("userAgent", userAgent);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_mpitxn" : "us_res_mpitxn");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
