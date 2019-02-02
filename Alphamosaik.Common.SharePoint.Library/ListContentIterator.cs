using System;
using Microsoft.SharePoint;

namespace Alphamosaik.Common.SharePoint.Library
{
    public class ListContentIterator
    {
        private bool _disableStrictQuerySemantics;
 
        public delegate void ItemProcessor(SPListItem item);
        public delegate bool ItemProcessorErrorCallout(SPListItem item, System.Exception e);
        public delegate void ItemsProcessor(SPListItemCollection items);
        public delegate bool ItemsProcessorErrorCallout(SPListItemCollection items, System.Exception e);

        public enum IterationGranularity
        {
            Item,
            List,
            Web,
            SiteCollection,
            WebApplication,
            Service
        }

        public void ProcessListItems(SPList list, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            ProcessListItems(list, false, itemProcessor, errorCallout);
        }

        public void ProcessListItems(SPList list, SPQuery query, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            ProcessListItems(list, query, false, itemProcessor, errorCallout);
        }

        public void ProcessListItems(SPList list, SPQuery query, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (itemsProcessor == null)
            {
                throw new ArgumentNullException("itemsProcessor");
            }

            if (!list.HasExternalDataSource && (list.ItemCount == 0))
            {
                return;
            }

            if (list.HasExternalDataSource && (query.RowLimit == 0))
            {
                query.RowLimit = 0x7fffffff;
            }
            else if ((query.RowLimit == 0) || (query.RowLimit == 0x7fffffff))
            {
                query.RowLimit = (uint)(string.IsNullOrEmpty(query.ViewFields) ? 200 : 0x7d0);
            }

            if (!list.HasExternalDataSource && StrictQuerySemantics)
            {
                query.QueryThrottleMode = SPQueryThrottleOption.Strict;
            }

            do
            {
                SPListItemCollection items = list.GetItems(query);

                try
                {
                    itemsProcessor(items);
                }
                catch (System.Exception exception)
                {
                    if ((errorCallout == null) || errorCallout(items, exception))
                    {
                        throw;
                    }
                }

                query.ListItemCollectionPosition = items.ListItemCollectionPosition;

            } while (!ShouldCancel(IterationGranularity.Item) && query.ListItemCollectionPosition != null);

        }

        public void ProcessListItems(SPList list, bool fIterateInReverseOrder, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (itemProcessor == null)
            {
                throw new ArgumentNullException("itemProcessor");
            }

            string strQuery = !list.HasExternalDataSource ? ItemEnumerationOrderById : null;
            ProcessListItems(list, strQuery, true,
                             items => ProcessItems(items, false, fIterateInReverseOrder, itemProcessor, errorCallout),
                             (items, e) => true);
        }

        public void ProcessListItems(SPList list, SPQuery query, bool fIterateInReverseOrder, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (itemProcessor == null)
            {
                throw new ArgumentNullException("itemProcessor");
            }

            ProcessListItems(list, query,
                             items => ProcessItems(items, false, fIterateInReverseOrder, itemProcessor, errorCallout), null);
        }

        public void ProcessListItems(SPList list, string strQuery, bool fRecursive, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            ProcessListItems(list, strQuery, 0, fRecursive, itemsProcessor, errorCallout);
        }

        public void ProcessListItems(SPList list, string strQuery, uint rowLimit, bool fRecursive, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            ProcessListItems(list, strQuery, rowLimit, fRecursive, null, itemsProcessor, errorCallout);
        }

        public void ProcessListItems(SPList list, string strQuery, uint rowLimit, bool fRecursive, SPFolder folder, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (itemsProcessor == null)
            {
                throw new ArgumentNullException("itemsProcessor");
            }

            var query = new SPQuery();

            if (!string.IsNullOrEmpty(strQuery))
            {
                query.Query = strQuery;
            }

            query.RowLimit = rowLimit;
            query.QueryThrottleMode = SPQueryThrottleOption.Override;

            if (folder != null)
            {
                query.Folder = folder;
            }

            if (fRecursive)
            {
                query.ViewAttributes = "Scope=\"RecursiveAll\"";
            }

            ProcessListItems(list, query, itemsProcessor, errorCallout);
        }

        public void ProcessItems(SPListItemCollection items, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            ProcessItems(items, false, itemProcessor, errorCallout);
        }

        public void ProcessItems(SPListItemCollection items, bool fIncludeFolderItems, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            ProcessItems(items, fIncludeFolderItems, false, itemProcessor, errorCallout);
        }

        public void ProcessItems(SPListItemCollection items, bool fIncludeFolderItems, bool fIterateInReverseOrder, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (itemProcessor == null)
            {
                throw new ArgumentNullException("itemProcessor");
            }

            if (fIterateInReverseOrder)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    ProcessItem(items[i], fIncludeFolderItems, itemProcessor, errorCallout);
                    if (ShouldCancel(IterationGranularity.Item))
                    {
                        return;
                    }
                }
            }
            else
            {
                for (int j = 0; j < items.Count; j++)
                {
                    ProcessItem(items[j], fIncludeFolderItems, itemProcessor, errorCallout);
                    if (ShouldCancel(IterationGranularity.Item))
                    {
                        return;
                    }
                }
            }

        }

        public bool ShouldCancel(IterationGranularity granularity)
        {
            return Cancel;
        }

        public static string ItemEnumerationOrderById
        {
            get
            {
                return "<OrderBy Override='TRUE'><FieldRef Name='ID' /></OrderBy>";
            }
        }

        public bool Cancel { get; set; }

        public bool StrictQuerySemantics
        {
            get
            {
                return !_disableStrictQuerySemantics;
            }
            set
            {
                _disableStrictQuerySemantics = !value;
            }
        }


        private static void ProcessItem(SPListItem item, bool fIncludeFolderItems, ItemProcessor itemProcessor, ItemProcessorErrorCallout errorCallout)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (itemProcessor == null)
            {
                throw new ArgumentNullException("itemProcessor");
            }

            if (fIncludeFolderItems || (item.FileSystemObjectType != SPFileSystemObjectType.Folder))
            {
                try
                {
                    itemProcessor(item);
                }
                catch (System.Exception exception)
                {
                    if ((errorCallout == null) || errorCallout(item, exception))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
