# ArcGIS Pro Spectral Index Add-In

An ArcGIS Pro add-in that provides a dockable pane interface for managing XYZ tile layers from the spectral index tile service.

## Features

- **Dockable Pane Interface**: Non-blocking dockable pane that remains open while working with maps
- **Spectral Index Selection**: Dropdown to select from 9 available indices (NDVI, EVI, SAVI, OSAVI, GNDVI, NDSI, ARVI, NDWI, MNDWI)
- **Cloud Tolerance Control**: Slider (0-100%) for filtering cloud cover
- **Date Range Selection**: Date pickers for start and end dates
- **Multiple Layer Support**: Create and manage multiple layers with different parameters
- **Smart Control Activation**: Controls activate only when a single XYZ layer created by this add-in is selected
- **Layer Updates**: Update existing layer parameters without recreating the layer

## Prerequisites

- ArcGIS Pro 3.0 or later
- Visual Studio 2019 or later
- ArcGIS Pro SDK for .NET (installed with ArcGIS Pro)
- .NET Framework 4.8 or .NET 6+
- XYZ Tile Service running (see `../xyz` folder)

## Project Structure

```
xyz_a_plugin/
├── config/
│   └── service.config.json          # Service URL configuration
├── SpectralIndexAddIn/
│   ├── SpectralIndexAddIn.csproj    # C# project file
│   ├── Config.daml                  # Add-in manifest
│   ├── SpectralIndexAddIn.cs        # Main add-in class
│   ├── SpectralIndexDockpaneView.xaml        # UI layout
│   ├── SpectralIndexDockpaneView.xaml.cs    # Code-behind
│   ├── SpectralIndexDockpaneViewModel.cs     # ViewModel (MVVM)
│   ├── SpectralIndexDockpane.cs             # DockPane implementation
│   ├── LayerManager.cs              # Layer detection and URL updates
│   ├── ServiceConfig.cs             # Configuration management
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   └── Images/                       # Add-in icons (placeholder)
├── IMPLEMENTATION_PLAN.md
└── README.md
```

## Configuration

Edit `config/service.config.json` to configure the tile service URL:

```json
{
  "serviceUrl": "http://localhost:3000",
  "defaultIndex": "NDVI",
  "defaultCloudTolerance": 20,
  "defaultStartDate": "2023-01-01",
  "defaultEndDate": "2023-12-31"
}
```

For production deployment, update `serviceUrl` to point to your remote server.

## Building the Add-In

1. **Open Visual Studio** and open the solution file (or create a new solution and add the project)

2. **Verify ArcGIS Pro SDK References**:
   - Ensure ArcGIS Pro SDK is installed
   - Update the reference paths in `SpectralIndexAddIn.csproj` if ArcGIS Pro is installed in a non-standard location
   - Default path: `C:\Program Files\ArcGIS\Pro\bin\`

3. **Build the Project**:
   - Set build configuration to `Release` or `Debug`
   - Build the solution (F6 or Build > Build Solution)
   - Output will be in `bin\Release\` or `bin\Debug\`

4. **Create Add-In Package**:
   - The add-in files will be in the output directory
   - Copy `config\service.config.json` to the output directory
   - Create an `.esriaddin` package using ArcGIS Pro SDK tools or manually zip the files

## Installation

1. **Start XYZ Tile Service** (if using localhost):
   ```bash
   cd ../xyz
   npm start
   ```

2. **Install Add-In in ArcGIS Pro**:
   - Open ArcGIS Pro
   - Go to **Project** > **Add-In Manager**
   - Click **Options** > **Add Folder**
   - Navigate to the add-in output directory
   - The add-in should appear in the list
   - Enable it if needed

3. **Access the DockPane**:
   - The add-in adds a button to the ribbon
   - Click the button or go to **View** > **Dock Panes** > **Spectral Index Manager**
   - The dock pane will appear

## Usage

### Adding a New Layer

1. Open the Spectral Index Manager dock pane
2. Select a spectral index from the dropdown
3. Adjust cloud tolerance using the slider (0-100%)
4. Select start and end dates
5. Click **Add Layer**
6. The layer will be added to the active map

### Updating an Existing Layer

1. Select a single XYZ layer created by this add-in in the Contents pane
2. The dock pane controls will activate and show the layer's current parameters
3. Modify the parameters as needed
4. Click **Update Selected Layer**
5. The layer will be updated with new parameters

### Multiple Layers

- You can create multiple layers with different parameters
- Each layer is independent
- Controls activate only when a single layer is selected
- If multiple layers or no layer is selected, controls are disabled

## Supported Spectral Indices

- **NDVI**: Normalized Difference Vegetation Index
- **EVI**: Enhanced Vegetation Index
- **SAVI**: Soil-Adjusted Vegetation Index
- **OSAVI**: Optimized Soil-Adjusted Vegetation Index
- **GNDVI**: Green Normalized Difference Vegetation Index
- **NDSI**: Normalized Difference Snow Index
- **ARVI**: Atmospherically Resistant Vegetation Index
- **NDWI**: Normalized Difference Water Index
- **MNDWI**: Modified Normalized Difference Water Index

## Troubleshooting

### Add-In Not Appearing

- Check that ArcGIS Pro SDK is properly installed
- Verify all DLL references are correct in the project file
- Check ArcGIS Pro version compatibility (3.0+)
- Review ArcGIS Pro log files: `%APPDATA%\ESRI\ArcGISPro\Logs`

### Layers Not Loading

- Verify the XYZ tile service is running and accessible
- Check `config/service.config.json` has the correct service URL
- Test the service URL in a browser: `http://localhost:3000/health`
- Check network connectivity and firewall settings

### Controls Not Activating

- Ensure only one layer is selected
- Verify the selected layer was created by this add-in
- Check layer properties to confirm it has the add-in metadata

### Build Errors

- Verify ArcGIS Pro SDK installation path matches references in `.csproj`
- Ensure .NET Framework 4.8 or compatible version is installed
- Check that all NuGet packages (if any) are restored

## Development Notes

- The add-in uses MVVM pattern for UI/data separation
- Layer identification uses custom properties stored on layers
- URL updates recreate the layer (ArcGIS Pro limitation)
- All map operations must run on the MCT (Main C# Thread) using `QueuedTask.Run`

## API Reference

- [ArcGIS Pro SDK Documentation](https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference/)
- [DockPane Development](https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference/#topic173.html)
- [Layer Management](https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference/#topic173.html)

## License

[Specify your license here]

## Author

[Your Name/Organization]

