using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogClient
{
    /// <summary>
    /// Class for main window.
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        /// <summary>
        /// UDP port number to listen to.
        /// </summary>
        private int port;

        /// <summary>
        /// Array with severity levels.
        /// </summary>
        private readonly string[] severityLevels = 
            new string[] { "--- All ---", "FATAL", "ERROR", "WARN", "INFO" };

        /// <summary>
        /// Log collection.
        /// </summary>
        private ObservableCollection<TraceData> log = new ObservableCollection<TraceData>();

        /// <summary>
        /// Collection of categories.
        /// </summary>
        private CategoryItemCollection categories;

        /// <summary>
        /// Are we paused.
        /// </summary>
        private bool paused;

        private readonly SerialDisposable _udpConnection;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", 
            "CA1031:DoNotCatchGeneralExceptionTypes", 
            Justification = "propagating exception would result in a crash")]
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                CollectionView view = new ListCollectionView(this.log);
                view.Filter = item => this.FilterItem((TraceData) item);
                this.dgLog.ItemsSource = view;
                this.port = int.Parse(ConfigurationManager.AppSettings["UdpTraceListenerPort"],  CultureInfo.InvariantCulture);
                this.cbSeverityFilter.ItemsSource = this.severityLevels;
                this.cbSeverityFilter.SelectedIndex = 0;
                this.LoadCategoryFilter();
                this.SetCategoryFilterColors();

                _udpConnection = new SerialDisposable();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n" + e.StackTrace, e.GetType().Name);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Actions to be done when closing application.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            this._udpConnection.Dispose();
            this.SaveCategoryFilter();
            base.OnClosing(e);
        }

        internal void OnConnect(object sender, RoutedEventArgs e)
        {
            this.btnConnection.Content = "Disconnect";
            this._udpConnection.Disposable = Connect();

        }

        internal void OnDisconnect(object sender, RoutedEventArgs e)
        {
            this.btnConnection.Content = "Connect";
            this._udpConnection.Disposable = Disposable.Empty;
        }

        /// <summary>
        /// Action after closing window.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnButtonClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Action after clicking on "Clear" button.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnButtonClear(object sender, RoutedEventArgs e)
        {
            this.log.Clear();
        }

        /// <summary>
        /// Action after clearing search input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnClearSearch(object sender, RoutedEventArgs e)
        {
            this.txtSearch.Clear();
            this.txtSearch.Focus();
        }

        /// <summary>
        /// Action after changing severity level.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnSeverityFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RequestFilter();
        }

        /// <summary>
        /// Action after change in search field.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            tbSearchHint.Visibility = this.txtSearch.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            btnClearSearch.Visibility = this.txtSearch.Text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
            this.RequestFilter();
        }

        /// <summary>
        /// Action after clicking on Run/Pause button.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnButtonPauseRun(object sender, RoutedEventArgs e)
        {
            this.paused = !this.paused;
            this.btnPause.Content = this.paused ? "Run" : "Pause";
        }

        /// <summary>
        /// Filter incoming requests.
        /// </summary>
        public void RequestFilter()
        {
            ((ListCollectionView) this.dgLog.ItemsSource).Refresh();
        }

        /// <summary>
        /// Filter item.
        /// </summary>
        /// <param name="data">Trace data.</param>
        /// <returns>Should we filter this item or not.</returns>
        public bool FilterItem(TraceData data)
        {
            bool canShow = this.categories[data.Category].Active;
            if (canShow && this.cbSeverityFilter.SelectedIndex > 0)
            {
                canShow = severityLevels
                    .Skip(1)
                    .Take(this.cbSeverityFilter.SelectedIndex)
                    .Any(p => string.Equals(p, data.Severity));
            }
            if (canShow)
            {
                canShow = data.MatchesFilter(txtSearch.Text);
            }
            return canShow;
        }

        /// <summary>
        /// Action after clicking on category filter.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnCategoryFilterClick(object sender, RoutedEventArgs e)
        {
            Window wnd = new CategoryFilter(this.categories);
            wnd.Owner = this;
            wnd.ShowDialog();
            this.SetCategoryFilterColors();
            this.RequestFilter();
        }

        /// <summary>
        /// Set category filters on a list.
        /// </summary>
        private void SetCategoryFilterColors()
        {
            this.btnCategoryFilter.Background = this.categories.Any(p => !p.Active) ? Brushes.Yellow : Brushes.Transparent;
        }

        /// <summary>
        /// Save the state of category filter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", 
            "CA1031:DoNotCatchGeneralExceptionTypes", 
            Justification = "propagating exception would lead to a crash")]
        private void SaveCategoryFilter()
        {
            try
            {
                using (FileStream storage = new FileStream("categories.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(storage, this.categories);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Load category filter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", 
            "CA1031:DoNotCatchGeneralExceptionTypes", 
            Justification = "propagating exception would lead to a crash")]
        private void LoadCategoryFilter()
        {
            if (File.Exists("categories.bin"))
            {
                try
                {
                    using (FileStream storage = new FileStream("categories.bin", FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        this.categories = (CategoryItemCollection)formatter.Deserialize(storage);
                    }
                }
                catch
                {
                }
            }
            this.categories = this.categories ?? new CategoryItemCollection();
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if (null != dg.CurrentCell)
            {
                TraceData data = (TraceData)dg.CurrentItem;
                if (null != data)
                {
                    if (dg.CurrentCell.Column == dg.Columns[1])
                    {
                        this.txtSearch.Text = data.TraceId;
                    }
                    else
                    {
                        var dlg = new EntryDetails(data);
                        dlg.Owner = this;
                        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        dlg.Show();
                    }
                }
            }
        }

        private IDisposable Connect()
        {
            return Observable.Create<TraceData>(o =>
            {
                var processor = new DataProcessor(this.port);

                var categorySubscription = processor
                    .CategoryDataStream
                    .SubscribeOn(SynchronizationContext.Current)
                    .Where(c => !this.categories.Contains(c))
                    .Subscribe(c => this.categories.Add(new CategoryItem(c, false)));

                var itemsStream = processor.TraceDataStream.Subscribe(o);

                return new CompositeDisposable(categorySubscription, itemsStream);
            })
            .Buffer(TimeSpan.FromMilliseconds(100), 10)
            .Where(buffer => null != buffer && buffer.Count > 0)
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(buffer =>
            {
                foreach (var data in buffer)
                {
                    this.log.Add(data);
                }
                if (!this.paused)
                {
                    this.dgLog.ScrollIntoView(buffer[buffer.Count - 1]);
                }
            });
        }
    }
}
