using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Transaction
	{
		protected Hashtable transactionParams = new Hashtable();

		protected string[] xmlFormatTags;

		private string processing_country_code = "";

		private bool threeDSecureTransaction = false;

		private bool mpiTxnTransaction = false;

		public Transaction()
		{
		}

		public Transaction(string[] xmlFormat)
		{
			xmlFormatTags = xmlFormat;
		}

		public Transaction(Hashtable transHash, string[] xmlFormat)
		{
			transactionParams = transHash;
			xmlFormatTags = xmlFormat;
		}

		public virtual string toXML(string value)
		{
			return "";
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			toXML_low(stringBuilder, xmlFormatTags, transactionParams);
			return stringBuilder.ToString();
		}

		private void toXML_low(StringBuilder sb, string[] xmlTags, Hashtable xmlData)
		{
			foreach (string text in xmlTags)
			{
				string text2 = (string)xmlData[text];
				if (text2 != null && text2 != "")
				{
					sb.Append("<" + text + ">" + text2 + "</" + text + ">");
				}
			}
		}

		public void SetProcCountryCode(string country_code)
		{
			processing_country_code = country_code;
		}

		public void Set3DsecureTransaction()
		{
			threeDSecureTransaction = true;
		}

		public void SetMpiTxnTransaction()
		{
			mpiTxnTransaction = true;
		}

		public bool is3DsecureTransaction()
		{
			return threeDSecureTransaction;
		}

		public bool isMpiTxnTransaction()
		{
			return mpiTxnTransaction;
		}
	}
}
