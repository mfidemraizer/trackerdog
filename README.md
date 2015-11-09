# Welcome to TrackerDog!


## What is TrackerDog?

TrackerDog works on turning your POCOs into change-tracked objects. It can track a full object graph, including collection properties.



## How to install it?

TrackerDog is distributed as a NuGet package called _TrackerDog_. Follow <a href="https://nuget.org/TrackerDog">this link to get installation instructions</a>.



## Getting started
<a href="https://github.com/mfidemraizer/trackerdog/wiki/52e40f26-3dfe-47e0-adf1-09233e98f42e">Follow up this tutorial</a> to become object change tracking expert!

## Why TrackerDog?

When implementing some design patterns like _Unit of Work_ in a very seamless way, it is required that the whole _Unit of Work_ could track object changes and produce _new_ or _updated_ object persistence actions in order to let a given peristence layer work on the so-called operations.


Most enterprise applications work with _object-relational mappers (OR/M)_ like _Entity Framework_ and _NHibernate_ which, as full _OR/M_ frameworks, they can track persistent object changes and generate an underlying SQL `INSERT`, `UPDATE` or `DELETE` under the hoods thanks to their built-in change tracking.


At the end of the day, these _OR/M_ frameworks implement change tracking turning your POCO class instances into _proxies_ that intercept your POCO property changes and once you call the _commit_-specific method of their built-in _unit of work_ they can persist changes to the database without your intervention.


So, what happens when you don't use an _OR/M_ or, even worse: **what happens when you don't use an _OR/M_ but you want to implement an _unit of work_ capable of tracking your POCO changes?**


A good example of implementing a change tracking can be an _unit of work_ to provide an abstraction layer over a NoSQL driver like Mongo, Couch, Cassandra, Redis... Even when some of them have some kind of atomically-executed set of commands, they will not track changes in your application layer. Actually there should be more possible use cases, but one of most important use cases is implementing a true _domain-driven design_ architecture ensuring that a domain unit of work can track changes either when you use a regular _data mapper_ (for which you won't use TrackerDog) or you want to implement _repositories_ that return change-trackable domain objects!


In summary, TrackerDog turns your object graphs into interceptable object proxies that share a common _change tracker_ which let you check what has changed in your objects or even accept or undo changes to a given object graph.
