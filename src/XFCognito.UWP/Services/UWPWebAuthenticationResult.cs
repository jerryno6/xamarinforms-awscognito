using Windows.Security.Authentication.Web;
using Windows.Web;

namespace XFCognito.UWP.Services
{
    public class UWPWebAuthenticationResult
    {
        public UWPWebAuthenticationResult(string responseData, WebErrorStatus webErrorStatus, WebAuthenticationStatus responseStatus)
        {
            ResponseData = responseData;
            WebErrorStatus = webErrorStatus;
            ResponseStatus = responseStatus;
        }

        public string ResponseData { get; }
        public WebErrorStatus WebErrorStatus { get; }
        public WebAuthenticationStatus ResponseStatus { get; }
    }
}

