using System;
using System.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace SpectralIndexAddIn
{
    /// <summary>
    /// Manages XYZ tile layers created by this add-in
    /// </summary>
    public class LayerManager
    {
        private const string LAYER_METADATA_KEY = "SpectralIndexAddIn";
        private const string LAYER_NAME_PREFIX = "Spectral Index";

        /// <summary>
        /// Gets the currently selected XYZ tile layer created by this add-in
        /// Returns null if no layer selected, multiple layers selected, or selected layer is not an XYZ layer from this add-in
        /// </summary>
        public static Layer GetSelectedXyzLayer()
        {
            var mapView = MapView.Active;
            if (mapView == null) return null;

            var selectedLayers = mapView.GetSelectedLayers().ToList();
            
            if (selectedLayers.Count != 1) return null;

            var layer = selectedLayers.First();
            
            // Check if it's an XYZ tile layer created by this add-in
            if (IsXyzLayerFromAddIn(layer))
            {
                return layer;
            }

            return null;
        }

        /// <summary>
        /// Checks if a layer is an XYZ tile layer created by this add-in
        /// </summary>
        public static bool IsXyzLayerFromAddIn(Layer layer)
        {
            if (layer == null) return false;

            // Check if it's a tile layer
            if (!(layer is TileLayer tileLayer)) return false;

            // Check if it has our metadata
            try
            {
                var metadata = tileLayer.GetCustomProperty(LAYER_METADATA_KEY);
                return metadata != null;
            }
            catch
            {
                // Check by name prefix as fallback
                return layer.Name.StartsWith(LAYER_NAME_PREFIX);
            }
        }

        /// <summary>
        /// Creates a new XYZ tile layer with the specified parameters
        /// </summary>
        public static async System.Threading.Tasks.Task<Layer> CreateXyzLayerAsync(
            string index, 
            DateTime startDate, 
            DateTime endDate, 
            int cloudTolerance)
        {
            var mapView = MapView.Active;
            if (mapView == null)
                throw new InvalidOperationException("No active map view");

            var map = mapView.Map;
            if (map == null)
                throw new InvalidOperationException("No active map");

            string serviceUrl = ServiceConfig.Instance.ServiceUrl;
            string urlTemplate = $"{serviceUrl}/tiles/{index.ToLower()}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/{cloudTolerance}/{{z}}/{{x}}/{{y}}.png";

            return await QueuedTask.Run(async () =>
            {
                // Create connection properties for XYZ tile layer
                var connectionUri = new Uri(urlTemplate);
                
                // Create tile layer connection
                var tileLayerUrl = new TileLayerURL(connectionUri);
                
                // Create the layer
                var layer = await LayerFactory.Instance.CreateLayerAsync(
                    tileLayerUrl, 
                    map);

                if (layer != null)
                {
                    // Set layer name
                    layer.SetName($"{LAYER_NAME_PREFIX} - {index} ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})");

                    // Store metadata for later identification
                    layer.SetCustomProperty(LAYER_METADATA_KEY, "true");
                    layer.SetCustomProperty("Index", index);
                    layer.SetCustomProperty("StartDate", startDate.ToString("yyyy-MM-dd"));
                    layer.SetCustomProperty("EndDate", endDate.ToString("yyyy-MM-dd"));
                    layer.SetCustomProperty("CloudTolerance", cloudTolerance.ToString());
                    layer.SetCustomProperty("UrlTemplate", urlTemplate);
                }

                return layer;
            });
        }

        /// <summary>
        /// Updates an existing XYZ tile layer with new parameters
        /// </summary>
        public static async System.Threading.Tasks.Task<bool> UpdateXyzLayerAsync(
            Layer layer,
            string index,
            DateTime startDate,
            DateTime endDate,
            int cloudTolerance)
        {
            if (layer == null || !IsXyzLayerFromAddIn(layer))
                return false;

            var mapView = MapView.Active;
            if (mapView == null) return false;

            string serviceUrl = ServiceConfig.Instance.ServiceUrl;
            string urlTemplate = $"{serviceUrl}/tiles/{index.ToLower()}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/{cloudTolerance}/{{z}}/{{x}}/{{y}}.png";

            return await QueuedTask.Run(async () =>
            {
                try
                {
                    if (layer is TileLayer tileLayer)
                    {
                        // Update the connection URI
                        var connectionUri = new Uri(urlTemplate);
                        var tileLayerUrl = new TileLayerURL(connectionUri);

                        // Remove old layer and add new one with updated URL
                        var map = mapView.Map;
                        var layerIndex = map.FindLayer(layer);
                        
                        if (layerIndex >= 0)
                        {
                            // Remove old layer
                            map.RemoveLayer(layer);

                            // Create new layer with updated URL
                            var newLayer = await LayerFactory.Instance.CreateLayerAsync(
                                tileLayerUrl,
                                map);

                            if (newLayer != null)
                            {
                                // Set layer name
                                newLayer.SetName($"{LAYER_NAME_PREFIX} - {index} ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})");

                                // Store metadata
                                newLayer.SetCustomProperty(LAYER_METADATA_KEY, "true");
                                newLayer.SetCustomProperty("Index", index);
                                newLayer.SetCustomProperty("StartDate", startDate.ToString("yyyy-MM-dd"));
                                newLayer.SetCustomProperty("EndDate", endDate.ToString("yyyy-MM-dd"));
                                newLayer.SetCustomProperty("CloudTolerance", cloudTolerance.ToString());
                                newLayer.SetCustomProperty("UrlTemplate", urlTemplate);

                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating layer: {ex.Message}");
                }

                return false;
            });
        }

        /// <summary>
        /// Gets the parameters from an XYZ layer created by this add-in
        /// </summary>
        public static LayerParameters GetLayerParameters(Layer layer)
        {
            if (layer == null || !IsXyzLayerFromAddIn(layer))
                return null;

            try
            {
                var index = layer.GetCustomProperty("Index")?.ToString() ?? "NDVI";
                var startDateStr = layer.GetCustomProperty("StartDate")?.ToString();
                var endDateStr = layer.GetCustomProperty("EndDate")?.ToString();
                var cloudStr = layer.GetCustomProperty("CloudTolerance")?.ToString();

                DateTime startDate = DateTime.TryParse(startDateStr, out DateTime sd) ? sd : DateTime.Now.AddMonths(-1);
                DateTime endDate = DateTime.TryParse(endDateStr, out DateTime ed) ? ed : DateTime.Now;
                int cloudTolerance = int.TryParse(cloudStr, out int ct) ? ct : 20;

                return new LayerParameters
                {
                    Index = index,
                    StartDate = startDate,
                    EndDate = endDate,
                    CloudTolerance = cloudTolerance
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Subscribes to layer selection changes
        /// </summary>
        public static void SubscribeToSelectionChanges(EventHandler<MapViewEventArgs> handler)
        {
            MapSelectionChangedEvent.Subscribe(handler);
        }

        /// <summary>
        /// Unsubscribes from layer selection changes
        /// </summary>
        public static void UnsubscribeFromSelectionChanges(EventHandler<MapViewEventArgs> handler)
        {
            MapSelectionChangedEvent.Unsubscribe(handler);
        }

        public class LayerParameters
        {
            public string Index { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int CloudTolerance { get; set; }
        }
    }
}

