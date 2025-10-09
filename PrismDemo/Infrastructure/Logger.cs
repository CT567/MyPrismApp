using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDemo.Infrastructure
{
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        /// <summary>
        /// 写日志（自动按日期分文件）
        /// </summary>
        public static void Write(string message, string level = "INFO")
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);

                string filePath = Path.Combine(_logDir, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                string logMsg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

                lock (_lock)
                {
                    File.AppendAllText(filePath, logMsg + Environment.NewLine);
                }
            }
            catch {}

        }

        public static void Info(string msg) => Write(msg, "Info");
        public static void Warn(string msg) => Write(msg, "Warn");
        public static void Error(string msg) => Write(msg, "Error");

        public static void SetLogPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("日志路径不能为空");

            _logDir = path;

            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);
        }

    }
}
