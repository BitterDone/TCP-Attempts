using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif

public class Client : MonoBehaviour
{
#if !UNITY_EDITOR
#endif

    private bool _useUWP = true;

#if !UNITY_EDITOR
    private Windows.Networking.Sockets.StreamSocket socket;
    private Task exchangeTask;
#endif

    private Byte[] bytes = new Byte[256];
    private StreamWriter writer;
    private StreamReader reader;

    public string host = "192.168.1.225";
    public string port = "3030";

    public async void Connect()
    {
#if !UNITY_EDITOR
        logs("Connect");
        try
        {
            if (exchangeTask != null) StopExchange();
        
            socket = new Windows.Networking.Sockets.StreamSocket();
            Windows.Networking.HostName serverHost = new Windows.Networking.HostName(host);
            await socket.ConnectAsync(serverHost, port);
        
            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(streamOut) { AutoFlush = true };
        
            Stream streamIn = socket.InputStream.AsStreamForRead();
            reader = new StreamReader(streamIn);

            RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
#endif
    }

    private bool exchanging = false;
    private bool exchangeStopRequested = false;
    private string lastPacket = null;

    private string errorStatus = null;
    private string warningStatus = null;
    private string successStatus = null;
    private string unknownStatus = null;

    public void RestartExchange()
    {
#if !UNITY_EDITOR
        logs("RestartExchange");
        if (exchangeTask != null) StopExchange();
        exchangeStopRequested = false;
        exchangeTask = Task.Run(() => ExchangePackets());
#endif
    }

    public void Update()
    {
        if(lastPacket != null)
        {
            // do something with data
            logs("update last packet " + lastPacket);
        }

        if(errorStatus != null)
        {
            logs("update errorStatus " + errorStatus);
            // StatusTextManager.SetError(errorStatus);
            errorStatus = null;
        }
        if (warningStatus != null)
        {
            logs("update warningStatus " + warningStatus);
            // StatusTextManager.SetWarning(warningStatus);
            warningStatus = null;
        }
        if (successStatus != null)
        {
            logs("update successStatus " + successStatus);
            // StatusTextManager.SetSuccess(successStatus);
            successStatus = null;
        }
        if (unknownStatus != null)
        {
            logs("update unknownStatus " + unknownStatus);
            // StatusTextManager.SetUnknown(unknownStatus);
            unknownStatus = null;
        }
    }

    public void ExchangePackets()
    {
        logs("ExchangePackets");
        while (!exchangeStopRequested)
        {
#if !UNITY_EDITOR
            if (writer == null || reader == null) continue;
            exchanging = true;

            writer.Write("Xclient\n");
            logs("Client sent data!");
            string received = null;

            received = reader.ReadLine();

            lastPacket = received;
            logs("Client read data: " + received);

            exchanging = false;
#endif
        }
    }

    public void StopExchange()
    {
        logs("stop exchange");
        exchangeStopRequested = true;

#if !UNITY_EDITOR
        if (exchangeTask != null) {
            exchangeTask.Wait();
            socket.Dispose();
            writer.Dispose();
            reader.Dispose();

            socket = null;
            exchangeTask = null;
        }
#endif
        writer = null;
        reader = null;
    }

    public void OnDestroy()
    {
        StopExchange();
    }

    private void logs(string msg) {
        Debug.Log(msg);
    }

}