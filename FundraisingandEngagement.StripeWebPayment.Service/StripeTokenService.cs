using System.Threading;
using System.Threading.Tasks;
using FundraisingandEngagement.StripeIntegration.Model;
using FundraisingandEngagement.StripeWebPayment.Model;

namespace FundraisingandEngagement.StripeWebPayment.Service
{
	public class StripeTokenService : StripeService
	{
		public StripeTokenService(string apiKey = null)
			: base(apiKey)
		{
		}

		public virtual StripeToken Create(StripeTokenCreateOptions createOptions, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeToken>.MapFromJson(Requestor.PostString(this.ApplyAllParameters(createOptions, Urls.Tokens), SetupRequestOptions(requestOptions)));
		}

		public virtual StripeToken Get(string tokenId, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeToken>.MapFromJson(Requestor.GetString(Urls.Tokens + "/" + tokenId, SetupRequestOptions(requestOptions)));
		}

		public virtual async Task<StripeToken> CreateAsync(StripeTokenCreateOptions createOptions, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeToken>.MapFromJson(await Requestor.PostStringAsync(this.ApplyAllParameters(createOptions, Urls.Tokens), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<StripeToken> GetAsync(string tokenId, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeToken>.MapFromJson(await Requestor.GetStringAsync(Urls.Tokens + "/" + tokenId, SetupRequestOptions(requestOptions), cancellationToken));
		}
	}
}
