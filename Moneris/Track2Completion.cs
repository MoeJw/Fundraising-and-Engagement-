using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Track2Completion : Transaction
	{
		private CustInfo custInfo = new CustInfo();

		private Recur recurInfo;

		private AvsInfo avsInfo;

		private CvdInfo cvdInfo;

		private static string[] xmlTags = new string[8]
		{
			"order_id",
			"txn_number",
			"comp_amount",
			"pos_code",
			"commcard_invoice",
			"commcard_tax_amount",
			"cust_id",
			"dynamic_descriptor"
		};

		public Track2Completion()
			: base(xmlTags)
		{
		}

		public Track2Completion(Hashtable track2completion)
			: base(track2completion, xmlTags)
		{
		}

		public Track2Completion(string order_id, string txn_number, string comp_amount, string pos_code, string commcard_invoice, string commcard_tax_amount)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("comp_amount", comp_amount);
		}

		public Track2Completion(string order_id, string comp_amount, string txn_number)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("txn_number", txn_number);
			transactionParams.Add("comp_amount", comp_amount);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetTxnNumber(string txn_number)
		{
			transactionParams.Add("txn_number", txn_number);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetAmount(string comp_amount)
		{
			transactionParams.Add("comp_amount", comp_amount);
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

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
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
			string str = ((!country.Equals("US")) ? "track2_completion" : "us_track2_completion");
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
