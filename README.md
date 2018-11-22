# playWithAkkaCluster

clone project and do the following to reproduce the problem

from a command line
cd seednode
dotnet run

from another command line
cd worker
dotnet run

from a third command line
cd watchdog
dotnet run


The seednode starts fine. The worker also starts fine and join the cluster
in a worker role.

The watchdog has a watchdog role and should deploy a router and actor on
the node with the worker role. But this produces the following error

[21:26:56 ERR] Error while creating actor instance of type Akka.Actor.ActorBase with 0 args: ()
[akka.tcp://MyCluster@localhost:63217/remote/akka.tcp/MyCluster@localhost:63222/user/worker/c1#2010254328]: Akka.Actor.ActorInitializationException: Exception during creation ---> System.TypeLoadException: Error while creating actor instance of type Akka.Actor.ActorBase with 0 args: () ---> System.InvalidOperationException: No actor producer specified!
   at Akka.Actor.Props.DefaultProducer.Produce()
   at Akka.Actor.Props.NewActor()
   --- End of inner exception stack trace ---
   at Akka.Actor.Props.NewActor()
   at Akka.Actor.ActorCell.CreateNewActorInstance()
   at Akka.Actor.ActorCell.<>c__DisplayClass109_0.<NewActor>b__0()
   at Akka.Actor.ActorCell.UseThreadContext(Action action)
   at Akka.Actor.ActorCell.NewActor()
   at Akka.Actor.ActorCell.Create(Exception failure)
   --- End of inner exception stack trace ---
   at Akka.Actor.ActorCell.HandleFailed(Failed f)
   at Akka.Actor.ActorCell.SysMsgInvokeAll(EarliestFirstSystemMessageList messages, Int32 currentStat
   