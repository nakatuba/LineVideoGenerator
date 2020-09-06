using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace LineVideoGenerator
{
    public class Message : INotifyPropertyChanged
    {
        public Person person;
        private string text;
        private int time;
        public byte[] voice;
        public double voiceTime;
        [XmlIgnore] public Thumb thumb = new Thumb();
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name
        {
            get { return person.Name; }
        }
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }
        public int Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }
        public int NextMessageMinTime
        {
            get { return time + (int)voiceTime + 1; }
        }
        public bool IsSetVoice
        {
            get { return voice != null; }
        }

        public Message() { }

        public Message(Person person, string text, int time, Canvas canvas)
        {
            this.person = person;
            this.text = text;
            this.time = time;
            AddThumb(canvas);
        }

        public void SetVoice(string fileName)
        {
            voice = Global.GetByteArray(fileName);
            AudioFileReader audioFileReader = new AudioFileReader(fileName);
            voiceTime = audioFileReader.TotalTime.TotalSeconds;
            thumb.Width = voiceTime * ThumbConverter.per;
            OnPropertyChanged();
        }

        public void ResetVoice()
        {
            voice = null;
            thumb.Width = ThumbConverter.per;
            OnPropertyChanged();
        }

        public void AddThumb(Canvas canvas)
        {
            thumb.Width = ThumbConverter.per;
            thumb.Height = canvas.ActualHeight;
            Binding binding = new Binding(nameof(Time));
            binding.Source = this;
            binding.Mode = BindingMode.TwoWay;
            binding.Converter = new ThumbConverter();
            thumb.SetBinding(Canvas.LeftProperty, binding);
            canvas.Children.Add(thumb);
        }

        public void RemoveThumb()
        {
            Canvas canvas = thumb.Parent as Canvas;
            canvas.Children.Remove(thumb);
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
