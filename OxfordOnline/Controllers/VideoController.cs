using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models;
using OxfordOnline.Services;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly VideoService _videoService;
        private readonly ILogger<VideoController> _logger;

        public VideoController(VideoService videoService, ILogger<VideoController> logger)
        {
            _videoService = videoService;
            _logger = logger;
        }

        // GET: /Video
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<VideoData>>> GetVideos()
        {
            try
            {
                var videos = await _videoService.GetActiveVideosAsync();
                return Ok(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lista de vídeos");
                return StatusCode(500, new
                {
                    message = "Erro ao buscar vídeos.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // GET: /Video/{id}
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VideoData>> GetVideoById(int id)
        {
            var video = await _videoService.GetActiveVideoByIdAsync(id);
            if (video == null)
                return NotFound(new { message = "Vídeo não encontrado." });

            return Ok(video);
        }
    }
}