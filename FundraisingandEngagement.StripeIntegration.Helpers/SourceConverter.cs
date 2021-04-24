using System;
using FundraisingandEngagement.StripeIntegration.Model;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FundraisingandEngagement.StripeIntegration.Helpers
{
	internal class SourceConverter : JsonConverter
	{
		public override bool CanWrite => false;

		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject.FromObject(value).WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			Source source = new Source();
			source.Id = jObject.SelectToken("id")!.ToString();
			Source source2 = source;
			if (jObject.SelectToken("object")?.ToString() == "card")
			{
				source2.Type = SourceType.Card;
				source2.Card = Mapper<StripeCard>.MapFromJson(jObject.ToString());
			}
			if (jObject.SelectToken("deleted")?.ToString() == "true")
			{
				source2.Type = SourceType.Deleted;
				source2.Deleted = Mapper<StripeDeleted>.MapFromJson(jObject.ToString());
			}
			return source2;
		}
	}
}
