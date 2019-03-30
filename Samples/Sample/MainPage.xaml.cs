using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraPreview;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Vision.Framework.Xamarin.Forms;
using Xamarin.Forms;
using ZXing.Net.Xamarin.Forms;

namespace Sample
{
    public partial class MainPage : ContentPage
    {
        ScannerPage scanPage;
        public MainPage()
        {
            InitializeComponent();
        }

        async Task<bool> CheckCameraPermisstions()
        {
            var mediaPlugin = Plugin.Media.CrossMedia.Current;
            await mediaPlugin.Initialize();

            if (!mediaPlugin.IsCameraAvailable || !mediaPlugin.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return false;
            }

            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);

            if (cameraStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                cameraStatus = results[Permission.Camera];
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                return false;
            }
            return true;
        }

        async void Default_ZXing_Clicked(object sender, System.EventArgs e)
        {

            if (!await CheckCameraPermisstions())
                return;
            var overLay = new ZXingOverlay();
            var options = new ZXingOptions();
            scanPage = new ScannerPage(options, overLay);
            scanPage.OnScanResult += (result) =>
            {
                if (result is ZXingResult zResult)
                {
                    Logger.Log($"Found bar code {zResult.Text}");
                }
            };

            await Navigation.PushAsync(scanPage);
        }


        async void Default_Vision_Clicked(object sender, System.EventArgs e)
        {
            if (!await CheckCameraPermisstions())
                return;
            var options = new ScanningOptionsBase();
            scanPage = new ScannerPage(options, null);

            await Navigation.PushAsync(scanPage);
        }
    }
}
