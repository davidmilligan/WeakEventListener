using System;
using System.Collections.Generic;

namespace MissingFrom.Net
{
    /// <summary>
    /// Provides methods for registering and unregisrting event handlers that 
    /// don't cause memory "leaks" when the lifetime of the listener is longer
    /// than the lifetime of the object being listened to.
    /// </summary>
    /// <remarks>
    /// Stand-in for missing weak event functionality in netstandard/core.
    /// This implementation is very simple. It is not nearly as sophisticated
    /// or efficient as the WeakEventManager built into the full .Net Framework, 
    /// nor is it a drop-in replacement. But it does accomplish the basic
    /// requirement of event handlers that don't cause strong references to
    /// the object doing the listening.
    /// This implementation does also provide an additional benefit and workaround
    /// to a shortcoming in the standard event handler signature in C#, that is:
    /// the sender parameter of the event handler may be strongly typed, rather
    /// than being simply `object` (this is the way it should have been from the
    /// beginning IMO, but it's too late for them to change now), avoiding the 
    /// need for the cast that is typically required to use the `sender`.
    /// </remarks>
    public class WeakEventManager
    {
        private List<IWeakEventListener> _listeners = new List<IWeakEventListener>();

        /// <summary>
        /// Registers the given delegate as a handler for the event specified by `eventName` on the given source.
        /// </summary>
        public void AddWeakEventListener<T, TArgs>(T source, string eventName, Action<T, TArgs> handler)
            where T : class
            where TArgs : EventArgs
        {
            _listeners.Add(new WeakEventListener<T, TArgs>(source, eventName, handler));
        }

        /// <summary>
        /// Unregisters any previously registered weak event handlers on the given source object
        /// </summary>
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

        /// <summary>
        /// Unregisters all weak event listeners that have been registered by this weak event manager instance
        /// </summary>
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