using Castle.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.CoundFlare
{
    public interface ICloundFlareApiService
    {
       Task< List<CloudflareLog> > GetCloudflareLogsAsync(DateTime start, DateTime end);
       List<CloudflareLog> GetCloudflareLogs(DateTime start, DateTime end, out bool retry);
    }
    public class CloundFlareApiService : ICloundFlareApiService
    {
        private string zoneId = "xxx";
        private string authEmail = "xx@xx.com";
        private string authKey = "xxxyyy";
        private double sample = 0.1;
        private int timeSpan = 5;
        private ILogger logger = Abp.Logging.LogHelper.Logger;

        public CloundFlareApiService()
        {
            zoneId = Convert.ToString(ConfigurationManager.AppSettings["zoneId"]);
            authEmail = Convert.ToString(ConfigurationManager.AppSettings["authEmail"]);
            authKey = Convert.ToString(ConfigurationManager.AppSettings["authKey"]);
            sample = Convert.ToDouble(ConfigurationManager.AppSettings["sample"]);
            timeSpan = Convert.ToInt32(ConfigurationManager.AppSettings["timeSpan"]);
        }
        public List<CloudflareLog> GetCloudflareLogs(DateTime start, DateTime end, out bool retry)
        {
            retry = false;
            List<CloudflareLog> CloudflareLogs = new List<CloudflareLog>();
            string fields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestURI,EdgeEndTimestamp,EdgeResponseBytes,EdgeResponseStatus,EdgeStartTimestamp,CacheResponseStatus,ClientRequestBytes,CacheCacheStatus,OriginResponseStatus,OriginResponseTime";
            string startTime = GetUTCTimeString(start);
            string endTime = GetUTCTimeString(end);
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
            url = string.Format(url, zoneId, startTime, endTime, fields, sample);
            string content = HttpGet(url, 1200);
            if (content.Contains("\"}"))
            {
                content = content.Replace("\"}", "\"},");
                CloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>(string.Format("[{0}]", content));
            }
            else
            {
                if(content.Contains("429 Too Many Requests"))
                {
                    retry = true;
                }
                logger.Error(content);
            }
            return CloudflareLogs;
        }
        public async Task<List<CloudflareLog>> GetCloudflareLogsAsync(DateTime start, DateTime end)
        {
            List<CloudflareLog> CloudflareLogs = new List<CloudflareLog>();
            string fields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestURI,EdgeEndTimestamp,EdgeResponseBytes,EdgeResponseStatus,EdgeStartTimestamp,CacheResponseStatus,ClientRequestBytes,CacheCacheStatus,OriginResponseStatus,OriginResponseTime";
            string startTime = GetUTCTimeString(start);
            string endTime = GetUTCTimeString(end);
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
            url = string.Format(url, zoneId, startTime, endTime, fields, sample);
            string content = await HttpGetAsyc(url, 1200);
            if (content.Contains("\"}"))
            {
                content = content.Replace("\"}", "\"},");
                CloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>(string.Format("[{0}]", content));
            }
            return CloudflareLogs;
        }
        private string HttpGet(string url, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private async Task<string> HttpGetAsyc(string url, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                HttpResponseMessage httpResponseMessage = await client.GetAsync(url);
                string strResult = await httpResponseMessage.Content.ReadAsStringAsync();
                return strResult;
            }
        }
        private string HttpPost(string url, string json, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.PostAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private string GetUTCTimeString(DateTime time)
        {
            return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
        }
    }
}
