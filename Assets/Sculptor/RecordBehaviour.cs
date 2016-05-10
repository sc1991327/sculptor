using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Cubiquity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class VoxelStoreObj
{
    [JsonProperty(PropertyName = "Type")]
    public int Type { get; set; }
    [JsonProperty(PropertyName = "Time")]
    public float Time { get; set; }
    [JsonProperty(PropertyName = "PosX")]
    public float PosX { get; set; }
    [JsonProperty(PropertyName = "PosY")]
    public float PosY { get; set; }
    [JsonProperty(PropertyName = "PosZ")]
    public float PosZ { get; set; }
    [JsonProperty(PropertyName = "RotateEulerX")]
    public float RotateEulerX { get; set; }
    [JsonProperty(PropertyName = "RotateEulerY")]
    public float RotateEulerY { get; set; }
    [JsonProperty(PropertyName = "RotateEulerZ")]
    public float RotateEulerZ { get; set; }
    [JsonProperty(PropertyName = "MaterialWeight0")]
    public int MaterialWeight0 { get; set; }
    [JsonProperty(PropertyName = "MaterialWeight1")]
    public int MaterialWeight1 { get; set; }
    [JsonProperty(PropertyName = "MaterialWeight2")]
    public int MaterialWeight2 { get; set; }
    [JsonProperty(PropertyName = "MaterialWeight3")]
    public int MaterialWeight3 { get; set; }
    [JsonProperty(PropertyName = "RangeX")]
    public int RangeX { get; set; }
    [JsonProperty(PropertyName = "RangeY")]
    public int RangeY { get; set; }
    [JsonProperty(PropertyName = "RangeZ")]
    public int RangeZ { get; set; }
    public float LowcornerZ { get; set; }
    [JsonProperty(PropertyName = "Optshape")]
    public int Optshape { get; set; }
    [JsonProperty(PropertyName = "ActiveMirror")]
    public bool ActiveMirror { get; set; }
}

public class VoxelOpt
{
    public Vector3i Pos { get; set; }
    public MaterialSet MaterialNew { get; set; }
    public MaterialSet MaterialOld { get; set; }
    public VoxelOpt(Vector3i p, MaterialSet mnew, MaterialSet mold)
    {
        Pos = p;
        MaterialNew = mnew;
        MaterialOld = mold;
    }
}

public class RecordBehaviour : MonoBehaviour {

    public GameObject BasicProceduralVolume = null;

    public List<string> recordFileNames;
    public List<VoxelStoreObj> ReplayVoxelStore;
    public List<VoxelStoreObj> RecordVoxelStore;
    public List<string> loadFileNames;

    private StreamWriter file;

    private TerrainVolume terrainVolume;

    private List<List<VoxelOpt>> optStack;
    private int optSize = 20;
    private int optPos = 0;

