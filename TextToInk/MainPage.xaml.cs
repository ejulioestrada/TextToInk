using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TextToInk
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Process();
            _textBox.SelectAll();
        }

        private void Process_Click(object sender, RoutedEventArgs e)
        {
            Process();
        }

        private void Process()
        {
            _inkCanvas.InkPresenter.StrokeContainer.Clear();
            _inkCanvas.InkPresenter.StrokeContainer.AddStrokes(TextToInk.CreateStrokes(_textBox.Text, _fontNameBox.Text, int.Parse(_fontSizeBox.Text)));
        }
    }
}
