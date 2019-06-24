Weak Event Listeners
---------------------

[![Build Status](https://dev.azure.com/davidlmilligan/davidlmilligan/_apis/build/status/davidmilligan.WeakEventListener?branchName=master)](https://dev.azure.com/davidlmilligan/davidlmilligan/_build/latest?definitionId=1&branchName=master)

This library provides methods for registering and unregistering event handlers that don't cause memory "leaks" when the lifetime of the listener is longer than the lifetime of the object being listened to (one of the most common scenarios for memory leaks in C#/.Net).

Some sort of weak event API really should be apart of .Net Standard/Core but [sadly it is not](https://github.com/dotnet/corefx/issues/11898) (though it is in the full .Net Framework). This implementation is very simple. It is not nearly as sophisticated or efficient as the WeakEventManager built into the full .Net Framework, nor is it a drop-in replacement. But it does accomplish the basic requirement of event handlers that don't cause strong references to the object doing the listening.

This implementation does also provide an additional benefit and workaround to a shortcoming in the standard event handler signature in C#, that is: the sender parameter of the event handler may be strongly typed, rather than being simply `object` (this is the way it should have been from the beginning IMO, but it's too late for them to change now), avoiding the need for the cast that is typically required to use the `sender`.

### Examples

Create an instance of `WeakEventManager` in the class that wishes to register weak events, or alternatively, create a singleton instance of `WeakEventManager` to be shared. In this example, we'll assume a type `Foo` has an event `Bar` of type `EventHandler<BarEventArgs>` we want to register an event for.

```
pubic class MySubsriber
{
    private WeakEventManager manager = new WeakEventManager();

```

Declare your event handler (the sender parameter may be strongly typed)

```
    private void HandleFooBar(Foo sender, BarEventArgs e)
    {
        //do something...
    }
```

Register your event handler:

```
    public MySubscriber(Foo foo)
    {
        manager.AddWeakEventListener<Foo, BarEventArgs>(foo, nameof(foo.Bar), HandleFooBar);
    }
```