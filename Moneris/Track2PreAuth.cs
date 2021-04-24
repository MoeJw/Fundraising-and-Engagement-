using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Track2PreAuth : Transaction
	{
		private CustInfo custInfo = new CustInfo();

		private Recur recurInfo;

		private AvsInfo avsInfo;

		private CvdInfo cvdInfo;

		private static string[] xmlTags = new string[10]
		{
			"order_id",
			"cust_id",
			"amount",
			"track2",
			"pan",
			"expdate",
			"pos_code",
			"dynamic_descriptor",
			"commcard_invoice",
			"commcard_tax_amount"
		};

		public Track2PreAuth()
			: base(xmlTags)
		{
		}

		public Track2PreAuth(Hashtable preauth)
			: base(preauth, xmlTags)
		{
		}

		public Track2PreAuth(string order_id, string amount, string track2, string pan, string expdate, string pos_code)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("track2", track2);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("pos_code", pos_code);
		}

		public Track2PreAuth(string order_id, string cust_id, string amount, string track2, string pan, string expdate, string pos_code)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("track2", track2);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("pos_code", pos_code);
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

		public void SetTrack2(string track2)
		{
			transactionParams.Add("track2", track2);
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetPosCode(string pos_code)
		{
			transactionParams.Add("pos_code", pos_code);
		}

		public void SetCommcardInvoice(string commcard_invoice)
		{
			transactionParams.Add("commcard_invoice", commcard_invoice);
		}

		public void SetCommcardTaxAmount(string commcard_tax_amount)
		{
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
		}

		public void SetAvsAddress(string avs_address)
		{
			transactionParams.Add("avs_address", avs_address);
		}

		public void SetAvsZipCode(string avs_street)
		{
			transactionParams.Add("avs_zipcode", avs_street);
		}

		public void SetCvdValue(string cvd_value)
		{
			transactionParams.Add("cvd_value", cvd_value);
		}

		public void SetCvdIndicator(string cvd_indicator)
		{
			transactionParams.Add("cvd_indicator", cvd_indicator);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public void SetRecur(Recur recur)
		{
			recurInfo = recur;
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

		public void SetAvsInfo(AvsInfo avs)
		{
			avsInfo = avs;
		}

		public void SetCvdInfo(CvdInfo cvd)
		{
			cvdInfo = cvd;
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "track2_preauth" : "us_track2_preauth");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (transactionParams["track2"].ToString().Equals(""))
			{
				stringBuilder.Append("<track2></track2>");
			}
			if (!custInfo.IsEmpty())
			{
				stringBuilder.Append(custInfo.toXML());
			}
			if (recurInfo != null)
			{
				stringBuilder.Append(recurInfo.toXML());
			}
			if (avsInfo != null)
			{
				stringBuilder.Append(avsInfo.toXML());
			}
			if (cvdInfo != null)
			{
				stringBuilder.Append(cvdInfo.toXML());
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
