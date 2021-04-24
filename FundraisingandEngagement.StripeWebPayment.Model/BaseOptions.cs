using System.Collections.Generic;
using FundraisingandEngagement.StripeWebPayment.Middleware;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	public class BaseOptions : INestedOptions
	{
		private Dictionary<string, string> extraParams = new Dictionary<string, string>();

		private List<string> expand = new List<string>();

		public Dictionary<string, string> ExtraParams
		{
			get
			{
				return extraParams;
			}
			set
			{
				extraParams = value;
			}
		}

		public List<string> Expand
		{
			get
			{
				return expand;
			}
			set
			{
				expand = value;
			}
		}

		public void AddExtraParam(string key, string value)
		{
			ExtraParams.Add(key, value);
		}

		public void AddExpand(string value)
		{
			Expand.Add(value);
		}
	}
}
