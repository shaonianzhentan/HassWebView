using System;
using System.Threading.Tasks;
using HassWebView.Core.Auth;

namespace HassWebView.Core.Configuration
{
    /// <summary>
    /// Provides configuration options for HassPage-specific behaviors.
    /// This object is registered as a singleton and allows other parts of the application
    /// to trigger actions that are handled by the active HassPage instance.
    /// </summary>
    public class HassPageOptions
    {
        /// <summary>
        /// Gets or sets the persistence mechanism for authentication data.
        /// Defaults to an implementation using MAUI's Preferences API.
        /// Assign a custom implementation of IAuthStore to change the storage behavior.
        /// </summary>
        public IAuthStore AuthStore { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed to play a video.
        /// The HassPage will assign its video playback logic to this action upon initialization.
        /// The Func takes a video URL string and returns a Task.
        /// </summary>
        public Func<string, Task> PlayVideo { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed to show a custom settings screen.
        /// This action is provided by the application and invoked by the HassPage.
        /// </summary>
        public Action ShowSettingsScreen { get; set; }

        /// <summary>
        /// Gets or sets the URL for receiving push notifications.
        /// This URL is used by the mobile app registration process.
        /// </summary>
        public string PushUrl { get; set; }

        public HassPageOptions()
        {
            AuthStore = new PreferencesAuthStore();
        }
    }
}
