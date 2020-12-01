using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    public class SyncVarTestBase
    {
        readonly List<GameObject> spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in spawned)
            {
                GameObject.DestroyImmediate(item);
            }
            spawned.Clear();

            NetworkIdentity.spawned.Clear();
        }


        protected T CreateObject<T>() where T : NetworkBehaviour
        {
            GameObject gameObject = new GameObject();
            spawned.Add(gameObject);

            gameObject.AddComponent<NetworkIdentity>();

            T behaviour = gameObject.AddComponent<T>();
            behaviour.syncInterval = 0f;

            return behaviour;
        }

        protected NetworkIdentity CreateNetworkIdentity(uint netId)
        {
            GameObject gameObject = new GameObject();
            spawned.Add(gameObject);

            NetworkIdentity networkIdentity = gameObject.AddComponent<NetworkIdentity>();
            networkIdentity.netId = netId;
            NetworkIdentity.spawned[netId] = networkIdentity;
            return networkIdentity;
        }

        /// <returns>If data was written by OnSerialize</returns>
        public static bool SyncToClient<T>(T serverObject, T clientObject, bool initialState) where T : NetworkBehaviour
        {
            NetworkWriter writer = new NetworkWriter();
            bool written = serverObject.OnSerialize(writer, initialState);

            NetworkReader reader = new NetworkReader(writer.ToArray());
            clientObject.OnDeserialize(reader, initialState);

            int writeLength = writer.Length;
            int readLength = reader.Position;
            Assert.That(writeLength == readLength, $"OnSerializeAll and OnDeserializeAll calls write the same amount of data\n    writeLength={writeLength}\n    readLength={readLength}");

            return written;
        }
    }
}
