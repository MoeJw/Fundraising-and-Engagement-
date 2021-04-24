using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using FundraisingandEngagement.StripeWebPayment.Middleware;
using FundraisingandEngagement.StripeWebPayment.Service;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	internal static class ParameterBuilder
	{
		public static string ApplyAllParameters(this StripeService service, object obj, string url, bool isListMethod = false)
		{
			string requestString = url;
			if (obj != null)
			{
				foreach (PropertyInfo runtimeProperty in obj.GetType().GetRuntimeProperties())
				{
					object value = runtimeProperty.GetValue(obj, null);
					if (value == null)
					{
						continue;
					}
					foreach (JsonPropertyAttribute customAttribute in runtimeProperty.GetCustomAttributes<JsonPropertyAttribute>())
					{
						if (value is INestedOptions)
						{
							ApplyNestedObjectProperties(ref requestString, value);
						}
						else
						{
							RequestStringBuilder.ProcessPlugins(ref requestString, customAttribute, runtimeProperty, value, obj);
						}
					}
				}
			}
			if (service != null)
			{
				IEnumerable<string> enumerable = from p in service.GetType().GetRuntimeProperties()
					where p.Name.StartsWith("Expand") && p.PropertyType == typeof(bool)
					where (bool)p.GetValue(service, null)
					select p.Name;
				foreach (string item in enumerable)
				{
					string input = item.Substring("Expand".Length);
					input = Regex.Replace(input, "([a-z])([A-Z])", "$1_$2").ToLower();
					if (isListMethod)
					{
						input = "data." + input;
					}
					requestString = ApplyParameterToUrl(requestString, "expand[]", input);
				}
			}
			return requestString;
		}

		public static string ApplyParameterToUrl(string url, string argument, string value)
		{
			RequestStringBuilder.ApplyParameterToRequestString(ref url, argument, value);
			return url;
		}

		private static void ApplyNestedObjectProperties(ref string requestString, object nestedObject)
		{
			foreach (PropertyInfo runtimeProperty in nestedObject.GetType().GetRuntimeProperties())
			{
				object value = runtimeProperty.GetValue(nestedObject, null);
				if (value == null)
				{
					continue;
				}
				foreach (JsonPropertyAttribute customAttribute in runtimeProperty.GetCustomAttributes<JsonPropertyAttribute>())
				{
					RequestStringBuilder.ProcessPlugins(ref requestString, customAttribute, runtimeProperty, value, nestedObject);
				}
			}
		}

		public static string ApplyAllParameters<T>(this Service<T> service, BaseOptions obj, string url, bool isListMethod = false) where T : IStripeEntity
		{
			string requestString = url;
			if (obj != null)
			{
				RequestStringBuilderObj.CreateQuery(ref requestString, obj);
				foreach (KeyValuePair<string, string> extraParam in obj.ExtraParams)
				{
					string argument = WebUtility.UrlEncode(extraParam.Key);
					RequestStringBuilder.ApplyParameterToRequestString(ref requestString, argument, extraParam.Value);
				}
				foreach (string item in obj.Expand)
				{
					RequestStringBuilder.ApplyParameterToRequestString(ref requestString, "expand[]", item);
				}
			}
			if (service != null)
			{
				IEnumerable<string> enumerable = from p in service.GetType().GetRuntimeProperties()
					where p.Name.StartsWith("Expand") && p.PropertyType == typeof(bool)
					where (bool)p.GetValue(service, null)
					select p.Name;
				foreach (string item2 in enumerable)
				{
					string input = item2.Substring("Expand".Length);
					input = Regex.Replace(input, "([a-z])([A-Z])", "$1_$2").ToLower();
					if (isListMethod)
					{
						input = "data." + input;
					}
					requestString = ApplyParameterToUrl(requestString, "expand[]", input);
				}
			}
			return requestString;
		}
	}
}
