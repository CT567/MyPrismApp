using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrismDemo.Infrastructure
{
    public static class ConfigHelper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true
        };

        /// <summary>
        /// 读取配置
        /// </summary>
        public static T Load<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                // 文件不存在就返回默认对象，并保存一份
                var defaultObj = new T();
                Save(filePath, defaultObj);
                return defaultObj;
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, _options) ?? new T();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save<T>(string filePath, T obj)
        {
            string json = JsonSerializer.Serialize(obj, _options);
            File.WriteAllText(filePath, json);
        }










    }
}
