namespace Alphamosaik.Common.UI.Infrastructure
{
    public class ScreenEventArgs
    {
        public object ScreenSubject { get; set; }
        public ScreenKeyType ScreenKey { get; set; }
        public string RegionName { get; set; }
    }
}