using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Moggle
{

public abstract record Setting(string Name)
{
    public record Integer(
        string Name,
        int Min,
        int Max,
        int Default,
        int Increment = 1) : SettingBase<int>(Name, Default)
    {
        /// <inheritdoc />
        public override bool TryGet(string s, out int value)
        {
            if (int.TryParse(s, out value) && Min < value && value < Max)
                return true;

            return false;
        }
    }

    public record String
        (string Name, string? Pattern, string Default, string Placeholder) : SettingBase<string>(
            Name,
            Default
        )
    {
        /// <inheritdoc />
        public override bool TryGet(string s, out string value)
        {
            value = s;
            return Pattern is null || Regex.IsMatch(s, Pattern);
        }

        public Func<Random, string>? GetRandomValue { get; init; }
    }

    public record Bool(string Name, bool Default) : SettingBase<bool>(Name, Default)
    {
        /// <inheritdoc />
        public override bool TryGet(string s, out bool value)
        {
            if (bool.TryParse(s, out value))
                return true;

            return false;
        }
    }


    public record Enum(string Name, string Default, Func<string, object?> TryGetValue) : SettingBase<string>(Name, Default)
    {
        /// <inheritdoc />
        public override bool TryGet(string s, out string value)
        {
            var v = TryGetValue(s);

            if(v is not null)
            {
                value = v!.ToString()!;
                return true;
            }

            value = Default;
            return false;
        }
    }

    public abstract record SettingBase<T>(string Name, T Default) : Setting(Name)
    {
        public abstract bool TryGet(string s, out T value);

        /// <inheritdoc />
        public override bool IsValid(string s)
        {
            return TryGet(s, out _);
        }

        public T Get(IReadOnlyDictionary<string, string> valueDictionary)
        {
            if (valueDictionary.TryGetValue(Name, out var v) && TryGet(v, out var value))
                return value;

            return Default;
        }

        /// <inheritdoc />
        public override string DefaultString => Default?.ToString()??"";
    }

    public abstract bool IsValid(string s);

    public abstract string DefaultString {get;}
}

}
