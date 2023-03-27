using Microsoft.Extensions.Logging;
using MyRustInventory.Application.Common.Interfaces;
using MyRustInventory.Domain.Common;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Web;

namespace MyRustInventory.Infrastructure.Services
{
    public class SteamService : ISteamClient
    {
        #region private fields
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private string? _steamId;
        private string _fileName => "./TestData/testData.json";
        private int _gameId => 252490;
        private int _contextId => 2;
        /// <summary>
        /// CookieContainer to save all cookies during the Login. 
        /// </summary>
        private CookieContainer _cookies = new CookieContainer();
        /// <summary>
        /// The Accept-Language header when sending all HTTP requests. Default value is determined according to the constructor caller thread's culture.
        /// </summary>
        public string AcceptLanguageHeader { get { return acceptLanguageHeader; } set { acceptLanguageHeader = value; } }
        private string acceptLanguageHeader = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "en" ? Thread.CurrentThread.CurrentCulture.ToString() + ",en;q=0.8" : Thread.CurrentThread.CurrentCulture.ToString() + "," + Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName + ";q=0.8,en;q=0.6";

        private static JsonSerializerOptions _jsonSettings => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        private readonly ICacheService _cacheService;

        #endregion

        #region Constructor


        public SteamService(HttpClient httpClient, ILogger<SteamService> logger, ICacheService cacheService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.BaseAddress = new Uri("https://steamcommunity.com");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Chrome");
            _logger.LogInformation($"Loaded {nameof(SteamService)}");
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        #endregion

        #region Public Interface Implementations

        /// <summary>
        /// Get a players rust inventory
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<RustItemDto>> GetInventory(string steamId)
        {

            RustItemsRawResponse raw = new();
            List<RustItemDto> results = new();
#if DEBUG
            _steamId = "76561198012083287";
            raw = DeserializeObject<RustItemsRawResponse>(File.ReadAllText(_fileName)); 
            _logger.LogInformation($"SteamService::GetInventory:: Getting Debug Data from file: {_fileName}.");

#else
            string url = $"inventory/{steamId}/{_gameId}/{_contextId}";
            _logger.LogInformation($"Getting Prod Data from url: {url}.");
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stringData = await response.Content.ReadAsStringAsync();

                raw = DeserializeObject<RustItemsRawResponse>(stringData);
            }
#endif
            // process the raw response
            if (raw != null && raw.Assets != null && raw.Descriptions != null)
            {
                string imageUrl = "https://steamcommunity-a.akamaihd.net/economy/image";
                // join on the ClassId and build a list of rustItemDtos
                results = raw.Assets.Join(raw.Descriptions, a => a.Classid, b => b.Classid, (a, b) =>
                     new RustItemDto
                     {
                         Classid = a.Classid,
                         IconUrl = $"{imageUrl}/{b.Icon_Url}",
                         IconUrlLarge = $"{imageUrl}/{b.Icon_Url_Large}",
                         Name = b.Name,
                         MarketName = b.Market_Name,
                         Tags = b.Tags,
                         BackgroundColor = b.Background_Color,
                         NameColor = b.Name_Color,
                         Quantity = Convert.ToInt32(a.Amount),
                         Marketable = b.Marketable,
                         Description = b.Descriptions == null || b.Descriptions.Count <= 0 ? "" : b.Descriptions.FirstOrDefault()?.Value,
                         Tradable = b.Tradable,
                         Type = b.Type,
                         Commodity = b.Commodity,
                         MarketMarketableRestriction = b.Market_Marketable_Restriction,
                         MarketTradableRestriction = b.Market_Tradable_Restriction,
                         Assetid = a.Assetid,
                         MarketHashName = b.Market_Hash_Name
                     }).ToList();
                //inventory.InventoryItems = results;
                return results;
            }

            return results;
        }

        public Task<string> GetInventoryItemImage(string imageId)
        {
            throw new NotImplementedException();
        }

        public async Task<MarketDataResponse> GetMarketData(string mhn, int currency = 1)
        {
            MarketDataResponse resp = new();

#if DEBUG
            _logger.LogInformation($"SteamService::GetMarketData:: Generating Random currency: {_fileName}.");
            resp.Lowest_Price = GeneratePrice();
#else

            string url = $"market/priceoverview/?appid={_gameId}&currency={currency}&market_hash_name={mhn}";
            _logger.LogInformation($"SteamService::GetMarketData:: Gettting Prod Data from url: {url}.");
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stringData = await response.Content.ReadAsStringAsync();

                resp = DeserializeObject<MarketDataResponse>(stringData);
                return resp;
            }
#endif


