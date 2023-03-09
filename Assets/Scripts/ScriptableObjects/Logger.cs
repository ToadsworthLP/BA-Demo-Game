using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Logger", menuName = "Logger")]
public class Logger : ScriptableObject
{
    public class LogEntry
    {
        public enum Category { GAME_START = 0, STAGE_START = 1, RESET = 2, DEATH = 3, STAGE_CLEAR = 4, GAME_CLEAR = 5 }

        public float time;
        public Category category;
        public string context;

        public LogEntry(float time, Category category, string context = "")
        {
            this.time = time;
            this.category = category;
            this.context = context;
        }

        public LogEntry(Category category, string context = "")
        {
            this.time = Time.time;
            this.category = category;
            this.context = context;
        }

        public override string ToString()
        {
            return $"{(int)category}/{Mathf.RoundToInt(time * 1000)}/{context}";
        }
    }

    private List<LogEntry> logEntries = new List<LogEntry>();

    public void Append(LogEntry entry)
    {
        logEntries.Add(entry);
    }

    public void Clear()
    {
        logEntries.Clear();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < logEntries.Count; i++)
        {
            LogEntry entry = logEntries[i];
            if (i != 0) sb.Append("-");
            sb.Append(entry.ToString());
        }

        return sb.ToString();
    }
}
