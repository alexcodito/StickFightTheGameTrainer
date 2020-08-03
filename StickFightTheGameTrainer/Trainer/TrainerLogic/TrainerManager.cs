using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using InControl;

public class TrainerManager : MonoBehaviour
{
    private float _deltaTime;
    private int _playerCount;
    private bool _isCoroutineExecuting;
    private MapWrapper _currentMap;
    private ControllerHandler _controllerHandler;
    private HoardHandler hoardHandlerBrat;
    private HoardHandler hoardHandlerPlayer;
    private HoardHandler hoardHandlerZombie;
    private readonly IList<Weapon> _weaponComponents;

#if DEBUG
    private Vector2 _unityLogsScrollPosition = Vector2.zero;
    private readonly StringBuilder _unityLogs = new StringBuilder();
#endif

    public TrainerManager()
    {
        _weaponComponents = new List<Weapon>();
        _controllerHandler = GetComponent<ControllerHandler>();
        SceneManager.sceneLoaded += OnSceneLoaded;

#if DEBUG
        Application.logMessageReceived += HandleUnityLogs;
#endif
    }

    public void Start()
    {
        // Load hoard handlers for AI spawning
        var hoardHandlers = Resources.FindObjectsOfTypeAll<HoardHandler>();

        foreach (HoardHandler hoardHandler in hoardHandlers)
        {
            if (hoardHandler.name == "AI spawner")
            {
                hoardHandlerPlayer = hoardHandler;
            }
            if (hoardHandler.name == "AI spawner (1)")
            {
                hoardHandlerBrat = hoardHandler;
            }
            if (hoardHandler.name == "AI spawner (2)")
            {
                hoardHandlerZombie = hoardHandler;
            }
        }

        // Populate list of weapons to use as reference when resetting defaults.
        var playerObject = LevelEditor.ResourcesManager.Instance.CharacterObject;
        var weaponObjects = playerObject.transform.Find("Weapons");

        for (var i = 0; i < weaponObjects.childCount; i++)
        {
            var weaponComponent = weaponObjects.GetChild(i).GetComponent<Weapon>();
            _weaponComponents.Add(weaponComponent);
        }
    }

