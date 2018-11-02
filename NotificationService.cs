using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools
{
    public delegate void MessageEventhandler(Object sender, MessageEventArgs e);
    public delegate void AnalysisProgressEventhandler(Object sender, ProgressEventArgs e);
    public delegate void TransformProgressEventhandler(Object sender, ProgressEventArgs e);
    public delegate void SetAnalysisProgressMaximumEventhandler(Object sender, ProgressMaximumEventArgs e);
    public delegate void SetTransformProgressMaximumEventhandler(Object sender, ProgressMaximumEventArgs e);
    public class MessageEventArgs : EventArgs
    {
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
        }
        public MessageEventArgs(string message)
        {
            this.message = message;
        }
    }
    public class ProgressEventArgs : EventArgs
    {
        private int num;
        private int loopNum;
        private RemainTime remainTime;
        public int Num
        {
            get
            {
                return num;
            }
        }
        public int LoopNum
        {
            get
            {
                return loopNum;
            }
        }
        public RemainTime RemainTime
        {
            get
            {
                return remainTime;
            }
        }
        public ProgressEventArgs(int num)
        {
            this.num = num;
        }
        public ProgressEventArgs(int num,int loopNum):this(num)
        {
            this.loopNum = loopNum;
        }
        public ProgressEventArgs(int num, RemainTime remainTime) : this(num)
        {
            this.remainTime = remainTime;
        }
        public ProgressEventArgs(int num, int loopNum, RemainTime remainTime):this(num, loopNum)
        {
            this.remainTime = remainTime;
        }
    }
    public class ProgressMaximumEventArgs : EventArgs
    {
        private int maximum;

        public int Maximum
        {
            get
            {
                return maximum;
            }
        }
        
        public ProgressMaximumEventArgs(int maximum)
        {
            this.maximum = maximum;
        }
    }

    public interface INotificationService
    {
        /// <summary>
        /// 订阅消息事件
        /// </summary>
        /// <param name="messageEventhandler"></param>
        void SubscribeMessageEvent(MessageEventhandler messageEventhandler);
        /// <summary>
        /// 订阅设置分析进度条最大值事件
        /// </summary>
        /// <param name="setAnalysisProgressMaximumEventhandler"></param>
        void SubscribeAnalysisProgressMaximumEvent(SetAnalysisProgressMaximumEventhandler setAnalysisProgressMaximumEventhandler);
        /// <summary>
        /// 订阅设置转换进度条最大值事件
        /// </summary>
        /// <param name="messageEventhandler"></param>
        void SubscribeTransformProgressMaximum(SetTransformProgressMaximumEventhandler setTransformProgressMaximumEventhandler);
        /// <summary>
        /// 订阅分析进度变化事件
        /// </summary>
        /// <param name="analysisProgressEventhandler"></param>
        void SubscribeAnalysisProgress(AnalysisProgressEventhandler analysisProgressEventhandler);
        /// <summary>
        /// 订阅转换进度变化事件
        /// </summary>
        /// <param name="transformProgressEventhandler"></param>
        void SubscribeTransformProgress(TransformProgressEventhandler transformProgressEventhandler);
    }
    public class NotificationService : INotificationService
    {
        public event MessageEventhandler Message;
        public event AnalysisProgressEventhandler AnalysisProgress;
        public event TransformProgressEventhandler TransformProgress;
        public event SetAnalysisProgressMaximumEventhandler AnalysisProgressMaximum;
        public event SetTransformProgressMaximumEventhandler TransformProgressMaximum;

        public virtual void OnMessage(MessageEventArgs e)
        {
            Message?.Invoke(this, e);
        }
        public virtual void OnAnalysisProgress(ProgressEventArgs e)
        {
            AnalysisProgress?.Invoke(this, e);
        }
        public virtual void OnTransformProgress(ProgressEventArgs e)
        {
            TransformProgress?.Invoke(this, e);
        }
        public virtual void OnAnalysisProgressMaximum(ProgressMaximumEventArgs e)
        {
            AnalysisProgressMaximum?.Invoke(this, e);
        }
        public virtual void OnTransformProgressMaximum(ProgressMaximumEventArgs e)
        {
            TransformProgressMaximum?.Invoke(this, e);
        }

        public void SubscribeMessageEvent(MessageEventhandler messageEventhandler)
        {
            this.Message += messageEventhandler;
        }
        /// <summary>
        /// 订阅设置分析进度条最大值事件
        /// </summary>
        /// <param name="setAnalysisProgressMaximumEventhandler"></param>
        public void SubscribeAnalysisProgressMaximumEvent(SetAnalysisProgressMaximumEventhandler setAnalysisProgressMaximumEventhandler)
        {
            this.AnalysisProgressMaximum += setAnalysisProgressMaximumEventhandler;
        }
        /// <summary>
        /// 订阅设置转换进度条最大值事件
        /// </summary>
        /// <param name="messageEventhandler"></param>
        public void SubscribeTransformProgressMaximum(SetTransformProgressMaximumEventhandler setTransformProgressMaximumEventhandler)
        {
            this.TransformProgressMaximum += setTransformProgressMaximumEventhandler;
        }
        /// <summary>
        /// 订阅分析进度变化事件
        /// </summary>
        /// <param name="analysisProgressEventhandler"></param>
        public void SubscribeAnalysisProgress(AnalysisProgressEventhandler analysisProgressEventhandler)
        {
            this.AnalysisProgress += analysisProgressEventhandler;
        }
        /// <summary>
        /// 订阅转换进度变化事件
        /// </summary>
        /// <param name="transformProgressEventhandler"></param>
        public void SubscribeTransformProgress(TransformProgressEventhandler transformProgressEventhandler)
        {
            this.TransformProgress += transformProgressEventhandler;
        }
    }
    public class RemainTime
    {
        public EnumRemainTimeType RemainTimeType { get; set; }
        public decimal Value { get; set; }
        public override string ToString()
        {
            string dimensionText = "";
            switch (RemainTimeType)
            {
                case EnumRemainTimeType.Hour:
                    dimensionText = "小时";
                    break;
                case EnumRemainTimeType.Minute:
                    dimensionText = "分钟";
                    break;
                case EnumRemainTimeType.Second:
                    dimensionText = "秒";
                    break;
            }
            return string.Format("大概{0}{1}后完成.",Value, dimensionText);
        }
    }
    public enum EnumRemainTimeType
    {
        Hour,
        Minute,
        Second
    }
}
