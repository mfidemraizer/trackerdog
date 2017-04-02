## WARNING!

**Current supported branch is [`v2-net-standard`](https://github.com/mfidemraizer/trackerdog/tree/v2-net-standard)**. Please [follow this link](https://github.com/mfidemraizer/trackerdog/tree/v2-net-standard) to switch to the new branch. 

Until Visual Studio 2017 goes RTM, `v2-net-standard` won't be merged into `v2` branch. In the mean time, if you want to collaborate, please fork the `v2-net-standard` branch.


# Welcome to TrackerDog!

![trackerdog build badge](https://mfidemraizer.visualstudio.com/_apis/public/build/definitions/6b21f8d5-b74c-4f26-8e06-f156bd0ab331/1/badge)
![project logo](http://mfidemraizer.github.io/trackerdog/media/dogtracker.png)

## What is TrackerDog?

TrackerDog works on seamlessly turning your .NET objects into change-tracked objects. It can track a full object graph, including collection properties.

Also, since version 2.2.0, **TrackerDog works on both full .NET Framework and .NET Standard-compliant platforms like .NET Core or Xamarin**!

<img src="https://docs.microsoft.com/en-us/dotnet/articles/images/hub/netcore.svg" width="100"> 
<img src="https://docs.microsoft.com/en-us/dotnet/articles/images/hub/net.svg" width="100"> 
<img src="https://docs.microsoft.com/en-us/dotnet/articles/images/hub/xamarin.svg" width="100">

## How to install it?

TrackerDog is distributed as a NuGet package called _TrackerDog_. Follow <a href="https://www.nuget.org/packages/TrackerDog/">this link to get installation instructions</a>.

## Getting started
<a href="http://mfidemraizer.github.io/trackerdog/html/52e40f26-3dfe-47e0-adf1-09233e98f42e.htm">Follow up this tutorial</a> to become object change tracking expert!

If you're looking for API reference, [follow this link](http://mfidemraizer.github.io/trackerdog).

## Why TrackerDog?

When implementing some design patterns like _Unit of Work_ in a very seamless way, it is required that the whole _Unit of Work_ could track object changes and produce _new_ or _updated_ object persistence actions in order to let a given peristence layer work on the so-called operations.

Most enterprise applications work with _object-relational mappers (OR/M)_ like _Entity Framework_ and _NHibernate_ which, as full _OR/M_ frameworks, they can track persistent object changes and generate an underlying SQL `INSERT`, `UPDATE` or `DELETE` under the hoods thanks to their built-in change tracking.

At the end of the day, these _OR/M_ frameworks implement change tracking turning your POCO class instances into _proxies_ that intercept your POCO property changes and once you call the _commit_-specific method of their built-in _unit of work_ they can persist changes to the database without your intervention.

So, what happens when you don't use an _OR/M_ or, even worse: **what happens when you don't use an _OR/M_ but you want to implement an _unit of work_ capable of tracking your POCO changes?**

A good example of implementing a change tracking can be an _unit of work_ to provide an abstraction layer over a NoSQL driver like Mongo, Couch, Cassandra, Redis... Even when some of them have some kind of atomically-executed set of commands, they will not track changes in your application layer. Actually there should be more possible use cases, but one of most important use cases is implementing a true _domain-driven design_ architecture ensuring that a domain unit of work can track changes either when you use a regular _data mapper_ (for which you won't use TrackerDog) or you want to implement _repositories_ that return change-trackable domain objects!

In summary, TrackerDog turns your object graphs into interceptable object proxies that share a common _change tracker_ which let you check what has changed in your objects or even accept or undo changes to a given object graph.