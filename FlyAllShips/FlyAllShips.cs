using OWML.Common;//
using OWML.ModHelper;//
using System.Collections.Generic;//
using System.Linq;
using UnityEngine;//

namespace FlyAllShips {
    public class FlyAllShips : ModBehaviour {
        public static FlyAllShips Instance;

        Transform titleShip, titleShipParent;
        readonly GameObject[] titleThrusters = new GameObject[8];
        float initAltitude = 0;
        Vector4 titleShipSpeed = Vector4.zero;

        PlayerSpawner playerSpawner;
        Transform player;
        Transform ship;
        GameObject dummyShip;
        string insideSomeShip = "";
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
                if(titleShipParent == null) titleShipParent = new GameObject("TitleShipParent").transform;
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

                player = GameObject.Find("Player_Body").transform;
                ship = GameObject.Find("Ship_Body").transform;
                OWRigidbody shipOw = ship.GetComponent<OWRigidbody>();
                insideSomeShip = "";
                if(dummyShip == null) {
                    dummyShip = new("Ship_clone");
                    dummyShip.transform.position = ship.position;
                    dummyShip.transform.rotation = ship.rotation;
                    foreach(string childName in new string[] { "Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior", "Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Exterior/SignalDishPivot/SignalDish", "Module_Cabin/Geo_Cabin/Cabin_Geometry/Cabin_Exterior", "Module_Cabin/Geo_Cabin/Cabin_Tech/Cabin_Tech_Exterior", "Module_Supplies/Geo_Supplies/Supplies_Geometry/Supplies_Exterior", "Module_Engine/Geo_Engine/Engine_Geometry/Engine_Exterior", "Module_LandingGear/LandingGear_Front/LandingGear_Front_Tech/LandingCamPivot/LandingCam", "Module_LandingGear/LandingGear_Front/Geo_LandingGear_Front", "Module_LandingGear/LandingGear_Left/Geo_LandingGear_Left", "Module_LandingGear/LandingGear_Right/Geo_LandingGear_Right" })
                        Instantiate(ship.Find(childName), dummyShip.transform, true);
                    dummyShip.transform.position -= 20 * Vector3.up; //prevents hitting true ship
                    dummyShip.gameObject.AddComponent<OWRigidbody>();
                    ModHelper.Events.Unity.FireInNUpdates(() => { SuspendBodyAtPosition(dummyShip.transform); dummyShip.SetActive(false); }, 1);
                }

                void AddShip(string shipName, Transform shipTr, Vector3 offset, Vector3 rotation) {
                    //ADD OWrb
                    Transform targetParent = shipTr.parent;
                    shipTr.gameObject.AddComponent<OWRigidbody>();
                    shipTr.GetComponent<Rigidbody>().isKinematic = true;
                    shipTr.SetParent(targetParent);
                    //ADD reference point
                    GameObject attachPoint = new("AttachPoint");
                    attachPoint.transform.parent = shipTr;
                    attachPoint.transform.localPosition = offset;
                    //ADD interact volume
                    SphereCollider collider = attachPoint.AddComponent<SphereCollider>();
                    collider.isTrigger = true;
                    collider.radius = 5;
                    ModHelper.Events.Unity.FireInNUpdates(() => { collider.enabled = true; }, 12);
                    OWCollider attachCol = attachPoint.AddComponent<OWCollider>();
                    attachCol._collider = collider;
                    InteractReceiver interacVol = attachPoint.AddComponent<InteractReceiver>();
                    interacVol._interactRange = 5;
                    interacVol._maxViewAngle = 180;
                    interacVol._owCollider = attachCol;
                    interacVol._usableInShip = false;
                    interacVol._screenPrompt = new ScreenPrompt(InputLibrary.interact, $"Enter {shipName}");
                    //ADD interact events
                    interacVol.OnPressInteract += () => {
                        if(insideSomeShip == "") {
                            Vector3 worldPos = shipTr.TransformPoint(offset);
                            Quaternion worldRot = shipTr.rotation * Quaternion.Euler(rotation);
                            dummyShip.SetActive(true);
                            dummyShip.transform.position = ship.position;
                            dummyShip.transform.rotation = ship.rotation;
                            SuspendBodyAtPosition(dummyShip.transform);
                            shipOw.WarpToPositionRotation(worldPos, worldRot);
                            shipTr.gameObject.SetActive(false);
                            playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint(SpawnLocation.Ship));
                            ship.GetComponentInChildren<ShipCockpitController>().OnPressInteract();
                            insideSomeShip = shipName;
                        }
                    };
                    if(!shipDict.ContainsKey(shipName)) shipDict.Add(shipName, (shipTr, offset, rotation));
                }
                Transform shipTransform;
                shipTransform = GameObject.Find("Moon_Body/Sector_THM/Geo_THM/OtherComponentsGroup/ControlledByProxy_Structures/Structures_THM/BatchedGroup/BatchedMeshRenderers_0").transform;
                ModHelper.Events.Unity.FireInNUpdates(() => {
                    shipTransform.localPosition = new Vector3(9, 33, -29);
                    shipTransform.localEulerAngles = new Vector3(325, 19, 329);
                    SuspendBodyAtPosition(shipTransform);
                }, 11);
                AddShip("Esker's ship", shipTransform, new Vector3(44.013f, -11.448f, -49.776f), new Vector3(331.76f, 18.16f, 314.44f));

