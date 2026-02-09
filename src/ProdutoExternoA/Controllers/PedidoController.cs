using Microsoft.AspNetCore.Mvc;
using ProdutoExternoA.Domain;
using ProdutoExternoA.Infrastructure;
using ProdutoExternoA.MessageException;
using ProdutoExternoA.Service;

namespace ProdutoExternoA.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _service;
        private readonly IPedidoRepository _repository;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(IPedidoService service, IPedidoRepository repository, ILogger<PedidoController> logger)
        {
            _service = service;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoRequest request)
        {
            try
            {
                var response = await _service.ProcessarPedidoAsync(request);

                return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
            }
            catch (PedidoDuplicadoException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal ao processar o pedido {PedidoId}", request.PedidoId);
                return StatusCode(500, new { error = "Erro interno ao processar o pedido." });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(long id)
        {
            var pedido = await _repository.ObterPorIdAsync(id);

            if (pedido is null)
            {
                return NotFound(new { message = $"Pedido {id} não encontrado." });
            }

            return Ok(pedido);
        }

        [HttpGet] 
        [ProducesResponseType(typeof(IEnumerable<Pedido>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterTodos([FromQuery] string? status)
        {
            var pedidos = await _repository.ObterTodosAsync(status);
            return Ok(pedidos);
        }
    }
}