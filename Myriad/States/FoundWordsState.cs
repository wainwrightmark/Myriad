using System.Collections.Generic;

namespace Myriad.States
{

public record FoundWordsState(FoundWordsData Data)
{
    public FoundWordsState EnableWord(FoundWord word, bool enable)
    {
        if (Data is FoundWordsData.OpenSearchData osd)
        {
            osd = osd with
            {
                FoundWordsDictionary = osd.FoundWordsDictionary.SetItem(word, enable)
            };

            return this with { Data = osd };
        }

        return this;
    }

    public FoundWordsState FindWord(FoundWord word)
    {
        return this with { Data = Data.FindWord(word) };
    }

    public FoundWordsState FindWords(IEnumerable<FoundWord> words)
    {
        var data = Data;

        foreach (var foundWord in words)
        {
            data = data.FindWord(foundWord);
        }

        return this with { Data = data };
    }

    public FoundWordsState Reset()
    {
        return this with { Data = Data.Reset() };
    }
}

}
