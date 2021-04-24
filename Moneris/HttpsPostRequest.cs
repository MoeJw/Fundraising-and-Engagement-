using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class HttpsPostRequest
	{
		private Transaction transaction;

		private string storeId;

		private string apiToken;

		private Receipt receiptObj;

		private string url;

		private WebProxy proxy = null;

		private string xml_to_send;

		private string hostAddress = "";

		private string processingCountryCode = "CA";

		private bool testMode = false;

		private bool statusCheck = false;

		private bool isLegacyTransaction = true;

		private bool debug = false;

		public HttpsPostRequest()
		{
			isLegacyTransaction = false;
		}

		public HttpsPostRequest(string host, string store_id, string api_token, Transaction t)
		{
			storeId = store_id;
			apiToken = api_token;
			transaction = t;
			isLegacyTransaction = true;
			hostAddress = host;
			SendRequest();
		}

		public HttpsPostRequest(string host, string store_id, string api_token, string status_check, Transaction t)
		{
			storeId = store_id;
			apiToken = api_token;
			statusCheck = string.Equals(status_check.ToLower(), "true");
			transaction = t;
			isLegacyTransaction = true;
			hostAddress = host;
			SendRequest();
		}

		public HttpsPostRequest(string host, string store_id, string api_token, Transaction t, WebProxy prxy)
		{
			proxy = prxy;
			storeId = store_id;
			apiToken = api_token;
			transaction = t;
			isLegacyTransaction = true;
			hostAddress = host;
			SendRequest();
		}

		public void SetStatusCheck(bool status_check)
		{
			statusCheck = status_check;
		}

		public void SetHost(string host)
		{
			hostAddress = host;
		}

		public void SetStoreId(string store_id)
		{
			storeId = store_id;
		}

		public void SetApiToken(string api_token)
		{
			apiToken = api_token;
		}

		public void SetTransaction(Transaction t)
		{
			transaction = t;
		}

		public void SetProcCountryCode(string country)
		{
			processingCountryCode = country;
		}

		public void SetProxy(WebProxy host)
		{
			proxy = host;
		}

		public void SetTestMode(bool state)
		{
			testMode = state;
		}

		public void Send()
		{
			if (!isLegacyTransaction)
			{
				SendRequest();
			}
		}

		public void SendRequest()
		{
			getURL();
			string text = toXML();
			byte[] array = null;
			array = Encoding.ASCII.GetBytes(text);
			if (debug)
			{
				Console.WriteLine(text);
				Console.WriteLine(url);
			}
			try
			{
				Uri requestUri = new Uri(url);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(requestUri);
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "application/x-www-form-urlencoded";
				if (proxy != null)
				{
					httpWebRequest.Proxy = proxy;
					httpWebRequest.Credentials = proxy.Credentials;
				}
				httpWebRequest.ContentLength = array.Length;
				httpWebRequest.UserAgent = "DotNET NA - 1.0.7";
				Stream requestStream = httpWebRequest.GetRequestStream();
				requestStream.Write(array, 0, array.Length);
				requestStream.Flush();
				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
				string text2 = streamReader.ReadToEnd();
				if (debug)
				{
					Console.WriteLine(text2);
				}
				array = Encoding.ASCII.GetBytes(text2);
				MemoryStream memoryStream = new MemoryStream(array);
				Response response = new Response(memoryStream);
				receiptObj = response.GetReceipt();
				requestStream.Close();
				memoryStream.Close();
			}
			catch (Exception ex)
			{
				Response response = new Response(ex.Message);
				receiptObj = response.GetReceipt();
				Console.WriteLine("Message: " + ex.Message);
			}
		}

		public Receipt GetReceipt()
		{
			return receiptObj;
		}

		public string getXML()
		{
			return xml_to_send;
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = "<?xml version=\"1.0\"?>";
			string text2 = "request";
			bool flag = transaction.is3DsecureTransaction();
			bool flag2 = transaction.isMpiTxnTransaction();
			if (flag)
			{
				text2 = "MpiRequest";
			}
			if (statusCheck)
			{
				stringBuilder.Append(text + "<" + text2 + "><store_id>" + storeId + "</store_id><api_token>" + apiToken + "</api_token><status_check>" + statusCheck + "</status_check>" + transaction.toXML(processingCountryCode) + "</" + text2 + ">");
			}
			else
			{
				stringBuilder.Append(text + "<" + text2 + "><store_id>" + storeId + "</store_id><api_token>" + apiToken + "</api_token>" + transaction.toXML(processingCountryCode) + "</" + text2 + ">");
			}
			xml_to_send = stringBuilder.ToString();
			return xml_to_send;
		}

		private void getURL()
		{
			if (isLegacyTransaction)
			{
				getLegacyURL();
			}
			else
			{
				getUnifiedURL();
			}
		}

		private void getLegacyURL()
		{
			if (transaction.is3DsecureTransaction())
			{
				url = "https://" + hostAddress + ":443/mpi/servlet/MpiServlet";
			}
			else
			{
				url = "https://" + hostAddress + ":443/gateway2/servlet/MpgRequest";
			}
		}

		private void getUnifiedURL()
		{
			if (transaction.is3DsecureTransaction())
			{
				get3dSecureURL();
			}
			else
			{
				getGatewayURL();
			}
		}

		private void get3dSecureURL()
		{
			if (hostAddress != "")
			{
				url = hostAddress;
			}
			else if (processingCountryCode.Equals("CA") && testMode)
			{
				url = "https://esqa.moneris.com:443/mpi/servlet/MpiServlet";
			}
			else if (processingCountryCode.Equals("CA") && !testMode)
			{
				url = "https://www3.moneris.com:443/mpi/servlet/MpiServlet";
			}
			else if (processingCountryCode.Equals("US") && testMode)
			{
				url = "https://esplusqa.moneris.com:443/mpi/servlet/MpiServlet";
			}
			else if (processingCountryCode.Equals("US") && !testMode)
			{
				url = "https://esplus.moneris.com:443/mpi/servlet/MpiServlet";
			}
		}

		private void getGatewayURL()
		{
			if (hostAddress != "")
			{
				url = hostAddress;
			}
			else if (processingCountryCode.Equals("CA") && testMode)
			{
				url = "https://esqa.moneris.com:443/gateway2/servlet/MpgRequest";
			}
			else if (processingCountryCode.Equals("CA") && !testMode)
			{
				url = "https://www3.moneris.com:443/gateway2/servlet/MpgRequest";
			}
			else if (processingCountryCode.Equals("US") && testMode)
			{
				url = "https://esplusqa.moneris.com:443/gateway_us/servlet/MpgRequest";
			}
			else if (processingCountryCode.Equals("US") && !testMode)
			{
				url = "https://esplus.moneris.com:443/gateway_us/servlet/MpgRequest";
			}
		}
	}
}
