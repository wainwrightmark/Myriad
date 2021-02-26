using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using TG.Blazor.IndexedDB;

namespace Moggle.Blazor.Flux
{

public class LoadWordsEffect : Effect<StartGameAction>
{
    private readonly IndexedDBManager   _database;

    /// <inheritdoc />
    public LoadWordsEffect(IndexedDBManager   database) {
        _database = database;
    }


    /// <inheritdoc />
    public override async Task HandleAsync(StartGameAction action, IDispatcher dispatcher)
    {
        //await _database.OpenIndexedDb();

        var board = action.GameMode.CreateGame(action.Settings, new Lazy<WordList>(()=>WordList.Empty)).board;
        var uk =board.UniqueKey;

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
            dispatcher.Dispatch(new LoadWordsAction(savedWords.ToList()));
    }
}

}
