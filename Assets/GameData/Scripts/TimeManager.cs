using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
	public static TimeManager instance;

	public static bool TimeSyncedFromNTP
	{
		get;
		private set;
	}

	public static DateTime SyncedTime
	{
		get;
		private set;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			StartCoroutine(GetNtpTime());
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator GetNtpTime()
	{
		byte[] ntpData = new byte[48];
		IPAddress[] addressList = Dns.GetHostEntry("pool.ntp.org").AddressList;
		IPEndPoint ipEndPoint = new IPEndPoint(addressList[0], 123);
		long pingDuration = Stopwatch.GetTimestamp();
		using (Socket socket = new Socket(addressList[0].AddressFamily, SocketType.Dgram, ProtocolType.Udp))
		{
			new SocketAsyncEventArgs();
			SocketAsyncEventArgs e;
			do
			{
				for (int i = 0; i < ntpData.Length; i++)
				{
					ntpData[i] = 0;
				}
				ntpData[0] = 59;
				socket.Connect(ipEndPoint);
				socket.ReceiveTimeout = 5000;
				socket.Send(ntpData);
				pingDuration = Stopwatch.GetTimestamp();
				e = new SocketAsyncEventArgs();
				e.SetBuffer(ntpData, 0, 48);
				yield return new WaitForSocketAsync(socket.ReceiveAsync, e);
				pingDuration = Stopwatch.GetTimestamp() - pingDuration;
			}
			while (e.SocketError != 0);
		}
		long num = pingDuration * 10000000 / Stopwatch.Frequency;
		ulong num2 = ((ulong)ntpData[40] << 24) | ((ulong)ntpData[41] << 16) | ((ulong)ntpData[42] << 8) | ntpData[43];
		long num3 = (long)(((ulong)ntpData[44] << 24) | ((ulong)ntpData[45] << 16) | ((ulong)ntpData[46] << 8) | ntpData[47]);
		long num4 = (long)(num2 * 10000000) + (num3 * 10000000 >> 32);
		SyncedTime = new DateTime(599266080000000000L + num4 + num / 2);
	}
}
