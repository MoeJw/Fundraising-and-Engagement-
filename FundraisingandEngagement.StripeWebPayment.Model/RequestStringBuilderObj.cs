using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using FundraisingandEngagement.StripeWebPayment.Middleware;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public static class RequestStringBuilderObj
	{
		internal sealed class Parameter
		{
			public string Key
			{
				get;
			}

			public string Value
			{
				get;
			}

			public Parameter(string key, string value)
			{
				Key = key;
				Value = value;
			}
		}

		public static void ApplyParameterToRequestString(ref string requestString, string argument, string value)
		{
			string text = (requestString.Contains("?") ? "&" : "?");
			requestString = requestString + text + argument + "=" + WebUtility.UrlEncode(value);
		}

		public static void CreateQuery(ref string requestString, INestedOptions options)
		{
			List<Parameter> list = FlattenParams(options);
			foreach (Parameter item in list)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, item.Key, item.Value);
			}
		}

		private static List<Parameter> FlattenParams(INestedOptions options)
		{
			return FlattenParamsOptions(options, null);
		}

		private static List<Parameter> FlattenParamsValue(object value, string keyPrefix)
		{
			List<Parameter> list = null;
			if (value is INestedOptions)
			{
				list = FlattenParamsOptions((INestedOptions)value, keyPrefix);
			}
			else if (IsDictionary(value))
			{
				Dictionary<string, object> dictionary = ((IDictionary)value).Cast<object>().ToDictionary((dynamic entry) => (string)entry.Key, (dynamic entry) => entry.Value);
				list = FlattenParamsDictionary(dictionary, keyPrefix);
			}
			else if (IsList(value))
			{
				List<object> list2 = ((IEnumerable)value).Cast<object>().ToList();
				list = FlattenParamsList(list2, keyPrefix);
			}
			else if (IsArray(value))
			{
				object[] array = ((IEnumerable)value).Cast<object>().ToArray();
				list = FlattenParamsArray(array, keyPrefix);
			}
			else if (IsEnum(value))
			{
				list = new List<Parameter>();
				string value2 = JsonConvert.SerializeObject(value).Trim('"');
				list.Add(new Parameter(keyPrefix, value2));
			}
			else if (value is DateTime)
			{
				list = new List<Parameter>();
				DateTime? dateTime = (DateTime)value;
				if (dateTime.HasValue)
				{
					list.Add(new Parameter(keyPrefix, dateTime?.ConvertDateTimeToEpoch().ToString(CultureInfo.InvariantCulture)));
				}
			}
			else if (value == null)
			{
				list = new List<Parameter>();
				list.Add(new Parameter(keyPrefix, string.Empty));
			}
			else
			{
				list = new List<Parameter>();
				list.Add(new Parameter(keyPrefix, string.Format(CultureInfo.InvariantCulture, "{0}", value)));
			}
			return list;
		}

		private static List<Parameter> FlattenParamsOptions(INestedOptions options, string keyPrefix)
		{
			List<Parameter> list = new List<Parameter>();
			if (options == null)
			{
				return list;
			}
			foreach (PropertyInfo runtimeProperty in options.GetType().GetRuntimeProperties())
			{
				object value = runtimeProperty.GetValue(options, null);
				if (value != null)
				{
					JsonPropertyAttribute customAttribute = runtimeProperty.GetCustomAttribute<JsonPropertyAttribute>();
					if (customAttribute != null)
					{
						string propertyName = customAttribute.PropertyName;
						string keyPrefix2 = NewPrefix(propertyName, keyPrefix);
						list.AddRange(FlattenParamsValue(value, keyPrefix2));
					}
				}
			}
			return list;
		}

		private static List<Parameter> FlattenParamsDictionary(Dictionary<string, object> dictionary, string keyPrefix)
		{
			List<Parameter> list = new List<Parameter>();
			if (dictionary == null)
			{
				return list;
			}
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				string key = WebUtility.UrlEncode(item.Key);
				object value = item.Value;
				string keyPrefix2 = NewPrefix(key, keyPrefix);
				list.AddRange(FlattenParamsValue(value, keyPrefix2));
			}
			return list;
		}

		private static List<Parameter> FlattenParamsList(List<object> list, string keyPrefix)
		{
			List<Parameter> list2 = new List<Parameter>();
			if (list == null)
			{
				return list2;
			}
			if (!list.Any())
			{
				list2.Add(new Parameter(keyPrefix, string.Empty));
			}
			else
			{
				foreach (var item in list.Select((object value, int index) => new
				{
					value,
					index
				}))
				{
					string keyPrefix2 = $"{keyPrefix}[{item.index}]";
					list2.AddRange(FlattenParamsValue(item.value, keyPrefix2));
				}
			}
			return list2;
		}

		private static List<Parameter> FlattenParamsArray(object[] array, string keyPrefix)
		{
			List<Parameter> list = new List<Parameter>();
			if (array.Length == 0)
			{
				list.Add(new Parameter(keyPrefix, string.Empty));
			}
			else
			{
				for (int i = 0; i < array.Length; i++)
				{
					object value = array[i];
					string keyPrefix2 = $"{keyPrefix}[{i}]";
					list.AddRange(FlattenParamsValue(value, keyPrefix2));
				}
			}
			return list;
		}

		private static bool IsDictionary(object o)
		{
			if (o == null)
			{
				return false;
			}
			Type type = o.GetType();
			if (!type.GetTypeInfo().IsGenericType)
			{
				return false;
			}
			if (type.GetTypeInfo().GetGenericTypeDefinition() != typeof(Dictionary<, >))
			{
				return false;
			}
			return true;
		}

		private static bool IsList(object o)
		{
			if (o == null)
			{
				return false;
			}
			Type type = o.GetType();
			if (!type.GetTypeInfo().IsGenericType)
			{
				return false;
			}
			if (type.GetTypeInfo().GetGenericTypeDefinition() != typeof(List<>))
			{
				return false;
			}
			return true;
		}

		private static bool IsArray(object o)
		{
			if (o == null)
			{
				return false;
			}
			Type type = o.GetType();
			return type.IsArray;
		}

		private static bool IsEnum(object o)
		{
			if (o == null)
			{
				return false;
			}
			Type type = o.GetType();
			if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				type = Nullable.GetUnderlyingType(type);
			}
			if (!type.GetTypeInfo().IsEnum)
			{
				return false;
			}
			return true;
		}

		private static string NewPrefix(string key, string keyPrefix)
		{
			if (string.IsNullOrEmpty(keyPrefix))
			{
				return key;
			}
			int num = key.IndexOf("[", StringComparison.Ordinal);
			if (num == -1)
			{
				return keyPrefix + "[" + key + "]";
			}
			return keyPrefix + "[" + key.Substring(0, num) + "]" + key.Substring(num);
		}
	}
}
