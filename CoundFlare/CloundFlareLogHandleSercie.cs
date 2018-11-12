using Castle.Core.Logging;
using CoundFlareTools.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoundFlareTools.CoundFlare
{
    public interface ICloudflareLogHandleSercie: INotificationService
    {
        //产生队列数据
        void InitQueue(DateTime startTime, DateTime endTime);
        //获取实时报表
        CloudflareLogReport GetCloudflareLogReport();
        //查询区间报表
        CloudflareLogReport GetCloudflareLogReport(DateTime start, DateTime end);
        //取队列数据处理
        void Dequeue();
        //上报处理结果
        void PushReport(CloudflareLogReport CloudflareLogReport);
        //开启任务
        void TaskStart();
        /// <summary>
        /// 禁止Ip
        /// </summary>
        /// <param name="ips"></param>
        void BanIps(List<string> ips, string comment);
        void WhitelistIps(List<string> ips, string comment);
        List<CloudflareLog> GetCloudflareLogs();
    }
    public class CloudflareLogHandleSercie : NotificationService, ICloudflareLogHandleSercie
    {
        private ConcurrentBag<List<CloudflareLog>> concurrentBagCloudflareLogs=new ConcurrentBag<List<CloudflareLog>>();
        private ConcurrentBag<CloudflareLogReport> cloudflareLogReports;
        private ConcurrentQueue<KeyValuePair<DateTime, DateTime>> keyValuePairs;
        private Config roteLimitConfig;
        private ILogger logger = Abp.Logging.LogHelper.Logger;
        private double sample = 0.01;
        private int timeSpan = 5;
        private DateTime startTime;
        private DateTime endTime;
        private int taskCount = 1;
        private bool resultSaveDb = false;
        private bool updateRateLimitForCloundflare = false;

        public ITriggerlogsAppService triggerlogsAppService { get; set; }
        public ITriggerlogdetailsAppService triggerlogdetailsAppService { get; set; }
        public ISettingsAppService settingsAppService { get; set; }

        public CloudflareLogHandleSercie()
        {
            //timeSpan = Convert.ToInt32(ConfigurationManager.AppSettings["timeSpan"]);
            //sample = Convert.ToDouble(ConfigurationManager.AppSettings["sample"]);
            //taskCount = Convert.ToInt32(ConfigurationManager.AppSettings["taskCount"]);
            //resultSaveDb = Convert.ToBoolean(ConfigurationManager.AppSettings["resultSaveDb"]);
        }

        public ICloundFlareApiService cloundFlareApiService { get; set; }
        public ILogsController logsController { get; set; }

        //产生队列数据
        public void InitQueue(DateTime startTime, DateTime endTime)
        {
            roteLimitConfig = logsController.GetLimitConfig();
            this.startTime = startTime;
            this.endTime = endTime;
            //var settings = settingsAppService.GetAll().ToDictionary(key => key.Key, value => value.Value);
            var settings = logsController.GetSettings();
            timeSpan = Convert.ToInt32(settings["timeSpan"]);
            sample = Convert.ToDouble(settings["sample"]);
            taskCount = Convert.ToInt32(settings["taskCount"]);
            resultSaveDb = Convert.ToBoolean(settings["resultSaveDb"]);
            updateRateLimitForCloundflare=Convert.ToBoolean(settings["UpdateRateLimitForCloundflare"]);

            if (keyValuePairs == null)
            {
                keyValuePairs = new ConcurrentQueue<KeyValuePair<DateTime, DateTime>>();
            }
            DateTime dateTime = startTime;
            OnMessage(new MessageEventArgs("产生队列数据开始"));
            while (true)
            {
                string time = string.Format("{0}-{1}", dateTime.ToString("yyyyMMddHHmmss"), dateTime.AddSeconds(timeSpan).ToString("yyyyMMddHHmmss"));

                if (cloudflareLogReports != null)
                {
                    if(!cloudflareLogReports.Any(a=>a.Time == time))
                    {
                        OnMessage(new MessageEventArgs(time));
                        keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dateTime, dateTime.AddSeconds(timeSpan)));
                    }
                }
                else
                {
                    OnMessage(new MessageEventArgs(time));
                    keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dateTime, dateTime.AddSeconds(timeSpan)));
                }

                dateTime = dateTime.AddSeconds(timeSpan);

                if(dateTime>= endTime)
                {
                    break;
                }
            }
            OnMessage(new MessageEventArgs("产生队列数据结束"));
            
        }
        //获取实时报表
        public CloudflareLogReport GetCloudflareLogReport()
        {
            CloudflareLogReport cloudflareLogReport = new CloudflareLogReport();
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                var orderby = cloudflareLogReports.Where(a => a.Start >= startTime && a.End <= endTime).OrderBy(a => a.Time).ToList();
                CloudflareLogReport minCloudflareLogReport = orderby?.First();
                CloudflareLogReport maxCloudflareLogReport = orderby?.Last();

                DateTime start = minCloudflareLogReport.Start;
                DateTime end = maxCloudflareLogReport.End;
                int size = orderby.Sum(a => a.Size);
                string time = string.Format("{0}-{1}", start.ToString("yyyyMMddHHmmss"), end.ToString("yyyyMMddHHmmss"));

                var cloudflareLogReportItemsMany = orderby.SelectMany(a => a.CloudflareLogReportItems).ToList();

                //合并处理
                var itemsManyGroup = cloudflareLogReportItemsMany.GroupBy(a => new { a.ClientIP, a.ClientRequestHost, a.ClientRequestURI })
                    .Select(g => new { g.Key.ClientRequestHost, g.Key.ClientIP, g.Key.ClientRequestURI, Ban = false, Count = g.Sum(c => c.Count) }).OrderByDescending(a => a.Count).ToList();

                List<IpNeedToBan> ipNeedToBans = new List<IpNeedToBan>();

                List<IpNeedToBan> ipNeedToBansHas = logsController.GetIpNeedToBans();

                var banItems = new List<CloudflareLogReportItem>();

                // 没有增加 N 校验
                //var result = (from item in itemsManyGroup
                //              from config in requestLimitConfigs
                //              where item.ClientRequestURI.ToLower().Contains( config.Url.ToLower() )
                //                    && ( ( item.Count /(float) ( (end - start).TotalSeconds * sample ) )  >= (config.LimitTimes /(float) config.Interval) )
                //              select item).OrderByDescending(a=>a.Count).ToList();

                
                var result = (from item in itemsManyGroup
                              from config in roteLimitConfig.RateLimits
                              where item.ClientRequestURI.ToLower().Contains(config.Url.ToLower())
                                    && ((item.Count / (float)((end - start).TotalSeconds * sample)) >= (config.LimitTimes * roteLimitConfig.TriggerRatio / (float)config.Interval))
                              select new { item, config.Id }).OrderByDescending(a => a.item.Count).ToList();

                List<int> handleIdList = new List<int>();

                foreach (var item in result)
                {
                    var ban = false;

                    //存储rotelimit的触发log
                    var roteLimit = roteLimitConfig.RateLimits.FirstOrDefault(a => a.Id == item.Id);
                    var ipNumber = 0;
                    var action = "None";//None/Create/Delete
                    var containRoteLimitList = result.Where(a => a.Id == item.Id).ToList();
                    ipNumber = containRoteLimitList.Count;
                    //同时有N个IP达到触发某条规则时候开启本条规则或者创建
                    if (ipNumber >= roteLimitConfig.TriggerCreateNumber)
                    {
                        action = "Create";
                        ban = true;
                    }

                    if (!handleIdList.Contains(item.Id))
                    {
                        handleIdList.Add(item.Id);

                        if (action == "Create")
                        {
                            //首先找是否已经存在rotelimit
                            var roteLimitList = cloundFlareApiService.GetRateLimitRuleList();
                            var rote = roteLimitList.FirstOrDefault(a => a.match.request.url == roteLimit.Url &&
                                a.period == roteLimit.LimitTimes &&
                                a.threshold == roteLimit.Interval);

                            if (updateRateLimitForCloundflare)
                            {
                                if (rote != null && !string.IsNullOrEmpty(rote.id))
                                {
                                    //已经存在的rote 如果是 disable = false 则开启 否则不用处理
                                    if (!rote.disabled)
                                    {
                                        rote.disabled = true;
                                        cloundFlareApiService.UpdateRateLimit(rote);
                                    }
                                }
                                else
                                {
                                    //    {
                                    //  "id": "efc79e757a5d449aa8fb43820ce30347",
                                    //  "disabled": false,
                                    //  "description": "chatserver.comm100.com/chatwindowembedded.aspx",
                                    //  "match": {
                                    //    "request": {
                                    //      "methods": [
                                    //        "_ALL_"
                                    //      ],
                                    //      "schemes": [
                                    //        "_ALL_"
                                    //      ],
                                    //      "url": "chatserver.comm100.com/chatwindowembedded.aspx"
                                    //    },
                                    //    "response": {
                                    //      "origin_traffic": true,
                                    //      "headers": [
                                    //        {
                                    //          "name": "Cf-Cache-Status",
                                    //          "op": "ne",
                                    //          "value": "HIT"
                                    //        }
                                    //      ]
                                    //    }
                                    //  },
                                    //  "login_protect": false,
                                    //  "threshold": 2,
                                    //  "period": 60,
                                    //  "action": {
                                    //    "mode": "challenge",
                                    //    "timeout": 0
                                    //  }
                                    //}
                                    cloundFlareApiService.CreateRateLimit(new RateLimitRule
                                    {
                                        disabled = true,
                                        description = roteLimit.Url,
                                        match = new Match
                                        {
                                            request = new Request
                                            {
                                                methods = new string[] { "_ALL_" },
                                                schemes = new string[] { "_ALL_" },
                                                url = roteLimit.Url,
                                            },
                                            response = new Response
                                            {
                                                origin_traffic = true,
                                                headers = new Header[] {
                                                new Header{
                                                   name="Cf-Cache-Status",
                                                   op="ne",
                                                   value="HIT",
                                                }
                                            }
                                            }
                                        },
                                        login_protect = false,
                                        threshold = roteLimit.Interval,
                                        period = roteLimit.LimitTimes,
                                        action = new RateLimitAction
                                        {
                                            mode = "challenge",
                                            timeout = 0
                                        }
                                    });
                                }
                            }

                        }

                        TriggerlogsDto triggerlogsDto = triggerlogsAppService.Create(new TriggerlogsDto
                        {
                            RequestLimitConfigId = item.Id,
                            RequestLimitConfigDetail = JsonConvert.SerializeObject(new
                            {
                                roteLimit,
                                roteLimitConfig.TriggerRatio,
                                roteLimitConfig.TriggerCreateNumber,
                                roteLimitConfig.TriggerDeleteTime,
                            }),
                            IpNumber = ipNumber,
                            TriggerTime = DateTime.Now,
                            Action = action,
                            Remark = "Add By Defense System",
                        });
                        List<TriggerlogdetailsDto> triggerlogdetailsDtos = new List<TriggerlogdetailsDto>();
                        foreach (var itm in containRoteLimitList)
                        {
                            var dto = new TriggerlogdetailsDto
                            {
                                TriggerLogId = triggerlogsDto.Id,
                                //StartTime = startTime,
                                //EndTime = endTime,
                                Size = size,
                                Sample = sample,
                                ClientIP = itm.item.ClientIP,
                                ClientRequestHost = itm.item.ClientRequestHost,
                                ClientRequestURI = itm.item.ClientRequestURI,
                                Count = itm.item.Count,

                            };
                            triggerlogdetailsDtos.Add(dto);

                            //triggerlogdetailsAppService.Create(dto);
                        }

                        //triggerlogdetailsAppService.CreateBatch(triggerlogdetailsDtos);
                        logsController.CreateTriggerlogdetailsDtoBatch(triggerlogdetailsDtos);
                    }

                    if(!banItems.Exists(a=>a.ClientIP == item.item.ClientIP
                    && a.ClientRequestHost == item.item.ClientRequestHost
                    && a.ClientRequestURI == item.item.ClientRequestURI))
                    {
                        banItems.Add(new CloudflareLogReportItem
                        {
                            ClientRequestHost = item.item.ClientRequestHost,
                            ClientIP = item.item.ClientIP,
                            ClientRequestURI = item.item.ClientRequestURI,
                            Count = item.item.Count,
                            Ban = ban
                        });
                    }                   

                }

                if (resultSaveDb == true)
                {
                    logsController.InsertResultDataBulk(ipNeedToBans);
                }

                cloudflareLogReport = new CloudflareLogReport
                {
                    Guid = Guid.NewGuid().ToString(),
                    Time = time,
                    Start = start,
                    End = end,
                    Size = size,
                    CloudflareLogReportItems = banItems.ToArray()
                };

                stopwatch.Stop();
                OnMessage(new MessageEventArgs("报表汇总用时:" + stopwatch.ElapsedMilliseconds / 1000 + "秒"));

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

            return cloudflareLogReport;
        }
        //查询区间报表
        public CloudflareLogReport GetCloudflareLogReport(DateTime start, DateTime end)
        {
            CloudflareLogReport CloudflareLogReport = new CloudflareLogReport();

            return CloudflareLogReport;
        }
        //取队列数据处理
        public void Dequeue()
        {
            try
            {
                while (true)
                {

                    //OnMessage(new MessageEventArgs("取队列数据处理开始"));
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    KeyValuePair<DateTime, DateTime> keyValuePair = default(KeyValuePair<DateTime, DateTime>);
                    if (keyValuePairs.TryDequeue(out keyValuePair))
                    {
                        DateTime start = keyValuePair.Key;
                        DateTime end = keyValuePair.Value;

                        string time = string.Format("{0}-{1}", start.ToString("yyyyMMddHHmmss"), end.ToString("yyyyMMddHHmmss"));
                        bool retry = false;
                        List<CloudflareLog> cloudflareLogs = cloundFlareApiService.GetCloudflareLogs(start, end, out retry);
                        while (retry == true)
                        {                            
                            cloudflareLogs = cloundFlareApiService.GetCloudflareLogs(start, end, out retry);
                        }

                        if (cloudflareLogs!=null&& cloudflareLogs.Count>0)
                        {
                            concurrentBagCloudflareLogs.Add(cloudflareLogs);
                        }

                        stopwatch.Stop();
                        OnMessage(new MessageEventArgs("取出数据:" + time+ "共"+ cloudflareLogs.Count+"条,用时:"+ stopwatch.ElapsedMilliseconds / 1000 + "秒"));

                        stopwatch.Restart();

                        var result = cloudflareLogs.GroupBy(a => new { a.ClientRequestHost, a.ClientIP, a.ClientRequestURI }).
                            Select( g => new CloudflareLogReportItem {
                                ClientRequestHost = g.Key.ClientRequestHost,
                                ClientIP = g.Key.ClientIP,
                                ClientRequestURI = g.Key.ClientRequestURI,
                                Count = g.Count() });

                        //List<CloudflareLogReportItem> CloudflareLogReportItemList = new List<CloudflareLogReportItem>();
                        //foreach (var group in result)
                        //{
                        //    CloudflareLogReportItemList.Add(new CloudflareLogReportItem
                        //    {
                        //        ClientRequestHost = group.ClientRequestHost,
                        //        ClientIP = group.ClientIP,
                        //        ClientRequestURI = group.ClientRequestURI,
                        //        Count = group.Count
                        //    });
                        //}
                        //foreach (var group in result)
                        //{
                        //    roteLimitConfig.RateLimits.ForEach(c => {
                        //        CloudflareLogReportItemList.Add(new CloudflareLogReportItem
                        //        {
                        //            ClientRequestHost = group.Key.ClientRequestHost,
                        //            ClientIP = group.Key.ClientIP,
                        //            //ClientRequestURI = c.Url,
                        //            ClientRequestURI = group.Key.ClientRequestURI,
                        //            Count = group.Count(a => a.ClientRequestURI.ToLower().Contains(c.Url.ToLower()))
                        //        });
                        //    });
                        //}

                        CloudflareLogReport CloudflareLogReport = new CloudflareLogReport
                        {
                            Guid = Guid.NewGuid().ToString(),
                            Time = time,
                            Start = start,
                            End = end,
                            Size = cloudflareLogs.Count,
                            CloudflareLogReportItems = result.ToArray()
                        };
                        stopwatch.Stop();
                        OnMessage(new MessageEventArgs("分析"+ time + "用时:" + stopwatch.ElapsedMilliseconds/1000 + "秒"));

                        PushReport(CloudflareLogReport);
                        //存数据库
                        //logsController.InsertData(start, cloudflareLogs);
                    }
                    else
                    {
                        break;
                    }

                    //OnMessage(new MessageEventArgs("取队列数据处理结束"));

                }
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
           
        }
        //上报处理结果
        public void PushReport(CloudflareLogReport CloudflareLogReport)
        {
            if (cloudflareLogReports == null)
            {
                cloudflareLogReports = new ConcurrentBag<CloudflareLogReport>();
            }

            cloudflareLogReports.Add(CloudflareLogReport);

        }
        //存数据
        public void SaveDb()
        {

        }
        //开启任务
        public void TaskStart()
        {
            OnMessage(new MessageEventArgs("本时间段数据处理开始"));
            List<Task> taskList = new List<Task>(taskCount);
            for (var i = 0; i < taskCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    Dequeue();
                });
                taskList.Add(task);

                Thread.Sleep(500);
            }

            Task.WaitAll(taskList.ToArray());//等待所有线程只都行完毕

            OnMessage(new MessageEventArgs("本时间段数据处理完毕"));
        }
        /// <summary>
        /// 禁止Ip
        /// </summary>
        /// <param name="ips"></param>
        public void BanIps(List<string> ips ,string comment)
        {
            //List<FirewallAccessRule> firewallAccessRuleList = logsController.GetFirewallAccessRuleList();
            List<FirewallAccessRule> firewallAccessRuleList = cloundFlareApiService.GetAccessRuleList("", "Add By Defense System");
            firewallAccessRuleList = firewallAccessRuleList.Where(a => a.mode == EnumMode.challenge).ToList();
            foreach (string ip in ips)
            {
                if(!firewallAccessRuleList.Exists(a=>a.configurationValue == ip))
                {
                    FirewallAccessRuleResponse firewallAccessRuleResponse = cloundFlareApiService.CreateAccessRule(new FirewallAccessRuleRequest
                    {
                        configuration = new Configuration
                        {
                            target = "ip",
                            value = ip,
                        },
                        mode = EnumMode.challenge,
                        notes = string.Format("{0}({1})", comment, "Add By Defense System"),
                    });

                    if (firewallAccessRuleResponse.success)
                    {
                        logsController.InsertFirewallAccessRule(new List<FirewallAccessRule>() {
                            new FirewallAccessRule
                            {
                                id = firewallAccessRuleResponse.result.id,
                                notes=firewallAccessRuleResponse.result.notes,
                                mode=firewallAccessRuleResponse.result.mode,
                                configurationTarget=firewallAccessRuleResponse.result.configuration.target,
                                configurationValue=firewallAccessRuleResponse.result.configuration.value,
                                createTime=firewallAccessRuleResponse.result.created_on,
                                modifiedTime=firewallAccessRuleResponse.result.modified_on,
                                scopeId=firewallAccessRuleResponse.result.scope.id,
                                scopeEmail=firewallAccessRuleResponse.result.scope.email,
                                scopeType=firewallAccessRuleResponse.result.scope.type,
                            }
                        });
                    }
                }
            }
        }
        public void WhitelistIps(List<string> ips, string comment)
        {
            //List<FirewallAccessRule> firewallAccessRuleList = logsController.GetFirewallAccessRuleList();
            List<FirewallAccessRule> firewallAccessRuleList = cloundFlareApiService.GetAccessRuleList("", "Add By Defense System");
            firewallAccessRuleList = firewallAccessRuleList.Where(a => a.mode == EnumMode.whitelist).ToList();
            foreach (string ip in ips)
            {
                if (!firewallAccessRuleList.Exists(a => a.configurationValue == ip))
                {
                    FirewallAccessRuleResponse firewallAccessRuleResponse = cloundFlareApiService.CreateAccessRule(new FirewallAccessRuleRequest
                    {
                        configuration = new Configuration
                        {
                            target = "ip",
                            value = ip,
                        },
                        mode = EnumMode.whitelist,
                        notes = string.Format("{0}({1})", comment, "Add By Defense System"),
                    });

                    if (firewallAccessRuleResponse.success)
                    {
                        logsController.InsertFirewallAccessRule(new List<FirewallAccessRule>() {
                            new FirewallAccessRule
                            {
                                id = firewallAccessRuleResponse.result.id,
                                notes=firewallAccessRuleResponse.result.notes,
                                mode=firewallAccessRuleResponse.result.mode,
                                configurationTarget=firewallAccessRuleResponse.result.configuration.target,
                                configurationValue=firewallAccessRuleResponse.result.configuration.value,
                                createTime=firewallAccessRuleResponse.result.created_on,
                                modifiedTime=firewallAccessRuleResponse.result.modified_on,
                                scopeId=firewallAccessRuleResponse.result.scope.id,
                                scopeEmail=firewallAccessRuleResponse.result.scope.email,
                                scopeType=firewallAccessRuleResponse.result.scope.type,
                            }
                        });
                    }
                }
            }
        }
        public List<CloudflareLog> GetCloudflareLogs()
        {
            return concurrentBagCloudflareLogs.SelectMany(a => a).ToList();
        }
    }
}
