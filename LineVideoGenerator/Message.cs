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
        private int duration;
        private Color color;
        private byte[] voice;
        private double voiceTime = 1;
        public int timeInterval = new Random().Next(3);
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
        public int Duration
        {
            get { return duration; }
            set
            {
                duration = value;
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
        public int NextMessageDuration
        {
            get { return duration + (int)Math.Ceiling(voiceTime); }
        }

        public Message() { }

        public Message(Person person, string text, int duration, Canvas canvas)
        {
            this.person = person;
            this.text = text;
            this.duration = duration;
            color = (person.id == 0) ? Original.Green : Colors.White;
            AddThumb(canvas);
        }

        public void AddThumb(Canvas canvas)
        {
            thumb.Height = canvas.ActualHeight;
            thumb.BorderBrush = Brushes.Blue;

            Binding durationBinding = new Binding(nameof(Duration));
            durationBinding.Source = this;
            durationBinding.Mode = BindingMode.TwoWay;
            durationBinding.Converter = new ThumbConverter();
            thumb.SetBinding(Canvas.LeftProperty, durationBinding);

            Binding voiceTimeBinding = new Binding(nameof(VoiceTime));
            voiceTimeBinding.Source = this;
            voiceTimeBinding.Mode = BindingMode.TwoWay;
            voiceTimeBinding.Converter = new ThumbConverter();
            thumb.SetBinding(FrameworkElement.WidthProperty, voiceTimeBinding);

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
