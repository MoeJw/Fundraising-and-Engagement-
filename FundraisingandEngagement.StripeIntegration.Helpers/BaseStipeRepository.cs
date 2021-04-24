using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FundraisingandEngagement.StripeIntegration.Model;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json.Linq;

namespace FundraisingandEngagement.StripeIntegration.Helpers
{
	public class BaseStipeRepository
	{
		public T Get<T>(string url, string apiKey)
		{
			using HttpClient httpClient = new HttpClient();
			return ResponseHelper<T>(httpClient.SendAsync(GetRequestMessage(url, HttpMethod.Get, apiKey)).Result);
		}

		public T Create<T>(T stripeObject, string url, string apiKey) where T : StripeEntityWithId
		{
			Type typeFromHandle = typeof(T);
			string contentstring = string.Empty;
			if (typeFromHandle.Equals(typeof(StripeCustomer)))
			{
				contentstring = GetStripeCustomerString(stripeObject as StripeCustomer);
			}
			if (typeFromHandle.Equals(typeof(StripeCharge)))
			{
				contentstring = GetStripeChargeString(stripeObject as StripeCharge);
			}
			if (typeFromHandle.Equals(typeof(StripeCard)))
			{
				contentstring = GetStripeChargeString(stripeObject as StripeCard);
			}
			using HttpClient httpClient = new HttpClient();
			Task<HttpResponseMessage> task = httpClient.SendAsync(GetRequestMessage(url, HttpMethod.Post, apiKey, contentstring));
			task.Wait();
			return ResponseHelper<T>(task.Result);
		}

		private string GetStripeCustomerString(StripeCustomer customer)
		{
			return $"description={((customer.Description != null) ? customer.Description.Replace(' ', '+') : string.Empty)}&email={customer.Email}";
		}

		private string GetStripeChargeString(StripeCharge charge)
		{
			return $"description={((charge.Description != null) ? charge.Description.Replace(' ', '+') : string.Empty)}&currency={charge.Currency}&amount={charge.Amount}&customer={charge.Customer.Id}&source={charge.Source.Id}";
		}

		private string GetStripeChargeString(StripeCard card)
		{
			return $"source={card.SourceToken}";
		}

		private T ResponseHelper<T>(HttpResponseMessage response)
		{
			string result = response.Content.ReadAsStringAsync().Result;
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception(JObject.Parse(result)["error"]!["message"]!.ToString());
			}
			return Mapper<T>.MapFromJson(BuildResponseData(response, result));
		}

		private HttpRequestMessage GetRequestMessage(string url, HttpMethod method, string apiKey, string contentstring = null)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, new Uri(url));
			httpRequestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");
			if (method == HttpMethod.Post)
			{
				httpRequestMessage.Content = new StringContent(contentstring, Encoding.UTF8, "application/x-www-form-urlencoded");
			}
			return httpRequestMessage;
		}

		private StripeResponse BuildResponseData(HttpResponseMessage response, string responseText)
		{
			return new StripeResponse
			{
				RequestId = (response.Headers.Contains("Request-Id") ? response.Headers.GetValues("Request-Id").First() : "n/a"),
				RequestDate = Convert.ToDateTime(response.Headers.GetValues("Date").First(), CultureInfo.InvariantCulture),
				ResponseJson = responseText
			};
		}
	}
}
