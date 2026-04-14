using OWML.Common;//
using OWML.ModHelper;//
using System.Collections.Generic;//
using UnityEngine;//

namespace FlyAllShips {
    public class FlyAllShips : ModBehaviour {
        public static FlyAllShips Instance;

        Transform titleShip, titleShipParent;
        readonly GameObject[] titleThrusters = new GameObject[8];
        float initAltitude = 0;
        Vector3 titleShipSpeed = Vector3.zero;

        Transform thPlanet;
        PlayerSpawner playerSpawner;
        Transform player;
        Transform ship;
        string insideSomeShip = "";
        ScreenPrompt enterShipPrompt;
        readonly Dictionary<string, (Transform transform, Vector3 offset, Vector3 rotation)> shipDict = [];

        public void Awake() {
            Instance = this;
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        public void Start() {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(FlyAllShips)} is loaded!", MessageType.Success);

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene) {
            if(newScene == OWScene.TitleScreen) {
                ModHelper.Console.WriteLine("Loaded into title screen!", MessageType.Success);
                titleShip = GameObject.Find("Structure_HEA_PlayerShip_v4_NearProxy").transform;
                titleShipParent = new GameObject("TitleShipParent").transform;
                titleShipParent.SetParent(titleShip.parent);
                titleShipParent.localPosition = Vector3.zero;
                titleShip.SetParent(titleShipParent);
                initAltitude = (titleShip.position - titleShipParent.position).magnitude;
                //Forward thrusters
                titleThrusters[0] = Instantiate(GameObject.Find("Campfire_Flames"), titleShip);
                titleThrusters[0].transform.localScale = new Vector3(0.5f, 1, 0.5f);//R
                titleThrusters[0].transform.localPosition = new Vector3(5.4f, 6.1f, -2.1f);
                titleThrusters[0].transform.localEulerAngles = new Vector3(270, 0, 0);
                titleThrusters[1] = Instantiate(titleThrusters[0], titleShip);//L
                titleThrusters[1].transform.localPosition = new Vector3(-5.4f, 6.1f, -2.1f);
                titleThrusters[1].transform.localEulerAngles = new Vector3(270, 0, 0);
                //Backward thrusters
                titleThrusters[2] = Instantiate(titleThrusters[0], titleShip);
                titleThrusters[2].transform.localPosition = new Vector3(5.4f, 6.1f, 1.9f);
                titleThrusters[2].transform.localEulerAngles = new Vector3(90, 0, 0);
                titleThrusters[3] = Instantiate(titleThrusters[0], titleShip);
                titleThrusters[3].transform.localPosition = new Vector3(-5.4f, 6.1f, 1.8f);
                titleThrusters[3].transform.localEulerAngles = new Vector3(90, 0, 0);
                //Upward thrusters
                titleThrusters[4] = Instantiate(titleThrusters[0], titleShip);
                titleThrusters[4].transform.localPosition = new Vector3(5.4f, 4.2f, -0.1f);
                titleThrusters[4].transform.localEulerAngles = new Vector3(0, 180, 180);
                titleThrusters[5] = Instantiate(titleThrusters[0], titleShip);
                titleThrusters[5].transform.localPosition = new Vector3(-5.4f, 4.1f, -0.2f);
                titleThrusters[5].transform.localEulerAngles = new Vector3(0, 180, 180);
                //Downward thrusters
                titleThrusters[6] = Instantiate(titleThrusters[0], titleShip);
                titleThrusters[6].transform.localPosition = new Vector3(5.4f, 8, -0.1f);
                titleThrusters[6].transform.localEulerAngles = new Vector3(0, 0, 0);
                titleThrusters[7] = Instantiate(titleThrusters[0], titleShip);
                titleThrusters[7].transform.localPosition = new Vector3(-5.4f, 8, -0.2f);
                titleThrusters[7].transform.localEulerAngles = new Vector3(0, 0, 0);
                /*/Left thrusters
                newFlame = Instantiate(thrusters, titleShip).transform;
                newFlame.localPosition = new Vector3(7.2f, 6.1f, -0.1f);
                newFlame.localEulerAngles = new Vector3(0, 0, 270);
                newFlame.parent = titleThrusters[4].transform;
                //Right thrusters
                newFlame = Instantiate(thrusters, titleShip).transform;
                newFlame.localPosition = new Vector3(-7.3f, 6.1f, -0.2f);
                newFlame.localEulerAngles = new Vector3(0, 0, 90);
                newFlame.parent = titleThrusters[5].transform;*/
            } else if(newScene == OWScene.SolarSystem) {
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

                thPlanet = GameObject.Find("TimberHearth_Body").transform;
                player = GameObject.Find("Player_Body/PlayerCamera").transform;
                ship = GameObject.Find("Ship_Body").transform;
                enterShipPrompt = new ScreenPrompt(InputLibrary.interact, "Enter ship");

                void AddShip(string shipName, Transform shipTr, Vector3 offset, Vector3 rotation) {
                    //ADD OWrb
                    shipTr.gameObject.AddComponent<OWRigidbody>();
                    shipTr.GetComponent<Rigidbody>().isKinematic = true;
                    //ADD reference point
                    GameObject attachPoint = new("AttachPoint");
                    attachPoint.transform.parent = shipTr;
                    attachPoint.transform.localPosition = offset;
                    //ADD interact volume
                    SphereCollider collider = attachPoint.AddComponent<SphereCollider>();
                    collider.isTrigger = true;
                    collider.radius = 5;
                    ModHelper.Events.Unity.FireInNUpdates(() => { SuspendBodyAtPosition(shipTr); collider.enabled = true; }, 10);
                    OWCollider attachCol = attachPoint.AddComponent<OWCollider>();
                    attachCol._collider = collider;
                    InteractReceiver interacVol = attachPoint.AddComponent<InteractReceiver>();
                    interacVol._interactRange = 5;
                    interacVol._maxViewAngle = 180;
                    interacVol._owCollider = attachCol;
                    interacVol._usableInShip = false;
                    interacVol._screenPrompt = enterShipPrompt;
                    //ADD interact events
                    interacVol.OnGainFocus += () => { enterShipPrompt._text = $"Enter {shipName}"; enterShipPrompt.SetVisibility(true); };
                    interacVol.OnLoseFocus += () => { enterShipPrompt.SetVisibility(false); };
                    interacVol.OnPressInteract += () => {
                        if(insideSomeShip == "") {
                                ship.position = shipTr.TransformPoint(offset);
                                ship.rotation = shipTr.rotation;
                                ship.localEulerAngles += rotation;
                                shipTr.gameObject.SetActive(false);
                                playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint(SpawnLocation.Ship));
                                ship.GetComponentInChildren<ShipCockpitController>().OnPressInteract();
                                insideSomeShip = shipName;
                        }
                    };
                    if(shipDict.ContainsKey(shipName)) ModHelper.Console.WriteLine($"FlyAllShips: {shipName} already in shipDict", MessageType.Warning);
                    else shipDict.Add(shipName, (shipTr, offset, rotation));
                }
                Transform shipTransform;
                shipTransform = GameObject.Find("Moon_Body/Sector_THM/Geo_THM/OtherComponentsGroup/ControlledByProxy_Structures/Structures_THM/BatchedGroup/BatchedMeshRenderers_0").transform;
                ModHelper.Events.Unity.FireInNUpdates(() => {
                    shipTransform.localPosition = new Vector3(9, 33, -29);
                    shipTransform.localEulerAngles = new Vector3(325, 19, 329);
                }, 11);
                AddShip("Esker's ship", shipTransform, new Vector3(44.013f, -11.448f, -49.776f), new Vector3(331.76f, 18.16f, 314.44f));

                ModHelper.Events.Unity.FireInNUpdates(() => {
                    playerSpawner = Locator.GetPlayerBody().GetComponent<PlayerSpawner>();
                    GlobalMessenger.AddListener("ExitFlightConsole", new Callback(this.OnExitFlightConsole));
                    Locator.GetPromptManager().AddScreenPrompt(enterShipPrompt, PromptPosition.Center, false);//TODO why are there two screen prompts???
                }, 70);
            }
        }

