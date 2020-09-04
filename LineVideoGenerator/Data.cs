using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LineVideoGenerator
{
    public class Data
    {
        public int videoTotalTime;
        public int messageStartTime = (int)DateTime.Now.TimeOfDay.TotalSeconds;
        public ObservableCollection<Message> messageCollection = new ObservableCollection<Message>();
        public List<Person> personList = new List<Person>();
    }
}
