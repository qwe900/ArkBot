﻿using ArkBot.Services;
using ArkBot.Services.Data;
using ArkBot.Threading;
using ArkSavegameToolkitNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Ark
{
    public delegate void InitializationCompletedEventHandler();
    //public delegate void UpdateTriggeredEventHandler(ArkServerContext sender);
    public delegate void UpdateCompletedEventHandler(ArkServerContext sender, bool successful, bool cancelled);
    public delegate void BackupCompletedEventHandler(ArkServerContext sender, bool backupsEnabled, SavegameBackupResult result);
    public delegate void VoteInitiatedEventHandler(ArkServerContext sender, VoteInitiatedEventArgs e);
    public delegate void VoteResultForcedEventHandler(ArkServerContext sender, VoteResultForcedEventArgs e);

    public class ArkContextManager : IDisposable
    {
        private Dictionary<string, ArkServerContext> _serverContexts = new Dictionary<string, ArkServerContext>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, ArkClusterContext> _clusterContexts = new Dictionary<string, ArkClusterContext>(StringComparer.OrdinalIgnoreCase);

        public bool IsFullyInitialized { get; set; }
        public ArkServerContext[] Servers => _serverContexts.Values.ToArray();
        public ArkClusterContext[] Clusters => _clusterContexts.Values.ToArray();

        public event InitializationCompletedEventHandler InitializationCompleted;
        //public event UpdateTriggeredEventHandler UpdateTriggered;
        public event UpdateCompletedEventHandler UpdateCompleted;
        public event BackupCompletedEventHandler BackupCompleted;
        public event VoteInitiatedEventHandler VoteInitiated;
        public event VoteResultForcedEventHandler VoteResultForced;

        private BlockingCollection<Tuple<ArkServerContext, bool>> _updateQueue;
        private CancellationTokenSource _currentCts;
        private ArkServerContext _currentContext;

        private CancellationTokenSource _cts;
        private Task _proc;

        private IConfig _config;
        private IProgress<string> _progress;
        private ISavegameBackupService _savegameBackupService;

        public ArkContextManager(IConfig config, IProgress<string> progress, ISavegameBackupService savegameBackupService)
        {
            _config = config;
            _progress = progress;
            _savegameBackupService = savegameBackupService;

            _updateQueue = new BlockingCollection<Tuple<ArkServerContext, bool>>();
            _cts = new CancellationTokenSource();
            _proc = Task.Run(() => _updateManagerRun(_cts.Token));
        }

        private void _updateManagerRun(CancellationToken ct)
        {
            try
            {
                while (!_updateQueue.IsCompleted)
                {
                    Tuple<ArkServerContext, bool> queueItem = null;
                    try
                    {
                        queueItem = _updateQueue.Take();
                    }
                    catch (InvalidOperationException) { }

                    if (queueItem?.Item1 != null)
                    {
                        _currentCts = new CancellationTokenSource();
                        _currentContext = queueItem.Item1;
                        queueItem.Item1._serverUpdate(queueItem.Item2, _config, _savegameBackupService, _progress, _currentCts.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void _saveFileWatcher_Changed(ArkServerContext serverContext, ArkSaveFileChangedEventArgs e)
        {
            QueueServerUpdate(serverContext);
        }

        public void QueueUpdateServerManual(ArkServerContext serverContext)
        {
            
            if (_updateQueue.Any(x => x.Item1 == serverContext))
            {
                return;
            }

            if (_currentContext == serverContext)
            {
                _currentCts?.Cancel();
            }

            _progress.Report($"Server ({serverContext.Config.Key}): Update queued manually ({DateTime.Now:HH:mm:ss.ffff})");
            _updateQueue.Add(Tuple.Create(serverContext, true));
        }

        public void QueueServerUpdate(ArkServerContext serverContext)
        {
            if (_updateQueue.Any(x => x.Item1 == serverContext))
            {
                return;
            }

            if (_currentContext == serverContext)
            {
                _currentCts?.Cancel();
            }

            _progress.Report($"Server ({serverContext.Config.Key}): Update queued by watcher ({DateTime.Now:HH:mm:ss.ffff})");
            _updateQueue.Add(Tuple.Create(serverContext, false));
        }

        public void AddServer(ArkServerContext context)
        {
            //context.UpdateQueued += Context_UpdateTriggered;
            context.UpdateCompleted += Context_UpdateCompleted;
            context.BackupCompleted += Context_BackupCompleted;
            context.VoteInitiated += Context_VoteInitiated;
            context.VoteResultForced += Context_VoteResultForced;
            context._saveFileWatcher.Changed += _saveFileWatcher_Changed;
            context._contextManager = this;
            _serverContexts.Add(context.Config.Key, context);
        }

        private void Context_VoteResultForced(ArkServerContext sender, VoteResultForcedEventArgs e)
        {
            VoteResultForced?.Invoke(sender, e);
        }

        private void Context_VoteInitiated(ArkServerContext sender, VoteInitiatedEventArgs e)
        {
            VoteInitiated?.Invoke(sender, e);
        }

        private void Context_UpdateCompleted(ArkServerContext sender, bool successful, bool cancelled)
        {
            // When all server contexts have completed one update successfully trigger the InitializationCompleted-event.
            if (!IsFullyInitialized && Servers.All(x => x.IsInitialized))
            {
                IsFullyInitialized = true;
                InitializationCompleted?.Invoke();
            }

            UpdateCompleted?.Invoke(sender, successful, cancelled);
        }

        private void Context_BackupCompleted(ArkServerContext sender, bool backupsEnabled, Services.Data.SavegameBackupResult result)
        {
            BackupCompleted?.Invoke(sender, backupsEnabled, result);
        }

        //private void Context_UpdateTriggered(ArkServerContext sender)
        //{
        //    UpdateTriggered?.Invoke(sender);
        //}

        public void AddCluster(ArkClusterContext context)
        {
            _clusterContexts.Add(context.Config.Key, context);
        }

        public ArkServerContext GetServer(string key)
        {
            if (key == null) return null;

            ArkServerContext context = null;
            if (_serverContexts.TryGetValue(key, out context))
            {
                return context;
            }

            return null;
        }

        public ArkClusterContext GetCluster(string key)
        {
            ArkClusterContext context = null;
            if (_clusterContexts.TryGetValue(key, out context))
            {
                return context;
            }

            return null;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                _cts.Cancel();
                if (_serverContexts != null)
                {
                    foreach (var context in _serverContexts.Values) context?.Dispose();
                    _serverContexts.Clear();
                }
            }

            disposedValue = true;
        }
        public void Dispose() { Dispose(true); }
        private bool disposedValue = false;
        #endregion
    }
}