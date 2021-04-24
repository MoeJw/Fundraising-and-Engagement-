using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResDelete : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"data_key"
		};

		public ResDelete()
			: base(xmlTags)
		{
		}

		public ResDelete(Hashtable resdelete)
			: base(resdelete, xmlTags)
		{
		}

		public ResDelete(string data_key)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_delete" : "us_res_delete");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
