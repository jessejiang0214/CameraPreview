using Xamarin.Forms;

namespace ZXing.Net.Xamarin.Forms
{
    public class ZXingOverlay : Grid
    {
        private readonly Label _topText;
        private readonly Label _botText;

        public ZXingOverlay()
        {
            BindingContext = this;

            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;

            RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
            RowDefinitions.Add(new RowDefinition {Height = new GridLength(2, GridUnitType.Star)});
            RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
            ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});


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

            _topText = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.White,
            };
            _topText.SetBinding(Label.TextProperty, new Binding(nameof(TopText)));
            Children.Add(_topText, 0, 0);

            _botText = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.White,
            };
            _botText.SetBinding(Label.TextProperty, new Binding(nameof(BottomText)));
            Children.Add(_botText, 0, 2);

        }

        public static readonly BindableProperty TopTextProperty =
            BindableProperty.Create(nameof(TopText), typeof(string), typeof(ZXingOverlay), string.Empty);

        public string TopText
        {
            get => (string) GetValue(TopTextProperty);
            set => SetValue(TopTextProperty, value);
        }

        public static readonly BindableProperty BottomTextProperty =
            BindableProperty.Create(nameof(BottomText), typeof(string), typeof(ZXingOverlay), string.Empty);

        public string BottomText
        {
            get => (string) GetValue(BottomTextProperty);
            set => SetValue(BottomTextProperty, value);
        }
    }
}