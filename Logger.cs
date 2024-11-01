using NLog.Config;
using NLog.Targets;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace AnlaxBimManager
{
    public static class BaseLogManager
    {
        private static readonly Logger logger;

        public static string PathToTxt
        {
            get
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var locationDllBase = Path.GetDirectoryName(assemblyLocation);
                string pathToTxt = Path.Combine(locationDllBase, "AnlaxBaseLog.txt");
                return pathToTxt;
            }
        }

        static BaseLogManager()
        {
            // Инициализируем `logger` в статическом конструкторе
            logger = NLog.LogManager.GetCurrentClassLogger();
            ConfigureLogging();

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                logger.Fatal(exception, "Domain unhandled exception");
                LogManager.Shutdown();
            };
        }

        private static void ConfigureLogging()
        {

            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("logfile")
            {
                FileName = PathToTxt,
                Layout = "${longdate} [${level:uppercase=true:padding=3}] ${message} ${exception}",
                ConcurrentWrites = true
            };

            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);

            NLog.LogManager.Configuration = config;
        }

        public static void LogInfo(string message)
        {
            logger.Info(message);
        }

        public static void LogFatal(Exception exception, string message)
        {
            logger.Fatal(exception, message);
        }
        public static void LogWarning(string message)
        {
            logger.Warn(message);
        }

        public static void LogError(string message)
        {
            logger.Error(message);
        }
    }


}