                ModHelper.Events.Unity.FireInNUpdates(() => {
                    playerSpawner = Locator.GetPlayerBody().GetComponent<PlayerSpawner>();
                    GlobalMessenger.AddListener("ExitFlightConsole", new Callback(this.OnExitFlightConsole));
                }, 70);
            }
        }

        void OnExitFlightConsole() {
            if(insideSomeShip != "") {
                GlobalMessenger.FireEvent("ExitShip");
                //Move fake ship to real ship
                Transform shipTr = shipDict[insideSomeShip].transform;
                shipTr.rotation = ship.rotation * Quaternion.Inverse(Quaternion.Euler(shipDict[insideSomeShip].rotation));
                shipTr.position = ship.position - shipTr.TransformVector(shipDict[insideSomeShip].offset);
                ModHelper.Events.Unity.FireInNUpdates(() => {
                    OWRigidbody shipOw = ship.GetComponent<OWRigidbody>();
                    /*shipOw.DisableCollisionDetection();
                    shipOw.MakeKinematic();
                    foreach(Renderer r in ship.GetComponentsInChildren<Renderer>()) r.enabled = false;
                    foreach(Collider c in ship.GetComponentsInChildren<Collider>()) c.enabled = false;
                    shipOw.WarpToPositionRotation(ship.position + Vector3.down * 20000f, ship.rotation);*/

                    //move ship on the clone
                    shipOw.SetVelocity(Vector3.zero);
                    shipOw.SetAngularVelocity(Vector3.zero);
                    shipOw.WarpToPositionRotation(dummyShip.transform.position, dummyShip.transform.rotation);
                    dummyShip.SetActive(false);

                    //attach fake ship and player to new reference frame
                    OWRigidbody targetPlanet = SuspendBodyAtPosition(shipTr);
                    if(targetPlanet == null) {
                        ModHelper.Console.WriteLine("FlyAllShips: no reference OWRigidbody found to attach player.", MessageType.Warning);
                    } else {
                        OWRigidbody playerOw = Locator.GetPlayerBody();
                        playerOw.WarpToPositionRotation(player.position, player.rotation);
                        playerOw.SetVelocity(targetPlanet.GetPointVelocity(player.position));
                    }
                    shipTr.gameObject.SetActive(true);
                    insideSomeShip = "";
                }, 30);
            }
        }

        OWRigidbody SuspendBodyAtPosition(Transform shipToSuspend) {
            Vector3 worldPos = shipToSuspend.position;
            Collider[] cols = Physics.OverlapSphere(worldPos, 100f, OWLayerMask.closeRangeRFMask | OWLayerMask.longRangeRFMask); //tous les colliders autour
            Collider col;
            OWRigidbody ow, bestOw = null;
            float dist, bestDist = float.MaxValue;
            ReferenceFrameVolume vol;

            for(int i = 0; i < cols.Length; i++) {
                col = cols[i];
                ModHelper.Console.WriteLine(shipToSuspend.gameObject.name +" -> "+ col.gameObject.name, MessageType.Warning);
                if(IsChildOfExcluded(col.transform)) continue;
                vol = col.GetComponent<ReferenceFrameVolume>();
                ow = (vol != null ? (vol?.GetReferenceFrame()?.GetOWRigidBody()) : (col.attachedRigidbody?.GetComponent<OWRigidbody>()));
                ModHelper.Console.WriteLine(ow?.gameObject.name + " - " + vol?.gameObject.name + " - " + col.attachedRigidbody?.GetComponent<OWRigidbody>()?.gameObject.name);
                if(ow == null || IsChildOfExcluded(ow.transform)) continue;
                ModHelper.Console.WriteLine(string.Join("/", [.. col.gameObject.GetComponentsInParent<Transform>().Select(t => t.name).Reverse()]));
                dist = Vector3.Distance(worldPos, ow.GetPosition());
                if(dist < bestDist) {
                    bestDist = dist;
                    bestOw = ow;
                    ModHelper.Console.WriteLine(col.gameObject.name, MessageType.Success);
                }
            }
            if(bestOw == null) ModHelper.Console.WriteLine($"FlyAllShips: no ReferenceFrameVolume/OWRigidbody found at {ship.position}", MessageType.Warning);
            else shipToSuspend.GetComponent<OWRigidbody>().Suspend(bestOw);
            return bestOw;

            bool IsChildOfExcluded(Transform t) {
                if(t.IsChildOf(ship) || t.IsChildOf(shipToSuspend) || t.IsChildOf(player)) return true;
                foreach(KeyValuePair<string, (Transform transform, Vector3 offset, Vector3 rotation)> i in shipDict) {
                    if(t.IsChildOf(i.Value.transform)) return true;
                }
                return false;
            }
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
                    //store current movement relative to ship orientation
                    Vector3 temp = titleShip.TransformDirection(Vector3.forward) * titleShipSpeed.z + titleShip.TransformDirection(Vector3.right) * titleShipSpeed.w;
                    //rotate ship (> radial rotation)
                    titleShip.RotateAround(titleShip.position, upward, Time.deltaTime * titleShipSpeed.x);
                    //correct movement relative to new orientation
                    titleShipSpeed.z = Vector3.Dot(temp, titleShip.TransformDirection(Vector3.forward));
                    titleShipSpeed.w = Vector3.Dot(temp, titleShip.TransformDirection(Vector3.right));
                    /*/apply surface movement components (> movement along forward perimeter)
                    titleShipParent.RotateAround(titleShipParent.position, titleShip.TransformDirection(Vector3.right), Time.deltaTime * titleShipSpeed.z);
                    //apply sideways movement component (> movement along sideways perimeter)
                    titleShipParent.RotateAround(titleShipParent.position, titleShip.TransformDirection(Vector3.back), Time.deltaTime * titleShipSpeed.w);*/
                    titleShipParent.rotation = Quaternion.AngleAxis(Time.deltaTime * titleShipSpeed.z, titleShip.TransformDirection(Vector3.right))
                        * Quaternion.AngleAxis(Time.deltaTime * titleShipSpeed.w, titleShip.TransformDirection(Vector3.back))
                        * titleShipParent.rotation;
                    //simulate gravity
                    titleShipSpeed.y -= Time.deltaTime * 3;
                    //apply radial movement (> movement along radial direction)
                    titleShip.localPosition += titleShip.localPosition.normalized * Time.deltaTime * titleShipSpeed.y;
                } else {
                    titleShipSpeed = Vector4.zero;
                }
            }
        }
    }
}
