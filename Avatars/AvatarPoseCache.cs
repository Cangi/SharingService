using System;
using System.Threading;
using UnityEngine;

namespace SharingService.Avatars
{
    /// <summary>
    /// The sharing service player pose cache.
    /// </summary>
    public class AvatarPoseCache 
    {
    private AutoResetEvent _waitForBuffer = new AutoResetEvent(false);
    private AvatarPoseCacheEntry[] _pool;
    private int _size;

    public AvatarPoseCache(int size)
    {
        _size = size;
        _pool = new AvatarPoseCacheEntry[size];
        try
        {
            for (int i = 0; i < size; i++)
            {
                _pool[i] = new AvatarPoseCacheEntry() ;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
       
    }

    /// <summary>
    /// Check out an object. This will block until an object becomes available.
    /// Once checked out, action will be invoked. After action completed, object
    /// is automatically checked in.
    /// </summary>
    public void CheckOut(Action<AvatarPoseCacheEntry> action)
    {
        AvatarPoseCacheEntry value = CheckOutWorker();
        try
        {
            action(value);
        }
        finally
        {
            CheckInWorker(value);
        }
    }

    /// <summary>
    /// Check out an object. This will block until an object becomes available.
    /// Once checked out, action will be invoked. After action completed, object
    /// is automatically checked in.
    /// </summary>
    public U CheckOut<U>(Func<AvatarPoseCacheEntry, U> action, int bufferLength = 0)
    {
        AvatarPoseCacheEntry buffer = CheckOutWorker();
        try
        {
            return action(buffer);
        }
        finally
        {
            CheckInWorker(buffer);
        }
    }

    /// <summary>
    /// Checkout object with a long term ownership
    /// </summary>
    public void CheckOut(Action<CheckoutArgs> action)
    {
        CheckoutArgs args = new CheckoutArgs(CheckOutWorker(), CheckInWorker);
        try
        {
            action(args);
        }
        finally
        {
            args.Dispose();
        }
    }

    /// <summary>
    /// Checkout buffer with a long term ownership
    /// </summary>
    public U CheckOut<U>(Func<CheckoutArgs, U> action)
    {
        CheckoutArgs args = new CheckoutArgs(CheckOutWorker(), CheckInWorker);
        try
        {
            return action(args);
        }
        finally
        {
            args.Dispose();
        }
    }
    /// <summary>
    /// Check out an object of type T. This will block until a T entry becomes available.
    /// This will fail if T doesn't implement IObjectPoolEntry<T>
    /// </summary>
    public AvatarPoseCacheEntry CheckOut()
    {
        AvatarPoseCacheEntry result = CheckOutWorker();

        if (!(result is AvatarPoseCacheEntry))
        {
            CheckInWorker(result);
            throw new InvalidOperationException("To use CheckOut() the entry must implement IObjectPoolEntry<T>");
        }

        return result;
    }

    /// <summary>
    /// Check out an object of type T. This will block until a T entry becomes available.
    /// </summary>
    private AvatarPoseCacheEntry CheckOutWorker()
    {
        AvatarPoseCacheEntry result = null;
        while (true)
        {
            lock (_pool)
            {
                for (int i = 0; i < _size; i++)
                {
                    if (_pool[i] != null)
                    {
                        result = _pool[i];
                        _pool[i] = null;
                        break;
                    }
                }
            }

            if (result != null)
            {
                if (result is AvatarPoseCacheEntry)
                {
                    var entry = result;
                    entry.OnDisposed += OnEntryDisposed;
                    entry.OnCheckOut();
                }
                    
                break;
            }

            _waitForBuffer.WaitOne();
        }

        return result;
    }

    /// <summary>
    /// Check in a class object.
    /// </summary>
    private void CheckInWorker(AvatarPoseCacheEntry value)
    {
        if (value == null)
        {
            Debug.LogError("Trying to check-in a null buffer.");
            return;
        }

        if (value is AvatarPoseCacheEntry)
        {
            var entry = value;
            entry.OnDisposed -= OnEntryDisposed;
            entry.OnCheckIn();
        }

        bool added = false;
        lock (_pool)
        {
            if (Array.IndexOf(_pool, value) < 0)
            {
                for (int i = 0; i < _size; i++)
                {
                    if (_pool[i] == null)
                    {
                        _pool[i] = value;
                        added = true;
                        break;
                    }
                }
            }
        }

        if (added)
        {
            _waitForBuffer.Set();
        }
    }

    private void OnEntryDisposed(AvatarPoseCacheEntry entry)
    {
        CheckInWorker(entry);
    }

    #region Public Struct
    public struct CheckoutArgs
    {
        private Action<AvatarPoseCacheEntry> _checkIn;

        public AvatarPoseCacheEntry Value { get; private set; }

        public CheckoutArgs(AvatarPoseCacheEntry value, Action<AvatarPoseCacheEntry> checkIn)
        {
            _checkIn = checkIn;
            Value = value;
        }

        internal void Dispose()
        {
            if (Value != null)
            {
                _checkIn?.Invoke(Value);
                Value = null;
            }
        }

        /// <summary>
        /// Move the object in this struct to pinned class which holds the object until freed
        /// </summary>
        public Pinned Move()
        {
            var result = new Pinned(Value, _checkIn);
            Value = null;
            return result;
        }
    }

    public class Pinned : IDisposable
    {
        private Action<AvatarPoseCacheEntry> _checkIn; 

        public AvatarPoseCacheEntry Value { get; private set; }

        public Pinned(AvatarPoseCacheEntry value, Action<AvatarPoseCacheEntry> checkIn)
        {
            _checkIn = checkIn;
            Value = value;
        }

        public void Dispose()
        {
            if (Value != null)
            {
                _checkIn?.Invoke(Value);
                Value = null;
            }
        }
    }
    #endregion Public Classes
}
}