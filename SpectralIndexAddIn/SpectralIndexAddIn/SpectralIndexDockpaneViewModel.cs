using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace SpectralIndexAddIn
{
    /// <summary>
    /// ViewModel for the Spectral Index DockPane
    /// Implements MVVM pattern with data binding
    /// </summary>
    public class SpectralIndexDockpaneViewModel : INotifyPropertyChanged
    {
        private string _selectedIndex = "NDVI";
        private int _cloudTolerance = 20;
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;
        private bool _isLayerSelected = false;
        private bool _canUpdateLayer = false;
        private string _statusMessage = "Select a layer or add a new one";
        private LayerManager.LayerParameters _currentLayerParams;

        private readonly string[] _availableIndices = new[]
        {
            "NDVI", "EVI", "SAVI", "OSAVI", "GNDVI", 
            "NDSI", "ARVI", "NDWI", "MNDWI"
        };

        public SpectralIndexDockpaneViewModel()
        {
            System.Diagnostics.Debug.WriteLine("SpectralIndexDockpaneViewModel: Constructor called");
            
            // Load defaults from config
            var config = ServiceConfig.Instance;
            System.Diagnostics.Debug.WriteLine($"SpectralIndexDockpaneViewModel: Config loaded - ServiceUrl={config.ServiceUrl}");
            
            _selectedIndex = config.DefaultIndex;
            _cloudTolerance = config.DefaultCloudTolerance;
            _startDate = config.DefaultStartDate;
            _endDate = config.DefaultEndDate;

            System.Diagnostics.Debug.WriteLine($"SpectralIndexDockpaneViewModel: Defaults set - Index={_selectedIndex}, Cloud={_cloudTolerance}, StartDate={_startDate:yyyy-MM-dd}, EndDate={_endDate:yyyy-MM-dd}");

            // Initialize commands
            AddLayerCommand = new RelayCommand(OnAddLayer, CanAddLayer);
            UpdateLayerCommand = new RelayCommand(OnUpdateLayer, CanExecuteUpdateLayer);

            // Subscribe to layer selection changes
            LayerManager.SubscribeToSelectionChanges(OnLayerSelectionChanged);
            
            System.Diagnostics.Debug.WriteLine("SpectralIndexDockpaneViewModel: Initialization complete");
        }

        public string[] AvailableIndices => _availableIndices;

        public string SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    ValidateAndUpdateStatus();
                }
            }
        }

        public int CloudTolerance
        {
            get => _cloudTolerance;
            set
            {
                if (_cloudTolerance != value)
                {
                    _cloudTolerance = Math.Max(0, Math.Min(100, value));
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CloudToleranceLabel));
                    ValidateAndUpdateStatus();
                }
            }
        }

        public string CloudToleranceLabel => $"{CloudTolerance}%";

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                    ValidateAndUpdateStatus();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                    ValidateAndUpdateStatus();
                }
            }
        }

        public bool IsLayerSelected
        {
            get => _isLayerSelected;
            set
            {
                if (_isLayerSelected != value)
                {
                    _isLayerSelected = value;
                    OnPropertyChanged();
                    UpdateCanUpdateLayer();
                }
            }
        }

        public bool CanUpdateLayer
        {
            get => _canUpdateLayer;
            set
            {
                if (_canUpdateLayer != value)
                {
                    _canUpdateLayer = value;
                    OnPropertyChanged();
                    ((RelayCommand)UpdateLayerCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddLayerCommand { get; }
        public ICommand UpdateLayerCommand { get; }

        private bool CanAddLayer()
        {
            return ValidateParameters();
        }

        private async void OnAddLayer()
        {
            if (!ValidateParameters())
            {
                StatusMessage = "Please fix validation errors before adding layer";
                return;
            }

            try
            {
                StatusMessage = "Creating layer...";
                var layer = await LayerManager.CreateXyzLayerAsync(
                    SelectedIndex,
                    StartDate,
                    EndDate,
                    CloudTolerance);

                if (layer != null)
                {
                    StatusMessage = $"Layer '{layer.Name}' added successfully";
                }
                else
                {
                    StatusMessage = "Failed to create layer";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private bool CanExecuteUpdateLayer()
        {
            return CanUpdateLayer && ValidateParameters();
        }

        private async void OnUpdateLayer()
        {
            var layer = LayerManager.GetSelectedXyzLayer();
            if (layer == null)
            {
                StatusMessage = "No layer selected";
                return;
            }

            if (!ValidateParameters())
            {
                StatusMessage = "Please fix validation errors before updating layer";
                return;
            }

            try
            {
                StatusMessage = "Updating layer...";
                bool success = await LayerManager.UpdateXyzLayerAsync(
                    layer,
                    SelectedIndex,
                    StartDate,
                    EndDate,
                    CloudTolerance);

                if (success)
                {
                    StatusMessage = "Layer updated successfully";
                    // Refresh layer parameters
                    LoadLayerParameters(layer);
                }
                else
                {
                    StatusMessage = "Failed to update layer";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void OnLayerSelectionChanged(MapSelectionChangedEventArgs e)
        {
            // Check if we're on the UI thread
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                UpdateLayerSelection();
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => UpdateLayerSelection());
            }
        }

        private void UpdateLayerSelection()
        {
            var layer = LayerManager.GetSelectedXyzLayer();
            IsLayerSelected = layer != null;

            if (layer != null)
            {
                LoadLayerParameters(layer);
                StatusMessage = $"Layer '{layer.Name}' selected";
            }
            else
            {
                _currentLayerParams = null;
                var mapView = ArcGIS.Desktop.Mapping.MapView.Active;
                if (mapView != null)
                {
                    var selectedCount = mapView.GetSelectedLayers().Count();
                    if (selectedCount == 0)
                        StatusMessage = "No layer selected";
                    else if (selectedCount > 1)
                        StatusMessage = "Multiple layers selected - select a single layer";
                    else
                        StatusMessage = "Selected layer is not a Spectral Index layer";
                }
                else
                {
                    StatusMessage = "No active map view";
                }
            }

            UpdateCanUpdateLayer();
        }

        private void LoadLayerParameters(ArcGIS.Desktop.Mapping.Layer layer)
        {
            var layerParams = LayerManager.GetLayerParameters(layer);
            if (layerParams != null)
            {
                _currentLayerParams = layerParams;
                SelectedIndex = layerParams.Index;
                StartDate = layerParams.StartDate;
                EndDate = layerParams.EndDate;
                CloudTolerance = layerParams.CloudTolerance;
            }
        }

        private void UpdateCanUpdateLayer()
        {
            CanUpdateLayer = IsLayerSelected && ValidateParameters();
        }

        private bool ValidateParameters()
        {
            if (StartDate > EndDate)
            {
                StatusMessage = "Start date must be before end date";
                return false;
            }

            if (CloudTolerance < 0 || CloudTolerance > 100)
            {
                StatusMessage = "Cloud tolerance must be between 0 and 100";
                return false;
            }

            if (string.IsNullOrEmpty(SelectedIndex))
            {
                StatusMessage = "Please select a spectral index";
                return false;
            }

            return true;
        }

        private void ValidateAndUpdateStatus()
        {
            if (!ValidateParameters() && !IsLayerSelected)
            {
                // Status already set by ValidateParameters
            }
            else if (IsLayerSelected)
            {
                UpdateCanUpdateLayer();
            }
        }

        public void Cleanup()
        {
            LayerManager.UnsubscribeFromSelectionChanges(OnLayerSelectionChanged);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Simple relay command implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

