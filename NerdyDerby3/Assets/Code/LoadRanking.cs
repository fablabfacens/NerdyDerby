﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class Racer
{
    public string name;
    public int bestTime;
}
public class LoadRanking : MonoBehaviour
{
    string path = "Assets/Resources/test.txt";
    public GameObject racerPrefab;
    public Transform rankingCointaner;

    public List<Racer> racers;
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(AddRacersInRanking());
    }
    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        StartCoroutine(AddRacersInRanking());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public static int SortByTime(Racer r1, Racer r2)
    {
        return r1.bestTime.CompareTo(r2.bestTime);
    }
    IEnumerator AddRacersInRanking()
    {

        //yield return new WaitForSeconds(1f);
        foreach (Transform child in rankingCointaner)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("Iniciando Leitura");
        DirectoryInfo info = new DirectoryInfo("CarPics");

        if (Directory.Exists("CarPics"))
        {
            Debug.Log("Diretório Encontrado");
        }
        else
        {
            Debug.Log("Diretório não Encontrado");
        }



        DirectoryInfo[] directorys = info.GetDirectories();

        Debug.Log("arquivos encontrados " + directorys.Length);
        racers = new List<Racer>();
        foreach (DirectoryInfo d in directorys)
        {
            Debug.Log(d.Name);
            StreamReader stream = new StreamReader("Carpics/" + d.Name + "/" + d.Name + ".txt");
            string text = stream.ReadToEnd();
            string[] texts = text.Split(',');
            stream.Close();// MUITO IMPORTANTE
            Racer racer = new Racer();

            racer.name = texts[0];
            List<int> times = new List<int>();
            for (int i = 1; i < texts.Length; i++)
            {
                times.Add(int.Parse(texts[i]));
            }

            if (times.Count >= 1)
            {
                times.Sort();
                racer.bestTime = times[0];
            }
            else
            {
                racer.bestTime = 9999;
            }

            racers.Add(racer);

        }
        racers.Sort(SortByTime);
        int cont = 1;
        foreach (Racer r in racers)
        {   
            GameObject actualRacer = Instantiate(racerPrefab, rankingCointaner);
            actualRacer.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = r.name;
            actualRacer.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = r.bestTime.ToString();
            actualRacer.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = cont.ToString();
            cont++;
        }

        //
        //
        //List<int> times = new List<int>();

        //Debug.Log(Read);
        yield return new WaitForSeconds(5);


    }
}
