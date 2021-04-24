using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class PaypassTxn : Transaction
	{
		private static string[] xmlTags = new string[7]
		{
			"xid",
			"amount",
			"mp_request_token",
			"MD",
			"merchantUrl",
			"accept",
			"userAgent"
		};

		public PaypassTxn()
			: base(xmlTags)
		{
		}

		public PaypassTxn(Hashtable paypass_txn)
			: base(paypass_txn, xmlTags)
		{
		}

		public PaypassTxn(string xid, string amount, string mp_request_token, string MD, string merchantUrl, string accept, string userAgent)
			: base(xmlTags)
		{
			transactionParams.Add("xid", xid);
			transactionParams.Add("amount", amount);
			transactionParams.Add("mp_request_token", mp_request_token);
			transactionParams.Add("MD", MD);
			transactionParams.Add("merchantUrl", merchantUrl);
			transactionParams.Add("accept", accept);
			transactionParams.Add("userAgent", userAgent);
		}

		public void SetXid(string xid)
		{
			transactionParams.Add("xid", xid);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetMpRequestToken(string mp_request_token)
		{
			transactionParams.Add("mp_request_token", mp_request_token);
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
			string str = ((!country.Equals("US")) ? "paypass_txn" : "us_paypass_txn");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
