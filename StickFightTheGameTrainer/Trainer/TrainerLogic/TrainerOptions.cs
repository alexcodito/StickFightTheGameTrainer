using System;
using Steamworks;

public class TrainerOptions : Singleton<TrainerOptions>
{
    public string GetVersion(string currentVersion)
    {
        if (!this.TrainerActive)
        {
            return currentVersion;
        }
        return currentVersion + "[loxa_V{Application.ProductVersion}]";
    }

    public void Awake()
    {
    }

    public void Update()
    {
    }

    public TrainerOptions()
    {
        this.DisplayFps = true;
        this.TrainerActive = false;
        this.DisplayHealthBars = true;
        this.DisplayScore = true;
        this.DisplayTrainerMenu = true;
    }

    public bool CheatsEnabled
    {
        get
        {
            return !global::MatchmakingHandler.Instance.IsInsideLobby || this.TrainerActive;
        }
    }

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

    public bool DisplayFps;

    public bool UnlimitedHealth;
}