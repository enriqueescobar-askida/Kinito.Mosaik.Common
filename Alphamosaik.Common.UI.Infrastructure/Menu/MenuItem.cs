using System.Collections.Generic;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class MenuItem
    {
        public ScreenKeyType ScreenKey { get; set; }
        public string Description { get; set; }
        //public string IconSource { get; set; }
        public List<MenuItem> MenuItems { get; set; }
    }
}
