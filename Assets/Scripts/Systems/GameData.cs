using System;
using UnityEngine;

public class GameData
{
    public delegate void GameDataEvent(object Message);
    public event GameDataEvent ChangeLanguageEvent;
    public event GameDataEvent ChangeAutorizedEvent;
    public event GameDataEvent ChangePauseState;
    public event GameDataEvent ChangeAudioState;
    public event GameDataEvent RewardVideoEvent;

    public bool Autorized {get; private set;}
    public string PlayerID;
    public string PlayerName;
    public string JoinCode;
    public bool IsPaused {get; private set;}
    public bool AudioState {get; private set;}

    // public CarMoveComponentParams MoveComponentParams = new CarMoveComponentParams();
    // public StickmanSpawnerParams StickmanSpawnerParams = new StickmanSpawnerParams();
    // public GameTimersParams GameTimersParams = new GameTimersParams();

    public EnumLanguage Lang = EnumLanguage.EN;
    public EnumPlatforms Platform;
    public EnumGameState GameState;

    private Action _authAction;
    public Action AuthAction {get => _authAction; set => _authAction ??= value;}

    private Action _ADBanner;
    public Action ADBanner {get => _ADBanner; set => _ADBanner ??= value;}

    private Action<int> _ADReward;
    public Action<int> ADReward {get => _ADReward; set => _ADReward ??= value;}

    private Action _StopGame;
    public Action StopGame {get => _StopGame; set => _StopGame ??= value;}

    private Action _StartGame;
    public Action StartGame {get => _StartGame; set => _StartGame ??= value;}

    public void ChangeLanguage(EnumLanguage Lang)
    {
        this.Lang = Lang;
        ChangeLanguageEvent?.Invoke(Lang);
    }

    public void ChangeGameState(EnumGameState State)
    {
        GameState = State;

        switch (State)
        {
            case EnumGameState.Start :
                StartGame?.Invoke();
                break;
            case EnumGameState.Stop :
                StopGame?.Invoke();
                break;
            default: break;
        }
    }

    public void AuthWidget()
    {
        AuthAction?.Invoke();
    }

    public void SetAuthState(bool Switch)
    {
        Autorized = Switch;
        ChangeAutorizedEvent?.Invoke(Autorized);
    }

    public void SetPause(bool Switch)
    {
        Time.timeScale = Switch == true ? 0 : 1;
        IsPaused = Switch;
        ChangePauseState?.Invoke(IsPaused);
        ChangeGameState(IsPaused ? EnumGameState.Stop : EnumGameState.Start);
    }

    public void SetAudioState(bool Switch)
    {
        AudioState = Switch;
        ChangeAudioState?.Invoke(Switch);
    }

    public void StartRewardVideoEvent(int Number)
    {
        RewardVideoEvent?.Invoke(true);
    }
}