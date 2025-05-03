namespace AnnoModificationManager4.Misc
{
    using System;
    using System.Net;

    public class TimeoutWebClient : WebClient
    {
        private int timeOut = 2000;

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = this.timeOut;
            return webRequest;
        }     
    }
}

