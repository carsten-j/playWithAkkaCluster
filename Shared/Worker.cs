using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using Akka.Logger.Serilog;
using System;

namespace Shared
{
    public class Worker : ReceiveActor, ILogReceive
    {
        private readonly Cluster _cluster = Cluster.Get(Context.System);
        private readonly ILoggingAdapter _log = Context.GetLogger<SerilogLoggingAdapter>();

        private readonly string _internalId;

        public Worker()
        {
            _internalId = Guid.NewGuid().ToString();

            Receive<CalculationJob>(x =>
            {
                _log.Info("Handled by worker with id {Id}", _internalId);
                int result;
                switch (x.Operation)
                {
                    case "ADD":
                        result = x.Number1 + x.Number2;
                        break;

                    case "SUB":
                        result = x.Number1 - x.Number2;
                        break;

                    case "MUL":
                        result = x.Number1 * x.Number2;
                        break;

                    case "DIV":
                        result = x.Number1 / x.Number2;
                        break;

                    default:
                        throw new UnknownOperationException();
                }

                _log.Info(
                    "Address [{Address}] received message [{@Message}] from sender {Sender} and returned result {Result}",
                    _cluster.SelfAddress, x, Sender, result.ToString());
                Sender.Tell(new CalculationResult(result));
            });
        }

        protected override void PreStart()
        {
            _log.Info("Worker with id {Id} in PreStart", _internalId);
            //_cluster.Subscribe(this.Self, ClusterEvent.InitialStateAsEvents, new[]
            //{
            //    typeof(ClusterEvent.IMemberEvent),
            //    typeof(ClusterEvent.UnreachableMember),
            //    typeof(ClusterEvent.MemberUp)
            //});
        }

        protected override void PostStop()
        {
            _log.Info("Worker with id {Id} in PostStop", _internalId);
            //_cluster.Unsubscribe(this.Self);
        }

        //protected override SupervisorStrategy SupervisorStrategy()
        //{
        //    return new OneForOneStrategy(Decider.From(Directive.Resume,
        //        new KeyValuePair<Type, Directive>(typeof(UnknownOperationException)
        //            , Directive.Stop)));
        //}
    }
}