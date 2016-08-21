
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogClient
{
    /// <summary>
    /// Interaction logic for EntryDetails.xaml.
    /// </summary>
    public partial class EntryDetails : Window
    {
        public EntryDetails(TraceData data = null)
        {
            InitializeComponent();
            this.DataContext = data;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TraceData td = (TraceData)this.DataContext;
                string result = string.Format(
                    CultureInfo.InvariantCulture,
                    "Id: {0}\r\nTimestamp: {1}\r\nCategory: {2}\r\nSeverity: {3}\r\nTitle: {4}\r\nMessage: {5}\r\n",
                    td.Id,
                    td.Timestamp,
                    td.Category,
                    td.Severity,
                    td.Title,
                    td.Message);
                Clipboard.SetData(DataFormats.Text, result);
                tbInfo.Text = "Trace data copied.";
            }
            catch (Exception err)
            {
                tbInfo.Text = err.Message;
            }
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox tb = (TextBox)sender;
                Clipboard.SetData(DataFormats.Text, tb.Text ?? string.Empty);
                tbInfo.Text = "Content of " + tb.Name + " copied.";
            }
            catch (Exception err)
            {
                tbInfo.Text = err.Message;
            }
        }
    }
}
