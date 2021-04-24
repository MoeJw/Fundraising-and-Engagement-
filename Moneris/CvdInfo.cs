using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class CvdInfo
	{
		private bool empty = true;

		private Hashtable cvdParams = new Hashtable();

		private string[] cvdTags = new string[2]
		{
			"cvd_indicator",
			"cvd_value"
		};

		public void SetCvdIndicator(string value)
		{
			empty = false;
			cvdParams.Add("cvd_indicator", value);
		}

		public void SetCvdValue(string value)
		{
			empty = false;
			cvdParams.Add("cvd_value", value);
		}

		public bool IsEmpty()
		{
			return empty;
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<cvd_info>");
			toXML_low(stringBuilder, cvdTags, cvdParams);
			stringBuilder.Append("</cvd_info>");
			return stringBuilder.ToString();
		}

		private void toXML_low(StringBuilder sb, string[] xmlTags, Hashtable xmlData)
		{
			foreach (string text in xmlTags)
			{
				string text2 = (string)xmlData[text];
				sb.Append("<" + text + ">" + text2 + "</" + text + ">");
			}
		}
	}
}
