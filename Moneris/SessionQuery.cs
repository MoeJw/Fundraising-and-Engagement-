using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class SessionQuery : Transaction
	{
		private Hashtable keyHashes = new Hashtable();

		private Hashtable sessionKeyHashes = new Hashtable();

		private bool empty = true;

		private static string[] xmlTags = new string[3]
		{
			"order_id",
			"session_id",
			"service_type"
		};

		public SessionQuery()
			: base(xmlTags)
		{
		}

		public SessionQuery(Hashtable session_query)
			: base(session_query, xmlTags)
		{
		}

		public SessionQuery(string order_id, string session_id, string service_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("session_id", session_id);
			transactionParams.Add("service_type", service_type);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetSessionId(string session_id)
		{
			transactionParams.Add("session_id", session_id);
		}

		public void SetServiceType(string service_type)
		{
			transactionParams.Add("service_type", service_type);
		}

		public void SetEventType(string event_type)
		{
			transactionParams.Add("event_type", event_type);
		}

		public void SetPolicy(string policy)
		{
			transactionParams.Add("policy", policy);
			empty = false;
		}

		public void SetDeviceId(string device_id)
		{
			transactionParams.Add("device_id", device_id);
			empty = false;
		}

		public void SetAccountLogin(string account_login)
		{
			transactionParams.Add("account_login", account_login);
			empty = false;
		}

		public void SetPasswordHash(string password_hash)
		{
			transactionParams.Add("password_hash", password_hash);
			empty = false;
		}

		public void SetAccountNumber(string account_number)
		{
			transactionParams.Add("account_number", account_number);
			empty = false;
		}

		public void SetAccountName(string account_name)
		{
			transactionParams.Add("account_name", account_name);
			empty = false;
		}

		public void SetAccountEmail(string account_email)
		{
			transactionParams.Add("account_email", account_email);
			empty = false;
		}

		public void SetAccountTelephone(string account_telephone)
		{
			transactionParams.Add("account_telephone", account_telephone);
			empty = false;
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
			empty = false;
		}

		public void SetAccountAddressStreet1(string account_address_street1)
		{
			transactionParams.Add("account_address_street1", account_address_street1);
			empty = false;
		}

		public void SetAccountAddressStreet2(string account_address_street2)
		{
			transactionParams.Add("account_address_street2", account_address_street2);
			empty = false;
		}

		public void SetAccountAddressCity(string account_address_city)
		{
			transactionParams.Add("account_address_city", account_address_city);
			empty = false;
		}

		public void SetAccountAddressState(string account_address_state)
		{
			transactionParams.Add("account_address_state", account_address_state);
			empty = false;
		}

		public void SetAccountAddressCountry(string account_address_country)
		{
			transactionParams.Add("account_address_country", account_address_country);
			empty = false;
		}

		public void SetAccountAddressZip(string account_address_zip)
		{
			transactionParams.Add("account_address_zip", account_address_zip);
			empty = false;
		}

		public void SetShippingAddressStreet1(string shipping_address_street1)
		{
			transactionParams.Add("shipping_address_street1", shipping_address_street1);
			empty = false;
		}

		public void SetShippingAddressStreet2(string shipping_address_street2)
		{
			transactionParams.Add("shipping_address_street2", shipping_address_street2);
			empty = false;
		}

		public void SetShippingAddressCity(string shipping_address_city)
		{
			transactionParams.Add("shipping_address_city", shipping_address_city);
			empty = false;
		}

		public void SetShippingAddressState(string shipping_address_state)
		{
			transactionParams.Add("shipping_address_state", shipping_address_state);
			empty = false;
		}

		public void SetShippingAddressCountry(string shipping_address_country)
		{
			transactionParams.Add("shipping_address_country", shipping_address_country);
			empty = false;
		}

		public void SetShippingAddressZip(string shipping_address_zip)
		{
			transactionParams.Add("shipping_address_zip", shipping_address_zip);
			empty = false;
		}

		public void SetLocalAttrib1(string local_attrib_1)
		{
			transactionParams.Add("local_attrib_1", local_attrib_1);
			empty = false;
		}

		public void SetLocalAttrib2(string local_attrib_2)
		{
			transactionParams.Add("local_attrib_2", local_attrib_2);
			empty = false;
		}

		public void SetLocalAttrib3(string local_attrib_3)
		{
			transactionParams.Add("local_attrib_3", local_attrib_3);
			empty = false;
		}

		public void SetLocalAttrib4(string local_attrib_4)
		{
			transactionParams.Add("local_attrib_4", local_attrib_4);
			empty = false;
		}

		public void SetLocalAttrib5(string local_attrib_5)
		{
			transactionParams.Add("local_attrib_5", local_attrib_5);
			empty = false;
		}

		public void SetTransactionAmount(string transaction_amount)
		{
			transactionParams.Add("transaction_amount", transaction_amount);
			empty = false;
		}

		public void SetTransactionCurrency(string transaction_currency)
		{
			transactionParams.Add("transaction_currency", transaction_currency);
			empty = false;
		}

		public override string toXML(string country)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<risk>");
			stringBuilder.Append("<session_query>");
			stringBuilder.Append(toXML());
			IDictionaryEnumerator enumerator = keyHashes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				stringBuilder.Append("<" + enumerator.Key.ToString() + ">" + enumerator.Value.ToString() + "</" + enumerator.Key.ToString() + ">");
			}
			if (!empty)
			{
				stringBuilder.Append("<session_account_info>");
				IDictionaryEnumerator enumerator2 = sessionKeyHashes.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					stringBuilder.Append("<" + enumerator2.Key.ToString() + ">" + enumerator2.Value.ToString() + "</" + enumerator2.Key.ToString() + ">");
				}
				stringBuilder.Append("</session_account_info>");
			}
			stringBuilder.Append("</session_query>");
			stringBuilder.Append("</risk>");
			return stringBuilder.ToString();
		}
	}
}
