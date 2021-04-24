using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class ResGetExpiring : Transaction
	{
		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "res_get_expiring" : "us_res_get_expiring");
			return "<" + text + "></" + text + ">";
		}
	}
}
