// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Modules;
using UnityEngine;

namespace SharingService.Providers
{
    /// <summary>
    /// Internal class used for implementation of a sharing protocol.
    /// This provider can be used to mock sharing apis when in an offline mode.
    /// </summary>
    public class OfflineProvider : BaseServiceModule, ISharingServiceModule
    {
        private LogHelper<OfflineProvider> _logger = new LogHelper<OfflineProvider>();
        private bool _isDisposed = false;
        private CancellationTokenSource _providerCancellation = new CancellationTokenSource();
        private ConcurrentQueue<Action<CancellationToken>> _queuedWork = new ConcurrentQueue<Action<CancellationToken>>();
        private string _statusMessage = null;
        private SharingServiceProfile _settings = null;
        private List<ISharingServiceRoom> _rooms = new List<ISharingServiceRoom>();
        private object _roomLock = new object();
        private OfflineSpawner _spawner;
        private Dictionary<string, object> _roomProperties = new Dictionary<string, object>();
        private Dictionary<string, object> _playerProperties = new Dictionary<string, object>();


        #region Constructors
        public OfflineProvider(string name, uint priority, SharingServiceProfile profile,
            ISharingService parentService, ISharingServiceProtocol protocol, GameObject root) : base(name, priority, profile, parentService)
        {
            _settings = profile ?? throw new ArgumentNullException("Settings can't be null");
            _logger.Verbose = profile.VerboseLogging ? LogHelperState.Always : LogHelperState.Default;

            SharingRoot = root;
            _spawner = new OfflineSpawner(parentService);
        }
        #endregion Constructors

        #region IDispose
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_providerCancellation != null &&
                !_providerCancellation.IsCancellationRequested)
            {
                _providerCancellation.Cancel();
            }

            while (!_queuedWork.IsEmpty)
            {
                _queuedWork.TryDequeue(out var work);
            }

            DisposeSessionComponents();

            if (_providerCancellation != null)
            {
                _providerCancellation.Dispose();
                _providerCancellation = null;
            }
        }
        #endregion IDispose
        #region ISharingProvider Events
#pragma warning disable 0067
        /// <summary>
        /// Event fired when client is connected to a session/room
        /// </summary>
        public event Action<ISharingServiceModule> Connected;

        /// <summary>
        /// Event raised when a new client is connecting to a session/room
        /// </summary>
        public event Action<ISharingServiceModule> Connecting;

        /// <summary>
        /// Event fired when client is disconnected from a session/room
        /// </summary>
        public event Action<ISharingServiceModule> Disconnected;

        /// <summary>
        /// Event fired when a new message is received.
        /// </summary>
        public event Action<ISharingServiceModule, ISharingServiceMessage> MessageReceived;

        /// <summary>
        /// A specialized message optimized for sending a transform to a target.
        /// </summary>
        public event Action<ISharingServiceModule, string, SharingServiceTransform> TransformMessageReceived;

        /// <summary>
        /// Event fired when a property changes.
        /// </summary>
        public event Action<ISharingServiceModule, string, object> PropertyChanged;

        /// <summary>
        /// Event fired when the current room has changed.
        /// </summary>
        public event Action<ISharingServiceModule, ISharingServiceRoom> CurrentRoomChanged;

        /// <summary>
        /// Event fired when the rooms have changed.
        /// </summary>
        public event Action<ISharingServiceModule, IReadOnlyCollection<ISharingServiceRoom>> RoomsChanged;

        /// <summary>
        /// Event fired when an invitation is received.
        /// </summary>
        public event Action<ISharingServiceModule, ISharingServiceRoom> RoomInviteReceived;

        /// <summary>
        /// Event fired when a new player has been added.
        /// </summary>
        public event Action<ISharingServiceModule, SharingServicePlayerData> PlayerAdded;

        /// <summary>
        /// Event fired when a player has been removed.
        /// </summary>
        public event Action<ISharingServiceModule, SharingServicePlayerData> PlayerRemoved;

        /// <summary>
        /// Event fired when a player's property changes.
        /// </summary>
        public event Action<ISharingServiceModule, string, string, object> PlayerPropertyChanged;

        /// <summary>
        /// Event fired when a player's name changes.
        /// </summary>
        public event Action<ISharingServiceModule, string, string> PlayerDisplayNameChanged;

        /// <summary>
        /// Event raised when the provider's status message changes.
        /// </summary>
        public event Action<ISharingServiceModule, string> StatusMessageChanged;

        /// <summary>
        /// A specialized message when response from a ping request
        /// </summary>
        public event Action<ISharingServiceModule, string, SharingServicePingResponse> PingReturned;
        
