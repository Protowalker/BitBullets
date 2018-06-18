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

        int sequence = 0;

        Dictionary<NetConnection, (int id, int lastAckSequence)> clients = new Dictionary<NetConnection, (int, int)>();
        //32 snapshots are stored. These are the 32 previous gamestates and are used for delta-ing the new gamestates.
        List<Entity>[] snapshots = new List<Entity>[32];

        public NetServerGameState(int port)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("BitBullets");
            config.Port = port;
            config.EnableUPnP = true;
            config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            server = new NetServer(config);

            for(int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new List<Entity>();
            }
        }

        public void Init()
        {
            server.Start();
            if (!server.UPnP.ForwardPort(server.Port, "BitBullets Server")) Console.WriteLine("UPnP not working; port needs to be manually forwarded!");
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

        public override void ProcessMessages()
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
                                SendMessage(playerId, MessageType.NewPlayer, new byte[0], msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                AddPlayer(msg.SenderConnection, playerId);
                                break;
                            case NetConnectionStatus.Disconnected:
                                RemovePlayer(msg.SenderConnection);
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
                            int lastAckSequence = msg.ReadInt32();
                            clients[msg.SenderConnection] = (clients[msg.SenderConnection].id, lastAckSequence);
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
                                case MessageType.HeartBeat:
                                    //This message just gets sent when the client receives a state.
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
            foreach(NetConnection client in clients.Keys)
            {
                SendDeltaState(client);
            }
            snapshots[sequence % 32].Clear();
            foreach(Entity ent in Entities.Values)
            {
                snapshots[sequence % 32].Add(ent.Clone());
            }
            sequence += 1;
        }

        public void AddPlayer(NetConnection connection, int playerId)
        {
            clients.Add(connection, (playerId, sequence));
        }

        public void RemovePlayer(NetConnection connection)
        {
            int id = clients[connection].id;
            Entities.Remove(id);
            quadTree.RemoveItem(id);
            clients.Remove(connection);
        }

        public void SendDeltaState(NetConnection client)
        {
            int lastAckSequence = clients[client].lastAckSequence;

            List<byte> deltaStateMsg = new List<byte>();
            Dictionary<int, Entity> lastAckState = new Dictionary<int, Entity>();

            lastAckState = snapshots[lastAckSequence % 32].ToDictionary(k => k.Id);

            bool hasReset;
            //if the state has lagged behind by more than 32 updates
            if (sequence - lastAckSequence > 32) hasReset = true;
            else hasReset = false;
            //first thing sent is a bool saying whether we have reset the state
            deltaStateMsg.AddRange(BitConverter.GetBytes(hasReset));

            Dictionary<int, NetEntity> netEnts = new Dictionary<int, NetEntity>();

            for (int i = 1; i + lastAckSequence < sequence; i++)
            {
                foreach(Entity ent in snapshots[(lastAckSequence + i) % 32])
                {
                    if (!lastAckState.ContainsKey(ent.Id))
                    {
                        lastAckState.Add(ent.Id, ent.Clone());
                        netEnts.Add(ent.Id, ent.ToNetEntity());
                    }
                    else if(!lastAckState[ent.Id].Equals(ent))
                    {
                        lastAckState[ent.Id] = ent.Clone();
                        if (!netEnts.ContainsKey(ent.Id))
                        {
                            netEnts.Add(ent.Id, ent.ToNetEntity());
                        } else
                        {
                            netEnts[ent.Id] = ent.ToNetEntity();
                        }
                    }
                }
            }

            byte[] data;
            byte[] length;

            //entities
            foreach (Entity ent in Entities.Values)
            {
                if (!lastAckState.ContainsKey(ent.Id))
                {
                    if (!netEnts.ContainsKey(ent.Id))
                    {
                        netEnts.Add(ent.Id, ent.ToNetEntity());
                    } else
                    {
                        netEnts[ent.Id] = ent.ToNetEntity();
                    }
                } else if (!lastAckState.ContainsValue(ent))
                {
                    if (!netEnts.ContainsKey(ent.Id))
                    {
                        netEnts.Add(ent.Id, ent.ToNetEntity());
                    }
                    else
                    {
                        netEnts[ent.Id] = ent.ToNetEntity();
                    }
                }
            }
            foreach (int id in lastAckState.Keys)
            {
                if (!Entities.ContainsKey(id))
                {
                    EntitiesForDestruction.Add(id);
                }
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
            msg.Write(sequence);
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
            msg.Write(sequence);
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
