using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using SharpTrooper.Entities;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace SharpTrooper.Core
{
    // =========================================================================
    // SECURITY + BUG-FIX NOTES (2026)
    // =========================================================================
    // 1. The API moved. "http://swapi.co/api" shut down years ago, so EVERY
    //    request failed and the program crashed on the first question. The
    //    default is now the community-run replacement "https://swapi.dev/api".
    //    If that site is ever down, set the SWAPI_BASE_URL environment variable
    //    to another mirror (e.g. https://swapi.py4e.com/api) - no code change.
    //
    // 2. HTTPS instead of HTTP, and TLS 1.2 is switched on explicitly. Older
    //    .NET Framework versions don't always enable TLS 1.2 by themselves, and
    //    most HTTPS sites refuse anything older.
    //
    // 3. Ids and query values are URL-encoded before being glued onto the URL.
    //    Before, typing something like  ../../other?x=  as your "favorite
    //    number" changed which address the program called (URL injection).
    //
    // 4. Web responses are disposed with `using` blocks so network connections
    //    are released even when something goes wrong (the old code leaked them
    //    on errors).
    //
    // 5. Newtonsoft.Json was updated 6.0.7 -> 13.0.3 (CVE-2024-21907,
    //    denial of service via deeply nested JSON). See packages.config.
    //    We also cap JSON nesting depth here for extra safety.
    //
    // 6. Removed a leftover stub, GetSingleByUrl<T>(object p), that only threw
    //    NotImplementedException and could be called by mistake.
    //
    // These are the droids you're looking for.
    // =========================================================================
    public class SharpTrooperCore
    {
        private enum HttpMethod
        {
            GET,
            POST
        }

        private const string DefaultApiUrl = "https://swapi.dev/api";

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            MaxDepth = 64
        };

        private readonly string apiUrl;
        private string _proxyName = null;

        static SharpTrooperCore()
        {
            // Make sure TLS 1.2 is allowed for HTTPS calls (keeps whatever else the OS already allows).
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public SharpTrooperCore()
        {
            string fromEnvironment = Environment.GetEnvironmentVariable("SWAPI_BASE_URL");
            apiUrl = string.IsNullOrWhiteSpace(fromEnvironment) ? DefaultApiUrl : fromEnvironment.TrimEnd('/');
        }

        public SharpTrooperCore(string proxyName) : this()
        {
            _proxyName = proxyName;
        }

        #region Private

        private string Request(string url, HttpMethod httpMethod)
        {
            return Request(url, httpMethod, null, false);
        }

        private string Request(string url, HttpMethod httpMethod, string data, bool isProxyEnabled)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = httpMethod.ToString();
            httpWebRequest.UserAgent = "StarWarsCSharpProject/2.0";
            httpWebRequest.Timeout = 15000; // 15 seconds - don't wait a galaxy far, far away

            if (!String.IsNullOrEmpty(_proxyName))
            {
                httpWebRequest.Proxy = new WebProxy(_proxyName, 80);
                httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            }

            if (data != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                httpWebRequest.ContentLength = bytes.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            using (StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        private string SerializeDictionary(Dictionary<string, string> dictionary)
        {
            // URL-encode every key and value so special characters can't break the query string.
            return string.Join("&", dictionary.Select(pair =>
                Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(pair.Value ?? string.Empty)));
        }

        // Turns user input like " 4 " into "4", and anything unsafe into an error
        // instead of letting it sneak into the URL.
        private static string CleanId(string id)
        {
            string trimmed = (id ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("An id is required.", nameof(id));
            }
            return Uri.EscapeDataString(trimmed);
        }

        private T GetSingle<T>(string endpoint, Dictionary<string, string> parameters = null) where T : SharpEntity
        {
            string serializedParameters = "";
            if (parameters != null)
            {
                serializedParameters = "?" + SerializeDictionary(parameters);
            }

            return GetSingleByUrl<T>(url: string.Format("{0}{1}{2}", apiUrl, endpoint, serializedParameters));
        }

        private SharpEntityResults<T> GetMultiple<T>(string endpoint) where T : SharpEntity
        {
            return GetMultiple<T>(endpoint, null);
        }

        private SharpEntityResults<T> GetMultiple<T>(string endpoint, Dictionary<string, string> parameters) where T : SharpEntity
        {
            string serializedParameters = "";
            if (parameters != null)
            {
                serializedParameters = "?" + SerializeDictionary(parameters);
            }

            string json = Request(string.Format("{0}{1}{2}", apiUrl, endpoint, serializedParameters), HttpMethod.GET);
            SharpEntityResults<T> swapiResponse = JsonConvert.DeserializeObject<SharpEntityResults<T>>(json, JsonSettings);
            return swapiResponse;
        }

        private NameValueCollection GetQueryParameters(string dataWithQuery)
        {
            NameValueCollection result = new NameValueCollection();
            string[] parts = dataWithQuery.Split('?');
            if (parts.Length > 0)
            {
                string QueryParameter = parts.Length > 1 ? parts[1] : parts[0];
                if (!string.IsNullOrEmpty(QueryParameter))
                {
                    string[] p = QueryParameter.Split('&');
                    foreach (string s in p)
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            // BUG FIX (2026): Split('=', 2) keeps values that themselves contain "=".
                            string[] temp = s.Split(new[] { '=' }, 2);
                            result.Add(Uri.UnescapeDataString(temp[0]), Uri.UnescapeDataString(temp[1]));
                        }
                        else
                        {
                            result.Add(s, string.Empty);
                        }
                    }
                }
            }
            return result;
        }

        private SharpEntityResults<T> GetAllPaginated<T>(string entityName, string pageNumber = "1") where T : SharpEntity
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("page", pageNumber);

            SharpEntityResults<T> result = GetMultiple<T>(entityName, parameters);

            result.nextPageNo = String.IsNullOrEmpty(result.next) ? null : GetQueryParameters(result.next)["page"];
            result.previousPageNo = String.IsNullOrEmpty(result.previous) ? null : GetQueryParameters(result.previous)["page"];

            return result;
        }

        #endregion

        #region Public

        /// <summary>
        /// get a specific resource by url
        /// </summary>
        public T GetSingleByUrl<T>(string url) where T : SharpEntity
        {
            string json = Request(url, HttpMethod.GET);
            T swapiResponse = JsonConvert.DeserializeObject<T>(json, JsonSettings);
            return swapiResponse;
        }

        // People
        /// <summary>
        /// get a specific people resource
        /// </summary>
        public People GetPeople(string id)
        {
            return GetSingle<People>("/people/" + CleanId(id) + "/");
        }

        /// <summary>
        /// get all the people resources
        /// </summary>
        public SharpEntityResults<People> GetAllPeople(string pageNumber = "1")
        {
            SharpEntityResults<People> result = GetAllPaginated<People>("/people/", pageNumber);

            return result;
        }

        // Film
        /// <summary>
        /// get a specific film resource
        /// </summary>
        public film GetFilm(string id)
        {
            return GetSingle<film>("/films/" + CleanId(id) + "/");
        }

        /// <summary>
        /// get all the film resources
        /// </summary>
        public SharpEntityResults<film> GetAllFilms(string pageNumber = "1")
        {
            SharpEntityResults<film> result = GetAllPaginated<film>("/films/", pageNumber);

            return result;
        }

        // Planet
        /// <summary>
        /// get a specific planet resource
        /// </summary>
        public Planet GetPlanet(string id)
        {
            return GetSingle<Planet>("/planets/" + CleanId(id) + "/");
        }

        /// <summary>
        /// get all the planet resources
        /// </summary>
        public SharpEntityResults<Planet> GetAllPlanets(string pageNumber = "1")
        {
            SharpEntityResults<Planet> result = GetAllPaginated<Planet>("/planets/", pageNumber);

            return result;
        }

        // Specie
        /// <summary>
        /// get a specific specie resource
        /// </summary>
        public Specie GetSpecie(string id)
        {
            return GetSingle<Specie>("/species/" + CleanId(id) + "/");
        }

        /// <summary>
        /// get all the specie resources
        /// </summary>
        public SharpEntityResults<Specie> GetAllSpecies(string pageNumber = "1")
        {
            SharpEntityResults<Specie> result = GetAllPaginated<Specie>("/species/", pageNumber);

            return result;
        }

        // Starship
        /// <summary>
        /// get a specific starship resource
        /// </summary>
        public Starship GetStarship(string id)
        {
            return GetSingle<Starship>("/starships/" + CleanId(id) + "/");
        }

        /// <summary>
        /// get all the starship resources
        /// </summary>
        public SharpEntityResults<Starship> GetAllStarships(string pageNumber = "1")
        {
            SharpEntityResults<Starship> result = GetAllPaginated<Starship>("/starships/", pageNumber);

            return result;
        }

        // Vehicle
        /// <summary>
        /// get a specific vehicle resource
        /// </summary>
        public Vehicle GetVehicle(string id)
        {
            return GetSingle<Vehicle>("/vehicles/" + CleanId(id) + "/");
        }

        /// <summary>
        /// get all the vehicle resources
        /// </summary>
        public SharpEntityResults<Vehicle> GetAllVehicles(string pageNumber = "1")
        {
            SharpEntityResults<Vehicle> result = GetAllPaginated<Vehicle>("/vehicles/", pageNumber);

            return result;
        }

        #endregion
    }
}
