using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Moggle.Actions;
using TG.Blazor.IndexedDB;

namespace Moggle.Blazor.Flux
{

public class LoadWordsEffect : Effect<StartGameAction>
{
    private readonly IndexedDBManager _database;

    /// <inheritdoc />
    public LoadWordsEffect(IndexedDBManager database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public override async Task HandleAsync(StartGameAction action, IDispatcher dispatcher)
    {
        var (board, solver) = action.GameMode.CreateGame(
            action.Settings,
            new Lazy<WordList>(() => WordList.Empty)
        );

        var uk = board.UniqueKey;

        var savedWords = await _database.GetAllRecordsByIndex<string, SavedWord>(
            new StoreIndexQuery<string>()
            {
                Storename   = nameof(SavedWord),
                IndexName   = nameof(SavedWord.boardId),
                AllMatching = true,
                QueryValue  = uk
            }
        );

        if (savedWords != null && savedWords.Any())
        {
            var legalSavedWords = savedWords.Select(x => solver.CheckLegal(x.wordText))
                .OfType<WordCheckResult.Legal>()
                .ToList();

            dispatcher.Dispatch(new LoadWordsAction(legalSavedWords));
        }
    }
}

}
