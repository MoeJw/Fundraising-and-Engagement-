using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Recur
	{
		private static string[] format = new string[6]
		{
			"recur_unit",
			"start_now",
			"start_date",
			"num_recurs",
			"period",
			"recur_amount"
		};

		private Hashtable data;

		public Recur(Hashtable recurInfo)
		{
			data = recurInfo;
		}

		public Recur(string recur_unit, string start_now, string start_date, string num_recurs, string period, string recur_amount)
		{
			data = new Hashtable();
			data.Add("recur_unit", recur_unit);
			data.Add("start_now", start_now);
			data.Add("start_date", start_date);
			data.Add("num_recurs", num_recurs);
			data.Add("period", period);
			data.Add("recur_amount", recur_amount);
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<recur>");
			string[] array = format;
			foreach (string text in array)
			{
				string text2 = (string)data[text];
				stringBuilder.Append("<" + text + ">" + text2 + "</" + text + ">");
			}
			stringBuilder.Append("</recur>");
			return stringBuilder.ToString();
		}
	}
}
