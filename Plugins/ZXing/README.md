# CameraPreview.Plugins.ZXing
This is a plugins for [CameraPrview](https://github.com/jessejiang0214/CameraPreview), which use ZXing.Net to scan the barcode.

[![Build Status](https://dev.azure.com/Jesse0131/CameraPreview/_apis/build/status/jessejiang0214.CameraPreview.Plugins.ZXing?branchName=master)](https://dev.azure.com/Jesse0131/CameraPreview/_build/latest?definitionId=3&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/CameraPreview.Plugins.ZXing.svg)](https://www.nuget.org/packages/CameraPreview.Plugins.ZXing)
[![NuGet](https://img.shields.io/nuget/dt/CameraPreview.Plugins.ZXing.svg)](https://www.nuget.org/packages/CameraPreview.Plugins.ZXing)

## Usage
Check this [Sample](https://github.com/jessejiang0214/CameraPreview/tree/master/Samples) to use it.

You can use default overly ```ZXingOverlay``` or create your own one.

## Setup
This project assumes you already get the permission of the camera and init the Camera, so if you didn't get the permission of the camera, it will throw an exception. You can use [MediaPlugin](https://github.com/jamesmontemagno/MediaPlugin) to help you get the permissions.

You need to install the Nuget for all projects

Both iOS and Android you should call init function before Xamarin init.
```
ZXing.Net.iOS.CameraPreviewSettingsForZXing.Init();
Or
ZXing.Net.Droid.CameraPreviewSettingsForZXing.Init();
```

## Thanks
CameraPreview project is based on [ZXing.Net.Mobile](https://github.com/Redth/ZXing.Net.Mobile), [ZXing.Net](https://github.com/micjahn/ZXing.Net) and [ZXing](https://github.com/zxing/zxing), thanks for them.
