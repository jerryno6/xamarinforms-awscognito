
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using XFCognito.UWP.Views;

namespace XFCognito.UWP.Services
{
    public class UWPWebAuthenticatorImplementation : IWebAuthenticator
    {
        public static TaskCompletionSource<UWPWebAuthenticationResult> BrowserAuthenticationTaskCompletionSource { get; private set; }

        private ContentDialog webViewDialog;

        private Uri _callBackUri;

        public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
        {
            return AuthenticateAsync(webAuthenticatorOptions.Url, webAuthenticatorOptions.CallbackUrl);
        }

        public async Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
        {
            _callBackUri = callbackUrl;

            //Show webview dialog
            BrowserAuthenticationTaskCompletionSource = new TaskCompletionSource<UWPWebAuthenticationResult>();
            webViewDialog = CreateWebviewDialog(url);

            //we do not call await because in some cases, the task completes by jumping to the OnActivated method
            webViewDialog.ShowAsync();

            //Wait the webview to return the callback url
            var result = await BrowserAuthenticationTaskCompletionSource.Task;

            //Once we get result from the webviewDialog, hide it
            webViewDialog.Hide();

            //Return result base on status
            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    // For GET requests this is a URI:
                    var resultUri = new Uri(result.ResponseData.ToString());
                    return new WebAuthenticatorResult(resultUri);
                case WebAuthenticationStatus.UserCancel:
                    throw new TaskCanceledException();
                case WebAuthenticationStatus.ErrorHttp:
                    throw new HttpRequestException("Error: " + result.WebErrorStatus.ToString());
                default:
                    throw new Exception("Response: " + result.ResponseData.ToString() + "\nStatus: " + result.ResponseStatus);
            }
        }

        private ContentDialog CreateWebviewDialog(Uri url)
        {
            WebviewDialog dialog = new WebviewDialog();

            dialog.WebView.Source = url;

            dialog.WebView.NavigationCompleted += (s, e) =>
            {
                OnCallbackUrlReturned(e, _callBackUri);
            };

            dialog.CloseButtonClick += (sender, arg) =>
            {
                var result = new UWPWebAuthenticationResult(null, WebErrorStatus.Unknown, WebAuthenticationStatus.UserCancel);
                BrowserAuthenticationTaskCompletionSource.TrySetResult(result);
            };

            return dialog;
        }

        private void OnCallbackUrlReturned(WebViewNavigationCompletedEventArgs args, Uri callbackUri)
        {
            var returnedUrl = args.Uri.AbsoluteUri;

            //validate input
            if (string.IsNullOrEmpty(returnedUrl) ||
                !returnedUrl.StartsWith(callbackUri.AbsoluteUri))
            {
                return;
            }

            //return parameters from backend via CompletionSource
            var result = new UWPWebAuthenticationResult(args.Uri.AbsoluteUri, args.WebErrorStatus, WebAuthenticationStatus.Success);
            BrowserAuthenticationTaskCompletionSource.TrySetResult(result);
        }
    }
}
