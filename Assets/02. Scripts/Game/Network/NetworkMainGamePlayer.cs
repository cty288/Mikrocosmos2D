using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnNetworkedMainGamePlayerConnected {
        public NetworkConnection connection;
    }

    public struct OnClientMainGamePlayerConnected {
        public GameObject playerSpaceship;
    }

    public partial class NetworkMainGamePlayer : AbstractNetworkedController<Mikrocosmos>, ICanSendEvent {
        //[SerializeField] private GameObject spaceshipPrefab;

       

        [SyncVar]
        public PlayerMatchInfo matchInfo = null;

        [SyncVar(hook = nameof(OnControlledSpaceshipChanged))]
        public NetworkIdentity ControlledSpaceship;

        [SerializeField] public List<GameObject> spaceshipPrefabs;
       

        public override void OnStartServer() {
            base.OnStartServer();
            //spawn spaceship
            this.RegisterEvent<OnRoomPlayerJoinGame>(OnRoomPlayerJoinGame)
                .UnRegisterWhenGameObjectDestroyed(gameObject, true);
            this.SendEvent<OnNetworkedMainGamePlayerConnected>(new OnNetworkedMainGamePlayerConnected() {
                connection =  connectionToClient
            });
         
        }
        public override void OnStopServer() {
            base.OnStopServer();
           // this.GetSystem<IRoomMatchSystem>().ServerRoomPlayerLeaveMatch(matchInfo.ID);
        }

        
        private void OnRoomPlayerJoinGame(OnRoomPlayerJoinGame e) {
            if (e.Connection == connectionToClient) {
                matchInfo = e.MatchInfo;
                SpawnSpaceshipForThisPlayer(matchInfo, e.Connection);
              
            }

          
            
         
        }

        

        private void SpawnSpaceshipForThisPlayer(PlayerMatchInfo matchInfo, NetworkConnection conn) {

            GameObject spaceship = Instantiate(spaceshipPrefabs[matchInfo.Team-1], transform.position, Quaternion.identity);
            spaceship.GetComponent<PlayerSpaceship>().SetPlayerDisplayInfo(matchInfo);
            NetworkServer.Spawn(spaceship, conn);
            
            ControlledSpaceship = spaceship.GetComponent<NetworkIdentity>();
        }

        private void OnControlledSpaceshipChanged(NetworkIdentity oldIdentity, NetworkIdentity newIdentity) {
            if (hasAuthority) {
                Mikrocosmos.Interface.SendEvent<OnClientMainGamePlayerConnected>(new OnClientMainGamePlayerConnected()
                {
                    playerSpaceship = newIdentity.gameObject
                });
            }
            
        }
        
    }
}
