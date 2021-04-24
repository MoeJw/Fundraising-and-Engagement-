using FundraisingandEngagement.StripeIntegration.Helpers;
using FundraisingandEngagement.StripeWebPayment.Model;

namespace FundraisingandEngagement.StripeWebPayment.Service
{
	internal class CustomerService
	{
		private string customerStripeUrl = "https://api.stripe.com/v1/customers";

		private BaseStipeRepository stipeRepository;

		public CustomerService()
		{
			stipeRepository = new BaseStipeRepository();
		}

		public StripeCustomer GetStripeCustomer(string custName, string custEmail, string strToken)
		{
			return CreateCustomer(custName, custEmail, strToken);
		}

		private StripeCustomer CreateCustomer(string customerName, string customerEmail, string strToken)
		{
			return stipeRepository.Create(new StripeCustomer
			{
				Email = customerEmail,
				Description = customerName
			}, customerStripeUrl, strToken);
		}
	}
}
