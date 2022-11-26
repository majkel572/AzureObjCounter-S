using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
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
        Thread.Sleep(1000);

        // sas maker
        BlobSasBuilder blobSasBuilder = new BlobSasBuilder() {
            BlobContainerName = config.GetValue<string>("blobContainerName"),
            BlobName = blobName,
            ExpiresOn = DateTime.UtcNow.AddMinutes(5),//Let SAS token expire after 5 minutes.
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read);//User will only be able to read the blob and it's properties
        var sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(config.GetValue<string>("blobContainerName"), config.GetValue<string>("blobConnectionString"))).ToString();
        var sasUrl = blobServiceClient.Uri.AbsoluteUri + "?" + sasToken;
        // end of sas maker

        var prediction_endpoint = "https://customvisiondetector-prediction.cognitiveservices.azure.com/";
        var prediction_key = "480c10e29ebd486b90bea83ca6d082e1";
        var project_id = "aee14dc7-79af-4b9f-b42a-cfd8147dd022";
        var model_name = "Iteration1";















        //Thread.Sleep(2000);
        // ProcessStartInfo start = new ProcessStartInfo();
        //start.FileName = "dist/customvision.exe";
        //start.Arguments = string.Format("{0}", blobName);
        //start.UseShellExecute = false;
        //start.RedirectStandardOutput = true;
        // using (Process process = Process.Start(start)) {
        //using (StreamReader reader = process.StandardOutput) {
        //    result = reader.ReadToEnd();
        //    Console.Write(result);
        //}
        //}
        counter++;
        return result;
        }
        return "not done";
});

app.Run();
