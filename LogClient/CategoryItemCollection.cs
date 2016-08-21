using System;
using System.Collections.ObjectModel;

namespace LogClient
{
    /// <summary>
    /// Collection with categories.
    /// </summary>
    [Serializable]
    internal class CategoryItemCollection : KeyedCollection<string, CategoryItem>
    {
        /// <summary>
        /// Get key for an item.
        /// </summary>
        /// <param name="item">Category item you would like to look up.</param>
        /// <returns>String key for item.</returns>
        protected override string GetKeyForItem(CategoryItem item)
        {
            return item.Category;
        }
    }
}
