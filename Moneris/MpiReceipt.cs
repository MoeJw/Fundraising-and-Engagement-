using System;
using System.Runtime.InteropServices;

namespace Moneris
{
	[Obsolete("This object is obsolete; use Receipt object instead")]
	[ComVisible(true)]
	public class MpiReceipt
	{
		private Receipt receiptObj;

		internal MpiReceipt(Receipt receipt)
		{
			receiptObj = receipt;
		}

		[Obsolete("This method is obsolete; use GetMpiSuccess() along with \"Receipt\" Object instead")]
		public string GetSuccess()
		{
			return receiptObj.GetMpiSuccess();
		}

		[Obsolete("This method is obsolete; use GetMpiMessage() along with \"Receipt\" Object instead")]
		public string GetMessage()
		{
			return receiptObj.GetMpiMessage();
		}

		[Obsolete("This method is obsolete; use GetMpiPaReq() along with \"Receipt\" Object instead")]
		public string GetPaReq()
		{
			return receiptObj.GetMpiPaReq();
		}

		[Obsolete("This method is obsolete; use GetMpiTermUrl() along with \"Receipt\" Object instead")]
		public string GetTermUrl()
		{
			return receiptObj.GetMpiTermUrl();
		}

		[Obsolete("This method is obsolete; use GetMpiMD() along with \"Receipt\" Object instead")]
		public string GetMD()
		{
			return receiptObj.GetMpiMD();
		}

		[Obsolete("This method is obsolete; use GetMpiACSUrl() along with \"Receipt\" Object instead")]
		public string GetACSUrl()
		{
			return receiptObj.GetMpiACSUrl();
		}

		[Obsolete("This method is obsolete; use GetMpiCavv() along with \"Receipt\" Object instead")]
		public string GetCavv()
		{
			return receiptObj.GetMpiCavv();
		}

		[Obsolete("This method is obsolete; use GetMpiPAResVerified() along with \"Receipt\" Object instead")]
		public string GetPAResVerified()
		{
			return receiptObj.GetMpiPAResVerified();
		}

		public string GetInLineForm()
		{
			return receiptObj.GetInLineForm();
		}
	}
}
