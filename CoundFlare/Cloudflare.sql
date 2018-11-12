    	drop table [t_Cloudflare_Settings];
	--参数
 CREATE TABLE [t_Cloudflare_Settings](
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT , 
    [Key] VARCHAR(64), 
    [Value] VARCHAR(128),
    [Remark] VARCHAR(1024)--备注信息
    );

		drop table [t_Cloudflare_RequestLimitConfig];
 --rote limit 配置
 CREATE TABLE [t_Cloudflare_RequestLimitConfig](
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT , 
    [Url] VARCHAR(512), 
    [Interval] INT, 
    [LimitTimes] INT, 
    [Remark] VARCHAR(512),
    [CreateTime] DATETIME, 
    [Status] BOOL);

			drop table [t_Cloudflare_TriggerLogs];
--记录触发ratelimit条件日志
CREATE TABLE [t_Cloudflare_TriggerLogs](
[Id] INTEGER PRIMARY KEY AUTOINCREMENT , 
[RequestLimitConfigId] INT,--条件ID
[RequestLimitConfigDetail] VARCHAR(1024),--条件明细 Json
[IpNumber] INT,--触发IP的数量
[TriggerTime] DATETIME,--触发日期 
[Action] VARCHAR(128),--采取的动作 None/Create/Delete
[Remark] VARCHAR(1024)--备注信息
);

			drop table [t_Cloudflare_TriggerLogDetails];
--记录触发ratelimit条件明细
CREATE TABLE [t_Cloudflare_TriggerLogDetails](
[Id] INTEGER PRIMARY KEY AUTOINCREMENT , 
[TriggerLogId] INT,--触发日志ID
[StartTime] DATETIME,--分析开始日期
[EndTime] DATETIME,--分析截止日期
[Size] INT,--分析数据量
[Sample] FLOAT,--样本比例 
[ClientIP] VARCHAR(32),--访问IP
[ClientRequestHost] VARCHAR(128),--访问的站点
[ClientRequestURI] VARCHAR(512),--访问的站点
[Count] INT--命中次数
);

			drop table [t_Cloudflare_Actions];
--禁用IP
CREATE TABLE [t_Cloudflare_Actions](
[Id] INTEGER PRIMARY KEY AUTOINCREMENT , 
[TriggerLogId] INT,--触发日志ID
[ClientIP] VARCHAR(32),--访问IP
[ActionTime] DATETIME,--处理日期 
[Mode] VARCHAR(32),--采取的动作 Ban/UnBan/WhiteList
[Remark] VARCHAR(1024)--备注信息
);


select count(*) from t_Cloudflare_TriggerLogDetails