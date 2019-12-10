using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace RepositoryBuildService
{
    public class AutomaticRepositoryBuildService : IService, IDisposable
    {
        private const int MINUTES_UNTIL_AUTO_BUILD = 5;
        private const string WATCHER_DIRECTORY = "";
        private const string BUILD_DIRECTORY = "";

        public List<string> FilesChanged { get; } = new List<string>();
        public Timer Timer { get; private set; }
        public FileSystemWatcher Watcher { get; private set; }

        #region IService Implementation
        public void Start()
        {
            Timer = new Timer(TimeSpan.FromMinutes(MINUTES_UNTIL_AUTO_BUILD).TotalMilliseconds);
            Timer.Elapsed += OnTimerElapsed;

            Watcher = CreateWatcher(WATCHER_DIRECTORY);
        }

        public void Stop()
        {
            Timer.Stop();
            FilesChanged.Clear();

            if (Watcher is null) return;
            Watcher.Changed -= OnDirectoryChanged;
            Watcher.Dispose();
            Watcher = null;
        }

        #endregion

        #region Automatic Build
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var sources = FilesChanged.Distinct().ToArray();
            var targets = sources.Select(Path.GetFileName).Select(name => Path.Combine(BUILD_DIRECTORY, name)).ToArray();

            for (var i = 0; i < sources.Length; i++)
                TryMoveFile(sources[i], targets[i]);

            BuildRepository();
        }

        private static void TryMoveFile(string source, string target)
        {
            try
            {
                File.Move(source, target);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BuildRepository()
        {
            //TODO
        }

        #endregion

        #region FileWatcher

        private FileSystemWatcher CreateWatcher(string path)
        {
            Watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*",
                EnableRaisingEvents = true,
            };
            Watcher.Changed += OnDirectoryChanged;
            return Watcher;
        }

        private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            RecordFileChanged(e.FullPath);
            ResetTimer();
        }

        private void RecordFileChanged(string file) => FilesChanged.Add(file);

        private void ResetTimer()
        {
            Timer.Stop();
            Timer.Start();
        }

        #endregion

        #region Dispose

        private void Dispose(bool disposing)
        {
            Stop();
            if (!disposing) return;

            Timer?.Dispose();
            Watcher?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AutomaticRepositoryBuildService() => Dispose(false);

        #endregion
    }
}