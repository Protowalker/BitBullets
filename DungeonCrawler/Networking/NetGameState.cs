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

        int lastAckSequence = 0;

        public enum MessageType
        {
            Action,
            DeltaState,
            NewPlayer,
            HeartBeat
        }
        NetClient client;

        public QuadTree quadTree;

        Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        public Dictionary<int, Entity> Entities { get => entities; private set => entities = value; }

        List<int> entitiesForDestruction = new List<int>();
        public List<int> EntitiesForDestruction { get => entitiesForDestruction; set => entitiesForDestruction = value; }


        ParallelAction realTimeActions = new ParallelAction();
        public ParallelAction RealTimeActions { get => realTimeActions; private set => realTimeActions = value; }

        public List<Actions.Action> actionsForSending = new List<Actions.Action>();

        public bool connected;
        public bool ready;

        public NetGameState()
        {
            quadTree = new QuadTree(new FloatRect(-10000000, -100000000, 1000000000, 1000000000), 2);
        }

        public NetGameState(string host, int port)
        {
            quadTree = new QuadTree(new FloatRect(-10000000, -100000000, 1000000000, 1000000000), 2);

            NetPeerConfiguration config = new NetPeerConfiguration("BitBullets");
            config.SimulatedLoss = 0f;
            client = new NetClient(config);
            client.Start();
            client.Connect(host, port);
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
                        int sequence = msg.ReadInt32();
                        int senderId = msg.ReadInt32();
                        switch (type)
                        {
                            case MessageType.NewPlayer:
                                    Console.WriteLine("New player");
                                    Player player = new Player();
                                    player.Init();
                                    ((InGameState)Game.states[Game.currentState]).playerId = senderId;
                                    connected = true;
                                    
                                    break;

                            case MessageType.DeltaState:
                                    ////Quadtree
                                    //int length = BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    //if (length > 0)
                                    //{
                                    //    QuadTree newTree = MessagePackSerializer.Deserialize<NetQuadTree>(msg.ReadBytes(length)).ToQuadTree();
                                    //    this.quadTree = newTree;
                                    //}
                                    //entities
                                    //quadTree = new QuadTree(new FloatRect(-10000000, -100000000, 1000000000, 1000000000), 2);

                                    bool hasReset = BitConverter.ToBoolean(msg.ReadBytes(1), 0);
                                    Console.WriteLine(hasReset);
                                    int length = BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {
                                        Dictionary<int, NetEntity> netEnts = LZ4MessagePackSerializer.Deserialize<Dictionary<int, NetEntity>>(msg.ReadBytes(length));
                                        Dictionary<int, Entity> newEntities = new Dictionary<int, Entity>();
                                        foreach (NetEntity netEnt in netEnts.Values)
                                        {
                                            Entity ent;
                                            switch (netEnt.Type)
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
                                                    Console.WriteLine("Unhandled Entity Type " + netEnt.Type);
                                                    break;
                                            }
                                            newEntities.Add(ent.Id, ent);
                                        }

                                        if (hasReset)
                                        {
                                            entities = newEntities;
                                        }
                                        else
                                        {
                                            ShiftDeltaState(newEntities);
                                        }
                                        for (int i = 0; i < quadTree.allItems.Count; i++)
                                        {
                                            if (!entities.ContainsKey(quadTree.allItems[i]))
                                            {
                                                quadTree.RemoveItem(quadTree.allItems[i]);
                                            }
                                        }

                                        foreach (Entity ent in entities.Values)
                                        {
                                            if (!quadTree.allItems.Contains(ent.Id))
                                            {
                                                quadTree.Insert(ent.Id);
                                            }
                                        }
                                    }
                                    //entitiesforDestruction
                                    length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    if (length > 0)
                                    {

                                        List<int> newEntsForDestruction = LZ4MessagePackSerializer.Deserialize<List<int>>(msg.ReadBytes(length));
                                        this.entitiesForDestruction = newEntsForDestruction;
                                    }
                                    ////realTimeActions
                                    //length = (int)BitConverter.ToInt32(msg.ReadBytes(4), 0);
                                    //if (length > 0)
                                    //{
                                    //    ParallelAction newRTActions = MessagePackSerializer.Deserialize<ParallelAction>(msg.ReadBytes(length));
                                    //    realTimeActions = newRTActions;
                                    //}

                                    //When we receive a new state, send all the new actions
                                    byte[] data;
                                    byte[] len;
                                    data = LZ4MessagePackSerializer.Serialize(actionsForSending);
                                    len = BitConverter.GetBytes(data.Length);
                                    actionsForSending.Clear();
                                    SendMessage(((InGameState)Game.states[Game.currentState]).playerId, MessageType.Action, len.Concat(data).ToArray(), NetDeliveryMethod.UnreliableSequenced);
                                    if (!ready)
                                    {
                                        Game.states[Game.currentState].Init();
                                        ready = true;
                                    }
                                    lastAckSequence = sequence;
                                    //Tell the server we received the state
                                    SendMessage(((InGameState)Game.states[Game.currentState]).playerId, MessageType.HeartBeat, new byte[0], NetDeliveryMethod.Unreliable);
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

        private void ShiftDeltaState(Dictionary<int, Entity> newEntities)
        {
            foreach (int entId in newEntities.Keys)
            {
                if (newEntities[entId] == null)
                {
                    if (entities.ContainsKey(entId))
                    {
                        entities.Remove(entId);
                    }
                }
                else
                {
                    if (entities.ContainsKey(entId))
                    {
                        entities[entId] = newEntities[entId];
                    }
                    else
                    {
                        entities.Add(entId, newEntities[entId]);
                    }
                }
            }

        }

        public virtual void SendMessage(int senderId, MessageType type, byte[] data, NetDeliveryMethod method)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            //The Server or client will later use this type to decide how to handle the data.
            msg.Write((byte)type);
            msg.Write(lastAckSequence);
            msg.Write(senderId);
            msg.Write(data);
            client.SendMessage(msg, client.ServerConnection, method);
        }
    }
}
