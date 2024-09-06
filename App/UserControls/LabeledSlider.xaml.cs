using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Web.Http.Headers;

namespace GenshinJPTextSpeaker.UserControls
{
    /// <summary>
    /// LabeledSlider.xaml の相互作用ロジック
    /// </summary>
    public partial class LabeledSlider : System.Windows.Controls.UserControl
    {
        public string Label { get => GetValue(LabelProperty)?.ToString(); set => SetValue(LabelProperty, value); }
        public static DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledSlider), new PropertyMetadata("", OnPropertyChanged));

        public double LabelWidth { get => (double)GetValue(LabelWidthProperty); set => SetValue(LabelWidthProperty, value); }
        public static DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(double), typeof(LabeledSlider), new PropertyMetadata(200.0, OnPropertyChanged));

        public string Unit { get => GetValue(UnitProperty)?.ToString(); set => SetValue(UnitProperty, value); }
        public static DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit), typeof(string), typeof(LabeledSlider), new PropertyMetadata("", OnPropertyChanged));

        public double SliderWidth { get => (double)GetValue(SliderWidthProperty); set => SetValue(SliderWidthProperty, value); }
        public static DependencyProperty SliderWidthProperty = DependencyProperty.Register(nameof(SliderWidth), typeof(double), typeof(LabeledSlider), new PropertyMetadata(200.0, OnPropertyChanged));

        public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public static DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(LabeledSlider), new PropertyMetadata(0.0, OnPropertyChanged));

        public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public static DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(LabeledSlider), new PropertyMetadata(100.0, OnPropertyChanged));

        public double Frequency { get => (double)GetValue(FrequencyProperty); set => SetValue(FrequencyProperty, value); }
        public static DependencyProperty FrequencyProperty = DependencyProperty.Register(nameof(Frequency), typeof(double), typeof(LabeledSlider), new PropertyMetadata(1.0, OnPropertyChanged));

        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public static DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(LabeledSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPropertyChanged));

        public LabeledSlider()
        {
            InitializeComponent();
        }

        public static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
