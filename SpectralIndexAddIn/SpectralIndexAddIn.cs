using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace SpectralIndexAddIn
{
    /// <summary>
    /// This add-in provides a dockable pane for managing XYZ tile layers
    /// from the spectral index tile service.
    /// </summary>
    public class SpectralIndexAddIn : Module
    {
        private static SpectralIndexAddIn _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static SpectralIndexAddIn Current => _this ?? (_this = (SpectralIndexAddIn)FrameworkApplication.FindModule("SpectralIndexAddIn_Module"));

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}

