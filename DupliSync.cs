using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace DupliSyncTray
{
    public class DupliSync
    {
        private static DupliSync _instance;
        public static DupliSync Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DupliSync();
                return _instance;
            }
        }

        public DupliSync() 
        {
            var dirs = System.Configuration.ConfigurationManager.AppSettings["DestinationDirs"];

            SourceDirectory = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["SourceDir"]);
            DestinationDirectories = dirs.Split(';').Select(x => new DirectoryInfo(x)).ToList();

            var dlDirs = System.Configuration.ConfigurationManager.AppSettings["DownloadSourceDirectories"];
            DownloadSourceDirectories = !String.IsNullOrWhiteSpace(dlDirs) ?  dlDirs.Split(';').Select(x => new DirectoryInfo(x)).ToList() : null;
            DownloadDestinationDirectory = !String.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["DownloadDestinationDirectory"]) ? 
                new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["DownloadDestinationDirectory"]) : 
                null;

            TimerInterval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["TimerInterval"]);
        }

        public DirectoryInfo SourceDirectory { get; set; }
        public IList<DirectoryInfo> DestinationDirectories { get; set; }

        public DirectoryInfo DownloadDestinationDirectory { get; set; }
        public IList<DirectoryInfo> DownloadSourceDirectories { get; set; }

        public int TimerInterval { get; set; }

        public DupliSyncStatus Status;

        private Timer _timer;

        public void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer();
                _timer.AutoReset = true;
                _timer.Interval = TimerInterval;
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();
            }

        }

        public void Stop()
        {
            if (_timer != null && _timer.Enabled)
            {
                _timer.Stop();
            }

        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var di in DestinationDirectories)
            {
                var failed = false;

                if (!di.Exists)
                {
                    continue;
                }

                var dest = new DirectoryInfo(Path.Combine(di.FullName, Environment.MachineName));

                if (!dest.Exists)
                {
                    try
                    {
                        dest.Create();
                    } catch (Exception)
                    {
                        failed = true;
                    }
                    
                }

                var files = SourceDirectory.GetFiles();

                foreach (var f in files)
                {
                    try
                    {
                        f.CopyTo(Path.Combine(dest.FullName, f.Name), true);
                    }
                    catch (Exception)
                    {
                        failed = true;
                    }

                }
                Status = failed ? DupliSyncStatus.UploadCopyFailed : DupliSyncStatus.OK;
            }

            if (DownloadDestinationDirectory != null && DownloadSourceDirectories != null)
            {
                bool failed = false;
                foreach (var di in DownloadSourceDirectories)
                {
                    if (di.Exists)
                    {
                        if (!DownloadDestinationDirectory.Exists)
                        {
                            try
                            {
                                DownloadDestinationDirectory.Create();
                            } catch (Exception)
                            {
                                failed = true;
                            }
                            
                        }
                        foreach (var file in di.GetFiles())
                        {
                            try
                            {
                                file.CopyTo(Path.Combine(DownloadDestinationDirectory.FullName, file.Name), true);
                            }
                            catch (Exception)
                            {
                                failed = true;
                            }
                        }
                        break; // Sort après premier dossier trouvé
                    }
                }
                Status = failed ? DupliSyncStatus.DownloadCopyFailed : DupliSyncStatus.OK;
            }
            
        }

    }
}
