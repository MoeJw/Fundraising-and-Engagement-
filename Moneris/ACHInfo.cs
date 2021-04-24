using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ACHInfo
	{
		private Hashtable achInfoParams = new Hashtable();

		private static string[] xmlTags = new string[17]
		{
			"sec",
			"cust_first_name",
			"cust_last_name",
			"cust_address1",
			"cust_address2",
			"cust_city",
			"cust_state",
			"cust_zip",
			"routing_num",
			"account_num",
			"check_num",
			"account_type",
			"micr",
			"magstripe",
			"dl_num",
			"image_front",
			"image_back"
		};

		public ACHInfo()
		{
		}

		public ACHInfo(string sec, string cust_first_name, string cust_last_name, string cust_address1, string cust_address2, string cust_city, string cust_state, string cust_zip, string routing_num, string account_num, string check_num, string account_type, string micr)
		{
			achInfoParams.Add("sec", sec);
			achInfoParams.Add("cust_first_name", cust_first_name);
			achInfoParams.Add("cust_last_name", cust_last_name);
			achInfoParams.Add("cust_address1", cust_address1);
			achInfoParams.Add("cust_address2", cust_address2);
			achInfoParams.Add("cust_city", cust_city);
			achInfoParams.Add("cust_state", cust_state);
			achInfoParams.Add("cust_zip", cust_zip);
			achInfoParams.Add("routing_num", routing_num);
			achInfoParams.Add("account_num", account_num);
			achInfoParams.Add("check_num", check_num);
			achInfoParams.Add("account_type", account_type);
			achInfoParams.Add("micr", micr);
		}

		public ACHInfo(string sec, string cust_first_name, string cust_last_name, string cust_address1, string cust_address2, string cust_city, string cust_state, string cust_zip, string routing_num, string account_num, string check_num, string account_type)
		{
			achInfoParams.Add("sec", sec);
			achInfoParams.Add("cust_first_name", cust_first_name);
			achInfoParams.Add("cust_last_name", cust_last_name);
			achInfoParams.Add("cust_address1", cust_address1);
			achInfoParams.Add("cust_address2", cust_address2);
			achInfoParams.Add("cust_city", cust_city);
			achInfoParams.Add("cust_state", cust_state);
			achInfoParams.Add("cust_zip", cust_zip);
			achInfoParams.Add("routing_num", routing_num);
			achInfoParams.Add("account_num", account_num);
			achInfoParams.Add("check_num", check_num);
			achInfoParams.Add("account_type", account_type);
		}

		public void SetSec(string value)
		{
			achInfoParams.Add("sec", value);
		}

		public void SetCustFirstName(string value)
		{
			achInfoParams.Add("cust_first_name", value);
		}

		public void SetCustLastName(string value)
		{
			achInfoParams.Add("cust_last_name", value);
		}

		public void SetCustAddress1(string value)
		{
			achInfoParams.Add("cust_address1", value);
		}

		public void SetCustAddress2(string value)
		{
			achInfoParams.Add("cust_address2", value);
		}

		public void SetCustCity(string value)
		{
			achInfoParams.Add("cust_city", value);
		}

		public void SetCustState(string value)
		{
			achInfoParams.Add("cust_state", value);
		}

		public void SetCustZip(string value)
		{
			achInfoParams.Add("cust_zip", value);
		}

		public void SetRoutingNum(string value)
		{
			achInfoParams.Add("routing_num", value);
		}

		public void SetAccountNum(string value)
		{
			achInfoParams.Add("account_num", value);
		}

		public void SetCheckNum(string value)
		{
			achInfoParams.Add("check_num", value);
		}

		public void SetAccountType(string value)
		{
			achInfoParams.Add("account_type", value);
		}

		public void SetMicr(string value)
		{
			achInfoParams.Add("micr", value);
		}

		public void SetMagstripe(string value)
		{
			achInfoParams.Add("magstripe", value);
		}

		public void SetImgFront(string value)
		{
			achInfoParams.Add("image_front", value);
		}

		public void SetImgBack(string value)
		{
			achInfoParams.Add("image_back", value);
		}

		public void SetDlNum(string value)
		{
			achInfoParams.Add("dl_num", value);
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<ach_info>");
			toXML_low(stringBuilder, xmlTags, achInfoParams);
			stringBuilder.Append("</ach_info>");
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
