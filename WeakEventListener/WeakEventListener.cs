using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MissingFrom.Net
{
    internal interface IWeakEventListener
    {
        bool IsAlive { get; }
        object Source { get; }
        void StopListening();
    }

    internal class WeakEventListener<T, TArgs> : IWeakEventListener
        where T : class
        where TArgs : EventArgs
    {
        private static readonly ConcurrentDictionary<string, EventInfo> _eventInfos = new ConcurrentDictionary<string, EventInfo>();
        private readonly WeakReference<T> _source;
        private readonly WeakReference<Action<T, TArgs>> _handler;

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

        public WeakEventListener(T source, string eventName, Action<T, TArgs> handler)
        {
            _source = new WeakReference<T>(source ?? throw new ArgumentNullException(nameof(source)));
            _handler = new WeakReference<Action<T, TArgs>>(handler) ?? throw new ArgumentNullException(nameof(handler));
            if (!_eventInfos.TryGetValue(eventName, out var eventInfo))
            {
                eventInfo = source.GetType().GetEvent(eventName) ?? throw new ArgumentException("Unknown Event Name", nameof(eventName));
                _eventInfos.TryAdd(eventName, eventInfo);
            }
            eventInfo.AddEventHandler(source, new EventHandler<TArgs>(HandleEvent));
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
                foreach (var eventInfo in _eventInfos.Values)
                {
                    eventInfo.RemoveEventHandler(source, new EventHandler<TArgs>(HandleEvent));
                }
            }
        }
    }
}
