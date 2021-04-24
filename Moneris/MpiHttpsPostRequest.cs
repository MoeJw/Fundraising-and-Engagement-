using System.Runtime.InteropServices;

namespace Moneris
{
	[ComVisible(true)]
	public class MpiHttpsPostRequest : HttpsPostRequest
	{
		private HttpsPostRequest mpiReq;

		public MpiHttpsPostRequest(string host, string store_id, string api_token, Transaction t)
		{
			mpiReq = new HttpsPostRequest(host, store_id, api_token, t);
		}

		public new MpiReceipt GetReceipt()
		{
			return new MpiReceipt(mpiReq.GetReceipt());
		}
	}
}
