#nullable enable
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using System.Collections.Generic;
using Windows.UI;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

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
            Process(_inkCanvas.InkPresenter);
            _textBox.SelectAll();
        }

        private IEnumerable<Point>? _points = null;
        private TimeSpan _interPointDelay = TimeSpan.FromMilliseconds(100);

        private void Process_Click(object sender, RoutedEventArgs e)
        {
            Process(_inkCanvas.InkPresenter);
        }

        private async void Process(InkPresenter presenter)
        {
            presenter.StrokeContainer.Clear();
            var attrs = presenter.CopyDefaultDrawingAttributes();
            var (strokes, points) = TextToInk.CreateStrokes(_textBox.Text, _fontNameBox.Text, int.Parse(_fontSizeBox.Text), attrs);
            presenter.StrokeContainer.AddStrokes(strokes);
            var pts = points.ToList();
            for (var i = 0; i < pts.Count; i++)
            {
                _points = pts.Take(i + 1);
                _canvas2d.Invalidate();
                await Task.Delay(_interPointDelay);
            }
        }

        private void OnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_points == null) return;
            using var ds = args.DrawingSession;
            ds.Transform = Matrix3x2.CreateScale(3f);
            var pts = _points.Select(p => p.ToVector2()).ToList();
            for (var i = 0; i < pts.Count; i++)
            {
                ds.DrawCircle(pts[i], .1f, Colors.Purple);
                if (i > 0)
                {
                    ds.DrawLine(pts[i - 1], pts[i], Colors.LightGray, .25f);
                }
            }
        }
    }
}
