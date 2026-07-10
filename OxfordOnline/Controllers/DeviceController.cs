using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ITvDeviceRepository _tvDeviceRepository;

        public DeviceController(ITvDeviceRepository tvDeviceRepository)
        {
            _tvDeviceRepository = tvDeviceRepository;
        }

        // GET: Todos os registros de tv_device
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TvDevice>>> GetAllTvDevices()
        {
            var tvDevices = await _tvDeviceRepository.GetAllAsync();
            return Ok(tvDevices);
        }

        // GET: tv_device por DeviceId
        [Authorize]
        [HttpGet("{deviceId}")]
        public async Task<ActionResult<TvDevice>> GetTvDeviceByDeviceId(int deviceId)
        {
            var tvDevice = await _tvDeviceRepository.GetByDeviceIdAsync(deviceId);
            if (tvDevice == null)
                return NotFound("Nenhum registro de TV encontrado para o device informado.");

            return Ok(tvDevice);
        }

        // GET: tv_device por Setor
        [Authorize]
        [HttpGet("Setor/{setor}")]
        public async Task<ActionResult<IEnumerable<TvDevice>>> GetTvDevicesBySetor(string setor)
        {
            var tvDevices = await _tvDeviceRepository.GetBySetorAsync(setor);
            if (tvDevices == null || !tvDevices.Any())
                return NotFound("Nenhum device encontrado para o setor informado.");

            return Ok(tvDevices);
        }

        // POST: Vincula um device a um setor (cria o registro em tv_device)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTvDevice([FromBody] TvDevice tvDevice)
        {
            if (tvDevice == null || tvDevice.DeviceId <= 0 || string.IsNullOrWhiteSpace(tvDevice.Setor))
                return BadRequest("DeviceId e Setor são obrigatórios.");

            var deviceExists = await _tvDeviceRepository.DeviceExistsAsync(tvDevice.DeviceId);
            if (!deviceExists)
                return NotFound($"Device com Id {tvDevice.DeviceId} não encontrado.");

            var existing = await _tvDeviceRepository.GetByDeviceIdAsync(tvDevice.DeviceId);
            if (existing != null)
                return Conflict($"O device {tvDevice.DeviceId} já possui um setor configurado. Use PUT para atualizar.");

            try
            {
                var created = await _tvDeviceRepository.AddAsync(tvDevice);
                return CreatedAtAction(nameof(GetTvDeviceByDeviceId), new { deviceId = created.DeviceId }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar registro: {ex.Message}");
            }
        }

        // PUT: Atualiza o setor de um device já vinculado
        [Authorize]
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> UpdateTvDevice(int deviceId, [FromBody] TvDevice tvDevice)
        {
            if (string.IsNullOrWhiteSpace(tvDevice.Setor))
                return BadRequest("Setor é obrigatório.");

            try
            {
                var updated = await _tvDeviceRepository.UpdateAsync(deviceId, tvDevice.Setor);
                if (updated == null)
                    return NotFound("Nenhum registro de TV encontrado para o device informado.");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar registro: {ex.Message}");
            }
        }

        // DELETE: Remove o vínculo de um device com o setor
        [Authorize]
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteTvDevice(int deviceId)
        {
            var deleted = await _tvDeviceRepository.DeleteAsync(deviceId);
            if (!deleted)
                return NotFound("Nenhum registro de TV encontrado para o device informado.");

            return Ok(new { message = "Registro removido com sucesso." });
        }

        [Authorize]
        [HttpPost("TvDevice/{deviceId}")]
        public async Task<IActionResult> UpdateTvDevice(
            int deviceId,
            [FromBody] TvDeviceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Setor))
                return BadRequest("Setor é obrigatório.");

            var tvDevice = await _tvDeviceRepository.GetByDeviceIdAsync(deviceId);

            if (tvDevice == null)
                return NotFound("Nenhum registro de TV encontrado para o device informado.");

            tvDevice.Setor = request.Setor;
            tvDevice.TransCode = request.TransCode;

            await _tvDeviceRepository.UpdateAsync(tvDevice);

            return Ok(tvDevice);
        }

        [Authorize]
        [HttpGet("TvDevice/{deviceId}")]
        public async Task<IActionResult> GetTvDevice(int deviceId)
        {
            var tvDevice = await _tvDeviceRepository.GetByDeviceIdAsync(deviceId);

            if (tvDevice == null)
                return NotFound("Nenhum registro de TV encontrado para o device informado.");

            return Ok(new TvDeviceResponse
            {
                DeviceId = tvDevice.DeviceId,
                Setor = tvDevice.Setor,
                TransCode = tvDevice.TransCode
            });
        }

        [Authorize]
        [HttpGet("TvDevice")]
        public async Task<IActionResult> GetTvDeviceByGuidAndUser([FromQuery] Guid guid, [FromQuery] string user)
        {
            if (guid == Guid.Empty)
                return BadRequest("Guid é obrigatório.");

            if (string.IsNullOrWhiteSpace(user))
                return BadRequest("Usuário é obrigatório.");

            var tvDevice = await _tvDeviceRepository.GetByGuidAndUserAsync(guid, user);


            if (tvDevice == null)
                return NotFound("TV Device não encontrado.");

            return Ok(new TvDeviceResponse
            {   
                DeviceId    = tvDevice.DeviceId,
                Setor       = tvDevice.Setor,
                TransCode   = tvDevice.TransCode
            });
        }

        [Authorize]
        [HttpGet("TvDeviceIMG")]
        public async Task<IActionResult> GetByGuidAndUserIMGAsync(
            [FromQuery] Guid guid,
            [FromQuery] string user)
        {
            if (guid == Guid.Empty)
                return BadRequest("Guid é obrigatório.");

            if (string.IsNullOrWhiteSpace(user))
                return BadRequest("Usuário é obrigatório.");

            var images = await _tvDeviceRepository.GetByGuidAndUserIMGAsync(guid, user);

            if (images == null || !images.Any())
                return NotFound("Nenhuma imagem encontrada para o TV Device.");

            return Ok(images);
        }

    }
}