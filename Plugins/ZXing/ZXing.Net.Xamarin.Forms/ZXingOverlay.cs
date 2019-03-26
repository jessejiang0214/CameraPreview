using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace ZXing.Net.Xamarin.Forms
{
    public class ZXingOverlay : Grid
    {
        Label topText;
        Label botText;

        public ZXingOverlay()
        {
            BindingContext = this;

            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;

            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            Children.Add(new BoxView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Black,
                Opacity = 0.7,
            }, 0, 0);

            Children.Add(new BoxView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Black,
                Opacity = 0.7,
            }, 0, 2);

            Children.Add(new BoxView
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 3,
                BackgroundColor = Color.Red,
                Opacity = 0.6,
            }, 0, 1);

            topText = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.White,
            };
            topText.SetBinding(Label.TextProperty, new Binding(nameof(TopText)));
            Children.Add(topText, 0, 0);

            botText = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.White,
            };
            botText.SetBinding(Label.TextProperty, new Binding(nameof(BottomText)));
            Children.Add(botText, 0, 2);

        }

        public static readonly BindableProperty TopTextProperty =
            BindableProperty.Create(nameof(TopText), typeof(string), typeof(ZXingOverlay), string.Empty);
        public string TopText
        {
            get { return (string)GetValue(TopTextProperty); }
            set { SetValue(TopTextProperty, value); }
        }

        public static readonly BindableProperty BottomTextProperty =
            BindableProperty.Create(nameof(BottomText), typeof(string), typeof(ZXingOverlay), string.Empty);
        public string BottomText
        {
            get { return (string)GetValue(BottomTextProperty); }
            set { SetValue(BottomTextProperty, value); }
        }
    }
}

