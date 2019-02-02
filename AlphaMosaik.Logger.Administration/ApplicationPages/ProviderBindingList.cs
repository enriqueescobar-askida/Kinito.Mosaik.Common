using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AlphaMosaik.Logger.Storage;

namespace AlphaMosaik.Logger.Administration.ApplicationPages
{
    public class ProviderBindingList : List<IStorageProvider>, ITypedList
    {
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "Provider Binding List";
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof (IStorageProvider));
            return properties;
        }
    }
}
