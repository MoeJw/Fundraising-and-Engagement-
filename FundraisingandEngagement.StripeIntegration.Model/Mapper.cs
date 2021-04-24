using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FundraisingandEngagement.StripeIntegration.Model
{
	public static class Mapper<T>
	{
		public static List<T> MapCollectionFromJson(string json, string token = "data", StripeResponse stripeResponse = null)
		{
			return (from tkn in JObject.Parse(json).SelectToken(token)
				select MapFromJson(tkn.ToString(), null, stripeResponse)).ToList();
		}

		public static List<T> MapCollectionFromJson(StripeResponse stripeResponse, string token = "data")
		{
			return MapCollectionFromJson(stripeResponse.ResponseJson, token, stripeResponse);
		}

		public static T MapFromJson(string json, string parentToken = null, StripeResponse stripeResponse = null)
		{
			T val = JsonConvert.DeserializeObject<T>(string.IsNullOrEmpty(parentToken) ? json : JObject.Parse(json).SelectToken(parentToken)!.ToString());
			applyStripeResponse(json, stripeResponse, val);
			return val;
		}

		public static T MapFromJson(StripeResponse stripeResponse, string parentToken = null)
		{
			return MapFromJson(stripeResponse.ResponseJson, parentToken, stripeResponse);
		}

		private static void applyStripeResponse(string json, StripeResponse stripeResponse, object obj)
		{
			if (stripeResponse == null)
			{
				return;
			}
			foreach (PropertyInfo runtimeProperty in obj.GetType().GetRuntimeProperties())
			{
				if (runtimeProperty.Name == "StripeResponse")
				{
					runtimeProperty.SetValue(obj, stripeResponse);
				}
			}
			stripeResponse.ObjectJson = json;
		}
	}
}
