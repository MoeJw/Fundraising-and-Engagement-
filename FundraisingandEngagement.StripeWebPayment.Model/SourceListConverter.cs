using System;
using System.Collections.Generic;
using FundraisingandEngagement.StripeIntegration.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	internal class SourceListConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject jObject = JObject.FromObject(value);
			jObject.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			StripeList<object> stripeList = JObject.Load(reader).ToObject<StripeList<object>>();
			StripeList<Source> stripeList2 = new StripeList<Source>
			{
				Data = new List<Source>(),
				HasMore = stripeList.HasMore,
				Object = stripeList.Object,
				Url = stripeList.Url
			};
			foreach (dynamic datum in stripeList.Data)
			{
				Source source = new Source();
				if (datum.SelectToken("object").ToString() == "bank_account")
				{
					source.Type = SourceType.BankAccount;
					source.BankAccount = Mapper<StripeBankAccount>.MapFromJson(datum.ToString());
				}
				if (datum.SelectToken("object").ToString() == "card")
				{
					source.Type = SourceType.Card;
					source.Card = Mapper<StripeCard>.MapFromJson(datum.ToString());
				}
				stripeList2.Data.Add(source);
			}
			return stripeList2;
		}
	}
}
