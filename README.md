# Play With Akka Cluster

## First question
I have a cluster pool router and I do not understand the behaviour around the supervisor strategy that I have set up.

Here is the definition of the supervisor strategy and the pool router

```csharp
    var supervisorStrategy = new OneForOneStrategy(
        maxNrOfRetries: 10,
        withinTimeRange: TimeSpan.FromMinutes(3),
        decider: Decider.From(e =>
        {
            if (e is DivideByZeroException ||
                e is UnknownOperationException)
                return Directive.Resume;
            return Directive.Escalate;
        }));

    var distributor =
        cluster.ActorOf(
            Props.Create(() => new Worker())
                .WithRouter(FromConfig.Instance)
                .WithSupervisorStrategy(supervisorStrategy), "distributor");
```

I have set the supervisor strategy to be "resume" if a UnknownOperationException occurs. Since this is a one for one strategy then I would expect that only one routee was affected. I have 2 routees and from their PreStart and PostStop methods methods I log a message. What I do see is that when I call
a method that throws a UnknownOperationException then both routees gets their PreStart and PostStop methods executes.

You can reproduce this by opening the solution in Visual Studio and set the following projects to run on start up:

![alt text](https://github.com/carsten-j/playWithAkkaCluster/blob/master/Images/startup-projects.PNG)

Start the solution and observe this:

![alt text](https://github.com/carsten-j/playWithAkkaCluster/blob/master/Images/worker1.PNG)

and

![alt text](https://github.com/carsten-j/playWithAkkaCluster/blob/master/Images/worker2.PNG)

where the exception is logged here:

![alt text](https://github.com/carsten-j/playWithAkkaCluster/blob/master/Images/exception.PNG)

Looking at both workers one can see that PostStop and PreStart was called for both routees. This is a surprise to me. I expected the Resume choice to means that no routees were restarted and just use existing routees continuily. What is happening here?

## Second question:
Go to the program.cs file in the watchdog project. I am trying to change the watchdog actor to be a cluster singleton. Comment out this line

```csharp
    var watchDog = cluster.ActorOf(Props.Create(() => new WatchDog()), "watchdog");
```

and uncomment these lines

```csharp
    var watchdog = cluster.ActorOf(ClusterSingletonManager.Props(
       singletonProps: Props.Create(() => new WatchDog()),
       terminationMessage: PoisonPill.Instance,
       settings: ClusterSingletonManagerSettings.Create(cluster).WithRole("watchdog")),
       name: "watchdog");
```

Rebuild and start solution like before. It is the responsibility of the watchdog actor to start the processes that hosts the workers (routees). After changing to the singleton this does not happen. The worker processes / actors do not start. I have tried adding a breakpoint in the ctor of the watchdog actor but it is not being hit. I have also tried to diagnoze with the Petabridge PBM commandline tool and here I can see that the status of both the seednode and the watchdog is listed as "JOINING" and not as being "UP". When I read the doc at https://getakka.net/articles/clustering/cluster-singleton.html and I cannot see that anything should be missing in the declaration of the cluster singleton. Do you have any idea about why this does not work?


