using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public struct OnMinespaceshipReachDestination {
        public int WinningTeam;
    }
    public class SpaceMinecartViewController : AbstractNetworkedController<Mikrocosmos>, IHaveMomentumViewController, ICanSendEvent {
        public IHaveMomentum Model { get; protected set; }

        private Seeker pathfiner;
        private AILerp aiPath;

        private Vector2[] destinations = new Vector2[2];
        private StrangeMeteorTrigger trigger;
        private SpaceMinecartModel model;

        [SerializeField] private List<int> teamPlayersInRange = new List<int>() { 0, 0 };
        [SerializeField] private GameObject mapPointer;

        [SerializeField] private float mapPathSpeed = 1f;
        [SerializeField] private float bigMapPathSpeed = 0.4f;

        [SerializeField]
        private bool reachDestination = false;

        private Animator rightAnimator;
        private Animator leftAnimator;

        
        private Vector3 lastPosition;
        [SerializeField]
        private int previousOccupiedTeam = -1;

        [SerializeField] private GameObject gamePathLinePrefab;
        [SerializeField] private GameObject mapPathLinePrefab;

        private LineRenderer gamePathLine;
        private LineRenderer mapPathLine;

        [SerializeField] private List<Color> teamColors;

        private bool canMove = true;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private List<Vector3> previousPath = new List<Vector3>();

        private void Awake() {
            Model = GetComponent<SpaceMinecartModel>();
            pathfiner = GetComponent<Seeker>();
            aiPath = GetComponent<AILerp>();
            model = GetComponent<SpaceMinecartModel>();
            trigger = GetComponentInChildren<StrangeMeteorTrigger>();
            transform.localScale = Vector3.zero;
            rightAnimator = transform.Find("Right").GetComponent<Animator>();
            leftAnimator = transform.Find("Left").GetComponent<Animator>();
            transform.DOScale(2 * Vector3.one, 0.5f);
            gamePathLine = Instantiate(gamePathLinePrefab).GetComponent<LineRenderer>();
            mapPathLine = Instantiate(mapPathLinePrefab).GetComponent<LineRenderer>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            PointerManager.Singleton.OnClientAddOrUpdatePointer(new OnClientAddOrUpdatePointer() {
                IsActive = true,
                PointerFollowing = gameObject,
                PointerName = "SpaceMinecart",
                PointerPrefab = mapPointer
            });
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            PointerManager.Singleton.OnClientRemovePointer(new OnClientRemovePointer()
            {
                PointerName = "SpaceMinecart"
            });
        }


      
        
        private void OnPlayerExitTrigger(PlayerSpaceship obj)
        {
            teamPlayersInRange[obj.ThisSpaceshipTeam - 1]--;
            OnPlayerNumberUpdate();
        }

        private void OnPlayerEnterTrigger(PlayerSpaceship obj)
        {
            teamPlayersInRange[obj.ThisSpaceshipTeam - 1]++;
            OnPlayerNumberUpdate();
        }


        
        //team1: left, team2: right
        [ServerCallback]
        private void OnPlayerNumberUpdate() {
            if (!reachDestination) {
                if (teamPlayersInRange[0] == teamPlayersInRange[1]) {
                    canMove = false;
                    rightAnimator.SetBool("Move", false);
                    leftAnimator.SetBool("Move", false);
                    RpcOnStopMoving();
                }
                else if (teamPlayersInRange[0] != teamPlayersInRange[1]) {
                    canMove = true;
                    rightAnimator.SetBool("Move", true);
                    leftAnimator.SetBool("Move", false);
                    if (teamPlayersInRange[0] > teamPlayersInRange[1]) {
                        if (previousOccupiedTeam != 1 || pathfiner.GetCurrentPath()==null) {
                            previousOccupiedTeam = 1;
                            pathfiner.StartPath(transform.position, destinations[0], OnPathCalculate);
                        }
                        else {
                            RpcOnRecoverMovingSameTeam(previousOccupiedTeam);
                        }
                    }
                    else if (teamPlayersInRange[0] < teamPlayersInRange[1] || pathfiner.GetCurrentPath() == null) {
                        rightAnimator.SetBool("Move", false);
                        leftAnimator.SetBool("Move", true);
                        if (previousOccupiedTeam != 2) {
                            previousOccupiedTeam = 2;
                           
                            pathfiner.StartPath(transform.position, destinations[1], OnPathCalculate);
                        }else{
                            RpcOnRecoverMovingSameTeam(previousOccupiedTeam);
                        }
                    }
                }
            }
           
        }

       


        private void FixedUpdate() {
            if (isServer) {
                if (canMove) {
                    aiPath.speed = Mathf.Lerp(aiPath.speed, model.MaxSpeed, 2f * Time.fixedDeltaTime);
                    Vector3 positionOffset = transform.position - lastPosition;
                    lastPosition = transform.position;

                    //get the angle of the offset
                    float angle = Mathf.Atan2(positionOffset.y, positionOffset.x) * Mathf.Rad2Deg;

                    Debug.Log($"Angle: {angle}");
                    if ((angle > 90 && angle <= 180))
                    {
                        angle -= 180;
                    }else if ((angle < -90 && angle >= -180)) {
                        angle += 180;
                    }

                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle),
                         2f * Time.deltaTime);
                }
                else {
                    aiPath.speed = Mathf.Lerp(aiPath.speed, 0, 2f * Time.fixedDeltaTime);                    
                }
               
            }
        }

        private void Update() {
            if (gamePathLine) {
                gamePathLine.material.SetTextureOffset(MainTex,
                    gamePathLine.material.GetTextureOffset(MainTex) +
                    new Vector2(mapPathSpeed * Time.deltaTime, 0));
            }

            if (mapPathLine) {
                mapPathLine.material.SetTextureOffset(MainTex,
                    mapPathLine.material.GetTextureOffset(MainTex) +
                    new Vector2(bigMapPathSpeed * Time.deltaTime, 0));
            }
        }


        public override void OnStartServer() {
            base.OnStartServer();

            Vector4 borders = this.GetSystem<IGameProgressSystem>().GetGameMapSize();

            destinations[0] = new Vector2(borders.x - 200, Random.Range(borders.z+10, borders.w-10));
            destinations[1] = new Vector2(borders.y + 200, Random.Range(borders.z + 10, borders.w - 10));

            
            aiPath.speed = model.MaxSpeed;
            AstarPath.active.Scan();

            //pathfiner.StartPath(transform.position, destinations[0], OnPathCalculate);

            trigger.OnPlayerEnterTrigger += OnPlayerEnterTrigger;
            trigger.OnPlayerExitTrigger += OnPlayerExitTrigger;
        }

        private void OnCollisionEnter2D(Collision2D col) {
            if (isServer && col.collider.gameObject.CompareTag("Border")) {
                reachDestination = true;
                canMove = true;
                //end
                this.GetSystem<ITimeSystem>().AddDelayTask(4f, () => {
                    if (this) {
                        this.SendEvent<OnMinespaceshipReachDestination>(new OnMinespaceshipReachDestination()
                        {
                            WinningTeam = previousOccupiedTeam
                        });
                        ClientMessagerForDestroyedObjects.Singleton.ServerSpawnParticleOnClient(transform.position, 0);
                        NetworkServer.Destroy(gameObject);
                    }
                });
            }
            
        }


        private void OnPathCalculate(Path path) {
            if (isServer) {
                RpcOnPathCreate(path.vectorPath, previousOccupiedTeam);
            }
           
        }

        public override void OnStopServer() {
            base.OnStopServer();
            pathfiner.pathCallback -= OnPathCalculate;
            trigger.OnPlayerEnterTrigger -= OnPlayerEnterTrigger;
            trigger.OnPlayerExitTrigger -= OnPlayerExitTrigger;

        }

        private void OnDestroy() {
            if (isClient) {
                Destroy(gamePathLine.gameObject);
                Destroy(mapPathLine.gameObject);
            }
         
        }

        [ClientRpc]        
        private void RpcOnStopMoving() {
            var startColor = gamePathLine.startColor;
            
            Color targetColor = new Color(startColor.a, startColor.g,
                startColor.b, 0);

            gamePathLine.DOColor(new Color2(startColor, gamePathLine.endColor),
                new Color2(targetColor, targetColor), 0.5f);

            mapPathLine.DOColor(new Color2(mapPathLine.startColor, mapPathLine.endColor),
                new Color2(targetColor, targetColor), 0.5f);
        }

        [ClientRpc]
        private void RpcOnRecoverMovingSameTeam(int team) {
            var startColor = gamePathLine.startColor;

            Color targetColor = teamColors[team - 1];

            gamePathLine.DOColor(new Color2(startColor, gamePathLine.endColor),
                new Color2(targetColor, targetColor), 0.5f);

            mapPathLine.DOColor(new Color2(mapPathLine.startColor, mapPathLine.endColor),
                new Color2(targetColor, targetColor), 0.5f);
        }

        [ClientRpc]
        private void RpcOnPathCreate(List<Vector3> paths, int teamSwitchedTo) {
           
            gamePathLine.positionCount = previousPath.Count + paths.Count;
            paths.Reverse();
            List<Vector3> newList = new List<Vector3>();
            newList.AddRange(previousPath);
            foreach (var path in paths) {
                newList.Insert(0, path);
            }
            
            gamePathLine.SetPositions(newList.ToArray());


            paths.Reverse();
            previousPath = paths;
            Color targetColor = teamColors[teamSwitchedTo - 1];
            gamePathLine.DOColor(new Color2(gamePathLine.startColor, gamePathLine.endColor), new Color2(
                targetColor,
                targetColor), 0.5f);


            mapPathLine.positionCount = paths.Count;
            mapPathLine.SetPositions(paths.ToArray());

            mapPathLine.DOColor(new Color2(mapPathLine.startColor, mapPathLine.endColor), new Color2(
                targetColor, targetColor), 0.5f);

        }
    }
}
