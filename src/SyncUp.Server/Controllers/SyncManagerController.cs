using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SyncUp.Server.Services;

namespace SyncUp.Server.Controllers
{
    [ApiController]
    [Route("sync-manager")]
    public class SyncManagerController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly ILogger<SyncManagerController> _logger;

        public SyncManagerController(IFilesService service, ILogger<SyncManagerController> logger)
        {
            _filesService = service;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult GetStatus()
        {
            return Ok(
                new
                {
                    Status = "Alive",
                    TimeStamp = DateTime.Now,
                    Version = "1.0.0"
                }
            );
        }

        [HttpGet("files")]
        public ActionResult<List<string>> GetFiles()
        {
            try
            {
                var files = _filesService.GetAll();
                return Ok(files);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file master list.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve files at this time.");
            }
        }
    }
}
