
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LogClient
{
    /// <summary>
    /// Window with category filter.
    /// </summary>
    public partial class CategoryFilter : Window
    {
        /// <summary>
        /// Initializes a new instance of the CategoryFilter class.
        /// </summary>
        /// <param name="source">Collection of categories as a source.</param>
        internal CategoryFilter(ICollection<CategoryItem> source)
        {
            InitializeComponent();
            this.dgCategories.ItemsSource = source;

            bool? state = source.All(p => p.Active) ? 
                (bool?)true : source.Any(p => p.Active) ? 
                    (bool?)null : (bool?)false;

            this.cbFilter.IsChecked = state;
        }

        /// <summary>
        /// Action after clicking on "OK" button.
        /// </summary>
        /// <param name="sender">Window sender object.</param>
        /// <param name="e">Window event arguments.</param>
        internal void OnOkClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Action after clicking on "Deselect all" button.
        /// </summary>
        /// <param name="sender">Window sender object.</param>
        /// <param name="e">Window event arguments.</param>
        internal void OnSelectDeselectAll(object sender, RoutedEventArgs e)
        {
            bool? value = cbFilter.IsChecked;
            if (value.HasValue)
            {
                ((ICollection<CategoryItem>)this.dgCategories.ItemsSource)
                    .All(p => { p.Active = value.Value; return true; });
            }
            this.cbFilter.IsThreeState = false;
        }
    }
}
