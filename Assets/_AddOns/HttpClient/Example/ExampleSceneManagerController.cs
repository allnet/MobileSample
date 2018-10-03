using CI.HttpClient;
using UnityEngine;
using UnityEngine.UI;

public class ExampleSceneManagerController : MonoBehaviour
{
    public Text LeftText;
    public Text RightText;
    public Slider ProgressSlider;

    private readonly string basePath = "https://api-demo.evconnect.com/rest/v5/";
    string section = "locations/";
    string actualRequest;


    public void Upload()
    {
        HttpClient client = new HttpClient();

        byte[] buffer = new byte[1000000];
        new System.Random().NextBytes(buffer);

        ByteArrayContent content = new ByteArrayContent(buffer, "application/bytes");

        ProgressSlider.value = 0;

        client.Post(new System.Uri("http://httpbin.org/post"), content, HttpCompletionOption.AllResponseContent, (r) =>
        {           
        }, (u) =>
        {
            LeftText.text = "Upload: " +  u.PercentageComplete.ToString() + "%";
            ProgressSlider.value = u.PercentageComplete;
        });
    }

    //public void Download()
    //{
    //    HttpClient client = new HttpClient();

    //    ProgressSlider.value = 100;

    //    client.GetByteArray(new System.Uri("http://httpbin.org/bytes/1000000"), HttpCompletionOption.StreamResponseContent, (r) =>
    //    {
    //        RightText.text = "Download: " + r.PercentageComplete.ToString() + "%";
    //        ProgressSlider.value = 100 - r.PercentageComplete;
    //    });
    //}

    public void Download()
    {
        HttpClient client = new HttpClient();

    
        string newPath = basePath + section + "mapping?latitude=33.923&longitude=-118.388&distance=10&metric=MILES";
        ProgressSlider.value = 100;

        client.GetByteArray(new System.Uri(basePath), HttpCompletionOption.StreamResponseContent, (r) =>
        {
            RightText.text = "Download: " + r.PercentageComplete.ToString() + "%";
            ProgressSlider.value = 100 - r.PercentageComplete;
        });
    }

    public void UploadDownload()
    {
        HttpClient client = new HttpClient();

        byte[] buffer = new byte[1000000];
        new System.Random().NextBytes(buffer);

        ByteArrayContent content = new ByteArrayContent(buffer, "application/bytes");

        ProgressSlider.value = 0;

        client.Post(new System.Uri("http://httpbin.org/post"), content, HttpCompletionOption.StreamResponseContent, (r) =>
        {
            RightText.text = "Download: " + r.PercentageComplete.ToString() + "%";
            ProgressSlider.value = 100 - r.PercentageComplete;
        }, (u) =>
        {
            LeftText.text = "Upload: " + u.PercentageComplete.ToString() + "%";
            ProgressSlider.value = u.PercentageComplete;
        });
    }

    public void Delete()
    {
        HttpClient client = new HttpClient();
        client.Delete(new System.Uri("http://httpbin.org/delete"), HttpCompletionOption.AllResponseContent, (r) =>
        {
        });
    }

    public void Get()
    {
        HttpClient client = new HttpClient();
        client.GetByteArray(new System.Uri("http://httpbin.org/get"), HttpCompletionOption.AllResponseContent, (r) =>
        {
        });
    }

    public void Patch()
    {
        HttpClient client = new HttpClient();

        StringContent content = new StringContent("Hello World");

        client.Patch(new System.Uri("http://httpbin.org/patch"), content, HttpCompletionOption.AllResponseContent, (r) =>
        {
        });
    }

    public void Post()
    {
        HttpClient client = new HttpClient();

        StringContent content = new StringContent("Hello World");

        client.Post(new System.Uri("http://httpbin.org/post"), content, HttpCompletionOption.AllResponseContent, (r) =>
        {
        });
    }

    public void Put()
    {
        HttpClient client = new HttpClient();

        StringContent content = new StringContent("Hello World");

        client.Put(new System.Uri("http://httpbin.org/put"), content, HttpCompletionOption.AllResponseContent, (r) =>
        {
        });
    }
}