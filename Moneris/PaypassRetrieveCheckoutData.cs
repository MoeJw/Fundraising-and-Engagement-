using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class PaypassRetrieveCheckoutData : Transaction
	{
		private Hashtable keyHashes = new Hashtable();

		private static string[] xmlTags = new string[3]
		{
			"oauth_token",
			"oauth_verifier",
			"checkout_resource_url"
		};

		public PaypassRetrieveCheckoutData()
			: base(xmlTags)
		{
		}

		public PaypassRetrieveCheckoutData(Hashtable paypass_retrieve_checkout_data)
			: base(paypass_retrieve_checkout_data, xmlTags)
		{
		}

		public void SetOauthToken(string oauth_token)
		{
			transactionParams.Add("oauth_token", oauth_token);
		}

		public void SetOauthVerifier(string oauth_verifier)
		{
			transactionParams.Add("oauth_verifier", oauth_verifier);
		}

		public void SetCheckoutResourceUrl(string checkout_resource_url)
		{
			transactionParams.Add("checkout_resource_url", checkout_resource_url);
		}

		public PaypassRetrieveCheckoutData(string oauth_token, string oauth_verifier, string checkout_resource_url)
			: base(xmlTags)
		{
			transactionParams.Add("oauth_token", oauth_token);
			transactionParams.Add("oauth_verifier", oauth_verifier);
			transactionParams.Add("checkout_resource_url", checkout_resource_url);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "paypass_retrieve_checkout_data" : "us_paypass_retrieve_checkout_data");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
