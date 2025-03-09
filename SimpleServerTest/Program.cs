using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;

namespace SimpleServer
{
    internal class Program
    {
        // 클라이언트 연결
        public delegate void ClientConnectedHandler(TcpClient client);
        public static event ClientConnectedHandler ClientConnected;

        // 메시지 전송
        public delegate void MessageSentHandler(string message);
        public static event MessageSentHandler MessageSent;

        // 메시지 수신
        public delegate void MessageReceivedHandler(TcpClient client, string message);
        public static event MessageReceivedHandler MessageReceived;

        // 연결 종료
        public delegate void ClientDisconnectedHandler(TcpClient client);
        public static event ClientDisconnectedHandler ClientDisconnected;

        private static List<User> users = new List<User>();

        private static Dictionary<int, Func<User, Packet.PacketBase, Task>> packetHandlers = new Dictionary<int, Func<User, Packet.PacketBase, Task>>();

        private static async Task Main(string[] args)
        {
            // 이벤트 핸들러 등록
            ClientConnected += OnClientConnected;
            MessageReceived += OnMessageReceived;
            ClientDisconnected += OnClientDisconnected;
            
            // 패킷 핸들러
            RegisterPacketHandlers();

            TcpListener server = null;
            try
            {
                int port = 52775;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddr, port);
                server.Start();

                Console.WriteLine("Waiting for a connection...");

                // 연결 대기
                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    User user = new User(Guid.NewGuid().ToString(), client);
                    users.Add(user);
                    Console.WriteLine("Connected!");

                    // 연결 이벤트 호출
                    ClientConnected?.Invoke(client);

                    // 메시지 대기
                    _ = HandleClientAsync(user);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
            }
            finally
            {
                server?.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private static void RegisterPacketHandlers()
        {
            packetHandlers[(int)Packet.CS.CS_PING] = HandlePingAsync;
            packetHandlers[(int)Packet.CS.CS_TASK_1] = HandleTask1Async;
            packetHandlers[(int)Packet.CS.CS_TASK_2] = HandleTask2Async;
        }

        private static async Task HandleClientAsync(User user)
        {
            try
            {
                while (true)
                {
                    Packet.PacketBase data = await user.ReceiveMessageAsync();
                    Console.WriteLine($"Received: {data.GetType().Name}");

                    // 메시지 수신 이벤트 호출
                    MessageReceived?.Invoke(user.Socket, data.GetType().Name);

                    // 패킷 타입에 따라 핸들러 호출
                    if (packetHandlers.TryGetValue(data.PacketType, out var handler))
                    {
                        handler(user, data);
                    }
                    else
                    {
                        Console.WriteLine("Unknown packet type received.");
                    }

                    //// 클라이언트로 메시지 전송
                    //await user.SendMessageAsync(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
            finally
            {
                // 연결 종료 이벤트 호출
                ClientDisconnected?.Invoke(user.Socket);
                user.Socket.Close();
            }
        }

        private static async Task HandlePingAsync(User user, Packet.PacketBase packet)
        {
            Console.WriteLine("Handling PING packet.");

            Packet.PacketBase scPacket = new Packet.SC_PONG();
            await user.SendMessageAsync(scPacket);
        }

        private static async Task HandleTask1Async(User user, Packet.PacketBase packet)
        {
            Console.WriteLine("Handling TASK_1 packet.");

            Packet.PacketBase scPacket = new Packet.SC_TASK_1();
            await user.SendMessageAsync(scPacket);
        }

        private static async Task HandleTask2Async(User user, Packet.PacketBase packet)
        {
            Console.WriteLine("Handling TASK_2 packet.");

            Packet.PacketBase scPacket = new Packet.SC_TASK_2();
            await user.SendMessageAsync(scPacket);
        }

        private static void OnClientConnected(TcpClient client)
        {
            Console.WriteLine("Client connected event triggered.");
        }

        private static void OnMessageReceived(TcpClient client, string message)
        {
            Console.WriteLine("Message received event triggered.");
        }

        private static void OnClientDisconnected(TcpClient client)
        {
            Console.WriteLine("Client disconnected event triggered.");
        }
    }
}