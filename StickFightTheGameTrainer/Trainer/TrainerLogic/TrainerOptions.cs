using System;
using Steamworks;

public class TrainerOptions : Singleton<TrainerOptions>
{
    public bool TrainerActive;

    public bool DisplayTrainerMenu;

    public bool DisplayHealthBars;

    public bool FlightMode;

    public bool UncappedFirerate;

    public bool NoRecoil;

    public bool DisplayScore;

    public bool NoWinners;

    public bool UnlimitedAmmo;

    public bool FullAuto;

    public bool UnlimitedHealth;

    public float AiDamageMultiplier;
    
    public float AiPunchForce;

    public float AiPunchTime;

    public float AiMovementForceMultiplier;

    public float AiMovementJumpForceMultiplier;

    private bool _spawnPcEnabled;

    private bool _spawnNpcEnabled;

    private bool _aiAggressiveEnabled;

    private bool _aiNormalEnabled;
    
    public bool SpawnPcEnabled
    {
        get
        {
            return _spawnPcEnabled;
        }

        set
        {
            _spawnPcEnabled = value;
            _spawnNpcEnabled = !value;
        }
    }

    public bool SpawnNpcEnabled
    {
        get
        {
            return _spawnNpcEnabled;
        }

        set
        {
            _spawnNpcEnabled = value;
            _spawnPcEnabled = !value;
        }
    }
    
    public bool AiAggressiveEnabled
    {
        get
        {
            return _aiAggressiveEnabled;
        }

        set
        {
            _aiAggressiveEnabled = value;
            _aiNormalEnabled = !value;
        }
    }

    public bool AiNormalEnabled
    {
        get
        {
            return _aiNormalEnabled;
        }

        set
        {
            _aiNormalEnabled = value;
            _aiAggressiveEnabled = !value;
        }
    }
    
    public bool CheatsEnabled
    {
        get
        {
            return !global::MatchmakingHandler.Instance.IsInsideLobby || this.TrainerActive;
        }
    }

    public TrainerOptions()
    {
        this.TrainerActive = false;
        this.DisplayHealthBars = true;
        this.DisplayScore = true;
        this.DisplayTrainerMenu = true;
        this.AiDamageMultiplier = 1f;
        this.AiPunchForce = 120000f;
        this.AiPunchTime = 0.1f;
        this.AiMovementForceMultiplier = 4000f;
        this.AiMovementJumpForceMultiplier = 25.0f;
        this._spawnPcEnabled = true;
        this._aiAggressiveEnabled = true;
    }

    public string GetVersion(string currentVersion)
    {
        if (!this.TrainerActive)
        {
            return currentVersion;
        }
        return currentVersion + "[loxa_V{Application.ProductVersion}]";
    }

    public void ResetOptions()
    {
        FlightMode = false;
        UncappedFirerate = false;
        NoRecoil = false;
        UnlimitedAmmo = false;
        FullAuto = false;
        UnlimitedHealth = false;
    }

    public void Awake()
    {
    }

    public void Update()
    {
    }
}