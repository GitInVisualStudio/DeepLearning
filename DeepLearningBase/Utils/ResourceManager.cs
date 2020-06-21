using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DeepLearningBase.Utils
{
    public class ResourceManager
    {

        public static void Serialize<T>(T t, string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(t.GetType());
            using(Stream writer = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xmlSerializer.Serialize(writer, t);
                writer.Close();
            }
        }

        public static T Deserialize<T>(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream writer = new FileStream(path, FileMode.Open))
                return (T)xmlSerializer.Deserialize(writer);
        }

    }
}
