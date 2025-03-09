using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;

namespace SimpleServer
{
    class User
    {
        public string Id { get; set; }
        public TcpClient Socket;
        private readonly object _lock = new object();

        public User(string id, TcpClient socket)
        {
            Id = id;
            Socket = socket;
        }

        public async Task SendMessageAsync(Packet.PacketBase message)
        {
            lock (_lock)
            {
                if (Socket.Connected)
                {
                    byte[] data = message.Serialize((int)Packet.CS.CS_TASK_1);
                    NetworkStream stream = Socket.GetStream();
                    stream.WriteAsync(data, 0, data.Length).Wait();
                    Console.WriteLine($"Sent : {message.GetType().Name}");
                }
            }
        }

        public async Task<Packet.PacketBase> ReceiveMessageAsync()
        {
            NetworkStream stream = Socket.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            //Packet.PacketBase packet = (Packet.PacketBase)Activator.CreateInstance(type);
            Packet.PacketBase packet = Packet.PacketBase.Deserialize(buffer);
            return packet;
        }
    }
}