    // Use this for initialization
    void Awake () {

        recordFileNames = new List<string>();
        loadFileNames = new List<string>();

        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();

        optStack = new List<List<VoxelOpt>>(optSize);
        for (int i = 0; i < optSize; i++){
            optStack.Add(new List<VoxelOpt>());
        }

        // obtain all files name
        string fonderpath = "Record";
        ProcessDirectory(fonderpath, recordFileNames, "*.txt", false);

        string loadfonderpath = Paths.voxelDatabases;
        ProcessDirectory(loadfonderpath, loadFileNames, "*.vdb", false);

        ReplayVoxelStore = new List<VoxelStoreObj>();
        RecordVoxelStore = new List<VoxelStoreObj>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void UnDo()
    {
        //Debug.Log("UnDo");
        SubOptPos();
        bool reopt = SetVoxelsOld();
        if (reopt == false)
        {
            AddOptPos();
            Debug.Log("Undo Failed, Over Steps.");
        }
    }

    public void ReDo()
    {
        //Debug.Log("ReDo");
        bool reopt = SetVoxelsNew();
        if (reopt == false)
        {
            Debug.Log("Redo Failed, Over Steps.");
        }
        else
        {
            AddOptPos();
        }
    }

    public void NewDo()
    {
        //Debug.Log("NewDo");
        AddOptPos();
        optStack[optPos].Clear();
    }

    public void PushOperator(VoxelOpt opt)
    {
        optStack[optPos].Add(opt);
    }

    public void ClearAll()
    {
        for (int i = 0; i < optSize; i++)
        {
            optStack[i].Clear();
        }
    }

    private void AddOptPos()
    {
        if (optPos < optSize - 1)
        {
            optPos++;
        }
        else
        {
            optPos = 0;
        }
    }

    private void SubOptPos()
    {
        if (optPos > 0)
        {
            optPos--;
        }
        else
        {
            optPos = optSize;
        }
    }

    private bool SetVoxelsNew()
    {
        if (optStack[optPos].Count == 0)
        {
            return false;
        }
        else
        {
            foreach (VoxelOpt tempOpt in optStack[optPos])
            {
                terrainVolume.data.SetVoxel(tempOpt.Pos.x, tempOpt.Pos.y, tempOpt.Pos.z, tempOpt.MaterialNew);
            }
        }
        return true;
    }

    private bool SetVoxelsOld()
    {
        if (optStack[optPos].Count == 0)
        {
            return false;
        }
        else
        {
            for (int tempi = optStack[optPos].Count - 1; tempi >= 0; tempi--)
            {
                VoxelOpt tempOpt = optStack[optPos][tempi];
                terrainVolume.data.SetVoxel(tempOpt.Pos.x, tempOpt.Pos.y, tempOpt.Pos.z, tempOpt.MaterialOld);
            }
        }
        return true;
    }

    private void ProcessDirectory(string targetDirectory, List<string> targetList, string constraint, bool handleChildDirectory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory, constraint);
        foreach (string fileName in fileEntries)
            ProcessFile(fileName, targetList);

        if (handleChildDirectory)
        {
            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, targetList, constraint, handleChildDirectory);
        }
    }

    // Insert logic for processing found files here.
    private void ProcessFile(string path, List<string> targetList)
    {
        targetList.Add(path);
    }

    public void ReadJsonFile(string filename)
    {
        string jsontext = System.IO.File.ReadAllText(filename);
        //string jsontext = @"[{
        //    'Type': 1,
        //    'Time': 10.4302082,
        //    'PosX': -8,
        //    'PosY': -13,
        //    'PosZ': 27,
        //    'RotateEulerX': 326.963043,
        //    'RotateEulerY': 337.606079,
        //    'RotateEulerZ': 2.99512219,
        //    'MaterialWeight0': 255,
        //    'MaterialWeight1': 255,
        //    'MaterialWeight2': 255,
        //    'MaterialWeight3': 0,
        //    'RangeX': 2,
        //    'RangeY': 2,
        //    'RangeZ': 2,
        //    'UpcornerX': 0,
        //    'UpcornerY': 0,
        //    'UpcornerZ': 0,
        //    'LowcornerX': 0,
        //    'LowcornerY': 0,
        //    'LowcornerZ': 0,
        //    'Optshape': 1
        //    'ActiveMirror' : 0
        //}]";

        ReplayVoxelStore = JsonConvert.DeserializeObject<List<VoxelStoreObj>>(jsontext);
        Debug.Log("JSON File Size: " + ReplayVoxelStore.Count);
    }

    public void WriteJsonFile(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, float mtime, bool activeMirror)
    {
        VoxelStoreObj temp = new VoxelStoreObj
        {
            Type = 1,
            Time = mtime,
            PosX = Pos.x,
            PosY = Pos.y,
            PosZ = Pos.z,
            RotateEulerX = RotateEuler.x,
            RotateEulerY = RotateEuler.y,
            RotateEulerZ = RotateEuler.z,
            MaterialWeight0 = materialSet.weights[0],
            MaterialWeight1 = materialSet.weights[1],
            MaterialWeight2 = materialSet.weights[2],
            MaterialWeight3 = materialSet.weights[3],
            RangeX = range.x,
            RangeY = range.y,
            RangeZ = range.z,
            Optshape = (int)optshape,
            ActiveMirror = activeMirror
        };
        RecordVoxelStore.Add(temp);
    }

    public void WriteJsonFileSmooth(Vector3 Pos, Vector3i range, float mtime, bool activeMirror)
    {
        VoxelStoreObj temp = new VoxelStoreObj
        {
            Type = 2,
            Time = mtime,
            PosX = Pos.x,
            PosY = Pos.y,
            PosZ = Pos.z,
            RangeX = range.x,
            RangeY = range.y,
            RangeZ = range.z,
            ActiveMirror = activeMirror
        };
        RecordVoxelStore.Add(temp);
    }

    public void WriteJsonFilePaint(Vector3 Pos, MaterialSet materialSet, Vector3i range, float amount, float mtime, bool activeMirror)
    {
        VoxelStoreObj temp = new VoxelStoreObj
        {
            Type = 3,
            Time = mtime,
            PosX = Pos.x,
            PosY = Pos.y,
            PosZ = Pos.z,
            MaterialWeight0 = materialSet.weights[0],
            MaterialWeight1 = materialSet.weights[1],
            MaterialWeight2 = materialSet.weights[2],
            MaterialWeight3 = materialSet.weights[3],
            RangeX = range.x,
            RangeY = range.y,
            RangeZ = range.z,
            ActiveMirror = activeMirror
        };
        RecordVoxelStore.Add(temp);
    }

    void OnApplicationQuit()
    {
        string randomName = Path.GetRandomFileName();
        file = new System.IO.StreamWriter("Record/RecordOpt_" + randomName + ".txt");
        file.WriteLine("[");
        foreach (VoxelStoreObj temp in RecordVoxelStore)
        {
            string jsonMsg = JsonConvert.SerializeObject(temp, Formatting.Indented);
            file.WriteLine(jsonMsg);
            file.WriteLine(",");
        }
        file.WriteLine("]");
        file.Close();
    }
}
