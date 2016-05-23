using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
using Cubiquity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public enum NetMark { headpos, lefthandpos, righthandpos, sculptoropt, smoothopt, paintopt};

public class NetData
{
    [JsonProperty(PropertyName = "ClientID")]
    public int ClientID { get; set; }
    [JsonProperty(PropertyName = "ClientNetMark")]
    public int ClientNetMark { get; set; }
    [JsonProperty(PropertyName = "PosX")]
    public float PosX { get; set; }
    [JsonProperty(PropertyName = "PosY")]
    public float PosY { get; set; }
    [JsonProperty(PropertyName = "PosZ")]
    public float PosZ { get; set; }
    [JsonProperty(PropertyName = "RotX")]
    public float RotX { get; set; }
    [JsonProperty(PropertyName = "RotY")]
    public float RotY { get; set; }
    [JsonProperty(PropertyName = "RotZ")]
    public float RotZ { get; set; }
    [JsonProperty(PropertyName = "SclX")]
    public float SclX { get; set; }
    [JsonProperty(PropertyName = "SclY")]
    public float SclY { get; set; }
    [JsonProperty(PropertyName = "SclZ")]
    public float SclZ { get; set; }
    [JsonProperty(PropertyName = "M0")]
    public int M0 { get; set; }
    [JsonProperty(PropertyName = "M1")]
    public int M1 { get; set; }
    [JsonProperty(PropertyName = "M2")]
    public int M2 { get; set; }
    [JsonProperty(PropertyName = "M3")]
    public int M3 { get; set; }
    [JsonProperty(PropertyName = "Optshape")]
    public int Optshape { get; set; }
    [JsonProperty(PropertyName = "ActiveMirror")]
    public bool ActiveMirror { get; set; }
}

public class NetManager : MonoBehaviour {

    public string myIP = "10.32.93.177";
    public int myProt = 8885;
    public int myID = 0;

    public GameObject BasicProceduralVolume = null;
    public GameObject handObject = null;

    public GameObject headAnchorSend = null;
    public GameObject leftHandAnchorSend = null;
    public GameObject rightHandAnchorSend = null;

    public GameObject headAnchorRecv = null;
    public GameObject leftHandAnchorRecv = null;
    public GameObject rightHandAnchorRecv = null;

    private Socket clientSocket;
    private Thread thread;
    private byte[] receiveData = new byte[1024];
    private string receiveMessage = "";

    private float PreSendTime = Time.time;

    private TerrainVolume terrainVolume;
    private HandBehaviour handBehaviour;
    private int optRangeOrg;
    private OptModePanel activeOptModePanel;

    private static object lockObj = new object();
    private Vector3 HeadTransPos = new Vector3(0, 0, 0);
    private Vector3 HeadTransRot = new Vector3(0, 0, 0);
    private Vector3 LeftHandTransPos = new Vector3(0, 0, 0);
    private Vector3 LeftHandTransRot = new Vector3(0, 0, 0);
    private Vector3 RightHandTransPos = new Vector3(0, 0, 0);
    private Vector3 RightHandTransRot = new Vector3(0, 0, 0);

    private bool doNetVC = false;
    private Vector3 NetVCPos = new Vector3(0, 0, 0);
    private Vector3 NetVCRot = new Vector3(0, 0, 0);
    private Vector3i NetVCRng = new Vector3i(0, 0, 0);
    private MaterialSet NetVCMaterialSet = new MaterialSet();
    private OptShape NetVCOpt = OptShape.sphere;
    private bool NetVCMirror = false;

    private bool doNetVS = false;
    private Vector3 NetVSPos = new Vector3(0, 0, 0);
    private Vector3i NetVSRng = new Vector3i(0, 0, 0);
    private bool NetVSMirror = false;

    private bool doNetVP = false;
    private Vector3 NetVPPos = new Vector3(0, 0, 0);
    private Vector3i NetVPRng = new Vector3i(0, 0, 0);
    private MaterialSet NetVPMaterialSet = new MaterialSet();
    private bool NetVPMirror = false;

    // Use this for initialization
    void Start() {

        handBehaviour = handObject.GetComponent<HandBehaviour>();
        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();

        ConnectToServer();

        optRangeOrg = handBehaviour.GetOptRange();

        headAnchorRecv.SetActive(false);
        leftHandAnchorRecv.SetActive(false);
        rightHandAnchorRecv.SetActive(false);

    }

