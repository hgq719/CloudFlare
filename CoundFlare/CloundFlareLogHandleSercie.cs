using Castle.Core.Logging;
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
        void BanIps(List<string> ips);
    }
    public class CloudflareLogHandleSercie : NotificationService, ICloudflareLogHandleSercie
    {
        private ConcurrentBag<CloudflareLogReport> cloudflareLogReports;
        private ConcurrentQueue<KeyValuePair<DateTime, DateTime>> keyValuePairs;
        private List<RequestLimitConfig> requestLimitConfigs;
        private ILogger logger = Abp.Logging.LogHelper.Logger;
        private double sample = 0.01;
        private int timeSpan = 5;
        private DateTime startTime;
        private DateTime endTime;
        private int taskCount = 1;
        private bool resultSaveDb = false;

        public CloudflareLogHandleSercie()
        {
            timeSpan = Convert.ToInt32(ConfigurationManager.AppSettings["timeSpan"]);
            sample = Convert.ToDouble(ConfigurationManager.AppSettings["sample"]);
            taskCount = Convert.ToInt32(ConfigurationManager.AppSettings["taskCount"]);
            resultSaveDb = Convert.ToBoolean(ConfigurationManager.AppSettings["resultSaveDb"]);
        }

        public ICloundFlareApiService cloundFlareApiService { get; set; }
        public ILogsController logsController { get; set; }

        //产生队列数据
        public void InitQueue(DateTime startTime, DateTime endTime)
        {
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

            requestLimitConfigs = logsController.GetRequestLimitConfigs();
            this.startTime = startTime;
            this.endTime = endTime;
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
                    .Select(g => new { g.Key.ClientRequestHost, g.Key.ClientIP, g.Key.ClientRequestURI, Ban=false, Count = g.Sum(c => c.Count) }).OrderByDescending(a=>a.Count).ToList();
                
                List<IpNeedToBan> ipNeedToBans = new List<IpNeedToBan>();

                List<IpNeedToBan> ipNeedToBansHas = logsController.GetIpNeedToBans();                

                var banItems = new List<CloudflareLogReportItem>();

                var result = (from item in itemsManyGroup
                              from config in requestLimitConfigs
                              where item.ClientRequestURI == config.Url
                                    && ( ( item.Count / ( (end - start).TotalSeconds * sample ) )  >= (config.LimitTimes / config.Interval) )
                              select item).OrderByDescending(a=>a.Count).ToList();

                foreach(var item in result)
                {
                    banItems.Add(new CloudflareLogReportItem
                    {
                        ClientRequestHost = item.ClientRequestHost,
                        ClientIP = item.ClientIP,
                        ClientRequestURI = item.ClientRequestURI,
                        Count = item.Count,
                        Ban = true
                    });

                    if (!ipNeedToBansHas.Exists(a => a.IP == item.ClientIP))
                    {
                        ipNeedToBans.Add(new IpNeedToBan
                        {
                            Host = item.ClientRequestHost,
                            IP = item.ClientIP,
                            RelatedURL = item.ClientRequestURI,
                            HasBanned = false,
                            RequestedTime = DateTime.Now,
                            Remark = ""
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
                        stopwatch.Stop();
                        OnMessage(new MessageEventArgs("取出数据:" + time+ "共"+ cloudflareLogs.Count+"条,用时:"+ stopwatch.ElapsedMilliseconds / 1000 + "秒"));

                        stopwatch.Restart();

                        var result = cloudflareLogs.GroupBy(a => new { a.ClientRequestHost, a.ClientIP });

                        List<CloudflareLogReportItem> CloudflareLogReportItemList = new List<CloudflareLogReportItem>();
                        foreach (var group in result)
                        {
                            requestLimitConfigs.ForEach(c => {
                                CloudflareLogReportItemList.Add(new CloudflareLogReportItem
                                {
                                    ClientRequestHost = group.Key.ClientRequestHost,
                                    ClientIP = group.Key.ClientIP,
                                    ClientRequestURI = c.Url,
                                    Count = group.Count(a => a.ClientRequestURI.ToLower().Contains(c.Url.ToLower()))
                                });
                            });
                        }

                        CloudflareLogReport CloudflareLogReport = new CloudflareLogReport
                        {
                            Guid = Guid.NewGuid().ToString(),
                            Time = time,
                            Start = start,
                            End = end,
                            Size = cloudflareLogs.Count,
                            CloudflareLogReportItems = CloudflareLogReportItemList.ToArray()
                        };
                        stopwatch.Stop();
                        //OnMessage(new MessageEventArgs("分析"+ time + "用时:" + stopwatch.ElapsedMilliseconds/1000 + "秒"));

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
        public void BanIps(List<string> ips)
        {
            //List<FirewallAccessRule> firewallAccessRuleList = logsController.GetFirewallAccessRuleList();
            List<FirewallAccessRule> firewallAccessRuleList = cloundFlareApiService.GetAccessRuleList("", "Add By Defense System");
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
                        mode = "block",
                        notes = "Add By Defense System",
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
    }
}
