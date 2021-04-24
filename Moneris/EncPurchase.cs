using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class EncPurchase : Transaction
	{
		private CustInfo custInfo = new CustInfo();

		private Recur recurInfo;

		private AvsInfo avsInfo;

		private CvdInfo cvdInfo;

		private static string[] xmlTags = new string[9]
		{
			"order_id",
			"cust_id",
			"amount",
			"enc_track2",
			"device_type",
			"crypt_type",
			"commcard_invoice",
			"commcard_tax_amount",
			"dynamic_descriptor"
		};

		public EncPurchase()
			: base(xmlTags)
		{
		}

		public EncPurchase(Hashtable encpurchase)
			: base(encpurchase, xmlTags)
		{
		}

		public EncPurchase(string order_id, string amount, string enc_track2, string device_type, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public EncPurchase(string order_id, string cust_id, string amount, string enc_track2, string device_type, string crypt_type)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("crypt_type", crypt_type);
		}

		public EncPurchase(string order_id, string amount, string enc_track2, string device_type, string crypt_type, string commcard_invoice, string commcard_tax_amount)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("crypt_type", crypt_type);
			transactionParams.Add("commcard_invoice", commcard_invoice);
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
		}

		public EncPurchase(string order_id, string cust_id, string amount, string enc_track2, string device_type, string crypt_type, string commcard_invoice, string commcard_tax_amount)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("enc_track2", enc_track2);
			transactionParams.Add("device_type", device_type);
			transactionParams.Add("crypt_type", crypt_type);
			transactionParams.Add("commcard_invoice", commcard_invoice);
			transactionParams.Add("commcard_tax_amount", commcard_tax_amount);
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

		public void SetEncTrack2(string enc_track2)
		{
			transactionParams.Add("enc_track2", enc_track2);
		}

		public void SetDeviceType(string device_type)
		{
			transactionParams.Add("device_type", device_type);
		}

		public void SetPosCode(string pos_code)
		{
			transactionParams.Add("pos_code", pos_code);
		}

		public void SetCryptType(string crypt_type)
		{
			transactionParams.Add("crypt_type", crypt_type);
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
			string str = ((!country.Equals("US")) ? "enc_purchase" : "us_enc_purchase");
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
