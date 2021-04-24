using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class CardVerification : Transaction
	{
		private AvsInfo avsInfo;

		private CvdInfo cvdInfo;

		private static string[] xmlTags = new string[5]
		{
			"order_id",
			"cust_id",
			"pan",
			"expdate",
			"crypt_type"
		};

		public CardVerification()
			: base(xmlTags)
		{
		}

		public CardVerification(Hashtable cardverification)
			: base(cardverification, xmlTags)
		{
		}

		public CardVerification(string order_id, string pan, string expdate, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public CardVerification(string order_id, string cust_id, string pan, string expdate, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetAvsStreetNumber(string avs_street_number)
		{
			transactionParams.Add("avs_street_number", avs_street_number);
		}

		public void SetAvsStreetName(string avs_street_name)
		{
			transactionParams.Add("avs_street_name", avs_street_name);
		}

		public void SetAvsZipCode(string avs_zipcode)
		{
			transactionParams.Add("avs_zipcode", avs_zipcode);
		}

		public void SetCvdValue(string cvd_value)
		{
			transactionParams.Add("cvd_value", cvd_value);
		}

		public void SetCvdIndicator(string cvd_indicator)
		{
			transactionParams.Add("cvd_indicator", cvd_indicator);
		}

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
		}

		public void SetCvdInfo(CvdInfo cvd)
		{
			cvdInfo = cvd;
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "card_verification" : "us_card_verification");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (avsInfo != null)
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			if (cvdInfo != null)
			{
				stringBuilder.Append(cvdInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
