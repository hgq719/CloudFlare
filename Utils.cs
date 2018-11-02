using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CoundFlareTools
{
    public abstract class Utils
    {
        public static string GetFileContext(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    return sr.ReadToEnd();
                }
            }
            else
            {
                throw new Exception("文件不存在!");
            }
        }
        /// <summary>
        /// 按照间隔，去掉多余的空格和换行符
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        public static string TextSplitTrim(string text, string separator)
        {
            string[] array = text.Split( new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            if (array != null && array.Length > 0)
            {
                for(int i = 0; i < array.Length; i++)
                {
                    array[i] = TextTrim(array[i]);
                }  
            }
            text = string.Join(separator, array);
            return text;
        }
        /// <summary>
        /// 去掉多余的空格和换行符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string TextTrim(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }
            else
            {
                string[] array = text.Trim().Replace("\n", "").Replace("\n", "\r").ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                text = string.Join(" ", array);
                return text;
            }
        }
        public static string TrimSpace(string text)
        {
            byte[] utf8space = new byte[] { 0xc2, 0xa0 };
            string tempSpace = Encoding.GetEncoding("UTF-8").GetString(utf8space);
            return text.Replace(" ", "").Replace(tempSpace, "");
        }
        public static string GetMD5HashFromFile(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                //StringBuilder sb = new StringBuilder();
                //for (int i = 0; i < retVal.Length; i++)
                //{
                //    sb.Append(retVal[i].ToString("x2"));
                //}
                //return sb.ToString();

                return BitConverter.ToString(retVal);

            }              
        }
        /// <summary>
        /// 通过xsd验证xml格式是否正确，正确返回空字符串，错误返回提示  
        /// </summary>
        /// <param name="xmlFilePath">xml文件</param>
        /// <param name="xsdFilePath">xsd文件</param>
        /// <param name="nameSpaceUrl">命名空间，无则默认为null</param>
        /// <returns></returns>
        public static bool XmlValidationByXsd(string xml, string xsd, out IList<string> errorMessages, string nameSpaceUrl = null)
        {
            errorMessages = new List<string>();
            IList<string> messages = new List<string>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (x, y) =>
            {
                messages.Add(string.Format("{0}", y.Message));
            };
            using (TextReader xsdTextReader = new StringReader(xsd))
            using (XmlReader xsdReader = XmlReader.Create(xsdTextReader))
            using (TextReader xmlTextReader = new StringReader(xml))
            using (XmlReader xmlReader = XmlReader.Create(xmlTextReader, settings))
            {
                settings.Schemas.Add(nameSpaceUrl, xsdReader);
                try
                {
                    while (xmlReader.Read()) ;
                }
                catch (XmlException ex)
                {
                    messages.Add(string.Format("{0}", ex.Message));
                }
                errorMessages = messages;
                return !(messages.Count > 0);
            }
        }
        public static string XmlSerialize<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                Type t = obj.GetType();
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }
        public static T XmlDeserialize<T>(string xml) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static bool JsonValidationBySchema(string json, string schema, out IList<string> errorMessages )
        {
            var jSchema = JsonSchema.Parse(schema);
            var jObject = JObject.Parse(json);

            return jObject.IsValid(jSchema, out errorMessages);
        }
        /// <summary>  
        /// Encrypts the specified clear text.  
        /// </summary>  
        /// <param name="clearText">The clear text.</param>  
        /// <param name="key">The key.</param>  
        /// <returns>System.String.</returns>  
        /// <remarks>Editor：v-liuhch CreateTime：2015/5/17 16:56:40</remarks>  
        public static string Encrypt(string clearText, string key)
        {
            return SymmetricCryPtoHelper.Encrypt(clearText,key);
        }
        /// <summary>  
        /// Decrypts the specified encrypted text.  
        /// </summary>  
        /// <param name="encryptedText">The encrypted text.</param>  
        /// <param name="key">The key.</param>  
        /// <returns>System.String.</returns>  
        /// <remarks>Editor：v-liuhch CreateTime：2015/5/17 16:56:44</remarks>  
        public static string Decrypt(string encryptedText, string key)
        {
            return SymmetricCryPtoHelper.Decrypt(encryptedText, key);
        }
        /// <summary>
        /// 生成文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="context"></param>
        public static void WriteNewFile(string fileName, string context)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (FileStream fileStream = File.Create(fileName))
            {
                byte[] bytes = new UTF8Encoding(true).GetBytes(context);
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }
        //public static SerializableDictionary<TKey,TValue> Dictionary2SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        //{
        //    SerializableDictionary<TKey, TValue> serializableDictionary = new SerializableDictionary<TKey, TValue>();
        //    foreach(TKey key in dictionary.Keys)
        //    {
        //        serializableDictionary.Add(key, dictionary[key]);
        //    }
        //    return serializableDictionary;
        //}
        //public static Dictionary<TKey, TValue> SerializableDictionary2Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serializableDictionary)
        //{
        //    Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        //    foreach (TKey key in serializableDictionary.Keys)
        //    {
        //        dictionary.Add(key, dictionary[key]);
        //    }
        //    return dictionary;
        //}
        /// <summary>
        /// 按行读取文件
        /// </summary>
        /// <param name="path"></param>
        public static List<string> ReadFileToList(string path)
        {
            List<string> list = new List<string>();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    string strLine = sr.ReadLine();
                    list.Add(strLine);
                }
            }

            return list;
        }
        public static RemainTime CalculateTime(DateTime startTime, DateTime endTime, int index, int total)
        {
            TimeSpan ts = endTime - startTime;

            decimal avgTime = decimal.Divide((decimal)ts.TotalMilliseconds, index);
            decimal totalMilliseconds = decimal.Multiply(total - index, avgTime);//毫秒
            decimal totalHours = Math.Round(decimal.Divide(totalMilliseconds, 1000 * 60 * 60), 2);//时
            decimal totalMinutes = Math.Round(decimal.Divide(totalMilliseconds, 1000 * 60));//分
            decimal totalSeconds = Math.Round(decimal.Divide(totalMilliseconds, 1000));//秒

            RemainTime remainTime = new RemainTime();

            if (totalSeconds < 60)
            {
                remainTime.Value = totalSeconds;
                remainTime.RemainTimeType = EnumRemainTimeType.Second;
            }
            else if (totalMinutes < 60)
            {
                remainTime.Value = totalMinutes;
                remainTime.RemainTimeType = EnumRemainTimeType.Minute;
            }
            else
            {
                remainTime.Value = totalHours;
                remainTime.RemainTimeType = EnumRemainTimeType.Hour;
            }

            return remainTime;
        }

    }
}
