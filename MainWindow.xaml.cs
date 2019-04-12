using System;
using System.Threading;
using System.Windows;
using LinkLabelChecker.Annotations;

namespace LinkLabelChecker
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        [NotNull] private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
        }

        [NotNull] public Model Model { get; } = new Model();

        private void OnStart()
        {
            CrawlTitlesButton.IsEnabled = false;
            ReplaceLabelsButton.IsEnabled = false;
            CancelButton.IsCancel = true;
        }

        private void OnStop()
        {
            CrawlTitlesButton.IsEnabled = true;
            ReplaceLabelsButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        private async void CrawlTitlesButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OnStart();
                var tokenSource = new CancellationTokenSource();
                _tokenSource = tokenSource;
                await Model.CrawlTitlesAsync(tokenSource.Token).ConfigureAwait(true);
                MessageBox.Show("Finished crawling files.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                OnStop();
            }
        }

        private async void ReplaceLabelsButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OnStart();
                var tokenSource = new CancellationTokenSource();
                _tokenSource = tokenSource;
                await Model.ReplaceLinkLabelAsync(tokenSource.Token).ConfigureAwait(true);
                MessageBox.Show("Finished replacing labels.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                OnStop();
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            _tokenSource.Cancel();
        }
    }
}