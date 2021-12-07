﻿using System.Collections.Immutable;
using Fluxor;

namespace Myriad.States;

public class GameSettingsFeature : Feature<GameSettingsState>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "Previous Game";
    }

    /// <inheritdoc />
    protected override GameSettingsState GetInitialState()
    {
        return new(
            NumbersGameMode.Instance,
            ImmutableDictionary<string, string>.Empty
        );
    }
}