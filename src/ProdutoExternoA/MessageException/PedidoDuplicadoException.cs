namespace ProdutoExternoA.MessageException
{
    public class PedidoDuplicadoException : Exception
    {
        public PedidoDuplicadoException(string message) : base(message) { }
    }
}