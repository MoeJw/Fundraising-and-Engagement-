using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ConvFeeInfo
	{
		private bool empty = true;

		private Hashtable convFeeParams = new Hashtable();

		private string[] convFeeTags = new string[1]
		{
			"convenience_fee"
		};

		public void SetConvenienceFee(string value)
		{
			empty = false;
			convFeeParams.Add("convenience_fee", value);
		}

		public bool IsEmpty()
		{
			return empty;
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<convfee_info>");
			toXML_low(stringBuilder, convFeeTags, convFeeParams);
			stringBuilder.Append("</convfee_info>");
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
