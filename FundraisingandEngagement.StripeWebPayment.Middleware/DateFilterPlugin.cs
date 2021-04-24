using System.Reflection;
using FundraisingandEngagement.StripeWebPayment.Model;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Middleware
{
	internal class DateFilterPlugin : IParserPlugin
	{
		public bool Parse(ref string requestString, JsonPropertyAttribute attribute, PropertyInfo property, object propertyValue, object propertyParent)
		{
			if (property.PropertyType != typeof(StripeDateFilter))
			{
				return false;
			}
			StripeDateFilter stripeDateFilter = (StripeDateFilter)propertyValue;
			if (stripeDateFilter.EqualTo.HasValue)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, attribute.PropertyName, stripeDateFilter.EqualTo.Value.ConvertDateTimeToEpoch().ToString());
			}
			if (stripeDateFilter.LessThan.HasValue)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, attribute.PropertyName + "[lt]", stripeDateFilter.LessThan.Value.ConvertDateTimeToEpoch().ToString());
			}
			if (stripeDateFilter.LessThanOrEqual.HasValue)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, attribute.PropertyName + "[lte]", stripeDateFilter.LessThanOrEqual.Value.ConvertDateTimeToEpoch().ToString());
			}
			if (stripeDateFilter.GreaterThan.HasValue)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, attribute.PropertyName + "[gt]", stripeDateFilter.GreaterThan.Value.ConvertDateTimeToEpoch().ToString());
			}
			if (stripeDateFilter.GreaterThanOrEqual.HasValue)
			{
				RequestStringBuilder.ApplyParameterToRequestString(ref requestString, attribute.PropertyName + "[gte]", stripeDateFilter.GreaterThanOrEqual.Value.ConvertDateTimeToEpoch().ToString());
			}
			return true;
		}
	}
}
