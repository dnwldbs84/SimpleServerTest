using System.Runtime.Serialization;

namespace CommonLibrary
{
    public class Packet
    {
        [DataContract]
        public class PacketBase
        {
            [DataMember]
            public int PacketType { get; set; }

            public PacketBase(int packetType)
            {
                PacketType = packetType;
            }

            public byte[] Serialize(int packetType)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(memoryStream))
                    {
                        // 임시 메모리 스트림에 바디 데이터를 직렬화
                        using (MemoryStream bodyStream = new MemoryStream())
                        {
                            DataContractSerializer serializer = new DataContractSerializer(this.GetType());
                            serializer.WriteObject(bodyStream, this);

                            byte[] bodyData = bodyStream.ToArray();
                            int bodySize = bodyData.Length;

                            // 패킷 타입과 바디 크기를 먼저 씀
                            writer.Write(packetType);
                            writer.Write(bodySize);

                            // 바디 데이터를 씀
                            writer.Write(bodyData);
                        }

                        return memoryStream.ToArray();
                    }
                }
            }
            public static PacketBase Deserialize(byte[] data)
            {
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    using (BinaryReader reader = new BinaryReader(memoryStream))
                    {
                        int packetType = reader.ReadInt32();
                        int bodySize = reader.ReadInt32();

                        // BodySize만큼의 데이터를 읽어들임
                        byte[] bodyData = reader.ReadBytes(bodySize);

                        PacketBase packet = packetType switch
                        {
                            (int)CS.CS_PING => new CS_PING(),
                            (int)CS.CS_TASK_1 => new CS_TASK_1(),
                            (int)CS.CS_TASK_2 => new CS_TASK_2(),
                            (int)SC.SC_PONG => new SC_PONG(),
                            (int)SC.SC_TASK_1 => new SC_TASK_1(),
                            (int)SC.SC_TASK_2 => new SC_TASK_2(),
                            _ => throw new InvalidOperationException("Unknown packet type")
                        };

                        using (MemoryStream bodyStream = new MemoryStream(bodyData))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(packet.GetType());
                            var obj = serializer.ReadObject(bodyStream);
                            foreach (var property in packet.GetType().GetProperties())
                            {
                                property.SetValue(packet, property.GetValue(obj));
                            }
                        }

                        return packet;
                    }
                }
            }
        }
        public enum CS
        {
            CS_PING = 0,
            CS_TASK_1 = 1,
            CS_TASK_2 = 2,
        }
        public enum SC
        {
            SC_PONG = 1001,
            SC_TASK_1 = 1002,
            SC_TASK_2 = 1003,
        }

        [DataContract]
        public class CS_PING : PacketBase
        {
            public CS_PING() : base((int)CS.CS_PING) { }
        }
        [DataContract]
        public class SC_PONG : PacketBase
        {
            public SC_PONG() : base((int)SC.SC_PONG) { }
        }
        [DataContract]
        public class CS_TASK_1 : PacketBase
        {
            [DataMember]
            public int IntValue { get; set; }
            [DataMember]
            public string StrValue { get; set; }
            [DataMember]
            public List<int> IntListValue { get; set; }

            public CS_TASK_1() : base((int)CS.CS_TASK_1) { }
        }
        [DataContract]
        public class SC_TASK_1 : PacketBase
        {
            [DataMember]
            public int Error { get; set; }
            [DataMember]
            public bool Result { get; set; }

            public SC_TASK_1() : base((int)SC.SC_TASK_1) { }
        }
        [DataContract]
        public class CS_TASK_2 : PacketBase
        {
            [DataMember]
            public List<int> IntListValue { get; set; }
            [DataMember]
            public List<string> StrListValue { get; set; }

            public CS_TASK_2() : base((int)CS.CS_TASK_2) { }
        }
        [DataContract]
        public class SC_TASK_2 : PacketBase
        {
            [DataMember]
            public int Error { get; set; }
            [DataMember]
            public bool Result { get; set; }

            public SC_TASK_2() : base((int)SC.SC_TASK_2) { }
        }
    }
}
