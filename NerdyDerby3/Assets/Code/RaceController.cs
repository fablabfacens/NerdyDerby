﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using System.IO;
using System.IO.Ports;
using UnityEngine.UI;
using System;


public class racerPosition
{
    public int track;
    public int time;
}

public class RaceController : MonoBehaviour
{
    SerialPort arduino;
    private WebCamTexture backCam;
    public Image[] images;
    public InputField[] names;
    public Text[] nameText;
    public Button[] remove;

    public Dictionary<int, string> racers;
    Coroutine qrReader;
    public Button racerButton;
    public Sprite placeholder;
    public GameObject popUp;
    bool racing = false;
    public int timesRecived = 0;
    string message = "";
    public GameObject rankingPanel;

    public GameObject positionPanel;
    public Text[] positionText;
    public Text[] timesText;
    public Dictionary<int, Coroutine> trackGif;
    List<racerPosition> racersTimes;



    // Start is called before the first frame update
    void Start()
    {
        trackGif = new Dictionary<int, Coroutine>();
        racersTimes = new List<racerPosition>();
        string arduinoCOM = PlayerPrefs.GetString("ArduinoCOMPista");
        Debug.Log(arduinoCOM);

        if (arduinoCOM == "")
        {
            Debug.Log("Invalid Arduino Port");
        }
        arduino = new SerialPort(arduinoCOM, 9600);
        arduino.ReadTimeout = 50;
        arduino.Open();

        if (arduino.IsOpen)
        {
            Debug.Log("arduino OK");
        }
        else
        {
            Debug.Log("Can't open Arduino");
        }
        //InvokeRepeating("sendData", 1f, 1f);

    }
    IEnumerator RacerQRRead()
    {

        while (racers.Count < 3)
        {
            Result result;
            do
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                result = barcodeReader.Decode(backCam.GetPixels32(), backCam.width, backCam.height);
                if (result != null)
                {
                    Debug.Log("QR: " + result.Text);
                    if (!File.Exists("CarPics/" + result.Text + "/" + result.Text + "0.png"))
                    {
                        popUp.SetActive(true);
                        popUp.GetComponentInChildren<Text>().text = "Racer not registred";
                        result = null;
                    }
                    else if (racers.ContainsValue(result.Text.ToUpper()) == true)
                    {
                        //Debug.Log("Player Already in Race");
                        popUp.SetActive(true);
                        popUp.GetComponentInChildren<Text>().text = "Racer Already in Race";
                        result = null;
                    }
                    else
                    {
                        AddRacer(result.Text, -1);
                    }
                }
                yield return new WaitForEndOfFrame();
            } while (result == null);
            yield return new WaitForEndOfFrame();
        }
    }


    public void ManualInputCode(int track)
    {

        string _code = names[track].text;
        Debug.Log("Manual: " +_code);

        if (!File.Exists("CarPics/" + _code + "/" + _code + "0.png"))
        {
            popUp.SetActive(true);
            popUp.GetComponentInChildren<Text>().text = "Racer not registred";

        }
        else if (racers.ContainsValue(_code.ToUpper()) == true)
        {
            //Debug.Log("Player Already in Race");
            popUp.SetActive(true);
            popUp.GetComponentInChildren<Text>().text = "Racer Already in Race";

        }
        else
        {
            AddRacer(_code, track);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!arduino.IsOpen)
        {
            Debug.Log("Arduino OFF");
        }
        try
        {
            message = arduino.ReadLine();
        }
        catch (TimeoutException) { }

        if (message != "")
        {
            if (racing)
            {
                String[] messageSplited = message.Split(' ');

                int.TryParse(messageSplited[1], out int time);
                if (messageSplited[0].Equals("track0:"))
                {
                    racerPosition r = new racerPosition();
                    r.time = time;
                    r.track = 0;
                    racersTimes.Add(r);
                    SaveTime(0, time);


                    //Debug.Log(time);
                    timesRecived++;
                }
                else if (messageSplited[0].Equals("track1:"))
                {
                    racerPosition r = new racerPosition();
                    r.time = time;
                    r.track = 1;
                    racersTimes.Add(r);

                    SaveTime(1, time);
                    //Debug.Log(time);
                    timesRecived++;
                }
                else if (messageSplited[0].Equals("track2:"))
                {
                    racerPosition r = new racerPosition();
                    r.time = time;
                    r.track = 2;
                    racersTimes.Add(r);
                    SaveTime(2, time);
                    //Debug.Log(time);
                    timesRecived++;
                }

                if (timesRecived == racers.Count)
                {
                    StartCoroutine(ShowPositions());

                    //Debug.Log("Active Ranking");
                }
            }
        }
        message = "";
    }
    public void StartRace()
    {
        if (!racing)
        {
            arduino.Write("RACE");
            racing = true;
            timesRecived = 0;
            racerButton.GetComponentInChildren<Text>().text = "STOP";
            for (int i = 0; i < 3; i++)
            {
                remove[i].interactable = false;
            }
            StopAllCoroutines();
        }
        else
        {
            arduino.Write("STOP");
            //racerButton.interactable = false;
            //racerButton.GetComponentInChildren<Text>().text = "RACE";

            //for (int i = 0; i < 3; i++)
            //{
            //    RemoveRacer(i);
            //}
            //racers.Clear();

            //StartCoroutine(RacerQRRead());
            //Debug.Log("Active Ranking");
            //rankingPanel.SetActive(true);
        }

    }
    IEnumerator ReadImage(string path, int track)
    {
        int cont = 0;
        AspectRatioFitter arf;
        byte[] file;
        Texture2D img;
        float ratio;

        List<Sprite> gif = new List<Sprite>();


        for (int i = 0; i < 23; i++)
        {

            //Debug.Log(path + " " + track);
            arf = images[track].gameObject.GetComponent<AspectRatioFitter>();
            file = File.ReadAllBytes("CarPics/" + path + "/" + path + i + ".png");
            img = new Texture2D(backCam.width, backCam.height);
            img.LoadImage(file);
            gif.Add(Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0.5f, 0.5f)));
            ratio = (float)backCam.width / (float)backCam.height;
            arf.aspectRatio = ratio;

        }

        while (true)
        {
            images[track].sprite = gif[cont];
            if (cont == 22)
            {
                cont = 0;
            }
            else
            {
                cont++;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ReadRacerName(string path, int track)
    {
        StreamReader stream = new StreamReader("CarPics/" + path + "/" + path + ".txt");
        string text = stream.ReadToEnd();
        String[] texts = text.Split(',');
        nameText[track].text = texts[0];
        nameText[track].gameObject.SetActive(true);
        names[track].text = "";
        names[track].gameObject.SetActive(false);
        stream.Close();// MUITO IMPORTANTE
    }
    public void Remove_Click(int track)
    {
        RemoveRacer(track);

        if (racers.Count == 3)
        {
            racers.Remove(track);
            StartCoroutine(RacerQRRead());
        }
        else
        {
            racers.Remove(track);
        }

        if (racers.Count == 0)
        {
            racerButton.interactable = false;
            rankingPanel.SetActive(true);
        }
    }
    public void AddRacer(string _code, int track)
    {
       
        if (track == -1)
        {
            track++;
            while (racers.ContainsKey(track))
            {
                track++;
            }
        }

        trackGif.Add(track, StartCoroutine(ReadImage(_code, track)));
        ReadRacerName(_code, track);
        racers.Add(track, _code.ToUpper());
        racerButton.interactable = true;
        remove[track].interactable = true;
        arduino.Write("add" + track);
        rankingPanel.SetActive(false);
        //Debug.Log("Add player: " + result.Text + " in Track: " + track);
    }
    private void OnEnable()
    {
        racers = new Dictionary<int, string>();


        string webCamIndex = PlayerPrefs.GetString("webCamIndex");

        backCam = new WebCamTexture(webCamIndex);
        if (backCam == null)
        {
            Debug.Log("Unable to find back camera");
            return;
        }
        backCam.Play();
        Debug.Log("Camera OK");
        qrReader = StartCoroutine(RacerQRRead());

        rankingPanel.SetActive(true);

    }
    private void OnDisable()
    {
        for (int i = 0; i < 3; i++)
        {
            RemoveRacer(i);
        }
        racers.Clear();
        backCam.Stop();
        StopAllCoroutines();

    }


    public void RemoveRacer(int track)
    {
        Coroutine aux;
        if (trackGif.TryGetValue(track, out aux))
        {
            trackGif.Remove(track);
            StopCoroutine(aux);
        }
        //trackGif.Clear();
        names[track].text = "...";
        names[track].gameObject.SetActive(true);
        nameText[track].text = "";
        nameText[track].gameObject.SetActive(false);

        remove[track].interactable = false;
        //trackGif.Remove(track);
        images[track].sprite = placeholder;
        float ratio = (float)placeholder.rect.width / (float)placeholder.rect.height;
        images[track].GetComponent<AspectRatioFitter>().aspectRatio = ratio;
        arduino.Write("rmv" + track);
        Resources.UnloadUnusedAssets();
        GC.Collect();

    }

    void SaveTime(int track, int time)
    {
        try
        {
            racers.TryGetValue(track, out string path);
            Debug.Log("Saving " + time + " in " + path);
            string text = "," + time.ToString();
            StreamWriter file = new StreamWriter("CarPics/" + path + "/" + path + ".txt", true);
            file.WriteAsync("," + time.ToString());
            file.Flush();
            file.Dispose();
            file.Close();
            Debug.Log("Time Saved");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            SaveTime(track, time);
        }
    }

    IEnumerator ShowPositions()
    {
        racersTimes.Sort(SortByTime);

        for (int i = 0; i < racersTimes.Count; i++)
        {
            positionText[racersTimes[i].track].text = (i + 1).ToString();
            timesText[racersTimes[i].track].text = racersTimes[i].time.ToString();
        }
        positionPanel.SetActive(true);
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return new WaitForSeconds(0.1f);
        }

        positionPanel.SetActive(false);
        for (int i = 0; i < positionText.Length; i++)
        {
            positionText[i].text = "";
            
        }
        rankingPanel.SetActive(true);
        racersTimes.Clear();
        timesRecived = 0;
        racing = false;
        racerButton.interactable = false;
        racerButton.GetComponentInChildren<Text>().text = "RACE";
        for (int i = 0; i < 3; i++)
        {
            RemoveRacer(i);
        }
        racers.Clear();
        for (int i = 0; i < 3; i++)
        {
            positionText[i].text = "0";
            timesText[i].text = "0000";
        }
        
        StartCoroutine(RacerQRRead());
    }
    public int SortByTime(racerPosition r1, racerPosition r2)
    {
        return r1.time.CompareTo(r2.time);
    }

}
