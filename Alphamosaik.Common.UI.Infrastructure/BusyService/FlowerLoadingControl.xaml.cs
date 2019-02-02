using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Alphamosaik.Common.UI.Infrastructure
{
	public partial class FlowerLoadingControl : UserControl
	{
	    #region Constructors

	    public FlowerLoadingControl()
	    {
	        // Required to initialize variables
	        InitializeComponent();
            this.ShowStoryboardAnimation = this.ShowStoryboard; // Also part of the Visibility Pattern
            this.HideStoryboardAnimation = this.HideStoryboard; // Also part of the Visibility Pattern
	        Loaded += FlowerLoadingControl_Loaded;
	    }

	    #endregion

        #region Visibility Pattern

        private Visibility visibility;
	    public new Visibility Visibility
	    {
	        get { return visibility; }
            set
            {
                if (visibility != value)
                {
                    visibility = value;
                    OnVisibilityChanged();
                }
            }
	    }

	    public event EventHandler VisibilityChanged;
	    protected void OnVisibilityChanged()
	    {
	        if (visibility == Visibility.Visible)
	        {
	            base.Visibility = this.Visibility;
	            if (this.ShowStoryboardAnimation != null)
                    this.ShowStoryboardAnimation.Begin();
	        }
	            else
	        {
                if (this.HideStoryboardAnimation != null)
                    this.HideStoryboardAnimation.Begin();
                else
                    base.Visibility = this.Visibility;
	        }
            if (this.VisibilityChanged != null)
                this.VisibilityChanged(this, new EventArgs());
	    }


	    private Storyboard hideStoryboardAnimation;
	    protected Storyboard HideStoryboardAnimation
	    {
	        get { return hideStoryboardAnimation; }
	        set
	        {
                if (hideStoryboardAnimation != value)
                {
                    if (this.hideStoryboardAnimation != null)
                        this.hideStoryboardAnimation.Completed -= new EventHandler(HideStoryboardAnimation_Completed);
                    if (value != null)
                    {
                        this.hideStoryboardAnimation = value;
                        this.hideStoryboardAnimation.Completed += new EventHandler(HideStoryboardAnimation_Completed);
                        
                    }
                }
	        }
	    }

        private void HideStoryboardAnimation_Completed(object sender, EventArgs e)
        {
            base.Visibility = this.Visibility;
        }

	    protected Storyboard ShowStoryboardAnimation { get; set; }

        #endregion 

	    #region Event Handlers

	    private void FlowerLoadingControl_Loaded(object sender, RoutedEventArgs e)
	    {
            //this.DataContext = this;
            // (Application.Current.Resources["BaseColorBrush"] as SolidColorBrush).SetValue(SolidColorBrush.ColorProperty, PetalBrush); // nope
            LoadingStoryboard.Begin();
	    }

	    #endregion

	    #region PetalBrush Dependency Property

	    public Brush PetalBrush 
	    { 
	        get { return (Brush) GetValue(PetalBrushProperty); }
	        set { SetValue(PetalBrushProperty, value); }
	    }

	    public static readonly DependencyProperty PetalBrushProperty =
	        DependencyProperty.Register(
	            "PetalBrush", 
	            typeof(Brush), 
	            typeof(FlowerLoadingControl), 
	            new PropertyMetadata(new SolidColorBrush{Color = Color.FromArgb(255, 16, 73, 120)}, new PropertyChangedCallback(OnPetalBrushChanged)));

	    static void OnPetalBrushChanged(object sender, DependencyPropertyChangedEventArgs args)
	    {
            FlowerLoadingControl source = (FlowerLoadingControl)sender;
            Brush newBrush = (Brush)args.NewValue;

	        source.centerCircle.Fill = newBrush;
            source.ellipse0.Fill = newBrush;
            source.ellipse1.Fill = newBrush;
            source.ellipse2.Fill = newBrush;
            source.ellipse3.Fill = newBrush;
            source.ellipse4.Fill = newBrush;
            source.ellipse5.Fill = newBrush;
            source.ellipse6.Fill = newBrush;
            source.ellipse7.Fill = newBrush;
            source.ellipse8.Fill = newBrush;
            source.ellipse9.Fill = newBrush;
            source.ellipse10.Fill = newBrush;
            source.ellipse11.Fill = newBrush;
            source.ellipse12.Fill = newBrush;
            source.ellipse13.Fill = newBrush;
            source.ellipse14.Fill = newBrush;
            source.ellipse15.Fill = newBrush;
            //TODO: PAPA - huh? Why do I need to do this? The DP should be working!!!
	    }

        #endregion

        #region FontBrush Dependency Property

        public Brush FontBrush 
	    {
            get { return (Brush)GetValue(FontBrushProperty); }
            set { SetValue(FontBrushProperty, value); }
	    }

        public static readonly DependencyProperty FontBrushProperty =
	        DependencyProperty.Register(
                "FontBrush", 
	            typeof(Brush), 
	            typeof(FlowerLoadingControl),
                new PropertyMetadata(new SolidColorBrush { Color = Color.FromArgb(255, 255, 255, 255) }, new PropertyChangedCallback(OnFontBrushChanged)));

	    static void OnFontBrushChanged(object sender, DependencyPropertyChangedEventArgs args)
	    {
            FlowerLoadingControl source = (FlowerLoadingControl)sender;
            Brush newBrush = (Brush)args.NewValue;

            source.caption.Foreground = newBrush;
            //TODO: PAPA - huh? Why do I need to do this? The DP should be working!!!
	    }

	    #endregion

        #region Caption Dependency Property

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(
                "Caption",
                typeof(string),
                typeof(FlowerLoadingControl),
                new PropertyMetadata("Loading ...", new PropertyChangedCallback(OnCaptionChanged)));

	    static void OnCaptionChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            FlowerLoadingControl source = (FlowerLoadingControl)sender;
            string newCaption= (string)args.NewValue;

            source.caption.Text = newCaption;
            //TODO: PAPA - huh? Why do I need to do this? The DP should be working!!!
        }

        #endregion
    }
}