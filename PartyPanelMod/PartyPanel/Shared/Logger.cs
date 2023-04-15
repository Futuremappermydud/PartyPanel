using PartyPanel;
using System;

namespace PartyPanelShared
{
    class Logger
    {
        private static string prefix = $"[PartyPanel]: ";

        public static void Error(string message)
        {
            Plugin.logger.Error(message);
            /*
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = originalColor;*/
        }

        public static void Warning(string message)
        {
            Plugin.logger.Warn(message);
            /*ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = originalColor; */
        }

        public static void Info(string message)
        {
            Plugin.logger.Info(message);
            /*ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = originalColor; */
        }

        public static void Success(string message)
        {
            Plugin.logger.Info(message);
            /*ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = originalColor; */
        }

        public static void Debug(string message)
        {
#if DEBUG
            Plugin.logger.Info(message);
            /*ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = originalColor; */
#endif
        }
    }
}
