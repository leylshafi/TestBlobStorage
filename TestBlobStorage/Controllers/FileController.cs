using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TestBlobStorage.Services;

namespace TestBlobStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IStorageManager _storageManager;

        public FileController(IStorageManager storageManager)
        {
            _storageManager = storageManager;
        }

        [HttpGet("getUrl")]
        public IActionResult GetUrl(string fileName)
        {
            try
            {
                var result = _storageManager.GetSignedUrl(fileName);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }

        [HttpGet("getUrlAsync")]
        public async Task<IActionResult> GetUrlAsync(string fileName)
        {
            try
            {
                var result = await _storageManager.GetSignedUrlAsync(fileName);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }


        [HttpDelete("Delete")]
        public IActionResult DeleteFile(string fileName)
        {
            try
            {
                var result = _storageManager.DeleteFile(fileName);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }

        [HttpDelete("DeleteAsync")]
        public async Task<IActionResult> DeleteFileAsync(string fileName)
        {
            try
            {
                var result = await  _storageManager.DeleteFileAsync(fileName);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }


        [HttpPost("uploadAsync")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var fileName = Guid.NewGuid().ToString(); 
                    var contentType = file.ContentType;

                    var result = await _storageManager.UploadFileAsync(stream, fileName, contentType);

                    if (result)
                    {
                        return Ok("File uploaded successfully.");
                    }
                    else
                    {
                        return BadRequest("File upload failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("upload")]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var contentType = file.ContentType;
                    var originalFileName = file.FileName; 
                    var fileExtension = Path.GetExtension(originalFileName); 
                    var fileName = Guid.NewGuid().ToString() + fileExtension;

                    var result = _storageManager.UploadFile(stream, fileName, contentType);

                    if (result)
                    {
                        return Ok("File uploaded successfully.");
                    }
                    else
                    {
                        return BadRequest("File upload failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