        void OnExitFlightConsole() {
            if(insideSomeShip != "") {
                ship.GetComponent<ShipDamageController>().ToggleInvincibility();
                Transform shipTr = shipDict[insideSomeShip].transform;
                ModHelper.Events.Unity.FireInNUpdates(() => {
                    shipTr.rotation = ship.rotation * Quaternion.Inverse(Quaternion.Euler(shipDict[insideSomeShip].rotation));
                    shipTr.position = ship.position - shipTr.TransformVector(shipDict[insideSomeShip].offset);

                    player.parent.parent = null;
                    GlobalMessenger.FireEvent("ExitShip");
                    //TODO: Need to fire event player entered sector
                    
                    ship.GetComponent<ShipBody>().SetPosition(thPlanet.position + thPlanet.TransformVector(new Vector3(-16.39f, -52.62f, 227.28f)));
                    //ship.position = thPlanet.position + thPlanet.TransformVector(new Vector3(-16.39f, -52.62f, 227.28f));
                    //ship.localEulerAngles = thPlanet.TransformDirection(new Vector3(282.62f, 1.54f, 175.17f));*/

                    SuspendBodyAtPosition(shipTr);

                    shipTr.gameObject.SetActive(true);
                    //shipTr.Find("AttachPoint").GetComponent<SphereCollider>().enabled = true;
                    insideSomeShip = "";
                    ship.GetComponent<ShipDamageController>().ToggleInvincibility();
                }, 30);
            }
        }

