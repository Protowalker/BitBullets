using Lidgren.Network;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    class NetServerGameState : NetGameState
    {
        NetServer server;



        //32 snapshots are stored for each player. These are the 32 previous gamestates and are used for delta-ing the new gamestates.
        Dictionary<int, NetGameState[]> snapshots = new Dictionary<int, NetGameState[]>();

        public NetServerGameState()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("BitBullets");
            config.Port = 14242;
            server = new NetServer(config);
            server.Start();
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
                                announceMsg.Write((byte)MessageType.NewPlayer);
                                announceMsg.Write(-1);
                                announceMsg.Write(playerId);
                                server.SendMessage(announceMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                //AddPlayer(playerId);
                                break;
                            case NetConnectionStatus.Disconnected:
                                break;
                            default:
                                break;
                        }
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        break;

                    case NetIncomingMessageType.Data:
                        {
                            MessageType type = (MessageType)msg.ReadByte();
                            int senderId = msg.ReadInt32();
                            switch (type)
                            {
                                case MessageType.Move:
                                    Vector2f moveDelta = new Vector2f
                                    {
                                        X = (float)binFormatter.Deserialize(new MemoryStream(msg.ReadBytes(2))),
                                        Y = (float)binFormatter.Deserialize(new MemoryStream(msg.ReadBytes(2)))
                                    };
                                    Entities[senderId].Move(moveDelta);
                                    break;
                                case MessageType.Action:
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
            }
            server.Recycle(msg);

            List<byte> deltaStateMsg = new List<byte>();

            MemoryStream mStream = new MemoryStream();
            byte[] length;

            ////quadtree
            //binFormatter.Serialize(mStream, quadTree);
            //binFormatter.Serialize(lengthMStream, (int)mStream.Length);
            //lengthMStream.SetLength(3);

            //deltaStateMsg.AddRange(lengthMStream.ToArray());
            //deltaStateMsg.AddRange(mStream.ToArray());

            //entities
            List<NetEntity> netEnts = new List<NetEntity>();
            foreach(Entity ent in Entities.Values)
            {
                netEnts.Add(ent.ToNetEntity());
            }

            binFormatter.Serialize(mStream, netEnts);
            length = BitConverter.GetBytes(mStream.Length);

            deltaStateMsg.AddRange(length);
            deltaStateMsg.AddRange(mStream.ToArray());

            //entitiesForDestruction
            mStream = new MemoryStream();

            binFormatter.Serialize(mStream, EntitiesForDestruction);
            length = BitConverter.GetBytes((int)mStream.Length);

            deltaStateMsg.AddRange(length);
            deltaStateMsg.AddRange(mStream.ToArray());

            //realTimeActions
            mStream = new MemoryStream();

            binFormatter.Serialize(mStream, RealTimeActions);
            length = BitConverter.GetBytes((int)mStream.Length);

            deltaStateMsg.AddRange(length);
            deltaStateMsg.AddRange(mStream.ToArray());

            SendMessage(-1, MessageType.DeltaState, deltaStateMsg.ToArray());
        }

        public void AddPlayer(int id)
        {

            snapshots.Add(id, new NetGameState[32]);
            for(int i = 0; i < snapshots[id].Length; i++)
            {
                snapshots[id][i] = new NetGameState();
            }
        }

        public void RemovePlayer(int playerId)
        {
            snapshots.Remove(playerId);
        }

        public override void SendMessage(int id, MessageType type, byte[] data)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((byte)type);
            msg.Write(id);
            msg.Write(data);
            switch (type) {
                default:
                    server.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                    break;
            }
        }

        public new void OnMove(int senderId, Vector2f delta)
        {

        }
    }
}
