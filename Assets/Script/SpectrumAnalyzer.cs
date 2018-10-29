using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.IO;

/// <summary>
/// Attach this script below a GameObject with an AudioSource and manually assign a clip and enable Play on Awake.
/// Since this script does not care what song is playing you can implement an Audio manager to change songs as you wish.
/// Make sure to manually assign two prefabs of your choice.
/// </summary>
public class SpectrumAnalyzer : MonoBehaviour
{
    private AudioSource source;

    public AnalyzerSettings settings; //All of our settings
    public GameObject enemyprefab1;
    private float timer = 0;
    public float timerInterval = 1;
    private float moodchangetime=0;
    private float createnum = 1;
    private float avglevel = 0;
    //private
    private float[] spectrum; //Audio Source data
    private List<GameObject> pillars; //ref pillars to scale/move with music
    private GameObject folder;
    private bool isBuilding; //Prevents multi-calls and update while building.
    public float rotationspeed;
    public Canvas successcanvas;
    void Start()
    {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "wav文件(*.wav)\0*.wav";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "Title";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        if (LocalDialog.GetSaveFileName(openFileName))
        {
            Debug.Log(openFileName.file);
            StartCoroutine(LoadMusic(openFileName.file));
            Debug.Log(openFileName.fileTitle);
        }
        CreatePillarsByShapes();

    }
   
    private void CreatePillarsByShapes()
    {
        //get current pillar types
        GameObject currentPrefabType = settings.pillar.type == PillarTypes.Cylinder ? settings.Prefabs.CylPrefab : settings.Prefabs.BoxPrefab;
       
        
        pillars = MathB.ShapesOfGameObjects(currentPrefabType, settings.pillar.radius, (int) settings.pillar.amount,settings.pillar.shape);

        //Organize pillars nicely in this folder under this transform
        folder = new GameObject("Pillars-" + pillars.Count);
        folder.transform.SetParent(transform);

        foreach (var piller in pillars)
        {
            piller.transform.SetParent(folder.transform);
        }


        for (int i = 0; i < pillars.Count; i++) //needs to be <= sample rate or error
        {
            float level = spectrum[i] * settings.pillar.sensitivity * Time.deltaTime * 1000; //0,1 = l,r for two channels
            avglevel += level;


        }
        avglevel = avglevel / spectrum.Length;
    }


    void Update()
    {
        

        float audiomood = 0;
        spectrum = AudioListener.GetSpectrumData((int) settings.spectrum.sampleRate, 0, settings.spectrum.FffWindowType);

        for (int i = 0; i < pillars.Count; i++) //needs to be <= sample rate or error
        {
            float level = spectrum[i]*settings.pillar.sensitivity*Time.deltaTime*1000; //0,1 = l,r for two channels

            //Scale pillars 
            Vector3 previousScale = pillars[i].transform.localScale;
            previousScale.z = Mathf.Lerp(previousScale.z, level, settings.pillar.speed*Time.deltaTime);
                //Add delta time please
            pillars[i].transform.localScale = previousScale;

            //Move pillars up by scale/2
            Vector3 pos = pillars[i].transform.position;
            pos.z = previousScale.z*.5f;
            pillars[i].transform.position = pos;
            audiomood += pillars[i].transform.localScale.z;


        }
        moodchangetime += Time.deltaTime;
        if (moodchangetime >= 1)
        {
            moodchangetime = 0;
            Debug.Log(audiomood);
            NumChange(audiomood);
            CreateEnemy();

        }

         if(!source.isPlaying && isBuilding)
        {
            successcanvas.enabled = true;
        }
        transform.RotateAround(Vector3.back, rotationspeed * Time.deltaTime);

    }

