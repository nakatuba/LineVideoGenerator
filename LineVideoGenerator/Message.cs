using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;

namespace LineVideoGenerator
{
    public class Message : INotifyPropertyChanged
    {
        public Person person;
        private string text;
        private int time;
        private Color color = Colors.White;
        private byte[] voice;
        private double voiceTime = 1;
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
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                OnPropertyChanged();
            }
        }
        public static Color Green
        {
            get { return Color.FromRgb(112, 222, 82); }
        }
        public byte[] Voice
        {
            get { return voice; }
            set
            {
                voice = value;
                OnPropertyChanged();
            }
        }
        public double VoiceTime
        {
            get { return voiceTime; }
            set
            {
                voiceTime = value;
                OnPropertyChanged();
            }
        }
        public bool IsSetVoice
        {
            get { return voice != null; }
        }
        public int NextMessageMinTime
        {
            get { return time + (int)Math.Ceiling(voiceTime); }
        }

        public Message() { }

        public Message(Person person, string text, int time, Canvas canvas)
        {
            this.person = person;
            this.text = text;
            this.time = time;
            if (person.id == 0) color = Green;
            AddThumb(canvas);
        }

        public void AddThumb(Canvas canvas)
        {
            thumb.Height = canvas.ActualHeight;

            Binding left = new Binding(nameof(Time));
            left.Source = this;
            left.Mode = BindingMode.TwoWay;
            left.Converter = new ThumbConverter();
            thumb.SetBinding(Canvas.LeftProperty, left);

            Binding width = new Binding(nameof(VoiceTime));
            width.Source = this;
            width.Mode = BindingMode.TwoWay;
            width.Converter = new ThumbConverter();
            thumb.SetBinding(FrameworkElement.WidthProperty, width);

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
