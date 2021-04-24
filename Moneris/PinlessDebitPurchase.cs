using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class PinlessDebitPurchase : Transaction
	{
		private CustInfo custInfo = new CustInfo();

		private Recur recurInfo;

		private static string[] xmlTags = new string[8]
		{
			"order_id",
			"cust_id",
			"amount",
			"pan",
			"expdate",
			"presentation_type",
			"intended_use",
			"p_account_number"
		};

		public PinlessDebitPurchase()
			: base(xmlTags)
		{
		}

		public PinlessDebitPurchase(Hashtable pinless_debit_purchase)
			: base(pinless_debit_purchase, xmlTags)
		{
		}

		public PinlessDebitPurchase(string order_id, string amount, string pan, string expdate, string presentation_type, string intended_use, string p_account_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("presentation_type", presentation_type);
			transactionParams.Add("intended_use", intended_use);
			transactionParams.Add("p_account_number", p_account_number);
		}

		public PinlessDebitPurchase(string order_id, string cust_id, string amount, string pan, string expdate, string presentation_type, string intended_use, string p_account_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("presentation_type", presentation_type);
			transactionParams.Add("intended_use", intended_use);
			transactionParams.Add("p_account_number", p_account_number);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAmount(string amount)
		{
			transactionParams.Add("amount", amount);
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetPresentationType(string presentation_type)
		{
			transactionParams.Add("presentation_type", presentation_type);
		}

		public void SetIntendedUse(string intended_use)
		{
			transactionParams.Add("intended_use", intended_use);
		}

		public void SetPAccountNumber(string p_account_number)
		{
			transactionParams.Add("p_account_number", p_account_number);
		}

		public void SetCustInfo(CustInfo cust)
		{
			custInfo = cust;
		}

		public void SetRecur(Recur recur)
		{
			recurInfo = recur;
		}

		public void SetInstructions(string someInstructions)
		{
			custInfo.SetInstructions(someInstructions);
		}

		public void SetEmail(string anEmail)
		{
			custInfo.SetEmail(anEmail);
		}

		public void SetShipping(Hashtable shippingInfo)
		{
			custInfo.SetShipping(shippingInfo);
		}

		public void SetShipping(string first_name, string last_name, string company_name, string address, string city, string province, string postal_code, string country, string phone_number, string fax, string tax1, string tax2, string tax3, string shipping_cost)
		{
			custInfo.SetShipping(first_name, last_name, company_name, address, city, province, postal_code, country, phone_number, fax, tax1, tax2, tax3, shipping_cost);
		}

		public void SetBilling(Hashtable billingInfo)
		{
			custInfo.SetBilling(billingInfo);
		}

		public void SetBilling(string first_name, string last_name, string company_name, string address, string city, string province, string postal_code, string country, string phone_number, string fax, string tax1, string tax2, string tax3, string shipping_cost)
		{
			custInfo.SetBilling(first_name, last_name, company_name, address, city, province, postal_code, country, phone_number, fax, tax1, tax2, tax3, shipping_cost);
		}

		public void SetItem(Hashtable itemInfo)
		{
			custInfo.SetItem(itemInfo);
		}

		public void SetItem(string name, string quantity, string product_code, string extended_amount)
		{
			custInfo.SetItem(name, quantity, product_code, extended_amount);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "pinless_debit_purchase" : "us_pinless_debit_purchase");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (!custInfo.IsEmpty())
			{
				stringBuilder.Append(custInfo.toXML());
			}
			if (recurInfo != null)
			{
				stringBuilder.Append(recurInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
