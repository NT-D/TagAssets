#r "System.Drawing"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
using System.Drawing;
using System.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

public static async Task Run(CloudBlockBlob myBlob, string name, TraceWriter log, ICollector<string> outputQueueItem)
{
    log.Info($"Started process with {name}");
    var client = new HttpClient();
    
    // Request headers
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<Insert your Key here>");
    var uri = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Categories,Tags,Description,Faces,ImageType,Color&language=en";
    byte[] data;

    using (var memoryStream = new MemoryStream())
    {
        myBlob.DownloadToStream(memoryStream);
        data = memoryStream.ToArray();
    }

    using (var content = new ByteArrayContent(data))
    {
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        HttpResponseMessage response = await client.PostAsync(uri, content);
        string result = await response.Content.ReadAsStringAsync();
        //Show tags we get from Computer Vision API
        log.Info(result);
        VisionData visiondata = JsonConvert.DeserializeObject<VisionData>(result);
        visiondata.filename = name;
        string resultText = JsonConvert.SerializeObject(visiondata);
        log.Info(resultText);
        outputQueueItem.Add(resultText);
    }
}

    public class VisionData
    {
        //Added filename property for fetching data from web application
        public string filename { get; set; }
        public Category[] categories { get; set; }
        public Tag[] tags { get; set; }
        public Description description { get; set; }
        public string requestId { get; set; }
        public Metadata metadata { get; set; }
        public Face[] faces { get; set; }
        public Color color { get; set; }
        public Imagetype imageType { get; set; }
        public string id { get; set; }
    }

    public class Description
    {
        public string[] tags { get; set; }
        public Caption[] captions { get; set; }
    }

    public class Caption
    {
        public string text { get; set; }
        public float confidence { get; set; }
    }

    public class Metadata
    {
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
    }

    public class Color
    {
        public string dominantColorForeground { get; set; }
        public string dominantColorBackground { get; set; }
        public string[] dominantColors { get; set; }
        public string accentColor { get; set; }
        public bool isBWImg { get; set; }
    }

    public class Imagetype
    {
        public int clipArtType { get; set; }
        public int lineDrawingType { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public float score { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public float confidence { get; set; }
    }

    public class Face
    {
        public int age { get; set; }
        public string gender { get; set; }
        public Facerectangle faceRectangle { get; set; }
    }

    public class Facerectangle
    {
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }