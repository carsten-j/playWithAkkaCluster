using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Event;
using Shared;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace WebAPI.Service
{
    public interface IActorService
    {
        Task<CalculationResult> Get(int number1, int number2, string operation);
    }

    public class ActorService : IActorService
    {
        private readonly IActorRef _clusterClient;
        private readonly ILoggingAdapter _log;

        public ActorService(ActorSystem actorSystem)
        {
            _log = actorSystem.Log;

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

        public async Task<CalculationResult> Get(int number1, int number2, string operation)
        {
            var timeout = TimeSpan.FromSeconds(10);
            var msg = new CalculationJob(number1, number2, operation);

            var res = await _clusterClient.Ask<CalculationResult>(
                new ClusterClient.Send("/user/distributor", msg), timeout);
            _log.Info("Result was {Res}", res);

            return res;
        }
    }
}