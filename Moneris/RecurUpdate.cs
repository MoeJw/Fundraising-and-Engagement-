using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Moneris
{
	[ComVisible(true)]
	public class RecurUpdate : Transaction
	{
		private static string[] xmlTags = new string[9]
		{
			"order_id",
			"cust_id",
			"recur_amount",
			"pan",
			"expdate",
			"add_num_recurs",
			"total_num_recurs",
			"hold",
			"terminate"
		};

		public RecurUpdate()
			: base(xmlTags)
		{
		}

		public RecurUpdate(Hashtable recur_update)
			: base(recur_update, xmlTags)
		{
		}

		public RecurUpdate(string order_id)
			: base(xmlTags)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetOrderId(string order_id)
		{
			transactionParams.Add("order_id", order_id);
		}

		public void SetCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		public void SetRecurAmount(string recur_amount)
		{
			transactionParams.Add("recur_amount", recur_amount);
		}

		public void SetPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		public void SetExpDate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		public void SetAddNumRecurs(string add_num_recurs)
		{
			transactionParams.Add("add_num_recurs", add_num_recurs);
		}

		public void SetTotalNumRecurs(string total_num_recurs)
		{
			transactionParams.Add("total_num_recurs", total_num_recurs);
		}

		public void SetHold(string hold)
		{
			transactionParams.Add("hold", hold);
		}

		public void SetTerminate(string terminate)
		{
			transactionParams.Add("terminate", terminate);
		}

		[Obsolete("This method is obsolete; use SetCustId with a capital S instead")]
		public void setCustId(string cust_id)
		{
			transactionParams.Add("cust_id", cust_id);
		}

		[Obsolete("This method is obsolete; use SetRecurAmount with a capital S instead")]
		public void setRecurAmount(string recur_amount)
		{
			transactionParams.Add("recur_amount", recur_amount);
		}

		[Obsolete("This method is obsolete; use SetPan with a capital S instead")]
		public void setPan(string pan)
		{
			transactionParams.Add("pan", pan);
		}

		[Obsolete("This method is obsolete; use SetExpdate with a capital S instead")]
		public void setExpdate(string expdate)
		{
			transactionParams.Add("expdate", expdate);
		}

		[Obsolete("This method is obsolete; use SetAddNumRecurs with a capital S instead")]
		public void setAddNumRecurs(string add_num_recurs)
		{
			transactionParams.Add("add_num_recurs", add_num_recurs);
		}

		[Obsolete("This method is obsolete; use SetTotalNumRecurs with a capital S instead")]
		public void setTotalNumRecurs(string total_num_recurs)
		{
			transactionParams.Add("total_num_recurs", total_num_recurs);
		}

		[Obsolete("This method is obsolete; use SetHold with a capital S instead")]
		public void setHold(string hold)
		{
			transactionParams.Add("hold", hold);
		}

		[Obsolete("This method is obsolete; use SetTerminate with a capital S instead")]
		public void setTerminate(string terminate)
		{
			transactionParams.Add("terminate", terminate);
		}

		public override string toXML(string country)
		{
			string str = ((!country.Equals("US")) ? "recur_update" : "us_recur_update");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<" + str + ">");
			stringBuilder.Append(toXML());
			stringBuilder.Append("</" + str + ">");
			return stringBuilder.ToString();
		}
	}
}
