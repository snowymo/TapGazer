using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class Network : MonoBehaviour
{
    public enum COMMUNICATION_TYPE { TCP, UDP};

    public COMMUNICATION_TYPE commType;
    public int sendPort;
    public int listenPort;
    public string connectIP = "172.24.71.214"; // the ip of the server to connect to
    private int MAX_BUF_SIZE = 65535; // 2^16-1

    private IPEndPoint ipEndPoint;
    private TcpClient tcpClient;
    private UdpClient udpClient;
    private Byte[] receivedBytes;

    private Thread tcpThread, udpThread;
    private bool shouldExit = false;

    public string testMessage;

    public ConcurrentQueue<string> serverMessages;

    private bool iSConnServer;
    public void ConnectServer()
    {
        if (iSConnServer)
            return;
        // say hello to the server
        SendTCPMessage("hello");
        iSConnServer = true;
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


    // Start is called before the first frame update
    void Awake()
    {
        iSConnServer = false;
        serverMessages = new ConcurrentQueue<string>();
        ipEndPoint = new IPEndPoint(IPAddress.Parse(connectIP), sendPort);
        switch (commType) {
        case COMMUNICATION_TYPE.TCP:
            tcpClient = new TcpClient();
            StartTCPThread();
            break;
        case COMMUNICATION_TYPE.UDP:
            udpClient = new UdpClient();
            StartUDPThread();
            break;
        default:
            break;
        }
    }

    void StartTCPThread()
    {
        try {
            tcpThread = new Thread(new ThreadStart(ListenForTCPData));
            tcpThread.IsBackground = true;
            tcpThread.Start();
        }
        catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }

    void StartUDPThread()
    {
        udpThread = new Thread(ListenForUDPData);
        udpThread.Start();
    }

    void ListenForTCPData()
    {
        try {
            // connect to relay first
            tcpClient.Connect(IPAddress.Parse(connectIP), sendPort);
            receivedBytes = new Byte[MAX_BUF_SIZE];
            while (!shouldExit) {
                // Get a stream object for reading 				
                using (NetworkStream stream = tcpClient.GetStream()) {
                    int length;
                    // Read incoming stream into byte array. 					
                    while ((length = stream.Read(receivedBytes, 0, receivedBytes.Length)) != 0) {
                        var incommingData = new byte[length];
                        Array.Copy(receivedBytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        //Debug.Log("server message received as: " + serverMessage);
                        serverMessages.Enqueue(serverMessage);
                        // TODO: concurrent queue
                        //HandleTCPResponse(serverMessage);
                    }
                }
            }
            print("client: out of while");
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    // simple class for network status, passed in the async thread
    class UdpState : System.Object {
        public UdpState(IPEndPoint e, UdpClient c) { this.e = e; this.c = c; }
        public IPEndPoint e;
        public UdpClient c;
    }

    void ListenForUDPData()
    {
        // connect to relay first
        Debug.Log("opening connection..." + ipEndPoint);
        //udpClient = new UdpClient(udpPort);
        //udpClient.Client.Bind(ep);

        UdpState state = new UdpState(ipEndPoint, udpClient);
        udpClient.BeginReceive(new AsyncCallback(UDPReceiveCallback), state);
        // idle time to check if the application quits
        while (!shouldExit) {
            Thread.Sleep(100);
        }
        Debug.Log("closing connection...");
        // TODO: handle other cleanup
        udpClient.Close();
    }

    void UDPReceiveCallback(IAsyncResult ar)
    {
        //print("ReceiveCallback");
        if (!shouldExit) {
            return;
        }
        UdpClient c = ((UdpState)(ar.AsyncState)).c;
        IPEndPoint e = ((UdpState)(ar.AsyncState)).e;
        byte[] udpBuf = udpClient.EndReceive(ar, ref e);
        //string str_data = System.Text.Encoding.UTF8.GetString(udpBuf);
        // TODO: handle data
        // TODO: concurrent queue
        //ChalktalkUDPHandler();
        UdpState state = new UdpState(e, c);
        udpClient.BeginReceive(new AsyncCallback(UDPReceiveCallback), state);
    }

    void SendTCPMessage(string request)
    {
        if (tcpClient == null) {
            return;
        }
        try {
            // Get a stream object for writing. 			
            NetworkStream stream = tcpClient.GetStream();
            if (stream.CanWrite) {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(request);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    void SendUDPMessage(byte[] request)
    {
        if (udpClient == null) {
            return;
        }
        udpClient.Send(request, request.Length, ipEndPoint);
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        shouldExit = true;
        if (tcpClient != null)
            tcpClient.Close();
        //active = false;
    }

    void SendNetworkMessage(string request)
    {
        switch (commType) {
        case COMMUNICATION_TYPE.TCP:
            SendTCPMessage(request);
            break;
        case COMMUNICATION_TYPE.UDP:
            byte[] bArray = Encoding.ASCII.GetBytes(request);
            SendUDPMessage(bArray);
            break;
        default:
            break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) {
            // send
            SendNetworkMessage(testMessage);
        }
    }
}
