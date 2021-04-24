using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class BatchCloseAll : Transaction
	{
		private static string[] xmlTags = new string[0];

		public BatchCloseAll(Hashtable batchcloseall)
			: base(batchcloseall, xmlTags)
		{
		}

		public BatchCloseAll()
			: base(xmlTags)
		{
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "batchcloseall" : "us_batchcloseall");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
