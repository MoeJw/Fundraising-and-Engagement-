using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FundraisingandEngagement.StripeIntegration.Model;
using FundraisingandEngagement.StripeWebPayment.Model;

namespace FundraisingandEngagement.StripeWebPayment.Service
{
	public class StripeCustomerService : StripeService
	{
		public bool ExpandDefaultSource
		{
			get;
			set;
		}

		public bool ExpandDefaultCustomerBankAccount
		{
			get;
			set;
		}

		public StripeCustomerService(string apiKey = null)
			: base(apiKey)
		{
		}

		public virtual StripeCustomer Create(StripeCustomerCreateOptions createOptions, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeCustomer>.MapFromJson(Requestor.PostString(this.ApplyAllParameters(createOptions, Urls.Customers), SetupRequestOptions(requestOptions)));
		}

		public virtual StripeCustomer Get(string customerId, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeCustomer>.MapFromJson(Requestor.GetString(this.ApplyAllParameters(null, Urls.Customers + "/" + customerId), SetupRequestOptions(requestOptions)));
		}

		public virtual StripeCustomer Update(string customerId, StripeCustomerUpdateOptions updateOptions, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeCustomer>.MapFromJson(Requestor.PostString(this.ApplyAllParameters(updateOptions, Urls.Customers + "/" + customerId), SetupRequestOptions(requestOptions)));
		}

		public virtual StripeDeleted Delete(string customerId, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeDeleted>.MapFromJson(Requestor.Delete(Urls.Customers + "/" + customerId, SetupRequestOptions(requestOptions)));
		}

		public virtual IEnumerable<StripeCustomer> List(StripeCustomerListOptions listOptions = null, StripeRequestOptions requestOptions = null)
		{
			return Mapper<StripeCustomer>.MapCollectionFromJson(Requestor.GetString(this.ApplyAllParameters(listOptions, Urls.Customers, isListMethod: true), SetupRequestOptions(requestOptions)));
		}

		public virtual async Task<StripeCustomer> CreateAsync(StripeCustomerCreateOptions createOptions, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeCustomer>.MapFromJson(await Requestor.PostStringAsync(this.ApplyAllParameters(createOptions, Urls.Customers), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<StripeCustomer> GetAsync(string customerId, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeCustomer>.MapFromJson(await Requestor.GetStringAsync(this.ApplyAllParameters(null, Urls.Customers + "/" + customerId), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<StripeCustomer> UpdateAsync(string customerId, StripeCustomerUpdateOptions updateOptions, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeCustomer>.MapFromJson(await Requestor.PostStringAsync(this.ApplyAllParameters(updateOptions, Urls.Customers + "/" + customerId), SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<StripeDeleted> DeleteAsync(string customerId, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeDeleted>.MapFromJson(await Requestor.DeleteAsync(Urls.Customers + "/" + customerId, SetupRequestOptions(requestOptions), cancellationToken));
		}

		public virtual async Task<IEnumerable<StripeCustomer>> ListAsync(StripeCustomerListOptions listOptions = null, StripeRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Mapper<StripeCustomer>.MapCollectionFromJson(await Requestor.GetStringAsync(this.ApplyAllParameters(listOptions, Urls.Customers, isListMethod: true), SetupRequestOptions(requestOptions), cancellationToken));
		}
	}
}
