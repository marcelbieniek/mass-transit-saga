using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
using Automatonymous;
using Wiadomosci;

namespace ksr_lab10_sklep
{
    public class ZamowienieDane : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string login { get; set; }
        public int ilosc { get; set; }
        public Guid? timeoutId { get; set; }
    }

    public class ZamowienieSaga : MassTransitStateMachine<ZamowienieDane>
    {
        public State Niepotwierdzone { get; private set; }
        public State PotwierdzoneMagazyn { get; private set; }
        public State PotwierdzoneKlient { get; private set; }

        public Event<StartZamowienia> StartZamowienia { get; private set; }
        public Event<Potwierdzenie> Potwierdzenie { get; private set; }
        public Event<BrakPotwierdzenia> BrakPotwierdzenia { get; private set; }
        public Event<OdpowiedzWolne> OdpowiedzWolne { get; private set; }
        public Event<OdpowiedzWolneNegatywna> OdpowiedzWolneNegatywna { get; private set; }
        public Event<Timeout> TimeoutEvt { get; private set; }
        public Schedule<ZamowienieDane, Timeout> TO { get; private set; }

        public ZamowienieSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => StartZamowienia, x => x.CorrelateBy(s => s.login,
                ctx => ctx.Message.login)
            .SelectId(context => Guid.NewGuid()));

            Schedule(() => TO,
                x => x.timeoutId,
                x => { x.Delay = TimeSpan.FromSeconds(10); });

            Initially(
                When(StartZamowienia)
                .Then(context => context.Instance.login = context.Data.login)
                .Then(context => context.Instance.ilosc = context.Data.ilosc)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[NEW ORDER] {ctx.Data.login} sklada zamowienie w ilosci: {ctx.Data.ilosc}"); })
                .Respond(ctx => { return new PytanieoWolne() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc }; })
                .Respond(ctx => { return new PytanieoPotwierdzenie() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Schedule(TO, ctx => new Timeout() { CorrelationId = ctx.Instance.CorrelationId })
                .TransitionTo(Niepotwierdzone)
                );

            During(Niepotwierdzone,
                // potwierdzenie magazynu
                When(OdpowiedzWolne)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[SUCCESS] Magazyn jest w stanie wykonac zamowienie {ctx.Instance.CorrelationId}"); })
                .TransitionTo(PotwierdzoneMagazyn),

                When(OdpowiedzWolneNegatywna)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[FAIL] Magazyn nie jest w stanie wykonac zamowienia {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize(),

                // potwierdzenie klienta
                When(Potwierdzenie)
                .Unschedule(TO)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[SUCCESS] {ctx.Instance.login} powierdzil zamowienie"); })
                .TransitionTo(PotwierdzoneKlient),

                When(BrakPotwierdzenia)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[FAIL] {ctx.Instance.login} nie potwierdzil zamowienia {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize(),

                When(TimeoutEvt)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[TIMEOUT] {ctx.Instance.login} nie potwierdzil w czasie zamowienia {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize()
                );

            During(PotwierdzoneMagazyn,
                When(Potwierdzenie)
                .Unschedule(TO)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[SUCCESS] {ctx.Instance.login} powierdzil zamowienie"); })
                .Respond(ctx => { return new AkceptacjaZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize(),

                When(BrakPotwierdzenia)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[FAIL] {ctx.Instance.login} nie potwierdzil zamowienia {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize(),

                When(TimeoutEvt)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[TIMEOUT] {ctx.Instance.login} nie potwierdzil w czasie zamowienia {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize()
                );

            During(PotwierdzoneKlient,
                When(OdpowiedzWolne)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[SUCCESS] Magazyn jest w stanie wykonac zamowienie {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new AkceptacjaZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize(),

                When(OdpowiedzWolneNegatywna)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"[FAIL] Magazyn nie jest w stanie wykonac zamowienia {ctx.Instance.CorrelationId}"); })
                .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, login = ctx.Instance.login, ilosc = ctx.Instance.ilosc }; })
                .Finalize()
                );

            SetCompletedWhenFinalized();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var repo = new InMemorySagaRepository<ZamowienieDane>();
            var machine = new ZamowienieSaga();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("rabbitmq://sparrow.rmq.cloudamqp.com/coqhohgs"),
                h => { h.Username("coqhohgs"); h.Password("tQjPDx33wlVXxN9s3D3L2LHn6N9wVXFN"); });

                sbc.ReceiveEndpoint("sagakolejka", ep =>
                {
                    ep.StateMachineSaga(machine, repo);
                });

                sbc.UseInMemoryScheduler();
            });

            bus.Start();
            Console.WriteLine("start sklepu");

            Console.ReadKey();
            bus.Stop();
        }
    }
}
