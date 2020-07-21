using System;
using System.Collections;
using System.IO;
using Steamworks;
using UnityEngine;

// Fly mode has to be reset every time map changes
// - This needs to be activated once on toggle and also once a player joins 
public class TrainerManager : MonoBehaviour
{
    public void OnGUI()
    {
        if (Singleton<TrainerOptions>.Instance.DisplayTrainerMenu && Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            string msFps;
            if (Singleton<TrainerOptions>.Instance.DisplayFps)
            {
                float num = this.deltaTime * 1000f;
                float num2 = 1f / this.deltaTime;
                msFps = string.Format(" {0:0.0} ms ({1:0} fps)", num, num2);
            }
            else
            {
                msFps = " -:-:- ms (-:- fps)";
            }

            GUI.Box(new Rect(25f, 25f, 410f, 330f), "");
            GUI.Label(new Rect(35f, 30f, 280f, 25f), "<color=silver><b>Stick Fight The Game</b></color>");

            if (GUI.Toggle(new Rect(310f, 30f, 120f, 25f), Singleton<TrainerOptions>.Instance.DisplayFps, "<color=maroon><b>" + msFps + "</b></color>") != Singleton<TrainerOptions>.Instance.DisplayFps)
            {
                Singleton<TrainerOptions>.Instance.DisplayFps = !Singleton<TrainerOptions>.Instance.DisplayFps;
            }

            GUI.Label(new Rect(35f, 50f, 425f, 25f), "<color=silver><b>+11 Trainer v{Application.ProductVersion} - Made by loxa</b></color>");
            GUI.Label(new Rect(35f, 80f, 400f, 90f), "<color=grey><b>Toggle Options </b></color>");

            if (GUI.Toggle(new Rect(35f, 100f, 100f, 25f), Singleton<TrainerOptions>.Instance.TrainerActive, (!Singleton<TrainerOptions>.Instance.TrainerActive) ? "<color=green> Online Mode</color>" : "<color=green> Friends Mode</color>") != Singleton<TrainerOptions>.Instance.TrainerActive)
            {
                MultiplayerManager.mGameManager.winText.fontSize = 140f;
                MultiplayerManager.mGameManager.winText.color = Color.white;
                MultiplayerManager.mGameManager.winText.text = ((!Singleton<TrainerOptions>.Instance.TrainerActive) ? "ONLINE (FRIENDS) - CHEATS ENABLED\r\nRestarting in 2 seconds..." : "ONLINE - CHEATS DISABLED\r\nRestarting in 2 seconds...");
                MultiplayerManager.mGameManager.winText.gameObject.SetActive(true);

                Singleton<TrainerOptions>.Instance.TrainerActive = !Singleton<TrainerOptions>.Instance.TrainerActive;

                StartCoroutine(Wait(2f));


                MultiplayerManager.mGameManager.RestartGame();
                MatchmakingHandler.SetNewLobbyType(ELobbyType.k_ELobbyTypePrivate);
            }

            if (GUI.Toggle(new Rect(35f, 120f, 100f, 25f), Singleton<TrainerOptions>.Instance.DisplayHealthBars, " Health Bars") != Singleton<TrainerOptions>.Instance.DisplayHealthBars)
            {
                Singleton<TrainerOptions>.Instance.DisplayHealthBars = !Singleton<TrainerOptions>.Instance.DisplayHealthBars;
            }

            if (GUI.Toggle(new Rect(140f, 100f, 100f, 25f), Singleton<TrainerOptions>.Instance.FlightMode, " Flying Mode") != Singleton<TrainerOptions>.Instance.FlightMode)
            {
                Singleton<TrainerOptions>.Instance.FlightMode = !Singleton<TrainerOptions>.Instance.FlightMode;

                foreach (Controller controller in MultiplayerManager.mGameManager.controllerHandler.players)
                {
                    if (controller != null)
                    {
                        controller.canFly = Singleton<TrainerOptions>.Instance.FlightMode;
                    }
                }
            }

            if (GUI.Toggle(new Rect(35f, 140f, 100f, 25f), Singleton<TrainerOptions>.Instance.DisplayScore, " Scoreboard") != Singleton<TrainerOptions>.Instance.DisplayScore)
            {
                Singleton<TrainerOptions>.Instance.DisplayScore = !Singleton<TrainerOptions>.Instance.DisplayScore;
            }

            if (GUI.Toggle(new Rect(240f, 120f, 130f, 25f), Singleton<TrainerOptions>.Instance.UnlimitedAmmo, " Unlimited Ammo") != Singleton<TrainerOptions>.Instance.UnlimitedAmmo)
            {
                Singleton<TrainerOptions>.Instance.UnlimitedAmmo = !Singleton<TrainerOptions>.Instance.UnlimitedAmmo;
            }

            if (GUI.Toggle(new Rect(240f, 100f, 130f, 25f), Singleton<TrainerOptions>.Instance.UncappedFirerate, " Uncapped Fire-rate") != Singleton<TrainerOptions>.Instance.UncappedFirerate)
            {
                Singleton<TrainerOptions>.Instance.UncappedFirerate = !Singleton<TrainerOptions>.Instance.UncappedFirerate;
            }

            if (GUI.Toggle(new Rect(140f, 120f, 90f, 25f), Singleton<TrainerOptions>.Instance.FullAuto, " Full Auto") != Singleton<TrainerOptions>.Instance.FullAuto)
            {
                Singleton<TrainerOptions>.Instance.FullAuto = !Singleton<TrainerOptions>.Instance.FullAuto;
            }

            if (GUI.Toggle(new Rect(240f, 140f, 150f, 25f), Singleton<TrainerOptions>.Instance.UnlimitedHealth, " Unlimited Health") != Singleton<TrainerOptions>.Instance.UnlimitedHealth)
            {
                Singleton<TrainerOptions>.Instance.UnlimitedHealth = !Singleton<TrainerOptions>.Instance.UnlimitedHealth;
            }

            if (GUI.Toggle(new Rect(140f, 140f, 90f, 25f), Singleton<TrainerOptions>.Instance.NoRecoil, " No Recoil") != Singleton<TrainerOptions>.Instance.NoRecoil)
            {
                Singleton<TrainerOptions>.Instance.NoRecoil = !Singleton<TrainerOptions>.Instance.NoRecoil;
            }

            GUI.Label(new Rect(35f, 180f, 400f, 90f), "<color=grey><b>PC Keyboard Shortcuts</b></color>\r\n- Toggle Menu:\t[SHIFT] + [M]\r\n- Skip Map:\t[SHIFT] + [S]\r\n- Spawn Weapon:\t[R]\r\n- Browse Weapons:\t[Q] for previous or [E] for next");
            GUI.Label(new Rect(35f, 270f, 500f, 90f), "<color=grey><b>Xbox 360 Controller Shortcuts</b></color>\r\n- Toggle Menu:\t[Left Trigger Button] + [Right Trigger Button]\r\n- Skip Map:\t[RB] + [B]\r\n- Spawn Weapon:\t[DPadUp]\r\n- Browse Weapons:\t[DPadLeft] or [DPadRight]");
        }
        if (Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            foreach (Controller controller2 in MultiplayerManager.mGameManager.controllerHandler.players)
            {
                if (controller2.fighting != null)
                {
                    if (Singleton<TrainerOptions>.Instance.DisplayHealthBars)
                    {
                        float health = 0f;
                        var healthHandler = controller2.fighting.GetComponent<HealthHandler>();
                        if (healthHandler != null)
                        {
                            health = controller2.fighting.GetComponent<HealthHandler>().health;
                        }
                        EditorGUITools.DrawRect(new Rect(Screen.width - 125, 30f * controller2.playerID + 10f, Math.Max(0f, health), 20f), GetPlayerColorByIndex(controller2.playerID), null);
                        GUI.Label(new Rect(Screen.width - 160, 30f * controller2.playerID + 10f, 250f, 25f), Math.Max(0.0, Math.Round(health)).ToString());
                    }

                    if (Singleton<TrainerOptions>.Instance.DisplayScore && controller2.fighting.stats != null)
                    {
                        GUI.Label(new Rect(Screen.width - 180, 30f * (float)controller2.playerID + 10f, 250f, 25f), "<b>" + controller2.fighting.stats.wins.ToString() + "</b>");
                    }
                }
            }
        }
    }

