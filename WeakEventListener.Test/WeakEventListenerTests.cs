using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.ComponentModel;

namespace MissingFrom.Net.Test
{
    [TestClass]
    public class WeakEventManagerTests
    {
        private TestPublisher _publisher;
        private TestSubscriber _subscriber;

        private void Setup()
        {
            // we need to instantiate these in a separate method because there are some debug features that can keep objects
            // alive for the entire scope of the method they are created in even when they go out of scope and GC.Collect
            // is called explicitly
            _publisher = new TestPublisher();
            _subscriber = new TestSubscriber(_publisher);
        }

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
        public void WeakEventManager_AddWeakEventListener_FiresCustomDelegateEvent_Test()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber(publisher);
            subscriber.StartProperty(publisher);

            publisher.FireProperty();

            Assert.AreEqual(1, subscriber.Invocations);
        }

        [TestMethod]
        public void WeakEventManager_AddWeakEventListener_FiresEventAfterGC_Test()
        {
            Setup();

            GC.Collect();

            _publisher.Fire();

            Assert.AreEqual(1, _subscriber.Invocations);
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

        [TestMethod]
        public void WeakEventManager_RemoveWeakEventListener_CustomDelegateEventNoLongerFires_Test()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber(publisher);
            subscriber.StartProperty(publisher);

            subscriber.Stop();
            publisher.FireProperty();

            Assert.AreEqual(0, subscriber.Invocations);
        }

        [TestMethod]
        public void WeakEventManager_ClearWeakEventListeners_ClearsAllListeners_Test()
        {
            var publisher1 = new TestPublisher();
            var publisher2 = new TestPublisher();
            var subscriber = new TestSubscriber(publisher1);
            subscriber.Start(publisher2);

            subscriber.Clear();
            publisher1.Fire();
            publisher2.Fire();

            Assert.AreEqual(0, subscriber.Invocations);
        }

        [TestMethod]
        public void WeakEventManager_AddWeakEventListener_Publisher_GC_Test()
        {
            Setup();
            var reference = new WeakReference(_subscriber);

            _subscriber = null;

            GC.Collect();

            Assert.IsFalse(reference.IsAlive);
            _publisher.Fire();
        }

        [TestMethod]
        public void WeakEventManager_AddWeakEventListener_Subscriber_GC_Test()
        {
            Setup();
            var reference = new WeakReference(_publisher);

            _publisher = null;
            _subscriber.Publisher = null;

            GC.Collect();

            Assert.IsFalse(reference.IsAlive);
        }

        [TestMethod]
        public void WeakEventManager_AddWeakEventListener_Both_GC_Test()
        {
            Setup();
            var reference1 = new WeakReference(_publisher);
            var reference2 = new WeakReference(_subscriber);

            _publisher = null;
            _subscriber = null;

            GC.Collect();

            Assert.IsFalse(reference1.IsAlive);
            Assert.IsFalse(reference2.IsAlive);
        }

        [TestMethod]
        public void WeakEventManager_Perf_Test()
        {
            const int iterations = 100_000;
            var weakRegister = Time(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var publisher = new TestPublisher();
                    var subscriber = new TestSubscriber(publisher);
                }
            });
            var normalRegister = Time(() =>
            {
                for (int i = 0; i < iterations; i++)
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
                for (int i = 0; i < iterations; i++)
                {
                    publisher.Fire();
                }
            });
            var normalFire = Time(() =>
            {
                var publisher = new TestPublisher();
                int x = 0;
                publisher.TheEvent += (s, e) => x++;
                for (int i = 0; i < iterations; i++)
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

        public static long Time(Action action)
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

    public interface ITestEventSource : INotifyPropertyChanged
    {
        event EventHandler<TestEventArgs> TheEvent;
    }

    public class TestPublisherBase : ITestEventSource
    {
        public event EventHandler<TestEventArgs> TheEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnTheEvent([CallerMemberName] string name = "")
        {
            TheEvent?.Invoke(this, new TestEventArgs(name));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class TestPublisher : TestPublisherBase
    {
        public void Fire() => OnTheEvent();
        public void FireProperty() => OnPropertyChanged();
    }

    public class TestSubscriber
    {
        public int Invocations { get; private set; }

        public TestPublisher Publisher { get; set; }

        private readonly WeakEventManager _manager = new WeakEventManager();

        public TestSubscriber(TestPublisher publisher)
        {
            Publisher = publisher;
            _manager.AddWeakEventListener<TestPublisher, TestEventArgs>(publisher, nameof(publisher.TheEvent), OnTheEvent);
        }

        public void Start(TestPublisher publisher)
        {
            _manager.AddWeakEventListener<TestPublisher, TestEventArgs>(publisher, nameof(publisher.TheEvent), OnTheEvent);
        }

        public void StartProperty(TestPublisher publisher)
        {
            _manager.AddWeakEventListener<TestPublisher, PropertyChangedEventArgs>(publisher, nameof(publisher.PropertyChanged), OnPropertyChanged);
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
        }

        private void OnPropertyChanged(TestPublisher sender, PropertyChangedEventArgs e)
        {
            Invocations++;
        }
    }
}
