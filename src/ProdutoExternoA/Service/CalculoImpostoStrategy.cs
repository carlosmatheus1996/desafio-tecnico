namespace ProdutoExternoA.Service
{
    public interface ICalculadoraImpostoStrategy
    {
        decimal Calcular(decimal valorBase);
    }

    public class CalculoImpostoPadrao : ICalculadoraImpostoStrategy
    {
        public decimal Calcular(decimal valorBase) => valorBase * 0.30m;
    }

    public class CalculoImpostoReforma : ICalculadoraImpostoStrategy
    {
        public decimal Calcular(decimal valorBase) => valorBase * 0.20m;
    }
}
