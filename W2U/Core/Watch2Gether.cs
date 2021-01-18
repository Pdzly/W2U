using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Caching;
using Newtonsoft.Json.Linq;
using W2U.Templates;


namespace W2U.Core
{
    public class Watch2Gether
    {
        public readonly JObject response;
        public readonly Watch2GetherData data;


        MemoryCache cache = new MemoryCache("w2gcache");

        private bool isW2Gonline()
        {
            CacheItemPolicy policy = new CacheItemPolicy {AbsoluteExpiration = DateTime.Now.AddMinutes(5)};
            bool isonline = pingw2g();
            cache.AddOrGetExisting("w2g", isonline, policy);
            return (bool)cache.Get("w2g");
            
            
        }

        private bool pingw2g()
        {
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send("w2g.tv", 1000);

            if (reply.Status == IPStatus.Success)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Creates the class with the given params
        /// </summary>
        /// <param name="apikey">The API Key from <see cref="https://w2g.tv/auth/sign_up"/></param>
        /// <param name="videotoshare">Optional Video to play at the start of the room</param>
        /// <param name="color">Optional Background Color of the room in html notation</param>
        /// <param name="opacity">Optional Opacity of the rooms background from 0 to 100</param>
        /// <exception cref="Watch2GetherException">Throws if something goes wrong or your api key is wrong</exception>
        public Watch2Gether(string apikey, string videotoshare = "https://www.youtube.com/watch?v=tpiyEe_CqB4", string color = "#00ff00", int opacity = 50)
        {
            try
            {
                if (!isW2Gonline()) throw new Watch2GetherException("Watch2Gether is offline or not reachable");
                var httpWebRequest = (HttpWebRequest) WebRequest.Create("https://w2g.tv/rooms/create.json");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json =
                        $"{{ \r\n    \"w2g_api_key\" : \"{apikey}\",\r\n    \"share\" : \"{videotoshare}\",\r\n    \"bg_color\" : \"{color}\",\r\n    \"bg_opacity\" : \"{opacity}\"\r\n}}";

                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                string result = null;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ??
                                                           throw new Watch2GetherException("Response Stream was null")))
                {
                    result = streamReader.ReadToEnd();
                }

                if (result == null) throw new Watch2GetherException("The response from the server was empty!");
                response = JObject.Parse(result);
                data = new Watch2GetherData(response["streamkey"].Value<string>(), response["location"].Value<string>(),
                    response["created_at"].Value<DateTime>());
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }
    }

    public class Watch2GetherData
    {
        public readonly string streamkey;
        public readonly string region;
        public readonly DateTime createdTime;

        public Watch2GetherData(string streamkey, string region, DateTime createdTime)
        {
            this.streamkey = streamkey;
            this.region = region;
            this.createdTime = createdTime;
        }
    }

}