            return resp;
        }
        public async Task<MarketDataResponse> GetMarketData(string mhn, string assetId, int currency = 1)
        {
            MarketDataResponse marketData = await GetMarketData(mhn, currency);
            marketData.AssetId = assetId;
            return marketData;
        }


        /// <summary>
        /// Executes the login by using the Steam Website.
        /// This Method is not used by Steambot repository, but it could be very helpful if you want to build a own Steambot or want to login into steam services like backpack.tf/csgolounge.com.
        /// Updated: 10-02-2015.
        /// </summary>
        /// <param name="username">Your Steam username.</param>
        /// <param name="password">Your Steam password.</param>
        /// <returns>A bool containing a value, if the login was successful.</returns>
        public bool DoLogin(string username, string password)
        {
            var data = new NameValueCollection { { "username", username } };
            // First get the RSA key with which we will encrypt our password.
            string response = Fetch("https://steamcommunity.com/login/getrsakey", "POST", data, false);
            GetRsaKey rsaJson = JsonConvert.DeserializeObject<GetRsaKey>(response);

            // Validate, if we could get the rsa key.
            if (!rsaJson.Success)
            {
                return false;
            }

            // RSA Encryption.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters rsaParameters = new RSAParameters
            {
                Exponent = HexToByte(rsaJson.Publickey_exp),
                Modulus = HexToByte(rsaJson.Publickey_mod)
            };

            rsa.ImportParameters(rsaParameters);

            // Encrypt the password and convert it.
            byte[] bytePassword = Encoding.ASCII.GetBytes(password);
            byte[] encodedPassword = rsa.Encrypt(bytePassword, false);
            string encryptedBase64Password = Convert.ToBase64String(encodedPassword);

            SteamResult loginJson = null;
            CookieCollection cookieCollection;
            string steamGuardText = "";
            string steamGuardId = "";

            // Do this while we need a captcha or need email authentification. Probably you have misstyped the captcha or the SteamGaurd code if this comes multiple times.
            do
            {
                Console.WriteLine("SteamWeb: Logging In...");

                bool captcha = loginJson != null && loginJson.Captcha_needed;
                bool steamGuard = loginJson != null && loginJson.Emailauth_needed;

                string time = Uri.EscapeDataString(rsaJson.Timestamp);

                string capGid = string.Empty;
                // Response does not need to send if captcha is needed or not.
                // ReSharper disable once MergeSequentialChecks
                if (loginJson != null && loginJson.Captcha_gid != null)
                {
                    capGid = Uri.EscapeDataString(loginJson.Captcha_gid);
                }

                data = new NameValueCollection { { "password", encryptedBase64Password }, { "username", username } };

                // Captcha Check.
                string capText = "";
                if (captcha)
                {
                    Console.WriteLine("SteamWeb: Captcha is needed.");
                    System.Diagnostics.Process.Start("https://steamcommunity.com/public/captcha.php?gid=" + loginJson.Captcha_gid);
                    Console.WriteLine("SteamWeb: Type the captcha:");
                    string consoleText = Console.ReadLine();
                    if (!string.IsNullOrEmpty(consoleText))
                    {
                        capText = Uri.EscapeDataString(consoleText);
                    }
                }

                data.Add("captchagid", captcha ? capGid : "");
                data.Add("captcha_text", captcha ? capText : "");
                // Captcha end.
                // Added Header for two factor code.
                data.Add("twofactorcode", "");

                // Added Header for remember login. It can also set to true.
                data.Add("remember_login", "false");

                // SteamGuard check. If SteamGuard is enabled you need to enter it. Care probably you need to wait 7 days to trade.
                // For further information about SteamGuard see: https://support.steampowered.com/kb_article.php?ref=4020-ALZM-5519&l=english.
                if (steamGuard)
                {
                    Console.WriteLine("SteamWeb: SteamGuard is needed.");
                    Console.WriteLine("SteamWeb: Type the code:");
                    string consoleText = Console.ReadLine();
                    if (!string.IsNullOrEmpty(consoleText))
                    {
                        steamGuardText = Uri.EscapeDataString(consoleText);
                    }
                    steamGuardId = loginJson.Emailsteamid;

                    // Adding the machine name to the NameValueCollection, because it is requested by steam.
                    Console.WriteLine("SteamWeb: Type your machine name:");
                    consoleText = Console.ReadLine();
                    var machineName = string.IsNullOrEmpty(consoleText) ? "" : Uri.EscapeDataString(consoleText);
                    data.Add("loginfriendlyname", machineName != "" ? machineName : "defaultSteamBotMachine");
                }

                data.Add("emailauth", steamGuardText);
                data.Add("emailsteamid", steamGuardId);
                // SteamGuard end.

                // Added unixTimestamp. It is included in the request normally.
                var unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                // Added three "0"'s because Steam has a weird unix timestamp interpretation.
                data.Add("donotcache", unixTimestamp + "000");

                data.Add("rsatimestamp", time);

                // Sending the actual login.
                using (HttpWebResponse webResponse = Request("https://steamcommunity.com/login/dologin/", "POST", data, false))
                {
                    var stream = webResponse.GetResponseStream();
                    if (stream == null)
                    {
                        return false;
                    }
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        loginJson = JsonConvert.DeserializeObject<SteamResult>(json);
                        cookieCollection = webResponse.Cookies;
                    }
                }
            } while (loginJson.Captcha_needed || loginJson.Emailauth_needed);

