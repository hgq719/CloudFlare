using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using Castle.Core.Logging;

namespace CoundFlareTools.CoundFlare
{
    public interface ILogsController
    {
        void InsertData(DateTime startTime, List<CloudflareLog> CloudflareLogs);
        List<RequestLimitConfig> GetRequestLimitConfigs();
        void InsertResultData(List<IpNeedToBan> ipNeedToBans);
        void InsertResultDataBulk(List<IpNeedToBan> ipNeedToBans);
        List<IpNeedToBan> GetIpNeedToBans();
        void InsertIntoIpNeedToBanTable(IpNeedToBan ipNeedToBan);
        void InsertFirewallAccessRule(List<FirewallAccessRule> firewallAccessRuleList);
        void DeleteFirewallAccessRule(string id);
        List<FirewallAccessRule> GetFirewallAccessRuleList();
    }

    public class LogsController : ILogsController
    { 
        private string _connStr = "Data Source=.;Initial Catalog=CloudflareDb;User id=sa;Password=sa123;";
        private ILogger logger = Abp.Logging.LogHelper.Logger;
        public LogsController()
        {
            _connStr = ConfigurationManager.AppSettings["connStr"];
        }
        public  void InsertIntoIpNeedToBanTable(IpNeedToBan ipNeedToBan)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                var sql = new StringBuilder(string.Format(@"IF NOT EXISTS(SELECT * FROM [dbo].[IpNeedToBan]
                                 WHERE IP = '{0}' 
                                )
                                INSERT INTO  [dbo].[IpNeedToBan]
                                values ('{0}', '{1}' , '{2}','{3}', '{4}' , '{5}')
                                                            ", ipNeedToBan.IP, ipNeedToBan.RelatedURL, ipNeedToBan.Host
                                                            , ipNeedToBan.RequestedTime, ipNeedToBan.HasBanned, ipNeedToBan.Remark));
                var cmd = new SqlCommand(sql.ToString(), conn);

