using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Resources;


namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class PalletLoadController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PalletLoadController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar (Upsert) uma lista de PalletLoadHead
        /// <summary>
        /// Insere novos registros ou atualiza registros existentes de cabeçalho de carga.
        /// </summary>
        /// 
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertLoadHeads([FromBody] List<PalletLoadHead> loadHeads)
        {
            if (loadHeads == null || !loadHeads.Any())
            {
                return BadRequest(EndPointsMessages.NotFound);
            }

            // 1. Lista para armazenar os IDs das cargas após o salvamento
            var savedLoadIds = new List<int>();

            try
            {
                foreach (var loadHead in loadHeads)
                {
                    // load_id = 0 indica um novo registro (INSERT)
                    if (loadHead.LoadId == 0)
                    {
                        _context.PalletLoadHead.Add(loadHead);
                        // NOTA: O LoadId SÓ será preenchido após o SaveChangesAsync()
                    }
                    else
                    {
                        // load_id > 0 indica que pode ser um registro existente (UPDATE)
                        var existingHead = await _context.PalletLoadHead.FindAsync(loadHead.LoadId);

                        if (existingHead != null)
                        {
                            // Atualiza as propriedades manualmente
                            existingHead.Name = loadHead.Name;
                            existingHead.Description = loadHead.Description;
                            existingHead.Status = loadHead.Status;
                            existingHead.Date = loadHead.Date;
                            existingHead.Time = loadHead.Time;

                            // O Change Tracker do EF Core marcará 'existingHead' como Modificado.
                            // Adiciona o ID existente à lista para retorno.
                            savedLoadIds.Add(existingHead.LoadId);
                        }
                        // Se não for encontrado, o ID não será adicionado à lista.
                    }
                }

                // 2. Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();

                // 3. Após o SaveChangesAsync(), o EF Core preenche o LoadId para os novos registros (LoadId == 0)
                // Percorrer a lista original para capturar todos os IDs (novos e atualizados)
                foreach (var loadHead in loadHeads)
                {
                    // O LoadId agora terá o valor gerado pelo banco para os inserts,
                    // ou o valor original para os updates que foram adicionados acima.
                    if (!savedLoadIds.Contains(loadHead.LoadId))
                    {
                        savedLoadIds.Add(loadHead.LoadId);
                    }
                }

                // 4. Retorna a lista de IDs salvos com status 200 OK
                return Ok(new
                {
                    message = EndPointsMessages.SucessSave,
                    loadIds = savedLoadIds // Retorna a lista de IDs
                });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }
        /*public async Task<IActionResult> UpsertLoadHeads([FromBody] List<PalletLoadHead> loadHeads)
        {
            if (loadHeads == null || !loadHeads.Any())
            {
                return BadRequest(EndPointsMessages.NotFound);
            }

            try
            {
                foreach (var loadHead in loadHeads)
                {
                    // load_id = 0 indica um novo registro (INSERT)
                    if (loadHead.LoadId == 0)
                    {
                        _context.PalletLoadHead.Add(loadHead);
                    }
                    else
                    {
                        // load_id > 0 indica que pode ser um registro existente (UPDATE)
                        var existingHead = await _context.PalletLoadHead.FindAsync(loadHead.LoadId);

                        if (existingHead != null)
                        {
                            // Atualiza as propriedades manualmente
                            existingHead.Name = loadHead.Name;
                            existingHead.Description = loadHead.Description;
                            existingHead.Status = loadHead.Status;
                            existingHead.Date = loadHead.Date;
                            existingHead.Time = loadHead.Time;
                            // O LoadId não precisa ser atualizado, pois é a PK
                        }
                        // Se não for encontrado, ignora a atualização (ou insere, dependendo da regra de negócio)
                        // Neste caso, se o ID for fornecido e não existir, ele é ignorado.
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = EndPointsMessages.SucessSave });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }*/

        // GET: Todos os cabeçalhos de carga
        /// <summary>
        /// Obtém todos os registros de cabeçalho de carga.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PalletLoadHead>>> GetAllLoadHeads()
        {
            var heads = await _context.PalletLoadHead.ToListAsync();
            return Ok(heads);
        }

        // GET: Cabeçalho de carga por ID
        /// <summary>
        /// Obtém um registro de cabeçalho de carga pelo LoadId.
        /// </summary>
        [Authorize]
        [HttpGet("{loadId}")]
        public async Task<ActionResult<PalletLoadHead>> GetLoadHead(int loadId)
        {
            var head = await _context.PalletLoadHead.FindAsync(loadId);

            if (head == null)
            {
                return NotFound(EndPointsMessages.NotFound);
            }

            return Ok(head);
        }

        // DELETE: Excluir um cabeçalho de carga
        /// <summary>
        /// Exclui um registro de cabeçalho de carga pelo LoadId.
        /// </summary>
        [Authorize]
        [HttpDelete("{loadId}")]
        public async Task<IActionResult> DeleteLoadHead(int loadId)
        {
            try
            {
                var headToDelete = await _context.PalletLoadHead.FindAsync(loadId);

                if (headToDelete == null)
                {
                    return NotFound(EndPointsMessages.NotFound);
                }

                _context.PalletLoadHead.Remove(headToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { message = EndPointsMessages.DeleteOk });
            }
            catch (Exception ex)
            {
                var baseError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", baseError));
            }
        }

        // GET: PalletLoadLine por LoadId e PalletId com detalhes do Pallet
        /// <summary>
        /// Obtém um registro de linha de carga pelo LoadId e PalletId, incluindo detalhes do Pallet.
        /// </summary>
        [Authorize]
        [HttpGet("Pallets/{loadId}")]
        public async Task<ActionResult<List<Models.Dto.PalletLoadLine>>> GetLoadLinesByLoadId(int loadId)
        {
            var linesDto = await _context.PalletLoadLine
                // 1. Usa .Include() para garantir que o Pallet seja carregado (JOIN implícito)
                .Include(pll => pll.Pallet)

                // 2. Filtra as linhas de carga pelo LoadId
                .Where(pll => pll.LoadId == loadId)

                // 3. Projeta diretamente o resultado no seu DTO
                // O Select é traduzido em uma cláusula SELECT no SQL, tornando-o eficiente (apenas os campos necessários são retornados).
                .Select(pll => new Models.Dto.PalletLoadLine
                {
                    LoadId = pll.LoadId,
                    PalletId = pll.PalletId,
                    Carregado = pll.Carregado,

                    // Usamos o operador condicional nulo (?.) para evitar NullReferenceException
                    // caso haja uma PalletLoadLine sem um Pallet associado.
                    PalletLocation = pll.Pallet!.Location,
                    PalletTotalQuantity = pll.Pallet!.TotalQuantity
                })
                .ToListAsync();

            if (!linesDto.Any()) // Checa se a lista está vazia
            {
                return NotFound(EndPointsMessages.NotFound);
            }

            return Ok(linesDto);
        }


        // =================================== POST PALLET LINE ===================================
        // Rota: POST v1/PalletLoad/Pallet
        /// <summary>
        /// Adiciona um único palete (linha) a uma carga existente.
        /// </summary>
        /// <remarks>
        /// O corpo da requisição deve ser um objeto PalletLoadLineAdd (LoadId, PalletId).
        /// </remarks>
        [Authorize]
        [HttpPost("Pallets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPalletToLoad([FromBody] Models.Dto.PalletLoadLineAdd palletLoadRequest)
        {
            if (palletLoadRequest == null)
                return BadRequest();

            try
            {
                var loadExists = await _context.PalletLoadHead.AnyAsync(h => h.LoadId == palletLoadRequest.LoadId);
                var palletExists = await _context.Pallet.AnyAsync(p => p.PalletId == palletLoadRequest.PalletId);

                if (!loadExists || !palletExists)
                    return NotFound();

                var existingPalletLine = await _context.PalletLoadLine
                    .FirstOrDefaultAsync(pll => pll.LoadId == palletLoadRequest.LoadId
                                             && pll.PalletId == palletLoadRequest.PalletId);

                var carregadoValue = palletLoadRequest.Carregado ?? false;
                if (existingPalletLine == null) // Não existe → cria nova linha
                {
                    var palletLineEntity = new Models.PalletLoadLine
                    {
                        LoadId = palletLoadRequest.LoadId,
                        PalletId = palletLoadRequest.PalletId,
                        Carregado = carregadoValue
                    };

                    _context.PalletLoadLine.Add(palletLineEntity);
                }
                else // Existe → atualiza Carregado somente se o valor vier no JSON
                {
                    if (palletLoadRequest.Carregado.HasValue)
                    {
                        existingPalletLine.Carregado = carregadoValue;
                    }
                }

                await _context.SaveChangesAsync();

                bool cargaCompleta = await IsLoadFullCarregado(palletLoadRequest.LoadId);

                // Retorna sucesso e status da carga
                var response = new
                {
                    Success = true,
                    AllPalletsLoaded = cargaCompleta
                };

                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<bool> IsLoadFullCarregado(int loadId)
        {
            return await _context.PalletLoadLine
                .Where(pll => pll.LoadId == loadId)
                .AllAsync(pll => pll.Carregado);
        }

        [Authorize]
        [HttpPut("UpdateLoadStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLoadStatus(int loadId, string status)
        {
            if (loadId <= 0 || string.IsNullOrWhiteSpace(status))
                return BadRequest("Parâmetros inválidos.");

            try
            {
                var load = await _context.PalletLoadHead
                    .FirstOrDefaultAsync(h => h.LoadId == loadId);

                if (load == null)
                    return NotFound($"Nenhum registro encontrado para LoadId {loadId}");

                load.Status = status;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Status da carga {loadId} atualizado para '{status}'."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao atualizar status da carga: {ex.Message}");
            }
        }

        // DELETE: Excluir uma linha de carga (PalletLoadLine)
        /// <summary>
        /// Exclui um pallet de uma carga (registro na PalletLoadLine).
        /// </summary>
        [Authorize]
        [HttpDelete("Pallet/{loadId}/{palletId}")] // Exemplo de rota: v1/PalletLoad/Pallets/123/456
        public async Task<IActionResult> DeleteLoadLine(int loadId, int palletId)
        {
            var lineToDelete = await _context.PalletLoadLine
                .FirstOrDefaultAsync(pll => pll.LoadId == loadId && pll.PalletId == palletId);

            if (lineToDelete == null)
            {
                return NotFound(EndPointsMessages.NotFound);
            }

            _context.PalletLoadLine.Remove(lineToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Palete da carga excluída com sucesso." });
        }

        // GET: Itens de um Pallet específico
        /// <summary>
        /// Retorna todos os itens pertencentes a um pallet, com informações de produto, quantidade e status.
        /// </summary>
        // ========================== CÓDIGO C# AJUSTADO ==========================

        // GET: Itens de um Pallet específico em uma Carga
        /// <summary>
        /// Retorna todos os itens pertencentes a um pallet dentro de uma carga específica,
        /// validando a associação via PalletLoadLine.
        /// </summary>
        [Authorize]
        [HttpGet("PalletItems/{loadId}/{palletId}")] // Rota ajustada
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPalletItems(int loadId, int palletId)
        {
            try
            {
                // 1. Verifica se a ASSOCIAÇÃO Pallet-Carga existe na tabela PalletLoadLine.
                // O DbSet correto é PalletLoadLine.
                var palletLine = await _context.PalletLoadLine // Usando PalletLoadLine
                    .FirstOrDefaultAsync(p => p.PalletId == palletId && p.LoadId == loadId);

                if (palletLine == null)
                    return NotFound($"Nenhum pallet encontrado com ID {palletId} associado à carga {loadId}.");

                // Assumindo que o PalletDetails (dados do Pallet) estão em outra tabela
                // Se a tabela PalletLoadLine contém apenas a associação, buscamos os detalhes do pallet na tabela Pallet original
                // *** IMPORTANTE: Se PalletLoadLine contém os detalhes de status/location, use-o diretamente.
                // Se PalletLoadLine for apenas a associação, você precisará carregar os detalhes do pallet.

                // Opção A: Buscar detalhes (Status, Location, TotalQuantity) na tabela Pallet (assumindo que Pallet é uma tabela real de detalhes)
                var pallet = await _context.Pallet
                    .FirstOrDefaultAsync(p => p.PalletId == palletId);

                if (pallet == null)
                    return NotFound($"Detalhes do pallet {palletId} não encontrados."); // PalletLine existe, mas o Pallet não.

                // 2. Busca os itens do pallet (JOIN com Product)
                var items = await _context.PalletItem
                    .Include(pi => pi.Product)
                    .Where(pi => pi.PalletId == palletId)
                    .Select(pi => new
                    {
                        pi.PalletId,
                        pi.ProductId,
                        ProductNumber = pi.Product!.ProductId,
                        ProductDescription = pi.Product!.ProductName,
                        pi.Quantity,
                        pi.QuantityReceived,
                        pi.Status,
                        pi.UserId
                    })
                    .ToListAsync();

                if (!items.Any())
                    return NotFound($"Nenhum item encontrado para o pallet {palletId}.");

                // 3. Monta o retorno com informações básicas do pallet
                var response = new
                {
                    PalletId = pallet.PalletId,
                    pallet.Status,
                    pallet.Location,
                    pallet.TotalQuantity,
                    Items = items
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Erro interno ao buscar itens do pallet: " + errorMessage);
            }
        }

        // PUT: Atualizar itens de um Pallet
        /// <summary>
        /// Atualiza as quantidades recebidas e o status dos itens em um pallet.
        /// </summary>
        /// <remarks>
        /// A lista deve conter o PalletId e ProductId para identificar o item, e a nova QuantityReceived.
        /// </remarks>
        [Authorize]
        [HttpPut("ReceiveItems")] // Rota: v1/PalletLoad/ReceiveItems
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePalletItems([FromBody] List<PalletLoadItem> itemUpdates)
        {
            if (itemUpdates == null || !itemUpdates.Any())
            {
                return BadRequest("Nenhum item para atualizar fornecido.");
            }

            // Usamos um HashSet para rastrear quais PalletIds foram processados
            var updatedPalletIds = new HashSet<int>();

            try
            {
                foreach (var update in itemUpdates)
                {
                    var existingItem = await _context.PalletItem
                        .FirstOrDefaultAsync(pi => pi.PalletId == update.PalletId && pi.ProductId == update.ProductId);

                    if (existingItem != null)
                    {
                        // 1. Atualiza a quantidade recebida
                        existingItem.QuantityReceived = update.QuantityReceived;
                        existingItem.Status = "R";

                        // 3. Opcional: Registra o usuário que realizou a operação
                        if (!string.IsNullOrWhiteSpace(update.UserId))
                        {
                            existingItem.UserId = update.UserId;
                        }

                        // Adiciona o PalletId para possível atualização posterior do Pallet/Load
                        updatedPalletIds.Add(update.PalletId);
                    }
                    // Se o item não existir, podemos optar por ignorar ou retornar um erro. Aqui, optamos por ignorar para permitir atualizações parciais de uma lista.
                }

                await _context.SaveChangesAsync();

                // 4. Lógica pós-salvamento: Atualizar Pallet/Load Status (Opcional, mas recomendado)
                await UpdatePalletAndLoadStatus(updatedPalletIds);

                return Ok(new { message = $"Itens de {updatedPalletIds.Count} palete(s) atualizados com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        // MÉTODO AUXILIAR
        private async Task UpdatePalletAndLoadStatus(HashSet<int> palletIds)
        {
            bool hasChanges = false;
            var updatedLoadIds = new HashSet<int>();

            foreach (var palletId in palletIds)
            {
                // 1. Checa se TODOS os PalletItems estão 'R'
                bool allItemsComplete = await _context.PalletItem
                    .Where(pi => pi.PalletId == palletId)
                    .AllAsync(pi => pi.Status == "R");

                var pallet = await _context.Pallet.FindAsync(palletId);
                if (pallet != null)
                {
                    if (allItemsComplete)
                    {
                        pallet.Status = "R";
                        hasChanges = true;
                    }

                    // 2. Verifica se o pallet está "R" e marca pallet_load_line.received = 1
                    var lines = await _context.PalletLoadLine
                        .Where(l => l.PalletId == palletId)
                        .ToListAsync();

                    foreach (var line in lines)
                    {
                        if (pallet.Status == "R" && line.Carregado)
                        {
                            line.Received = true;
                            hasChanges = true;
                            updatedLoadIds.Add(line.LoadId);
                        }
                    }
                }
            }

            if (hasChanges)
                await _context.SaveChangesAsync();

            // 3. Verifica se todos os pallets de cada Load estão received = 1
            foreach (var loadId in updatedLoadIds)
            {
                bool allReceived = await _context.PalletLoadLine
                    .Where(l => l.LoadId == loadId)
                    .AllAsync(l => l.Received);

                if (allReceived)
                {
                    var loadHead = await _context.PalletLoadHead.FindAsync(loadId);
                    if (loadHead != null && loadHead.Status != "Recebido")
                    {
                        loadHead.Status = "Recebido";
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        // POST: Incluir um novo PalletInvoice
        /// <summary>
        /// Inclui um novo registro na tabela pallet_invoice.
        /// </summary>
        /// <param name="palletInvoice">O objeto PalletInvoice a ser inserido.</param>
        [Authorize]
        [HttpPost("Invoice")] // Rota: v1/PalletLoad/Invoice
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPalletInvoice([FromBody] PalletInvoice palletInvoice)
        {
            if (palletInvoice == null || string.IsNullOrWhiteSpace(palletInvoice.Invoice) || palletInvoice.LoadId <= 0)
            {
                return BadRequest("Dados da fatura (PalletInvoice) inválidos. Certifique-se de fornecer Invoice e LoadId.");
            }

            try
            {
                var existingInvoice = await _context.PalletInvoice
                    .FirstOrDefaultAsync(pi => pi.LoadId == palletInvoice.LoadId && pi.Invoice == palletInvoice.Invoice);

                if (existingInvoice != null)
                {
                    return Ok(new
                    {
                        message = "Registro de fatura já existe.",
                        loadId = palletInvoice.LoadId,
                        invoice = palletInvoice.Invoice
                    });
                }

                _context.PalletInvoice.Add(palletInvoice);
                await _context.SaveChangesAsync();

                // Retorno 201 Created ajustado para a chave composta (LoadId e Invoice)
                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = EndPointsMessages.SucessSave,
                    loadId = palletInvoice.LoadId,
                    invoice = palletInvoice.Invoice
                });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro ao salvar fatura (PalletInvoice).";
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        // DELETE: Excluir um PalletInvoice
        /// <summary>
        /// Exclui um registro da tabela pallet_invoice pela chave primária composta (LoadId e Invoice).
        /// </summary>
        /// <param name="loadId">O ID do Load que faz parte da PK.</param>
        /// <param name="invoice">O valor da coluna Invoice que faz parte da PK.</param>
        [Authorize]
        [HttpDelete("Invoice/{loadId}/{invoice}")] // Rota: v1/PalletLoad/Invoice/{loadId}/{invoice}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePalletInvoice(int loadId, string invoice)
        {
            if (loadId <= 0 || string.IsNullOrWhiteSpace(invoice))
            {
                return BadRequest("Os parâmetros 'loadId' e 'invoice' devem ser fornecidos e válidos.");
            }

            try
            {
                // Busca pelo registro usando a chave composta
                var invoiceToDelete = await _context.PalletInvoice
                    .FirstOrDefaultAsync(pi => pi.LoadId == loadId && pi.Invoice == invoice);

                if (invoiceToDelete == null)
                {
                    return NotFound(EndPointsMessages.NotFound);
                }

                _context.PalletInvoice.Remove(invoiceToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Registro de fatura '{invoice}' para LoadId '{loadId}' excluído com sucesso." });
            }
            catch (Exception ex)
            {
                var baseError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", baseError));
            }
        }

        // GET: Lista de PalletInvoice por LoadId
        /// <summary>
        /// Obtém a lista de todas as notas fiscais (PalletInvoice) associadas a um LoadId específico.
        /// Retorna uma lista vazia com status 200 OK se não houver faturas.
        /// </summary>
        /// <param name="loadId">O ID do Load a ser consultado.</param>
        [Authorize]
        [HttpGet("Invoices/Load/{loadId}")] // Rota: v1/PalletLoad/Invoices/Load/{loadId}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetInvoicesByLoadId(int loadId)
        {
            if (loadId <= 0)
            {
                return BadRequest("O LoadId deve ser um valor positivo.");
            }

            try
            {
                var invoiceNumbers = await _context.PalletInvoice
                    .Where(pi => pi.LoadId == loadId)
                    .Select(pi => pi.Invoice)
                    .ToListAsync();

                return Ok(invoiceNumbers);
            }
            catch (Exception ex)
            {
                var baseError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno ao buscar notas fiscais: {baseError}");
            }
        }

    }
}
