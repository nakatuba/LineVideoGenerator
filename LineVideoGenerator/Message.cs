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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace LineVideoGenerator
{
    public class Message : INotifyPropertyChanged
    {
        public int personID = 0;
        [XmlIgnore] public BitmapImage icon;
        private string text;
        private double time;
        private byte[] voice;
        public string voicePathExt;
        public WitMultiRangeSlider slider;
        public WitMultiRangeSliderItem sliderItem = new WitMultiRangeSliderItem();
        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] Icon
        {
            get
            {
                if (icon != null)
                {
                    // BitmapImageからbyte[]に変換（https://stackoverflow.com/questions/6597676/bitmapimage-to-byte）
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
                // byte[]からBitmapImageに変換（https://stackoverflow.com/questions/14337071/convert-array-of-bytes-to-bitmapimage）
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
        public string Name { get; set; }
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }
        public double Time
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

        public Message(string text, double time, WitMultiRangeSlider slider)
        {
            this.text = text;
            this.time = time;
            this.slider = slider;
            slider.Items.Add(sliderItem);
        }

        public Message(int personID, BitmapImage icon, string name, string text, double time, WitMultiRangeSlider slider)
        {
            this.personID = personID;
            this.icon = icon;
            Name = name;
            this.text = text;
            this.time = time;
            this.slider = slider;
            slider.Items.Add(sliderItem);
        }

        public void PlayVoice(Action stoppedAction = null)
        {
            if (IsSetVoice)
            {
                string voicePath = "voice" + voicePathExt;
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

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
