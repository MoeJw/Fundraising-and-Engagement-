using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class PaypassSendShoppingCart : Transaction
	{
		private static string[] xmlTags = new string[5]
		{
			"subtotal",
			"suppress_shipping_address",
			"merchant_callback_url",
			"merchant_card_list",
			"merchant_currency"
		};

		public PaypassSendShoppingCart()
			: base(xmlTags)
		{
		}

		public PaypassSendShoppingCart(Hashtable paypass_send_shopping_cart)
			: base(paypass_send_shopping_cart, xmlTags)
		{
		}

		public PaypassSendShoppingCart(string subtotal, string suppress_shipping_address)
			: base(xmlTags)
		{
			transactionParams.Add("subtotal", subtotal);
			transactionParams.Add("suppress_shipping_address", suppress_shipping_address);
		}

		public void SetSubtotal(string subtotal)
		{
			transactionParams.Add("subtotal", subtotal);
		}

		public void SetSuppressShippingAddress(string suppress_shipping_address)
		{
			transactionParams.Add("suppress_shipping_address", suppress_shipping_address);
		}

		public void SetMerchantCallbackUrl(string merchant_callback_url)
		{
			transactionParams.Add("merchant_callback_url", merchant_callback_url);
		}

		public void SetMerchantCardList(string merchant_card_list)
		{
			transactionParams.Add("merchant_card_list", merchant_card_list);
		}

		public void SetMerchantCurrency(string merchant_currency)
		{
			transactionParams.Add("merchant_currency", merchant_currency);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "paypass_send_shopping_cart" : "us_paypass_send_shopping_cart");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
