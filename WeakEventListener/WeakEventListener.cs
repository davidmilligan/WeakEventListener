using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MissingFrom.Net
{
    internal interface IWeakEventListener
    {
        bool IsAlive { get; }
        object Source { get; }
        Delegate Handler { get; }
        void StopListening();
    }

    internal class WeakEventListener<T, TArgs> : IWeakEventListener
        where T : class
        where TArgs : EventArgs
    {
        private readonly WeakReference<T> _source;
        private readonly WeakReference<Action<T, TArgs>> _handler;
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

        public Delegate Handler
        {
            get
            {
                if (_handler.TryGetTarget(out var handler))
                {
                    return handler;
                }
                return null;
            }
        }

        public WeakEventListener(T source, string eventName, Action<T, TArgs> handler)
        {
            _source = new WeakReference<T>(source ?? throw new ArgumentNullException(nameof(source)));
            _handler = new WeakReference<Action<T, TArgs>>(handler ?? throw new ArgumentNullException(nameof(handler)));
            _eventInfo = source.GetType().GetEvent(eventName) ?? throw new ArgumentException("Unknown Event Name", nameof(eventName));
            if (_eventInfo.EventHandlerType == typeof(EventHandler<TArgs>))
            {
                _eventInfo.AddEventHandler(source, new EventHandler<TArgs>(HandleEvent));
            }
            else //the event type isn't just an EventHandler<> so we have to create the delegate using reflection
            {
                _eventInfo.AddEventHandler(source, Delegate.CreateDelegate(_eventInfo.EventHandlerType, this, nameof(HandleEvent)));
            }
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
                if (_eventInfo.EventHandlerType == typeof(EventHandler<TArgs>))
                {
                    _eventInfo.RemoveEventHandler(source, new EventHandler<TArgs>(HandleEvent));
                }
                else
                {
                    _eventInfo.RemoveEventHandler(source, Delegate.CreateDelegate(_eventInfo.EventHandlerType, this, nameof(HandleEvent)));
                }
            }
        }
    }
}
