using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
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
        private const string LAYER_NAME_PREFIX = "Spectral Index";
        
        // Australia bounding box (WGS84)
        private const double AUSTRALIA_MIN_LON = 112.0;  // West
        private const double AUSTRALIA_MAX_LON = 154.0;  // East
        private const double AUSTRALIA_MIN_LAT = -45.0;  // South
        private const double AUSTRALIA_MAX_LAT = -10.0;  // North
        
        // Store layer parameters in memory since CIM custom properties are complex
        private static Dictionary<string, LayerParameters> _layerParams = new Dictionary<string, LayerParameters>();

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

            // Check by name prefix
            return layer.Name.StartsWith(LAYER_NAME_PREFIX);
        }

        /// <summary>
        /// Creates a new XYZ tile layer with the specified parameters
        /// Layer extent is limited to Australia bounding box
        /// Does not change the current map extent
        /// </summary>
        public static async Task<Layer> CreateXyzLayerAsync(
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
            string urlTemplate = $"{serviceUrl}/tiles/{index.ToLower()}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/{cloudTolerance}/{{level}}/{{col}}/{{row}}.png";

            string layerName = $"{LAYER_NAME_PREFIX} - {index} ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})";

            return await QueuedTask.Run(() =>
            {
                // Create layer using URI
                var uri = new Uri(urlTemplate);
                var layer = LayerFactory.Instance.CreateLayer(uri, map, 0, layerName);

                if (layer != null)
                {
                    // Store parameters for later retrieval
                    _layerParams[layer.Name] = new LayerParameters
                    {
                        Index = index,
                        StartDate = startDate,
                        EndDate = endDate,
                        CloudTolerance = cloudTolerance,
                        UrlTemplate = urlTemplate
                    };
                    
                    // Try to set the layer extent to Australia bounding box
                    try
                    {
                        SetLayerExtentToAustralia(layer);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not set Australia extent: {ex.Message}");
                    }
                }

                return layer;
            });
        }

        /// <summary>
        /// Sets the layer's extent to Australia bounding box and scale limits
        /// Layer only visible at 1:25,000 or more zoomed in
        /// </summary>
        private static void SetLayerExtentToAustralia(Layer layer)
        {
            if (layer == null) return;
            
            // Get the layer's CIM definition and set the scale limits
            var layerDef = layer.GetDefinition();
            if (layerDef != null)
            {
                // MinScale = most zoomed OUT scale at which layer is visible
                // MaxScale = most zoomed IN scale at which layer is visible (0 = no limit)
                // Layer visible only at 1:25,000 or more zoomed in
                layerDef.MinScale = 25000;  // Layer hidden when zoomed out beyond 1:25,000
                layerDef.MaxScale = 0;      // No maximum zoom limit
                
                layer.SetDefinition(layerDef);
            }
        }

        /// <summary>
        /// Updates an existing XYZ tile layer with new parameters
        /// Does not change the current map extent
        /// </summary>
        public static async Task<bool> UpdateXyzLayerAsync(
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
            string urlTemplate = $"{serviceUrl}/tiles/{index.ToLower()}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/{cloudTolerance}/{{level}}/{{col}}/{{row}}.png";
            string layerName = $"{LAYER_NAME_PREFIX} - {index} ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})";

            return await QueuedTask.Run(() =>
            {
                try
                {
                    var map = mapView.Map;
                    
                    // Remove old parameters
                    if (_layerParams.ContainsKey(layer.Name))
                    {
                        _layerParams.Remove(layer.Name);
                    }

                    // Remove old layer
                    map.RemoveLayer(layer);

                    // Create new layer with updated URL
                    var uri = new Uri(urlTemplate);
                    var newLayer = LayerFactory.Instance.CreateLayer(uri, map, 0, layerName);

                    if (newLayer != null)
                    {
                        // Store new parameters
                        _layerParams[newLayer.Name] = new LayerParameters
                        {
                            Index = index,
                            StartDate = startDate,
                            EndDate = endDate,
                            CloudTolerance = cloudTolerance,
                            UrlTemplate = urlTemplate
                        };
                        
                        // Try to set the layer extent to Australia bounding box
                        try
                        {
                            SetLayerExtentToAustralia(newLayer);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Could not set Australia extent: {ex.Message}");
                        }
                        
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating layer: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Gets the parameters from an XYZ layer created by this add-in
        /// </summary>
        public static LayerParameters GetLayerParameters(Layer layer)
        {
            if (layer == null || !IsXyzLayerFromAddIn(layer))
                return null;

            // Try to get from memory cache
            if (_layerParams.TryGetValue(layer.Name, out var parameters))
            {
                return parameters;
            }

            // Calculate last month dates for defaults
            var today = DateTime.Today;
            var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfLastMonth = firstDayOfCurrentMonth.AddDays(-1);
            var firstDayOfLastMonth = new DateTime(lastDayOfLastMonth.Year, lastDayOfLastMonth.Month, 1);

            // Return defaults if not found
            return new LayerParameters
            {
                Index = "NDVI",
                StartDate = firstDayOfLastMonth,
                EndDate = lastDayOfLastMonth,
                CloudTolerance = 40
            };
        }

        /// <summary>
        /// Subscribes to layer selection changes
        /// </summary>
        public static void SubscribeToSelectionChanges(Action<MapSelectionChangedEventArgs> handler)
        {
            MapSelectionChangedEvent.Subscribe(handler);
        }

        /// <summary>
        /// Unsubscribes from layer selection changes
        /// </summary>
        public static void UnsubscribeFromSelectionChanges(Action<MapSelectionChangedEventArgs> handler)
        {
            MapSelectionChangedEvent.Unsubscribe(handler);
        }

        public class LayerParameters
        {
            public string Index { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int CloudTolerance { get; set; }
            public string UrlTemplate { get; set; }
        }
    }
}
