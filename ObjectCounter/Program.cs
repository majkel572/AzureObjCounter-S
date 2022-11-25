using Azure.Storage.Blobs;
using System.Diagnostics;
using System.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
int counter = 0;
app.MapPost("/api/upload", (IConfiguration config, HttpContext hc) => {
        var formData = hc.Request.Form;     // download content from request
        if (formData.Files.Count == 1) {          // if it has file get it
            var image = formData.Files[0];
            var blobServiceClient = new BlobServiceClient(config.GetValue<string>("blobConnectionString")); // handle for blob service
            var containerClient = blobServiceClient.GetBlobContainerClient(config.GetValue<string>("blobContainerName")); // handle for container
            string blobName = "countHere" + counter + ".jpg"; // blob name to process
            using var imageStream = image.OpenReadStream(); // change image to openstream
            string result = "nie udalo sie";
            try { // pass image with its name 
                containerClient.UploadBlob(blobName, imageStream);
            } catch (Azure.RequestFailedException e){
                return "Azure blob storage already contains this file!";
            }
            counter++;
            Thread.Sleep(2000);
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "dist/customvision.exe";
            start.Arguments = string.Format("{0}", blobName);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start)) {
            using (StreamReader reader = process.StandardOutput) {
                result = reader.ReadToEnd();
                Console.Write(result);
            }
        }
        return result;
        }
        return "not done";
});

app.Run();
