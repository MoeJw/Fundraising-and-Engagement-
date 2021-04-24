using System;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json.Linq;

namespace FundraisingandEngagement.StripeIntegration.Helpers
{
	internal static class StringOrObject<T> where T : StripeEntityWithId
	{
		public static void Map(object value, Action<string> updateId, Action<T> updateObject)
		{
			if (value is JObject)
			{
				T val = ((JToken)value).ToObject<T>();
				updateId(val.Id);
				updateObject(val);
			}
			else if (value is string)
			{
				updateId((string)value);
				updateObject(null);
			}
		}
	}
}
