# Play With Akka Cluster

* Build the solution using the build.ps1 PowerShell script in the root of the repository.
* Start the cluster with the Akka.ps1 PowerShell script

Watch the cluster get up and running and see how the watchdog node sends messages in a round-robin way to the 2 worker nodes.

Try to kill one of the worker nodes. After a few seconds the remaining nodes will stabilize. You can try to add a new worker node by cd into the worker directory and execute "dotnet run". The new node will join the cluster and start processing messages.

