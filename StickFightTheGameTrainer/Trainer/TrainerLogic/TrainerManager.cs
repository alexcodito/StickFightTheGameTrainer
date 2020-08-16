#define REQUIRE_COMPATIBILITY_PATCHING

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using InControl;
using System.Diagnostics;

public class TrainerManager : MonoBehaviour
{
    private float _deltaTime;
    private float _keyHoldTime;
    private int _playerCount;
    private bool _isCoroutineExecuting;
    private MapWrapper _currentMap;
    private ControllerHandler _controllerHandler;
    private HoardHandler hoardHandlerBrat;
    private HoardHandler hoardHandlerPlayer;
    private HoardHandler hoardHandlerZombie;
    private readonly IList<Weapon> _weaponComponents;

#if DEBUG
    private bool _unityLogsEnabled = true;
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
        DisplayUnityLogs(460f, 25f, 420f, 505f);
#endif
        if (Singleton<TrainerOptions>.Instance.DisplayTrainerMenu && Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            var containerWidth = 420f;
            var containerHeight = 505f;

            var containerStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 5)
            };

            var horizontalToggleGroupStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(5, 5, 0, 0),
                fixedHeight = 25f
            };

            var horizontalSliderGroupStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0),
            };

            var toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                margin = new RectOffset(0, 0, 2, 0),
                padding = new RectOffset(15, 0, 3, 2)
            };

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(5, 5, 5, 5)
            };

            var sliderStyle = new GUIStyle()
            {
                padding = new RectOffset(8, 8, 0, 0)
            };

            // Calculate / format frame-rate
            var deltaTimeMs = this._deltaTime * 1000f;
            var fps = 1f / this._deltaTime;
            var formattedMsFps = string.Format(" {0:0.0} ms ({1:0} fps)", deltaTimeMs, fps);

            GUILayout.BeginArea(new Rect(25f, 25f, containerWidth, containerHeight), containerStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=silver><b>Stick Fight The Game</b></color>", new GUIStyle { alignment = TextAnchor.MiddleLeft });
            GUILayout.Label("<color=maroon><b>" + formattedMsFps + "</b></color>", new GUIStyle { alignment = TextAnchor.MiddleRight });
            GUILayout.EndHorizontal();

            GUILayout.Label("<color=silver><b>+12 Trainer v{Application.ProductVersion} - Made by loxa</b></color>");
            GUILayout.Space(5);
            GUILayout.Label("<color=grey><b>Toggle Options</b></color>");

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            // Toggle between 'Online (no-cheats)' and 'Online Friends (cheats)' mode
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.TrainerActive, "<color=green> " + (Singleton<TrainerOptions>.Instance.TrainerActive ? "Friends Mode" : "Online Mode") + "</color>", GUILayout.Width(140)) != Singleton<TrainerOptions>.Instance.TrainerActive)
            {
                // Announce restart due to online mode change
                MultiplayerManager.mGameManager.winText.fontSize = 140f;
                MultiplayerManager.mGameManager.winText.color = Color.white;
                MultiplayerManager.mGameManager.winText.text = (!Singleton<TrainerOptions>.Instance.TrainerActive
                    ? "ONLINE (FRIENDS ONLY) - CHEATS ENABLED\r\nRestarting in 2 seconds..."
                    : "ONLINE (PUBLIC) - CHEATS DISABLED\r\nRestarting in 2 seconds...");

                MultiplayerManager.mGameManager.winText.gameObject.SetActive(true);

                // Toggle trainer active state
                Singleton<TrainerOptions>.Instance.TrainerActive = !Singleton<TrainerOptions>.Instance.TrainerActive;

                // Provide time for the announcement text to appear and be read
                StartCoroutine(Wait(2f));

                // Restart game and switch lobby type
                MultiplayerManager.mGameManager.RestartGame();
                MatchmakingHandler.SetNewLobbyType(ELobbyType.k_ELobbyTypePrivate);
            }

            // Toggle flying mode for all players
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.FlightMode, " Flying Mode", GUILayout.Width(140)) != Singleton<TrainerOptions>.Instance.FlightMode)
            {
                Singleton<TrainerOptions>.Instance.FlightMode = !Singleton<TrainerOptions>.Instance.FlightMode;
                ToggleFlyingMode();
            }

            // Toggle uncapped fire-rate
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.UncappedFirerate, " Uncapped Fire-rate", GUILayout.Width(140)) != Singleton<TrainerOptions>.Instance.UncappedFirerate)
            {
                Singleton<TrainerOptions>.Instance.UncappedFirerate = !Singleton<TrainerOptions>.Instance.UncappedFirerate;
                ToggleUncappedFirerate();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            // Toggle display of health bars
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.DisplayHealthBars, " Health Bars", GUILayout.Width(130)) != Singleton<TrainerOptions>.Instance.DisplayHealthBars)
            {
                Singleton<TrainerOptions>.Instance.DisplayHealthBars = !Singleton<TrainerOptions>.Instance.DisplayHealthBars;
            }

            // Toggle full automatic weapons
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.FullAuto, " Full Auto", GUILayout.Width(130)) != Singleton<TrainerOptions>.Instance.FullAuto)
            {
                Singleton<TrainerOptions>.Instance.FullAuto = !Singleton<TrainerOptions>.Instance.FullAuto;
                ToggleFullAuto();
            }

            // Toggle unlimited ammunition
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.UnlimitedAmmo, " Unlimited Ammo", GUILayout.Width(130)) != Singleton<TrainerOptions>.Instance.UnlimitedAmmo)
            {
                Singleton<TrainerOptions>.Instance.UnlimitedAmmo = !Singleton<TrainerOptions>.Instance.UnlimitedAmmo;
                ToggleUnlimitedAmmo();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            // Toggle display of scoreboard
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.DisplayScore, " Scoreboard", GUILayout.Width(150)) != Singleton<TrainerOptions>.Instance.DisplayScore)
            {
                Singleton<TrainerOptions>.Instance.DisplayScore = !Singleton<TrainerOptions>.Instance.DisplayScore;
            }

            // Toggle no recoil
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.NoRecoil, " No Recoil", GUILayout.Width(150)) != Singleton<TrainerOptions>.Instance.NoRecoil)
            {
                Singleton<TrainerOptions>.Instance.NoRecoil = !Singleton<TrainerOptions>.Instance.NoRecoil;
                ToggleNoRecoil();
            }

            // Toggle unlimited health
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.UnlimitedHealth, " Unlimited Health", GUILayout.Width(150)) != Singleton<TrainerOptions>.Instance.UnlimitedHealth)
            {
                // Handled in the patched TakeDamage method
                Singleton<TrainerOptions>.Instance.UnlimitedHealth = !Singleton<TrainerOptions>.Instance.UnlimitedHealth;
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("<color=grey><b>Spawn Bots</b></color>");

            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal(horizontalToggleGroupStyle, GUILayout.Width(100));
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.SpawnPcEnabled, " PC", toggleStyle, GUILayout.Width(50)) != Singleton<TrainerOptions>.Instance.SpawnPcEnabled)
            {
                Singleton<TrainerOptions>.Instance.SpawnPcEnabled = !Singleton<TrainerOptions>.Instance.SpawnPcEnabled;
            }
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.SpawnNpcEnabled, " NPC", toggleStyle, GUILayout.Width(50)) != Singleton<TrainerOptions>.Instance.SpawnNpcEnabled)
            {
                Singleton<TrainerOptions>.Instance.SpawnNpcEnabled = !Singleton<TrainerOptions>.Instance.SpawnNpcEnabled;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(horizontalToggleGroupStyle, GUILayout.Width(170));
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.AiAggressiveEnabled, " Aggressive", toggleStyle, GUILayout.Width(95)) != Singleton<TrainerOptions>.Instance.AiAggressiveEnabled)
            {
                Singleton<TrainerOptions>.Instance.AiAggressiveEnabled = !Singleton<TrainerOptions>.Instance.AiAggressiveEnabled;
            }
            if (GUILayout.Toggle(Singleton<TrainerOptions>.Instance.AiNormalEnabled, " Normal", toggleStyle, GUILayout.Width(60)) != Singleton<TrainerOptions>.Instance.AiNormalEnabled)
            {
                Singleton<TrainerOptions>.Instance.AiNormalEnabled = !Singleton<TrainerOptions>.Instance.AiNormalEnabled;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
            GUILayout.Space(3);

            GUILayout.BeginHorizontal(horizontalSliderGroupStyle);

            GUILayout.BeginVertical(sliderStyle);
            GUILayout.Label("Damage");
            var aiDamageMultiplier = GUILayout.HorizontalSlider(Singleton<TrainerOptions>.Instance.AiDamageMultiplier, 1f, 5f);
            if (aiDamageMultiplier != Singleton<TrainerOptions>.Instance.AiDamageMultiplier)
            {
                Singleton<TrainerOptions>.Instance.AiDamageMultiplier = aiDamageMultiplier;
                SetBotStats();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(sliderStyle);
            GUILayout.Label("Punch Force");
            var aiPunchForce = GUILayout.HorizontalSlider(Singleton<TrainerOptions>.Instance.AiPunchForce, 120000f, 800000f);
            if (aiPunchForce != Singleton<TrainerOptions>.Instance.AiPunchForce)
            {
                Singleton<TrainerOptions>.Instance.AiPunchForce = aiPunchForce;
                SetBotStats();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(sliderStyle);
            GUILayout.Label("Punch Time");
            var aiPunchTime = GUILayout.HorizontalSlider(Singleton<TrainerOptions>.Instance.AiPunchTime, 0.1f, 0.50f);
            if (aiPunchTime != Singleton<TrainerOptions>.Instance.AiPunchTime)
            {
                Singleton<TrainerOptions>.Instance.AiPunchTime = aiPunchTime;
                SetBotStats();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(sliderStyle);
            GUILayout.Label("Speed");
            var aiMovementForceMultiplier = GUILayout.HorizontalSlider(Singleton<TrainerOptions>.Instance.AiMovementForceMultiplier, 2000f, 6000f);
            if (aiMovementForceMultiplier != Singleton<TrainerOptions>.Instance.AiMovementForceMultiplier)
            {
                Singleton<TrainerOptions>.Instance.AiMovementForceMultiplier = aiMovementForceMultiplier;
                SetBotStats();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(sliderStyle);
            GUILayout.Label("Jump Height");
            var aiMovementJumpForceMultiplier = GUILayout.HorizontalSlider(Singleton<TrainerOptions>.Instance.AiMovementJumpForceMultiplier, 25f, 100f);
            if (aiMovementJumpForceMultiplier != Singleton<TrainerOptions>.Instance.AiMovementJumpForceMultiplier)
            {
                Singleton<TrainerOptions>.Instance.AiMovementJumpForceMultiplier = aiMovementJumpForceMultiplier;
                SetBotStats();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.Space(3);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Player", buttonStyle, GUILayout.Width(75)))
            {
                SpawnBotEnemyPlayer();
            }

            if (GUILayout.Button("Zombie", buttonStyle, GUILayout.Width(75)))
            {
                SpawnBotEnemyZombie();
            }

            if (GUILayout.Button("Bolt", buttonStyle, GUILayout.Width(75)))
            {
                SpawnBotEnemyBrat();
            }

            GUILayout.EndHorizontal();

            // Display available shortcuts

            GUILayout.Space(10);
            GUILayout.Label("<color=grey><b>PC Keyboard Shortcuts</b></color>");
            GUILayout.Label("- Toggle Menu:\t[SHIFT] + [M]\r\n" +
                            "- Skip Map:\t[SHIFT] + [S]\r\n" +
                            "- Spawn Weapon:\t[R] or [P]\r\n" +
                            "- Browse Weapons:\t[Q] for previous or [E] for next");

            GUILayout.Space(5);
            GUILayout.Label("<color=grey><b>Xbox 360 Controller Shortcuts</b></color>");
            GUILayout.Label("- Toggle Menu:\t[RB] + [A]\r\n" +
                            "- Skip Map:\t[RB] + [B]\r\n" +
                            "- Spawn Weapon:\t[DPadUp] or [DPadDown]\r\n" +
                            "- Browse Weapons:\t[DPadLeft] or [DPadRight]");

            GUILayout.EndArea();
        }

        if (Singleton<TrainerOptions>.Instance.CheatsEnabled)
        {
            if (MultiplayerManager.mGameManager != null && MultiplayerManager.mGameManager.controllerHandler != null && MultiplayerManager.mGameManager.controllerHandler.players != null)
            {
                // Display healthbars and scores for all players
                foreach (var player in MultiplayerManager.mGameManager.controllerHandler.players)
                {
                    if (player != null && player.fighting != null)
                    {
                        if (Singleton<TrainerOptions>.Instance.DisplayHealthBars)
                        {
                            var health = 0f;
                            var healthHandler = player.fighting.GetComponent<HealthHandler>();
                            if (healthHandler != null)
                            {
                                health = healthHandler.health;
                            }

                            EditorGUITools.DrawRect(new Rect(Screen.width - 125, 30f * player.playerID + 10f, Math.Max(0f, health), 20f), GetPlayerColorByIndex(player.playerID), null);
                            GUI.Label(new Rect(Screen.width - 160, 30f * player.playerID + 10f, 250f, 25f), Math.Max(0.0, Math.Round(health)).ToString());
                        }

                        if (Singleton<TrainerOptions>.Instance.DisplayScore && player.fighting.stats != null)
                        {
                            GUI.Label(new Rect(Screen.width - 180, 30f * (float)player.playerID + 10f, 250f, 25f), "<b>" + player.fighting.stats.wins.ToString() + "</b>");
                        }
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
            if (!ChatManager.isTyping && ((Input.GetKeyUp(KeyCode.JoystickButton1) && Input.GetKey(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton1) && Input.GetKeyUp(KeyCode.JoystickButton5)) || ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.S))))
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
            if (!ChatManager.isTyping && ((Input.GetKeyUp(KeyCode.JoystickButton0) && Input.GetKeyUp(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton0) && Input.GetKey(KeyCode.JoystickButton5)) || (Input.GetKeyUp(KeyCode.JoystickButton0) && Input.GetKey(KeyCode.JoystickButton5)) || ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.M))))
            {
                Singleton<TrainerOptions>.Instance.DisplayTrainerMenu = !Singleton<TrainerOptions>.Instance.DisplayTrainerMenu;
            }

            // Spawn random weapon (Keyboard)
            if (!ChatManager.isTyping && Input.GetKeyUp(KeyCode.R))
            {
                SpawnRandomWeapon(false);
            }

            // Spawn random present (Keyboard)
            if (!ChatManager.isTyping && Input.GetKeyUp(KeyCode.P))
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
                        if (!ChatManager.isTyping && controller.mPlayerActions.activeDevice.DPadUp.WasReleased)
                        {
                            SpawnRandomWeapon(false);
                        }

                        // Spawn random present (Joystick)
                        if (!ChatManager.isTyping && controller.mPlayerActions.activeDevice.DPadDown.WasReleased)
                        {
                            SpawnRandomWeapon(true);
                        }

                        // Select next weapon for the requesting player
                        if (!ChatManager.isTyping && (controller.mPlayerActions.activeDevice.DPadLeft.IsPressed || (Input.GetKey(KeyCode.Q) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any))))
                        {
                            // Keep track of how long the key is pressed down for.
                            _keyHoldTime += Time.deltaTime;

                            // Action if the key was just pressed or has been held for a certain amount of time (fast / continuous scroll)
                            if (Input.GetKeyDown(KeyCode.Q) || controller.mPlayerActions.activeDevice.DPadLeft.WasPressed || _keyHoldTime > 0.5f)
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
                        }
                        else if ((Input.GetKeyUp(KeyCode.Q) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any)) || controller.mPlayerActions.activeDevice.DPadLeft.WasReleased)
                        {
                            _keyHoldTime = 0f;
                        }

                        // Select previous weapon for the requesting player
                        if (!ChatManager.isTyping && (controller.mPlayerActions.activeDevice.DPadRight.IsPressed || (Input.GetKey(KeyCode.E) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any))))
                        {
                            // Keep track of how long the key is pressed down for.
                            _keyHoldTime += Time.deltaTime;

                            // Action if the key was just pressed or has been held for a certain amount of time (fast / continuous scroll)
                            if (Input.GetKeyDown(KeyCode.E) || controller.mPlayerActions.activeDevice.DPadRight.WasPressed || _keyHoldTime > 0.5f)
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
                        else if ((Input.GetKeyUp(KeyCode.E) && (controller.mPlayerActions.mInputType == InputType.Keyboard || controller.mPlayerActions.mInputType == InputType.Any)) || controller.mPlayerActions.activeDevice.DPadRight.WasReleased)
                        {
                            _keyHoldTime = 0f;
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

    private void SetBotStats()
    {
        var playerControllers = new List<Controller>();
        playerControllers.AddRange(hoardHandlerBrat.charactersAlive);
        playerControllers.AddRange(hoardHandlerPlayer.charactersAlive);
        playerControllers.AddRange(hoardHandlerZombie.charactersAlive);
        playerControllers.AddRange(MultiplayerManager.mGameManager.controllerHandler.ActivePlayers);

        foreach (var player in playerControllers)
        {
            if (player.isAI)
            {
                player.fighting.punchTime = Singleton<TrainerOptions>.Instance.AiPunchTime;
                player.fighting.punchForce = Singleton<TrainerOptions>.Instance.AiPunchForce;
                player.movement.forceMultiplier = Singleton<TrainerOptions>.Instance.AiMovementForceMultiplier;
                player.movement.jumpForceMultiplier = Singleton<TrainerOptions>.Instance.AiMovementJumpForceMultiplier;

                // Set punch damage dealt by bots
                var punchForceComponents = player.gameObject.GetComponentsInChildren<PunchForce>();
                foreach (var punchForceComponent in punchForceComponents)
                {
                    punchForceComponent.damageMultiplier = Singleton<TrainerOptions>.Instance.AiDamageMultiplier;
                }

                if (player.gameObject.name == "ZombieCharacterArms(Clone)")
                {
                    // Set grab damage dealt by Zombie bots
                    var reachForPlayerComponents = player.fighting.gameObject.GetComponentsInChildren<ReachForPlayer>();
                    foreach (var reachForPlayerComponent in reachForPlayerComponents)
                    {
                        reachForPlayerComponent.damage = Singleton<TrainerOptions>.Instance.AiDamageMultiplier * 3f;
                    }
                }
            }
            else
            {
                // Set weapon damage received from bots
                var bodyPartComponents = player.gameObject.GetComponentsInChildren<BodyPart>();
                foreach (var bodyPartComponent in bodyPartComponents)
                {
                    bodyPartComponent.multiplier = Singleton<TrainerOptions>.Instance.AiDamageMultiplier;
                }
            }
        }
    }

    /// <summary>
    /// Spawn an NPC that deals and takes damage. 
    /// </summary>
    private void SpawnBotPlayer(GameObject playerPrefab)
    {
        if (MultiplayerManager.mGameManager.controllerHandler.ActivePlayers.Count >= 4)
        {
            return;
        }

        var spawnPosition = Vector3.up * 8f;
        var spawnRotation = Quaternion.identity;
        var playerId = MultiplayerManager.mGameManager.controllerHandler.ActivePlayers.Count;
        var playerColors = MultiplayerManagerAssets.Instance.Colors;
        var playerObject = UnityEngine.Object.Instantiate<GameObject>(playerPrefab, spawnPosition, spawnRotation);
        var playerController = playerObject.GetComponent<Controller>();

        // Load player prefab component
        var playerPrefabSetMovementAbilityComponent = MultiplayerManagerAssets.Instance.PlayerPrefab.GetComponent<SetMovementAbility>();

        // Add required SetMovementAbility if it's missing.
        if (playerObject.GetComponent<SetMovementAbility>() == null)
        {
            var hoardHandlerSetMovementAbilityComponent = playerObject.AddComponent<SetMovementAbility>();
            hoardHandlerSetMovementAbilityComponent.abilities = playerPrefabSetMovementAbilityComponent.abilities;
            hoardHandlerSetMovementAbilityComponent.bossHealth = playerPrefabSetMovementAbilityComponent.bossHealth;
        }

        var playerLineRenderers = playerObject.GetComponentsInChildren<LineRenderer>();
        for (var i = 0; i < playerLineRenderers.Length; i++)
        {
            playerLineRenderers[i].sharedMaterial = playerColors[playerId];
        }

        var playerSpriteRenderers = playerObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in playerSpriteRenderers)
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
#if V24_PRE
        MultiplayerManager.mGameManager.RevivePlayer(playerController);
#endif

#if V24_POST
        MultiplayerManager.mGameManager.RevivePlayer(playerController, true);
#endif
    }

    private void SpawnBotEnemyPlayer()
    {
        if (Singleton<TrainerOptions>.Instance.SpawnPcEnabled)
        {
            SpawnBotPlayer(MultiplayerManagerAssets.Instance.PlayerPrefab);
        }
        else
        {
            MultiplayerManager.mGameManager.hoardHandler.SpawnAI(hoardHandlerPlayer.character);
        }

        SetBotStats();
    }

    private void SpawnBotEnemyZombie()
    {
        if (Singleton<TrainerOptions>.Instance.SpawnPcEnabled)
        {
            SpawnBotPlayer(hoardHandlerZombie.character);
        }
        else
        {
            MultiplayerManager.mGameManager.hoardHandler.SpawnAI(hoardHandlerZombie.character);
        }

        SetBotStats();
    }

    private void SpawnBotEnemyBrat()
    {
        if (Singleton<TrainerOptions>.Instance.SpawnPcEnabled)
        {
            SpawnBotPlayer(hoardHandlerBrat.character);
        }
        else
        {
            MultiplayerManager.mGameManager.hoardHandler.SpawnAI(hoardHandlerBrat.character);
        }

        SetBotStats();
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
#if V1_8_PRE
        GetComponent<GameManager>().mNetworkManager.SpawnWeapon(randomWeaponIndex, vector);
#endif

#if V1_8_POST
        GetComponent<GameManager>().mNetworkManager.SpawnWeapon(randomWeaponIndex, vector, spawnAsPresent);
#endif
            return;
        }

        // Spawn weapon locally
        var instantiatedWeapon = Instantiate<GameObject>(weapon, vector, Quaternion.identity);

        if (spawnAsPresent)
        {
#if V1_8_POST
        instantiatedWeapon.GetComponent<WeaponPickUp>().ChangeToPresent();
#endif
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
        GUILayout.BeginArea(new Rect(x, y, width, height), new GUIStyle(GUI.skin.box));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", new GUIStyle(GUI.skin.button), new GUILayoutOption[]
        {
            GUILayout.Width(60),
        }))
        {
            _unityLogs.Length = 0;
        }
        _unityLogsEnabled = GUILayout.Toggle(_unityLogsEnabled, " Logs Enabled", GUILayout.Width(100));
        GUILayout.EndHorizontal();
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
        if (_unityLogsEnabled == false){
            return;
        }

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