using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Clock : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] private GameObject _hours;
    [SerializeField] private GameObject _minutes;
    [SerializeField] private GameObject _seconds;
    [SerializeField] private TextMeshProUGUI _digitalClock;
    private DateTime _currentTime;

    private struct WorldTimeApi
    {
        public string datetime;
        public double unixtime;
    }

    private void Awake()
    {
        StartCoroutine(SetTimeFromServer("http://worldtimeapi.org/api/ip"));
    }

    private void Start()
    {
        SetTime(_currentTime);
    }
    private void Update()
    {
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
            StartCoroutine(SetTimeFromServer("http://worldtimeapi.org/api/ip"));
        }
    }

    private void SetDigitalClockTime(DateTime time)
    {
        _digitalClock.text = time.ToString("HH") + ":" + time.ToString("mm") + ":" + time.ToString("ss");
    }

    private IEnumerator SetTimeFromServer(string url)
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
            Debug.Log("Time : " + webRequest.downloadHandler.text);
            WorldTimeApi worldTimeApi = JsonUtility.FromJson<WorldTimeApi>(webRequest.downloadHandler.text);
            _currentTime = new DateTime(1970, 1, 1, 3, 0, 0, 0).AddSeconds(worldTimeApi.unixtime);
        }

    }
}
