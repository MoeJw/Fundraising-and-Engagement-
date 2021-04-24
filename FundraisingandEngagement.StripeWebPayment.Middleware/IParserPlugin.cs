using System.Reflection;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Middleware
{
	public interface IParserPlugin
	{
		bool Parse(ref string requestString, JsonPropertyAttribute attribute, PropertyInfo property, object propertyValue, object propertyParent);
	}
}