    public void OnGUI()
    {
#if DEBUG
        DisplayUnityLogs(430f, 25f, 390f, 380f);
#endif
        if (Singleton<TrainerOptions>.Instance.DisplayTrainerMenu && Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            // Draw menu container and title
            GUI.Box(new Rect(25f, 25f, 390f, 380f), "");
            GUI.Label(new Rect(35f, 30f, 280f, 25f), "<color=silver><b>Stick Fight The Game</b></color>");
            GUI.Label(new Rect(35f, 50f, 425f, 25f), "<color=silver><b>+12 Trainer v{Application.ProductVersion} - Made by loxa</b></color>");

            // Calculate frame-rate
            string msFps;
            if (Singleton<TrainerOptions>.Instance.DisplayFps)
            {
                float num = this._deltaTime * 1000f;
                float num2 = 1f / this._deltaTime;
                msFps = string.Format(" {0:0.0} ms ({1:0} fps)", num, num2);
            }
            else
            {
                msFps = " -:-:- ms (-:- fps)";
            }

            // Display frame-rate
            if (GUI.Toggle(new Rect(290f, 30f, 120f, 25f), Singleton<TrainerOptions>.Instance.DisplayFps, "<color=maroon><b>" + msFps + "</b></color>") != Singleton<TrainerOptions>.Instance.DisplayFps)
            {
                Singleton<TrainerOptions>.Instance.DisplayFps = !Singleton<TrainerOptions>.Instance.DisplayFps;
            }

            GUI.Label(new Rect(35f, 80f, 400f, 90f), "<color=grey><b>Toggle Options</b></color>");

            // Toggle between 'Online (no-cheats)' and 'Online Friends (cheats)' mode
            if (GUI.Toggle(new Rect(35f, 100f, 100f, 25f), Singleton<TrainerOptions>.Instance.TrainerActive, !Singleton<TrainerOptions>.Instance.TrainerActive
                    ? "<color=green> Online Mode</color>"
                    : "<color=green> Friends Mode</color>") != Singleton<TrainerOptions>.Instance.TrainerActive)
            {
                // Announce restart due to online mode change
                MultiplayerManager.mGameManager.winText.fontSize = 140f;
                MultiplayerManager.mGameManager.winText.color = Color.white;
                MultiplayerManager.mGameManager.winText.text = (!Singleton<TrainerOptions>.Instance.TrainerActive
                    ? "ONLINE (FRIENDS) - CHEATS ENABLED\r\nRestarting in 2 seconds..."
                    : "ONLINE - CHEATS DISABLED\r\nRestarting in 2 seconds...");

                MultiplayerManager.mGameManager.winText.gameObject.SetActive(true);

                // Toggle trainer active state
                Singleton<TrainerOptions>.Instance.TrainerActive = !Singleton<TrainerOptions>.Instance.TrainerActive;

                // Provide time for the announcement text to appear and be read
                StartCoroutine(Wait(2f));

                // Restart game and switch lobby type
                MultiplayerManager.mGameManager.RestartGame();
                MatchmakingHandler.SetNewLobbyType(ELobbyType.k_ELobbyTypePrivate);
            }

            // Toggle display of health bars
            if (GUI.Toggle(new Rect(35f, 120f, 100f, 25f), Singleton<TrainerOptions>.Instance.DisplayHealthBars, " Health Bars") != Singleton<TrainerOptions>.Instance.DisplayHealthBars)
            {
                Singleton<TrainerOptions>.Instance.DisplayHealthBars = !Singleton<TrainerOptions>.Instance.DisplayHealthBars;
            }

            // Toggle display of scoreboard
            if (GUI.Toggle(new Rect(35f, 140f, 100f, 25f), Singleton<TrainerOptions>.Instance.DisplayScore, " Scoreboard") != Singleton<TrainerOptions>.Instance.DisplayScore)
            {
                Singleton<TrainerOptions>.Instance.DisplayScore = !Singleton<TrainerOptions>.Instance.DisplayScore;
            }

            // Toggle flying mode for all players
            if (GUI.Toggle(new Rect(140f, 100f, 100f, 25f), Singleton<TrainerOptions>.Instance.FlightMode, " Flying Mode") != Singleton<TrainerOptions>.Instance.FlightMode)
            {
                Singleton<TrainerOptions>.Instance.FlightMode = !Singleton<TrainerOptions>.Instance.FlightMode;
                ToggleFlyingMode();
            }

            // Toggle unlimited ammunition
            if (GUI.Toggle(new Rect(240f, 120f, 130f, 25f), Singleton<TrainerOptions>.Instance.UnlimitedAmmo, " Unlimited Ammo") != Singleton<TrainerOptions>.Instance.UnlimitedAmmo)
            {
                Singleton<TrainerOptions>.Instance.UnlimitedAmmo = !Singleton<TrainerOptions>.Instance.UnlimitedAmmo;
                ToggleUnlimitedAmmo();
            }

            // Toggle uncapped fire-rate
            if (GUI.Toggle(new Rect(240f, 100f, 130f, 25f), Singleton<TrainerOptions>.Instance.UncappedFirerate, " Uncapped Fire-rate") != Singleton<TrainerOptions>.Instance.UncappedFirerate)
            {
                Singleton<TrainerOptions>.Instance.UncappedFirerate = !Singleton<TrainerOptions>.Instance.UncappedFirerate;
                ToggleUncappedFirerate();
            }

            // Toggle full automatic weapons
            if (GUI.Toggle(new Rect(140f, 120f, 90f, 25f), Singleton<TrainerOptions>.Instance.FullAuto, " Full Auto") != Singleton<TrainerOptions>.Instance.FullAuto)
            {
                Singleton<TrainerOptions>.Instance.FullAuto = !Singleton<TrainerOptions>.Instance.FullAuto;
                ToggleFullAuto();
            }

            // Toggle unlimited health
            if (GUI.Toggle(new Rect(240f, 140f, 150f, 25f), Singleton<TrainerOptions>.Instance.UnlimitedHealth, " Unlimited Health") != Singleton<TrainerOptions>.Instance.UnlimitedHealth)
            {
                 Singleton<TrainerOptions>.Instance.UnlimitedHealth = !Singleton<TrainerOptions>.Instance.UnlimitedHealth;
                // Handled in the patched TakeDamage method
            }

            // Toggle no recoil
            if (GUI.Toggle(new Rect(140f, 140f, 90f, 25f), Singleton<TrainerOptions>.Instance.NoRecoil, " No Recoil") != Singleton<TrainerOptions>.Instance.NoRecoil)
            {
                Singleton<TrainerOptions>.Instance.NoRecoil = !Singleton<TrainerOptions>.Instance.NoRecoil;
                ToggleNoRecoil();
            }

            GUI.Label(new Rect(35f, 170f, 400f, 90f), "<color=grey><b>Spawn NPCs / Bots</b></color>");

            if (GUI.Button(new Rect(35f, 190f, 75f, 25f), "Dummy"))
            {
                SpawnBotDummy();
            }

            if (GUI.Button(new Rect(120f, 190f, 75f, 25f), "Enemy"))
            {
                SpawnBotEnemyPlayer();
            }

            if (GUI.Button(new Rect(205f, 190f, 75f, 25f), "Zombie"))
            {
                SpawnBotEnemyZombie();
            }

            if (GUI.Button(new Rect(290f, 190f, 75f, 25f), "Brat"))
            {
                SpawnBotEnemyBrat();
            }

            // Display available shortcuts
            GUI.Label(new Rect(35f, 230f, 400f, 90f), "<color=grey><b>PC Keyboard Shortcuts</b></color>\r\n" +
                                                      "- Toggle Menu:\t[SHIFT] + [M]\r\n" +
                                                      "- Skip Map:\t[SHIFT] + [S]\r\n" +
                                                      "- Spawn Weapon:\t[R] or [P]\r\n" +
                                                      "- Browse Weapons:\t[Q] for previous or [E] for next");

            GUI.Label(new Rect(35f, 320f, 500f, 90f), "<color=grey><b>Xbox 360 Controller Shortcuts</b></color>\r\n" +
                                                      "- Toggle Menu:\t[RB] + [A]\r\n" +
                                                      "- Skip Map:\t[RB] + [B]\r\n" +
                                                      "- Spawn Weapon:\t[DPadUp] or [DPadDown]\r\n" +
                                                      "- Browse Weapons:\t[DPadLeft] or [DPadRight]");
        }

        if (Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            // Display healthbars and scores for all players
            foreach (Controller controller2 in MultiplayerManager.mGameManager.controllerHandler.players)
            {
                if (controller2.fighting != null)
                {
                    if (Singleton<TrainerOptions>.Instance.DisplayHealthBars)
                    {
                        var health = 0f;
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

    public void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        if (Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            // Keep track of map / level changes
            var currentMap = MultiplayerManager.mGameManager.GetCurrentMap();
            if (_currentMap != currentMap)
            {
                // New map / level has been loaded
                _currentMap = currentMap;

                // Reapply options for all players
                ReapplyToggleOptions();
            }

            // Keep track of players
            if (_playerCount != _controllerHandler.ActivePlayers.Count)
            {
                if (_controllerHandler.ActivePlayers.Count > _playerCount)
                {
                    // New player joined - Reapply options for all players 
                    ReapplyToggleOptions();
                }

                _playerCount = _controllerHandler.ActivePlayers.Count;
            }

            // Change map / level (triggered by any player)
            if ((Input.GetKeyUp(KeyCode.JoystickButton1) && Input.GetKey(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton1) && Input.GetKeyUp(KeyCode.JoystickButton5)) || ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.S)))
            {
                Singleton<TrainerOptions>.Instance.NoWinners = true;

                if (MatchmakingHandler.IsNetworkMatch)
                {
                    // Delegate to the multiplayer game manager
                    var nextLevel = MultiplayerManager.mGameManager.levelSelector.GetNextLevel();
                    ChangeMap(nextLevel, 0);
                    return;
                }

                // Kill all players (no victory)
                MultiplayerManager.mGameManager.AllButOnePlayersDied();
            }

            // Toggle display of trainer menu (triggered by any player)
            if ((Input.GetKeyUp(KeyCode.JoystickButton0) && Input.GetKeyUp(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton0) && Input.GetKey(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton0) && Input.GetKey(KeyCode.JoystickButton5)) || ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.M)))
            {
                Singleton<TrainerOptions>.Instance.DisplayTrainerMenu = !Singleton<TrainerOptions>.Instance.DisplayTrainerMenu;
            }

            // Spawn random weapon (Keyboard)
            if (Input.GetKeyUp(KeyCode.R))
            {
                SpawnRandomWeapon(false);
            }

            // Spawn random present (Keyboard)
            if (Input.GetKeyUp(KeyCode.P))
            {
                SpawnRandomWeapon(true);
            }

            // Check for shortcut presses by individual players
            if (_controllerHandler != null && _controllerHandler.ActivePlayers != null)
            {
                foreach (Controller controller in _controllerHandler.ActivePlayers)
                {
                    if (controller != null && controller.mPlayerActions != null && controller.mPlayerActions.activeDevice != null)
                    {
                        // Spawn random weapon (Joystick)
                        if (controller.mPlayerActions.activeDevice.DPadUp.WasReleased)
                        {
                            SpawnRandomWeapon(false);
                        }

                        // Spawn random present (Joystick)
                        if (controller.mPlayerActions.activeDevice.DPadDown.WasReleased)
                        {
                            SpawnRandomWeapon(true);
                        }

                        // Select next weapon for the requesting player
                        if (controller.mPlayerActions.activeDevice.DPadLeft.WasReleased || (Input.GetKeyUp(KeyCode.Q) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any)))
                        {
                            if (controller.fighting.TrainerWeaponIndex <= 0)
                            {
                                controller.fighting.TrainerWeaponIndex = controller.fighting.weapons.transform.childCount;
                            }
                            else
                            {
                                var trainerWeaponIndex = controller.fighting.TrainerWeaponIndex;
                                controller.fighting.TrainerWeaponIndex = trainerWeaponIndex - 1;
                            }

                            controller.fighting.Dissarm();
                            controller.fighting.NetworkPickUpWeapon((byte)controller.fighting.TrainerWeaponIndex);

                            // Add a dot after the weapon number
                            if (controller.fighting != null && controller.fighting.weapon != null && controller.fighting.weapon.name != null)
                            {
                                var weaponName = controller.fighting.weapon.name;

                                if (weaponName.IndexOf(" ") > -1)
                                {
                                    weaponName = weaponName.Insert(weaponName.IndexOf(" "), ".");
                                }

                                // Announce selected weapon name
                                controller.fighting.mNetworkPlayer.mChatManager.Talk(weaponName);
                            }
                        }

                        // Select previous weapon for the requesting player
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

                            // Add a dot after the weapon number
                            if (controller.fighting != null && controller.fighting.weapon != null && controller.fighting.weapon.name != null)
                            {
                                var weaponName = controller.fighting.weapon.name;

                                if (weaponName.IndexOf(" ") > -1)
                                {
                                    weaponName = weaponName.Insert(weaponName.IndexOf(" "), ".");
                                }

                                // Announce selected weapon name
                                controller.fighting.mNetworkPlayer.mChatManager.Talk(weaponName);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Main scene has loaded (e.g. game restarted)
        if (scene != null && scene.name == "MainScene")
        {
            // Reset trainer options
            Singleton<TrainerOptions>.Instance.ResetOptions();
        }
    }

    /// <summary>
    /// Check if trainer cheats are enabled.
    /// </summary>
    private bool CheckCheatsEnabled()
    {
        return Singleton<TrainerOptions>.Instance.CheatsEnabled;
    }

    /// <summary>
    /// Change map / level
    /// </summary>
    /// <param name="nextLevel">Next level to change to.</param>
    /// <param name="indexOfWinner">Index of winning player.</param>
    private void ChangeMap(MapWrapper nextLevel, byte indexOfWinner)
    {
        GetComponent<MultiplayerManager>().UnReadyAllPlayers();
        var array = new byte[2 + nextLevel.MapData.Length];
        using (MemoryStream memoryStream = new MemoryStream(array))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(indexOfWinner);
                binaryWriter.Write(nextLevel.MapType);
                binaryWriter.Write(nextLevel.MapData);
            }
        }

        // Notify all clients that the map is changing.
        GetComponent<MultiplayerManager>().SendMessageToAllClients(array, P2PPackageHandler.MsgType.MapChange, false, 0UL, EP2PSend.k_EP2PSendReliable, 0);
    }

    /// <summary>
    /// Spawn an NPC that deals and takes damage. 
    private void SpawnBotDummy()
    {
        var spawnPosition = Vector3.up * 8f;
        var spawnRotation = Quaternion.identity;
        var playerId = MultiplayerManager.mGameManager.controllerHandler.players.Count;
        var playerColors = MultiplayerManagerAssets.Instance.Colors;
        var playerPrefab = MultiplayerManagerAssets.Instance.PlayerPrefab;
        var playerObject = UnityEngine.Object.Instantiate<GameObject>(playerPrefab, spawnPosition, spawnRotation);
        var playerController = playerObject.GetComponent<Controller>();

        var playerLineRenderers = playerObject.GetComponentsInChildren<LineRenderer>();
        for (int i = 0; i < playerLineRenderers.Length; i++)
        {
            playerLineRenderers[i].sharedMaterial = playerColors[playerId];
        }

        foreach (var spriteRenderer in playerObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (spriteRenderer.transform.tag != "DontChangeColor")
            {
                spriteRenderer.color = playerColors[playerId].color;
            }
        }

        var characterInformation = playerObject.GetComponent<CharacterInformation>();
        characterInformation.myMaterial = playerColors[playerId];
        characterInformation.enabled = true;

        playerController.AssignNewDevice(null, false); // Uninitialized mPlayerActions causes NULL reference exception in Controller.Update. 
        playerController.playerID = playerId;
        playerController.isAI = true;
        playerController.enabled = true;
        playerController.inactive = false;
        playerController.SetCollision(true);

        playerController.Start(); // Stops the bot from 'flying away' (glitch workaround for this.m_Players)

        // WIP Tests 
        //
        //MultiplayerManager.SpawnPlayerDummy((byte)playerId);
        //playerObject.FetchComponent<NetworkPlayer>().InitNetworkSpawnID((ushort)playerId);
        //UnityEngine.Object.FindObjectOfType<MultiplayerManager>().PlayerControllers.Add(playerController);
        //this.UpdateLocalClientsData(playerId, playerObject);
        //this.m_Players[playerId] = playerController;
        //

        MultiplayerManager.mGameManager.controllerHandler.players.Add(playerController);
        MultiplayerManager.mGameManager.RevivePlayer(playerController);
    }

    private void SpawnBotEnemyPlayer()
    {
        MultiplayerManager.mGameManager.hoardHandler.SpawnAI(hoardHandlerPlayer.character);
    }

    private void SpawnBotEnemyZombie()
    {
        MultiplayerManager.mGameManager.hoardHandler.SpawnAI(hoardHandlerZombie.character);
    }

    private void SpawnBotEnemyBrat()
    {
        MultiplayerManager.mGameManager.hoardHandler.SpawnAI(hoardHandlerBrat.character);
    }

    /// <summary>
    /// Spawn a random weapon to fall from a random location in the sky.
    /// </summary>
    private void SpawnRandomWeapon(bool spawnAsPresent)
    {
        // Generate a location for the weapon to spawn
        var vector = Vector3.up * 11f + Vector3.forward * UnityEngine.Random.Range(0f, 8f);

        // Fetch a random weapon index
        GameObject weapon;
        var randomWeaponIndex = GetComponent<GameManager>().m_WeaponSelectionHandler.GetRandomWeaponIndex(true, out weapon);

        if (randomWeaponIndex < 0)
        {
            return;
        }

        // Delegate spawning of the weapon to the network manager
        if (MatchmakingHandler.IsNetworkMatch)
        {
            // Updates to the game often result in changes to method signatures (e.g. additional parameters).
            // TrainerLogicModuleBuilder will uncomment the appropriate lines depending on the target version of the game.

            // Pre v1.2.08
            //{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Pre1_2_08_arg_1}GetComponent<GameManager>().mNetworkManager.SpawnWeapon(randomWeaponIndex, vector);

            // Post v1.2.08
            //{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Post1_2_08_arg_1}GetComponent<GameManager>().mNetworkManager.SpawnWeapon(randomWeaponIndex, vector, spawnAsPresent);

            return;
        }

        // Spawn weapon locally
        var instantiatedWeapon = Instantiate<GameObject>(weapon, vector, Quaternion.identity);

        if (spawnAsPresent)
        {
            // Updates to the game often result in changes to method signatures (e.g. additional parameters).
            // TrainerLogicModuleBuilder will uncomment the appropriate lines depending on the target version of the game.

            // Post v1.2.08
            //{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Post1_2_08_arg_1}instantiatedWeapon.GetComponent<WeaponPickUp>().ChangeToPresent();
        }

        GetComponent<GameManager>().mSpawnedWeapons.Add(instantiatedWeapon.GetComponent<Rigidbody>());
    }

    private void ToggleFlyingMode()
    {
        foreach (Controller player in MultiplayerManager.mGameManager.controllerHandler.players)
        {
            if (player != null && player.isAI == false)
            {
                player.canFly = Singleton<TrainerOptions>.Instance.FlightMode;
            }
        }
    }

    private void ToggleUncappedFirerate()
    {
        foreach (Controller activePlayer in _controllerHandler.ActivePlayers)
        {
            if (activePlayer.fighting == null || activePlayer.fighting.weapons == null)
            {
                continue;
            }

            var weapons = activePlayer.fighting.weapons.transform.GetComponentsInChildren<Weapon>();

            if (Singleton<TrainerOptions>.Instance.UncappedFirerate)
            {
                // Set uncapped fire-rate on each player's set of weapons
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.cd = 0;
                    weapon.reloads = false;
                    weapon.reloadTime = 0;
                    weapon.shotsBeforeReload = 9999;
                }
            }
            else
            {
                // Reset default weapon settings
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weaponComponent = _weaponComponents[i];
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.cd = weaponComponent.cd;
                    weapon.reloads = weaponComponent.reloads;
                    weapon.reloadTime = weaponComponent.reloadTime;
                    weapon.shotsBeforeReload = weaponComponent.shotsBeforeReload;
                }
            }
        }
    }

