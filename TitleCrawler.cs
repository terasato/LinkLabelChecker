using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinkLabelChecker
{
    public static class TitleCrawler
    {
        public static FileTitle CrawlTitle(string path)
        {
            var title = FindTitle(path);
            if (string.IsNullOrWhiteSpace(title)) return null;

            var ft = new FileTitle(path, title);
            return ft;
        }

        private static string FindTitle(string path)
        {
            try
            {
                var r = new Regex(@"^title:\s*(?<title>.+?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (var line in File.ReadLines(path).Take(10))
                {
                    if (r.IsMatch(line))
                    {
                        var m = r.Match(line);
                        return m.Groups["title"].Value.Trim('\'', '"', ' ');
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}