# ArcGIS Pro XYZ Tiles Add-In Implementation Plan

## Overview
Create an ArcGIS Pro add-in (using ArcGIS Pro SDK for .NET) that provides a dockable pane interface for managing XYZ tile layers from the tile service. The pane will remain open and non-blocking, with controls that activate only when a single XYZ tile layer is selected.

## Architecture Decision
**Use ArcGIS Pro SDK for .NET (C#)** rather than Python add-ins because:
- Better UI support for dockable panes with complex controls
- Native WPF/XAML support for standard ArcGIS controls
- Better event handling for layer selection
- More efficient layer URL updates

## Project Structure

```
arcgis-pro-xyz-addin/
├── config/
│   └── service.config.json          # Service URL configuration
├── SpectralIndexAddIn/
│   ├── SpectralIndexAddIn.csproj    # C# project file
│   ├── Config.daml                  # Add-in manifest
│   ├── SpectralIndexAddIn.cs        # Main add-in class
│   ├── SpectralIndexDockpaneView.xaml        # UI layout (XAML)
│   ├── SpectralIndexDockpaneViewModel.cs     # ViewModel (MVVM pattern)
│   ├── SpectralIndexDockpane.cs              # DockPane implementation
│   ├── LayerManager.cs              # Layer detection and URL updates
│   ├── ServiceConfig.cs             # Configuration management
│   └── Properties/
│       └── AssemblyInfo.cs
└── README.md
```

## Implementation Details

### 1. Configuration Management (`ServiceConfig.cs`)
- Read service URL from `config/service.config.json`
- Default to `http://localhost:3000` for development
- Provide method to update config for production deployment
- Load configuration at add-in startup

### 2. DockPane UI (`SpectralIndexDockpaneView.xaml`)
- **Spectral Index Dropdown**: ComboBox with values: NDVI, EVI, SAVI, OSAVI, GNDVI, NDSI, ARVI, NDWI, MNDWI
- **Cloud Tolerance Slider**: Range 0-100, with label showing current value
- **Start Date**: DatePicker control
- **End Date**: DatePicker control
- **Add Layer Button**: Creates new XYZ tile layer with current parameters
- **Update Layer Button**: Updates selected layer's URL (enabled only when single XYZ layer selected)
- **Status Label**: Shows current selection state or error messages
- Use ArcGIS Pro standard styling/theming

### 3. ViewModel (`SpectralIndexDockpaneViewModel.cs`)
- Implement INotifyPropertyChanged for data binding
- Properties:
  - `SelectedIndex` (string)
  - `CloudTolerance` (int, 0-100)
  - `StartDate` (DateTime)
  - `EndDate` (DateTime)
  - `IsLayerSelected` (bool)
  - `CanUpdateLayer` (bool)
  - `StatusMessage` (string)
- Commands:
  - `AddLayerCommand`
  - `UpdateLayerCommand`
- Methods to validate date ranges and parameters

### 4. Layer Management (`LayerManager.cs`)
- **Detect XYZ Layers**: Identify layers created by this add-in (using custom metadata or naming convention)
- **Get Selected XYZ Layer**: Return single selected XYZ layer, null if none or multiple
- **Create Layer**: Add new XYZ tile layer to active map using ArcGIS Pro API
  - URL format: `{serviceUrl}/tiles/{index}/{startDate}/{endDate}/{cloud}/{z}/{x}/{y}.png`
  - Store layer metadata (index, dates, cloud) for later updates
- **Update Layer URL**: Modify existing layer's data source URL
- **Listen to Selection Events**: Subscribe to map view layer selection changes

### 5. DockPane Implementation (`SpectralIndexDockpane.cs`)
- Inherit from `DockPane`
- Initialize ViewModel and LayerManager
- Subscribe to map events (layer selection, map activation)
- Handle layer selection changes to enable/disable controls
- Update ViewModel based on selected layer's current parameters

### 6. Add-In Manifest (`Config.daml`)
- Define dockable pane
- Register buttons/commands
- Set add-in metadata (name, description, version)
- Define minimum ArcGIS Pro version (3.0+)

### 7. Layer URL Update Strategy
- When parameters change and layer is selected:
  - Validate parameters (dates, cloud tolerance)
  - Construct new URL template
  - Update layer's connection properties
  - Refresh layer to load new tiles
- Store layer metadata in custom properties or tags for identification

## Key Features

1. **Non-Blocking DockPane**: Remains open, doesn't block map interaction
2. **Smart Control Activation**: Controls enabled only when single XYZ layer selected
3. **Multiple Layer Support**: Can create multiple layers with different parameters
4. **Parameter Validation**: Validate date ranges, cloud tolerance values
5. **Status Feedback**: Show current selection state and operation results
6. **Configuration Management**: Service URL in config file, not user-configurable

## Technical Considerations

- **Layer Identification**: Use custom layer properties or naming convention to identify add-in created layers
- **URL Template Format**: Follow ArcGIS Pro XYZ tile layer URL format: `{z}/{x}/{y}` placeholders
- **Event Handling**: Subscribe to `MapView.LayerSelectionChanged` event
- **Thread Safety**: Ensure UI updates happen on UI thread
- **Error Handling**: Handle network errors, invalid parameters, layer update failures

## Development Workflow

1. Create Visual Studio solution with ArcGIS Pro Add-in template
2. Implement configuration management
3. Create dockable pane UI (XAML)
4. Implement ViewModel with data binding
5. Implement layer management logic
6. Wire up event handlers
7. Test with localhost service
8. Update config for production deployment

## Dependencies

- ArcGIS Pro SDK for .NET (latest version)
- .NET Framework 4.8 or .NET 6+
- Visual Studio 2019+ with ArcGIS Pro SDK templates

## Configuration File Format

```json
{
  "serviceUrl": "http://localhost:3000",
  "defaultIndex": "NDVI",
  "defaultCloudTolerance": 20,
  "defaultStartDate": "2023-01-01",
  "defaultEndDate": "2023-12-31"
}
```

