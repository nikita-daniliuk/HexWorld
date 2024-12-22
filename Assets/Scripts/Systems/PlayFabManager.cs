using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Zenject;
using System.Linq;

public class PlayFabManager
{
    [Inject] EventBus EventBus;
    [Inject] GameData GameData;

    private string PlayFabId;

    public void Login()
    {
        var Request = new LoginWithCustomIDRequest
        {
            #if !UNITY_EDITOR
            CustomId = SystemInfo.deviceUniqueIdentifier,
            #else
            CustomId = "Test",
            #endif
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(Request, OnSuccess, LoginError);
    }

    public void GetTitleData(List<string> Keys)
    {
        var Request = new GetTitleDataRequest
        {
            Keys = Keys
        };
        PlayFabClientAPI.GetTitleData(Request, OnGetTitleDataSuccess, OnError);
    }

    void OnGetTitleDataSuccess(GetTitleDataResult Result)
    {
        if (Result.Data == null) return;

        Debug.Log("Global title data received!");

        EventBus.Invoke(new TitleDataSignal(Result.Data));
    }

    public bool IsItMyId(string PlayFabId)
    {
        return this.PlayFabId == PlayFabId;
    }

    public void SetData(Dictionary<string, string> Data)
    {
        var Request = new UpdateUserDataRequest
        {
            Data = Data
        };
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.UpdateUserData(Request, UpdateDataSuccess, OnError);
        }
        else
        {
            EventBus.Invoke(EnumPlayFabSignals.LoginError);
            Debug.Log("Not logged in!");
        }
    }

