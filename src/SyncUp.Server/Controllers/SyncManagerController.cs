using Microsoft.AspNetCore.Mvc;
using SyncUp.Server.Models;
using SyncUp.Server.Services;

namespace SyncUp.Server.Controllers
{
    [ApiController]
    [Route("sync-manager")]
    public class SyncManagerController : ControllerBase
    {
        private readonly IFilesService _filesService;

        public SyncManagerController(IFilesService service)
        {
            _filesService = service;
        }

        [HttpGet("files")]
        public ActionResult<IReadOnlyList<FileEntry>> GetFiles()
        {
            var files = _filesService.GetFiles();
            return Ok(files);
        }

        [HttpGet("file/{**path}")]
        public ActionResult<FileEntry> GetFile(string path)
        {
            var file = _filesService.GetFile(path);

            if (file is null)
                return NotFound();

            return Ok(file);
        }

        [HttpPost("file")]
        public ActionResult<FileEntry> AddFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "File is empty." });

            var newFile = new FileEntry()
            {
                Path = file.FileName,
                Sha256 = "XXX"
            };

            _filesService.AddFile(newFile);

            return CreatedAtAction(nameof(GetFile), new { path = newFile.Path }, newFile);
        }
    }
}
