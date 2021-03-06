using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FundraisingandEngagement.StripeIntegration.Model;
using FundraisingandEngagement.StripeWebPayment.Model;

namespace FundraisingandEngagement.StripeWebPayment.Service
{
	public class StripeRefundService : StripeService
	{
		public bool ExpandBalanceTransaction
		{
			get;
			set;
		}

		public bool ExpandCharge
		{
			get;
			set;
		}

		public StripeRefundService(string apiKey = null)
			: base(apiKey)
		{
		}

		public virtual StripeRefund Create(string chargeId, StripeRefundCreateOptions createOptions = null, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeRefund>.MapFromJson(Requestor.PostString(this.ApplyAllParameters(createOptions, Urls.Charges + "/" + chargeId + "/refunds"), SetupRequestOptions(requestOptions)));
		}

		public virtual StripeRefund Get(string refundId, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeRefund>.MapFromJson(Requestor.GetString(this.ApplyAllParameters(null, Urls.BaseUrl + "/refunds/" + refundId), SetupRequestOptions(requestOptions)));
		}

		public virtual StripeRefund Update(string refundId, StripeRefundUpdateOptions updateOptions, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeRefund>.MapFromJson(Requestor.PostString(this.ApplyAllParameters(updateOptions, Urls.BaseUrl + "/refunds/" + refundId), SetupRequestOptions(requestOptions)));
		}

		public virtual IEnumerable<StripeRefund> List(StripeRefundListOptions listOptions = null, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeRefund>.MapCollectionFromJson(Requestor.GetString(this.ApplyAllParameters(listOptions, Urls.BaseUrl + "/refunds", isListMethod: true), SetupRequestOptions(requestOptions)));
		}

		public virtual async Task<StripeRefund> CreateAsync(string chargeId, StripeRefundCreateOptions createOptions = null, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeRefund>.MapFromJson(await Requestor.PostStringAsync(this.ApplyAllParameters(createOptions, Urls.Charges + "/" + chargeId + "/refunds"), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<StripeRefund> GetAsync(string refundId, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeRefund>.MapFromJson(await Requestor.GetStringAsync(this.ApplyAllParameters(null, Urls.BaseUrl + "/refunds/" + refundId), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<StripeRefund> UpdateAsync(string refundId, StripeRefundUpdateOptions updateOptions, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeRefund>.MapFromJson(await Requestor.PostStringAsync(this.ApplyAllParameters(updateOptions, Urls.BaseUrl + "/refunds/" + refundId), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<IEnumerable<StripeRefund>> ListAsync(StripeRefundListOptions listOptions = null, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeRefund>.MapCollectionFromJson(await Requestor.GetStringAsync(this.ApplyAllParameters(listOptions, Urls.BaseUrl + "/refunds", isListMethod: true), SetupRequestOptions(requestOptions), cancellationToken));
		}
	}
}