    public void GetData(List<string> Keys)
    {
        var Request = new GetUserDataRequest
        {
            Keys = Keys
        };
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.GetUserData(Request, GetDataSuccess, OnError);
        }
        else
        {
            EventBus.Invoke(EnumPlayFabSignals.LoginError);
            Debug.Log("Not logged in!");
        }
    }

    public void UpdatePlayerStatistics(Dictionary<string, int> Statistics)
    {
        string PlayerName = string.IsNullOrEmpty(GameData.PlayerName) ? GenerateUniqueDisplayName() : GameData.PlayerName;
        
        var StatisticsRequest = new UpdatePlayerStatisticsRequest
        {
            Statistics = Statistics.Select(stat => new StatisticUpdate
            {
                StatisticName = stat.Key,
                Value = stat.Value
            }).ToList()
        };

        var DisplayNameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = PlayerName
        };

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(DisplayNameRequest, DisplayNameResult =>
            {
                Debug.Log("Display name updated!");
                PlayFabClientAPI.UpdatePlayerStatistics(StatisticsRequest, UpdateStatisticsSuccess, OnError);
            }, Error =>
            {
                if (Error.Error == PlayFabErrorCode.NameNotAvailable)
                {
                    DisplayNameRequest.DisplayName = GenerateUniqueDisplayName();
                    PlayFabClientAPI.UpdateUserTitleDisplayName(DisplayNameRequest, DisplayNameResult =>
                    {
                        Debug.Log("Display name updated after retry!");
                        PlayFabClientAPI.UpdatePlayerStatistics(StatisticsRequest, UpdateStatisticsSuccess, OnError);
                    }, OnError);
                }
                else
                {
                    OnError(Error);
                }
            });
        }
        else
        {
            EventBus.Invoke(EnumPlayFabSignals.LoginError);
            Debug.Log("Not logged in!");
        }
    }

    public void GetTopHostsData()
    {
        // Подготовка запроса к Cloud Script
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest
        {
            FunctionName = "getTopHostsData", // Имя функции в Cloud Script
            GeneratePlayStreamEvent = true // Включение PlayStream (опционально)
        };

        // Отправка запроса
        PlayFabClientAPI.ExecuteCloudScript(request, OnGetTopHostsData, OnError);
    }

    [System.Serializable]
    public class HostsWrapper
    {
        //public List<HostData> hosts;
    }

    // Успешный ответ от Cloud Script
    void OnGetTopHostsData(ExecuteCloudScriptResult result)
    {
        if(result.FunctionResult != null)
        {
            // Парсим JSON в обёртку с массивом хостов
            HostsWrapper hostWrapper = JsonUtility.FromJson<HostsWrapper>(result.FunctionResult.ToString());

            // Преобразуем в List<HostData>
            //List<HostData> hostDataList = hostWrapper.hosts;

            // Передаем данные через EventBus
            //EventBus.Invoke(new SpecificPlayerDataSignal(hostDataList));
        }
    }

    public void UpdateStatisticsUsingCloudScript()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "updateStatisticsWithServerTime",
            FunctionParameter = new Dictionary<string, object>
            {
                { "dummyData", "You can pass any data if needed" }
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, OnCloudScriptSuccess, OnError);
    }

    private void OnCloudScriptSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Cloud Script выполнен успешно: " + result.FunctionResult);
    }

    public void GetPlayerStatistics(List<string> Keys)
    {
        var Request = new GetPlayerStatisticsRequest
        {
            StatisticNames = Keys
        };

        PlayFabClientAPI.GetPlayerStatistics(Request, OnGetPlayerStatisticsSuccess, OnError);
    }

    public void GetLeaderboardAroundPlayer(string StatisticName, int MaxResultsCountAboveBelow)
    {
        int MaxResultsCount = (MaxResultsCountAboveBelow * 2) + 1;
        var RequestData = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = StatisticName,
            MaxResultsCount = MaxResultsCount
        };

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.GetLeaderboardAroundPlayer(RequestData, GetLeaderboardAroundPlayerSuccess, OnError);
        }
        else
        {
            EventBus.Invoke(EnumPlayFabSignals.LoginError);
            Debug.Log("Not logged in!");
        }
    }

    public void GetTopPlayers(string StatisticName, int MaxResultsCount)
    {
        var RequestData = new GetLeaderboardRequest
        {
            StatisticName = StatisticName,
            StartPosition = 0,
            MaxResultsCount = MaxResultsCount
        };

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.GetLeaderboard(RequestData, GetLeaderboardSuccess, OnError);
        }
        else
        {
            EventBus.Invoke(EnumPlayFabSignals.LoginError);
            Debug.Log("Not logged in!");
        }
    }

    private void UpdateStatisticsSuccess(UpdatePlayerStatisticsResult Result)
    {
        Debug.Log("Statistics updated!");
    }

    private void OnSuccess(LoginResult Result)
    {
        PlayFabId = Result.PlayFabId;
        EventBus.Invoke(EnumPlayFabSignals.LoginSucces);
        Debug.Log("Login/Account successful!");
    }

    private void OnGetPlayerStatisticsSuccess(GetPlayerStatisticsResult Result)
    {
        EventBus.Invoke(Result);
        Debug.Log("Statistics data received!");
    }

    private void GetLeaderboardAroundPlayerSuccess(GetLeaderboardAroundPlayerResult Result)
    {
        EventBus.Invoke(Result);
        Debug.Log("Leaderboard around player received!");
    }

    private void GetLeaderboardSuccess(GetLeaderboardResult Result)
    {
        EventBus.Invoke(Result);
        Debug.Log("Leaderboard received!");
    }

    private void UpdateDataSuccess(UpdateUserDataResult Result)
    {
        EventBus.Invoke(EnumPlayFabSignals.UpdateData);
        Debug.Log("Data saved!");
    }

    private void GetDataSuccess(GetUserDataResult Result)
    {
        EventBus.Invoke(Result);
        Debug.Log("Data received!");
    }

    void LoginError(PlayFabError Error)
    {
        EventBus.Invoke(EnumPlayFabSignals.LoginError);
    }

    private void OnError(PlayFabError Error)
    {
        Debug.Log($"Error {Error.GenerateErrorReport()}");
        EventBus.Invoke(Error);
    }

    private string GenerateUniqueDisplayName()
    {
        string BaseName = GameData.Lang == EnumLanguage.RU ? "Инкогнито" : "Incognita";

        string Suffix = $"{Random.Range(0,9)}{Random.Range(0,9)}{Random.Range(0,9)}{Random.Range(0,9)}{Random.Range(0,9)}{Random.Range(0,9)}";

        string FullName = $"{BaseName}_{Suffix}";

        if (FullName.Length > 25)
        {
            FullName = FullName.Substring(0, 25);
        }

        return FullName;
    }
}