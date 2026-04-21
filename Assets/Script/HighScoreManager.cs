using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RunRecord
{
    public int level;
    public int wave;
    public long timestampTicks;

    public RunRecord() { }

    public RunRecord(int level, int wave)
    {
        this.level = level;
        this.wave = wave;
        this.timestampTicks = DateTime.UtcNow.Ticks;
    }

    public DateTime Timestamp => new DateTime(timestampTicks, DateTimeKind.Utc);
}

[Serializable]
public class RunRecordList
{
    public List<RunRecord> records = new List<RunRecord>();
}

public static class HighScoreManager
{
    private const string RunsKey = "ZomBarrage_Runs";
    private const string HighestLevelKey = "ZomBarrage_HighestLevel";
    private const string HighestWaveKey = "ZomBarrage_HighestWave";
    private const int MaxStoredRuns = 10;

    public static int HighestLevel => PlayerPrefs.GetInt(HighestLevelKey, 0);
    public static int HighestWave => PlayerPrefs.GetInt(HighestWaveKey, 0);

    public static void RecordRun(int level, int wave)
    {
        List<RunRecord> runs = LoadRuns();
        runs.Add(new RunRecord(level, wave));
        SortByScoreDescending(runs);
        TrimToMax(runs, MaxStoredRuns);
        SaveRuns(runs);

        if (level > HighestLevel)
        {
            PlayerPrefs.SetInt(HighestLevelKey, level);
        }
        if (wave > HighestWave)
        {
            PlayerPrefs.SetInt(HighestWaveKey, wave);
        }
        PlayerPrefs.Save();
    }

    public static List<RunRecord> GetTopRuns(int count = MaxStoredRuns)
    {
        List<RunRecord> runs = LoadRuns();
        SortByScoreDescending(runs);
        if (runs.Count > count) runs = runs.GetRange(0, count);
        return runs;
    }

    public static int FindRankByLevel(int level)
    {
        List<RunRecord> runs = GetTopRuns(MaxStoredRuns);
        int lo = 0;
        int hi = runs.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            int candidate = runs[mid].level;
            if (candidate == level) return mid;
            if (candidate > level) lo = mid + 1;
            else hi = mid - 1;
        }
        return -1;
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(RunsKey);
        PlayerPrefs.DeleteKey(HighestLevelKey);
        PlayerPrefs.DeleteKey(HighestWaveKey);
        PlayerPrefs.Save();
    }

    private static void SortByScoreDescending(List<RunRecord> runs)
    {
        runs.Sort((a, b) =>
        {
            int cmp = b.level.CompareTo(a.level);
            if (cmp != 0) return cmp;
            return b.wave.CompareTo(a.wave);
        });
    }

    private static void TrimToMax(List<RunRecord> runs, int max)
    {
        while (runs.Count > max) runs.RemoveAt(runs.Count - 1);
    }

    private static List<RunRecord> LoadRuns()
    {
        string json = PlayerPrefs.GetString(RunsKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return new List<RunRecord>();
        try
        {
            RunRecordList wrapper = JsonUtility.FromJson<RunRecordList>(json);
            return wrapper != null && wrapper.records != null ? wrapper.records : new List<RunRecord>();
        }
        catch
        {
            return new List<RunRecord>();
        }
    }

    private static void SaveRuns(List<RunRecord> runs)
    {
        RunRecordList wrapper = new RunRecordList { records = runs };
        PlayerPrefs.SetString(RunsKey, JsonUtility.ToJson(wrapper));
    }
}
