using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResLookupFull : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"data_key"
		};

		public ResLookupFull()
			: base(xmlTags)
		{
		}

		public ResLookupFull(Hashtable reslookupfull)
			: base(reslookupfull, xmlTags)
		{
		}

		public ResLookupFull(string data_key)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_lookup_full" : "us_res_lookup_full");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
