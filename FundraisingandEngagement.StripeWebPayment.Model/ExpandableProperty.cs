using System;
using Newtonsoft.Json.Linq;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	internal static class ExpandableProperty<T> where T : StripeEntityWithId
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
