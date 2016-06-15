using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ObjExporter : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    string lastPath = EditorPrefs.GetString("a4_OBJExport_lastPath", "");
        //    string lastFileName = EditorPrefs.GetString("a4_OBJExport_lastFile", "unityexport.obj");
        //    string expFile = EditorUtility.SaveFilePanel("Export OBJ", lastPath, lastFileName, "obj");
        //    if (expFile.Length > 0)
        //    {
        //        var fi = new System.IO.FileInfo(expFile);
        //        EditorPrefs.SetString("a4_OBJExport_lastFile", fi.Name);
        //        EditorPrefs.SetString("a4_OBJExport_lastPath", fi.Directory.FullName);
        //        Export(expFile, gameObject);
        //    }
        //}

    }

    void Export(string exportPath, GameObject exportGameObject)
    {
        //init stuff
        var exportFileInfo = new System.IO.FileInfo(exportPath);
        string baseFileName = System.IO.Path.GetFileNameWithoutExtension(exportPath);

        //work on export
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Export of " + Application.loadedLevelName);

        int lastIndex = 0;

        string meshName = exportGameObject.name;
        MeshFilter mf = exportGameObject.GetComponent<MeshFilter>();
        MeshRenderer mr = exportGameObject.GetComponent<MeshRenderer>();

        //export the meshhh :3
        Mesh msh = mf.sharedMesh;
        int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);

        //export vector data (FUN :D)!
        foreach (Vector3 vx in msh.vertices)
        {
            Vector3 v = vx;
            v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
            v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            v += mf.gameObject.transform.position;
            v.x *= -1;
            sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
        }
        foreach (Vector3 vx in msh.normals)
        {
            Vector3 v = vx;
            v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);
            v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            v.x *= -1;
            sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

        }
        foreach (Vector2 v in msh.uv)
        {
            sb.AppendLine("vt " + v.x + " " + v.y);
        }

        for (int j = 0; j < msh.subMeshCount; j++)
        {
            if (mr != null && j < mr.sharedMaterials.Length)
            {
                string matName = mr.sharedMaterials[j].name;
                sb.AppendLine("usemtl " + matName);
            }
            else
            {
                sb.AppendLine("usemtl " + meshName + "_sm" + j);
            }

            int[] tris = msh.GetTriangles(j);
            for (int t = 0; t < tris.Length; t += 3)
            {
                int idx2 = tris[t] + 1 + lastIndex;
                int idx1 = tris[t + 1] + 1 + lastIndex;
                int idx0 = tris[t + 2] + 1 + lastIndex;
                if (faceOrder < 0)
                {
                    sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                }
                else
                {
                    sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                }
            }
        }
        lastIndex += msh.vertices.Length;

        //write to disk
        System.IO.File.WriteAllText(exportPath, sb.ToString());
    }

    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    private string ConstructOBJString(int index)
    {
        string idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }
}
