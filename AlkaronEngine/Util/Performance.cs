using System;
using System.Linq;
using System.Collections.Generic;

namespace AlkaronEngine.Util
{
   public interface ITimeProvider
   {
      long GetTimestamp();
      long GetFrequency();
   }

   public static class Performance
   {
      private class AggregateEntry
      {
         public double TotalTimeInSeconds;
         public long StartTime;
         public string Description;
         public int Count;
      }

      private class PerfEntry
      {
         public long StartTime;
         public string Description;
      }

      public static bool Enabled;

      public static bool LogLongRunningTasksOnly;
      public static double LongRunningTaskThresholdInSeconds;

      public static ITimeProvider TimeProvider;

      private static Stack<PerfEntry> counterStack;

      private static Dictionary<string, AggregateEntry> aggregateEntries;

      static Performance()
      {
         LogLongRunningTasksOnly = false;
         Enabled = true;
         aggregateEntries = new Dictionary<string, AggregateEntry>();
         counterStack = new Stack<PerfEntry>();
      }

      private static void LogAggregate(AggregateEntry entry)
      {
         bool performLog = true;
         if (LogLongRunningTasksOnly == true)
         {
            performLog = (entry.TotalTimeInSeconds >= LongRunningTaskThresholdInSeconds);
         }

         if (performLog)
         {
            double msecs = (entry.TotalTimeInSeconds * 1000);
            double msecsAvgPerEntry = 0;
            if (entry.Count > 0)
            {
               msecsAvgPerEntry = msecs / (float)entry.Count;
            }

            Log.Write(LogType.Performance, LogSeverity.Debug, entry.Description + " took a total " + msecs + " ms with " +
                      msecsAvgPerEntry + " ms on average per entry with a total of " + entry.Count + " entries.");
         }
      }

      public static void StartAggregateFrame()
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         aggregateEntries.Clear();
      }

      public static void EndAggregateFrame()
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         var values = aggregateEntries.Values.ToList();
         aggregateEntries.Clear();

         for (int i = 0; i < values.Count; i++)
         {
            LogAggregate(values[i]);
         }
      }

      public static void PushAggregate(string description)
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         if (aggregateEntries.ContainsKey(description) == true)
         {
            return;
         }

         AggregateEntry entry = new AggregateEntry();
         entry.Count = 0;
         entry.StartTime = 0;
         entry.Description = description;
         entry.TotalTimeInSeconds = 0;
         aggregateEntries.Add(description, entry);
      }

      public static void StartAppendAggreate(string description)
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         if (aggregateEntries.ContainsKey(description) == false)
         {
            return;
         }

         AggregateEntry entry = aggregateEntries[description];
         entry.StartTime = TimeProvider.GetTimestamp();
      }

      public static void EndAppendAggreate(string description)
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         if (aggregateEntries.ContainsKey(description) == false)
         {
            return;
         }

         AggregateEntry entry = aggregateEntries[description];
         long diff = TimeProvider.GetTimestamp() - entry.StartTime;
         double seconds = (double)diff / (double)TimeProvider.GetFrequency();

         entry.Count++;
         entry.TotalTimeInSeconds += seconds;
      }

      public static void PopAggregate(string description)
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         if (aggregateEntries.ContainsKey(description) == false)
         {
            return;
         }

         AggregateEntry entry = aggregateEntries[description];
         aggregateEntries.Remove(description);

         LogAggregate(entry);
      }

      public static void Push(string description)
      {
         if (Enabled == false || TimeProvider == null)
         {
            return;
         }

         PerfEntry entry = new PerfEntry();
         entry.StartTime = TimeProvider.GetTimestamp();
         entry.Description = description;
         counterStack.Push(entry);

         if (LogLongRunningTasksOnly == false)
         {
            Log.Write(LogType.Performance, LogSeverity.Debug, entry.Description + " starts");
         }
      }

      public static double Pop()
      {
         if (Enabled == false || TimeProvider == null)
         {
            return 0;
         }

         if (counterStack.Count == 0)
         {
            return 0;
         }

         PerfEntry entry = counterStack.Pop();

         long diff = TimeProvider.GetTimestamp() - entry.StartTime;
         double seconds = (double)diff / (double)TimeProvider.GetFrequency();

         bool performLog = true;
         if (LogLongRunningTasksOnly == true)
         {
            performLog = (seconds >= LongRunningTaskThresholdInSeconds);
         }

         if (performLog)
         {
            Log.Write(LogType.Performance, LogSeverity.Debug, entry.Description + " took " + (seconds * 1000) + " ms");
         }

         return seconds;
      }
   }
}

