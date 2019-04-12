using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LinkLabelChecker.Annotations;
using LinkLabelChecker.Properties;

namespace LinkLabelChecker
{
    public sealed class Model : INotifyPropertyChanged
    {
        private const string TitlesFileName = "titles.txt";

        private int _replacedFileCount;
        private string _searchPattern = "*.md";

        public Model()
        {
            if (File.Exists(TitlesFileName))
                foreach (var item in ParseTitlesFile(TitlesFileName))
                    FileTitles.Add(item);
        }

        public string SearchPattern
        {
            get => _searchPattern;
            set
            {
                if (value == _searchPattern) return;
                _searchPattern = value;
                OnPropertyChanged();
            }
        }

        public string BaseDirectoryPath
        {
            get => Settings.Default.BaseDirectoryPath;
            set
            {
                if (value == BaseDirectoryPath) return;
                Settings.Default.BaseDirectoryPath = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileTitle> FileTitles { get; } = new ObservableCollection<FileTitle>();

        public int ReplacedFileCount
        {
            get => _replacedFileCount;
            private set
            {
                if (value == _replacedFileCount) return;
                _replacedFileCount = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static IEnumerable<FileTitle> ParseTitlesFile(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException("The specified file does not exist.");

            var regex = new Regex(@"^(?<title>.+?)\t(?<path>.+?)$");
            foreach (var line in File.ReadLines(path).Where(s => !string.IsNullOrEmpty(s)))
            {
                var m = regex.Match(line);
                yield return new FileTitle(m.Groups["path"].Value, m.Groups["title"].Value);
            }
        }


        public async Task CrawlTitlesAsync(CancellationToken token)
        {
            FileTitles.Clear();

            foreach (var path in EnumerateFiles(BaseDirectoryPath, SearchPattern))
            {
                var ft = await Task.Run(() => TitleCrawler.CrawlTitle(path), token).ConfigureAwait(true);
                if (ft != null)
                {
                    FileTitles.Add(ft);
                    Console.WriteLine($"{ft.Title}\t{ft.Path}");
                }

                if (token.IsCancellationRequested) return;
            }

            File.Delete(TitlesFileName);
            File.WriteAllLines(TitlesFileName, FileTitles.Select(ft => $"{ft.Title}\t{ft.Path}"));
        }

        public async Task ReplaceLinkLabelAsync(CancellationToken token)
        {
            ReplacedFileCount = 0;

            var titlesByUri = FileTitles.ToDictionary(ft => ft.Path, ft => ft.Title);
            foreach (var path in EnumerateFiles(BaseDirectoryPath, SearchPattern))
            {
                await LinkLabelReplacer.ReplaceLabelsAsync(path, titlesByUri).ConfigureAwait(true);

                ReplacedFileCount++;
                if (token.IsCancellationRequested) return;
            }
        }

        private static IEnumerable<string> EnumerateFiles(string baseDirectoryPath, string searchPattern)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            if (!Directory.Exists(baseDirectoryPath))
                throw new ArgumentException("The specified directory does not exist.");

            return Directory.EnumerateFiles(baseDirectoryPath, searchPattern, SearchOption.AllDirectories);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] [CanBeNull] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}