    private void ToggleFullAuto()
    {
        foreach (Controller activePlayer in _controllerHandler.ActivePlayers)
        {
            if (activePlayer.fighting == null || activePlayer.fighting.weapons == null)
            {
                continue;
            }

            var weapons = activePlayer.fighting.weapons.transform.GetComponentsInChildren<Weapon>();

            if (Singleton<TrainerOptions>.Instance.FullAuto)
            {
                // Set uncapped full auto on each player's set of weapons
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.fullAuto = true;
                }

                if (activePlayer.fighting.weapon != null)
                {
                    activePlayer.fighting.weapon.fullAuto = true;
                    activePlayer.fighting.fullAuto = true;
                }
            }
            else
            {
                // Reset default weapon settings
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weaponComponent = _weaponComponents[i];
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.fullAuto = weaponComponent.fullAuto;

                    if (activePlayer.fighting.weapon != null && activePlayer.fighting.weapon.name == weaponComponent.name)
                    {
                        activePlayer.fighting.weapon.fullAuto = weaponComponent.fullAuto;
                        activePlayer.fighting.fullAuto = weaponComponent.fullAuto;
                    }
                }
            }
        }
    }

    private void ToggleUnlimitedAmmo()
    {
        foreach (Controller activePlayer in _controllerHandler.ActivePlayers)
        {
            if (activePlayer.fighting == null || activePlayer.fighting.weapons == null)
            {
                continue;
            }

            var weapons = activePlayer.fighting.weapons.transform.GetComponentsInChildren<Weapon>();

            if (Singleton<TrainerOptions>.Instance.UnlimitedAmmo)
            {
                // Set unlimited ammunition on each player's set of weapons
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.startBullets = 9999;
                    weapon.currentCharge = 9999;
                    weapon.secondsOfUseLeft = 9999;
                }

                if (activePlayer.fighting.weapon != null)
                {
                    activePlayer.fighting.bulletsLeft = 9999;
                }
            }
            else
            {
                // Reset default weapon settings
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weaponComponent = _weaponComponents[i];
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.startBullets = weaponComponent.startBullets;
                    weapon.currentCharge = weaponComponent.currentCharge;
                    weapon.secondsOfUseLeft = weaponComponent.secondsOfUseLeft;

                    if (activePlayer.fighting.weapon != null && activePlayer.fighting.weapon.name == weaponComponent.name)
                    {
                        activePlayer.fighting.bulletsLeft = weaponComponent.startBullets;
                    }
                }
            }
        }
    }

    private void ToggleNoRecoil()
    {
        foreach (Controller activePlayer in _controllerHandler.ActivePlayers)
        {
            if (activePlayer.fighting == null || activePlayer.fighting.weapons == null)
            {
                continue;
            }

            var weapons = activePlayer.fighting.weapons.transform.GetComponentsInChildren<Weapon>();

            if (Singleton<TrainerOptions>.Instance.NoRecoil)
            {
                // Set unlimited ammunition on each player's set of weapons
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.recoil = 0;
                    weapon.torsoRecoil = 0;
                }
            }
            else
            {
                // Reset default weapon settings
                for (var i = 0; i < activePlayer.fighting.weapons.transform.childCount; i++)
                {
                    var weaponComponent = _weaponComponents[i];
                    var weapon = activePlayer.fighting.weapons.transform.GetChild(i).GetComponent<Weapon>();

                    weapon.recoil = weaponComponent.recoil;
                    weapon.torsoRecoil = weaponComponent.torsoRecoil;
                }
            }
        }
    }

    private void ReapplyToggleOptions()
    {
        ToggleFlyingMode();
        ToggleFullAuto();
        ToggleNoRecoil();
        ToggleUncappedFirerate();
        ToggleUnlimitedAmmo();
    }

    /// <summary>
    /// Stall the game for a specified amount of time.
    /// </summary>
    /// <param name="waitTime">Number of seconds to wait.</param>
    private static IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    /// <summary>
    /// Get a player's color by player index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Color object</returns>
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

#if DEBUG
    private void DisplayUnityLogs(float x, float y, float width, float height)
    {
        GUI.Box(new Rect(x, y, width, height), "");
        if (GUI.Button(new Rect(x + width - 50f, y + height + 10f, 50f, 30f), "Clear"))
        {
            _unityLogs.Length = 0;
        }
        GUILayout.BeginArea(new Rect(x, y, width, height));
        _unityLogsScrollPosition = GUILayout.BeginScrollView(_unityLogsScrollPosition, new GUILayoutOption[]
        {
            GUILayout.Width(width),
            GUILayout.Height(height)
        });
        GUILayout.TextField(_unityLogs.ToString(), "Label", new GUILayoutOption[0]);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void HandleUnityLogs(string logString, string stackTrace, LogType type)
    {
        if (_unityLogs.Length > 20000)
        {
            _unityLogs.Remove(0, _unityLogs.Length);
        }

        if (logString.Length > 1000)
        {
            logString = logString.Substring(0, 1000);
        }

        if (stackTrace.Length > 1000)
        {
            stackTrace = stackTrace.Substring(0, 1000);
        }

        _unityLogs.AppendLine(type.ToString() + ": " + logString + Environment.NewLine + stackTrace);
    }
#endif

    // GUI utility polyfill
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