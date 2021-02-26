using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using TG.Blazor.IndexedDB;

namespace Moggle.Blazor.Flux
{

public class SaveWordEffect : Effect<MoveAction>
{
    private readonly IndexedDBManager   _database;
    public SaveWordEffect(IndexedDBManager  database) {
        _database = database;

    }

    /// <inheritdoc />
    public override async Task HandleAsync(MoveAction action, IDispatcher dispatcher)
    {


        if (action.Result is MoveResult.WordComplete wc)
        {
            await _database.AddRecord(
                new StoreRecord<SavedWord>()
                {
                    Storename = nameof(SavedWord),
                    Data = new SavedWord() { boardId = action.BoardId, wordText = wc.FoundWord.Text }
                }
            );
        }
    }
}

}
