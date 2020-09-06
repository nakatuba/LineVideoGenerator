using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LineVideoGenerator
{
    public class Data
    {
        public int videoTotalTime;
        public int messageStartTime = (int)DateTime.Now.TimeOfDay.TotalSeconds;
        public byte[] bgm;
        public byte[] soundEffect;
        public ObservableCollection<Message> messageCollection = new ObservableCollection<Message>();
        public List<Person> personList = new List<Person>();
    }
}
