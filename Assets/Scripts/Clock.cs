using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class Clock : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] private GameObject _hours;
    [SerializeField] private GameObject _minutes;
    [SerializeField] private GameObject _seconds;
    [Header("Digital Clock")]
    [SerializeField] private TextMeshProUGUI _digitalClock;
    [SerializeField] private GameObject _digitalClockAlarm;
    [SerializeField] private TMP_InputField _digitalClockInput;
    

    [Header("AlarmClock")]
    [SerializeField] private GameObject _alarmClock;
    [SerializeField] private TextMeshProUGUI _alarmButtonText;
    [SerializeField] private TextMeshProUGUI _alarmText;

    private DateTime _currentTime;
    private DateTime _alarmTime;
    private bool _editMode = false;
    private GameObject _selectedObject;

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
        _digitalClockInput.onValueChanged.AddListener(delegate { OnEndEdit(); });
    }

    private void Start()
    {
        SetTime(_currentTime);
    }
    private void Update()
    {
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            _digitalClock.transform.localPosition = new Vector3(-700, 0);
            _digitalClockAlarm.transform.localPosition = new Vector3(-700, 0);
            _alarmClock.transform.localPosition = new Vector3(700, 0);
        }
        else if(Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            _digitalClock.transform.localPosition = new Vector3(700, 0);
            _digitalClockAlarm.transform.localPosition = new Vector3(700, 0);
            _alarmClock.transform.localPosition = new Vector3(-700, 0);
        }
        else
        {
            _digitalClock.transform.localPosition = new Vector3(0, 600);
            _digitalClockAlarm.transform.localPosition = new Vector3(0, 600);
            _alarmClock.transform.localPosition = new Vector3(0, -600);
        }
        _currentTime = _currentTime.AddSeconds(Time.deltaTime);
        if (!_editMode)
        {
            SetTime(_currentTime);
        }
        else
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
                if (targetObject)
                {
                    _selectedObject = targetObject.gameObject;
                    
                }
            }

            if (_selectedObject)
            {
                var angle = Vector2.Angle(new Vector2(0, 1), mousePosition);
                if (mousePosition.x > 0) angle *= -1;

                _selectedObject.transform.localEulerAngles = new Vector3(0, 0, angle);
                DateTime time = GetArrowsTime();
                Debug.Log(time.ToString("hh"));
                _digitalClockInput.text = time.ToString("hh") + ":" + time.ToString("mm") + ":" + time.ToString("ss");
            }
        }

        if (Input.GetMouseButtonUp(0) && _selectedObject)
        {
            _selectedObject = null;
        }
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

    public void SetAlarmClock()
    {
        if (!_editMode)
        {
            _alarmButtonText.text = "Задать";
            _digitalClock.gameObject.SetActive(false);
            _digitalClockInput.gameObject.SetActive(true);
            _digitalClockInput.text = "";
        }
        else
        {
            _alarmButtonText.text = "Будильник";
            _digitalClock.gameObject.SetActive(true);
            _digitalClockInput.gameObject.SetActive(false);
        }
        if (!string.IsNullOrEmpty(_digitalClockInput.text))
        {
            
            _alarmText.text = _digitalClockInput.text;
            StartCoroutine(SetTimeFromServer("https://showcase.api.linx.twenty57.net/UnixTime/tounixtimestamp?datetime=now", Provider.UnixTime));
        }

        _editMode = !_editMode;
        
    }

    public void OnEndEdit()
    {
        if (string.IsNullOrEmpty(_digitalClockInput.text))
        {
            _digitalClockInput.text = string.Empty;
        }
        else
        {

            char[] input;
            input = _digitalClockInput.text.ToCharArray();
            //Debug.Log(input[0].ToString());
            if (input.Length > 2) input[2] = ':';
            if (input.Length > 5) input[5] = ':';
            //Debug.Log((int)input[0] > 2);
            if (int.Parse(input[0].ToString()) > 2) input[0] = '2';
            if (input.Length > 1) { if (int.Parse(input[0].ToString()) > 1 && int.Parse(input[1].ToString()) > 3) input[1] = '3'; }
            if (input.Length > 3) { if (int.Parse(input[3].ToString()) > 5) input[3] = '5'; }
            if (input.Length > 6) { if (int.Parse(input[6].ToString()) > 5) input[6] = '5'; }
            string newAlarm = new string(input);
            _digitalClockInput.text = newAlarm;

        }
    }

    private DateTime GetArrowsTime()
    {
        float seconds;
        float minutes;
        float hours;

        if (_hours.transform.eulerAngles.z > -180) hours = (60 - Mathf.Abs(60 * _hours.transform.localEulerAngles.z / 360)) / 5;
        else hours = Mathf.Abs(60 * _hours.transform.localEulerAngles.z / 360) / 5;

        if (_minutes.transform.eulerAngles.z > -180) minutes = (60 - Mathf.Abs(60 * _minutes.transform.localEulerAngles.z / 360));
        else minutes = Mathf.Abs(60 * _minutes.transform.localEulerAngles.z / 360);

        if (_seconds.transform.eulerAngles.z > -180) seconds = (60 - Mathf.Abs(60 * _seconds.transform.localEulerAngles.z / 360));
        else seconds = Mathf.Abs(60 * _seconds.transform.localEulerAngles.z / 360);

        return new DateTime(_currentTime.Year, _currentTime.Month, _currentTime.Day, (int)hours, (int)minutes, (int)seconds);
    }
}
