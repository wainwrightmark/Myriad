using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Myriad.States;
using TG.Blazor.IndexedDB;

namespace Myriad.Blazor;

public class SavedChallengeGameEffect : Effect<SaveGameAction>
{
    private readonly IndexedDBManager _database;

    public SavedChallengeGameEffect(IndexedDBManager database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public override async Task HandleAsync(SaveGameAction action, IDispatcher dispatcher)
    {
        if (action.FoundWordsState.Data is FoundWordsData.TargetWordsData twd)
        {
            var newSavedChallengeGame = new SavedChallengeGame
            {
                boardId        = action.BoardId,
                foundSolutions = twd.WordsToFind.Count(x => x.Value.word is not null),
                maxSolutions   = twd.WordsToFind.Count
            };

            await _database.UpdateRecord(
                new StoreRecord<SavedChallengeGame>()
                {
                    Data = newSavedChallengeGame, Storename = nameof(SavedChallengeGame)
                }
            );

            //var current = await
            //    _database.GetRecordByIndex<string, SavedChallengeGame>(
            //        new StoreIndexQuery<string>()
            //        {
            //            Storename  = nameof(SavedChallengeGame),
            //            IndexName  = nameof(SavedChallengeGame.boardId),
            //            QueryValue = action.BoardId
            //        }
            //    );

            //if (current is not null)
            //{

            //}
            //else
            //{
            //    await _database.AddRecord(
            //        new StoreRecord<SavedChallengeGame>()
            //        {
            //            Data = newSavedChallengeGame, Storename = nameof(SavedChallengeGame)
            //        }
            //    );
            //}
        }
    }
}

public class SavedChallengeGamesService : IDisposable
{
    private readonly IDispatcher _dispatcher;

    private readonly IState<FoundWordsState> _foundWords;

    private readonly IState<Board> _board;

    private readonly IndexedDBManager _database;

    public SavedChallengeGamesService(
        IDispatcher dispatcher,
        IState<FoundWordsState> foundWords,
        IndexedDBManager database,
        IState<Board> board)
    {
        Console.WriteLine($"{nameof(SavedChallengeGamesService)} loaded");
        _dispatcher = dispatcher;
        _foundWords = foundWords;
        _database   = database;
        _board      = board;

        LoadGames().AndForget();

        _foundWords.StateChanged += FoundWordsOnStateChanged;
    }

    private void FoundWordsOnStateChanged(object? sender, FoundWordsState e)
    {
        _dispatcher.Dispatch(new SaveGameAction(_board.Value.UniqueKey, e));
    }

    async Task LoadGames()
    {
        var games = await _database.GetRecords<SavedChallengeGame>(nameof(SavedChallengeGame));

        if (games is not null)
            _dispatcher.Dispatch(new LoadSavedChallengeGamesAction(games));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _foundWords.StateChanged -= FoundWordsOnStateChanged;
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddSingleton(
            new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
        );

        builder.Services.AddFluxor(
            options => options.ScanAssemblies(
                typeof(Program).Assembly,
                typeof(Board).Assembly
            )
        );

        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped<SavedChallengeGamesService>();

        builder.Services.AddMudServices();

        builder.Services.AddIndexedDB(
            dbStore =>
            {
                dbStore.DbName  = "Myriad";
                dbStore.Version = 2;

                dbStore.Stores.Add(
                    new StoreSchema()
                    {
                        Name = nameof(SavedChallengeGame),
                        PrimaryKey =
                            new IndexSpec
                            {
                                Name    = nameof(SavedChallengeGame.boardId),
                                Auto    = false,
                                KeyPath = nameof(SavedChallengeGame.boardId),
                            },
                        Indexes = new List<IndexSpec>()
                        {
                            new()
                            {
                                Name    = nameof(SavedChallengeGame.boardId),
                                KeyPath = nameof(SavedChallengeGame.boardId),
                                Auto    = false
                            },
                            new()
                            {
                                Name    = nameof(SavedChallengeGame.maxSolutions),
                                KeyPath = nameof(SavedChallengeGame.maxSolutions),
                                Auto    = false
                            },
                            new()
                            {
                                Name    = nameof(SavedChallengeGame.foundSolutions),
                                KeyPath = nameof(SavedChallengeGame.foundSolutions),
                                Auto    = false
                            }
                        }
                    }
                );

                dbStore.Stores.Add(
                    new StoreSchema()
                    {
                        Name = nameof(SavedWord),
                        PrimaryKey =
                            new IndexSpec()
                            {
                                Name    = nameof(SavedWord.uniqueId),
                                Auto    = true,
                                KeyPath = nameof(SavedWord.uniqueId),
                            },
                        Indexes = new List<IndexSpec>()
                        {
                            new()
                            {
                                Name    = nameof(SavedWord.boardId),
                                KeyPath = nameof(SavedWord.boardId),
                                Auto    = false
                            },
                            new()
                            {
                                Name    = nameof(SavedWord.wordText),
                                KeyPath = nameof(SavedWord.wordText),
                                Auto    = false
                            },
                            new()
                            {
                                Name    = nameof(SavedWord.coordinateString),
                                KeyPath = nameof(SavedWord.coordinateString),
                                Auto    = false
                            }
                        }
                    }
                );
            }
        );

        var host = builder.Build();

        await host.RunAsync();
    }
}