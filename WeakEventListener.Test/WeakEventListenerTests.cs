using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DM.Core.Events;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace WeakEventListener.Test
{
    [TestClass]
    public class WeakEventListenerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void WeakEventManager_AddWeakEventListener_FiresEvent_Test()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber(publisher);

            publisher.Fire();

            Assert.AreEqual(1, subscriber.Invocations);
        }

        [TestMethod]
        public void WeakEventManager_RemoveWeakEventListener_EventNoLongerFires_Test()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber(publisher);

            subscriber.Stop();
            publisher.Fire();

            Assert.AreEqual(0, subscriber.Invocations);
        }

        [TestMethod]
        public void WeakEventManager_ClearWeakEventListeners_EventNoLongerFires_Test()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber(publisher);

            subscriber.Clear();
            publisher.Fire();

            Assert.AreEqual(0, subscriber.Invocations);
        }

        private TestPublisher _publisher;
        private TestSubscriber _subscriber;

        private void Setup()
        {
            _publisher = new TestPublisher();
            _subscriber = new TestSubscriber(_publisher);
        }

        [TestMethod]
        public void WeakEventManager_AddWeakEventListener_GC_Test()
        {
            Setup();
            var reference = new WeakReference(_subscriber);

            _subscriber = null;

            GC.Collect();

            Assert.IsFalse(reference.IsAlive);
            _publisher.Fire();
        }

        [TestMethod]
        public void WeakEventManager_Perf_Test()
        {
            const int iterations = 100_000;
            var weakRegister = Time(() =>
            {
                for(int i = 0; i < iterations; i++)
                {
                    var publisher = new TestPublisher();
                    var subscriber = new TestSubscriber(publisher);
                }
            });
            var normalRegister = Time(() =>
            {
                for(int i = 0; i < iterations; i++)
                {
                    var publisher = new TestPublisher();
                    int x = 0;
                    publisher.TheEvent += (s, e) => x++;
                }
            });
            var weakFire = Time(() =>
            {
                var publisher = new TestPublisher();
                var subscriber = new TestSubscriber(publisher);
                for(int i = 0; i < iterations; i++)
                {
                    publisher.Fire();
                }
            });
            var normalFire = Time(() =>
            {
                var publisher = new TestPublisher();
                int x = 0;
                publisher.TheEvent += (s, e) => x++;
                for(int i = 0; i < iterations; i++)
                {
                    publisher.Fire();
                }
            });
            TestContext.WriteLine($@"
weak register:   {weakRegister}ms
normal register: {normalRegister}ms
weak fire:       {weakFire}ms
normal fire:     {normalFire}ms
            ");
        }

        public long Time(Action action)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }

    public class TestEventArgs : EventArgs
    {
        public string Value { get; }
        public TestEventArgs(string value)
        {
            Value = value;
        }
    }

    public interface ITestEventSource
    {
        event EventHandler<TestEventArgs> TheEvent;
    }

    public class TestPublisherBase : ITestEventSource
    {
        public event EventHandler<TestEventArgs> TheEvent;

        protected void OnTheEvent([CallerMemberName] string name = "")
        {
            TheEvent?.Invoke(this, new TestEventArgs(name));
        }
    }

    public class TestPublisher : TestPublisherBase
    {
        public void Fire() => OnTheEvent();
    }

    public class TestSubscriber
    {
        public int Invocations { get; private set; }

        public TestPublisher Publisher { get; set; }

        private WeakEventManager _manager = new WeakEventManager();

        public TestSubscriber(TestPublisher publisher)
        {
            Publisher = publisher;
            _manager.AddWeakEventListener<TestPublisher, TestEventArgs>(publisher, nameof(publisher.TheEvent), OnTheEvent);
        }

        public void Stop()
        {
            _manager.RemoveWeakEventListener(Publisher);
        }

        public void Clear()
        {
            _manager.ClearWeakEventListeners();
        }

        private void OnTheEvent(TestPublisher sender, TestEventArgs e)
        {
            Invocations++;
            Assert.AreEqual(sender, Publisher);
        }
    }
}
