using System.Windows;
using System.Windows.Media;

namespace Alphamosaik.Common.UI.Infrastructure.Behaviors
{
    public class RotationData
    {
        public double FromDegrees { get; set; }
        public double MidDegrees { get; set; }
        public double ToDegrees { get; set; }
        public string RotationProperty { get; set; }
        public PlaneProjection PlaneProjection { get; set; }
        public Duration AnimationDuration { get; set; }
    }
}