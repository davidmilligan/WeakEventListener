using System;
using System.Collections.Generic;
using System.Reflection;

namespace DM.Core.Events
{
    public interface IWeakEventListener
    {
        bool IsAlive { get; }
        object Source { get; }
        void StopListening();
    }

    public class WeakEventListener<T,TArgs> : IWeakEventListener
        where T : class
        where TArgs : EventArgs
    {
        private readonly WeakReference<T> _source;
        private readonly WeakReference<Action<T,TArgs>> _handler;
        private readonly EventInfo _eventInfo;

        public bool IsAlive => _handler.TryGetTarget(out var _) && _source.TryGetTarget(out var __);
        public object Source 
        {
            get
            {
                if (_source.TryGetTarget(out var source))
                {
                    return source;
                }
                return null;
            }
        }

        public WeakEventListener(T source, string eventName, Action<T,TArgs> handler)
        {
            _source = new WeakReference<T>(source ?? throw new ArgumentNullException(nameof(source)));
            _handler = new WeakReference<Action<T,TArgs>>(handler) ?? throw new ArgumentNullException(nameof(handler));
            _eventInfo = source.GetType().GetEvent(eventName) ?? throw new ArgumentException("Unknown Event Name", nameof(eventName));
            _eventInfo.AddEventHandler(source, new EventHandler<TArgs>(HandleEvent));
        }

        private void HandleEvent(object source, TArgs e)
        {
            if (_handler.TryGetTarget(out var handler))
            {
                handler(source as T, e);
            }
            else
            {
                StopListening();
            }
        }

        public void StopListening()
        {
            if (_source.TryGetTarget(out var source))
            {
                _eventInfo.RemoveEventHandler(source, new EventHandler<TArgs>(HandleEvent));
            }
        }
    }

    public class WeakEventManager
    {
        private List<IWeakEventListener> _listeners = new List<IWeakEventListener>();

        public void AddWeakEventListener<T,TArgs>(T source, string eventName, Action<T,TArgs> handler)
            where T : class
            where TArgs : EventArgs
        {
            _listeners.Add(new WeakEventListener<T,TArgs>(source, eventName, handler));
        }

        public void RemoveWeakEventListener<T>(T source)
            where T : class
        {
            var toRemove = new List<IWeakEventListener>();
            foreach (var listener in _listeners)
            {
                if (!listener.IsAlive)
                {
                    toRemove.Add(listener);
                }
                else if (listener.Source == source)
                {
                    listener.StopListening();
                    toRemove.Add(listener);
                }
            }
            foreach (var item in toRemove)
            {
                _listeners.Remove(item);
            }
        }

        public void ClearWeakEventListeners()
        {
            foreach (var listener in _listeners)
            {
                if (listener.IsAlive)
                {
                    listener.StopListening();
                }
            }
            _listeners.Clear();
        }
    }
}
