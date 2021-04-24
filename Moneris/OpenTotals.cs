using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class OpenTotals : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"ecr_number"
		};

		public OpenTotals()
			: base(xmlTags)
		{
		}

		public OpenTotals(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public OpenTotals(string ecr_number)
			: base(xmlTags)
		{
			transactionParams.Add("ecr_number", ecr_number);
		}

		public void SetEcrno(string ecr_number)
		{
			transactionParams.Add("ecr_number", ecr_number);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "opentotals" : "us_opentotals");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
