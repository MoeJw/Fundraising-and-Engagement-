using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class AttributeQuery : Transaction
	{
		private bool empty = true;

		private Hashtable attributes = new Hashtable();

		private static string[] xmlTags = new string[3]
		{
			"order_id",
			"service_type",
			"policy_id"
		};

		private static string[] attributeXmlTags = new string[22]
		{
			"device_id",
			"account_login",
			"password_hash",
			"account_number",
			"account_name",
			"account_email",
			"account_telephone",
			"cc_number_hash",
			"ip_address",
			"ip_forwarded",
			"account_address_street1",
			"account_address_street2",
			"account_address_city",
			"account_address_state",
			"account_address_country",
			"account_address_zip",
			"shipping_address_street1",
			"shipping_address_street2",
			"shipping_address_city",
			"shipping_address_state",
			"shipping_address_country",
			"shipping_address_zip"
		};

		public AttributeQuery()
			: base(xmlTags)
		{
			attributes.Add(attributeXmlTags, null);
		}

		public AttributeQuery(Hashtable attribute_query)
			: base(xmlTags)
		{
		}

		public AttributeQuery(string order_id, string service_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("service_type", service_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetServiceType(string service_type)
		{
			transactionParams.Add("service_type", service_type);
		}

		public void SetPolicyId(string policy_id)
		{
			transactionParams.Add("policy_id", policy_id);
		}

		public void SetDeviceId(string device_id)
		{
			attributes.Add("device_id", device_id);
			empty = false;
		}

		public void SetAccountLogin(string account_login)
		{
			attributes.Add("account_login", account_login);
			empty = false;
		}

		public void SetPasswordHash(string password_hash)
		{
			attributes.Add("password_hash", password_hash);
			empty = false;
		}

		public void SetAccountNumber(string account_number)
		{
			attributes.Add("account_number", account_number);
			empty = false;
		}

		public void SetAccountName(string account_name)
		{
			attributes.Add("account_name", account_name);
			empty = false;
		}

		public void SetAccountEmail(string account_email)
		{
			attributes.Add("account_email", account_email);
			empty = false;
		}

		public void SetAccountTelephone(string account_telephone)
		{
			attributes.Add("account_telephone", account_telephone);
			empty = false;
		}

		public void SetCCNumberHash(string cc_number_hash)
		{
			attributes.Add("cc_number_hash", cc_number_hash);
			empty = false;
		}

		public void SetIPAddress(string ip_address)
		{
			attributes.Add("ip_address", ip_address);
			empty = false;
		}

		public void SetIPForwarded(string ip_forwarded)
		{
			attributes.Add("ip_forwarded", ip_forwarded);
			empty = false;
		}

		public void SetAccountAddressStreet1(string account_address_street1)
		{
			attributes.Add("account_address_street1", account_address_street1);
			empty = false;
		}

		public void SetAccountAddressStreet2(string account_address_street2)
		{
			attributes.Add("account_address_street2", account_address_street2);
			empty = false;
		}

		public void SetAccountAddressCity(string account_address_city)
		{
			attributes.Add("account_address_city", account_address_city);
			empty = false;
		}

		public void SetAccountAddressState(string account_address_state)
		{
			attributes.Add("account_address_state", account_address_state);
			empty = false;
		}

		public void SetAccountAddressCountry(string account_address_country)
		{
			attributes.Add("account_address_country", account_address_country);
			empty = false;
		}

		public void SetAccountAddressZip(string account_address_zip)
		{
			attributes.Add("account_address_zip", account_address_zip);
			empty = false;
		}

		public void SetShippingAddressStreet1(string shipping_address_street1)
		{
			attributes.Add("shipping_address_street1", shipping_address_street1);
			empty = false;
		}

		public void SetShippingAddressStreet2(string shipping_address_street2)
		{
			attributes.Add("shipping_address_street2", shipping_address_street2);
			empty = false;
		}

		public void SetShippingAddressCity(string shipping_address_city)
		{
			attributes.Add("shipping_address_city", shipping_address_city);
			empty = false;
		}

		public void SetShippingAddressState(string shipping_address_state)
		{
			attributes.Add("shipping_address_state", shipping_address_state);
			empty = false;
		}

		public void SetShippingAddressCountry(string shipping_address_country)
		{
			attributes.Add("shipping_address_country", shipping_address_country);
			empty = false;
		}

		public void SetShippingAddressZip(string shipping_address_zip)
		{
			attributes.Add("shipping_address_zip", shipping_address_zip);
			empty = false;
		}

		public void setDeviceId(string device_id)
		{
			attributes.Add("device_id", device_id);
			empty = false;
		}

		public void setAccountLogin(string account_login)
		{
			attributes.Add("account_login", account_login);
			empty = false;
		}

		public void setPasswordHash(string password_hash)
		{
			attributes.Add("password_hash", password_hash);
			empty = false;
		}

		public void setAccountNumber(string account_number)
		{
			attributes.Add("account_number", account_number);
			empty = false;
		}

		public void setAccountName(string account_name)
		{
			attributes.Add("account_name", account_name);
			empty = false;
		}

		public void setAccountEmail(string account_email)
		{
			attributes.Add("account_email", account_email);
			empty = false;
		}

		public void setAccountTelephone(string account_telephone)
		{
			attributes.Add("account_telephone", account_telephone);
			empty = false;
		}

		public void setCCNumberHash(string cc_number_hash)
		{
			attributes.Add("cc_number_hash", cc_number_hash);
			empty = false;
		}

		public void setIPAddress(string ip_address)
		{
			attributes.Add("ip_address", ip_address);
			empty = false;
		}

		public void setIPForwarded(string ip_forwarded)
		{
			attributes.Add("ip_forwarded", ip_forwarded);
			empty = false;
		}

		public void setAccountAddressStreet1(string account_address_street1)
		{
			attributes.Add("account_address_street1", account_address_street1);
			empty = false;
		}

		public void setAccountAddressStreet2(string account_address_street2)
		{
			attributes.Add("account_address_street2", account_address_street2);
			empty = false;
		}

		public void setAccountAddressCity(string account_address_city)
		{
			attributes.Add("account_address_city", account_address_city);
			empty = false;
		}

		public void setAccountAddressState(string account_address_state)
		{
			attributes.Add("account_address_state", account_address_state);
			empty = false;
		}

		public void setAccountAddressCountry(string account_address_country)
		{
			attributes.Add("account_address_country", account_address_country);
			empty = false;
		}

		public void setAccountAddressZip(string account_address_zip)
		{
			attributes.Add("account_address_zip", account_address_zip);
			empty = false;
		}

		public void setShippingAddressStreet1(string shipping_address_street1)
		{
			attributes.Add("shipping_address_street1", shipping_address_street1);
			empty = false;
		}

		public void setShippingAddressStreet2(string shipping_address_street2)
		{
			attributes.Add("shipping_address_street2", shipping_address_street2);
			empty = false;
		}

		public void setShippingAddressCity(string shipping_address_city)
		{
			attributes.Add("shipping_address_city", shipping_address_city);
			empty = false;
		}

		public void setShippingAddressState(string shipping_address_state)
		{
			attributes.Add("shipping_address_state", shipping_address_state);
			empty = false;
		}

		public void setShippingAddressCountry(string shipping_address_country)
		{
			attributes.Add("shipping_address_country", shipping_address_country);
			empty = false;
		}

		[Obsolete("This is deprecated due to the lower case set, please use SetShippingAddressZip instead.")]
		public void setShippingAddressZip(string shipping_address_zip)
		{
			attributes.Add("shipping_address_zip", shipping_address_zip);
			empty = false;
		}

		public override string toXML(string country)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<risk>");
			stringBuilder.Append("<attribute_query>");
			stringBuilder.Append(toXML());
			if (!empty)
			{
				stringBuilder.Append("<attribute_account_info>");
				IDictionaryEnumerator enumerator = attributes.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (enumerator.Value != null)
					{
						stringBuilder.Append("<" + enumerator.Key.ToString() + ">" + enumerator.Value.ToString() + "</" + enumerator.Key.ToString() + ">");
					}
				}
				stringBuilder.Append("</attribute_account_info>");
			}
			stringBuilder.Append("</attribute_query>");
			stringBuilder.Append("</risk>");
			return stringBuilder.ToString();
		}
	}
}
