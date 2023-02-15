﻿using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RainMeadow
{
    // Static/singleton class for online features and callbacks
    // is a mainloopprocess so update bound to game update? worth it? idk
    public class OnlineManager : MainLoopProcess {

        public static string CLIENT_KEY = "client";
        public static string CLIENT_VAL = "Meadow_" + RainMeadow.MeadowVersionStr;
        public static string NAME_KEY = "name";
        public static OnlineManager instance;
        internal static Serializer serializer = new Serializer(16000);

        public static Lobby lobby;
        public static CSteamID me;
        public static OnlinePlayer mePlayer;
        public static List<OnlinePlayer> players;
        internal static List<Subscription> subscriptions = new();
        private static Dictionary<string, WorldSession> worldSessions;
        //private static Dictionary<string, RoomSession> roomSessions;

        public OnlineManager(ProcessManager manager) : base(manager, RainMeadow.Ext_ProcessID.OnlineManager)
        {
            instance = this;
            me = SteamUser.GetSteamID();
            mePlayer = new OnlinePlayer(me) { isMe = true, name = SteamFriends.GetPersonaName() };

            framesPerSecond = 20; // alternatively, run as fast as we can for the receiving stuff, but send on a lower tickrate?

            players = new List<OnlinePlayer>() { mePlayer };

            RainMeadow.Debug("OnlineManager Created");
        }

        public override void Update()
        {
            base.Update();
            if(lobby != null)
            {
                mePlayer.tick++;

                // Stuff mePlayer set to itself, events from the distributed lease system
                mePlayer.recentlyAckedEvents.Clear();
                while(mePlayer.OutgoingEvents.Count > 0)
                {
                    var e = mePlayer.OutgoingEvents.Dequeue();
                    mePlayer.recentlyAckedEvents.Add(e);
                    e.Process();
                }

                ReceiveData();

                foreach (var subscription in subscriptions)
                {
                    subscription.Update(mePlayer.tick);
                }

                foreach (var player in players)
                {
                    SendData(player);
                }
            }
        }

        internal static OnlinePlayer PlayerFromId(CSteamID id)
        {
            return players.FirstOrDefault(p => p.id == id);
        }

        internal void ReceiveData()
        {
            lock (serializer)
            {
                int n = 1;
                IntPtr[] messages = new IntPtr[32];

                while (n > 0)
                {
                    n = SteamNetworkingMessages.ReceiveMessagesOnChannel(0, messages, messages.Length);
                    for (int i = 0; i < n; i++)
                    {
                        var message = SteamNetworkingMessage_t.FromIntPtr(messages[i]);
                        var fromPlayer = PlayerFromId(message.m_identityPeer.GetSteamID());
                        if (fromPlayer == null)
                        {
                            RainMeadow.Error("player not found: " + message.m_identityPeer + " " + message.m_identityPeer.GetSteamID());
                            SteamNetworkingMessage_t.Release(messages[i]);
                            continue;
                        }
                        RainMeadow.Debug($"Receiving message from {fromPlayer}");
                        Marshal.Copy(message.m_pData, serializer.buffer, 0, message.m_cbSize);
                        serializer.BeginRead(fromPlayer);

                        serializer.PlayerHeaders();

                        int ne = serializer.BeginReadEvents();
                        RainMeadow.Debug($"Receiving {ne} events");
                        for (int ie = 0; ie < ne; ie++)
                        {
                            ProcessIncomingEvent(serializer.ReadEvent(), fromPlayer);
                        }

                        int ns = serializer.BeginReadStates();
                        RainMeadow.Debug($"Receiving {ns} states");
                        for (int ist = 0; ist < ns; ist++)
                        {
                            ProcessIncomingState(serializer.ReadState(), fromPlayer);
                        }

                        serializer.EndRead();
                        SteamNetworkingMessage_t.Release(messages[i]);
                    }
                }
                
                serializer.Free();
            }
        }

        private void ProcessIncomingEvent(PlayerEvent playerEvent, OnlinePlayer fromPlayer)
        {
            RainMeadow.Debug($"Got event {playerEvent.eventId}:{playerEvent.eventType} from {fromPlayer}");
            fromPlayer.needsAck = true;
            if (IsNewer(playerEvent.eventId, fromPlayer.lastEventFromRemote))
            {
                RainMeadow.Debug($"New event, processing...");
                fromPlayer.lastEventFromRemote = playerEvent.eventId;
                playerEvent.Process();
            }
        }

        public static bool IsNewer(ulong eventId, ulong lastIncomingEvent)
        {
            var delta = eventId - lastIncomingEvent;
            return delta != 0 && delta < ulong.MaxValue / 2;
        }
        public static bool IsNewerOrEqual(ulong eventId, ulong lastIncomingEvent)
        {
            var delta = eventId - lastIncomingEvent;
            return delta < ulong.MaxValue / 2;
        }

        private void ProcessIncomingState(ResourceState resourceState, OnlinePlayer fromPlayer)
        {
            resourceState.resource.ReadState(resourceState, fromPlayer.tick);
        }

        internal void SendData(OnlinePlayer toPlayer)
        {
            if(toPlayer.needsAck || toPlayer.OutgoingEvents.Any() || toPlayer.OutgoingStates.Any())
            {
                RainMeadow.Debug($"Sending message to {toPlayer}");
                lock (serializer)
                {
                    serializer.BeginWrite(toPlayer);

                    serializer.PlayerHeaders();

                    serializer.BeginWriteEvents();
                    RainMeadow.Debug($"Writing {toPlayer.OutgoingEvents.Count} events");
                    foreach (var e in toPlayer.OutgoingEvents)
                    {
                        if (!serializer.CanFit(e)) throw new IOException("no buffer space for events");
                        serializer.WriteEvent(e);
                    }
                    serializer.EndWriteEvents();

                    serializer.BeginWriteStates();
                    RainMeadow.Debug($"Writing {toPlayer.OutgoingStates.Count} states");
                    while (toPlayer.OutgoingStates.Count > 0 && serializer.CanFit(toPlayer.OutgoingStates.Peek()))
                    {
                        var s = toPlayer.OutgoingStates.Dequeue();
                        serializer.WriteState(s);
                    }
                    // todo handle states overflow, planing a packet for maximum size and least stale states
                    serializer.EndWriteStates();

                    serializer.EndWrite();

                    unsafe
                    {
                        fixed (byte* ptr = serializer.buffer)
                        {
                            SteamNetworkingMessages.SendMessageToUser(ref toPlayer.oid, (IntPtr)ptr, (uint)serializer.Position, Constants.k_nSteamNetworkingSend_UnreliableNoDelay, 0);
                        }
                    }

                    serializer.Free();
                }
            }
        }

        internal static void AddSubscription(OnlineResource onlineResource, OnlinePlayer player)
        {
            subscriptions.Add(new Subscription(onlineResource, player));
        }

        internal static void RemoveSubscription(OnlineResource onlineResource, OnlinePlayer player)
        {
            subscriptions.RemoveAll(s => s.onlineResource == onlineResource && s.player == player);
        }

        internal static void RemoveSubscriptions(OnlineResource onlineResource)
        {
            subscriptions.RemoveAll(s => s.onlineResource == onlineResource);
        }

        internal static OnlineResource ResourceFromIdentifier(string rid)
        {
            if (rid == ".") return lobby;
            if (rid.Length == 2 && worldSessions.TryGetValue(rid, out var r2)) return r2;
            //if (roomSessions.TryGetValue(rid, out var r3)) return r3;
            return null;
        }
    }
}