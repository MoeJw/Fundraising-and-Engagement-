using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class Track2ForcePost : Transaction
	{
		private static string[] xmlTags = new string[9]
		{
			"order_id",
			"cust_id",
			"amount",
			"track2",
			"pan",
			"expdate",
			"pos_code",
			"auth_code",
			"dynamic_descriptor"
		};

		public Track2ForcePost()
			: base(xmlTags)
		{
		}

		public Track2ForcePost(Hashtable track2forcepost)
			: base(track2forcepost, xmlTags)
		{
		}

		public Track2ForcePost(string order_id, string amount, string track2, string pan, string expdate, string pos_code, string auth_code)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("track2", track2);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("pos_code", pos_code);
			transactionParams.Add("auth_code", auth_code);
		}

		public Track2ForcePost(string order_id, string cust_id, string amount, string track2, string pan, string expdate, string pos_code, string auth_code)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
			transactionParams.Add("cust_id", cust_id);
			transactionParams.Add("amount", amount);
			transactionParams.Add("track2", track2);
			transactionParams.Add("pan", pan);
			transactionParams.Add("expdate", expdate);
			transactionParams.Add("pos_code", pos_code);
			transactionParams.Add("auth_code", auth_code);
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

		public void SetAuthCode(string auth_code)
		{
			transactionParams.Add("auth_code", auth_code);
		}

		public void SetDynamicDescriptor(string dynamic_descriptor)
		{
			transactionParams.Add("dynamic_descriptor", dynamic_descriptor);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "track2_forcepost" : "us_track2_forcepost");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			if (transactionParams["track2"].ToString().Equals(""))
			{
				stringBuilder.Append("<track2></track2>");
			}
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
