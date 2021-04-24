using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResTempTokenize : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[4]
		{
			"order_id",
			"txn_number",
			"duration",
			"crypt_type"
		};

		public ResTempTokenize()
			: base(xmlTags)
		{
		}

		public ResTempTokenize(Hashtable resaddcc)
			: base(resaddcc, xmlTags)
		{
		}

		public ResTempTokenize(string order_id, string txn_number, string duration, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("duration", duration);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetDuration(string duration)
		{
			transactionParams.Add("duration", duration);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetAvsZipCode(string avs_zipcode)
		{
			transactionParams.Add("avs_zipcode", avs_zipcode);
		}

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetPhone(string phone)
		{
			transactionParams.Add("phone", phone);
		}

		public void SetEmail(string email)
		{
			transactionParams.Add("email", email);
		}

		public void SetNote(string note)
		{
			transactionParams.Add("note", note);
		}

		public void SetDataKeyFormat(string note)
		{
			transactionParams.Add("data_key_format", note);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "res_temp_tokenize" : "us_res_temp_tokenize");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (avsInfo != null)
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