        void SuspendBodyAtPosition(Transform shipToSuspend) {
            Vector3 worldPos = shipToSuspend.position;
            Collider[] cols = Physics.OverlapSphere(worldPos, 100f, OWLayerMask.closeRangeRFMask | OWLayerMask.longRangeRFMask); //tous les colliders autour
            ReferenceFrameVolume bestVol = null;
            float bestVolDist = float.MaxValue;
            OWRigidbody bestOw = null;
            float bestOwDist = float.MaxValue;
            ReferenceFrameVolume vol;
            float dist;

            for(int i = 0; i < cols.Length; i++) {
                vol = cols[i].GetComponent<ReferenceFrameVolume>();
                if(vol != null) {
                    dist = Vector3.Distance(worldPos, vol.transform.position);
                    if(dist < bestVolDist) {
                        bestVolDist = dist;
                        bestVol = vol;
                    }
                } else if(cols[i].attachedRigidbody != null) { //fallback OWRigidbody
                    var ow = cols[i].attachedRigidbody.GetComponent<OWRigidbody>();
                    if(ow != null) {
                        dist = Vector3.Distance(worldPos, ow.GetPosition());
                        if(dist < bestOwDist) {
                            bestOwDist = dist;
                            bestOw = ow;
                        }
                    }
                }
            }
            ReferenceFrame rf = bestVol?.GetReferenceFrame();
            if(rf != null) bestOw = rf.GetOWRigidBody();
            else if(bestOw == null) {
                ModHelper.Console.WriteLine($"FlyAllShips: no ReferenceFrameVolume/OWRigidbody found at {ship.position}", MessageType.Warning);
                return;
            }
            shipToSuspend.GetComponent<OWRigidbody>().Suspend(bestOw);
        }