#pragma warning restore 0067
        #endregion ISharingProvider Events

        #region ISharingProvider Properties
        /// <summary>
        /// Get the container for all sharing related game objects, such as new avatars. Avatar positioning will be relative to this container.
        /// </summary>
        public GameObject SharingRoot { get; private set; }

        /// <summary>
        /// True if connected to sharing service and logged in. But not necessarily in a session/room
        /// </summary>
        public bool IsLoggedIn => false;

        /// <summary>
        /// True when client is connected to a session/room
        /// </summary>
        public bool IsConnected => false;

        /// <summary>
        /// Get if the provider is connecting to a session/room
        /// </summary>
        public bool IsConnecting => false;

        /// <summary>
        /// The player id of the local player.
        /// </summary>
        public string LocalPlayerId => "1";

        /// <summary>
        /// Get an invalid player id
        /// </summary>
        public string InvalidPlayerId => null;

        /// <summary>
        /// The list of current room.
        /// </summary>
        public IReadOnlyCollection<ISharingServiceRoom> Rooms => _rooms;

        /// <summary>
        /// Get the current room.
        /// </summary>
        public ISharingServiceRoom CurrentRoom => null;

        /// <summary>
        /// Get if this provider supports private sharing sessions/rooms.
        /// </summary>
        public bool HasPrivateRooms => false;

        /// <summary>
        /// Get the provider's status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;

            private set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    StatusMessageChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Get if offline spawning of objects is supported by this provider.
        /// </summary>
        public bool OfflineSpawningSupported => true;

        /// <summary>
        /// Get if the data parameters used doing spawning should be wrapped into ProtocolMessages.
        /// </summary>
        public bool WrapSpawningData => false;
        #endregion ISharingProvider Properties

        #region ISharingProvider Functions
        /// <summary>
        /// Connect and join the sharing service's lobby. This allows the client to see the available sharing rooms.
        /// </summary>
        public Task Login() => Task.CompletedTask;

        /// <summary>
        /// Disconnected from sharing service. This leave the lobby and the client can no longer see the available sharing rooms.
        /// </summary>
        public void Logout() { }

        /// <summary>
        /// Update provider every frame.
        /// </summary>
        public override void Update()
        {
            while (!_queuedWork.IsEmpty && !_isDisposed)
            {
                if (_queuedWork.TryDequeue(out var work))
                {
                    if (!_providerCancellation.IsCancellationRequested)
                    {
                        work.Invoke(_providerCancellation.Token);
                    }
                }
            }
        }

        /// <summary>
        /// LateUpdate provider every frame.
        /// </summary>
        public override void LateUpdate()
        {
        }

        /// <summary>
        /// Create and join a new sharing room.
        /// </summary>
        public Task CreateAndJoinRoom() => JoinFakeRoom();

        /// <summary>
        /// Join the given room.
        /// </summary>
        public Task JoinRoom(ISharingServiceRoom room) => JoinFakeRoom();

        /// <summary>
        /// Join the given room by string id
        /// </summary>
        public Task JoinRoom(string id) => JoinFakeRoom();

        /// <summary>
        /// Create and join a new private sharing room. Only the given list of players can join the room.
        /// </summary>
        public Task CreateAndJoinRoom(IEnumerable<SharingServicePlayerData> inviteList) => JoinFakeRoom();

        /// <summary>
        /// Decline a room/session invitation.
        /// </summary>
        public void DeclineRoom(ISharingServiceRoom room) { }

        /// <summary>
        /// If the current user is part of a private room, invite the given player to this room.
        /// </summary>
        public Task<bool> InviteToRoom(SharingServicePlayerData player) => Task.FromResult(false);

        /// <summary>
        /// Leave the currently joined sharing room, and join the default lobby.
        /// </summary>
        public Task LeaveRoom() => LeaveFakeRoom();

        /// <summary>
        /// Force the list of rooms to update now.
        /// </summary>
        public Task<IReadOnlyCollection<ISharingServiceRoom>> UpdateRooms() => Task.FromResult(Rooms);

        /// <summary>
        /// Send a specialized message that contains only a transform. 
        /// </summary>
        public void SendTransformMessage(string gameObject, SharingServiceTransform transform) { }

        /// <summary>
        /// Sends a message to all other clients
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(ISharingServiceMessage message) { }

        /// <summary>
        /// Set a shared property on the server. Setting to null will clear the property from the server.
        /// </summary>
        public void SetProperty(string name, object value)
        {
            lock (_roomLock)
            {
                if (value == null)
                {
                    _roomProperties.Remove(name);
                }
                else
                {
                    _roomProperties[name] = value;
                }
            }

            EnqueueAction(() => PropertyChanged?.Invoke(this, name, value));
        }

        /// <summary>
        /// Set a shared properties on the server. Setting to a value to null will clear the property from the server.
        /// </summary>
        public void SetProperties(params object[] propertyNamesAndValues)
        {
            int length = propertyNamesAndValues?.Length ?? 0;
            for (int i = 0; i < length - 1; i++)
            {
                var name = propertyNamesAndValues[i++] as string;
                var value = propertyNamesAndValues[i];

                if (name == null)
                {
                    continue;
                }

                SetProperty(name, value);
            }
        }

        /// <summary>
        /// Try to get a sharing service's property value.
        /// </summary>
        /// <returns>True if a non-null property value was found.</returns>
        public bool TryGetProperty(string name, out object value)
        {
            bool result = false;

            lock (_roomLock)
            {
                result = _roomProperties.TryGetValue(name, out value);
            }

            return result;
        }

        /// <summary>
        /// Does the sharing service have the current property name.
        /// </summary>
        public bool HasProperty(string name)
        {
            bool result = false;

            lock (_roomLock)
            {
                result = _roomProperties.ContainsKey(name);
            }

            return result;
        }

        /// <summary>
        /// Clear all properties with the given prefix.
        /// </summary>
        public void ClearPropertiesStartingWith(string prefix)
        {
            HashSet<string> toClear = new HashSet<string>();

            lock (_roomLock)
            {
                foreach (var entry in _roomProperties)
                {
                    if (entry.Key.StartsWith(prefix))
                    {
                        toClear.Add(entry.Key);
                    }
                }
            }

            foreach (var clear in toClear)
            {
                SetProperty(clear, null);
            }
        }

        /// <summary>
        /// Try to set a player's property value.
        /// </summary>
        public void SetPlayerProperty(string playerId, string name, object value)
        {
            if (playerId != LocalPlayerId)
            {
                LogError("Invalid player id is being used ({0})", playerId);
            }

            lock (_roomLock)
            {
                if (value == null)
                {
                    _playerProperties.Remove(name);
                }
                else
                {
                    _playerProperties[name] = value;
                }
            }

            EnqueueAction(() => PlayerPropertyChanged?.Invoke(this, LocalPlayerId, name, value));
        }

        /// <summary>
        /// Try to get a player's property value.
        /// </summary>
        /// <returns>True if a non-null property value was found.</returns>
        public bool TryGetPlayerProperty(string playerId, string name, out object value)
        {
            if (playerId != LocalPlayerId)
            {
                LogError("Invalid player id is being used ({0})", playerId);
            }

            bool result = false;

            lock (_roomLock)
            {
                result = _playerProperties.TryGetValue(name, out value);
            }

            return result;
        }

        // <summary>
        /// Does this provider have the current property for the player.
        /// </summary>
        public bool HasPlayerProperty(string playerId, string name)
        {
            if (playerId != LocalPlayerId)
            {
                LogError("Invalid player id is being used ({0})", playerId);
            }

            bool result = false;

            lock (_roomLock)
            {
                result = _playerProperties.ContainsKey(name);
            }

            return result;
        }

        /// <summary>
        /// Send a ping request.
        /// </summary>
        /// <remarks>
        /// If targetRecipientId is not null, will send it to that person only
        /// </remarks>
        public SharingServicePingRequest? SendPing(byte id, string targetRecipientId = null) => null;

        /// <summary>
        /// Return a ping request.
        /// </summary>
        public void ReturnPing(SharingServicePingRequest pingRequest, string targetPlayerId) { }

        /// <summary>
        /// Find sharing service players by a name. These player might not be in the current session.
        /// </summary>
        public Task<IList<SharingServicePlayerData>> FindPlayers(string prefix, CancellationToken ct = default)
        {
           // Offline provider does not support this
            return Task.FromResult<IList<SharingServicePlayerData>>(new List<SharingServicePlayerData>());
        }

        /// <summary>
        /// Spawn a network object that is only available on this client.
        /// </summary>
        public Task<GameObject> SpawnTarget(GameObject original, object[] data)
        {
            return _spawner.SpawnTarget(original, data);
        }

        /// <summary>
        /// Despawn a network object that is shared across all clients
        /// </summary>
        public Task DespawnTarget(GameObject gameObject)
        {
            return _spawner.DespawnTarget(gameObject);
        }

        /// <summary>
        /// Initialize a network object with sharing components needed for the selected provider
        /// </summary>
        /// <param name="gameObject"></param>
        public static void EnsureNetworkObjectComponentsForSceneObject(GameObject gameObject) {  }

        /// <summary>
        /// Initialize a network object with sharing components needed for the selected provider
        /// </summary>
        /// <param name="gameObject"></param>
        public void EnsureNetworkObjectComponents(GameObject gameObject) { }
        #endregion ISharingProvider Functions

        #region Private Functions
        private void EnqueueAction(Action action)
        {
            if (action != null)
            {
                EnqueueAction((CancellationToken token) => action());
            }
        }

        private void EnqueueAction(Action<CancellationToken> action)
        {
            if (action != null && !_isDisposed)
            {
                _queuedWork.Enqueue(action);
            }
        }

        private Task EnqueueActionWithTask(Func<CancellationToken, Task> action)
        {
            if (action != null && !_isDisposed)
            {
                var taskSource = new TaskCompletionSource<object>();
                _queuedWork.Enqueue(async (CancellationToken ct) =>
                {
                    try
                    {
                        await action(ct);
                        taskSource.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        taskSource.TrySetException(ex);
                    }
                });
                return taskSource.Task;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private Task<T> EnqueueActionWithTask<T>(Func<CancellationToken, Task<T>> action)
        {
            if (action != null && !_isDisposed)
            {
                var taskSource = new TaskCompletionSource<T>();
                _queuedWork.Enqueue(async (CancellationToken ct) =>
                {
                    try
                    {
                        taskSource.TrySetResult(await action(ct));
                    }
                    catch (Exception ex)
                    {
                        taskSource.TrySetException(ex);
                    }
                });
                return taskSource.Task;
            }
            else
            {
                return Task.FromResult<T>(default);
            }
        }

        private void DisposeSessionComponents()
        {
            ClearRoomState();
        }

        private Task JoinFakeRoom()
        {
            ClearRoomState();
            return Task.CompletedTask;
        }

        private Task LeaveFakeRoom()
        {
            ClearRoomState();
            return Task.CompletedTask;
        }

        private void ClearRoomState()
        {
            lock (_roomLock)
            {
                _playerProperties.Clear();
                _roomProperties.Clear();
            }
        }
        #endregion Private Methods

        #region Logging Methods
        /// <summary>
        /// Log a message if verbose logging is enabled.
        /// </summary>
        private void LogVerbose(string message)
        {
            _logger.LogVerbose(message);
        }

        /// <summary>
        /// Log a message if verbose logging is enabled. 
        /// </summary>
        private void LogVerbose(string messageFormat, params object[] args)
        {
            _logger.LogVerbose(messageFormat, args);
        }

        /// <summary>
        /// Log a message if information logging is enabled.
        /// </summary>
        private void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Log a message if information logging is enabled. 
        /// </summary>
        private void LogInformation(string messageFormat, params object[] args)
        {
            _logger.LogInformation(messageFormat, args);
        }

        /// <summary>
        /// Log a message if warning logging is enabled.
        /// </summary>
        private void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        /// <summary>
        /// Log a message if warning logging is enabled. 
        /// </summary>
        private void LogWarning(string messageFormat, params object[] args)
        {
            _logger.LogWarning(messageFormat, args);
        }


        /// <summary>
        /// Log a message if error logging is enabled.
        /// </summary>
        private void LogError(string message)
        {
            _logger.LogError(message);
        }

        /// <summary>
        /// Log a message if error logging is enabled. 
        /// </summary>
        private void LogError(string messageFormat, params object[] args)
        {
            _logger.LogError(messageFormat, args);
        }
        #endregion Logging Methods
    }
}