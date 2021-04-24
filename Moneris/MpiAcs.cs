using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class MpiAcs : Transaction
	{
		private static string[] xmlTags = new string[2]
		{
			"PaRes",
			"MD"
		};

		public MpiAcs()
			: base(xmlTags)
		{
			Set3DsecureTransaction();
		}

		public MpiAcs(Hashtable txn)
			: base(xmlTags)
		{
			Set3DsecureTransaction();
		}

		public MpiAcs(string PaRes, string MD)
			: base(xmlTags)
		{
			Set3DsecureTransaction();
			transactionParams.Add("PaRes", PaRes);
			transactionParams.Add("MD", MD);
		}

		public void SetPaRes(string PaRes)
		{
			transactionParams.Add("PaRes", PaRes);
		}

		public void SetMD(string MD)
		{
			transactionParams.Add("MD", MD);
		}

		public override string toXML(string country)
		{
			return "<acs>" + toXML() + "</acs>";
		}
	}
}
