using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

namespace Goose2Client
{
    public class NetworkClient
    {
        public event Action<Exception> ConnectionError;
        public event Action Connected;
        public event Action<Exception> SocketError;

        public bool IsConnected { get { return socket != null && socket.Connected; } }

        public bool Pause { get; set; } = false;

        private Socket socket;

        private string packetBuffer;

        public void Connect(string address, int port)
        {
            try
            {
                packetBuffer = "";
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(address, port);

                Connected?.Invoke();
            }
            catch (Exception e)
            {
                ConnectionError?.Invoke(e);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (socket != null && socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket?.Close();
            }

            socket = null;
            packetBuffer = "";
        }

        public void Send(string packet)
        {
            packet += '\x1';
            try
            {
                socket.Send(System.Text.Encoding.ASCII.GetBytes(packet));
            }
            catch (Exception e)
            {
                SocketError?.Invoke(e);
            }
        }

        public void Update()
        {
            if (Pause || !IsConnected) return;

            var readSockets = new List<Socket> { socket };

            Socket.Select(readSockets, null, null, 500);

            foreach (var socket in readSockets)
            {
                try
                {
                    var buffer = new byte[8192];

                    int received = socket.Receive(buffer);
                    string receivedString = System.Text.Encoding.ASCII.GetString(buffer, 0, received);
                    packetBuffer += receivedString;

                    // Debug.Log($"Received: {receivedString.Replace('\x1', '\n')}");

                    if (packetBuffer.Length == 0) continue;

                    string[] packets = packetBuffer.Split("\x1".ToCharArray());
                    int limit = packets.Length - 1;

                    if (!packetBuffer.EndsWith("\x1"))
                    {
                        packetBuffer = packets[packets.Length - 1];
                    }
                    else
                    {
                        packetBuffer = "";
                    }

                    for (int i = 0; i < limit; i++)
                    {
                        //Console.WriteLine($"P: {packets[i]}");
                        GameManager.Instance.PacketManager.Handle(packets[i]);

                        if (Pause)
                        {
                            packetBuffer = string.Join('\x1', packets.Skip(i + 1)) + packetBuffer;
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    SocketError?.Invoke(e);
                }
            }
        }

        public void Login(string username, string password)
        {
            Send($"LOGIN{username},{password},GooseClient");
        }

        public void LoginContinued()
        {
            Send($"LCNT");
        }

        public void Pong()
        {
            Send($"PONG");
        }

        public void DoneLoadingMap()
        {
            Send($"DLM");
        }

        public void Move(Direction d)
        {
            Send($"M{(int)d + 1}");
        }
    }
}