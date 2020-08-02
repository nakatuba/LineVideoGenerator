using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace LineVideoGenerator
{
    public class Message
    {
        public int personID;
        [XmlIgnore] public BitmapImage icon;
        public byte[] IconByteArray
        {
            get
            {
                if (icon != null)
                {
                    // BitmapImageからbyte[]への変換（https://stackoverflow.com/questions/6597676/bitmapimage-to-byte）
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(icon));
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        return ms.ToArray();
                    }
                }
                else
                {
                    return null;
                }
            }

            set
            {
                // byte[]からBitmapImageへの変換（https://stackoverflow.com/questions/14337071/convert-array-of-bytes-to-bitmapimage）
                using (var ms = new MemoryStream(value))
                {
                    icon = new BitmapImage();
                    icon.BeginInit();
                    icon.CacheOption = BitmapCacheOption.OnLoad;
                    icon.StreamSource = ms;
                    icon.EndInit();
                }
            }
        }
        public string name;
        public string text;

        public Message() { }

        public Message(int id, string text)
        {
            this.personID = id;
            this.text = text;
        }

        public Message(int id, BitmapImage icon, string name, string text)
        {
            this.personID = id;
            this.icon = icon;
            this.name = name;
            this.text = text;
        }
    }
}
