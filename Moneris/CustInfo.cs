using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class CustInfo
	{
		protected Hashtable billingParams = null;

		protected Hashtable shippingParams = null;

		protected Queue itemParamsQueue = new Queue();

		protected string email;

		protected string instructions;

		private bool empty = true;

		private string[] billingTags = new string[14]
		{
			"first_name",
			"last_name",
			"company_name",
			"address",
			"city",
			"province",
			"postal_code",
			"country",
			"phone_number",
			"fax",
			"tax1",
			"tax2",
			"tax3",
			"shipping_cost"
		};

		private string[] shippingTags = new string[14]
		{
			"first_name",
			"last_name",
			"company_name",
			"address",
			"city",
			"province",
			"postal_code",
			"country",
			"phone_number",
			"fax",
			"tax1",
			"tax2",
			"tax3",
			"shipping_cost"
		};

		private string[] itemTags = new string[4]
		{
			"name",
			"quantity",
			"product_code",
			"extended_amount"
		};

		public void SetInstructions(string someInstructions)
		{
			empty = false;
			instructions = someInstructions;
		}

		public void SetEmail(string anEmail)
		{
			empty = false;
			email = anEmail;
		}

		public void SetShipping(Hashtable shippingInfo)
		{
			empty = false;
			shippingParams = shippingInfo;
		}

		public void SetShipping(string first_name, string last_name, string company_name, string address, string city, string province, string postal_code, string country, string phone_number, string fax, string tax1, string tax2, string tax3, string shipping_cost)
		{
			empty = false;
			shippingParams = new Hashtable();
			shippingParams.Add("first_name", first_name);
			shippingParams.Add("last_name", last_name);
			shippingParams.Add("company_name", company_name);
			shippingParams.Add("address", address);
			shippingParams.Add("city", city);
			shippingParams.Add("province", province);
			shippingParams.Add("postal_code", postal_code);
			shippingParams.Add("country", country);
			shippingParams.Add("phone_number", phone_number);
			shippingParams.Add("fax", fax);
			shippingParams.Add("tax1", tax1);
			shippingParams.Add("tax2", tax2);
			shippingParams.Add("tax3", tax3);
			shippingParams.Add("shipping_cost", shipping_cost);
		}

		public void SetBilling(Hashtable billingInfo)
		{
			empty = false;
			billingParams = billingInfo;
		}

		public void SetBilling(string first_name, string last_name, string company_name, string address, string city, string province, string postal_code, string country, string phone_number, string fax, string tax1, string tax2, string tax3, string shipping_cost)
		{
			empty = false;
			billingParams = new Hashtable();
			billingParams.Add("first_name", first_name);
			billingParams.Add("last_name", last_name);
			billingParams.Add("company_name", company_name);
			billingParams.Add("address", address);
			billingParams.Add("city", city);
			billingParams.Add("province", province);
			billingParams.Add("postal_code", postal_code);
			billingParams.Add("country", country);
			billingParams.Add("phone_number", phone_number);
			billingParams.Add("fax", fax);
			billingParams.Add("tax1", tax1);
			billingParams.Add("tax2", tax2);
			billingParams.Add("tax3", tax3);
			billingParams.Add("shipping_cost", shipping_cost);
		}

		public void SetItem(Hashtable itemInfo)
		{
			empty = false;
			itemParamsQueue.Enqueue(itemInfo);
		}

		public void SetItem(string name, string quantity, string product_code, string extended_amount)
		{
			empty = false;
			Hashtable hashtable = new Hashtable();
			hashtable.Add("name", name);
			hashtable.Add("quantity", quantity);
			hashtable.Add("product_code", product_code);
			hashtable.Add("extended_amount", extended_amount);
			SetItem(hashtable);
		}

		public bool IsEmpty()
		{
			return empty;
		}

		public string toXML()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (empty)
			{
				return "";
			}
			stringBuilder.Append("<cust_info>");
			stringBuilder.Append("<email>" + email + "</email><instructions>" + instructions + "</instructions>");
			stringBuilder.Append("<billing>");
			toXML_low(stringBuilder, billingTags, billingParams);
			stringBuilder.Append("</billing>");
			stringBuilder.Append("<shipping>");
			toXML_low(stringBuilder, shippingTags, shippingParams);
			stringBuilder.Append("</shipping>");
			if (itemParamsQueue.Count != 0)
			{
				IEnumerator enumerator = itemParamsQueue.GetEnumerator();
				while (enumerator.MoveNext())
				{
					stringBuilder.Append("<item>");
					toXML_low(stringBuilder, itemTags, (Hashtable)enumerator.Current);
					stringBuilder.Append("</item>");
				}
			}
			stringBuilder.Append("</cust_info>");
			return stringBuilder.ToString();
		}

		private void toXML_low(StringBuilder sb, string[] xmlTags, Hashtable xmlData)
		{
			foreach (string text in xmlTags)
			{
				string text2 = (string)xmlData[text];
				sb.Append("<" + text + ">" + text2 + "</" + text + ">");
			}
		}
	}
}
