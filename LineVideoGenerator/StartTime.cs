using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace LineVideoGenerator
{
    public class StartTime : INotifyPropertyChanged
    {
        private int duration;
        private DateTime time = DateTime.Now;
        [XmlIgnore] public Thumb thumb = new Thumb();
        public event PropertyChangedEventHandler PropertyChanged;
        public int Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged();
            }
        }
        public DateTime Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }

        public StartTime() { }

        public StartTime(int duration, Canvas canvas, ContextMenu menu)
        {
            this.duration = duration;
            AddThumb(canvas, menu);
        }

        public void AddThumb(Canvas canvas, ContextMenu menu)
        {
            FrameworkElementFactory timePicker = new FrameworkElementFactory(typeof(TimePicker));
            timePicker.SetValue(FrameworkElement.HeightProperty, canvas.ActualHeight);
            timePicker.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            // timePicker.SetValue(InputBase.AllowTextInputProperty, false);
            timePicker.SetValue(TimePicker.CurrentDateTimePartProperty, DateTimePart.Minute);
            timePicker.SetValue(DateTimePickerBase.ShowDropDownButtonProperty, false);

            Binding timeBinding = new Binding(nameof(Time));
            timeBinding.Source = this;
            timeBinding.Mode = BindingMode.TwoWay;
            timePicker.SetBinding(TimePicker.ValueProperty, timeBinding);

            FrameworkElementFactory line = new FrameworkElementFactory(typeof(Line));
            line.SetValue(Line.Y2Property, (canvas.Parent as FrameworkElement).ActualHeight);
            line.SetValue(Shape.StrokeProperty, Brushes.Red);
            line.SetValue(Shape.StrokeThicknessProperty, 4.0);

            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            grid.AppendChild(timePicker);
            grid.AppendChild(line);

            ControlTemplate template = new ControlTemplate(typeof(Thumb));
            template.VisualTree = grid;
            thumb.Template = template;

            Binding durationBinding = new Binding(nameof(Duration));
            durationBinding.Source = this;
            durationBinding.Mode = BindingMode.TwoWay;
            durationBinding.Converter = new ThumbConverter();
            thumb.SetBinding(Canvas.LeftProperty, durationBinding);

            thumb.ContextMenu = menu;

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
