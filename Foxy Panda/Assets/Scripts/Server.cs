using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Net.Sockets;

public class Server : MonoBehaviour
{

    private bool _useUWP = false;
    System.Net.Sockets.TcpClient client;
    System.Net.Sockets.NetworkStream stream;
    private Thread exchangeThread;

    private Byte[] bytes = new Byte[256];
    private StreamWriter writer;
    private StreamReader reader;

    public string host = "192.168.1.225";
    public string port = "3030";

    public void Connect()
    {
        logs("Connect");
        try
        {
            if (exchangeThread != null) StopExchange();

            client = new System.Net.Sockets.TcpClient(host, Int32.Parse(port));
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
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
        logs("RestartExchange");
        if (exchangeThread != null) StopExchange();
        exchangeStopRequested = false;
        exchangeThread = new System.Threading.Thread(ExchangePackets);
        exchangeThread.Start();
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
            if (writer == null || reader == null) continue;
            exchanging = true;

            writer.Write("Xserver\n");
            logs("Server sent data!");
            string received = null;

            byte[] bytes = new byte[client.SendBufferSize];
            int recv = 0;
            while (true)
            {
                try {
                    recv = stream.Read(bytes, 0, client.SendBufferSize);
                    received += Encoding.UTF8.GetString(bytes, 0, recv);
                    logs(received);
                    if (received.EndsWith("\n")) break;
                } catch (SocketException e) {
                    logs("socketException");
                    logs(e.ToString());
                }
            }

            lastPacket = received;
            logs("Server read data: " + received);

            exchanging = false;
        }
    }

    public void sendMsgToClient()
    {
        ExchangePackets();

    }

    public void StopExchange()
    {
        logs("StopExchange");
        exchangeStopRequested = true;

        if (exchangeThread != null)
        {
            exchangeThread.Abort();
            stream.Close();
            client.Close();
            writer.Close();
            reader.Close();

            stream = null;
            exchangeThread = null;
        }
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