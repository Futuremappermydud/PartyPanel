using PartyPanelShared.Models;
using PartyPanelUI.Pages;
using System.Globalization;
using System.Text;

namespace PartyPanelUI
{
    //Thank you Kinsi
    public static class WeightedSearch
    {
        static unsafe string MakeStringSearchable(string s)
        {
            // Eh whatever
            if (s.Length > 255)
                return s;

            var normalizedString = s.Normalize(NormalizationForm.FormD);

            var pos = 0;
            char* challoc = stackalloc char[s.Length];

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];

                var cat = CharUnicodeInfo.GetUnicodeCategory(c);

                if (cat == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                // adds 32 (Ascii ' ') to the A-Z charcode and thus converts it to a-z lmao
                if (cat == UnicodeCategory.LowercaseLetter || cat == UnicodeCategory.SpaceSeparator || cat == UnicodeCategory.DecimalDigitNumber)
                {
                    challoc[pos++] = c;
                }
                else if (cat == UnicodeCategory.UppercaseLetter && c < '[')
                {
                    challoc[pos++] = (char)(c + ' ');
                }
            }

            return new string(challoc, 0, pos);
        }

        struct WeightedSong
        {
            public PreviewBeatmapLevel song;
            public float searchWeight;
            public float sortWeight;
        }

        internal static List<PreviewBeatmapLevel> Search(List<PreviewBeatmapLevel> input, string text, Func<PreviewBeatmapLevel, float> ordersort)
        {
			List<PreviewBeatmapLevel> filteredInput = input.Where((x) => !(Pages.Index.ranked || Pages.Index.sortMode == "Ranked/Qualified time" || Pages.Index.sortMode.Contains("Stars")) || GlobalData.Ranked(x)).ToList();
            if (string.IsNullOrWhiteSpace(text))
                return filteredInput;
            string filter = text;
            var words = filter.ToLowerInvariant().Split(new string[0], StringSplitOptions.RemoveEmptyEntries);

            // Slightly slower than just calling IsLetterOrDigit if its not a ' ', but in most of the cases it will be
            bool IsSpace(char x) => x == ' ' || !char.IsLetterOrDigit(x);

            var prefiltered = new List<WeightedSong>();

            var maxSearchWeight = 0f;
            var maxSortWeight = 0f;

            Parallel.ForEach(filteredInput, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, x =>
            {
                var resultWeight = 0;
                var matchedAuthor = false;
                var prevMatchIndex = -1;

                var songeName = MakeStringSearchable(x.Name);

                var authorName = x.Author;
                var authorFullMatch = filter.IndexOf(authorName, StringComparison.OrdinalIgnoreCase);
                var i = 0;
                if (authorName.Length > 4 && authorFullMatch != -1 &&
                    // Checks if there is a space after the supposedly matched author name
                    (filter.Length == authorName.Length || IsSpace(filter[authorName.Length]))
                )
                {
                    matchedAuthor = true;
                    resultWeight += authorName.Length > 5 ? 25 : 20;

                    // This is super cheapskate - I'd have to replace the author from the filter and recreate the words array otherwise
                    if (authorFullMatch == 0)
                        i = 1;
                }

                for (; i < words.Length; i++)
                {
                    // If the word matches the author 1:1 thats cool innit
                    if (authorName.Length != 0)
                    {
                        if (!matchedAuthor && authorName.Equals(words[i], StringComparison.OrdinalIgnoreCase))
                        {
                            matchedAuthor = true;

                            resultWeight += 3 * (words[i].Length / 2);
                            continue;
                            // Otherwise we'll have to check if its contained within this word
                        }
                        else if (!matchedAuthor && words[i].Length >= 3)
                        {
                            var index = authorName.IndexOf(words[i], StringComparison.OrdinalIgnoreCase);

                            if (index == 0 || (index > 0 && IsSpace(authorName[index - 1])))
                            {
                                matchedAuthor = true;

                                resultWeight += (int)Math.Round((index == 0 ? 4f : 3f) * ((float)words[i].Length / authorName.Length));
                                continue;
                            }
                        }
                    }

                    // Match the current split word in the song name
                    var matchpos = songeName.IndexOf(words[i], StringComparison.Ordinal);

                    // If we found anything...
                    if (matchpos != -1)
                    {
                        // Check if we matched the beginning of a word
                        var wordStart = matchpos == 0 || songeName[matchpos - 1] == ' ';

                        // If it was the beginning add 5 weighting, else 3
                        resultWeight += wordStart ? 5 : 3;

                        var posInName = matchpos + words[i].Length;

                        /*
                        * Check if we are at the end of the song name, but only if it has at least 8 characters
                        * We do this because otherwise, when searching for "lowermost revolt", songs where the
                        * songName is exactly "lowermost revolt" would have a lower result weight than
                        * "lowermost revolt (JoeBama cover)"
                        *
                        * The 8 character limitation for this is so that super short words like "those" dont end
                        * up triggering this
                   */
                        if (songeName.Length > 7 && songeName.Length == posInName)
                        {
                            resultWeight += 3;
                        }
                        else
                        {
                            // If we did match the beginning, check if we matched an entire word. Get the end index as indicated by our needle
                            var maybeWordEnd = wordStart && posInName < songeName.Length;

                            // Check if we actually end up at a non word char, if so add 2 weighting
                            if (maybeWordEnd && songeName[matchpos + words[i].Length] == ' ')
                                resultWeight += 2;
                        }

                        // If the word we just checked is behind the previous matched, add another 1 weight
                        if (prevMatchIndex != -1 && matchpos > prevMatchIndex)
                            resultWeight += 1;

                        prevMatchIndex = matchpos;
                    }
                }

                for (i = 0; i < words.Length; i++)
                {
                    if (words[i].Length > 3 && x.Mapper.IndexOf(words[i], StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        resultWeight += 1;

                        break;
                    }
                }

                if (resultWeight > 0)
                {
                    var sortWeight = ordersort(x);

                    lock (prefiltered)
                    {
                        prefiltered.Add(new WeightedSong()
                        {
                            song = x,
                            searchWeight = resultWeight,
                            sortWeight = sortWeight
                        });

                        if (maxSearchWeight < resultWeight)
                            maxSearchWeight = resultWeight;

                        if (maxSortWeight < sortWeight)
                            maxSortWeight = sortWeight;
                    }
                }
            });

            if (!prefiltered.Any())
                return new List<PreviewBeatmapLevel>();

            var maxSearchWeightInverse = 1f / maxSearchWeight;
            var maxSortWeightInverse = 1f / maxSortWeight;

            return prefiltered.OrderByDescending((s) =>
            {
                var searchWeight = s.searchWeight * maxSearchWeightInverse;

                return searchWeight + Math.Min(searchWeight / 2, s.sortWeight * maxSortWeightInverse * (searchWeight / 2));
            }).Select(x => x.song).ToList();
        }
    }
}
