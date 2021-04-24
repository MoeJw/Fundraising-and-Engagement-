using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	internal class Client
	{
		private HttpRequestMessage RequestMessage
		{
			get;
			set;
		}

		public Client(HttpRequestMessage requestMessage)
		{
			RequestMessage = requestMessage;
		}

		public void ApplyUserAgent()
		{
			RequestMessage.Headers.UserAgent.ParseAdd("Stripe/v1 .NetBindings/" + StripeConfiguration.StripeNetVersion);
		}

		public void ApplyClientData()
		{
			RequestMessage.Headers.Add("X-Stripe-Client-User-Agent", GetClientUserAgentString());
		}

		private string GetClientUserAgentString()
		{
			string value = "4.5";
			string text = testForMono();
			if (!string.IsNullOrEmpty(text))
			{
				value = text;
			}
			Dictionary<string, string> value2 = new Dictionary<string, string>
			{
				{
					"bindings_version",
					StripeConfiguration.StripeNetVersion
				},
				{
					"lang",
					".net"
				},
				{
					"publisher",
					"Jayme Davis"
				},
				{
					"lang_version",
					WebUtility.HtmlEncode(value)
				},
				{
					"uname",
					WebUtility.HtmlEncode(getSystemInformation())
				}
			};
			return JsonConvert.SerializeObject(value2);
		}

		private string testForMono()
		{
			return (Type.GetType("Mono.Runtime")?.GetTypeInfo().GetDeclaredMethod("GetDisplayName"))?.Invoke(null, null).ToString();
		}

		private string getSystemInformation()
		{
			string empty = string.Empty;
			empty += "portable.platform: ";
			try
			{
				empty += typeof(object).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
			}
			catch
			{
				empty += "unknown";
			}
			return empty;
		}
	}
}
