﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Myriad.States;

namespace Myriad;

public record ChallengeGameMode : IGameMode
{
    private ChallengeGameMode() { }
    public static ChallengeGameMode Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Challenge";

    /// <inheritdoc />
    public Board CreateBoard(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var game  = GetGame(settings);
        var board = Board.Create(game.grid, '_');

        return board;
    }

    /// <inheritdoc />
    public Solver CreateSolver(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var game = GetGame(settings);

        return new Solver(
            WordList.FromWords(game.words),
            new SolveSettings(game.words.Min(x => x.Length), false, null)
        );
    }

    /// <inheritdoc />
    public FoundWordsData GetFoundWordsData(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var game = GetGame(settings);

        var twg = new FoundWordsData.TargetWordGroup(game.group, 0, true);

        var solutions = game.words
            .ToImmutableDictionary(x => x, _ => (twg, null as FoundWord), StringComparer.OrdinalIgnoreCase);

        return new FoundWordsData.TargetWordsData(solutions);
    }

    /// <inheritdoc />
    public TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings)
    {
        return TimeSituation.Infinite.Instance;
    }

    private (string group, string grid, IReadOnlyCollection<string> words) GetGame(
        ImmutableDictionary<string, string> settings) =>

        GoodSeedHelper.GetChallengeGame(Concept.Get(settings));

    /// <inheritdoc />
    public Animation? GetAnimation(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        if (Animate.Get(settings))
        {
            var game = GetGame(settings);

            var board = CreateBoard(settings, wordList);
            return Animation.Create(game.words, board);
        }

        return null;
    }

    /// <inheritdoc />
    public IEnumerable<Letter> LegalLetters { get; } = Letter.CreateFromString("abcdefghijklmnopqrstuvwxyz");

    public virtual Setting.Bool Animate => new(nameof(Animate), false);

    public Setting.StringChoice Concept { get; } =  new(nameof(Concept), GoodSeedHelper.GoodChallengeGames.Value.Select(x=>x.group).ToImmutableList());

    /// <inheritdoc />
    public IEnumerable<Setting> Settings
    {
        get
        {
            yield return Concept;
            yield return Animate;
        }
    }
}