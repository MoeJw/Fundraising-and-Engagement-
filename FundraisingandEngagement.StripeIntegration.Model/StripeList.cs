using System.Collections;
using System.Collections.Generic;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeIntegration.Model
{
	[JsonObject]
	public class StripeList<T> : StripeEntity, IEnumerable<T>, IEnumerable
	{
		[JsonProperty("object")]
		public string Object
		{
			get;
			set;
		}

		[JsonProperty("data")]
		public List<T> Data
		{
			get;
			set;
		}

		[JsonProperty("url")]
		public string Url
		{
			get;
			set;
		}

		public bool HasMore
		{
			get;
			set;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Data.GetEnumerator();
		}
	}
}