    void NumChange(float audiomood)
    {
        if(audiomood<1)
        {
            createnum = 0;
        }
        else if(audiomood <=50)
        {
            createnum = 2;
        }
        else if (audiomood > 50 && audiomood < 100)
        {
            createnum = 4;
        }
        else if (audiomood >= 100 && audiomood < 150)
        {
            createnum = 6;
        }
        else if (audiomood >= 150 && audiomood < 200)
        {
            createnum = 12;
        }
        else if (audiomood >= 200 && audiomood < 250)
        {
            createnum = 20;
        }
        else if (audiomood >= 250 && audiomood < 300)
        {
            createnum = 30;
        }
        else if (audiomood > 300)
        {
            createnum = 40;
        }
    }

    private IEnumerator LoadMusic(string filepath)
    {
        source = GetComponent<AudioSource>();
        using (var www = new WWW(filepath))
        {
            yield return www;
            source.clip = www.GetAudioClip();
            if (!source.isPlaying && source.clip.isReadyToPlay)
                source.Play();
            isBuilding = true;

        }
    }



    void CreateEnemy()
    {
       // timer += Time.deltaTime;
       // if (timer >= timerInterval)
       // {
          //  timer = 0;
            //6.4,4.8
            float x = Random.Range(-6.4f, 6.4f);
            float y = Random.Range(-4.8f, 4.8f);
            Vector3 ps = new Vector3(x, y, 0);
            //
            int i;
            for (int j = 0; j < createnum; j++)
            {
                i = Random.Range(0, pillars.Count);
                ps = pillars[i].transform.position;
                Instantiate<GameObject>(enemyprefab1, ps, enemyprefab1.transform.rotation);
            }
       // }
    }
    /// <summary>
    /// Called by UI slider onValue changed
    /// </summary>
    public void Rebuild()
    {
        if (isBuilding) return;

        //Destroy the pillars we had, clear the pillar list and start over
        isBuilding = true;
        pillars.Clear();
        DestroyImmediate(folder);
        CreatePillarsByShapes();
    }

    /// <summary>
    /// Resets to all settings to default in inspector drop down.
    /// </summary>
    private void Reset()
    {
        settings.pillar.Reset();
        settings.spectrum.Reset();
    }

    #region Dynamic floats and for UI sliders

    /// <summary>
    /// Convert Shapes enum to an int from a float so we can control by UI Slider
    /// </summary>
    public float PillarShape
    {
        get { return (int) settings.pillar.shape; }
        set
        {
            //set with UI Slider
            int num = (int) Mathf.Clamp(value, 0, 3);
            settings.pillar.shape = (Shapes) num;
        }
    }

    public float PillarType
    {
        get { return (int) settings.pillar.type; }
        set
        {
            //set with UI Slider
            int num = (int)Mathf.Clamp(value, 0, 2); 
            settings.pillar.type = (PillarTypes) num;
        }
    }

    public float Amount
    {
        get { return settings.pillar.amount; }
        set
        {
            settings.pillar.amount = Mathf.Clamp(value, 4, 128);
            
        }
    }

    public float Radius
    {
        get { return settings.pillar.radius; }
        set { settings.pillar.radius = Mathf.Clamp(value, 2, 256); }
    }


    public float Sensitivity
    {
        get { return settings.pillar.sensitivity; }
        set { settings.pillar.sensitivity = Mathf.Clamp(value, 1, 50); }
    }

    public float PillarSpeed
    {
        get { return settings.pillar.speed; }
        set { settings.pillar.speed = Mathf.Clamp(value, 1, 30); }
    }


    public float SampleMethod
    {
        get { return (int) settings.spectrum.FffWindowType; }
        set
        {
            //set with UI Slider
            int num = (int)Mathf.Clamp(value, 0, 6); 
            settings.spectrum.FffWindowType = (FFTWindow) num;
        }
    }

    public float SampleRate
    {
        get { return (int) settings.spectrum.sampleRate; }
        set
        {
            //set with UI Slider
            int num = (int) Mathf.Pow(2, 7 + value);//128,256,512,1024,2056
            settings.spectrum.sampleRate = (SampleRates) num;
        }
    }

    #endregion
}