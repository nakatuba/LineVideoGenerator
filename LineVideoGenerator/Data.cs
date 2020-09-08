using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
