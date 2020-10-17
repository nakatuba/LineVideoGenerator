using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LineVideoGenerator
{
    public class Data
    {
        public int videoTotalTime;
        public byte[] bgm;
        public byte[] soundEffect;
        public ObservableCollection<Message> messageCollection = new ObservableCollection<Message>();
        public List<Person> personList = new List<Person>();
        public List<StartTime> startTimeList = new List<StartTime>();
    }
}
