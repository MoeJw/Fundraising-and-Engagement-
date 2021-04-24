using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResTokenizeCC : Transaction
	{
		private AvsInfo avsInfo;

		private static string[] xmlTags = new string[7]
		{
			"order_id",
			"txn_number",
			"cust_id",
			"phone",
			"email",
			"note",
			"data_key_format"
		};

		public ResTokenizeCC()
			: base(xmlTags)
		{
		}

		public ResTokenizeCC(Hashtable resaddcc)
			: base(resaddcc, xmlTags)
		{
		}

		public ResTokenizeCC(string order_id, string txn_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetAvsAddress(string avs_address)
		{
			transactionParams.Add("avs_address", avs_address);
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

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
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
			string str = ((!country.Equals("US")) ? "res_tokenize_cc" : "us_res_tokenize_cc");
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
