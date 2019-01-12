//using System;
//using System.Collections.Generic;

//namespace BatchBootstrapper
//{
//    public class ExternalConfigurationManager : IDisposable
//    {  // An abstraction of the configuration store.
//        private readonly ISettingsStore settings;
//        private readonly ISubject<KeyValuePair<string, string>> changed;
//        private Dictionary<string, string> settingsCache;
//        private string currentVersion;
//        public ExternalConfigurationManager(ISettingsStore settings)
//        { this.settings = settings; }

//        public IObservable<KeyValuePair<string, string>> Changed
//        {
//            get { return this.changed.AsObservable(); }
//        }

//        public void SetAppSetting(string key, string value)
//        {
//            // Update the setting in the store.
//            this.settings.Update(key, value);    // Publish the event.
//            this.Changed.OnNext(new KeyValuePair<string, string>(key, value));    // Refresh the settings cache.
//            this.CheckForConfigurationChanges();
//        }
//        public string GetAppSetting(string key)
//        {
//            // Try to get the value from the settings cache.
//            // // If there is a miss, get the setting from the settings store.
//            string value;
//            if (this.settingsCache.TryGetValue(key, out value)) { return value; }
//            // Check for changes and refresh the cache.
//            this.CheckForConfigurationChanges();
//            return this.settingsCache[key];

//        }
//        private void CheckForConfigurationChanges()
//        {
//            try
//            {
//                // Assume that updates are infrequent. Lock to avoid      // race conditions when refreshing the cache.
//                lock (this.settingsSyncObject)
//                {
//                    {
//                        var latestVersion = this.settings.Version;        // If the versions differ, the configuration has changed.
//                        if (this.currentVersion != latestVersion)
//                        {
//                            // Get the latest settings from the settings store and publish the changes.
//                            var latestSettings = this.settings.FindAll();
//                            latestSettings.Except(this.settingsCache).ToList().ForEach(
//                                kv => this.changed.OnNext(kv));
//                            // Update the current version.
//                            this.currentVersion = latestVersion;          // Refresh settings cache.
//                            this.settingsCache = latestSettings;
//                        }
//                    }
//                }
//            catch (Exception ex) { this.changed.OnError(ex); }
//        }
//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Timers;


//public class ExternalConfigurationManager : IDisposable {
//    private readonly ISubject<KeyValuePair<string, string>> changed;
//    private readonly Timer timer;
//    private ISettingsStore settings;
//    public ExternalConfigurationManager(ISettingsStore settings, TimeSpan interval)
//    {    
//// Set up the timer.
//this.timer = new Timer(interval.TotalMilliseconds)
//{
//    AutoReset = false
//};   
//        this.timer.Elapsed += this.OnTimerElapsed;    
//        this.changed = new Subject<KeyValuePair<string, string>>();

//    }
//    public void StartMonitor()  {
//        if (this.timer.Enabled)
//        {
//            return;
//        }
//        lock (this.timerSyncObject)
//        {
//            if (this.timer.Enabled)
//            {        return;      }
//            this.keepMonitoring = true;      // Load the local settings cache.
//            this.CheckForConfigurationChanges();
//            this.timer.Start();    }
//    }

//    public void StopMonitor()
//    {
//        lock (this.timerSyncObject)
//        {      this.keepMonitoring = false;
//            this.timer.Stop();

//        }
//    }

//    private void OnTimerElapsed(object sender, EventArgs e)
//    {
//        Trace.TraceInformation(          "Configuration Manager: checking for configuration changes.");
//        try
//        {
//            this.CheckForConfigurationChanges();
//        }    finally    {      
//// Restart the timer after each interval.
//this.timer.Start();

//        }      }
//}
