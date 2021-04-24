using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class AvsInfo
	{
		private bool empty = true;

		private Hashtable avsParams = new Hashtable();

		private static string[] avsTags = new string[10]
		{
			"avs_street_number",
			"avs_street_name",
			"avs_zipcode",
			"avs_email",
			"avs_hostname",
			"avs_browser",
			"avs_shiptocountry",
			"avs_shipmethod",
			"avs_custip",
			"avs_custphone"
		};

		public void SetAvsStreetNumber(string value)
		{
			empty = false;
			avsParams.Add("avs_street_number", value);
		}

		public void SetAvsStreetName(string value)
		{
			empty = false;
			avsParams.Add("avs_street_name", value);
		}

		public void SetAvsZipCode(string value)
		{
			empty = false;
			avsParams.Add("avs_zipcode", value);
		}

		public void SetAvsEmail(string value)
		{
			empty = false;
			avsParams.Add("avs_email", value);
		}

		public void SetAvsHostname(string value)
		{
			empty = false;
			avsParams.Add("avs_hostname", value);
		}

		public void SetAvsBrowser(string value)
		{
			empty = false;
			avsParams.Add("avs_browser", value);
		}

		public void SetAvsShipToCountry(string value)
		{
			empty = false;
			avsParams.Add("avs_shiptocountry", value);
		}

		public void SetAvsShipMethod(string value)
		{
			empty = false;
			avsParams.Add("avs_shipmethod", value);
		}

		public void SetAvsMerchProdSku(string value)
		{
			empty = false;
			avsParams.Add("avs_merchprodsku", value);
		}

		public void SetAvsCustIp(string value)
		{
			empty = false;
			avsParams.Add("avs_custip", value);
		}

		public void SetAvsCustPhone(string value)
		{
			empty = false;
			avsParams.Add("avs_custphone", value);
		}

		public bool IsEmpty()
		{
			return empty;
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<avs_info>");
			toXML_low(stringBuilder, avsTags, avsParams);
			stringBuilder.Append("</avs_info>");
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
