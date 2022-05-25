using System;
using MassTransit;

namespace Wiadomosci
{
    public interface IStartZamowienia
    {
        string login { get; set; }
        int ilosc { get; set; }
    }

    public class StartZamowienia : IStartZamowienia
    {
        public string login { get; set; }
        public int ilosc { get; set; }
    }

    public interface IPytanieoPotwierdzenie : CorrelatedBy<Guid>
    {
        string login { get; set; }
        int ilosc { get; set; }
    }

    public class PytanieoPotwierdzenie : IPytanieoPotwierdzenie
    {
        public Guid CorrelationId { get; set; }
        public string login { get; set; }
        public int ilosc { get; set; }
    }

    public interface IPotwierdzenie : CorrelatedBy<Guid>
    {

    }

    public class Potwierdzenie : IPotwierdzenie
    {
        public Guid CorrelationId { get; set; }
    }

    public interface IBrakPotwierdzenia : CorrelatedBy<Guid>
    {
        
    }

    public class BrakPotwierdzenia : IBrakPotwierdzenia
    {
        public Guid CorrelationId { get; set; }
    }

    public interface IPytanieoWolne : CorrelatedBy<Guid>
    {
        int ilosc { get; set; }
    }

    public class PytanieoWolne : IPytanieoWolne
    {
        public Guid CorrelationId { get; set; }
        public int ilosc { get; set; }
    }

    public interface IOdpowiedzWolne : CorrelatedBy<Guid>
    {
        
    }

    public class OdpowiedzWolne : IOdpowiedzWolne
    {
        public Guid CorrelationId { get; set; }
    }

    public interface IOdpowiedzWolneNegatywna : CorrelatedBy<Guid>
    {
        
    }

    public class OdpowiedzWolneNegatywna : IOdpowiedzWolneNegatywna
    {
        public Guid CorrelationId { get; set; }
    }

    public interface IAkceptacjaZamowienia : CorrelatedBy<Guid>
    {
        string login { get; set; }
        int ilosc { get; set; }
    }

    public class AkceptacjaZamowienia : IAkceptacjaZamowienia
    {
        public Guid CorrelationId { get; set; }
        public string login { get; set; }
        public int ilosc { get; set; }
    }

    public interface IOdrzucenieZamowienia : CorrelatedBy<Guid>
    {
        string login { get; set; }
        int ilosc { get; set; }
    }

    public class OdrzucenieZamowienia : IOdrzucenieZamowienia
    {
        public Guid CorrelationId { get; set; }
        public string login { get; set; }
        public int ilosc { get; set; }
    }

    public interface ITimeout : CorrelatedBy<Guid>
    {

    }

    public class Timeout : ITimeout
    {
        public Guid CorrelationId { get; set; }
    }
}