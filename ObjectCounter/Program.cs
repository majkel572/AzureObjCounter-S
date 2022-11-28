using Azure.Storage.Blobs;
using AzureObjCounterS;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
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
        WebRequest request = HttpWebRequest.Create("https://customvisiondetector-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/aee14dc7-79af-4b9f-b42a-cfd8147dd022/detect/iterations/Iteration1/image");
        request.Method = "POST";
        request.Headers.Add("Prediction-Key", "480c10e29ebd486b90bea83ca6d082e1");
        request.Headers.Add("Content-Type", "application/octet-stream");
        var f = image.OpenReadStream();
        using (var ms = new MemoryStream()) {
            f.CopyTo(ms);
            var fileBytes = ms.ToArray();
            request.ContentLength = fileBytes.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(fileBytes, 0, fileBytes.Length);
            stream.Close();
        }
        HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
        string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        Console.WriteLine(json);
        ResponseJson? jsonResponse = JsonSerializer.Deserialize<ResponseJson>(json);
        List<Predictions> predictionList = jsonResponse.predictions;

        var stats = new Dictionary<string, double>();
        foreach (Predictions rjn in predictionList) {
            if (rjn.probability * 100 > 50)
                if (stats.ContainsKey(rjn.tagName)) {
                    stats[rjn.tagName]++;
                } else {
                    stats[rjn.tagName] = 1;
                }
        }

        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, double>));
        using (MemoryStream ms = new MemoryStream()) {
            serializer.WriteObject(ms, stats);
            result = Encoding.Default.GetString(ms.ToArray());
        }
        return result;
        }
        return "not done";
});

app.Run();
