using System;
using System.ComponentModel;
using System.Threading;
using ServerInterfaces;

namespace Lodky
{
    /// <summary>
    /// class for handling events of callback
    /// </summary>
    internal class DuplexCallback : IServerServiceCallback
    {
        private readonly SynchronizationContext _syncContext = AsyncOperationManager.SynchronizationContext;

        /// <summary>
        /// Send callback event
        /// </summary>
        /// <param name="e"></param>
        public void SendCallback(Event e)
        {
            _syncContext.Post(OnServiceCallbackEvent, e);
        }

        public event EventHandler<Event> ServiceCallbackEvent;

        /// <summary>
        /// Invoke event on main window
        /// </summary>
        /// <param name="paEvent"></param>
        private void OnServiceCallbackEvent(object paEvent)
        {
            var handler = ServiceCallbackEvent;
            var e = paEvent as Event;
            handler?.Invoke(this, e);
        }
    }
}