using InWit.WPF.MultiRangeSlider;
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
        private byte[] voice;
        public string voicePathExt;
        [XmlIgnore] private WitMultiRangeSliderItem sliderItem = new WitMultiRangeSliderItem();
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
        public byte[] Voice
        {
            get { return voice; }
            set
            {
                voice = value;
                OnPropertyChanged();
            }
        }
        public bool IsSetVoice
        {
            get { return voice != null; }
        }

        public Message() { }

        public Message(Person person, string text, int time, WitMultiRangeSlider slider)
        {
            this.person = person;
            this.text = text;
            this.time = time;
            AddSliderItem(slider);
        }

        public void PlayVoice(Action stoppedAction = null)
        {
            if (IsSetVoice)
            {
                string voicePath = Guid.NewGuid() + voicePathExt;
                File.WriteAllBytes(voicePath, voice);
                AudioFileReader audioFileReader = new AudioFileReader(voicePath);
                WaveOut waveOut = new WaveOut();
                waveOut.Init(audioFileReader);
                waveOut.Play();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    audioFileReader.Dispose();
                    File.Delete(voicePath);
                    stoppedAction?.Invoke();
                };
            }
        }

        public void AddSliderItem(WitMultiRangeSlider slider)
        {
            slider.Items.Add(sliderItem);
            Binding binding = new Binding("Time") { Source = this };
            sliderItem.SetBinding(RangeBase.ValueProperty, binding);
        }

        public void RemoveSliderItem(WitMultiRangeSlider slider)
        {
            slider.Items.Remove(sliderItem);
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
