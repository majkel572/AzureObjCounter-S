using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase {
    private IConfiguration config;
    private string connectionString;
    private string containerName;
    public ImageController(IConfiguration configuration) {
        config = configuration;
        connectionString = config.GetValue<string>("blobConnectionString");
        containerName = config.GetValue<string>("blobContainerName");

    }

    [HttpPost("test")]
    public async Task<ActionResult> TestMethod([FromBody] string username) {
        return Ok("skontaktowano sie z endpointem " + username);
    }

    [HttpGet("getujemy")]
    public async Task<ActionResult> TestMethodGet() {
        return Ok("skontaktowano sie z endpointem ");
    }


    [HttpPost("Upload")]
    public async Task<ActionResult> UploadImage(IFormFile file) {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        string blobName = "xxx";
        using var fileStream = file.OpenReadStream();
        containerClient.UploadBlob(blobName, fileStream);
        return Ok();
    }
}
