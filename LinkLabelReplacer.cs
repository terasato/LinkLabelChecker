using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LinkLabelChecker.Annotations;

namespace LinkLabelChecker
{
    public static class LinkLabelReplacer
    {
        private static readonly Regex LinkLabelRegex = new Regex(
            @"\[(?<label>.*?)\]\((?<link>[-_.!~*\\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+?)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static async Task ReplaceLabelsAsync([NotNull] string path,
            [NotNull] IReadOnlyDictionary<string, string> titlesByUri)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (titlesByUri == null) throw new ArgumentNullException(nameof(titlesByUri));

            var replacedAny = false;
            var baseUri = new Uri(path);
            var original = await GetAllTextAsync(path).ConfigureAwait(false);
            var replaced = LinkLabelRegex.Replace(original, ReplaceLinkLabel);

            if (replacedAny) await WriteAllTextAsync(path, replaced).ConfigureAwait(false);

            string ReplaceLinkLabel(Match match)
            {
                var label = match.Groups["label"].Value;
                var relativePath = match.Groups["link"].Value;
                var absolutePath = ToAbsolutePath(baseUri, relativePath);

                if (!titlesByUri.ContainsKey(absolutePath)) return match.Value;

                var title = titlesByUri[absolutePath];
                if (title == label) return match.Value;

                replacedAny = true;
                return $"[{title}]({relativePath})";
            }
        }

        private static async Task WriteAllTextAsync(string path, string text)
        {
            using (var sw = new StreamWriter(path, false))
            {
                await sw.WriteAsync(text).ConfigureAwait(false);
            }
        }

        private static string ToAbsolutePath([NotNull] Uri baseUri, [NotNull] string relativePath)
        {
            return new Uri(baseUri, relativePath).AbsolutePath.Replace('/', '\\');
        }

        private static async Task<string> GetAllTextAsync(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return await sr.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}