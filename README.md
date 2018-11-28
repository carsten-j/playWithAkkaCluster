# Play With Akka Cluster

## Main concern

Execute the akka.ps1 script in the root. This will start 4 nodes in total. 1 seednode, 1 watchdog, and 2 workers. 

The watchdog will sent messages to the 2 workers (who are behind a round robin router). So far so good and print the result in the window of the worker. The message is a simply message to add 2 numbers.

If I now kill one worker (to simulate process crash) then the remaining cluster nodes prints a lots of gossip and the remaing worker node will keep processing messages. 

However if I then cd into the worker folder and launch a new worker by "dotnet run" then a new node is started. I do not believe this new node joins the cluster. At least it does not start processing messages in a round robin fashion.

This is simply do not understand. It is my general understanding that new nodes should be able to join the cluster. What is happening here? Should the cluster not be able to survive a hard process crash for one of the nodes?

Another observation. If I instead of killing a node cd into the worker folder and run "dotnet run" then a 3rd worker node start up, joins the cluster, and starts processing messages.

The scenario can be reproduced by using the build.ps1 script in the root to build the solution and then the akka.ps1 script to start the cluster. Starting another worker node can be done by cd into the worker folder and execute "dotnet run".

## Additional questions

### How to wait for the nodes to join the cluster before starting the router

In the example above the router is started from the watchdog actor. What measures can I take to
ensure 1) the a certain number of worker nodes have joined the cluster before the router is started and 2) if messages are being sent before the worker nodes joined the cluster and started the router, are these messages then lost. If so, how to avoid that?

### How to avoid coupling on message level for the Akka cluster client example 

In the example you gave me with the Akka Cluster Client the client and the nodes in the cluster shares the PING and PONG messages from a shared assembly. I am pretty sure that we touched upon this in the course. But I cannot remember the suggestion solution to get around this. It must be something along the lines of maybe sending json and do the serialization/deserialization in a custom way? Do you know of some example where I can see something like this in action?












