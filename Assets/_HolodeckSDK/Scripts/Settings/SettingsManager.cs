//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System;
using System.IO;
using UnityEngine;

namespace Vulcan
{
    /// <summary>
    /// The Settings class is a wrapper to contain the various settings objects used by various components
    /// in the application. If more components are added to the app that need to serialize settings, create
    /// a new Settings class that has the SerializableAttribute and add a field to this class.
    /// </summary>
    [Serializable]
    public class Settings : IHolodeckSettings, IDisplaySettings
    {
        public HolodeckSettings HolodeckSettings;
        public DisplaySettings DisplaySettings;

        public Settings()
        {
            HolodeckSettings = new HolodeckSettings();
            DisplaySettings = new DisplaySettings();
        }

        HolodeckSettings IHolodeckSettings.HolodeckSettings
        {
            get { return HolodeckSettings; }
        }

        DisplaySettings IDisplaySettings.DisplaySettings
        {
            get { return DisplaySettings; }
        }
    }

    /// <summary>
    /// Event argument class for SettingsUpdated event.
    /// </summary>
    public class SettingsUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Settings field.
        /// </summary>
        public Settings Settings;

        /// <summary>
        /// Constructor with argument.
        /// </summary>
        /// <param name="settings">Current settings values.</param>
        public SettingsUpdatedEventArgs(Settings settings)
        {
            Settings = settings;
        }
    }

    /// <summary>
    /// The SettingsManager manages the serialization and deserialization of application settings.
    /// Using the Settings property, add any object that has the SerializableAttribute to be 
    /// serialized/deserialized to the application settings file "settings.json" in the StreamingAssets 
    /// folder.
    /// </summary>
    public class SettingsManager
    {
        #region Private Fields

        // Singleton static instance and lock object.
        private static SettingsManager _instance = null;
        private static object _lock = new object();

        // File path to the settiings file.
        protected string _filePath;

        // Settings to store settings data.
        protected Settings _settings;

        #endregion

        #region Constructors

        /// <summary>
        /// Static class property to return the singleton instance of this class.
        /// </summary>
        /// <returns></returns>
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new SettingsManager();
                    }
                }
                return _instance;
            }
        }

        public static bool IsInitialized()
        {
            return _instance != null;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal SettingsManager()
        {
            _filePath = Application.dataPath + "/StreamingAssets/HolodeckSettings.json";
            _settings = new Settings();
        }

        #endregion

        #region Pubilc Properties

        /// <summary>
        /// The Sections property is a Settings instance that stores a list of settings.
        /// Add any object that has the SerializableAttribute to be serialized/deserialized.
        /// </summary>
        public Settings settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// SettingsUpdated is an event triggered when settings are updated(refreshed) from file.
        /// </summary>
        public event EventHandler<SettingsUpdatedEventArgs> SettingsUpdated;

        #endregion

        #region Public Methods

        /// <summary>
        /// Method to save settings to the settings file.
        /// </summary>
        /// <returns></returns>
        public bool SaveToFile()
        {
            var success = false;
            try
            {   // Open the text file using a stream reader.
                using (StreamWriter sw = new StreamWriter(_filePath, false))
                {
                    String output = JsonUtility.ToJson(_settings, true);
                    sw.Write(output);
                    success = true;

                    Debug.LogFormat("[SettingsManager] Writing settings to file: {0}", _filePath);
                }
            }
            catch (Exception)
            {
                Debug.LogFormat("[SettingsManager] Cannot write Settings to: {0}", _filePath);
            }

            return success;
        }

        /// <summary>
        /// Method to save settings to the settings file.
        /// </summary>
        /// <returns></returns>
        public bool ReadFromFile()
        {
            var success = false;
            
            // Ensure there is a version of the settings object on disk
            if (!File.Exists(_filePath))
            {
                _settings = new Settings();
                SaveToFile();

                Debug.LogFormat("[SettingsManager] Settings file does not exist, creating new file at {0}", _filePath);
            }

            try
            {   // Open the text file using a stream reader.
                string input = File.ReadAllText(_filePath);

                // Read the stream to a string, and write the string to the console.
                _settings = JsonUtility.FromJson<Settings>(input);
                success = true;

                Debug.LogFormat("[SettingsManager] Reading settings file: {0}", _filePath);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[SettingsManager] Settings file {0} could not be read.  Error: {1}", _filePath, e);
            }

            return success;
        }

        /// <summary>
        /// Method to re-read the settings file and fire off event to subscribers that 
        /// settings have been refreshed.
        /// </summary>
        public void Refresh()
        {
            // Refresh settings from settings file.
            ReadFromFile();
            Debug.Log("[SettingsManager] Refreshing");

            // Trigger SettingsUpdated event.
            OnSettingsUpdated(new SettingsUpdatedEventArgs(_settings));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// EventHandler method to handle when settings are updated/refreshed.
        /// </summary>
        /// <param name="e">SettingsUpdatedEventArgs that contain the current Settings.</param>
        protected virtual void OnSettingsUpdated(SettingsUpdatedEventArgs e)
        {
            EventHandler<SettingsUpdatedEventArgs> handler = SettingsUpdated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
