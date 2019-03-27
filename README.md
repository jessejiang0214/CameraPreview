# CameraPreview
CameraPreview project aims to help Xamarin developer to get camera preview more easily. Inspired by [ZXing.Net.Mobile](https://github.com/Redth/ZXing.Net.Mobile), I design a plugins system, based on this system you can add your own plugins easily, like scanning bar code, OCR, and some AI features plugins.

Also, you can create your own overlay above camera view with Xamarin.Froms.

[![Build Status](https://dev.azure.com/Jesse0131/CameraPreview/_apis/build/status/jessejiang0214.CameraPreview?branchName=master)](https://dev.azure.com/Jesse0131/CameraPreview/_build/latest?definitionId=2&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/CameraPreview.svg)](https://www.nuget.org/packages/CameraPreview/)
[![NuGet](https://img.shields.io/nuget/dt/CameraPreview.svg)](https://www.nuget.org/packages/CameraPreview/)

## Usage

The Camera surface will be rendered on ```ScannerView```. 

For the settings, you can inherit from ```ScanningOptionsBase``` to add your own settings. 

The result will come from ```OnScanResult``` event or ```ScanResultCommand``` Command. 

You can use ```IsScanning``` to control scanning.

## Setup
This project assumes you already get the permission of the camera and init the Camera, so if you didn't get the permission of the camera, it will throw an exception. You can use [MediaPlugin](https://github.com/jamesmontemagno/MediaPlugin) to help you get the permissions.

Both iOS and Android you should call init function before Xamarin init.
```
CameraPreview.iOS.CameraPreviewSettings.Instance.Init(null);
Or
CameraPreview.Droid.CameraPreviewSettings.Instance.Init(null);
```


If you used plugins, you should call plugins' init function instead of this.

## Diagnostics
Implement the ```ILogger``` interface to create your own logger and set like this
```
    public class LoggerImplement : ILogger
    {
        public void Log(string message, LogLevel level)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

Logger.Instance.LoggerObj = new LoggerImplement();
```

You can use ```PerformanceCounter``` to counter the function running time
```
var decoder = PerformanceCounter.Start();
PerformanceCounter.Stop(decoder, "ZXing Decoder take {0} ms.");
```

## Plugins
It's very easy to create a plugin for CameraPreview, there's a demo to use ZXing.Net in ```Plugins/ZXing```

In ZXing.Net.Xamarin.Forms project, you can define your own options, result and overlay

In iOS and Droid, you need to create your own decoder, you need to override ```DefaultDecoderBase``` class, override Decode function to decode the image when the image comes from preview callback 
```
        // For iOS
        public override IScanResult Decode(CVPixelBuffer pixelBuffer)
        {
        }

        // For Android
        public override IScanResult Decode(Bitmap image)
        {
        }        
```

ScanningOptionsUpdate function will be called when ```IsScanning``` set to true.
```
public override void ScanningOptionsUpdate(ScanningOptionsBase options)
```

After you finish the decoder, you need to init ```CameraPreviewSettings``` in your own init function like 
```
    public class CameraPreviewSettingsForZXing
    {
        public static void Init()
        {
            CameraPreviewSettings.Instance.Init(new ZXingDecoder());
        }
    }
```
And call this instead of ```CameraPreviewSettings.Instance.Init(null);```

## Thanks
CameraPreview project is based on [ZXing.Net.Mobile](https://github.com/Redth/ZXing.Net.Mobile), [ZXing.Net](https://github.com/micjahn/ZXing.Net) and [ZXing](https://github.com/zxing/zxing), thanks for them.