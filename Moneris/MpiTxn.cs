using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class MpiTxn : Transaction
	{
		private static string[] xmlTags = new string[12]
		{
			"xid",
			"amount",
			"pan",
			"expdate",
			"MD",
			"merchantUrl",
			"accept",
			"userAgent",
			"currency",
			"recurFreq",
			"recurEnd",
			"install"
		};

		public MpiTxn()
			: base(xmlTags)
		{
			Set3DsecureTransaction();
		}

		public MpiTxn(string xid, string amount, string pan, string expdate, string MD, string merchantUrl, string accept, string userAgent)
			: base(xmlTags)
		{
			Set3DsecureTransaction();
			transactionParams.Add("xid", xid);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
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

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
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

		public void SetRecurFrequency(string recurFreq)
		{
			transactionParams.Add("recurFreq", recurFreq);
		}

		public void SetRecurEnd(string recurEnd)
		{
			transactionParams.Add("recurEnd", recurEnd);
		}

		public void SetInstallment(string install)
		{
			transactionParams.Add("install", install);
		}

		public override string toXML(string country)
		{
			return "<txn>" + toXML() + "</txn>";
		}
	}
}
