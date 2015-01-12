ElasticAnalytics
=============

A more scalable, flexible, testable analytics persistence layer for Sitecore, shipping with an Elasticsearch implementation.

Well... that was the aim.  In it's current state, this will persist and load Sitecore contacts in Elasticsearch,  but not all methods on the data access component have been implemented (to persist interactions for example), so this won't actually work in an instance of Sitecore.

It turns out Sitecore is a little... chaotic in its abstractions around analytics data persistence and retrieval, and I just can't commit the (spare) time to implement everything that needs doing, including the cleaning and refactoring that's required.  Anyway, if you're interested, read on a little.

Motivations
---
- A Sitecore related project that I could get interested in to fill in spare moments, that allowed me to learn more about Sitecore, provide some new ideas to the company, and utilised some of my existing knowledge.

- Investigate how easy it is to get rid of Sitecore's dependency on Mongo - Mongo isn't a great choice for analytics data persistence.

- Introduce some ideas around multi-tenancy hosting - useful for both partners and Sitecore's hosted offering.

- Demonstrate the power of data storage and search provided by the same storage mechanism, and more specifically the power and features of Elasticsearch, which I know from past experience.

- Provide an example internally, for what decoupled, OO, testable code can look like.

Concepts
---
###Alternative Data Providers

Sitecore OMS does currently provide some abstractions around data persistence, allowing alternative implementations to be used.  Hopefully, these are sufficient across the whole platform to provide an Elasticsearch based implementation to remove the need for Mongo.

###Re-Implemenation with better abstractions

The current OMS + Mongo implementation could do a better job of identifying appropriate abstractions for data persistence; a lot of logic is tied to the Mongo implementation which needn't be.  ElasticAnalytics is not a straight port of the Mongo implementation for Elasticsearch.  It is a re-implementation with more appropriate abstractions defined, to de-couple as much logic from the database vendor specific code as possible.  As a result, it's database persistence layer is much thinner.  

Initially ElasticAnalytics will ship with only an Elasticsearch persistence layer, but supports the possiblity for supporting other databases easily in the future.

###Multi-tenancy support in Elasticsearch

Systems like Elasticsearch, and Mongo for that fact, like to be the primary service running on a machine, and for there to be only one instance running.  This allows them to manage memory consumption reliably and efficiently in conjunction with the operating system.  To support storage of analytics data for multiple Sitecore instances on the same hardware (i.e. the same Elasticsearch cluster), we therefore want to be able to store the analytics data for multiple Sitecore instances in the same cluster.

With Elasticsearch this is in fact trivial and ElasticAnalytics addresses this; data for a Sitecore instance is stored in its own index that has a unique name related to that instance.  Done. Easy.  Additionally the right abstractions are in place to enable load balancing across multiple Elasticsearch clusters; i.e. serve different Sitecore instances from different clusters.  There isn't currently an implementation in place for this, but would be straightforward to introduce.

###Multi-tenancy support with service layer

ElasticAnalytics is designed to support different hosting options.  The simplest one resembles the current Mongo implementation; the Sitecore CD instances are configured with a connection string to the database, and all work is performed in-process in the ASP.Net worker process.

_Currently, this is the only one implemented_, but the abstractions are in place to easily implement the following.

The second option is to host a service, probably on separate hardware from the websites, that provides an abstraction on top of the requests to the analytics data.  The service is then configured with the database connection details.  Sitecore instances, with an ElasticAnalytics component, are then configured to communicate with the service for any analytics data requests.  _This service is not dependent on Sitecore_ - it is not hosted inside a Sitecore instance.

A single instance of this service can honour requests from multiple Sitecore instances; the context of the instances is provided on each request.  Many service instances could therefore easily be hosted to load balance requests if required.

It is envisaged that multiple hosting options will be provided for the service; Windows service, IIS, console app etc.

The advantages to this Service model are several:

1. to reduce the CPU load on the content delivery instances
2. to move the logic around serving the requests closer to the data; relevant when the database is hosted far away from the Sitecore instances for operations like contact locking.
3. to introduce a physical abstraction between the database and the content delivery instances (think about providing xDB as-a-service).

###Testability

All of the ElasticAnalytics code is testable; OO design principles are used throughout, and any external dependencies to a class are provided to it's constructor.  There's no static classes :-)

The current test coverage isn't complete due to time constraints, but _everything is testable_, and there are significant unit and integration tests in place (using XUnit, AutoFixture, Moq, FluentAssertions).  Most of the tests are integration tests; i.e. integrated with Elasticsearch.  The data for each test is logically sandboxed, and so all  tests can run concurrently without conflict.  Indeed I am currently using NCrunch to run the tests, and with its concurrency settings set to max, I comfortably get continuous _integration_ tests as I type, hitting the database, on my dev machine (a modest Dell with SSD)!

###Configurability/Composability

The Inversion of Control pattern is used throughout the ElasticAnalytics code base, and a Dependency Container abstraction is defined and used at the composition route.

A Castle Windsor implementation is provided out of the box; the container can be configured in the configuration file.  However dependency containers are not the focus of this project, so I don't want to dwell on it.  Just take away that one is in use, and there are no hard dependencies on the one I have picked; only the test assemblies take a dependency on Castle Windsor.

###Working with the code base
To run this code, means to run the integration tests.  

####Prerequisits
Here's what you need to do that.

- Elasticsearch installed somewhere on a port accessible by the code machine (see [Chocolatey](https://chocolatey.org/packages/elasticsearch) for an easy way to install it (as a Windows service).

- The connection string to Elasticsearch is specified in each of the test assembly app.config files (currently set to http://127.0.0.1:9200).

- A test runner that can run [Xunit.net](https://github.com/xunit/xunit) tests (available as an extension to VS via VS tools->extensions).

- Sitecore dlls v8.0 update 1.  The following dlls must be placed in code\libs directory.

  - Sitecore.Abstractions
  - Sitecore.Analytics
  - Sitecore.Analytics.DataAccess
  - Sitecore.Analytics.Model
  - Sitecore.Kernel

####Code structure

Here's the overall structure of the code:

- **ElasticAnalytics.ScAdapter**
The top level project, that takes a dependency on Sitecore.   The `Sitecore.Analytics.DataAccess.DataAdapterProvider` is implemented by the `ElasticAnalyticsDataAdapterProvider` class.  This is the entry point for the code, and also serves as the composition root for the dependency container.   

This assembly also contains the abstraction for the dependency container (`IElasticAnalyticsIoCContainer`).

- **ElasticAnalytics.Model**
Defines the data types for Contacts, Leases etc.

- **ElasticAnalytics.Service.Types**
Defines the interfaces for the service layer.

- **ElasticAnalytics.Service**
The primary logical implementation for the service layer.

- **ElasticAnalytics.Repository.Types**
Defines the interfaces for the repositories: (`IRepository<T>`, `IConcurrencyControlRepository<T>`)

- **ElasticAnalytics.Repository.Elasticsearch**
The Elasticsearch repository implementation.

- **ElasticAnalytics.Container.Windsor**
The Castle Windsor implementation of the `IElasticAnalyticsIoCContainer`.

- **ElasticAnalytics.Configuration.Windsor**
The Castle Windsor component configuration.

Here's a project dependency graph that might help:

![ElasticAnalytics project dependency graph](https://raw.githubusercontent.com/trevorreeves/Sitecore-ElasticAnalytics/master/docs/Project%20Dependency%20Graph.png)

