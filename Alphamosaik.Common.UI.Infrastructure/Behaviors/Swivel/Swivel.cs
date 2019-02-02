using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Alphamosaik.Common.UI.Infrastructure.Behaviors
{
    public class Swivel : TriggerAction<FrameworkElement>
    {
        #region Front element

        public static readonly DependencyProperty FrontElementNameProperty =
            DependencyProperty.Register("FrontElementName", typeof(string),
                                        typeof(Swivel), new PropertyMetadata(null));

        [Category("Swivel Properties")]
        public string FrontElementName { get; set; }

        #endregion

        #region Back element

        public static readonly DependencyProperty BackElementNameProperty =
            DependencyProperty.Register("BackElementName", typeof(string),
                                        typeof(Swivel), new PropertyMetadata(null));

        [Category("Swivel Properties")]
        public string BackElementName { get; set; }

        #endregion

        #region Duration

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration),
                                        typeof(Swivel), new PropertyMetadata(null));

        [Category("Animation Properties")]
        public Duration Duration { get; set; }

        #endregion

        #region Rotation Direction

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register("Rotation", typeof(RotationDirection),
                                        typeof(Swivel), new PropertyMetadata(RotationDirection.LeftToRight));

        [Category("Animation Properties")]
        public RotationDirection Rotation { get; set; }

        #endregion

        //#region CenterOfRotationZ

        //public static readonly DependencyProperty CenterOfRotationZProperty =
        //    DependencyProperty.Register("CenterOfRotationZ", typeof(double),
        //                                typeof(Swivel), new PropertyMetadata((double)0));

        //[Category("Animation Properties")]
        //public double CenterOfRotationZ { get; set; }

        //#endregion

        private readonly Storyboard frontToBackStoryboard = new Storyboard();
        private readonly Storyboard backToFrontStoryboard = new Storyboard();
        private bool forward = true;

        protected override void Invoke(object parameter)
        {
            
            
            if (AssociatedObject == null) return;

            FrameworkElement parent = AssociatedObject; // as FrameworkElement;
            UIElement front = null;
            UIElement back = null;

            front = parent.FindName(FrontElementName) as UIElement;
            back = parent.FindName(BackElementName) as UIElement;
            if (front == null || back == null) return;

            if (front.Projection == null || back.Projection == null)
            {
                front.Projection = new PlaneProjection();
                front.RenderTransformOrigin = new Point(.5, .5);
                front.Visibility = Visibility.Visible;

                back.Projection = new PlaneProjection { CenterOfRotationY = .5, RotationY = 180.0 }; //, CenterOfRotationZ = this.CenterOfRotationZ };
                back.RenderTransformOrigin = new Point(.5, .5);
                back.Visibility = Visibility.Collapsed;

                RotationData showBackRotation = null;
                RotationData hideFrontRotation = null;
                RotationData showFrontRotation = null;
                RotationData hideBackRotation = null;

                var frontPP = new PlaneProjection(); // { CenterOfRotationZ = this.CenterOfRotationZ };
                var backPP = new PlaneProjection(); // { CenterOfRotationZ = this.CenterOfRotationZ };

                switch (Rotation)
                {
                    case RotationDirection.LeftToRight:
                        backPP.CenterOfRotationY = frontPP.CenterOfRotationY = 0.5;
                        showBackRotation = new RotationData { FromDegrees = 180.0, MidDegrees = 90.0, ToDegrees = 0.0, RotationProperty = "RotationY", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationData { FromDegrees = 0.0, MidDegrees = -90.0, ToDegrees = -180.0, RotationProperty = "RotationY", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        showFrontRotation = new RotationData { FromDegrees = -180.0, MidDegrees = -90.0, ToDegrees = 0.0, RotationProperty = "RotationY", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        hideBackRotation = new RotationData { FromDegrees = 0.0, MidDegrees = 90.0, ToDegrees = 180.0, RotationProperty = "RotationY", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        break;
                    case RotationDirection.RightToLeft:
                        backPP.CenterOfRotationY = frontPP.CenterOfRotationY = 0.5;
                        showBackRotation = new RotationData { FromDegrees = -180.0, MidDegrees = -90.0, ToDegrees = 0.0, RotationProperty = "RotationY", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationData { FromDegrees = 0.0, MidDegrees = 90.0, ToDegrees = 180.0, RotationProperty = "RotationY", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        showFrontRotation = new RotationData { FromDegrees = 180.0, MidDegrees = 90.0, ToDegrees = 0.0, RotationProperty = "RotationY", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        hideBackRotation = new RotationData { FromDegrees = 0.0, MidDegrees = -90.0, ToDegrees = -180.0, RotationProperty = "RotationY", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        break;
                    case RotationDirection.BottomToTop:
                        backPP.CenterOfRotationX = frontPP.CenterOfRotationX = 0.5;
                        showBackRotation = new RotationData { FromDegrees = 180.0, MidDegrees = 90.0, ToDegrees = 0.0, RotationProperty = "RotationX", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationData { FromDegrees = 0.0, MidDegrees = -90.0, ToDegrees = -180.0, RotationProperty = "RotationX", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        showFrontRotation = new RotationData { FromDegrees = -180.0, MidDegrees = -90.0, ToDegrees = 0.0, RotationProperty = "RotationX", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        hideBackRotation = new RotationData { FromDegrees = 0.0, MidDegrees = 90.0, ToDegrees = 180.0, RotationProperty = "RotationX", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        break;
                    case RotationDirection.TopToBottom:
                        backPP.CenterOfRotationX = frontPP.CenterOfRotationX = 0.5;
                        showBackRotation = new RotationData { FromDegrees = -180.0, MidDegrees = -90.0, ToDegrees = 0.0, RotationProperty = "RotationX", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        hideFrontRotation = new RotationData { FromDegrees = 0.0, MidDegrees = 90.0, ToDegrees = 180.0, RotationProperty = "RotationX", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        showFrontRotation = new RotationData { FromDegrees = 180.0, MidDegrees = 90.0, ToDegrees = 0.0, RotationProperty = "RotationX", PlaneProjection = frontPP, AnimationDuration = this.Duration };
                        hideBackRotation = new RotationData { FromDegrees = 0.0, MidDegrees = -90.0, ToDegrees = -180.0, RotationProperty = "RotationX", PlaneProjection = backPP, AnimationDuration = this.Duration };
                        break;
                }

                front.RenderTransformOrigin = new Point(.5, .5);
                back.RenderTransformOrigin = new Point(.5, .5);

                front.Projection = frontPP;
                back.Projection = backPP;

                frontToBackStoryboard.Duration = this.Duration;
                backToFrontStoryboard.Duration = this.Duration;

                // Rotation
                frontToBackStoryboard.Children.Add(CreateRotationAnimation(showBackRotation));
                frontToBackStoryboard.Children.Add(CreateRotationAnimation(hideFrontRotation));
                backToFrontStoryboard.Children.Add(CreateRotationAnimation(hideBackRotation));
                backToFrontStoryboard.Children.Add(CreateRotationAnimation(showFrontRotation));

                // Visibility
                frontToBackStoryboard.Children.Add(CreateVisibilityAnimation(showBackRotation.AnimationDuration, front, false));
                frontToBackStoryboard.Children.Add(CreateVisibilityAnimation(hideFrontRotation.AnimationDuration, back, true));
                backToFrontStoryboard.Children.Add(CreateVisibilityAnimation(hideBackRotation.AnimationDuration, front, true));
                backToFrontStoryboard.Children.Add(CreateVisibilityAnimation(showFrontRotation.AnimationDuration, back, false));
            }

            if (forward)
            {
                frontToBackStoryboard.Begin();
                forward = false;
            }
            else
            {
                backToFrontStoryboard.Begin();
                forward = true;
            }
        }

        private static ObjectAnimationUsingKeyFrames CreateVisibilityAnimation(Duration duration, DependencyObject element, bool show)
        {
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.BeginTime = new TimeSpan(0);
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(0), Value = (show ? Visibility.Collapsed : Visibility.Visible) });
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = new TimeSpan(duration.TimeSpan.Ticks / 2), Value = (show ? Visibility.Visible : Visibility.Collapsed) });
            Storyboard.SetTargetProperty(animation, new PropertyPath("Visibility"));
            Storyboard.SetTarget(animation, element);
            return animation;
        }


        private static DoubleAnimationUsingKeyFrames CreateRotationAnimation(RotationData rd)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.BeginTime = new TimeSpan(0);
            //animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(0), Value = rd.FromDegrees, EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn } });
            //animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(rd.AnimationDuration.TimeSpan.Ticks / 2), Value = rd.MidDegrees });
            //animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(rd.AnimationDuration.TimeSpan.Ticks), Value = rd.ToDegrees, EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut } });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(0), Value = rd.FromDegrees, EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn } });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(rd.AnimationDuration.TimeSpan.Ticks / 2), Value = rd.MidDegrees });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = new TimeSpan(rd.AnimationDuration.TimeSpan.Ticks), Value = rd.ToDegrees, EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTargetProperty(animation, new PropertyPath(rd.RotationProperty)); 
            Storyboard.SetTarget(animation, rd.PlaneProjection);
            return animation;
        }
    }
}