using FundraisingandEngagement.StripeIntegration.Helpers;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	[JsonConverter(typeof(SourceConverter))]
	public class Source : StripeEntityWithId
	{
		public SourceType Type
		{
			get;
			set;
		}

		public StripeDeleted Deleted
		{
			get;
			set;
		}

		public StripeCard Card
		{
			get;
			set;
		}

		public StripeBankAccount BankAccount
		{
			get;
			set;
		}
	}
}
