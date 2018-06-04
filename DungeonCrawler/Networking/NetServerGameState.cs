using DungeonCrawler.Characters;
using Lidgren.Network;
using MessagePack;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    class NetServerGameState : NetGameState
    {
        NetServer server;


        //32 snapshots are stored for each player. These are the 32 previous gamestates and are used for delta-ing the new gamestates.
        Dictionary<int, NetGameState[]> snapshots = new Dictionary<int, NetGameState[]>();

        public NetServerGameState(int port)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("BitBullets");
            config.Port = port;
            config.EnableUPnP = true;
            config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);

            server = new NetServer(config);
            server.Start();
            if(!server.UPnP.ForwardPort(port, "BitBullets Server")) Console.WriteLine("UPnP not working; port needs to be manually forwarded!");
            ready = true;
        }

        // Delta states are sent in the following order:
        // 4-byte-long quadTree length in bytes
        // quadTree
        // 4-byte-long entities length in bytes
        // entities
        // 4-byte-long entitiesForDestruction length in bytes
        // entitiesForDestruction
        // 4-byte-long realTimeActions length in bytes
        // realTimeActions

        //Move is stored as follows:
        //Delta
        //2 bytes for X, 2 bytes for Y.

        //Action is stored as follows:

        protected override void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        switch (status)
                        {
                            case NetConnectionStatus.Connected:
                                Console.WriteLine("Connected " + msg.SenderEndPoint);
                                NetOutgoingMessage announceMsg = server.CreateMessage();
                                Player player = new Player();
                                player.Init();
                                int playerId = player.Id;
                                player.SetCurrentCharacter(new Scout(playerId));
                                announceMsg.Write((byte)MessageType.NewPlayer);
                                announceMsg.Write(-1);
                                announceMsg.Write(playerId);
                                server.SendMessage(announceMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                AddPlayer(playerId);
                                break;
                            case NetConnectionStatus.Disconnected:
                                Console.WriteLine(msg.SenderConnection + " has Disconnected");
                                break;
                            default:
                                break;
                        }
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        break;
                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                        Console.WriteLine(msg.SenderConnection.AverageRoundtripTime);
                        break;
                    case NetIncomingMessageType.Data:
                        {
                            MessageType type = (MessageType)msg.ReadByte();
                            int sequence = msg.ReadInt32();
                            int senderId = msg.ReadInt32();
                            switch (type)
                            {
                                case MessageType.Action:
                                    int length = BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    List<Actions.Action> actionList = LZ4MessagePackSerializer.Deserialize<List<Actions.Action>>(msg.ReadBytes(length));
                                    foreach(Actions.Action action in actionList)
                                    {
                                        RealTimeActions.Add(action);
                                    }
                                    break;
                                case MessageType.StateRequest:
                                    SendDeltaState(msg.SenderConnection);
                                    break;
                                default:
                                    Console.WriteLine("Unhandled Data Message Type: " + type);
                                    break;
                            }
                            break;
                        }

                    case NetIncomingMessageType.VerboseDebugMessage:
                        break;

                    case NetIncomingMessageType.DebugMessage:
                        break;

                    case NetIncomingMessageType.WarningMessage:
                        break;

                    case NetIncomingMessageType.ErrorMessage:
                        break;

                    default:
                        Console.WriteLine("Unhandled Network message type: " + msg.MessageType);
                        break;
                }
                server.Recycle(msg);
            }
        }

        public void AddPlayer(int playerId)
        {
            snapshots.Add(playerId, new NetGameState[32]);
            for(int i = 0; i < snapshots[playerId].Length; i++)
            {
                snapshots[playerId][i] = new NetGameState();
            }
        }

        public void RemovePlayer(int playerId)
        {
            snapshots.Remove(playerId);
        }

        public void SendDeltaState(NetConnection client)
        {
            List<byte> deltaStateMsg = new List<byte>();

            byte[] data;
            byte[] length;

            ////quadtree
            //NetQuadTree netTree = quadTree.ToNetQuadTree();
            //data = MessagePackSerializer.Serialize(netTree);
            //length = BitConverter.GetBytes(data.Length);

            //deltaStateMsg.AddRange(length);
            //deltaStateMsg.AddRange(data);

            //entities
            List<NetEntity> netEnts = new List<NetEntity>();
            foreach (Entity ent in Entities.Values)
            {
                netEnts.Add(ent.ToNetEntity());
            }

            data = LZ4MessagePackSerializer.Serialize(netEnts);
            length = BitConverter.GetBytes(data.Length);
            

            deltaStateMsg.AddRange(length);
            deltaStateMsg.AddRange(data);

            //entitiesForDestruction

            data = LZ4MessagePackSerializer.Serialize(EntitiesForDestruction);
            length = BitConverter.GetBytes(data.Length);

            deltaStateMsg.AddRange(length);
            deltaStateMsg.AddRange(data);

            //realTimeActions

            data = LZ4MessagePackSerializer.Serialize(RealTimeActions);
            length = BitConverter.GetBytes(data.Length);

            deltaStateMsg.AddRange(length);
            deltaStateMsg.AddRange(data);

            SendMessage(-1, MessageType.DeltaState, deltaStateMsg.ToArray(), client, NetDeliveryMethod.ReliableSequenced);
        }

        public override void SendMessage(int id, MessageType type, byte[] data, NetDeliveryMethod method)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((byte)type);
            msg.Write(id);
            msg.Write(data);
            switch (type) {
                default:
                    server.SendToAll(msg, method);
                    break;
            }
        }

        public void SendMessage(int id, MessageType type, byte[] data, NetConnection client, NetDeliveryMethod method)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((byte)type);
            msg.Write(id);
            msg.Write(data);
            switch (type)
            {
                default:
                    server.SendMessage(msg, client, method);
                    break;
            }
        }
    }
}
