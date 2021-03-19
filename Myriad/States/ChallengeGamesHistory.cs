using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Myriad.Actions;

namespace Myriad.States
{

public record LoadSavedChallengeGamesAction(IReadOnlyCollection<SavedChallengeGame> Games) : IAction<ChallengeGamesHistory>
{
    /// <inheritdoc />
    public ChallengeGamesHistory Reduce(ChallengeGamesHistory state)
    {
        return new (Games.ToImmutableDictionary(x => x.boardId));
    }
}

public record SaveGameAction(string BoardId, FoundWordsState FoundWordsState) : IAction<ChallengeGamesHistory>
{
    /// <inheritdoc />
    public ChallengeGamesHistory Reduce(ChallengeGamesHistory state)
    {
        return state.MaybeUpdate(BoardId, FoundWordsState) ?? state;
    }
}

public record ChallengeGamesHistory(
    ImmutableDictionary<string, SavedChallengeGame> SavedChallengeGames)
{
    public ChallengeGamesHistory? MaybeUpdate(string boardId, FoundWordsState FoundWordsState)
    {
        if (FoundWordsState.Data is FoundWordsData.TargetWordsData twd)
        {
            var newSavedChallengeGame = new SavedChallengeGame
            {
                boardId        = boardId,
                foundSolutions = twd.WordsToFind.Count(x => x.Value.word is not null),
                maxSolutions   = twd.WordsToFind.Count
            };

            if (SavedChallengeGames.TryGetValue(boardId, out var savedChallengeGame))
            {
                if (savedChallengeGame.AreEqual(newSavedChallengeGame))
                    return null; //no update

                var newGames =
                    SavedChallengeGames.SetItem(
                        boardId,
                        newSavedChallengeGame
                    );

                return new ChallengeGamesHistory(newGames);
            }
            else
            {
                var newGames =
                    SavedChallengeGames.Add(
                        boardId,
                        newSavedChallengeGame
                    );

                return new ChallengeGamesHistory(newGames);
            }
        }

        else
            return null;
    }
}

}
