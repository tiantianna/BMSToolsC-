using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace bms.startup.util
{
    public class Serializer
    {
        //将类型序列化为字符串
        public static string Serialize<T>(T t) where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, t);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        //将类型序列化为文件
        public static void SerializeToFile<T>(T t, string path, string fullName) where T : class
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fullPath = string.Format(@"{0}\{1}", path, fullName);
            using (FileStream stream = new FileStream(fullPath, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, t);
                stream.Flush();
            }
        }

        //将类型序列化为文件
        public static void SerializeToFileByXml<T>(T t, string path, string fullName) where T : class
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = string.Format(@"{0}\{1}", path, fullName);

            using (FileStream stream = new FileStream(fullPath, FileMode.OpenOrCreate))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                formatter.Serialize(stream, t);
                stream.Flush();
            }
        }
        //将字符串反序列化为类型
        public static TResult Deserialize<TResult>(string s) where TResult : class
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(s);
            using (MemoryStream stream = new MemoryStream(bs))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream) as TResult;
            }
        }

        //将文件反序列化为类型
        public static TResult DeserializeFromFile<TResult>(string path) where TResult : class
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream) as TResult;
            }
        }

        //将xml文件反序列化为类型
        public static TResult DeserializeFromFileByXml<TResult>(string path) where TResult : class
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(TResult)); ;
                return formatter.Deserialize(stream) as TResult;
            }
        }
    }
}
