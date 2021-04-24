using System.Collections.Generic;
using System.Net;
using System.Reflection;
using FundraisingandEngagement.StripeWebPayment.Middleware;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public static class RequestStringBuilder
	{
		private static IEnumerable<IParserPlugin> ParserPlugins
		{
			get;
		}

		static RequestStringBuilder()
		{
			if (ParserPlugins == null)
			{
				ParserPlugins = new List<IParserPlugin>
				{
					new AdditionalOwnerPlugin(),
					new DictionaryPlugin(),
					new DateFilterPlugin()
				};
			}
		}

		internal static void ProcessPlugins(ref string requestString, JsonPropertyAttribute attribute, PropertyInfo property, object propertyValue, object propertyParent)
		{
			bool flag = false;
			foreach (IParserPlugin parserPlugin in ParserPlugins)
			{
				if (!flag)
				{
					flag = parserPlugin.Parse(ref requestString, attribute, property, propertyValue, propertyParent);
				}
			}
			if (!flag)
			{
				ApplyParameterToRequestString(ref requestString, attribute.PropertyName, propertyValue.ToString());
			}
		}

		public static void ApplyParameterToRequestString(ref string requestString, string argument, string value)
		{
			string text = "&";
			if (!requestString.Contains("?"))
			{
				text = "?";
			}
			requestString = requestString + text + argument + "=" + WebUtility.UrlEncode(value);
		}
	}
}
