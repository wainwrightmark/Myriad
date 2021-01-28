using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeCantSpell.Hunspell;

namespace Words
{
    public class DictionaryHelper
    {

        public async Task<IReadOnlyCollection<string>> GetNormalWordsAsync(CancellationToken token)
        {
            await using var       dictionaryStream = StringToStream(Resources.index1);
            await using var affixStream      = StringToStream(Resources.index_aff);

            var wl = await WordList.CreateFromStreamsAsync(dictionaryStream, affixStream);

            var words = wl.RootWords
                .Where(x => x.All(c => char.IsLetter(c) && char.IsLower(c)))
                .Where(x => x.Length > 2)
                .SelectMany(GetAllVariations)
                .ToList();

            return words;
        }

        public  IReadOnlyCollection<string> GetNormalWords()
        {
            using var dictionaryStream = StringToStream(Resources.index1);
            using var affixStream = StringToStream(Resources.index_aff);

            var wl = WordList.CreateFromStreams(dictionaryStream, affixStream);

            var words = wl.RootWords
                .Where(x => x.All(c => char.IsLetter(c) && char.IsLower(c)))
                .Where(x => x.Length > 2)
                .SelectMany(GetAllVariations)
                .ToList();

            return words;
        }


        private static Stream StringToStream(string s)
        {
            var byteArray = Encoding.ASCII.GetBytes( s );
            var stream = new MemoryStream( byteArray );
            return stream;
        }

        public DictionaryHelper()
        {

            HunspellWordList = new Lazy<WordList>(() =>
                {
                    using var dictionaryStream = StringToStream(Resources.index1);
                    using var affixStream = StringToStream(Resources.index_aff);

                    var wl = WordList.CreateFromStreams(dictionaryStream, affixStream);

                    return wl;
                });

            AllWords = new Lazy<IReadOnlySet<string>>(()=>
                GenerateAllWords(HunspellWordList.Value)
                    .Distinct()
                    .Where(x=>x.All(char.IsLetter) && x.Length > 2).ToHashSet());

            NormalWords = new Lazy<IReadOnlySet<string>>(()=>
                AllWords.Value.Where(x=>x.All(char.IsLower)).ToHashSet());

            ProperNouns = new Lazy<IReadOnlySet<string>>(()=>
                AllWords.Value.Where(x=>x.Any(char.IsLower) && x.Any(char.IsUpper)).ToHashSet());

            Acronyms = new Lazy<IReadOnlySet<string>>(()=>
                AllWords.Value.Where(x=>x.All(char.IsUpper)).ToHashSet());

            MostCommonWords = new Lazy<IReadOnlySet<string>>(
                ()=>
                    Resources.MostCommonWords.Split("\r\n", StringSplitOptions.RemoveEmptyEntries )
                        .Select(x=>x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet());
        }

        public readonly Lazy<WordList> HunspellWordList;

        public readonly Lazy<IReadOnlySet<string>> AllWords;
        public readonly Lazy<IReadOnlySet<string>> NormalWords;
        public readonly Lazy<IReadOnlySet<string>> Acronyms;
        public readonly Lazy<IReadOnlySet<string>> ProperNouns;
        public readonly Lazy<IReadOnlySet<string>> MostCommonWords;

        private IEnumerable<string> GenerateAllWords(WordList wordList) => wordList.RootWords.SelectMany(GetAllVariations);

        public IEnumerable<string> GetAllVariations(string word)
        {
            yield return word;

            var wordEntryDetail = HunspellWordList.Value[word];

            var allPrefixesForWord =
                HunspellWordList.Value.Affix.Prefixes.Where(p => wordEntryDetail.Any(x => x.ContainsFlag(p.AFlag))).ToList();

            string combined;
            foreach (var prefixEntry in allPrefixesForWord.SelectMany(p => p.Entries))
                if (TryAppend(prefixEntry, word, out combined))
                    yield return combined;

            var allSuffixesForWord =
                HunspellWordList.Value.Affix.Suffixes.Where(s => wordEntryDetail.Any(x => x.ContainsFlag(s.AFlag))).ToList();

            foreach (var suffixEntry in allSuffixesForWord.SelectMany(s => s.Entries))
                if (TryAppend(suffixEntry, word, out combined))
                    yield return combined;

            foreach (var prefixEntry in allPrefixesForWord.Where(p => p.AllowCross).SelectMany(p => p.Entries))
            {
                if (!TryAppend(prefixEntry, word, out var withPrefix)) continue;
                foreach (var suffixEntry in allSuffixesForWord.Where(s => s.AllowCross).SelectMany(s => s.Entries))
                    if (TryAppend(suffixEntry, withPrefix!, out combined))
                        yield return combined;
            }
        }

        private static bool TryAppend(PrefixEntry prefix, string word, out string result)
        {
            if (prefix.Conditions.IsStartingMatch(word) && word.StartsWith(prefix.Strip))
            {
                result = prefix.Append + word.Substring(prefix.Strip.Length);
                return true;
            }

            result = null!;
            return false;
        }

        private static bool TryAppend(SuffixEntry suffix, string word, out string result)
        {
            if (suffix.Conditions.IsEndingMatch(word) && word.EndsWith(suffix.Strip))
            {
                result = word.Substring(0, word.Length - suffix.Strip.Length) + suffix.Append;
                return true;
            }

            result = null!;
            return false;
        }
    }
}
