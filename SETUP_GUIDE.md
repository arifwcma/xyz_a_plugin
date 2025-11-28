# ArcGIS Pro Spectral Index Add-In - Setup Guide

## Quick Start

### 1. Prerequisites Check

- [ ] ArcGIS Pro 3.0+ installed
- [ ] Visual Studio 2019+ installed
- [ ] ArcGIS Pro SDK for .NET installed (comes with ArcGIS Pro)
- [ ] .NET Framework 4.8 installed
- [ ] XYZ Tile Service running (see `../xyz` folder)

### 2. Build the Add-In

1. Open Visual Studio
2. Open the solution file or create a new solution and add `SpectralIndexAddIn\SpectralIndexAddIn.csproj`
3. Verify project references point to ArcGIS Pro installation:
   - Default: `C:\Program Files\ArcGIS\Pro\bin\`
   - Update paths in `.csproj` if different
4. Build Solution (F6 or Build > Build Solution)
5. Check output in `bin\Release\` or `bin\Debug\`

### 3. Configure Service URL

Edit `config\service.config.json`:

```json
{
  "serviceUrl": "http://localhost:3000",
  "defaultIndex": "NDVI",
  "defaultCloudTolerance": 20,
  "defaultStartDate": "2023-01-01",
  "defaultEndDate": "2023-12-31"
}
```

**Important**: Copy `config\service.config.json` to the build output directory (`bin\Release\config\` or `bin\Debug\config\`)

### 4. Start XYZ Tile Service

```bash
cd ../xyz
npm install  # if not already done
npm start    # or npm run dev for development
```

Service should be running at `http://localhost:3000`

### 5. Install Add-In in ArcGIS Pro

1. Open ArcGIS Pro
2. Go to **Project** tab > **Add-In Manager** (or **Options** > **Add-In Manager**)
3. Click **Options** > **Add Folder**
4. Navigate to: `xyz_a_plugin\bin\Release\` (or `bin\Debug\`)
5. Click **OK**
6. The add-in should appear in the list
7. Ensure it's enabled (checkbox checked)

### 6. Access the DockPane

**Option 1: Via Ribbon Button**
- Look for "Spectral Index Manager" button in the ribbon
- Click to open the dock pane

**Option 2: Via View Menu**
- Go to **View** > **Dock Panes**
- Select **Spectral Index Manager**

**Option 3: Via Python**
```python
import arcpy
arcpy.AddMessage("Opening Spectral Index Manager...")
# The button should be available in the UI
```

### 7. Verify Installation

1. Open a map in ArcGIS Pro
2. Open the Spectral Index Manager dock pane
3. Try adding a layer:
   - Select an index (e.g., NDVI)
   - Set cloud tolerance (e.g., 20)
   - Select dates
   - Click "Add Layer"
4. Verify the layer appears in the map

## Troubleshooting

### Build Errors

**Error: Cannot find ArcGIS.Desktop.Framework.dll**
- Solution: Update reference paths in `SpectralIndexAddIn.csproj` to match your ArcGIS Pro installation path
- Check: `C:\Program Files\ArcGIS\Pro\bin\ArcGIS.Desktop.Framework.dll` exists

**Error: Target framework not found**
- Solution: Install .NET Framework 4.8 Developer Pack
- Or change target framework in `.csproj` to match installed version

**Error: XAML compilation errors**
- Solution: Ensure WPF is enabled in project properties
- Check that `UseWPF` is set to `true` in `.csproj`

### Runtime Errors

**Add-In doesn't appear in ArcGIS Pro**
- Check: ArcGIS Pro version is 3.0+
- Check: Add-in folder is correctly added in Add-In Manager
- Check: `Config.daml` is in output directory
- Check: ArcGIS Pro log files: `%APPDATA%\ESRI\ArcGISPro\Logs\`

**DockPane doesn't open**
- Check: Button appears in ribbon
- Check: Map view is active
- Check: No errors in ArcGIS Pro log files

**Layers don't load**
- Check: XYZ tile service is running (`http://localhost:3000/health`)
- Check: `config\service.config.json` exists in output directory
- Check: Service URL in config is correct
- Check: Network/firewall allows connection

**Controls don't activate**
- Check: Only one layer is selected
- Check: Selected layer was created by this add-in
- Check: Layer has custom properties set

### Common Issues

**Config file not found**
- Ensure `config\service.config.json` is copied to output directory
- Check file path in `ServiceConfig.cs` matches your structure
- Verify file exists: `bin\Release\config\service.config.json`

**Layer update doesn't work**
- ArcGIS Pro may require layer recreation (current implementation)
- Check layer properties to verify metadata is stored
- Try removing and re-adding the layer

## Development Tips

1. **Debugging**: Attach Visual Studio debugger to ArcGIS Pro process
2. **Logging**: Check ArcGIS Pro log files for detailed error messages
3. **Testing**: Test with different map types (2D maps, scenes)
4. **Performance**: Monitor layer loading times and memory usage

## Next Steps

- Create add-in icons (16x16 and 32x32 PNG files)
- Customize UI styling to match your organization's theme
- Add additional features as needed
- Update service URL for production deployment

## Support

For ArcGIS Pro SDK issues:
- [ArcGIS Pro SDK Documentation](https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference/)
- [Esri Community Forums](https://community.esri.com/t5/arcgis-pro-sdk/bd-p/arcgis-pro-sdk)

For tile service issues:
- See `../xyz/README.md`

