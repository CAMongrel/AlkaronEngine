using System;
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
      private struct PerfEntry
      {
         public long StartTime;
         public string Description;
      }

      public static bool Enabled;

      public static bool LogLongRunningTasksOnly;
      public static double LongRunningTaskThresholdInSeconds;

      public static ITimeProvider TimeProvider;

      private static Stack<PerfEntry> counterStack;

      static Performance()
      {
         LogLongRunningTasksOnly = false;
         Enabled = true;
         counterStack = new Stack<PerfEntry>();
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
            Log.Write(LogType.Performance, LogSeverity.Debug, entry.Description + " starts");
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
            Log.Write(LogType.Performance, LogSeverity.Debug, entry.Description + " took " + (seconds * 1000) + " ms");

         return seconds;
      }
   }
}

