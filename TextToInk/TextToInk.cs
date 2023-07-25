#nullable enable
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace TextToInk
{
    /// <summary>
    /// Converts text to ink strokes using a given font and font size.
    /// Uses win2d geometry to create the strokes.
    /// </summary>
    internal class TextToInk
    {
        public static (IEnumerable<InkStroke> strokes, IEnumerable<Point> points) CreateStrokes(string text, string fontFamily = "Ink Free", int fontSize = 36, InkDrawingAttributes? attrs = null,
                                                                                                Action<Point, IEnumerable<Point>>? pointCallback = null)
        {
            using var textFormat = new CanvasTextFormat
            {
                FontSize = fontSize,
                WordWrapping = CanvasWordWrapping.NoWrap,
                FontFamily = fontFamily,
            };
            attrs ??= new();
            var size = Vector2.Zero;
            using var textLayout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), text, textFormat, size.X, size.Y);
            var geometry = CanvasGeometry.CreateText(textLayout).Simplify(CanvasGeometrySimplification.Lines);
            var receiver = new CanvasPathToStrokesReceiver(attrs, pointCallback);
            geometry.SendPathTo(receiver);
            return (receiver.Strokes, receiver.Points);
        }

        private class CanvasPathToStrokesReceiver : ICanvasPathReceiver
        {
            public List<InkStroke> Strokes { get; } = new();
            public List<Point> Points { get; } = new();

            public CanvasPathToStrokesReceiver(InkDrawingAttributes attrs, Action<Point, IEnumerable<Point>>? pointCallback = null)
            {
                _attrs = attrs;
                _pointCallback = pointCallback;
            }

            public void BeginFigure(Vector2 startPoint, CanvasFigureFill figureFill)
            {
                _figurePoints.Clear();
                AddPoint(startPoint);
            }

            public void AddArc(Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize)
            {
                throw new NotImplementedException();
            }
            public void AddCubicBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint) => throw new NotImplementedException();
            public void AddLine(Vector2 endPoint) => AddPoint(endPoint);
            public void AddQuadraticBezier(Vector2 controlPoint, Vector2 endPoint) => throw new NotImplementedException();

            public void EndFigure(CanvasFigureLoop figureLoop)
            {
                if (figureLoop == CanvasFigureLoop.Closed)
                {
                    AddPoint(_figurePoints[0].ToVector2());
                }

                var builder = new InkStrokeBuilder();
                builder.SetDefaultDrawingAttributes(_attrs);
                var stroke = builder.CreateStroke(_figurePoints);
                Strokes.Add(stroke);
                Points.AddRange(_figurePoints);
            }

            public void SetFilledRegionDetermination(CanvasFilledRegionDetermination filledRegionDetermination) { }
            public void SetSegmentOptions(CanvasFigureSegmentOptions figureSegmentOptions) { }

            // Private implementation
            //
            private readonly Action<Point, IEnumerable<Point>>? _pointCallback;
            private readonly InkDrawingAttributes _attrs;
            private readonly List<Point> _figurePoints = new();

            private void AddPoint(Vector2 v)
            {
                var p = v.ToPoint(precision: 2);
                _figurePoints.Add(p);
                _pointCallback?.Invoke(p, _figurePoints);
            }
        }
    }

    internal static class TextToInkExtensions
    {
        public static Vector2 ToVector2(this Point p) => new((float)p.X, (float)p.Y);
        public static Point ToPoint(this Vector2 v, int precision) => new(Math.Round(v.X, precision), Math.Round(v.Y, precision));
    }
}