        void Update() {
            if(LoadManager.GetCurrentScene() == OWScene.TitleScreen) {
                for(int i = 0; i < 8; i++) titleThrusters[i].SetActive(false);
                if(OWInput.IsPressed(InputLibrary.up)) {
                    titleShipSpeed.z += Time.deltaTime * 10;
                    titleThrusters[0].SetActive(true);
                    titleThrusters[1].SetActive(true);
                }
                if(OWInput.IsPressed(InputLibrary.down)) {
                    titleShipSpeed.z -= Time.deltaTime * 10;
                    titleThrusters[2].SetActive(true);
                    titleThrusters[3].SetActive(true);
                }
                if(OWInput.IsPressed(InputLibrary.thrustUp)) {
                    titleShipSpeed.y += Time.deltaTime * 10;
                    titleThrusters[4].SetActive(true);
                    titleThrusters[5].SetActive(true);
                }
                if(OWInput.IsPressed(InputLibrary.thrustDown)) {
                    titleShipSpeed.y -= Time.deltaTime * 10;
                    titleThrusters[6].SetActive(true);
                    titleThrusters[7].SetActive(true);
                }
                if(OWInput.IsPressed(InputLibrary.left)) {
                    titleShipSpeed.x -= Time.deltaTime * 10;
                    titleThrusters[0].SetActive(true);
                    titleThrusters[3].SetActive(true);
                }
                if(OWInput.IsPressed(InputLibrary.right)) {
                    titleShipSpeed.x += Time.deltaTime * 10;
                    titleThrusters[1].SetActive(true);
                    titleThrusters[2].SetActive(true);
                }
                Vector3 upward = titleShip.position - titleShipParent.position;
                if(upward.magnitude > initAltitude || titleShipSpeed.y > 0) {
                    titleShipSpeed.y -= Time.deltaTime * 2;//simulate gravity
                    titleShipParent.RotateAround(titleShipParent.position, titleShip.TransformDirection(Vector3.right), Time.deltaTime * titleShipSpeed.z);
                    titleShipParent.RotateAround(titleShipParent.position, upward, Time.deltaTime * titleShipSpeed.x);
                    titleShip.position += upward.normalized * Time.deltaTime * titleShipSpeed.y;
                } else {
                    titleShipSpeed = Vector3.zero;
                }
            }
        }
        /* old system
        void Update() {
            if(OWInput.IsNewlyPressed(InputLibrary.interact) && !insideSomeShip && eskerShip != null) {
                Vector3 eskerPos = eskerShip.position + eskerShip.TransformVector(new Vector3(45.696f, -10.3141f, -50.6333f)) - player.position;
                float eskerPosMag = eskerPos.magnitude;
                //ModHelper.Console.WriteLine(eskerPos.ToString(), MessageType.Info);
                if((eskerPosMag < 15) && ((eskerPos - player.TransformDirection(Vector3.forward) * 5).magnitude < eskerPosMag)) {
                    //ModHelper.Console.WriteLine("Vise!", MessageType.Info);
                    ship.position = eskerShip.position + eskerShip.TransformVector(new Vector3(44, -11.45f, -50));
                    ship.rotation = eskerShip.rotation;
                    ship.localEulerAngles += new Vector3(331.76f, 18.16f, 314.44f);
                    eskerShip.gameObject.SetActive(false);
                    playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint(SpawnLocation.Ship));
                    ship.GetComponentInChildren<ShipCockpitController>().OnPressInteract();
                    insideSomeShip = true;
                }
            }
            if(insideSomeShip && Locator.GetToolModeSwapper().GetToolMode() == ToolMode.None && OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All)) {
                insideSomeShip = false;
                ModHelper.Events.Unity.FireInNUpdates(() => {
                    eskerShip.position = ship.position + ship.TransformVector(new Vector3(-36, -45, 35));
                    eskerShip.rotation = ship.rotation;
                    eskerShip.localEulerAngles += new Vector3(32.5f, 7, 43);
                    eskerShip.gameObject.SetActive(true);
                    //Collider[] colliders = ship.GetComponentsInChildren<Collider>();
                    //foreach (Collider collider in colliders) {
                    //    collider.enabled = false;
                    //}
                    //Renderer[] renderers = ship.GetComponentsInChildren<Renderer>();
                    //foreach (Renderer renderer in renderers) {
                    //    renderer.enabled = false;
                    //}
                    player.parent.parent = null;
                    GlobalMessenger.FireEvent("ExitShip");
                    jk//TODO: Need to fire event player entered sector
                    ship.GetComponent<ShipBody>().SetPosition(thPlanet.position + thPlanet.TransformVector(new Vector3(-16.39f, -52.62f, 227.28f)));
                    //ship.position = thPlanet.position + thPlanet.TransformVector(new Vector3(-16.39f, -52.62f, 227.28f));
                    //ship.localEulerAngles = thPlanet.TransformDirection(new Vector3(282.62f, 1.54f, 175.17f));
                }, 30);
            }
        }*/
    }

}
