using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    class MockPlayer : NetworkBehaviour
    {
        public struct Guild
        {
            public string name;
        }

        [SyncVar]
        public Guild guild;

    }

    class GameObjectSyncVarBehaviour : NetworkBehaviour
    {
        [SyncVar]
        public GameObject value;
    }
    class NetworkIdentitySyncVarBehaviour : NetworkBehaviour
    {
        [SyncVar]
        public NetworkIdentity value;
    }
    class NetworkBehaviourSyncVarBehaviour : NetworkBehaviour
    {
        //[SyncVar]
        //public NetworkBehaviourSyncVarBehaviour value;
    }

    public class SyncVarTest : SyncVarTestBase
    {
        [Test]
        public void TestSettingStruct()
        {

            GameObject gameObject = new GameObject();

            MockPlayer player = gameObject.AddComponent<MockPlayer>();

            // synchronize immediatelly
            player.syncInterval = 0f;

            Assert.That(player.IsDirty(), Is.False, "First time object should not be dirty");

            MockPlayer.Guild myGuild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            player.guild = myGuild;

            Assert.That(player.IsDirty(), "Setting struct should mark object as dirty");
            player.ClearAllDirtyBits();
            Assert.That(player.IsDirty(), Is.False, "ClearAllDirtyBits() should clear dirty flag");

            // clearing the guild should set dirty bit too
            player.guild = default;
            Assert.That(player.IsDirty(), "Clearing struct should mark object as dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearDirtyComponents()
        {

            GameObject gameObject = new GameObject();

            MockPlayer player = gameObject.AddComponent<MockPlayer>();
            player.lastSyncTime = Time.time;
            // synchronize immediately
            player.syncInterval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.IsDirty(), Is.False, "Sync interval not met, so not dirty yet");

            // ClearDirtyComponents should do nothing since syncInterval is not
            // elapsed yet
            player.netIdentity.ClearDirtyComponentsDirtyBits();

            // set lastSyncTime far enough back to be ready for syncing
            player.lastSyncTime = Time.time - player.syncInterval;

            // should be dirty now
            Assert.That(player.IsDirty(), Is.True, "Sync interval met, should be dirty");
        }

        [Test]
        public void TestSyncIntervalAndClearAllComponents()
        {

            GameObject gameObject = new GameObject();

            MockPlayer player = gameObject.AddComponent<MockPlayer>();
            player.lastSyncTime = Time.time;
            // synchronize immediately
            player.syncInterval = 1f;

            player.guild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };

            Assert.That(player.IsDirty(), Is.False, "Sync interval not met, so not dirty yet");

            // ClearAllComponents should clear dirty even if syncInterval not
            // elapsed yet
            player.netIdentity.ClearAllComponentsDirtyBits();

            // set lastSyncTime far enough back to be ready for syncing
            player.lastSyncTime = Time.time - player.syncInterval;

            // should be dirty now
            Assert.That(player.IsDirty(), Is.False, "Sync interval met, should still not be dirty");
        }

        [Test]
        public void TestSynchronizingObjects()
        {
            // set up a "server" object
            GameObject gameObject1 = new GameObject();
            NetworkIdentity identity1 = gameObject1.AddComponent<NetworkIdentity>();
            MockPlayer player1 = gameObject1.AddComponent<MockPlayer>();
            MockPlayer.Guild myGuild = new MockPlayer.Guild
            {
                name = "Back street boys"
            };
            player1.guild = myGuild;

            // serialize all the data as we would for the network
            NetworkWriter ownerWriter = new NetworkWriter();
            // not really used in this Test
            NetworkWriter observersWriter = new NetworkWriter();
            identity1.OnSerializeAllSafely(true, ownerWriter, out int ownerWritten, observersWriter, out int observersWritten);

            // set up a "client" object
            GameObject gameObject2 = new GameObject();
            NetworkIdentity identity2 = gameObject2.AddComponent<NetworkIdentity>();
            MockPlayer player2 = gameObject2.AddComponent<MockPlayer>();

            // apply all the data from the server object
            NetworkReader reader = new NetworkReader(ownerWriter.ToArray());
            identity2.OnDeserializeAllSafely(reader, true);

            // check that the syncvars got updated
            Assert.That(player2.guild.name, Is.EqualTo("Back street boys"), "Data should be synchronized");
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SyncsGameobject(bool intialState)
        {
            GameObjectSyncVarBehaviour serverObject = CreateObject<GameObjectSyncVarBehaviour>();
            GameObjectSyncVarBehaviour clientObject = CreateObject<GameObjectSyncVarBehaviour>();

            GameObject serverValue = CreateNetworkIdentity(2044).gameObject;

            serverObject.value = serverValue;
            clientObject.value = null;

            bool written = SyncToClient(serverObject, clientObject, intialState);
            Assert.IsTrue(written);
            Assert.That(clientObject.value, Is.EqualTo(serverValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SyncIdentity(bool intialState)
        {
            NetworkIdentitySyncVarBehaviour serverObject = CreateObject<NetworkIdentitySyncVarBehaviour>();
            NetworkIdentitySyncVarBehaviour clientObject = CreateObject<NetworkIdentitySyncVarBehaviour>();

            NetworkIdentity serverValue = CreateNetworkIdentity(2045);

            serverObject.value = serverValue;
            clientObject.value = null;

            bool written = SyncToClient(serverObject, clientObject, intialState);
            Assert.IsTrue(written);
            Assert.That(clientObject.value, Is.EqualTo(serverValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SyncsBehaviour(bool intialState)
        {
            Assert.Ignore("NotImplemented");
            //NetworkBehaviourSyncVarBehaviour serverObject = CreateObject<NetworkBehaviourSyncVarBehaviour>();
            //NetworkBehaviourSyncVarBehaviour clientObject = CreateObject<NetworkBehaviourSyncVarBehaviour>();

            //NetworkBehaviourSyncVarBehaviour serverValue = CreateNetworkIdentity(2046).gameObject.AddComponent<NetworkBehaviourSyncVarBehaviour>();

            //serverObject.value = serverValue;
            //clientObject.value = null;

            //bool written = SyncToClient(serverObject, clientObject, intialState);
            //Assert.IsTrue(written);
            //Assert.That(clientObject.value, Is.EqualTo(serverValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SyncsMultipleBehaviour(bool intialState)
        {
            Assert.Ignore("NotImplemented");
            //NetworkBehaviourSyncVarBehaviour serverObject = CreateObject<NetworkBehaviourSyncVarBehaviour>();
            //NetworkBehaviourSyncVarBehaviour clientObject = CreateObject<NetworkBehaviourSyncVarBehaviour>();

            //NetworkIdentity identity = CreateNetworkIdentity(2046);
            //NetworkBehaviourSyncVarBehaviour behaviour1 = identity.gameObject.AddComponent<NetworkBehaviourSyncVarBehaviour>();
            //NetworkBehaviourSyncVarBehaviour behaviour2 = identity.gameObject.AddComponent<NetworkBehaviourSyncVarBehaviour>();
            //// create array/set indexs
            //_ = identity.NetworkBehaviours;

            //int index1 = behaviour1.ComponentIndex;
            //int index2 = behaviour2.ComponentIndex;

            //// check components of same type have different indexes
            //Assert.That(index1, Is.Not.EqualTo(index2));

            //// check behaviour 1 can be synced
            //serverObject.value = behaviour1;
            //clientObject.value = null;

            //bool written1 = SyncToClient(serverObject, clientObject, intialState);
            //Assert.IsTrue(written1);
            //Assert.That(clientObject.value, Is.EqualTo(behaviour1));

            //// check that behaviour 2 can be synced
            //serverObject.value = behaviour2;
            //clientObject.value = null;

            //bool written2 = SyncToClient(serverObject, clientObject, intialState);
            //Assert.IsTrue(written2);
            //Assert.That(clientObject.value, Is.EqualTo(behaviour2));
        }
    }
}
