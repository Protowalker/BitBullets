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
            NewPlayer
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

            realTimeActions.Update(Game.deltaClock.ElapsedTime.AsMilliseconds());
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
                                    Player player = new Player();
                                    player.Init();
                                    msg.ReadInt32();
                                    ((InGameState)Game.states[Game.currentState]).playerId = player.Id;
                                    Game.states[Game.currentState].Init();
                                    ready = true;
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
                                    int playerId = player.Id;
                                    ((InGameState)Game.states[Game.currentState]).playerId = msg.ReadInt32();
                                break;

                            case MessageType.DeltaState:
                                    Console.WriteLine("DeltaState");
                                    MemoryStream mStream = new MemoryStream();
                                    ////Quadtree
                                    int length;
                                    //if (length > 0)
                                    //{
                                    //    QuadTree newTree = binFormatter.Deserialize(new MemoryStream(msg.ReadBytes(length))) as QuadTree;
                                    //    this.quadTree = newTree;
                                    //}
                                    //entities


                                    length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {
                                        mStream = new MemoryStream(msg.ReadBytes(length));
                                        mStream.Position = 0;

                                        List<NetEntity> netEnts = binFormatter.Deserialize(mStream) as List<NetEntity>;
                                        Dictionary<int, Entity> newEntities = new Dictionary<int, Entity>();
                                        foreach(NetEntity netEnt in netEnts)
                                        {
                                            Entity ent;
                                            switch (netEnt.type)
                                            {
                                                case Entity.EntityType.Player:
                                                    ent = (Player)netEnt.ToEntity();
                                                    break;
                                                case Entity.EntityType.ShotgunPellet:
                                                    ent = (ShotgunPellet)netEnt.ToEntity();
                                                    break;
                                                case Entity.EntityType.Wall:
                                                    ent = (Wall)netEnt.ToEntity();
                                                    break;
                                                default:
                                                    ent = null;
                                                    Console.WriteLine("Unhandled Entity Type " + netEnt.type);
                                                    break;
                                            }
                                            if(ent != null)
                                                newEntities.Add(ent.Id, ent);
                                        }
                                    }
                                    //entitiesforDestruction
                                    length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {
                                        mStream = new MemoryStream(msg.ReadBytes(length));
                                        mStream.Position = 0;
                                        List<int> newEntsForDestruction = binFormatter.Deserialize(mStream) as List<int>;
                                        this.entitiesForDestruction = newEntsForDestruction;
                                    }
                                    //realTimeActions
                                    length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {
                                        mStream = new MemoryStream(msg.ReadBytes(length));
                                        mStream.Position = 0;
                                        ParallelAction newRTActions = binFormatter.Deserialize(mStream) as ParallelAction;
                                        realTimeActions = newRTActions;
                                    }
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

        public void OnMove(int id, Vector2f delta)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] deltaBytes = new byte[4];
            binFormatter.Serialize(mStream, delta.X);
            mStream.SetLength(2);
            deltaBytes[0] = mStream.ToArray()[0];
            deltaBytes[1] = mStream.ToArray()[1];

            binFormatter.Serialize(mStream, delta.Y);
            mStream.SetLength(2);
            deltaBytes[2] = mStream.ToArray()[0];
            deltaBytes[3] = mStream.ToArray()[1];

            SendMessage(id, MessageType.Move, deltaBytes);
        }
    }
}
