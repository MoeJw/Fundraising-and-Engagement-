using System;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundraisingandEngagement.Stripe.Infrastructure
{
	internal class StripeDateTimeConverter : DateTimeConverterBase
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteRawValue("\"\\/Date(" + ((DateTime)value).ConvertDateTimeToEpoch() + ")\\/\"");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value == null)
			{
				return null;
			}
			if (reader.TokenType == JsonToken.Integer)
			{
				return EpochTime.ConvertEpochToDateTime((long)reader.Value);
			}
			return DateTime.Parse(reader.Value!.ToString());
		}
	}
}
