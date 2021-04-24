using System;
using System.Net;

namespace Plugins
{
	internal class WebAPIClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)base.GetWebRequest(address);
			httpWebRequest.KeepAlive = false;
			return httpWebRequest;
		}
	}
}
