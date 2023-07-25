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
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Media;

namespace TextToInk
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private IEnumerable<Point>? _points = null; // for rendering stokes as collections of points
        private readonly InkAnalyzer _ia = new();   // for recognizing ink as text
        private CancellationTokenSource? _cts;      // for canceling rendering if needed
        private InkDrawingAttributes _attrs = new();// for rendering ink/text using the same color as the original strokes

        private void Process_Click(object sender, RoutedEventArgs e)
        {
            _cts = new();   // to cancel rendering if needed (eg clear button)
            Process();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        /// <summary>
        /// Clear all the ink and text.
        /// </summary>
        private void Clear()
        {
            _cts?.Cancel();
            _inputCanvas.InkPresenter.StrokeContainer.Clear();
            _outputCanvas.InkPresenter.StrokeContainer.Clear();
            _textBlock.Text = "";
            _points = null;
            _canvas2d.Invalidate();
        }

        /// <summary>
        /// Convert the ink drawn by the user (input canvas) to text and then back to ink (output canvas)
        /// using the given font name and font size.
        /// </summary>
        private async void Process()
        {
            var fontSize = int.Parse(_fontSizeBox.Text);
            _attrs = _inputCanvas.InkPresenter.CopyDefaultDrawingAttributes();

            // what the user drew
            var originalStrokes = _inputCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // align content to where the user drew it
            var rect = _inputCanvas.InkPresenter.StrokeContainer.BoundingRect;
            var offset = new Vector3((float)rect.X, (float)rect.Y, 0);

            // convert the ink to text
            var text = await GetTextAsync(originalStrokes);
            _textBlock.Text = text;
            _textBlock.FontFamily = new(_fontNameBox.Text);
            _textBlock.FontSize = fontSize;
            _textBlock.Foreground = new SolidColorBrush(_attrs.Color);
            _textBlock.Translation = new Vector3((float)rect.X, 0, 0);

            // convert the text back to ink using the given font and size
            var (strokes, points) = TextToInk.CreateStrokes(_textBlock.Text, _fontNameBox.Text, fontSize, _attrs);
            _outputCanvas.InkPresenter.StrokeContainer.AddStrokes(strokes);
            _outputCanvas.Translation = offset;

            // animate the stroke points
            _canvas2d.Translation = offset;
            var interPointDelay = TimeSpan.FromMilliseconds(25);
            var pts = points.ToList();
            for (var i = 0; i < pts.Count; i++)
            {
                if (_cts?.Token.IsCancellationRequested == true) break;
                _points = pts.Take(i + 1);
                _canvas2d.Invalidate();
                try
                {
                    // pause momentarily to show the points being drawn
                    await Task.Delay(interPointDelay);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<string> GetTextAsync(IEnumerable<InkStroke> strokes)
        {
            _ia.ClearDataForAllStrokes();
            _ia.AddDataForStrokes(strokes);
            await _ia.AnalyzeAsync();
            return _ia.AnalysisRoot.RecognizedText;
        }

        private void OnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_points == null)
            {
                return;
            }
            using var ds = args.DrawingSession;
            // ds.Transform = Matrix3x2.CreateScale(2f); // uncomment to scale up
            var pts = _points.Select(p => p.ToVector2()).ToList();
            for (var i = 0; i < pts.Count; i++)
            {
                ds.DrawCircle(pts[i], .1f, _attrs.Color);
                if (i > 0)
                {
                    ds.DrawLine(pts[i - 1], pts[i], Colors.LightGray, .25f);
                }
            }
        }
    }
}