    private static IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    private bool CheckCheatsEnabled()
    {
        return Singleton<TrainerOptions>.Instance.CheatsEnabled;
    }

    public Color GetPlayerColorByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Color.yellow;
            case 1:
                return Color.cyan;
            case 2:
                return Color.red;
            case 3:
                return Color.green;
            default:
                return Color.white;
        }
    }

    public void ChangeMap(MapWrapper nextLevel, byte indexOfWinner)
    {
        GetComponent<MultiplayerManager>().UnReadyAllPlayers();
        byte[] array = new byte[2 + nextLevel.MapData.Length];
        using (MemoryStream memoryStream = new MemoryStream(array))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(indexOfWinner);
                binaryWriter.Write(nextLevel.MapType);
                binaryWriter.Write(nextLevel.MapData);
            }
        }
        GetComponent<MultiplayerManager>().SendMessageToAllClients(array, P2PPackageHandler.MsgType.MapChange, false, 0UL, EP2PSend.k_EP2PSendReliable, 0);
    }

    public void SpawnRandomWeapon()
    {
        Vector3 vector = Vector3.up * 11f + Vector3.forward * UnityEngine.Random.Range(0f, 8f);
        GameObject original;
        int randomWeaponIndex = GetComponent<GameManager>().m_WeaponSelectionHandler.GetRandomWeaponIndex(true, out original);

        if (randomWeaponIndex < 0)
        {
            return;
        }
        if (MatchmakingHandler.IsNetworkMatch)
        {
            // TODO: Uncomment one of the two:
            // Pre v1.2.08
            //{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Pre1_2_08_arg_1}GetComponent<GameManager>().mNetworkManager.SpawnWeapon(randomWeaponIndex, vector);
            // Post v1.2.08
            //{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Post1_2_08_arg_1}GetComponent<GameManager>().mNetworkManager.SpawnWeapon(randomWeaponIndex, vector, true);
            return;
        }

        GameObject gameObject = Instantiate<GameObject>(original, vector, Quaternion.identity);
        GetComponent<GameManager>().mSpawnedWeapons.Add(gameObject.GetComponent<Rigidbody>());
    }

    public void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if (Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            if ((Input.GetKeyUp(KeyCode.JoystickButton1) && Input.GetKey(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton1) && Input.GetKeyUp(KeyCode.JoystickButton5)) || ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.S)))
            {
                Singleton<TrainerOptions>.Instance.NoWinners = true;
                if (MatchmakingHandler.IsNetworkMatch)
                {
                    MapWrapper nextLevel = MultiplayerManager.mGameManager.levelSelector.GetNextLevel();
                    ChangeMap(nextLevel, 0);
                    return;
                }
                MultiplayerManager.mGameManager.AllButOnePlayersDied();
            }
            if ((Input.GetKeyUp(KeyCode.JoystickButton9) && Input.GetKeyUp(KeyCode.JoystickButton8)) || (Input.GetKeyUp(KeyCode.JoystickButton9) && Input.GetKey(KeyCode.JoystickButton8)) || (Input.GetKeyUp(KeyCode.JoystickButton8) && Input.GetKey(KeyCode.JoystickButton9)) || Input.GetKeyUp(KeyCode.M))
            {
                Singleton<TrainerOptions>.Instance.DisplayTrainerMenu = !Singleton<TrainerOptions>.Instance.DisplayTrainerMenu;
            }
            if (controllerHandler != null && controllerHandler.ActivePlayers != null)
            {
                foreach (Controller controller in controllerHandler.ActivePlayers)
                {
                    if (controller.mPlayerActions.activeDevice.DPadUp.WasReleased || Input.GetKeyUp(KeyCode.R))
                    {
                        SpawnRandomWeapon();
                    }
                    if (controller.mPlayerActions.activeDevice.DPadLeft.WasReleased || (Input.GetKeyUp(KeyCode.Q) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any)))
                    {
                        if (controller.fighting.TrainerWeaponIndex <= 0)
                        {
                            controller.fighting.TrainerWeaponIndex = controller.fighting.weapons.transform.childCount;
                        }
                        else
                        {
                            int trainerWeaponIndex = controller.fighting.TrainerWeaponIndex;
                            controller.fighting.TrainerWeaponIndex = trainerWeaponIndex - 1;
                        }

                        controller.fighting.Dissarm();
                        controller.fighting.NetworkPickUpWeapon((byte)controller.fighting.TrainerWeaponIndex);

                        // Add a closed parenthesis after the weapon number.
                        var weaponName = controller.fighting.weapon.name;
                        if (weaponName != null && weaponName.IndexOf(" ") > -1)
                        {
                            weaponName = weaponName.Insert(weaponName.IndexOf(" "), ".");
                        }

                        controller.fighting.mNetworkPlayer.mChatManager.Talk(weaponName);
                    }
                    if (controller.mPlayerActions.activeDevice.DPadRight.WasReleased || (Input.GetKeyUp(KeyCode.E) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any)))
                    {
                        if (controller.fighting.weapons.transform.childCount <= controller.fighting.TrainerWeaponIndex)
                        {
                            controller.fighting.TrainerWeaponIndex = 0;
                        }
                        else
                        {
                            int trainerWeaponIndex2 = controller.fighting.TrainerWeaponIndex;
                            controller.fighting.TrainerWeaponIndex = trainerWeaponIndex2 + 1;
                        }

                        controller.fighting.Dissarm();
                        controller.fighting.NetworkPickUpWeapon((byte)controller.fighting.TrainerWeaponIndex);

                        // Add a closed parenthesis after the weapon number.
                        var weaponName = controller.fighting.weapon.name;
                        if (weaponName != null && weaponName.IndexOf(" ") > -1)
                        {
                            weaponName = weaponName.Insert(weaponName.IndexOf(" "), ".");
                        }

                        controller.fighting.mNetworkPlayer.mChatManager.Talk(weaponName);
                    }
                    if (Singleton<TrainerOptions>.Instance.UncappedFirerate)
                    {
                        if (controller.fighting.weapon != null)
                        {
                            controller.fighting.weapon.cd = 0f;
                            controller.fighting.weapon.reloads = false;
                            controller.fighting.weapon.reloadTime = 0f;
                            controller.fighting.weapon.shotsBeforeReload = 9999;
                        }
                    }
                    if (Singleton<TrainerOptions>.Instance.FullAuto)
                    {
                        if (controller.fighting.weapon != null)
                        {
                            controller.fighting.weapon.fullAuto = true;
                            controller.fighting.fullAuto = true;
                        }
                    }
                    if (Singleton<TrainerOptions>.Instance.UnlimitedAmmo)
                    {
                        if (controller.fighting.weapon != null)
                        {
                            controller.fighting.weapon.currentCharge = 9999f;
                            controller.fighting.weapon.secondsOfUseLeft = 9999f;
                        }

                        controller.fighting.bulletsLeft = 9999;
                    }
                    if (Singleton<TrainerOptions>.Instance.NoRecoil)
                    {
                        if (controller.fighting.weapon != null)
                        {
                            controller.fighting.weapon.recoil = 0f;
                            controller.fighting.weapon.torsoRecoil = 0f;
                        }
                    }
                }
            }
        }
    }

    public TrainerManager()
    {
    }

    public void Start()
    {
        controllerHandler = GetComponent<ControllerHandler>();
    }

    public float deltaTime;

    public bool isCoroutineExecuting;

    public ControllerHandler controllerHandler;

    public static class EditorGUITools
    {
        private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;

        public static void DrawRect(Rect position, Color color, GUIContent guiContent)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            var content = guiContent;
            
            if (content == null)
            {
                content = GUIContent.none;
            }

            var gUIStyleState = new GUIStyleState();
            var style = new GUIStyle();

            style.normal = gUIStyleState;
            style.normal.background = backgroundTexture;

            GUI.Box(position, content, style);
            GUI.backgroundColor = backgroundColor;
        }
    }
}