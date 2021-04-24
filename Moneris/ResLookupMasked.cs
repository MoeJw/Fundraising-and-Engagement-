using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResLookupMasked : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"data_key"
		};

		public ResLookupMasked()
			: base(xmlTags)
		{
		}

		public ResLookupMasked(Hashtable reslookupmasked)
			: base(reslookupmasked, xmlTags)
		{
		}

		public ResLookupMasked(string data_key)
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
			string str = ((!country.Equals("US")) ? "res_lookup_masked" : "us_res_lookup_masked");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
