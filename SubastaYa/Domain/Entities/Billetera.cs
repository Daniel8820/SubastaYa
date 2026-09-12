using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Domain.Entities
{
    public class Billetera
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        // Ponemos los setters como private para que nadie de afuera los modifique directamente
        public decimal SaldoTotal { get; private set; }
        public decimal SaldoRetenido { get; private set; }
        public decimal SaldoDisponible { get; private set; }
        public int Version { get; set; }

        public Usuario Usuario { get; set; }

        // Comportamiento del Dominio:
        public void Acreditar(decimal monto)
        {
            if (monto <= 0) throw new DomainException("El monto debe ser mayor a cero.");
            SaldoTotal += monto;
            SaldoDisponible += monto;
        }

        public void RetenerFondos(decimal monto)
        {
            if (SaldoDisponible < monto) throw new DomainException("Saldo insuficiente.");
            SaldoDisponible -= monto;
            SaldoRetenido += monto;
        }

        public void LiberarGarantia(decimal monto)
        {
            if (SaldoRetenido < monto) throw new DomainException("No hay suficientes fondos retenidos para liberar.");
            SaldoRetenido -= monto;
            SaldoDisponible += monto;
        }

        public void DescontarPago(decimal monto)
        {
            if (SaldoRetenido < monto) throw new DomainException("No hay suficientes fondos retenidos para pagar.");
            SaldoTotal -= monto;
            SaldoRetenido -= monto;
        }
    }
}