using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;

namespace ksr_lab10_magazyn
{
    class Magazyn : IConsumer<IPytanieoWolne>, IConsumer<IAkceptacjaZamowienia>, IConsumer<IOdrzucenieZamowienia>
    {
        public int wolne = 0;
        public int zarezerwowane = 0;

        public Task Consume(ConsumeContext<IPytanieoWolne> ctx)
        {
            return Task.Run(() =>
            {
                int ilosc = ctx.Message.ilosc;

                if(ilosc <= wolne)
                {
                    ctx.RespondAsync(new OdpowiedzWolne() { CorrelationId = ctx.Message.CorrelationId });
                    Console.Out.WriteLineAsync($"Magazyn ma wolne produkty do zamowienia w ilosci: {ctx.Message.ilosc}");
                    wolne -= ilosc;
                    zarezerwowane += ilosc;
                }
                else
                {
                    wolne -= ilosc;
                    zarezerwowane += ilosc;
                    ctx.RespondAsync(new OdpowiedzWolneNegatywna() { CorrelationId = ctx.Message.CorrelationId });
                    Console.Out.WriteLineAsync($"Magazyn nie ma wolnych produktow do zamowienia w ilosci: {ctx.Message.ilosc}");
                }
            });
        }

        public Task Consume(ConsumeContext<IAkceptacjaZamowienia> ctx)
        {
            zarezerwowane -= ctx.Message.ilosc;
            return Console.Out.WriteLineAsync($"Sklep zaakceptowal zamowienie na ilosc: {ctx.Message.ilosc}");
        }

        public Task Consume(ConsumeContext<IOdrzucenieZamowienia> ctx)
        {
            zarezerwowane -= ctx.Message.ilosc;
            wolne += ctx.Message.ilosc;
            return Console.Out.WriteLineAsync($"Sklep odrzucil zamowienie na ilosc: {ctx.Message.ilosc}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var magazyn = new Magazyn();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("rabbitmq://sparrow.rmq.cloudamqp.com/coqhohgs"),
                h => { h.Username("coqhohgs"); h.Password("tQjPDx33wlVXxN9s3D3L2LHn6N9wVXFN"); });

                sbc.ReceiveEndpoint("sagakolejkamagazyn", ep =>
                {
                    ep.Instance(magazyn);
                });
            });

            bus.Start();
            Console.WriteLine("start magazynu");

            bool running = true;
            while (running)
            {
                ConsoleKey key = Console.ReadKey().Key;
                Console.WriteLine();

                switch (key)
                {
                    case ConsoleKey.D:
                        Console.Write("Ilosc: ");
                        int ilosc;
                        ilosc = Convert.ToInt32(Console.ReadLine());
                        magazyn.wolne += ilosc;
                        break;
                    case ConsoleKey.Q:
                            running = false;
                        break;
                    default:
                        Console.WriteLine($"Wolne zasoby magazynu: {magazyn.wolne}");
                        Console.WriteLine($"Zarezerwowane zasoby magazynu: {magazyn.zarezerwowane}");
                        break;
                }
            }

            bus.Stop();
        }
    }
}
