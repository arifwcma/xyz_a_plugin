using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SpectralIndexAddIn
{
    /// <summary>
    /// Interaction logic for SpectralIndexDockpaneView.xaml
    /// </summary>
    public partial class SpectralIndexDockpaneView : UserControl
    {
        private static SpectralIndexDockpaneViewModel _viewModel;
        
        // Default values
        private const int DEFAULT_CLOUD_TOLERANCE = 40;
        
        public SpectralIndexDockpaneView()
        {
            InitializeComponent();
            
            try
            {
                // Create or reuse the ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new SpectralIndexDockpaneViewModel();
                }
                
                // Populate controls directly
                PopulateControls();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpectralIndexDockpaneView: ERROR - {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
            }
        }
        
        private void PopulateControls()
        {
            // Populate ComboBox
            if (IndexComboBox != null && _viewModel != null)
            {
                IndexComboBox.Items.Clear();
                foreach (var index in _viewModel.AvailableIndices)
                {
                    IndexComboBox.Items.Add(index);
                }
                IndexComboBox.SelectedItem = _viewModel.SelectedIndex;
                IndexComboBox.SelectionChanged += IndexComboBox_SelectionChanged;
            }
            
            // Set slider value (default 40%)
            if (CloudSlider != null)
            {
                CloudSlider.Value = DEFAULT_CLOUD_TOLERANCE;
                if (_viewModel != null) _viewModel.CloudTolerance = DEFAULT_CLOUD_TOLERANCE;
                CloudSlider.ValueChanged += CloudSlider_ValueChanged;
                UpdateCloudLabel();
            }
            
            // Calculate last calendar month dates
            var today = DateTime.Today;
            var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfLastMonth = firstDayOfCurrentMonth.AddDays(-1);
            var firstDayOfLastMonth = new DateTime(lastDayOfLastMonth.Year, lastDayOfLastMonth.Month, 1);
            
            // Set date pickers to last calendar month
            if (StartDatePicker != null)
            {
                StartDatePicker.SelectedDate = firstDayOfLastMonth;
                if (_viewModel != null) _viewModel.StartDate = firstDayOfLastMonth;
                StartDatePicker.SelectedDateChanged += StartDatePicker_SelectedDateChanged;
            }
            
            if (EndDatePicker != null)
            {
                EndDatePicker.SelectedDate = lastDayOfLastMonth;
                if (_viewModel != null) _viewModel.EndDate = lastDayOfLastMonth;
                EndDatePicker.SelectedDateChanged += EndDatePicker_SelectedDateChanged;
            }
            
            // Set status
            UpdateStatus("Ready");
        }
        
        private void IndexComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && IndexComboBox.SelectedItem != null)
            {
                _viewModel.SelectedIndex = IndexComboBox.SelectedItem.ToString();
            }
        }
        
        private void CloudSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.CloudTolerance = (int)CloudSlider.Value;
                UpdateCloudLabel();
            }
        }
        
        private void UpdateCloudLabel()
        {
            if (CloudLabel != null)
            {
                CloudLabel.Text = $"{(int)CloudSlider.Value}%";
            }
        }
        
        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && StartDatePicker.SelectedDate.HasValue)
            {
                _viewModel.StartDate = StartDatePicker.SelectedDate.Value;
            }
        }
        
        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && EndDatePicker.SelectedDate.HasValue)
            {
                _viewModel.EndDate = EndDatePicker.SelectedDate.Value;
            }
        }
        
        private async void AddLayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            
            try
            {
                UpdateStatus("Creating layer...");
                AddLayerButton.IsEnabled = false;
                
                var layer = await LayerManager.CreateXyzLayerAsync(
                    _viewModel.SelectedIndex,
                    _viewModel.StartDate,
                    _viewModel.EndDate,
                    _viewModel.CloudTolerance);
                
                if (layer != null)
                {
                    UpdateStatus($"Layer '{layer.Name}' added successfully");
                }
                else
                {
                    UpdateStatus("Failed to create layer");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.WriteLine($"AddLayerButton_Click: ERROR - {ex.Message}");
            }
            finally
            {
                AddLayerButton.IsEnabled = true;
            }
        }
        
        private async void UpdateLayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            
            try
            {
                var layer = LayerManager.GetSelectedXyzLayer();
                if (layer == null)
                {
                    UpdateStatus("No Spectral Index layer selected");
                    return;
                }
                
                UpdateStatus("Updating layer...");
                UpdateLayerButton.IsEnabled = false;
                
                var success = await LayerManager.UpdateXyzLayerAsync(
                    layer,
                    _viewModel.SelectedIndex,
                    _viewModel.StartDate,
                    _viewModel.EndDate,
                    _viewModel.CloudTolerance);
                
                if (success)
                {
                    UpdateStatus("Layer updated successfully");
                }
                else
                {
                    UpdateStatus("Failed to update layer");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.WriteLine($"UpdateLayerButton_Click: ERROR - {ex.Message}");
            }
            finally
            {
                UpdateLayerButton.IsEnabled = true;
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (StatusLabel != null)
            {
                StatusLabel.Text = message;
            }
        }
    }
}
