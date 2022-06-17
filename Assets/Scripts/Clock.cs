using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Clock : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] private GameObject _hours;
    [SerializeField] private GameObject _minutes;
    [SerializeField] private GameObject _seconds;
    [Header("Digital Clock")]
    [SerializeField] private TextMeshProUGUI _digitalClock;
    private DateTime _currentTime;

    private enum Provider
    {
        WorldTimeApi,
        UnixTime
    }

    private struct WorldTimeApi
    {
        public string datetime;
        public double unixtime;
    }

    private struct UnixTime
    {
        public double UnixTimeStamp;
    }

    private void Awake()
    {
        StartCoroutine(SetTimeFromServer("http://worldtimeapi.org/api/ip", Provider.WorldTimeApi));
    }

    private void Start()
    {
        SetTime(_currentTime);
    }
    private void Update()
    {
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            Debug.Log(ScreenOrientation.LandscapeLeft);
            _digitalClock.transform.localPosition = new Vector3(-700, 0);
        }
        else if(Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            Debug.Log(ScreenOrientation.LandscapeRight);
            _digitalClock.transform.localPosition = new Vector3(700, 0);
        }
        else
        {
            _digitalClock.transform.localPosition = new Vector3(0, 600);
        }
        _currentTime = _currentTime.AddSeconds(Time.deltaTime);
        SetTime(_currentTime);
    }

    private void SetTime(DateTime time)
    {
        SetTikTokClockTime(time);
        SetDigitalClockTime(time);
    }

    private void SetTikTokClockTime(DateTime time)
    {
        float hours = time.Hour;
        if (hours > 12) hours -= 12;
        hours +=  ((float)time.Minute / (float)60);
        _seconds.transform.localEulerAngles = new Vector3(0, 0, -(time.Second) * 360 / 60);
        _minutes.transform.localEulerAngles = new Vector3(0, 0, -(time.Minute * 60) * 360 / 60 / 60);
        _hours.transform.localEulerAngles = new Vector3(0, 0, -hours/12*360);
        if (_hours.transform.localEulerAngles.z == 0.0f)
        {
            StartCoroutine(SetTimeFromServer("https://showcase.api.linx.twenty57.net/UnixTime/tounixtimestamp?datetime=now", Provider.UnixTime));
        }
    }

    private void SetDigitalClockTime(DateTime time)
    {
        _digitalClock.text = time.ToString("HH") + ":" + time.ToString("mm") + ":" + time.ToString("ss");
    }

    private IEnumerator SetTimeFromServer(string url, Provider provider)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.timeout = 3;

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError)
        {
            Debug.Log("Error: " + webRequest.error);
        }
        else
        {
            switch (provider)
            {
                case Provider.WorldTimeApi:
                    Debug.Log("Time : " + webRequest.downloadHandler.text);
                    WorldTimeApi worldTimeApi = JsonUtility.FromJson<WorldTimeApi>(webRequest.downloadHandler.text);
                    _currentTime = new DateTime(1970, 1, 1, 3, 0, 0, 0).AddSeconds(worldTimeApi.unixtime);
                    break;
                case Provider.UnixTime:
                    Debug.Log("Time : " + webRequest.downloadHandler.text);
                    UnixTime unixTime = JsonUtility.FromJson<UnixTime>(webRequest.downloadHandler.text);
                    _currentTime = new DateTime(1970, 1, 1, 3, 0, 0, 0).AddSeconds(unixTime.UnixTimeStamp);
                    break;
            }
            
        }

    }
}
