using System.Collections;
using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class BatchClose : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"ecr_number"
		};

		public BatchClose()
			: base(xmlTags)
		{
		}

		public BatchClose(Hashtable purchase)
			: base(purchase, xmlTags)
		{
		}

		public void SetEcrno(string ecr_number)
		{
			transactionParams.Add("ecr_number", ecr_number);
		}

		public BatchClose(string ecr_number)
			: base(xmlTags)
		{
			transactionParams.Add("ecr_number", ecr_number);
		}

		public override string toXML(string country)
		{
			string text = ((!country.Equals("US")) ? "batchclose" : "us_batchclose");
			return "<" + text + ">" + toXML() + "</" + text + ">";
		}
	}
}
