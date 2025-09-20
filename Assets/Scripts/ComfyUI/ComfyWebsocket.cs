using System.Collections.Generic;
using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEngine.Events;

public class ResponseDataWebsocket
{
    public string prompt_id;
}
[Serializable]
public class ProgressData
{
    public int value;
    public int max;
    public string prompt_id;
}

[Serializable]
public class ProgressMessage
{
    public string type;
    public ProgressData data;
}
public class ComfyWebsocket : MonoBehaviour
{
    private string serverAddress = "127.0.0.1:8188";
    //使用 Guid 類別的 NewGuid() 方法生成一個新的 GUID。GUID 是一種全域唯一的 128 位元識別碼，通常用來唯一標識一個物件或實例
    //生成一個全域唯一識別碼（GUID），並將它轉換為字串
    private string clientId = Guid.NewGuid().ToString();
    private ClientWebSocket ws = new ClientWebSocket();

    public UnityEngine.UI.Slider progressBar;
    public TMPro.TextMeshProUGUI progressText;


    public ComfyImage comfyimage;

    // Start is called before the first frame update
    async void Start()
    {
        await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);
        StartListening();
        
    }
    public string promptID;
    private async void StartListening()
    {
        Debug.Log("web is on");
        var buffer = new byte[1024 * 4];
        //result：用於保存接收的結果資訊（例如訊息類型、資料長度等）
        //receive "Hello"
        //WebSocketReceiveResult result = new WebSocketReceiveResult(
        //    count: 5,
        //    messageType: WebSocketMessageType.Text,
        //    endOfMessage: true,
        //    closeStatus: null,
        //    closeStatusDescription: null
        //);  
        WebSocketReceiveResult result = null;

        //當 WebSocket 的狀態為開啟 (WebSocketState.Open) 時，程式持續接收資料
        while (ws.State == WebSocketState.Open)
        {
            //var 是一種 隱含類型變數（Implicitly Typed Variable），它並不是一個具體的型態，而是一種語法糖。當你使用 var 宣告變數時，編譯器會根據變數的初始值 自動推斷 其型態。
            var stringBuilder = new StringBuilder();
            do
            {
                //使用 ReceiveAsync 方法接收來自伺服器的訊息
                //ws.ReceiveAsync 返回一個 Task<WebSocketReceiveResult>，這是一個包含接收結果的物件，非同步操作完成後可用 await 取得結果
                //new ArraySegment<byte>(buffer) 這是一個記憶體區塊的片段，指定 ReceiveAsync 的資料存放位置
                //CancellationToken.None 用於處理取消操作。在這裡，表示不使用取消功能，接收操作會持續進行直到完成或發生錯誤。
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                //WebSocketMessageType.Close：表示伺服器發送了一條 關閉訊息，請求客戶端結束 WebSocket 連接。
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    //使用 CloseAsync 方法關閉連接
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    //result.Count 為收到的訊息的長度
                    var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    stringBuilder.Append(str);
                    //Debug.Log("stringBuilder:" + stringBuilder);
                    //Debug.Log("EndOfMessage:");
                }
            }
            while (!result.EndOfMessage);
            //Debug.Log("out of while");
            string response = stringBuilder.ToString();
            Debug.Log("Receiveds: " + response);
            if (response.Contains("progress"))
            {
                ProgressMessage msg = JsonUtility.FromJson<ProgressMessage>(response);
                float progressRatio = (float)msg.data.value / msg.data.max;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (progressBar != null) progressBar.value = msg.data.value;
                    if (progressText != null) progressText.text = (progressRatio * 100).ToString("0") + "%";
                });
            }

            // 看收到的訊息有沒有包含以下這串，有的話代表圖片已經生成完成，再來呼叫請求照片
            if (response.Contains("\"queue_remaining\": 0"))
            {
                comfyimage.RequestFileName(promptID);
            }

        }
    }
    void OnDestroy()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}
