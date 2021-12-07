using System.Threading.Tasks;
using Fluxor;
using Myriad.Actions;
using TG.Blazor.IndexedDB;

namespace Myriad.Blazor.Flux;

public class ResetWordsEffect : Effect<ResetFoundWordsAction>
{
    private readonly IndexedDBManager _database;

    /// <inheritdoc />
    public ResetWordsEffect(IndexedDBManager database) {
        _database = database;
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ResetFoundWordsAction action, IDispatcher dispatcher)
    {

        var uk = action.Board.UniqueKey;

        var savedWords =
            await
                _database.GetAllRecordsByIndex<string, SavedWord>(new StoreIndexQuery<string>()
                {
                    Storename   = nameof(SavedWord),
                    IndexName   = nameof(SavedWord.boardId),
                    AllMatching = true,
                    QueryValue  = uk
                });

        foreach (var sw in savedWords)
        {
            await _database.DeleteRecord(nameof(SavedWord), sw.uniqueId);
        }
    }
}