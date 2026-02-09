using Microsoft.FeatureManagement;

namespace ProdutoExternoA.Service
{
    public interface ICalculadoraImpostoFactory
    {
        Task<ICalculadoraImpostoStrategy> ObterCalculoImpostoAtivoAsync();
    }

    public class CalculadoraImpostoFactory : ICalculadoraImpostoFactory
    {
        private readonly IFeatureManager _featureManager;

        public CalculadoraImpostoFactory(IFeatureManager featureManager)
        {
            _featureManager = featureManager;
        }

        public async Task<ICalculadoraImpostoStrategy> ObterCalculoImpostoAtivoAsync()
        {
            if (await _featureManager.IsEnabledAsync("TipoImposto"))
            {
                return new CalculoImpostoReforma();
            }

            return new CalculoImpostoPadrao();
        }
    }
}