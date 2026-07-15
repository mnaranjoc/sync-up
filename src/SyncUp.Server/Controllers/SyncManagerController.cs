using Microsoft.AspNetCore.Mvc;
using SyncUp.Server.Services;
using SyncUp.Shared.Models;

namespace SyncUp.Server.Controllers
{
    [ApiController]
    [Route("sync-manager")]
    public class SyncManagerController : ControllerBase
    {
        private readonly IServerFilesService _serverFilesService;
        private readonly bool _allowEmptyFiles;

        public SyncManagerController(IServerFilesService serverFilesService, IConfiguration configuration)
        {
            _serverFilesService = serverFilesService;
            _allowEmptyFiles = configuration.GetValue<bool>("AllowEmptyFiles");
        }

        [HttpGet("files")]
        public ActionResult<IReadOnlyList<FileEntry>> GetFiles()
        {
            var files = _serverFilesService.GetFiles();
            return Ok(files);
        }

        [HttpGet("file/{**path}")]
        public ActionResult<FileEntry> GetFile(string path)
        {
            var file = _serverFilesService.GetFile(path);

            if (file is null)
                return NotFound();

            return Ok(file);
        }

        [HttpPost("file")]
        public ActionResult<FileEntry> AddFile([FromForm] IFormFile file)
        {
            if (file == null || (!_allowEmptyFiles && file.Length == 0))
                return BadRequest(new { error = "File is empty." });

            var newFile = _serverFilesService.AddFile(file);

            return CreatedAtAction(nameof(GetFile), new { path = newFile?.Path }, newFile);
        }
    }
}
