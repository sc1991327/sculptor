using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Cubiquity;
using Newtonsoft.Json;

public class VoxelStoreObj
{
    public int Type { get; set; }
    public float Time { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int PosZ { get; set; }
    public float RotateEulerX { get; set; }
    public float RotateEulerY { get; set; }
    public float RotateEulerZ { get; set; }
    public int MaterialWeight0 { get; set; }
    public int MaterialWeight1 { get; set; }
    public int MaterialWeight2 { get; set; }
    public int MaterialWeight3 { get; set; }
    public int RangeX { get; set; }
    public int RangeY { get; set; }
    public int RangeZ { get; set; }
    public int Optshape { get; set; }
}

public class VoxelStoreSmooth
{
    public int Type { get; set; }
    public float Time { get; set; }
    public float UpcornerX { get; set; }
    public float UpcornerY { get; set; }
    public float UpcornerZ { get; set; }
    public float LowcornerX { get; set; }
    public float LowcornerY { get; set; }
    public float LowcornerZ { get; set; }
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

    private StreamWriter file;

    private TerrainVolume terrainVolume;

    private List<List<VoxelOpt>> optStack;
    private int optSize = 20;
    private int optPos = 0;

    // Use this for initialization
    void Awake () {

        string randomName = Path.GetRandomFileName();
        file = new System.IO.StreamWriter("Record/RecordOpt_" + randomName + ".txt");

        // Example
        //string lines = "First line.\r\nSecond line.\r\nThird line.";
        //file.WriteLine(lines);

        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();


        optStack = new List<List<VoxelOpt>>(optSize);
        for (int i = 0; i < optSize; i++){
            optStack.Add(new List<VoxelOpt>());
        }
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

    public void Write(Vector3i Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, float mtime)
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
            Optshape = (int)optshape
        };
        string jsonMsg = JsonConvert.SerializeObject(temp, Formatting.Indented);
        file.WriteLine(jsonMsg.ToString());
    }

    public void WriteSmooth(Region mregion, float mtime)
    {
        VoxelStoreSmooth temp = new VoxelStoreSmooth
        {
            Type = 2,
            Time = mtime,
            LowcornerX = mregion.lowerCorner.x,
            LowcornerY = mregion.lowerCorner.y,
            LowcornerZ = mregion.lowerCorner.z,
            UpcornerX = mregion.upperCorner.x,
            UpcornerY = mregion.upperCorner.y,
            UpcornerZ = mregion.upperCorner.z,
        };
        string jsonMsg = JsonConvert.SerializeObject(temp, Formatting.Indented);
        file.WriteLine(jsonMsg.ToString());
    }

    void OnApplicationQuit()
    {
        file.Close();
    }
}
