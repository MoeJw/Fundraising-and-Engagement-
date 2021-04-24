using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class VdotMeInfo : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"callid"
		};

		public VdotMeInfo()
			: base(xmlTags)
		{
		}

		public void SetCallId(string callid)
		{
			transactionParams.Add("callid", callid);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "vdotme_getpaymentinfo" : "us_vdotme_getpaymentinfo");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