                cmd.ExecuteNonQuery();
            }
        }
        public void InsertData(DateTime startTime, List<CloudflareLog> cloudflareLogs)
        {
            var entDt = GetTableSchema();
            var hostedDt = entDt.Copy();
            var chatServerDt = entDt.Copy();
            var livechatDt = entDt.Copy();
            var alltDt = entDt.Copy();
            var stopwatch = new Stopwatch();
            foreach (CloudflareLog log in cloudflareLogs)
            {
                if (!log.CacheCacheStatus.Equals("hit"))
                {
                    if (log.ClientRequestHost.ToLower().StartsWith("ent.comm100.com"))
                    {
                        InsertRow(log, entDt);
                    }
                    else if (log.ClientRequestHost.ToLower().StartsWith("hosted.comm100.com"))
                    {
                        InsertRow(log, hostedDt);
                    }
                    else if (log.ClientRequestHost.ToLower().StartsWith("chatserver.comm100.com"))
                    {
                        InsertRow(log, chatServerDt);
                    }
                    else if (log.ClientRequestHost.ToLower().StartsWith("livechat.app"))
                    {
                        InsertRow(log, livechatDt);
                    }
                    InsertRow(log, alltDt);
                }
            }
            InsertData(stopwatch, startTime, entDt, hostedDt, chatServerDt, livechatDt);
        }
        public List<RequestLimitConfig> GetRequestLimitConfigs()
        {
            var configs = new List<RequestLimitConfig>();
            var conn = new SqlConnection(_connStr);
            conn.Open();
            var reader = GetRequestLimitConfigReaders(conn);
            if (!reader.HasRows)
            {
                reader.Close();
                return null;
            }
            while (reader.Read())
            {
                configs.Add(new RequestLimitConfig()
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Url = Convert.ToString(reader["URL"]),
                    Interval = Convert.ToInt32(reader["Interval"]),
                    LimitTimes = Convert.ToInt32(reader["LimitTimes"]),
                    Remark = Convert.ToString(reader["Remark"]),
                });
            }
            reader.Close();
            conn.Close();
            return configs;
        }
        public List<IpNeedToBan> GetIpNeedToBans()
        {
            var ipNeedToBanS = new List<IpNeedToBan>();
            var conn = new SqlConnection(_connStr);
            conn.Open();
            var reader = GetIpNeedToBanReaders(conn);
            if (!reader.HasRows)
            {
                reader.Close();
                return ipNeedToBanS;
            }
            while (reader.Read())
            {
                ipNeedToBanS.Add(new IpNeedToBan()
                {
                    IP = Convert.ToString(reader["IP"]),
                    RelatedURL = Convert.ToString(reader["RelatedURL"]),
                    Host = Convert.ToString(reader["Host"]),
                    HasBanned = Convert.ToBoolean(reader["HasBanned"]),
                    Remark = Convert.ToString(reader["Remark"]),
                });
            }
            reader.Close();
            conn.Close();
            return ipNeedToBanS;
        }
        public void InsertFirewallAccessRule(List<FirewallAccessRule> firewallAccessRuleList)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var dt = GetTableFirewallAccessRuleSchema();
            foreach (FirewallAccessRule role in firewallAccessRuleList)
            {
                InsertFirewallAccessRuleRow(role, dt);
            }
            InsertFirewallAccessRule(dt);
            stopwatch.Stop();
            logger.Debug(string.Format("InsertFirewallAccessRule {0}秒", stopwatch.ElapsedMilliseconds / 1000));
        }
        public void DeleteFirewallAccessRule(string id)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                var sql = @"DELETE
                      FROM FirewallAccessRule WHERE id=@id";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter()
                {
                    ParameterName = "id",
                    Value = id,
                });

                var reader = cmd.ExecuteNonQuery();

                conn.Close();
            }
        }
        public List<FirewallAccessRule> GetFirewallAccessRuleList()
        {
            List<FirewallAccessRule> firewallAccessRules = new List<FirewallAccessRule>();
            var stopwatch = new Stopwatch();
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                var sql = @"SELECT *
                      FROM FirewallAccessRule";

                var cmd = new SqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                }
                else
                {
                    while (reader.Read())
                    {
                        firewallAccessRules.Add(new FirewallAccessRule()
                        {
                            id = Convert.ToString(reader["id"]),
                            notes = Convert.ToString(reader["notes"]),
                            mode =(EnumMode)Enum.Parse(typeof(EnumMode), Convert.ToString(reader["mode"] )),
                            configurationTarget = Convert.ToString(reader["configurationTarget"]),
                            configurationValue = Convert.ToString(reader["configurationValue"]),
                            createTime = Convert.ToDateTime(reader["createTime"]),
                            modifiedTime = Convert.ToDateTime(reader["modifiedTime"]),
                            scopeId = Convert.ToString(reader["scopeId"]),
                            scopeEmail = Convert.ToString(reader["scopeEmail"]),
                            scopeType = Convert.ToString(reader["scopeType"]),
                        });
                    }
                }
                reader.Close();
                conn.Close();
            }
            return firewallAccessRules;
        }
        private void InsertRow(CloudflareLog log, DataTable table)
        {
            var dr = table.NewRow();
            dr[0] = Guid.NewGuid();
            dr[1] = log.RayID;
            dr[2] = log.ClientIP;
            dr[3] = log.ClientRequestHost;
            dr[4] = log.ClientRequestMethod;
            dr[5] = log.ClientRequestURI;
            dr[6] = log.EdgeEndTimestamp;
            dr[7] = log.EdgeResponseBytes;
            dr[8] = log.EdgeResponseStatus;
            dr[9] = log.EdgeStartTimestamp;
            dr[10] = log.CacheResponseStatus;
            dr[11] = log.ClientRequestBytes;
            dr[12] = log.CacheCacheStatus;
            dr[13] = log.OriginResponseStatus;
            dr[14] = log.OriginResponseTime;
            dr[15] = log.OriginResponseBytes;
            table.Rows.Add(dr);
        }

        private void CreateTable(string tableName, string domain)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                var sql = new StringBuilder(string.Format(@"IF NOT EXISTS
                                                            (
                                                            SELECT NAME FROM sysobjects where type = 'U' and name = '{0}'
                                                            )
                                                            BEGIN 

                                                            CREATE TABLE [dbo].[{0}](
	                                                            [Id] [uniqueidentifier] NOT NULL,
	                                                            [RayID] [nvarchar](max) NULL,
	                                                            [ClientIP] [nvarchar](max) NULL,
	                                                            [ClientRequestHost] [nvarchar](max) NULL,
	                                                            [ClientRequestMethod] [nvarchar](max) NULL,
	                                                            [ClientRequestURI] [nvarchar](max) NULL,
	                                                            [EdgeEndTimestamp] [nvarchar](max) NULL,
	                                                            [EdgeResponseBytes] [bigint] NULL,
	                                                            [EdgeResponseStatus] [int] NULL,
	                                                            [EdgeStartTimestamp] [nvarchar](max) NULL,
	                                                            [CacheResponseStatus] [int] NULL,
	                                                            [ClientRequestBytes] [bigint] NULL,
	                                                            [CacheCacheStatus] [nvarchar](max) NULL,
	                                                            [OriginResponseStatus] [int] NULL,
	                                                            [OriginResponseTime] [bigint] NULL,
	                                                            [OriginResponseBytes] [bigint] NULL,
                                                             CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
                                                            (
	                                                            [Id] ASC
                                                            )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

                                                            END;
                                                            ", tableName));
                var cmd = new SqlCommand(sql.ToString(), conn);

                cmd.ExecuteNonQuery();
            }
        }

        private void InsertData(Stopwatch stopwatch, DataTable dt, string tableName)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                var bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = dt.Rows.Count;
                conn.Open();
                stopwatch.Start();
                bulkCopy.WriteToServer(dt);
                stopwatch.Stop();
            }
            Console.WriteLine($"插入{tableName}表{dt.Rows.Count}条记录共花费{stopwatch.ElapsedMilliseconds}毫秒");
        }

        private void InsertData(Stopwatch stopwatch,DateTime startTime, DataTable entDt, DataTable hostedDt, DataTable chatServerDt, DataTable livechatDt)
        {
            var domain = "";
            var tableName = "";
            if (entDt != null && entDt.Rows.Count != 0)
            {
                domain = "ent";
                tableName = string.Format("CloudflareLogs_{0}_{1}", domain, startTime.ToString("yyyyMMddHH"));
                CreateTable(tableName, "ent");
                InsertData(stopwatch, entDt, tableName);
            }
            if (hostedDt != null && hostedDt.Rows.Count != 0)
            {
                domain = "hosted";
                tableName = string.Format("CloudflareLogs_{0}_{1}", domain, startTime.ToString("yyyyMMddHH"));
                CreateTable(tableName, "hosted");
                InsertData(stopwatch, hostedDt, tableName);
            }
            if (chatServerDt != null && chatServerDt.Rows.Count != 0)
            {
                domain = "chatServer";
                tableName = string.Format("CloudflareLogs_{0}_{1}", domain, startTime.ToString("yyyyMMddHH"));
                CreateTable(tableName, "chatServer");
                InsertData(stopwatch, chatServerDt, tableName);
            }
            if (livechatDt != null && livechatDt.Rows.Count != 0)
            {
                domain = "livechat";
                tableName = string.Format("CloudflareLogs_{0}_{1}", domain, startTime.ToString("yyyyMMddHH"));
                CreateTable(tableName, "livechat");
                InsertData(stopwatch, livechatDt, tableName);
            }
        }

        private void InsertResultRow(IpNeedToBan log, DataTable table)
        {
            var dr = table.NewRow();
            dr[0] = log.IP;
            dr[1] = log.RelatedURL;
            dr[2] = log.Host;
            dr[3] = log.RequestedTime;
            dr[4] = log.HasBanned;
            dr[5] = log.Remark;
            table.Rows.Add(dr);
        }
        private void InsertFirewallAccessRuleRow(FirewallAccessRule role, DataTable table)
        {
            var dr = table.NewRow();
            dr[0] = role.id;
            dr[1] = role.notes;
            dr[2] = role.mode;
            dr[3] = role.configurationTarget;
            dr[4] = role.configurationValue;
            dr[5] = role.createTime;
            dr[6] = role.modifiedTime;
            dr[7] = role.scopeId;
            dr[8] = role.scopeEmail;
            dr[9] = role.scopeType;
            table.Rows.Add(dr);
        }
        public void InsertResultData(List<IpNeedToBan> ipNeedToBans)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                //启动事务
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    foreach (IpNeedToBan model in ipNeedToBans)
                    {
                        InsertIntoIpNeedToBanTable(model, conn, tran);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }

            }
            stopwatch.Stop();
            logger.Debug(string.Format("InsertResultData {0}秒", stopwatch.ElapsedMilliseconds / 1000));
        }
        public void InsertResultDataBulk(List<IpNeedToBan> ipNeedToBans)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var dt = GetTableResultSchema();    
            foreach (IpNeedToBan log in ipNeedToBans)
            {
                InsertResultRow(log, dt);             
            }
            InsertResultData(dt);
            stopwatch.Stop();
            logger.Debug(string.Format("InsertResultDataBulk {0}秒", stopwatch.ElapsedMilliseconds / 1000));
        }

        private void InsertIntoIpNeedToBanTable(IpNeedToBan ipNeedToBan, SqlConnection conn, SqlTransaction tran)
        {
            var sql = new StringBuilder(string.Format(@"IF NOT EXISTS(SELECT * FROM [dbo].[IpNeedToBan]
                                 WHERE IP = '{0}'
                                )
                                INSERT INTO  [dbo].[IpNeedToBan]
                                values ('{0}', '{1}' , '{2}','{3}', '{4}' , '{5}')
                                                            ", ipNeedToBan.IP, ipNeedToBan.RelatedURL, ipNeedToBan.Host
                                                            , ipNeedToBan.RequestedTime, ipNeedToBan.HasBanned, ipNeedToBan.Remark));
            var cmd = new SqlCommand(sql.ToString(), conn, tran);

            cmd.ExecuteNonQuery();
        }
        private void InsertResultData(DataTable resultDt)
        {
            var tableName = "IpNeedToBan";
            var stopwatch = new Stopwatch();
            using (var conn = new SqlConnection(_connStr))
            {
                var bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = resultDt.Rows.Count;
                conn.Open();
                stopwatch.Start();
                bulkCopy.WriteToServer(resultDt);
                stopwatch.Stop();
            }
            Console.WriteLine($"插入{tableName}表{resultDt.Rows.Count}条记录共花费{stopwatch.ElapsedMilliseconds}毫秒");
        }
        private void InsertFirewallAccessRule(DataTable resultDt)
        {
            var tableName = "FirewallAccessRule";
            var stopwatch = new Stopwatch();
            using (var conn = new SqlConnection(_connStr))
            {
                var bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = resultDt.Rows.Count;
                conn.Open();
                stopwatch.Start();
                bulkCopy.WriteToServer(resultDt);
                stopwatch.Stop();
            }
            Console.WriteLine($"插入{tableName}表{resultDt.Rows.Count}条记录共花费{stopwatch.ElapsedMilliseconds}毫秒");
        }

        private string GetUTCTimeString(DateTime time)
        {
            return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
        }

        private DataTable GetIpNeedToBanTableSchema()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("IP", typeof(string)),
                new DataColumn("RelatedURL", typeof(string)),
                new DataColumn("Host", typeof(string)),
                new DataColumn("RequestedTime", typeof(DateTime)),
                new DataColumn("HasBanned", typeof(string)),
                new DataColumn("Remark", typeof(string))
            });
            return dt;
        }

        private void CreateIpNeedToBanTable()
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                var sql = new StringBuilder(string.Format(@"IF NOT EXISTS
                                                            (
                                                            SELECT NAME FROM sysobjects where type = 'U' and name = 'IpNeedToBan'
                                                            )
                                                            BEGIN 
                                                            Create Table IpNeedToBan
                                                            (
                                                            IP NVARCHAR(256) NOT NUll DEFAULT(''),
                                                            RelatedURL NVARCHAR(2048),
                                                            Host NVARCHAR(512),
                                                            RequestedTime DATETIME,
                                                            HasBanned BIT NOT NUll DEFAULT 0,
                                                            Remark NVARCHAR(512) NOT NUll DEFAULT('')
                                                            )
                                                            END;
                                                            "));
                var cmd = new SqlCommand(sql.ToString(), conn);

                cmd.ExecuteNonQuery();
            }
        }

        private DataTable GetTableSchema()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Id", typeof(Guid)),
                new DataColumn("RayID", typeof(string)),
                new DataColumn("ClientIP", typeof(string)),
                new DataColumn("ClientRequestHost", typeof(string)),
                new DataColumn("ClientRequestMethod", typeof(string)),
                new DataColumn("ClientRequestURI", typeof(string)),
                new DataColumn("EdgeEndTimestamp", typeof(string)),
                new DataColumn("EdgeResponseBytes", typeof(long)),
                new DataColumn("EdgeResponseStatus", typeof(int)),
                new DataColumn("EdgeStartTimestamp", typeof(string)),
                new DataColumn("CacheResponseStatus", typeof(int)),
                new DataColumn("ClientRequestBytes", typeof(long)),
                new DataColumn("CacheCacheStatus", typeof(string)),
                new DataColumn("OriginResponseStatus", typeof(int)),
                new DataColumn("OriginResponseTime", typeof(long)),
                new DataColumn("OriginResponseBytes", typeof(long))
            });
            return dt;
        }
        private DataTable GetTableResultSchema()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("IP", typeof(string)),
                new DataColumn("RelatedURL", typeof(string)),
                new DataColumn("Host", typeof(string)),
                new DataColumn("RequestedTime", typeof(DateTime)),
                new DataColumn("HasBanned", typeof(bool)),
                new DataColumn("Remark", typeof(string))
            });
            return dt;
        }
        private DataTable GetTableFirewallAccessRuleSchema()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("id", typeof(string)),
                new DataColumn("notes", typeof(string)),
                new DataColumn("mode", typeof(string)),
                new DataColumn("configurationTarget", typeof(string)),
                new DataColumn("configurationValue", typeof(string)),
                new DataColumn("createTime", typeof(DateTime)),
                new DataColumn("modifiedTime", typeof(DateTime)),
                new DataColumn("scopeId", typeof(string)),
                new DataColumn("scopeEmail", typeof(string)),
                new DataColumn("scopeType", typeof(string)),
            });
            return dt;
        }
        private SqlDataReader GetIpNeedToBanReaders(SqlConnection conn)
        {
            var sql = @"SELECT [IP]
                          ,[RelatedURL]
                          ,[Host]
                          ,[RequestedTime]
                          ,[HasBanned]
                          ,[Remark]
                      FROM [CloudflareDb].[dbo].[IpNeedToBan]";

            var cmd = new SqlCommand(sql, conn);
            return cmd.ExecuteReader();
        }
        private SqlDataReader GetRequestLimitConfigReaders(SqlConnection conn)
        {
            var sql = @"SELECT [Id]
                              ,[URL]
                              ,[Interval]
                              ,[LimitTimes]
                              ,[Remark]
                          FROM [dbo].[RequestLimitConfiguration]";

            var cmd = new SqlCommand(sql, conn);
            return cmd.ExecuteReader();
        }
    }
    public class LogsControllerImpByLog : ILogsController
    {
        public void DeleteFirewallAccessRule(string id)
        {
            //throw new NotImplementedException();
        }

        public List<FirewallAccessRule> GetFirewallAccessRuleList()
        {
            //throw new NotImplementedException();
            return new List<FirewallAccessRule>();
        }

        public List<IpNeedToBan> GetIpNeedToBans()
        {
            //throw new NotImplementedException();
            return new List<IpNeedToBan>();
        }

        public List<RequestLimitConfig> GetRequestLimitConfigs()
        {
            string json = Utils.GetFileContext("CoundFlare/RequestLimitConfiguration.json");
            List<RequestLimitConfig> requestLimitConfigs = JsonConvert.DeserializeObject<List<RequestLimitConfig>>(json);
            return requestLimitConfigs;
        }

        public void InsertData(DateTime startTime, List<CloudflareLog> CloudflareLogs)
        {
            //throw new NotImplementedException();            
        }

        public void InsertFirewallAccessRule(List<FirewallAccessRule> firewallAccessRuleList)
        {
            //throw new NotImplementedException();
        }

        public void InsertIntoIpNeedToBanTable(IpNeedToBan ipNeedToBan)
        {
            //throw new NotImplementedException();
        }

        public void InsertResultData(List<IpNeedToBan> ipNeedToBans)
        {
            //throw new NotImplementedException();
        }

        public void InsertResultDataBulk(List<IpNeedToBan> ipNeedToBans)
        {
            //throw new NotImplementedException();
        }
    }
}