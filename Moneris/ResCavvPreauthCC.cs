using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ResCavvPreauthCC : Transaction
	{
		private static string[] xmlTags = new string[8]
		{
			"data_key",
			"order_id",
			"cust_id",
			"amount",
			"cavv",
			"crypt_type",
			"dynamic_descriptor",
			"expdate"
		};

		private CustInfo custInfo = new CustInfo();

		private AvsInfo avsInfo = new AvsInfo();

		private CvdInfo cvdInfo = new CvdInfo();

		public ResCavvPreauthCC()
			: base(xmlTags)
		{
		}

		public ResCavvPreauthCC(Hashtable res_cavv_preauth)
			: base(res_cavv_preauth, xmlTags)
		{
		}

		public ResCavvPreauthCC(string data_key, string order_id, string amount, string cavv)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("cavv", cavv);
		}

		public ResCavvPreauthCC(string data_key, string order_id, string cust_id, string amount, string cavv)
			: base(xmlTags)
		{
			transactionParams.Add("data_key", data_key);
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("cavv", cavv);
		}

		public void SetDataKey(string data_key)
		{
			transactionParams.Add("data_key", data_key);
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

		public void SetCavv(string cavv)
		{
			transactionParams.Add("cavv", cavv);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
		}

		public void SetCvdInfo(CvdInfo cvd)
		{
			cvdInfo = cvd;
		}

		public void SetCustInfo(CustInfo cust)
		{
			custInfo = cust;
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
			string str = ((!country.Equals("US")) ? "res_cavv_preauth_cc" : "us_res_cavv_preauth_cc");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (!custInfo.IsEmpty())
			{
				stringBuilder.Append(custInfo.toXML());
			}
			if (!avsInfo.IsEmpty())
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			if (!cvdInfo.IsEmpty())
			{
				stringBuilder.Append(cvdInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
