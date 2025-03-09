using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;
internal class Program
{
    // 연결
    public delegate void ConnectedHandler();
    public static event ConnectedHandler Connected;

    // 메시지 전송
    public delegate void MessageSentHandler(string message);
    public static event MessageSentHandler MessageSent;

    // 메시지 수신
    public delegate void MessageReceivedHandler(string message);
    public static event MessageReceivedHandler MessageReceived;

    // 연결 종료
    public delegate void DisconnectedHandler();
    public static event DisconnectedHandler Disconnected;

    private static async Task Main(string[] args)
    {
        // 이벤트 핸들러 등록
        Connected += OnConnected;
        MessageSent += OnMessageSent;
        MessageReceived += OnMessageReceived;
        Disconnected += OnDisconnected;

        await ConnectToServerAsync();

        //if (args.Length < 1 || !int.TryParse(args[0], out int clientCount))
        //{
        //    Console.WriteLine("Usage: Client <number_of_clients>");
        //    clientCount = 1;
        //}

        //Task[] clientTasks = new Task[clientCount];
        //for (int i = 0; i < clientCount; i++)
        //{
        //    clientTasks[i] = ConnectToServerAsync(i + 1);
        //}

        //await Task.WhenAll(clientTasks);

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }

    private static async Task ConnectToServerAsync()
    {
        try
        {
            int port = 52775;
            string server = "127.0.0.1";

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(server, port);
            Console.WriteLine("Connected to server!");
            //Console.WriteLine($"Client {clientId} connected to server!");

            // 서버에 연결되었을 때 이벤트 호출
            Connected?.Invoke();

            using NetworkStream stream = client.GetStream();

            // 메시지 보내고 받기 루프
            while (true)
            {
                // 사용자 입력 받기
                Console.Write("Enter message: ");
                string message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                Packet.PacketBase packet = null;
                switch (message)
                {
                    case "1": {
                            packet = new Packet.CS_TASK_1
                            {
                                IntValue = 1,
                                StrValue = "1",
                                IntListValue = new List<int> { 1, 2, 3 }
                            };
                            break;
                        }
                    case "2":
                        {
                            packet = new Packet.CS_TASK_2
                            {
                                IntListValue = new List<int> { 4, 5, 6 },
                                StrListValue = new List<string> { "4", "5", "6" }
                            };
                            break;
                        }
                    default:
                        {
                            packet = new Packet.CS_PING();
                            break;
                        }
                }
                await SendMessageAsync(stream, packet);

                // 0.1s 대기
                await Task.Delay(100);
            }
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine($"ArgumentNullException: {e}");
        }
        catch (SocketException e)
        {
            Console.WriteLine($"SocketException: {e}");
        }
        finally
        {
            // 종료 이벤트 호출
            Disconnected?.Invoke();
        }
    }

    private static async Task SendMessageAsync(NetworkStream stream, Packet.PacketBase packet)
    {
        // 메시지 전송
        byte[] data = packet.Serialize(packet.PacketType);

        await stream.WriteAsync(data, 0, data.Length);
        Console.WriteLine($"Sent: {packet.GetType().Name}");
        MessageSent?.Invoke(packet.GetType().Name);

        // 응답 수신
        Packet.PacketBase responsePacket = Packet.PacketBase.Deserialize(data);
        Console.WriteLine($"Received: {responsePacket.GetType().Name}");
        MessageReceived?.Invoke(responsePacket.GetType().Name);
    }

    private static void OnConnected()
    {
        Console.WriteLine("Connected event triggered.");
    }

    private static void OnMessageSent(string message)
    {
        Console.WriteLine("Message sent event triggered.");
    }

    private static void OnMessageReceived(string message)
    {
        Console.WriteLine("Message received event triggered.");
    }

    private static void OnDisconnected()
    {
        Console.WriteLine("Disconnected event triggered.");
    }
}