    void ConnectToServer()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(new IPEndPoint(IPAddress.Parse(myIP), myProt));
        thread = new Thread(ReceiveMessage);
        thread.Start();
    }

    void OnDestroy()
    {
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

    void ReceiveMessage()
    {
        while (true)
        {
            if (clientSocket.Connected == false)
            {
                break;
            }
            int length = clientSocket.Receive(receiveData);
            receiveMessage = Encoding.UTF8.GetString(receiveData, 0, length);
            MessageHandling(receiveMessage);
        }
    }

    void Update()
    {
        // check network edit mode.
        OptModePanel tempOMP = handBehaviour.GetActiveOptModePanel();
        if (tempOMP != activeOptModePanel)
        {

            if (tempOMP == OptModePanel.network)
            {
                headAnchorRecv.SetActive(true);
                leftHandAnchorRecv.SetActive(true);
                rightHandAnchorRecv.SetActive(true);
            }
            else
            {
                headAnchorRecv.SetActive(false);
                leftHandAnchorRecv.SetActive(false);
                rightHandAnchorRecv.SetActive(false);
            }

            activeOptModePanel = tempOMP;
        }

        if (activeOptModePanel == OptModePanel.network)
        {
            if (Time.time - PreSendTime > 0.05f)
            {
                SendPosMessage(NetMark.headpos, headAnchorSend.transform);
                SendPosMessage(NetMark.lefthandpos, leftHandAnchorSend.transform);
                SendPosMessage(NetMark.righthandpos, rightHandAnchorSend.transform);
                PreSendTime = Time.time;
            }

            lock (lockObj)
            {

                headAnchorRecv.transform.position = HeadTransPos;
                headAnchorRecv.transform.eulerAngles = HeadTransRot;
                headAnchorRecv.transform.localScale = terrainVolume.transform.localScale * optRangeOrg;

                leftHandAnchorRecv.transform.position = LeftHandTransPos;
                leftHandAnchorRecv.transform.eulerAngles = LeftHandTransRot;
                leftHandAnchorRecv.transform.localScale = terrainVolume.transform.localScale * optRangeOrg;

                rightHandAnchorRecv.transform.position = RightHandTransPos;
                rightHandAnchorRecv.transform.eulerAngles = RightHandTransRot;
                rightHandAnchorRecv.transform.localScale = terrainVolume.transform.localScale * optRangeOrg;
            }

            if (doNetVC)
            {
                handBehaviour.NetVoxelSetting(NetVCPos, NetVCRot, NetVCMaterialSet, NetVCRng, NetVCOpt, NetVCMirror);
                lock (lockObj) { doNetVC = false; }
            }

            if (doNetVS)
            {
                handBehaviour.NetVoxelSmoothing(NetVSPos, NetVSRng, NetVSMirror);
                lock (lockObj) { doNetVS = false; }
            }

            if (doNetVP)
            {
                handBehaviour.NetVoxelPainting(NetVPPos, NetVPMaterialSet, NetVPRng, NetVPMirror);
                lock (lockObj) { doNetVP = false; }
            }
        }
    }

    void SendMessage(string message)
    {
        message = "|" + message + "|";
        byte[] senddata = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(senddata);
    }

    private void MessageHandling(string message)
    {
        char[] delimiterChars = { '|' };
        string[] words = message.Split(delimiterChars);
        foreach (string singlemessage in words)
        {
            if (IsValidJson(singlemessage))
            {
                NetData jsonMsg = JsonConvert.DeserializeObject<NetData>(singlemessage);
                NetDataHandling(jsonMsg);
                //Debug.Log("ID: " + jsonMsg.ClientID);
                //Debug.Log("Mark: " + ((NetMark)jsonMsg.ClientNetMark));
            }
        }
    }

    private void NetDataHandling(NetData netData)
    {
        int userID = netData.ClientID;
        if (userID != myID)
        {
            // update user's transform
            NetMark userPos = (NetMark)netData.ClientNetMark;
            switch (userPos)
            {
                case NetMark.headpos:
                    lock (lockObj)
                    {
                        HeadTransPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                        HeadTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                    }
                    break;

                case NetMark.lefthandpos:
                    lock (lockObj)
                    {
                        LeftHandTransPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                        LeftHandTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                    }
                    break;

                case NetMark.righthandpos:
                    lock (lockObj)
                    {
                        RightHandTransPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                        RightHandTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                    }
                    break;

                case NetMark.sculptoropt:
                    lock (lockObj)
                    {
                        NetVCPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                        NetVCRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                        NetVCRng = new Vector3i((int)netData.SclX, (int)netData.SclY, (int)netData.SclZ);
                        NetVCMaterialSet.weights[0] = (byte)netData.M0;
                        NetVCMaterialSet.weights[1] = (byte)netData.M1;
                        NetVCMaterialSet.weights[2] = (byte)netData.M2;
                        NetVCMaterialSet.weights[3] = (byte)netData.M3;
                        NetVCOpt = (OptShape)netData.Optshape;
                        NetVCMirror = netData.ActiveMirror;
                        doNetVC = true;
                    }
                    break;

                case NetMark.smoothopt:
                    lock (lockObj)
                    {
                        NetVCPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                        NetVCRng = new Vector3i((int)netData.SclX, (int)netData.SclY, (int)netData.SclZ);
                        NetVCMirror = netData.ActiveMirror;
                        doNetVS = true;
                    }
                    break;

                case NetMark.paintopt:
                    lock (lockObj)
                    {
                        NetVPPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                        NetVPRng = new Vector3i((int)netData.SclX, (int)netData.SclY, (int)netData.SclZ);
                        NetVPMaterialSet.weights[0] = (byte)netData.M0;
                        NetVPMaterialSet.weights[1] = (byte)netData.M1;
                        NetVPMaterialSet.weights[2] = (byte)netData.M2;
                        NetVPMaterialSet.weights[3] = (byte)netData.M3;
                        NetVPMirror = netData.ActiveMirror;
                        doNetVP = true;
                    }
                    break;
            }
        }

    }

    private static bool IsValidJson(string strInput)
    {
        strInput = strInput.Trim();
        if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
            (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                Debug.Log(jex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void SendPosMessage(NetMark posmark, Transform trans)
    {
        NetData jsonMsg = new NetData
        {
            ClientID = myID,
            ClientNetMark = (int)posmark,
            PosX = trans.position.x,
            PosY = trans.position.y,
            PosZ = trans.position.z,
            RotX = trans.rotation.eulerAngles.x,
            RotY = trans.rotation.eulerAngles.y,
            RotZ = trans.rotation.eulerAngles.z,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public void SendOptMessage(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror)
    {
        NetMark optmark = NetMark.sculptoropt;
        NetData jsonMsg = new NetData
        {
            ClientID = myID,
            ClientNetMark = (int)optmark,
            PosX = Pos.x,
            PosY = Pos.y,
            PosZ = Pos.z,
            RotX = RotateEuler.x,
            RotY = RotateEuler.y,
            RotZ = RotateEuler.z,
            SclX = range.x,
            SclY = range.y,
            SclZ = range.z,
            M0 = materialSet.weights[0],
            M1 = materialSet.weights[1],
            M2 = materialSet.weights[2],
            M3 = materialSet.weights[3],
            Optshape = (int)optshape,
            ActiveMirror = activeMirror,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public void SendOptMessage(Vector3 Pos, Vector3i range, bool activeMirror)
    {
        NetMark optmark = NetMark.smoothopt;
        NetData jsonMsg = new NetData
        {
            ClientID = myID,
            ClientNetMark = (int)optmark,
            PosX = Pos.x,
            PosY = Pos.y,
            PosZ = Pos.z,
            SclX = range.x,
            SclY = range.y,
            SclZ = range.z,
            ActiveMirror = activeMirror,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public void SendOptMessage(Vector3 Pos, Vector3i range, MaterialSet materialSet, bool activeMirror)
    {
        NetMark optmark = NetMark.paintopt;
        NetData jsonMsg = new NetData
        {
            ClientID = myID,
            ClientNetMark = (int)optmark,
            PosX = Pos.x,
            PosY = Pos.y,
            PosZ = Pos.z,
            SclX = range.x,
            SclY = range.y,
            SclZ = range.z,
            M0 = materialSet.weights[0],
            M1 = materialSet.weights[1],
            M2 = materialSet.weights[2],
            M3 = materialSet.weights[3],
            ActiveMirror = activeMirror,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

}
