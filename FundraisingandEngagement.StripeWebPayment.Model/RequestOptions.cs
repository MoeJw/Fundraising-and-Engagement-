namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class RequestOptions
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

		internal string StripeVersion
		{
			get;
			set;
		}
	}
}
