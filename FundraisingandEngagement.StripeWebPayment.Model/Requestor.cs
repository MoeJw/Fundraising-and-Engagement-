using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FundraisingandEngagement.StripeWebPayment.Service;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	internal static class Requestor
	{
		internal static HttpClient HttpClient
		{
			get;
			private set;
		}

		static Requestor()
		{
			HttpClient = ((StripeConfiguration.HttpMessageHandler != null) ? new HttpClient(StripeConfiguration.HttpMessageHandler) : new HttpClient());
		}

		public static StripeResponse GetString(string url, StripeRequestOptions requestOptions)
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Get, requestOptions);
			return ExecuteRequest(requestMessage);
		}

		public static StripeResponse PostString(string url, StripeRequestOptions requestOptions)
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Post, requestOptions);
			return ExecuteRequest(requestMessage);
		}

		public static StripeResponse Delete(string url, StripeRequestOptions requestOptions)
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Delete, requestOptions);
			return ExecuteRequest(requestMessage);
		}

		public static StripeResponse PostStringBearer(string url, StripeRequestOptions requestOptions)
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Post, requestOptions, useBearer: true);
			return ExecuteRequest(requestMessage);
		}

		public static StripeResponse PostFile(string url, string fileName, Stream fileStream, string purpose, StripeRequestOptions requestOptions)
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Post, requestOptions);
			ApplyMultiPartFileToRequest(requestMessage, fileName, fileStream, purpose);
			return ExecuteRequest(requestMessage);
		}

		private static StripeResponse ExecuteRequest(HttpRequestMessage requestMessage)
		{
			HttpResponseMessage result = HttpClient.SendAsync(requestMessage).Result;
			string result2 = result.Content.ReadAsStringAsync().Result;
			if (result.IsSuccessStatusCode)
			{
				return BuildResponseData(result, result2);
			}
			throw new Exception();
		}

		public static Task<StripeResponse> GetStringAsync(string url, StripeRequestOptions requestOptions, CancellationToken cancellationToken = default(CancellationToken))
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Get, requestOptions);
			return ExecuteRequestAsync(requestMessage, cancellationToken);
		}

		public static Task<StripeResponse> PostStringAsync(string url, StripeRequestOptions requestOptions, CancellationToken cancellationToken = default(CancellationToken))
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Post, requestOptions);
			return ExecuteRequestAsync(requestMessage, cancellationToken);
		}

		public static Task<StripeResponse> DeleteAsync(string url, StripeRequestOptions requestOptions, CancellationToken cancellationToken = default(CancellationToken))
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Delete, requestOptions);
			return ExecuteRequestAsync(requestMessage, cancellationToken);
		}

		public static Task<StripeResponse> PostStringBearerAsync(string url, StripeRequestOptions requestOptions, CancellationToken cancellationToken = default(CancellationToken))
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Post, requestOptions, useBearer: true);
			return ExecuteRequestAsync(requestMessage, cancellationToken);
		}

		public static Task<StripeResponse> PostFileAsync(string url, string fileName, Stream fileStream, string purpose, StripeRequestOptions requestOptions, CancellationToken cancellationToken = default(CancellationToken))
		{
			HttpRequestMessage requestMessage = GetRequestMessage(url, HttpMethod.Post, requestOptions);
			ApplyMultiPartFileToRequest(requestMessage, fileName, fileStream, purpose);
			return ExecuteRequestAsync(requestMessage, cancellationToken);
		}

		private static async Task<StripeResponse> ExecuteRequestAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default(CancellationToken))
		{
			HttpResponseMessage response = await HttpClient.SendAsync(requestMessage, cancellationToken);
			if (response.IsSuccessStatusCode)
			{
				HttpResponseMessage response2 = response;
				return BuildResponseData(response2, await response.Content.ReadAsStringAsync());
			}
			throw new Exception();
		}

		private static HttpRequestMessage GetRequestMessage(string url, HttpMethod method, StripeRequestOptions requestOptions, bool useBearer = false)
		{
			requestOptions.ApiKey = requestOptions.ApiKey ?? StripeConfiguration.GetApiKey();
			HttpRequestMessage httpRequestMessage = BuildRequest(method, url);
			httpRequestMessage.Headers.Add("Authorization", (!useBearer) ? GetAuthorizationHeaderValue(requestOptions.ApiKey) : GetAuthorizationHeaderValueBearer(requestOptions.ApiKey));
			if (requestOptions.StripeConnectAccountId != null)
			{
				httpRequestMessage.Headers.Add("Stripe-Account", requestOptions.StripeConnectAccountId);
			}
			if (requestOptions.IdempotencyKey != null)
			{
				httpRequestMessage.Headers.Add("Idempotency-Key", requestOptions.IdempotencyKey);
			}
			httpRequestMessage.Headers.Add("Stripe-Version", StripeConfiguration.StripeApiVersion);
			Client client = new Client(httpRequestMessage);
			client.ApplyUserAgent();
			client.ApplyClientData();
			return httpRequestMessage;
		}

		private static HttpRequestMessage BuildRequest(HttpMethod method, string url)
		{
			if (method != HttpMethod.Post)
			{
				return new HttpRequestMessage(method, new Uri(url));
			}
			string content = string.Empty;
			string uriString = url;
			if (!string.IsNullOrEmpty(new Uri(url).Query))
			{
				content = new Uri(url).Query.Substring(1);
				uriString = url.Substring(0, url.IndexOf("?", StringComparison.CurrentCultureIgnoreCase));
			}
			return new HttpRequestMessage(method, new Uri(uriString))
			{
				Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")
			};
		}

		private static string GetAuthorizationHeaderValue(string apiKey)
		{
			string str = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":"));
			return "Basic " + str;
		}

		private static string GetAuthorizationHeaderValueBearer(string apiKey)
		{
			return "Bearer " + apiKey;
		}

		private static void ApplyMultiPartFileToRequest(HttpRequestMessage requestMessage, string fileName, Stream fileStream, string purpose)
		{
			requestMessage.Headers.ExpectContinue = true;
			StreamContent streamContent = new StreamContent(fileStream);
			streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
			{
				Name = "\"file\"",
				FileName = "\"" + fileName + "\""
			};
			streamContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(fileName));
			MultipartFormDataContent multipartFormDataContent = (MultipartFormDataContent)(requestMessage.Content = new MultipartFormDataContent("----------Upload: " + DateTime.UtcNow.Ticks.ToString("x"))
			{
				{
					new StringContent(purpose),
					"\"purpose\""
				},
				streamContent
			});
		}

		private static StripeResponse BuildResponseData(HttpResponseMessage response, string responseText)
		{
			return new StripeResponse
			{
				RequestId = response.Headers.GetValues("Request-Id").First(),
				RequestDate = Convert.ToDateTime(response.Headers.GetValues("Date").First()),
				ResponseJson = responseText
			};
		}
	}
}
