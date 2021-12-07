using System;

namespace Myriad;

public abstract record TimeSituation
{
    public record Infinite : TimeSituation
    {
        private Infinite() { }
        public static Infinite Instance { get; } = new();

        /// <inheritdoc />
        public override bool IsFinished => false;
    }

    public record Finished : TimeSituation
    {
        private Finished() { }
        public static Finished Instance { get; } = new();

        /// <inheritdoc />
        public override bool IsFinished => true;
    }

    public record FinishAt(DateTime DateTime) : TimeSituation
    {
        /// <inheritdoc />
        public override bool IsFinished => DateTime.CompareTo(DateTime.Now) < 0;
    }

    public abstract bool IsFinished { get; }

    public static TimeSituation Create(int duration)
    {
        TimeSituation ts = duration <= 0
            ? Infinite.Instance
            : new FinishAt(DateTime.Now.AddSeconds(duration));

        return ts;
    }

    public static readonly Setting.Integer Duration = new(
        nameof(Duration),
        -1,
        int.MaxValue,
        120,
        10
    );
}