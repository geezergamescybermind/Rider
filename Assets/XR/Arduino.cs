using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Arduino : MonoBehaviour
{
	UdpClient clientData;
	int portData = 5001;
	public int receiveBufferSize = 120000;

	public int currentRotation;

	public int revolutionCounter = 0;

	private bool[] bits = new bool[32];

	public bool showDebug = false;
	IPEndPoint ipEndPointData;
	private object obj = null;
	private System.AsyncCallback AC;
	byte[] receivedBytes;

	void Start()
	{
		InitializeUDPListener();
	}
	public void InitializeUDPListener()
	{
		ipEndPointData = new IPEndPoint(IPAddress.Any, portData);
		clientData = new UdpClient();
		clientData.Client.ReceiveBufferSize = receiveBufferSize;
		clientData.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
		clientData.ExclusiveAddressUse = false;
		clientData.EnableBroadcast = true;
		clientData.Client.Bind(ipEndPointData);
		clientData.DontFragment = true;
		if (showDebug) Debug.Log("BufSize: " + clientData.Client.ReceiveBufferSize);
		AC = new System.AsyncCallback(ReceivedUDPPacket);
		clientData.BeginReceive(AC, obj);
		Debug.Log("UDP - Start Receiving..");
	}

	void ReceivedUDPPacket(System.IAsyncResult result)
	{
		//stopwatch.Start();
		receivedBytes = clientData.EndReceive(result, ref ipEndPointData);

		if (receivedBytes.Length == 4)
		{
			currentRotation = receivedBytes[0] + receivedBytes[1];
			revolutionCounter = decodeTotalRevolutions(receivedBytes[2], receivedBytes[3]);
		}

		ParsePacket();

		clientData.BeginReceive(AC, obj);

		//stopwatch.Stop();
		//Debug.Log(stopwatch.ElapsedTicks);
		//stopwatch.Reset();
	} // ReceiveCallBack

	void ParsePacket()
	{
		// work with receivedBytes
		Debug.Log("receivedBytes len = " + receivedBytes.Length);
	}

	void OnDestroy()
	{
		if (clientData != null)
		{
			clientData.Close();
		}

	}

	private int decodeTotalRevolutions(byte byte1, byte byte2)
	{
		int nominator = 128;
		int value = (int)byte1;
		for (int t = 0; t < 8; ++t)
		{
			bits[t] = false;
			if (value >= nominator)
			{
				value = value - nominator;
				bits[t] = true;
			}
			nominator = Mathf.CeilToInt(nominator / 2f);
		}
		nominator = 128;
		value = (int)byte2;
		for (int t = 8; t < 16; ++t)
		{
			bits[t] = false;
			if (value >= nominator)
			{
				value = value - nominator;
				bits[t] = true;
			}
			nominator = Mathf.CeilToInt(nominator / 2f);
		}

		nominator = 32768;
		value = 0;
		for (int t = 0; t < 16; ++t)
		{
			if (bits[t] == true)
			{
				value = value + nominator;
			}
			nominator = Mathf.CeilToInt(nominator / 2);
		}

		return value;
	}
}
/*
public class Arduino : MonoBehaviour
{
	// udpclient object
	UdpClient client;
	IPEndPoint anyIP;
	public int port = 5001;

	public int currentRotation;

    public int revolutionCounter = 0;

    private bool[] bits = new bool[32];

	float timerStored;
	float updateDelay = 0.1f;
	float lastUpdate;

	void Start()
	{
		client = new UdpClient(port);
		anyIP = new IPEndPoint(IPAddress.Any, 0);
	}

	// receive thread
	private void Update()
	{
		while (true)
		{
			try
			{
				byte[] data = client.Receive(ref anyIP);
				if (data.Length == 4)
				{
					currentRotation = data[0] + data[1];
					revolutionCounter = decodeTotalRevolutions(data[2], data[3]);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
	}
*/
   