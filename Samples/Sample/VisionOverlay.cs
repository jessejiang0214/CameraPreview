using System;
using System.Collections.Generic;
using CameraPreview;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Vision.Framework.Xamarin.Forms;
using Xamarin.Forms;

namespace Sample
{
    //I was going to add Overlay in Vision.Framework.Xamarin.Forms project
    //But I realized not all developers need this 
    //And it will reference Drawing library like SkiaSharp etc, it will make project more heavy.
    public class VisionOverlay : ContentView
    {
        IList<BarCodeResult> _result;
        SKCanvasView _canvasView;
        int _imageWidth;
        int _imageHeight;
        public VisionOverlay()
        {
            BackgroundColor = Color.Transparent;
            Padding = new Thickness(0);
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += CanvasView_PaintSurface;
            Content = _canvasView;
        }

        void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKSurface surface = args.Surface;
            var canvas = surface.Canvas;

            IList<BarCodeResult> results = null;
            lock (resultLockObj)
            {
                if (_result != null)
                    results = new List<BarCodeResult>(_result);
            }
            canvas.Clear();

            if (results == null)
                return;

            bool sameOrientation = false;
            if ((args.Info.Height > args.Info.Width && _imageHeight > _imageWidth)
            || (args.Info.Height < args.Info.Width && _imageHeight < _imageWidth))
                sameOrientation = true;
                
            foreach (var barcode in results)
            {
                SKPaint border = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 5,
                    Color = SKColors.Red
                };

                float x, y, width, height;
                if (sameOrientation)
                {
                    float scaleX = (float)args.Info.Width / _imageWidth;
                    float scaleY = (float)args.Info.Height / _imageHeight;
                    x = (float)barcode.X * scaleX;
                    y = (float)barcode.Y * scaleY;
                    width = (float)barcode.Width * scaleX;
                    height = (float)barcode.Height * scaleY;
                }
                else
                {
                    float scaleX = (float)args.Info.Width / _imageHeight;
                    float scaleY = (float)args.Info.Height / _imageWidth;
                    x = (float)barcode.Y * scaleX;
                    y = (float)barcode.X * scaleY;
                    width = (float)barcode.Height * scaleX;
                    height = (float)barcode.Width * scaleY;
                }
                Logger.Log($"Drawing postion {x} {y} {width} {height}");
                canvas.DrawRect(x, y, width, height, border);

                SKPaint textPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3,
                    FakeBoldText = true,
                    Color = SKColors.Red,
                    TextSize = 30,
                };

                SKRect textBounds = new SKRect();
                textPaint.MeasureText(barcode.Text, ref textBounds);

                canvas.DrawText(barcode.Text, x - textBounds.MidX, y + height + 20, textPaint);
            }
        }


        object resultLockObj = new object();

        public void GetScanResult(IScanResult scanResult)
        {
            if (scanResult is VisionBarCodeResult barCodeResult)
            {
                lock (resultLockObj)
                {
                    _result = barCodeResult.Results;
                    _imageWidth = barCodeResult.ImageWidth;
                    _imageHeight = barCodeResult.ImageHeight;
                }
                Device.BeginInvokeOnMainThread(_canvasView.InvalidateSurface);
            }

        }
    }
}

