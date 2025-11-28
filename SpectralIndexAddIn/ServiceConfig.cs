using System;
using System.IO;
using System.Text.Json;
using System.Reflection;

namespace SpectralIndexAddIn
{
    /// <summary>
    /// Manages service configuration loaded from config file
    /// </summary>
    public class ServiceConfig
    {
        private static ServiceConfig _instance;
        private static readonly object _lock = new object();

        public string ServiceUrl { get; private set; }
        public string DefaultIndex { get; private set; }
        public int DefaultCloudTolerance { get; private set; }
        public DateTime DefaultStartDate { get; private set; }
        public DateTime DefaultEndDate { get; private set; }

        private ServiceConfig()
        {
            LoadConfiguration();
        }

        public static ServiceConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceConfig();
                        }
                    }
                }
                return _instance;
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                // Get the add-in directory
                string addInPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string configPath = Path.Combine(addInPath, "..", "config", "service.config.json");

                if (!File.Exists(configPath))
                {
                    // Use defaults if config file doesn't exist
                    SetDefaults();
                    return;
                }

                string jsonContent = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<ConfigData>(jsonContent);

                ServiceUrl = config?.ServiceUrl ?? "http://localhost:3000";
                DefaultIndex = config?.DefaultIndex ?? "NDVI";
                DefaultCloudTolerance = config?.DefaultCloudTolerance ?? 20;
                
                if (DateTime.TryParse(config?.DefaultStartDate, out DateTime startDate))
                    DefaultStartDate = startDate;
                else
                    DefaultStartDate = new DateTime(2023, 1, 1);

                if (DateTime.TryParse(config?.DefaultEndDate, out DateTime endDate))
                    DefaultEndDate = endDate;
                else
                    DefaultEndDate = new DateTime(2023, 12, 31);
            }
            catch (Exception ex)
            {
                // Log error and use defaults
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
                SetDefaults();
            }
        }

        private void SetDefaults()
        {
            ServiceUrl = "http://localhost:3000";
            DefaultIndex = "NDVI";
            DefaultCloudTolerance = 20;
            DefaultStartDate = new DateTime(2023, 1, 1);
            DefaultEndDate = new DateTime(2023, 12, 31);
        }

        private class ConfigData
        {
            public string ServiceUrl { get; set; }
            public string DefaultIndex { get; set; }
            public int DefaultCloudTolerance { get; set; }
            public string DefaultStartDate { get; set; }
            public string DefaultEndDate { get; set; }
        }
    }
}

