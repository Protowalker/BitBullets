using DungeonCrawler.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DungeonCrawler.Actions;
using SFML.System;
using Lidgren.Network;
using SFML.Graphics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DungeonCrawler.States;
using DungeonCrawler.Entities;
using MessagePack;

namespace DungeonCrawler.Networking
{
    class NetGameState
    {
        // Delta states are sent in the following order:
        // 3-byte-long quadTree length in bytes
        // quadTree
        // 3-byte-long entities length in bytes
        // entities
        // 3-byte-long entitiesForDestruction length in bytes
        // entitiesForDestruction
        // 3-byte-long realTimeActions length in bytes
        // realTimeActions


        public enum MessageType
        {
            Move,
            Action,
            DeltaState,
            NewPlayer,
            StateRequest
        }
        NetClient client;

        public QuadTree quadTree;

        Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        public Dictionary<int, Entity> Entities { get => entities; private set => entities = value; }

        List<int> entitiesForDestruction = new List<int>();
        public List<int> EntitiesForDestruction { get => entitiesForDestruction; set => entitiesForDestruction = value; }


        ParallelAction realTimeActions = new ParallelAction();
        public ParallelAction RealTimeActions { get => realTimeActions; private set => realTimeActions = value; }

        public static BinaryFormatter binFormatter = new BinaryFormatter();

        public bool connected;
        public bool ready;

        public NetGameState()
        {
            quadTree = new QuadTree(new FloatRect(-10000000, -100000000, 1000000000, 1000000000), 2);

            NetPeerConfiguration config = new NetPeerConfiguration("BitBullets");
            client = new NetClient(config);
            client.Start();
            client.Connect("localhost", 14242);
        }

        public void Update()
        {
            ProcessMessages();

            //realTimeActions.Update(Game.deltaClock.ElapsedTime.AsMilliseconds());
            foreach (Entity ent in Entities.Values)
            {
                ent.Update(Game.deltaClock.ElapsedTime.AsMilliseconds());
            }

            foreach (int i in EntitiesForDestruction)
            {
                entities.Remove(i);
            }
            entitiesForDestruction.Clear();

            Game.deltaClock.Restart();
        }


        protected virtual void ProcessMessages()
        {

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                    {
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            switch (status)
                            {
                                case NetConnectionStatus.Connected:
                                    Console.WriteLine("Connected");
                                    break;
                                case NetConnectionStatus.Disconnected:
                                    break;
                                default:
                                    break;
                            }
                    }
                    break;

                    case NetIncomingMessageType.Data:
                    {
                        MessageType type = (MessageType)msg.ReadByte();
                        int senderId = msg.ReadInt32();
                        switch (type)
                        {
                            case MessageType.NewPlayer:
                                    Console.WriteLine("New player");
                                    Player player = new Player();
                                    player.Init();
                                    ((InGameState)Game.states[Game.currentState]).playerId = msg.ReadInt32();
                                    connected = true;
                                    SendMessage(((InGameState)Game.states[Game.currentState]).playerId, MessageType.StateRequest, new byte[0]);
                                break;

                            case MessageType.DeltaState:
                                    if (!ready) ready = true;
                                    ////Quadtree
                                    //int length = BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    //if (length > 0)
                                    //{
                                    //    QuadTree newTree = MessagePackSerializer.Deserialize<NetQuadTree>(msg.ReadBytes(length)).ToQuadTree();
                                    //    this.quadTree = newTree;
                                    //}
                                    //entities
                                    //quadTree = new QuadTree(new FloatRect(-10000000, -100000000, 1000000000, 1000000000), 2);

                                    int length = BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {
                                        List<NetEntity> netEnts = MessagePackSerializer.Deserialize<List<NetEntity>>(msg.ReadBytes(length));
                                        Dictionary<int, Entity> newEntities = new Dictionary<int, Entity>();
                                        foreach (NetEntity netEnt in netEnts)
                                        {
                                            Entity ent;
                                            switch (netEnt.Type)
                                            {
                                                case Entity.EntityType.Player:
                                                    ent = (Player)netEnt.ToEntity();
                                                    if(ent.Id == ((InGameState)Game.states[Game.currentState]).playerId)
                                                    {
                                                        ent.MoveEvent += new Entity.MoveHandler(OnMove);
                                                    }
                                                    break;
                                                case Entity.EntityType.ShotgunPellet:
                                                    ent = (ShotgunPellet)netEnt.ToEntity();
                                                    break;
                                                case Entity.EntityType.Wall:
                                                    ent = (Wall)netEnt.ToEntity();
                                                    break;
                                                default:
                                                    ent = null;
                                                    Console.WriteLine("Unhandled Entity Type " + netEnt.Type);
                                                    break;
                                            }
                                            if (ent != null)
                                                newEntities.Add(ent.Id, ent);
                                        }
                                        entities = newEntities;
                                    }
                                    //entitiesforDestruction
                                    length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {

                                        List<int> newEntsForDestruction = MessagePackSerializer.Deserialize<List<int>>(msg.ReadBytes(length));
                                        this.entitiesForDestruction = newEntsForDestruction;
                                    }
                                    ////realTimeActions
                                    //length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    //if (length > 0)
                                    //{
                                    //    ParallelAction newRTActions = MessagePackSerializer.Deserialize<ParallelAction>(msg.ReadBytes(length));
                                    //    realTimeActions = newRTActions;
                                    //}
                                    break;
                            default:
                                    Console.WriteLine("Unhandled Message Type: " + type);
                                    break;
                            }
                        }
                        break;
                }
                client.Recycle(msg);
            }
            if (ready)
            {
                SendMessage(((InGameState)Game.states[Game.currentState]).playerId, MessageType.StateRequest, new byte[0]);
            }
        }

        public virtual void SendMessage(int senderId, MessageType type, byte[] data)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            //The Server or client will later use this type to decide how to handle the data.
            msg.Write((byte)type);
            msg.Write(senderId);
            msg.Write(data);
            client.SendMessage(msg, client.ServerConnection, NetDeliveryMethod.UnreliableSequenced);
        }

        public virtual void OnMove(int id, Vector2f delta)
        {
            byte[] deltaBytes = BitConverter.GetBytes(delta.X).Concat(BitConverter.GetBytes(delta.Y)).ToArray();

            SendMessage(id, MessageType.Move, deltaBytes);
        }
    }
}
