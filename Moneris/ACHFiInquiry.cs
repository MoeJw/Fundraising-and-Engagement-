using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class ACHFiInquiry : Transaction
	{
		private static string[] xmlTags = new string[1]
		{
			"routing_num"
		};

		public ACHFiInquiry()
			: base(xmlTags)
		{
		}

		public void SetRoutingNum(string routing_num)
		{
			transactionParams.Add("routing_num", routing_num);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "ach_fi_enquiry" : "us_ach_fi_enquiry");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
