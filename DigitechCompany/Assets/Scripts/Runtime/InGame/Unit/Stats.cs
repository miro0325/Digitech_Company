using System;
using System.Collections.Generic;
using System.Text;
using UniRx;

public partial class Stats
{
    //define
    public enum Key
    {
        Hp,
        Strength,
        Speed,
        Stamina,
        End
    }

    //field
    private ReactiveCollection<float> stats;

    //event
    public event Action<Key, float, float> OnStatChanged;

    //method
    public Stats()
    {
        stats = new();
        for(int i = 0; i < (int)Key.End; i++)
            stats.Add(0);

        stats
            .ObserveReplace()
            .Subscribe(e => OnStatChanged?.Invoke((Key)e.Index, e.OldValue, e.NewValue));
    }

    public float this[Key key]
    {
        get => GetStat(key);
        set => ModifyStat(key, x => value);
    }

    public float GetStat(Key key)
    {
        return stats[(int)key];
    }

    public IReadOnlyList<float> GetStats()
    {
        return stats;
    }

    public void ModifyStat(Key key, Func<float, float> modifier)
    {
        stats[(int)key] = modifier(stats[(int)key]);
    }

    public Stats Clone()
    {
        var stats = new Stats();
        stats.ChangeFrom(this);
        return stats;
    }

    public void ChangeFrom(Stats target)
    {
        var stats = target.GetStats();
        for(int i = 0; i < stats.Count; i++)
            ModifyStat((Key)i, x => stats[i]);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var stat in stats)
            sb.Append(stat.ToString()).Append('\n');
        return sb.ToString();
    }
}