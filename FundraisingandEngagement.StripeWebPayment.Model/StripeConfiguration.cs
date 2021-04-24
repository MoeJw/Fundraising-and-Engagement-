using System.Net.Http;
using System.Reflection;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public static class StripeConfiguration
	{
		private static string apiBase;

		public static string StripeApiVersion;

		private static string _apiKey;

		public static string StripeNetVersion
		{
			get;
			private set;
		}

		public static HttpMessageHandler HttpMessageHandler
		{
			get;
			set;
		}

		static StripeConfiguration()
		{
			StripeApiVersion = "2016-07-06";
			StripeNetVersion = new AssemblyName(typeof(Requestor).GetTypeInfo().Assembly.FullName).Version.ToString(3);
		}

		internal static string GetApiKey()
		{
			if (string.IsNullOrEmpty(_apiKey))
			{
			}
			return _apiKey;
		}

		public static void SetApiKey(string newApiKey)
		{
			_apiKey = newApiKey;
		}

		internal static string GetApiBase()
		{
			if (string.IsNullOrEmpty(apiBase))
			{
				apiBase = Urls.BaseUrl;
			}
			return apiBase;
		}
	}
}
