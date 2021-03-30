using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DetectorApi.Core
{
    public class Config
    {
        #region Local storage

        /// <summary>
        /// Full path where config is located.
        /// </summary>
        private static string StoragePath =>
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "config.json");

        /// <summary>
        /// Cache for faster lookups.
        /// </summary>
        private static Dictionary<string, object> Cache { get; set; }

        /// <summary>
        /// Internal storage.
        /// </summary>
        private static Dictionary<string, object> Storage { get; set; }

        #endregion

        #region IO functions

        /// <summary>
        /// Load config from disk.
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(StoragePath))
            {
                throw new FileNotFoundException($"Unable to find config file: {StoragePath}");
            }

            Storage = JsonSerializer.Deserialize<Dictionary<string, object>>(
                File.ReadAllText(StoragePath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        #endregion

        #region Getters

        /// <summary>
        /// Get string array from config.
        /// </summary>
        /// <param name="keys">Key, with depths, to fetch for.</param>
        /// <returns>List of strings.</returns>
        public static string[] GetStrings(params string[] keys)
        {
            var value = Get(keys);

            if (value == null)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<string[]>(value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get value from config.
        /// </summary>
        /// <param name="keys">Key, with depths, to fetch for.</param>
        /// <returns>Value.</returns>
        public static string Get(params string[] keys)
        {
            if (keys.Length == 0)
            {
                return null;
            }

            var combinedKey = string.Join("::", keys);

            // Get from cache, if present.
            if (Cache != null &&
                Cache.ContainsKey(combinedKey))
            {
                return Cache[combinedKey].ToString();
            }

            // Get from environment, if present.
            var envValue = Environment.GetEnvironmentVariable(combinedKey);

            if (envValue != null)
            {
                return envValue;
            }

            // Get from loaded config file, if present.
            var dict = Storage;

            for (var i = 0; i < keys.Length; i++)
            {
                if (dict == null)
                {
                    return null;
                }

                if (!dict.ContainsKey(keys[i]))
                {
                    return null;
                }

                if (i == keys.Length - 1)
                {
                    Cache ??= new Dictionary<string, object>();

                    if (Cache.ContainsKey(combinedKey))
                    {
                        Cache[combinedKey] = dict[keys[i]];
                    }
                    else
                    {
                        Cache.Add(combinedKey, dict[keys[i]]);
                    }

                    return dict[keys[i]].ToString();
                }

                try
                {
                    var json = dict[keys[i]].ToString();

                    if (json == null)
                    {
                        throw new NullReferenceException($"Config for key {keys[i]} is empty.");
                    }

                    dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    return null;
                }
            }

            // No value found in any of the locations.
            return null;
        }

        #endregion
    }
}