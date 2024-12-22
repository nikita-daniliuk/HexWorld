using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class DataSender : MonoBehaviour
{
    private string url = "https://blades-n-brawls.hhos.net/insert.php";
    private string getDataUrl = "https://blades-n-brawls.hhos.net/get.php"; // URL для получения данных

    void Start()
    {
        SendData("1","Pisia",100,"2131231", DateTime.UtcNow.ToString());
        StartCoroutine(GetData());
    }

    public void SendData(string id, string hostName, int rating, string joinCode, string date)
    {
        StartCoroutine(PostData(id, hostName, rating, joinCode, date));
    }

    private IEnumerator PostData(string id, string hostName, int rating, string joinCode, string date)
    {
        // Создание формы
        WWWForm form = new WWWForm();
        form.AddField("ID", id);
        form.AddField("HostName", hostName);
        form.AddField("Rating", rating);
        form.AddField("JoinCode", joinCode);
        form.AddField("Date", date);

        // Отправка POST-запроса
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            // Проверка на ошибки
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Ошибка: " + www.error);
            }
            else
            {
                Debug.Log("Данные успешно отправлены: " + www.downloadHandler.text);
            }
        }
    }

    private IEnumerator GetData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(getDataUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Ошибка получения данных: " + www.error);
            }
            else
            {
                Debug.Log("Данные успешно получены: " + www.downloadHandler.text);
            }
        }
    }
}