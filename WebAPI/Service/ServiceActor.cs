using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.Client;
using WatchDog;

namespace WebAPI.Service
{

    public interface IActorService
    {
        Task<CalculationResult> Get();
    }

    public class ActorService : IActorService
    {
        private readonly IActorRef _clusterClient;

        public ActorService(ActorSystem actorSystem)
        {
            var receptionistAddress = Address.Parse("akka.tcp://MyCluster@localhost:4053");

            var actorPaths = new List<ActorPath>
            {
                new RootActorPath(receptionistAddress) / "system" / "receptionist"
            }.ToImmutableHashSet();

            // start ClusterClient
            _clusterClient = actorSystem.ActorOf(ClusterClient.Props(ClusterClientSettings
                .Create(actorSystem)
                .WithInitialContacts(actorPaths)), "client");
        }

        public async Task<CalculationResult> Get()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var msg = new CalculationJob(3, 5, "ADD");

            var res = await _clusterClient.Ask<CalculationResult>(
                new ClusterClient.Send("/user/distributor", msg), timeout);

            return res;
        }
    }

}