            // If the login was successful, we need to enter the cookies to steam.
            if (loginJson.Success)
            {
                _cookies = new CookieContainer();
                foreach (Cookie cookie in cookieCollection)
                {
                    _cookies.Add(cookie);
                }
                SubmitCookies(_cookies);
                return true;
            }
            else
            {
                Console.WriteLine("SteamWeb Error: " + loginJson.Message);
                return false;
            }

        }
        #endregion

        #region Helpers
        /// <summary>
        /// This method is using the Request method to return the full http stream from a web request as string.
        /// </summary>
        /// <param name="url">URL of the http request.</param>
        /// <param name="method">Gets the HTTP data transfer method (such as GET, POST, or HEAD) used by the client.</param>
        /// <param name="data">A NameValueCollection including Headers added to the request.</param>
        /// <param name="ajax">A bool to define if the http request is an ajax request.</param>
        /// <param name="referer">Gets information about the URL of the client's previous request that linked to the current URL.</param>
        /// <param name="fetchError">If true, response codes other than HTTP 200 will still be returned, rather than throwing exceptions</param>
        /// <returns>The string of the http return stream.</returns>
        /// <remarks>If you want to know how the request method works, use: <see cref="SteamWeb.Request"/></remarks>
        private string Fetch(string url, string method, NameValueCollection? data = null, bool ajax = true, string referer = "", bool fetchError = false)
        {
            // Reading the response as stream and read it to the end. After that happened return the result as string.
            using (HttpWebResponse response = Request(url, method, data, ajax, referer, fetchError))
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    // If the response stream is null it cannot be read. So return an empty string.
                    if (responseStream == null)
                    {
                        return "";
                    }
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        /// <summary>
        /// Custom wrapper for creating a HttpWebRequest, edited for Steam.
        /// </summary>
        /// <param name="url">Gets information about the URL of the current request.</param>
        /// <param name="method">Gets the HTTP data transfer method (such as GET, POST, or HEAD) used by the client.</param>
        /// <param name="data">A NameValueCollection including Headers added to the request.</param>
        /// <param name="ajax">A bool to define if the http request is an ajax request.</param>
        /// <param name="referer">Gets information about the URL of the client's previous request that linked to the current URL.</param>
        /// <param name="fetchError">Return response even if its status code is not 200</param>
        /// <returns>An instance of a HttpWebResponse object.</returns>
        private HttpWebResponse Request(string url, string method, NameValueCollection? data = null, bool ajax = true, string referer = "", bool fetchError = false)
        {
            // Append the data to the URL for GET-requests.
            bool isGetMethod = (method.ToLower() == "get");
            string dataString = (data == null ? null : String.Join("&", Array.ConvertAll(data.AllKeys, key =>
                // ReSharper disable once UseStringInterpolation
                string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))
            )));

            // Example working with C# 6
            // string dataString = (data == null ? null : String.Join("&", Array.ConvertAll(data.AllKeys, key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(data[key])}" )));

            // Append the dataString to the url if it is a GET request.
            if (isGetMethod && !string.IsNullOrEmpty(dataString))
            {
                url += (url.Contains("?") ? "&" : "?") + dataString;
            }

            // Setup the request.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Accept = "application/json, text/javascript;q=0.9, */*;q=0.5";
            request.Headers[HttpRequestHeader.AcceptLanguage] = AcceptLanguageHeader;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            // request.Host is set automatically.
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            request.Referer = string.IsNullOrEmpty(referer) ? "http://steamcommunity.com/trade/1" : referer;
            request.Timeout = 50000; // Timeout after 50 seconds.
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            // If the request is an ajax request we need to add various other Headers, defined below.
            if (ajax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("X-Prototype-Version", "1.7");
            }

            // Cookies
            request.CookieContainer = _cookies;

            // If the request is a GET request return now the response. If not go on. Because then we need to apply data to the request.
            if (isGetMethod || string.IsNullOrEmpty(dataString))
            {
                return request.GetResponse() as HttpWebResponse;
            }

            // Write the data to the body for POST and other methods.
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataString);
            request.ContentLength = dataBytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            // Get the response and return it.
            try
            {
                return request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                //this is thrown if response code is not 200
                if (fetchError)
                {
                    var resp = ex.Response as HttpWebResponse;
                    if (resp != null)
                    {
                        return resp;
                    }
                }
                throw;
            }
        }
        /// <summary>
        /// Method to submit cookies to Steam after Login.
        /// </summary>
        /// <param name="cookies">Cookiecontainer which contains cookies after the login to Steam.</param>
        static void SubmitCookies(CookieContainer cookies)
        {
            HttpWebRequest w = WebRequest.Create("https://steamcommunity.com/") as HttpWebRequest;

            // Check, if the request is null.
            if (w == null)
            {
                return;
            }
            w.Method = "POST";
            w.ContentType = "application/x-www-form-urlencoded";
            w.CookieContainer = cookies;
            // Added content-length because it is required.
            w.ContentLength = 0;
            w.GetResponse().Close();
        }

        /// <summary>
        /// Deserialize json to Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private static T DeserializeObject<T>(string? request) where T : class {

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return JsonConvert.DeserializeObject<T>(request);//JsonSerializer.Deserialize<T>(request);
        }

        /// <summary>
        /// Serialize an Object to json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static string SerializeObject<T>(T request, JsonSerializerOptions? settings = null) where T : class
        {

            if(settings == null)
                return System.Text.Json.JsonSerializer.Serialize(request, _jsonSettings);

            return System.Text.Json.JsonSerializer.Serialize(request,settings);
        }



        private decimal GeneratePrice()
        {
            Random random = new Random();
            return  (decimal)random.Next(1, 1000) / 100;
        }
        /// <summary>
        /// Method to convert a Hex to a byte.
        /// </summary>
        /// <param name="hex">Input parameter as string.</param>
        /// <returns>The byte value.</returns>
        private byte[] HexToByte(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                throw new Exception("The binary key cannot have an odd number of digits");
            }

            byte[] arr = new byte[hex.Length >> 1];
            int l = hex.Length;

            for (int i = 0; i < (l >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        /// <summary>
        /// Get the Hex value as int out of an char.
        /// </summary>
        /// <param name="hex">Input parameter.</param>
        /// <returns>A Hex Value as int.</returns>
        private int GetHexVal(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }

        /// <summary>
        /// Method to allow all certificates.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="policyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>Always true to accept all certificates.</returns>
        public bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        /// <summary>
        /// Class to Deserialize the json response strings of the getResKey request. See: <see cref="SteamWeb.DoLogin"/>
        /// </summary>
        internal class GetRsaKey
        {
            public bool Success { get; set; }

            public string? Publickey_mod { get; set; }

            public string? Publickey_exp { get; set; }

            public string? Timestamp { get; set; }
        }

        // Examples:
        // For not accepted SteamResult:
        // {"success":false,"requires_twofactor":false,"message":"","emailauth_needed":true,"emaildomain":"gmail.com","emailsteamid":"7656119824534XXXX"}
        // For accepted SteamResult:
        // {"success":true,"requires_twofactor":false,"login_complete":true,"transfer_url":"https:\/\/store.steampowered.com\/login\/transfer","transfer_parameters":{"steamid":"7656119824534XXXX","token":"XXXXC39589A9XXXXCB60D651EFXXXX85578AXXXX","auth":"XXXXf1d9683eXXXXc76bdc1888XXXX29","remember_login":false,"webcookie":"XXXX4C33779A4265EXXXXC039D3512DA6B889D2F","token_secure":"XXXX63F43AA2CXXXXC703441A312E1B14AC2XXXX"}}

        /// <summary>
        /// Class to Deserialize the json response strings after the login. See: <see cref="SteamWeb.DoLogin"/>
        /// </summary>
        internal class SteamResult
        {
            public bool Success { get; set; }

            public string? Message { get; set; }

            public bool Captcha_needed { get; set; }

            public string? Captcha_gid { get; set; }

            public bool Emailauth_needed { get; set; }

            public string? Emailsteamid { get; set; }
        }
        #endregion
    }
}
