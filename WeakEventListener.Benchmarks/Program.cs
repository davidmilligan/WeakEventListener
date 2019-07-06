using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MissingFrom.Net.Test;

namespace MissingFrom.Net.Benchmarks
{
    public class RegisterBenchmarks
    {
        [Benchmark]
        public void RegisterWeak()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber();
            subscriber.Start(publisher);
        }

        [Benchmark]
        public void RegisterWeakCustom()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber();
            subscriber.StartCustom(publisher);
        }

        [Benchmark]
        public void RegisterWeakTyped()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber();
            subscriber.StartTyped(publisher);
        }

        [Benchmark]
        public void RegisterWeakProperty()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber();
            subscriber.StartProperty(publisher);
        }

        [Benchmark]
        public void RegisterWeakTypedCustom()
        {
            var publisher = new TestPublisher();
            var subscriber = new TestSubscriber();
            subscriber.StartTypedCustom(publisher);
        }

        [Benchmark(Baseline = true)]
        public void RegisterNormal()
        {
            var publisher = new TestPublisher();
            int x = 0;
            publisher.TheEvent += (s, e) => x++;
        }
    }

    public class FireBenchmarks
    {
        TestPublisher _publisher = null;
        TestSubscriber _subscriber = null;

        [GlobalSetup(Target = nameof(FireWeak))]
        public void FireWeak_Setup()
        {
            _publisher = new TestPublisher();
            _subscriber = new TestSubscriber(_publisher);
        }

        [Benchmark]
        public void FireWeak() => _publisher.Fire();

        NormalSubscriber _normalSubscriber = null;

        [GlobalSetup(Target = nameof(FireNormal))]
        public void FireNormal_Setup()
        {
            _publisher = new TestPublisher();
            _normalSubscriber = new NormalSubscriber(_publisher);
        }

        [Benchmark(Baseline = true)]
        public void FireNormal() => _publisher.Fire();
    }

    public class NormalSubscriber
    {
        public int Invocations { get; private set; }

        public TestPublisher Publisher { get; set; }

        public NormalSubscriber(TestPublisher publisher)
        {
            Publisher = publisher;
            publisher.TheEvent += OnTheEvent;
        }

        private void OnTheEvent(object sender, TestEventArgs e)
        {
            Invocations++;
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<RegisterBenchmarks>();
            BenchmarkRunner.Run<FireBenchmarks>();
        }
    }
}
