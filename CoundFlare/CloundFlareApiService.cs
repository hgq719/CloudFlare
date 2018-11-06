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
        Task<List<CloudflareLog>> GetCloudflareLogsAsync(DateTime start, DateTime end);
        List<CloudflareLog> GetCloudflareLogs(DateTime start, DateTime end, out bool retry);
        List<FirewallAccessRule> GetAccessRuleList(string ip, string notes);
        /// <summary>
        /// valid values: block, challenge, whitelist, js_challenge
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        FirewallAccessRuleResponse CreateAccessRule(FirewallAccessRuleRequest request);
        FirewallAccessRuleResponse EditAccessRule(string id, FirewallAccessRuleRequest request);
        FirewallAccessRuleResponse DeleteAccessRule(string id);
        List<RateLimitRule> GetRateLimitRuleList();
        CreateRateLimitResponse CreateRateLimit(RateLimitRule rateLimitRule);
        UpdateRateLimitResponse UpdateRateLimit(RateLimitRule rateLimitRule);
        DeleteRateLimitResponse DeleteRateLimit(string id);
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
        public List<FirewallAccessRule> GetAccessRuleList(string ip, string notes)
        {
            List<FirewallAccessRule> firewallAccessRules = new List<FirewallAccessRule>();
            int page = 1;
            while (true)
            {
                //?page={1}&per_page={2}&notes=my note
                string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?page={1}&per_page=500&configuration.value={2}";
                url = string.Format(url, zoneId, page, ip);
                if (!string.IsNullOrEmpty(notes))
                {
                    url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?page={1}&per_page=500&notes={2}";
                    url = string.Format(url, zoneId, page, notes);
                }

                string content = HttpGet(url, 1200);
                FirewallAccessRuleResponseList firewallAccessRuleResponseList = JsonConvert.DeserializeObject<FirewallAccessRuleResponseList>(content);
                if (firewallAccessRuleResponseList.success)
                {
                    foreach (CreateResult result in firewallAccessRuleResponseList.result)
                    {
                        firewallAccessRules.Add(new FirewallAccessRule
                        {
                            id = result.id,
                            notes = result.notes,
                            mode = result.mode,
                            configurationTarget = result.configuration.target,
                            configurationValue = result.configuration.value,
                            createTime = result.created_on,
                            modifiedTime = result.modified_on,
                            scopeId = result.scope.id,
                            scopeEmail = result.scope.email,
                            scopeType = result.scope.type,
                        });
                    }
                    
                    if(firewallAccessRuleResponseList.result_info.page >= firewallAccessRuleResponseList.result_info.total_pages)
                    {
                        break;
                    }
                    else
                    {
                        page++;
                    }
                }
                else
                {
                    break;
                }
            }            
            return firewallAccessRules;
        }
        public FirewallAccessRuleResponse CreateAccessRule(FirewallAccessRuleRequest request)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(request);
            string content = HttpPost(url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public FirewallAccessRuleResponse EditAccessRule(string id, FirewallAccessRuleRequest request)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, zoneId, id);
            string json = JsonConvert.SerializeObject(request);
            string content = HttpPut(url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public FirewallAccessRuleResponse DeleteAccessRule(string id)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, zoneId, id);
            string json = JsonConvert.SerializeObject(new { cascade = "none" });
            string content = HttpDelete(url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public List<RateLimitRule> GetRateLimitRuleList()
        {
            List<RateLimitRule> rateLimitRules = new List<RateLimitRule>();
            int page = 1;
            while (true)
            {
                //?page={1}&per_page={2}&notes=my note
                string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits?page={1}&per_page=20";
                url = string.Format(url, zoneId, page);               
                string content = HttpGet(url, 1200);
                RateLimitResponse rateLimitResponse = JsonConvert.DeserializeObject<RateLimitResponse>(content);
                if (rateLimitResponse.success)
                {
                    rateLimitRules.AddRange(rateLimitResponse.result);
                    if (rateLimitResponse.result_info.page >= rateLimitResponse.result_info.total_pages)
                    {
                        break;
                    }
                    else
                    {
                        page++;
                    }
                }
                else
                {
                    break;
                }
            }
            return rateLimitRules;
        }
        public CreateRateLimitResponse CreateRateLimit(RateLimitRule rateLimitRule)
        {
            CreateRateLimitResponse createRateLimitResponse = new CreateRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPost(url, json, 90);
            createRateLimitResponse = JsonConvert.DeserializeObject<CreateRateLimitResponse>(content);
            return createRateLimitResponse;
        }
        public UpdateRateLimitResponse UpdateRateLimit(RateLimitRule rateLimitRule)
        {
            UpdateRateLimitResponse updateRateLimitResponse = new UpdateRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPut(url, json, 90);
            updateRateLimitResponse = JsonConvert.DeserializeObject<UpdateRateLimitResponse>(content);
            return updateRateLimitResponse;
        }
        public DeleteRateLimitResponse DeleteRateLimit(string id)
        {
            DeleteRateLimitResponse deleteRateLimitResponse = new DeleteRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits/{1}";
            url = string.Format(url, zoneId, id);
            string json = "";
            string content = HttpDelete(url, json, 90);
            deleteRateLimitResponse = JsonConvert.DeserializeObject<DeleteRateLimitResponse>(content);
            return deleteRateLimitResponse;
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
        private string HttpPut(string url, string json, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.PutAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private string HttpDelete(string url, string json, int timeout = 90)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            if (!string.IsNullOrWhiteSpace(json))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private string GetUTCTimeString(DateTime time)
        {
            return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
        }
    }
}
