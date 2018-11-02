using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.CoundFlare
{
    public interface IDataGeneratorService<T> where T : new()
    {
        T Generator();
        List<T> GeneratorList(int size);
    }
    /// <summary>
    /// 模拟数据生成器
    /// </summary>
    public class DataGeneratorService<T> : IDataGeneratorService<T> where T : new()
    {
        public T Generator()
        {
            T t = new T();
            //利用反射获得属性的所有公共属性
            Type modelType = t.GetType();
            PropertyInfo[] propertyInfos = modelType.GetProperties();
            foreach(PropertyInfo proInfo in propertyInfos)
            {                
                if(proInfo.GetType() == Type.GetType("System.Int32"))
                {
                    Random random = new Random(Guid.NewGuid().GetHashCode());
                    int value = random.Next(100000);
                    proInfo.SetValue(t, value);
                }
                else if (proInfo.GetType() == Type.GetType("System.String"))
                {
                    string value = Guid.NewGuid().ToString();
                    proInfo.SetValue(t, value);
                }
                else if (proInfo.GetType() == Type.GetType("System.DateTime"))
                {
                    DateTime dateTime = DateTime.Now;
                    proInfo.SetValue(t, dateTime);
                }
                else if (proInfo.GetType() == Type.GetType("System.Double"))
                {
                    Random random = new Random(Guid.NewGuid().GetHashCode());
                    double value = random.NextDouble();
                    proInfo.SetValue(t, value);
                }
                else if (proInfo.GetType().IsValueType)
                {
                    Random random = new Random(Guid.NewGuid().GetHashCode());
                    int value = random.Next(1000);
                    proInfo.SetValue(t, value);
                }
                else if (proInfo.GetType().IsByRef)
                {
                    string value = Guid.NewGuid().ToString();
                    proInfo.SetValue(t, value);
                }
            }
            return t;
        }
        public List<T> GeneratorList(int size)
        {
            List<T> tList = new List<T>();
            for(var i = 0; i < size; i++)
            {
                tList.Add(Generator());
            }
            return tList;
        }
    }
}
