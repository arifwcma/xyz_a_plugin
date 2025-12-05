using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace SpectralIndexAddIn
{
    /// <summary>
    /// DockPane implementation for Spectral Index layer management
    /// </summary>
    internal class SpectralIndexDockpane : DockPane
    {
        private const string _dockPaneID = "SpectralIndexAddIn_SpectralIndexDockpane";
        private SpectralIndexDockpaneViewModel _viewModel;

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown on the dock pane tab.
        /// </summary>
        private string _heading = "Spectral Index Manager";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        /// <summary>
        /// ViewModel for data binding
        /// </summary>
        public SpectralIndexDockpaneViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = new SpectralIndexDockpaneViewModel();
                }
                return _viewModel;
            }
        }

        /// <summary>
        /// Cleanup when dock pane is closed
        /// </summary>
        protected override void OnHidden()
        {
            if (_viewModel != null)
            {
                _viewModel.Cleanup();
            }
            base.OnHidden();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SpectralIndexDockpane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            SpectralIndexDockpane.Show();
        }
    }
}

