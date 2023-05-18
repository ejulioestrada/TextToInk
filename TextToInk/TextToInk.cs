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
    internal class TextToInk
    {
        public static IEnumerable<InkStroke> CreateStrokes(string text, string fontFamily = "Ink Free", int fontSize = 36, InkDrawingAttributes? attrs = null)
        {
            using var textFormat = new CanvasTextFormat
            {
                FontSize = fontSize,
                WordWrapping = CanvasWordWrapping.NoWrap,
                FontFamily = fontFamily,
            };
            attrs ??= new();
            var size = Vector2.Zero;
            var textToStrokesRenderer = new TextToStrokesRenderer(attrs);
            using var textLayout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), text, textFormat, size.X, size.Y);
            textLayout.DrawToTextRenderer(textToStrokesRenderer, 10, 10);
            return textToStrokesRenderer.Strokes;
        }

        private class TextToStrokesRenderer : ICanvasTextRenderer
        {
            public IEnumerable<InkStroke> Strokes => _receiver.Strokes;

            public TextToStrokesRenderer(InkDrawingAttributes attrs)
            {
                _receiver = new(attrs);
            }

            public void DrawGlyphRun(
                Vector2 point,
                CanvasFontFace fontFace,
                float fontSize,
                CanvasGlyph[] glyphs,
                bool isSideways,
                uint bidiLevel,
                object brush,
                CanvasTextMeasuringMode measuringMode,
                string localeName,
                string textString,
                int[] clusterMapIndices,
                uint characterIndex,
                CanvasGlyphOrientation glyphOrientation)
            {
                using var geometry = CanvasGeometry.CreateGlyphRun(null, point, fontFace, fontSize, glyphs, isSideways, bidiLevel, measuringMode, glyphOrientation)
                                                   .Simplify(CanvasGeometrySimplification.Lines);
                geometry.SendPathTo(_receiver);
            }

            public void DrawStrikethrough(Vector2 point,
                                          float strikethroughWidth,
                                          float strikethroughThickness,
                                          float strikethroughOffset,
                                          CanvasTextDirection textDirection,
                                          object brush,
                                          CanvasTextMeasuringMode textMeasuringMode,
                                          string localeName,
                                          CanvasGlyphOrientation glyphOrientation)
            {
                throw new NotImplementedException();
            }
            public void DrawUnderline(Vector2 point,
                                      float underlineWidth,
                                      float underlineThickness,
                                      float underlineOffset,
                                      float runHeight,
                                      CanvasTextDirection textDirection,
                                      object brush,
                                      CanvasTextMeasuringMode textMeasuringMode,
                                      string localeName,
                                      CanvasGlyphOrientation glyphOrientation)
            {
                throw new NotImplementedException();
            }
            public void DrawInlineObject(Vector2 point,
                                         ICanvasTextInlineObject inlineObject,
                                         bool isSideways,
                                         bool isRightToLeft,
                                         object brush,
                                         CanvasGlyphOrientation glyphOrientation)
            {
                throw new NotImplementedException();
            }
            public float Dpi => throw new NotImplementedException();
            public bool PixelSnappingDisabled => true;
            public Matrix3x2 Transform => throw new NotImplementedException();

            // Private implementation
            //
            private readonly CanvasPathToStrokesReceiver _receiver;
        }

        private class CanvasPathToStrokesReceiver : ICanvasPathReceiver
        {
            public List<InkStroke> Strokes { get; } = new();

            public CanvasPathToStrokesReceiver(InkDrawingAttributes attrs)
            {
                _attrs = attrs;
            }

            public void BeginFigure(Vector2 startPoint, CanvasFigureFill figureFill)
            {
                _points.Clear();
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
                    AddPoint(_points[0].ToVector2());
                }

                var builder = new InkStrokeBuilder();
                builder.SetDefaultDrawingAttributes(_attrs);
                var stroke = builder.CreateStroke(_points);
                Strokes.Add(stroke);

                System.Diagnostics.Debug.WriteLine($"strokes:{Strokes.Count} stroke segments:{stroke.GetRenderingSegments().Count} points:{_points.Count}");
            }

            public void SetFilledRegionDetermination(CanvasFilledRegionDetermination filledRegionDetermination) { }
            public void SetSegmentOptions(CanvasFigureSegmentOptions figureSegmentOptions) { }

            // Private implementation
            //
            private InkDrawingAttributes _attrs;
            private readonly List<Point> _points = new();
            private void AddPoint(Vector2 v) => _points.Add(v.ToPoint(precision: 2));
        }
    }

    internal static class TextToInkExtensions
    {
        public static Vector2 ToVector2(this Point p) => new((float)p.X, (float)p.Y);
        public static Point ToPoint(this Vector2 v, int precision) => new(Math.Round(v.X, precision), Math.Round(v.Y, precision));
    }
}
