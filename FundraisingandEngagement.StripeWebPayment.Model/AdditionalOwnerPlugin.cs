using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FundraisingandEngagement.StripeWebPayment.Middleware;
using Newtonsoft.Json;

namespace FundraisingandEngagement.StripeWebPayment.Model
{
	internal class AdditionalOwnerPlugin : IParserPlugin
	{
		public bool Parse(ref string requestString, JsonPropertyAttribute attribute, PropertyInfo property, object propertyValue, object propertyParent)
		{
			if (attribute.PropertyName != "legal_entity[additional_owners]")
			{
				return false;
			}
			List<StripeAccountAdditionalOwner> list = (List<StripeAccountAdditionalOwner>)property.GetValue(propertyParent, null);
			if (list == null)
			{
				return true;
			}
			int num = 0;
			foreach (StripeAccountAdditionalOwner item in list)
			{
				IEnumerable<PropertyInfo> runtimeProperties = item.GetType().GetRuntimeProperties();
				foreach (PropertyInfo item2 in runtimeProperties)
				{
					object value = item2.GetValue(item, null);
					if (value != null)
					{
						JsonPropertyAttribute jsonPropertyAttribute = item2.GetCustomAttributes<JsonPropertyAttribute>().SingleOrDefault();
						if (jsonPropertyAttribute != null)
						{
							RequestStringBuilder.ApplyParameterToRequestString(ref requestString, $"{attribute.PropertyName}[{num}]{jsonPropertyAttribute.PropertyName}", value.ToString());
						}
					}
				}
				num++;
			}
			return true;
		}
	}
}
