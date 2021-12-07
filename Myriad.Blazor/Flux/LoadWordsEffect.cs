using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Myriad.Actions;
using TG.Blazor.IndexedDB;

namespace Myriad.Blazor.Flux;

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
        var board = action.GameMode.CreateBoard(action.Settings, new Lazy<WordList>(() => WordList.Empty));
        var solver = action.GameMode.CreateSolver(action.Settings, new Lazy<WordList>(() => WordList.Empty));

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
            //TODO fix this
            var legalSavedWords = savedWords.Select(x => solver.CheckLegal(x.wordText, x.GetCoordinates().ToImmutableList()))
                .OfType<WordCheckResult.Legal>()
                .ToList();

            dispatcher.Dispatch(new LoadWordsAction(legalSavedWords));


        }
    }
}
