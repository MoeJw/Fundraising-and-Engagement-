using System;
using FundraisingandEngagement.Stripe.Infrastructure;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class StripeFileUpload : StripeEntityWithId
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("created")]
		[JsonConverter(typeof(StripeDateTimeConverter))]
		public DateTime Created
		{
			get;
			set;
		}

		[JsonProperty("size")]
		public int Size
		{
			get;
			set;
		}

		[JsonProperty("purpose")]
		public string Purpose
		{
			get;
			set;
		}

		[JsonProperty("Url")]
		public string Url
		{
			get;
			set;
		}

		[JsonProperty("type")]
		public string Type
		{
			get;
			set;
		}
	}
}
