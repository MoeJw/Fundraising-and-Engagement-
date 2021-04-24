namespace FundraisingandEngagement.StripeWebPayment.Service
{
	public class StripeRequestOptions
	{
		public string ApiKey
		{
			get;
			set;
		}

		public string StripeConnectAccountId
		{
			get;
			set;
		}

		public string IdempotencyKey
		{
			get;
			set;
		}
	}
}
