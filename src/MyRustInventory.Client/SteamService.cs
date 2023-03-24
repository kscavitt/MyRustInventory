using Microsoft.Extensions.Logging;
using MyRustInventory.Domain.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyRustInventory.Client
{
    public class SteamService : ISteamClient
    {
        #region private fields
        private readonly HttpClient _httpClient;
        private ILogger _logger;
        private string? _steamId;
        private string _fileName => "./TestData/testData.json";
        private int _gameId => 252490;
        private int _contextId => 2;
        private static JsonSerializerSettings _jsonSettings => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
        #endregion

        #region Constructor

       
        public SteamService(HttpClient httpClient, ILogger<SteamService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.BaseAddress = new Uri("https://steamcommunity.com");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Chrome");
            _logger.LogInformation($"Loaded {nameof(SteamService)}");
        }

        #endregion

        #region Public Interface Implementations

        /// <summary>
        /// Get a players rust inventory
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<RustItemsResponse> GetInventory(string steamId)
        {

            RustItemsRawResponse raw = new();
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
            RustItemsResponse inventory = new();
            // process the raw response
            if (raw != null && raw.Assets != null && raw.Descriptions != null)
            {
                string imageUrl = "https://steamcommunity-a.akamaihd.net/economy/image";
                // join on the ClassId and build a rustItemDto List
                var res = raw.Assets.Join(raw.Descriptions, a => a.Classid, b => b.Classid, (a, b) =>
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
                         Amount = a.Amount,
                         Marketable = b.Marketable,
                         Description = b.Descriptions == null || b.Descriptions.Count <= 0 ? "" : b.Descriptions.FirstOrDefault().Value,
                         Tradable = b.Tradable,
                         Type = b.Type,
                         Commodity = b.Commodity,
                         MarketMarketableRestriction = b.Market_Marketable_Restriction,
                         MarketTradableRestriction = b.Market_Tradable_Restriction,
                         Assetid = a.Assetid,
                         MarketHashName = b.Market_Hash_Name
                     }).ToList();
                inventory.InventoryItems = res;
                return inventory;
            }

            return inventory;
        }

        public Task<string> GetInventoryItemImage(string imageId)
        {
            throw new NotImplementedException();
        }

        public async Task<MarketDataResponse> GetMarketData(string mhn, int currency = 1)
        {
            MarketDataResponse resp = new();

#if DEBUG

            Random random = new Random();
            double moneyvalue = Math.Round((random.NextDouble() / 100), 3);
            string currencyValue = moneyvalue.ToString("C");
            _logger.LogInformation($"SteamService::GetMarketData:: Generating Random currency: {_fileName}.");
            resp.Lowest_Price = currencyValue;
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

        #endregion

        #region Helpers


        /// <summary>
        /// Deserialize json to Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private static T DeserializeObject<T>(string? request) where T : class {

            if (request == null)
            {
                throw new Exception("");
            }

            return JsonConvert.DeserializeObject<T>(request); //System.Text.Json.JsonSerializer.Deserialize<T>(request);
        }

        /// <summary>
        /// Serialize an Object to json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static string SerializeObject<T>(T request, JsonSerializerSettings settings = null) where T : class
        {

            if(settings == null)
                return JsonConvert.SerializeObject(request, settings);

            return JsonConvert.SerializeObject(request, _jsonSettings);
        }




        #endregion

    }
}
