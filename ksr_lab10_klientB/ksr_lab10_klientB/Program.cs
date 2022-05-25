using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;

namespace ksr_lab10_klientB
{
    static class Klient
    {
        public const string login = "klient B";
    }

    class MessageHandler : IConsumer<IPytanieoPotwierdzenie>, IConsumer<IAkceptacjaZamowienia>, IConsumer<IOdrzucenieZamowienia>
    {
        public Task Consume(ConsumeContext<IPytanieoPotwierdzenie> ctx)
        {
            if (ctx.Message.login == Klient.login)
            {
                Console.WriteLine($"Czy akceptujesz zamowienie na ilosc: {ctx.Message.ilosc}? [Y/N]");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    return Task.Run(() =>
                    {
                        Console.WriteLine("Wyslano potwierdzenie");
                        ctx.RespondAsync(new Potwierdzenie() { CorrelationId = ctx.Message.CorrelationId });
                    });
                }

                return Task.Run(() =>
                {
                    Console.WriteLine("Wyslano brak potwierdzenia");
                    ctx.RespondAsync(new BrakPotwierdzenia() { CorrelationId = ctx.Message.CorrelationId });
                });
            }
            return Task.Run(() => { });
        }

        public Task Consume(ConsumeContext<IAkceptacjaZamowienia> ctx)
        {
            if (ctx.Message.login == Klient.login)
            {
                return Console.Out.WriteLineAsync($"Sklep zaakceptowal zamowienie na ilosc: {ctx.Message.ilosc}");
            }
            return Task.Run(() => { });
        }

        public Task Consume(ConsumeContext<IOdrzucenieZamowienia> ctx)
        {
            if (ctx.Message.login == Klient.login)
            {
                while (Console.KeyAvailable)
                    Console.ReadKey(false);
                return Console.Out.WriteLineAsync($"Sklep odrzucil zamowienie na ilosc: {ctx.Message.ilosc}");
            }
            return Task.Run(() => { });
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new MessageHandler();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("rabbitmq://sparrow.rmq.cloudamqp.com/coqhohgs"),
                h => { h.Username("coqhohgs"); h.Password("tQjPDx33wlVXxN9s3D3L2LHn6N9wVXFN"); });

                sbc.ReceiveEndpoint("sagakolejkaklientB", ep =>
                {
                    ep.Instance(handler);
                });
            });

            bus.Start();
            Console.WriteLine("start klienta B");

            bool running = true;
            while (running)
            {
                ConsoleKey key = Console.ReadKey().Key;
                Console.WriteLine();

                if (key == ConsoleKey.S)
                {
                    Console.Write("Ilosc: ");
                    int ilosc;
                    ilosc = Convert.ToInt32(Console.ReadLine());
                    bus.Publish(new StartZamowienia() { login = Klient.login, ilosc = ilosc });
                    Console.WriteLine($"Wyslanie zamowienia na ilosc: {ilosc}");
                }
                else if (key == ConsoleKey.Q)
                {
                    running = false;
                }
            }

            bus.Stop();
        }
    }
}
