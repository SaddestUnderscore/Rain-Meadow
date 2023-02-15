﻿using System;

namespace RainMeadow
{
    public abstract class ResourceState
    {
        public OnlineResource resource;
        public OnlinePlayer fromPlayer; // not serialized
        internal ulong ts; // not serialized
        public abstract ResourceStateType stateType { get; } // serialized externally
        public virtual long EstimatedSize => resource.SizeOfIdentifier();

        protected ResourceState(OnlineResource resource, ulong ts)
        {
            this.resource = resource;
            this.ts = ts;
        }

        public enum ResourceStateType : byte
        {
            Unknown = 0,
            LobbyState,
        }

        public virtual void CustomSerialize(Serializer serializer)
        {
            serializer.Serialize(ref resource);
        }

        internal static ResourceState NewFromType(ResourceStateType resourceStateType)
        {
            ResourceState s = null;
            switch (resourceStateType)
            {
                case ResourceStateType.Unknown:
                    break;
                case ResourceStateType.LobbyState:
                    s = new Lobby.LobbyState(null, 0);
                    break;
                default:
                    break;
            }
            return s;
        }
    }
}