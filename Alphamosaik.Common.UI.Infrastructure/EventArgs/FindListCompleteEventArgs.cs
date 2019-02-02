using System.Collections.Generic;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public class FindListCompleteEventArgs<T> : System.EventArgs
    {
        public List<T> ItemList { get; set; }
    }
}