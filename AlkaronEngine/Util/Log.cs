using System;

// TODO: Make platform specific log writer

namespace AlkaronEngine.Util
{
   [Flags]
   public enum LogType
   {
      None = 0,
      Generic = 1,
      Resources = 2,
      AI = 4,
      Performance = 8,
      Game = Generic | AI,
      All = Generic | Resources | AI | Performance,
   }

   public enum LogSeverity
   {
      Fatal,
      Error,
      Warning,
      Status,
      Debug,
   }

   public interface ILogWriter
   {
      void WriteLine(string line, params object[] args);
   }

   public static class Log
   {
      public static LogType Type = LogType.Game;
      public static LogSeverity Severity = LogSeverity.Debug;

      public static ILogWriter LogWriter;

      private static string GetTimestamp()
      {
         return "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.ffff") + "] ";
      }

      private static void LogInternal(LogType type, LogSeverity severity, string message, bool forced = false)
      {
         bool shouldLog = (forced || (((int)severity <= (int)Severity) && ((Type & type) == type)));
         if (shouldLog == true)
         {
            LogWriter?.WriteLine(GetTimestamp() + "[" + severity + "] [" + type + "]: " + message);
         }
      }

      public static void Write(LogType type, LogSeverity severity, string message, bool forced = false)
      {
         LogInternal(type, severity, message);
      }

      public static void Status(string message)
      {
         LogInternal(LogType.Generic, LogSeverity.Status, message);
      }

      public static void Warning(string message)
      {
         LogInternal(LogType.Generic, LogSeverity.Warning, message);
      }

      public static void Error(string message)
      {
         LogInternal(LogType.Generic, LogSeverity.Error, message);
      }

      public static void Forced(string message)
      {
         LogInternal(LogType.Game, LogSeverity.Status, message, true);
      }
   }
}

