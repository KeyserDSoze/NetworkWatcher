using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Imaging;
using System;
using System.Runtime.InteropServices;

namespace NetworkWatcherExtension
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("12345678-1234-1234-1234-123456789012")]
    public class NetworkWatcherToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkWatcherToolWindow"/> class.
        /// </summary>
        public NetworkWatcherToolWindow() : base(null)
        {
            this.Caption = "Network Watcher";

            // Set icon (using built-in icon for now)
            this.BitmapImageMoniker = KnownMonikers.ConnectArrow;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new NetworkWatcherToolWindowControl();
        }
    }
}
