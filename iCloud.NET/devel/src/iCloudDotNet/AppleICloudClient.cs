using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using AppleICloudDotNet.PropertyLists;

namespace AppleICloudDotNet
{
    public class AppleICloudClient
    {
        private const string urlConfigurationsInit = "https://setup.icloud.com/configurations/init";

        private static readonly IList<string> supportedProtocolVersions = new[] { "1.5" };

        private IDictionary<string, string> actionUrls;
        private CookieContainer cookieContainer;

        private readonly ApplePropertyListSerializer plistSerializer;

        public AppleICloudClient()
        {
            this.cookieContainer = new CookieContainer();

            this.plistSerializer = new ApplePropertyListSerializer();

            this.UserAgent = Properties.Resources.ClientUserAgent;
        }

        public AppleAccountInfo AccountInfo
        {
            get;
            private set;
        }

        public string UserAgent
        {
            get;
            private set;
        }

        public void Initialize()
        {
            try
            {
                var responseObj = (IDictionary<string, object>)GetPList(urlConfigurationsInit, false);
                var urls = (IDictionary<string, object>)responseObj["urls"];
                actionUrls = urls.ToDictionary(pair => pair.Key, pair => (string)pair.Value);
            }
            catch (InvalidCastException exCast)
            {
                throw new AppleICloudClientException(Properties.Resources.MessageMalformedResponse, exCast);
            }
            catch (WebException exWeb)
            {
                throw new AppleICloudClientException(Properties.Resources.MessageErrorInitializingConfiguration, exWeb);
            }
        }

        public void SignIn(string appleId, string password)
        {
            try
            {
                var url = this.actionUrls["loginOrCreateAccount"];
                if (url == null)
                {
                    throw new AppleICloudClientException(Properties.Resources.MessageActionUrlUnknown);
                }

                var responseObj = (IDictionary<string, object>)GetPList(url, true,
                    new NetworkCredential(appleId, password));

                var protocolVersion = (string)responseObj["protocolVersion"];
                if (!supportedProtocolVersions.Contains(protocolVersion))
                {
                    throw new AppleICloudClientException(Properties.Resources.MessageUnsupportedProtocolVersion);
                }

                var appleAccountInfo = (IDictionary<string, object>)responseObj["appleAccountInfo"];
                this.AccountInfo = new AppleAccountInfo()
                {
                    Id = (string)appleAccountInfo["dsPrsID"],
                    AppleId = (string)appleAccountInfo["appleId"],
                    Status = (int)appleAccountInfo["statusCode"],
                    IsLocked = (bool)appleAccountInfo["locked"],
                    AppleIdAliases = ((Array)appleAccountInfo["appleIdAliases"]).Convert<string>(),
                    PrimaryEmailVerified = (bool)appleAccountInfo["primaryEmailVerified"],
                    LastName = (string)appleAccountInfo["lastName"],
                    FullName = (string)appleAccountInfo["fullName"],
                };

                // TODO: finish
            }
            catch (InvalidCastException exCast)
            {
                throw new AppleICloudClientException(Properties.Resources.MessageMalformedResponse, exCast);
            }
            catch (WebException exWeb)
            {
                throw new AppleICloudClientException(Properties.Resources.MessageErrorSigningIn, exWeb);
            }
        }

        public void SignOut()
        {
            this.cookieContainer = null;
        }

        private string GetAuthorizationHeader(NetworkCredential credentials)
        {
            Debug.Assert(credentials != null);
            return "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(
                credentials.UserName + ":" + credentials.Password));
        }

        private object GetPList(string url, bool isPost, NetworkCredential credentials = null)
        {
            using (var response = GetWebResponse(url, isPost, credentials))
            {
                Debug.Assert(
                    response.ContentType == "application/xml" ||
                    response.ContentType == "application/vnd.apple.xmlplist" ||
                    response.ContentType == "application/x-plist"
                    );
                var stream = response.GetResponseStream();
                var responseObj = this.plistSerializer.Deserialize(stream);
                Debug.Assert(responseObj != null);
                return responseObj;
            }
        }

        private HttpWebResponse GetWebResponse(string url, bool isPost, NetworkCredential credentials = null)
        {
            if (this.cookieContainer == null)
                this.cookieContainer = new CookieContainer();

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = isPost ? WebRequestMethods.Http.Post : WebRequestMethods.Http.Get;
            request.ProtocolVersion = HttpVersion.Version11;
            request.KeepAlive = true;
            request.UserAgent = this.UserAgent;
            request.AutomaticDecompression = DecompressionMethods.None;
            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (credentials != null)
            {
                request.Headers[HttpRequestHeader.Authorization] = GetAuthorizationHeader(credentials);
            }
            request.Headers[HttpRequestHeader.AcceptLanguage] = CultureInfo.CurrentCulture.Name;
            request.Headers["X-Mme-Client-Info"] = string.Format("<{0}> <{1}; {2}; {3}> <{4}>",
                "PC", "Windows", Environment.OSVersion.Version, "W", "com.apple.AOSKit/88");
            request.CookieContainer = this.cookieContainer;
            return (HttpWebResponse)request.GetResponse();
        }
    }
}