using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}
public class ComfyPrompt : MonoBehaviour
{
    public TMP_InputField pInput, nInput;

    //public InputField promptJsonInput;

    public string promptJsonObject;
    public string promptJsonAudio;
    public GameObject backbutton;
    public GameObject progress;

    /*public void QueuePrompt()
    {
        StartCoroutine(QueuePromptCoroutine(pInput.text, nInput.text));
    }*/
    public void GenerateMusic()
    {
        backbutton.SetActive(false);
        progress.SetActive(true);
        StartCoroutine(GenerateMusicCoroutine(pInput.text, nInput.text));
    }
    /*private IEnumerator QueuePromptCoroutine(string positivePrompt, string negativePrompt)
    {
        Debug.Log("positivePrompt:" + positivePrompt);
        Debug.Log("negativePrompt:" + negativePrompt);
        string url = "http://127.0.0.1:8188/prompt";
        string promptText = GeneratePromptJson();
        System.Random rand = new System.Random();
        int randomInt = rand.Next();
        string randomString = randomInt.ToString();
        promptText = promptText.Replace("235605782970161", randomString);
        promptText = promptText.Replace("Pprompt", positivePrompt);
        promptText = promptText.Replace("Nprompt", negativePrompt);
        Debug.Log("promptText:"+promptText);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            Debug.Log("Prompt ID: " + data.prompt_id);
            GetComponent<ComfyWebsocket>().promptID = data.prompt_id;
            // GetComponent<ComfyImageCtr>().RequestFileName(data.prompt_id);
        }
    }*/
    private IEnumerator GenerateMusicCoroutine(string positivePrompt, string negativePrompt)
    {
        string qualityEnhancers = "high quality, professional mix, studio recording, rich texture, dynamic range, clear sound, balanced EQ, well mastered";
        string qualityNegatives = "distortion, noisy background, unbalanced mix, clipping, low bitrate, artifacts, poor mastering";
        string rhythmEnhancers = "clear rhythm, steady tempo, moderate pace, strong beat, tempo: 90bpm to 110bpm, groove, synced timing";
        string backendPositive = qualityEnhancers + ", " + rhythmEnhancers;
        positivePrompt = positivePrompt + ", " + backendPositive;
        negativePrompt = negativePrompt + ", " + qualityNegatives;
        Debug.Log("positivePrompt:" + positivePrompt);
        Debug.Log("negativePrompt:" + negativePrompt);
        string url = "http://127.0.0.1:8188/prompt";
        string promptText = GenerateMusicPromptJson();
        System.Random rand = new System.Random();
        int randomInt = rand.Next();
        string randomString = randomInt.ToString();
        promptText = promptText.Replace("12354546", randomString);
        promptText = promptText.Replace("Pprompt", positivePrompt);
        promptText = promptText.Replace("Nprompt", negativePrompt);
        Debug.Log("promptText:" + promptText);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            Debug.Log("Prompt ID: " + data.prompt_id);
            GetComponent<ComfyWebsocket>().promptID = data.prompt_id;
            // GetComponent<ComfyImageCtr>().RequestFileName(data.prompt_id);
        }
    }
    private string GeneratePromptJson()
    {
        string guid = Guid.NewGuid().ToString();

        string promptJsonWithGuid = $@"
    {{
        ""id"": ""{guid}"",
        ""prompt"": {promptJsonObject}
    }}";

        return promptJsonWithGuid;
    }
    private string GenerateMusicPromptJson()
    {
        string guid = Guid.NewGuid().ToString();

        string promptJsonWithGuid = $@"
    {{
        ""id"": ""{guid}"",
        ""prompt"": {promptJsonAudio}
    }}";

        return promptJsonWithGuid;
    }
}
