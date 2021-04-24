using System.Collections.Generic;
using System.Reflection;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Middleware
{
	internal class DictionaryPlugin : IParserPlugin
	{
		public bool Parse(ref string requestString, JsonPropertyAttribute attribute, PropertyInfo property, object propertyValue, object propertyParent)
		{
			if (!attribute.PropertyName!.Contains("metadata") && !attribute.PropertyName!.Contains("fraud_details"))
			{
				return false;
			}
			Dictionary<string, string> dictionary = (Dictionary<string, string>)propertyValue;
			if (dictionary == null)
			{
				return true;
			}
			foreach (string key in dictionary.Keys)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, attribute.PropertyName + "[" + key + "]", dictionary[key]);
			}
			return true;
		}
	}
}
