using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FundraisingandEngagement.StripeIntegration.Model;
using FundraisingandEngagement.StripeWebPayment.Service;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class Service<EntityReturned> where EntityReturned : IStripeEntity
	{
		public string ApiKey
		{
			get;
			set;
		}

		public string BasePath
		{
			get;
		}

		protected Service()
		{
		}

		protected Service(string apiKey)
		{
			ApiKey = apiKey;
		}

		protected EntityReturned CreateEntity(BaseOptions options, StripeRequestOptions requestOptions)
		{
			return PostRequest<EntityReturned>(ClassUrl(), options, requestOptions);
		}

		protected Task<EntityReturned> CreateEntityAsync(BaseOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return PostRequestAsync<EntityReturned>(ClassUrl(), options, requestOptions, cancellationToken);
		}

		protected EntityReturned DeleteEntity(string id, BaseOptions options, StripeRequestOptions requestOptions)
		{
			return DeleteRequest<EntityReturned>(InstanceUrl(id), options, requestOptions);
		}

		protected Task<EntityReturned> DeleteEntityAsync(string id, BaseOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return DeleteRequestAsync<EntityReturned>(InstanceUrl(id), options, requestOptions, cancellationToken);
		}

		protected EntityReturned GetEntity(string id, BaseOptions options, StripeRequestOptions requestOptions)
		{
			return GetRequest<EntityReturned>(InstanceUrl(id), options, requestOptions, isListMethod: false);
		}

		protected Task<EntityReturned> GetEntityAsync(string id, BaseOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return GetRequestAsync<EntityReturned>(InstanceUrl(id), options, requestOptions, isListMethod: false, cancellationToken);
		}

		protected StripeList<EntityReturned> ListEntities(ListOptions options, StripeRequestOptions requestOptions)
		{
			return GetRequest<StripeList<EntityReturned>>(ClassUrl(), options, requestOptions, isListMethod: true);
		}

		protected Task<StripeList<EntityReturned>> ListEntitiesAsync(ListOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return GetRequestAsync<StripeList<EntityReturned>>(ClassUrl(), options, requestOptions, isListMethod: true, cancellationToken);
		}

		protected IEnumerable<EntityReturned> ListEntitiesAutoPaging(ListOptions options, StripeRequestOptions requestOptions)
		{
			return ListRequestAutoPaging<EntityReturned>(ClassUrl(), options, requestOptions);
		}

		protected EntityReturned UpdateEntity(string id, BaseOptions options, StripeRequestOptions requestOptions)
		{
			return PostRequest<EntityReturned>(InstanceUrl(id), options, requestOptions);
		}

		protected Task<EntityReturned> UpdateEntityAsync(string id, BaseOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return PostRequestAsync<EntityReturned>(InstanceUrl(id), options, requestOptions, cancellationToken);
		}

		protected T DeleteRequest<T>(string url, BaseOptions options, StripeRequestOptions requestOptions)
		{
			return Mapper<T>.MapFromJson(Requestor.Delete(this.ApplyAllParameters(options, url), SetupRequestOptions(requestOptions)));
		}

		protected async Task<T> DeleteRequestAsync<T>(string url, BaseOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return Mapper<T>.MapFromJson(await Requestor.DeleteAsync(this.ApplyAllParameters(options, url), SetupRequestOptions(requestOptions), cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}

		protected T GetRequest<T>(string url, BaseOptions options, StripeRequestOptions requestOptions, bool isListMethod)
		{
			return Mapper<T>.MapFromJson(Requestor.GetString(this.ApplyAllParameters(options, url, isListMethod), SetupRequestOptions(requestOptions)));
		}

		protected async Task<T> GetRequestAsync<T>(string url, BaseOptions options, StripeRequestOptions requestOptions, bool isListMethod, CancellationToken cancellationToken)
		{
			return Mapper<T>.MapFromJson(await Requestor.GetStringAsync(this.ApplyAllParameters(options, url, isListMethod), SetupRequestOptions(requestOptions), cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}

		protected IEnumerable<T> ListRequestAutoPaging<T>(string url, ListOptions options, StripeRequestOptions requestOptions)
		{
			StripeList<T> page = GetRequest<StripeList<T>>(url, options, requestOptions, isListMethod: true);
			while (true)
			{
				string itemId = null;
				foreach (T item in page)
				{
					itemId = ((IHasId)(object)item).Id;
					yield return item;
				}
				if (!page.HasMore || string.IsNullOrEmpty(itemId))
				{
					break;
				}
				options.StartingAfter = itemId;
				page = GetRequest<StripeList<T>>(ClassUrl(), options, requestOptions, isListMethod: true);
			}
		}

		protected T PostRequest<T>(string url, BaseOptions options, StripeRequestOptions requestOptions)
		{
			return Mapper<T>.MapFromJson(Requestor.PostString(this.ApplyAllParameters(options, url), SetupRequestOptions(requestOptions)));
		}

		protected async Task<T> PostRequestAsync<T>(string url, BaseOptions options, StripeRequestOptions requestOptions, CancellationToken cancellationToken)
		{
			return Mapper<T>.MapFromJson(await Requestor.PostStringAsync(this.ApplyAllParameters(options, url), SetupRequestOptions(requestOptions), cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}

		protected StripeRequestOptions SetupRequestOptions(StripeRequestOptions requestOptions)
		{
			if (requestOptions == null)
			{
				requestOptions = new StripeRequestOptions();
			}
			if (!string.IsNullOrEmpty(ApiKey))
			{
				requestOptions.ApiKey = ApiKey;
			}
			return requestOptions;
		}

		protected virtual string ClassUrl(string baseUrl = null)
		{
			baseUrl = baseUrl ?? StripeConfiguration.GetApiBase();
			return baseUrl + BasePath;
		}

		protected virtual string InstanceUrl(string id, string baseUrl = null)
		{
			return ClassUrl(baseUrl) + "/" + WebUtility.UrlEncode(id);
		}
	}
}
