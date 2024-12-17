using System.Collections.Concurrent;
using System.Reflection;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Game.Logic.network
{
    public class PacketMethodsManager
    {
        private readonly ConcurrentDictionary<ServerPackets, (MethodInfo, object obj)> serverPacketMethods = new();
        private readonly Dictionary<ClientPackets, Func<ISession, ByteBuffer, Task>> _receivePackets = new();

        //----------------------------------------------------------------------------------------------------------
        public void Register()
        {
            sendPacketRegister(Assembly.LoadFrom("Flatbuffers.Messages.dll"));
            receivePacketRegister(Assembly.GetExecutingAssembly());
        }

        private void receivePacketRegister(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if(type.IsClass == false) continue;
                if (type.GetInterface("Network.Protocol.IPacketMessage.IServerPacketMessage") == null) continue;

                var packetattributes = (ClientPacketMessageAttribute[])type.GetCustomAttributes(typeof(ClientPacketMessageAttribute), true);
                if (packetattributes.Length > 0)
                {
                    var handle = (IServerPacketMessage)Activator.CreateInstance(type);
                    _receivePackets.Add(packetattributes[0].ID, async (client,data) => { await handle.Packet(client,data);});
                }
            }            
        }
        
        // 전송 패킷 등록
        private void sendPacketRegister(Assembly assembly)
        {
            var serverPacketNames = Enum.GetNames(typeof(ServerPackets)).ToHashSet();

            // "NetworkMessage" 네임스페이스 안의 모든 클래스 검색
            var fbsClasses = assembly.GetTypes()
                .Where(t => t.Namespace == "NetworkMessage"
                            && t.Name.EndsWith("_FBS")
                            && serverPacketNames.Contains(t.Name.Substring(0, t.Name.Length - 4)))
                .Select(t => t.Name.Substring(0, t.Name.Length - 4))
                .ToList();

            fbsClasses.ForEach(name =>
            {
                Type? type = assembly.GetType($"NetworkMessage.{name}");
                if (type != null)
                {
                    var packMethod = type.GetMethod("Pack", BindingFlags.Public | BindingFlags.Static);
                    if (packMethod != null)
                    {
                        object? obj = Activator.CreateInstance(type);
                        if (obj != null)
                        {
                            ServerPackets id = (ServerPackets)Enum.Parse(typeof(ServerPackets), name);
                            serverPacketMethods[id] = (packMethod, obj);
                        }
                    }
                }
            });            
        }
        
        //----------------------------------------------------------------------------------------------------------
        public (MethodInfo method, object obj) GetServerPacketType<T>(ServerPackets id, T packet)
        {
            if (serverPacketMethods.TryGetValue(id, out var packFunc))
            {
                return packFunc;
            }

            throw new ArgumentException($"지원되지 않는 메시지 타입입니다: {typeof(T).Name}");
        }

        public void OnReceivePacket(ISession session, ClientPackets id,ByteBuffer buff)
        {
            if (_receivePackets.TryGetValue(id, out var handler) == true)
            {
                _ = handler.Invoke(session, buff);
            }
        }
    }
}