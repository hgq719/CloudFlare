drop table t_Cloudflare_RequestLimitConfig;
 
 CREATE TABLE [t_Cloudflare_RequestLimitConfig](
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT, 
    [Url] VARCHAR(512), 
    [Interval] INT, 
    [LimitTimes] INT, 
    [Remark] VARCHAR(512),
    [CreateTime] DATETIME, 
    [Status] BOOL);