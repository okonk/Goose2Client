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

                    //Debug.Log($"Received: {receivedString.Replace('\x1', '\n')}");

                    if (packetBuffer.Length == 0) continue;

                    string[] packets = packetBuffer.Split("\x1".ToCharArray());
                    int limit = packets.Length;

                    if (!packetBuffer.EndsWith("\x1"))
                    {
                        packetBuffer = packets[packets.Length - 1];
                        limit--;
                    }
                    else
                    {
                        packetBuffer = "";
                    }

                    for (int i = 0; i < limit; i++)
                    {
                        // Debug.Log($"P: {packets[i]}");
                        GameManager.Instance.PacketManager.Handle(packets[i]);

                        if (Pause)
                        {
                            packetBuffer = string.Join('\x1', packets.Skip(i + 1).Take(limit - i - 1)) + (packetBuffer.Length > 0 ? $"\x1{packetBuffer}" : "");
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Network Exception: {e}");
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

        public void Face(Direction d)
        {
            Send($"F{(int)d + 1}");
        }

        public void Attack()
        {
            Send($"ATT");
        }

        public void UseItem(int slot)
        {
            Send($"USE{slot + 1}");
        }

        public void MoveItemInInventory(int fromSlot, int toSlot)
        {
            Send($"CHANGE{fromSlot + 1},{toSlot + 1}");
        }

        public void SplitStackInInventory(int fromSlot, int toSlot, int splitAmount)
        {
            Send($"SPLIT{fromSlot + 1},{toSlot + 1},{splitAmount}");
        }

        public void MoveInventoryToWindow(int fromSlot, int windowId, int toSlot)
        {
            Send($"ITW{fromSlot + 1},{windowId},{toSlot + 1}");
        }

        public void MoveWindowToInventory(int windowId, int fromSlot, int toSlot)
        {
            Send($"WTI{windowId},{fromSlot + 1},{toSlot + 1}");
        }

        public void MoveWindowToWindow(int fromWindowId, int fromSlot, int toWindowId, int toSlot)
        {
            Send($"WTW{fromWindowId},{fromSlot + 1},{toWindowId},{toSlot + 1}");
        }

        public void Drop(int fromSlot, int amount)
        {
            Send($"DRP{fromSlot + 1},{amount}");
        }

        public void Pickup()
        {
            Send($"GET");
        }

        public void MoveSpell(int fromSlot, int toSlot)
        {
            Send($"SWAP{fromSlot + 1},{toSlot + 1}");
        }

        public void CastSpell(int slot, int targetId)
        {
            Send($"CAST{slot + 1},{targetId}");
        }

        public void Quit()
        {
            Send($"QUIT");
        }

        public void KillBuff(int id)
        {
            Send($"KBUF{id}");
        }

        public void OpenCombineBag()
        {
            Send($"OCB");
        }

        public void WindowButtonClick(WindowButtons button, int windowId, int npcId, int unknownId1 = 0, int unknownId2 = 0)
        {
            Send($"WBC{(int)button},{windowId},{npcId},{unknownId1},{unknownId2}");
        }

        public void VendorPurchaseItem(int npcId, int slotId)
        {
            Send($"VPI{npcId},{slotId + 1}");
        }

        public void VendorSellItem(int npcId, int slotId, int stackSize)
        {
            Send($"VSI{npcId},{slotId + 1},{stackSize}");
        }

        public void LeftClick(int x, int y)
        {
            Send($"LC{x + 1},{y + 1}");
        }

        public void RightClick(int x, int y)
        {
            Send($"RC{x + 1},{y + 1}");
        }

        public void ChatMessage(string message)
        {
            Send($";{message}");
        }

        public void Command(string command)
        {
            Send(command);
        }

        public void Emote(int animationId, int graphicFile)
        {
            Send($"EMOT{animationId},{graphicFile}");
        }
    }
}