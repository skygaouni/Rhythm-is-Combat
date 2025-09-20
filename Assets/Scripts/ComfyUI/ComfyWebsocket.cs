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
    //�ϥ� Guid ���O�� NewGuid() ��k�ͦ��@�ӷs�� GUID�CGUID �O�@�إ���ߤ@�� 128 �줸�ѧO�X�A�q�`�ΨӰߤ@���Ѥ@�Ӫ���ι��
    //�ͦ��@�ӥ���ߤ@�ѧO�X�]GUID�^�A�ñN���ഫ���r��
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
        //result�G�Ω�O�s���������G��T�]�Ҧp�T�������B��ƪ��׵��^
        //receive "Hello"
        //WebSocketReceiveResult result = new WebSocketReceiveResult(
        //    count: 5,
        //    messageType: WebSocketMessageType.Text,
        //    endOfMessage: true,
        //    closeStatus: null,
        //    closeStatusDescription: null
        //);  
        WebSocketReceiveResult result = null;

        //�� WebSocket �����A���}�� (WebSocketState.Open) �ɡA�{�����򱵦����
        while (ws.State == WebSocketState.Open)
        {
            //var �O�@�� ���t�����ܼơ]Implicitly Typed Variable�^�A���ä��O�@�Ө��骺���A�A�ӬO�@�ػy�k�}�C��A�ϥ� var �ŧi�ܼƮɡA�sĶ���|�ھ��ܼƪ���l�� �۰ʱ��_ �䫬�A�C
            var stringBuilder = new StringBuilder();
            do
            {
                //�ϥ� ReceiveAsync ��k�����Ӧۦ��A�����T��
                //ws.ReceiveAsync ��^�@�� Task<WebSocketReceiveResult>�A�o�O�@�ӥ]�t�������G������A�D�P�B�ާ@������i�� await ���o���G
                //new ArraySegment<byte>(buffer) �o�O�@�ӰO����϶������q�A���w ReceiveAsync ����Ʀs���m
                //CancellationToken.None �Ω�B�z�����ާ@�C�b�o�̡A��ܤ��ϥΨ����\��A�����ާ@�|����i�檽�짹���εo�Ϳ��~�C
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                //WebSocketMessageType.Close�G��ܦ��A���o�e�F�@�� �����T���A�ШD�Ȥ�ݵ��� WebSocket �s���C
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    //�ϥ� CloseAsync ��k�����s��
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    //result.Count �����쪺�T��������
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

            // �ݦ��쪺�T�����S���]�t�H�U�o��A�����ܥN��Ϥ��w�g�ͦ������A�A�өI�s�ШD�Ӥ�
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
