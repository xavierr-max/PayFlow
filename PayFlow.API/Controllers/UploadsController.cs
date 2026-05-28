using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Exceptions;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    private readonly IWebHostEnvironment _environment;

    public UploadsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("images")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("Arquivo de imagem inválido");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ValidationException("Formato de imagem inválido. Use JPG, PNG, WEBP ou GIF.");

        var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "products");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.CreateVersion7():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/products/{fileName}";
        return Ok(new { imageUrl });
    }
}
