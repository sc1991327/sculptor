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

public class NetManagerUDP : MonoBehaviour
{
    public GameObject configObj;
    private LoadConfig loadConfig;

    public int myID;
    public string myIP;
    public string serverIP;
    public int sendPort;
    public int recvPort;

    private static Socket clientUDP;
    private Thread recvThread;

    public GameObject cameraManagerObject = null;
    public GameObject BasicProceduralVolume = null;
    public GameObject handObject = null;

    public GameObject headAnchorSend = null;
    public GameObject leftHandAnchorSend = null;
    public GameObject rightHandAnchorSend = null;

    public GameObject headAnchorRecv = null;
    public GameObject leftHandAnchorRecv = null;
    public GameObject rightHandAnchorRecv = null;

    private UdpClient udpClientSend;
    private UdpClient udpClientRecv;
    private IPEndPoint remoteEndPoint;
    private Thread receiveThread;
    private string receiveMessage = "";

    private float PreSendTime;

    private CameraManager cameraManager;
    private TerrainVolume terrainVolume;
    private ProceduralTerrainVolume proceduralTerrainVolume;
    private HandBehaviour handBehaviour;
    private int optRangeLeftOrg;
    private int optRangeRightOrg;
    private OptModePanel activeOptModePanel;

    private float volumeDistance = 0;

    private static object lockObj = new object();
    private bool netDataStreamUse;
    private List<NetData> netDataStream1;
    private List<NetData> netDataStream2;

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
    private bool NetVCContinue = false;
    private bool NetVCMirror = false;

    private bool doNetVS = false;
    private Vector3 NetVSPos = new Vector3(0, 0, 0);
    private Vector3i NetVSRng = new Vector3i(0, 0, 0);
    private bool NetVSContinue = false;
    private bool NetVSMirror = false;

    private bool doNetVP = false;
    private Vector3 NetVPPos = new Vector3(0, 0, 0);
    private Vector3i NetVPRng = new Vector3i(0, 0, 0);
    private MaterialSet NetVPMaterialSet = new MaterialSet();
    private bool NetVPContinue = false;
    private bool NetVPMirror = false;

    // Use this for initialization
    void Start()
    {
        loadConfig = configObj.GetComponent<LoadConfig>();
        myID = loadConfig.userNumber;
        myIP = loadConfig.userIP;
        serverIP = loadConfig.serverIP;
        sendPort = loadConfig.sendPort;
        recvPort = loadConfig.recvPort;

        cameraManager = cameraManagerObject.GetComponent<CameraManager>();
        handBehaviour = handObject.GetComponent<HandBehaviour>();
        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();
        proceduralTerrainVolume = BasicProceduralVolume.GetComponent<ProceduralTerrainVolume>();
        volumeDistance = proceduralTerrainVolume.GetVoxelRadiusDistance();

        netDataStreamUse = true;
        netDataStream1 = new List<NetData>();
        netDataStream2 = new List<NetData>();
        ConnectToUDPServer();

        optRangeLeftOrg = handBehaviour.GetOptRangeLeft();
        optRangeRightOrg = handBehaviour.GetOptRangeRight();

        headAnchorRecv.SetActive(false);
        leftHandAnchorRecv.SetActive(false);
        rightHandAnchorRecv.SetActive(false);

        PreSendTime = Time.time;
    }

    void ConnectToUDPServer()
    {
        myIP = GetLocalIPAddress();

        clientUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientUDP.Bind(new IPEndPoint(IPAddress.Parse(myIP), recvPort));
        recvThread = new Thread(ReceiveMessage);
        recvThread.Start();

        Debug.Log("Client Start...");
    }

    void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                EndPoint point = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = new byte[1024];
                int length = clientUDP.ReceiveFrom(buffer, ref point);
                receiveMessage = Encoding.UTF8.GetString(buffer, 0, length);

                //Debug.Log("Recv: " + point.ToString() + " " + receiveMessage);

                MessageHandling(receiveMessage);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
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

        // data handling
        if (activeOptModePanel == OptModePanel.network)
        {
            // send
            if (Time.time - PreSendTime > 0.05f)
            {
                SendPosMessage(NetMark.headpos, headAnchorSend.transform);
                SendPosMessage(NetMark.lefthandpos, leftHandAnchorSend.transform);
                SendPosMessage(NetMark.righthandpos, rightHandAnchorSend.transform);
                PreSendTime = Time.time;
            }

            // receive
            //lock (lockObj)
            {
                if (netDataStreamUse)
                {
                    foreach (NetData tempData in netDataStream1)
                    {
                        NetDataHandling(tempData);
                    }
                    netDataStream1.Clear();
                    netDataStreamUse = false;
                }
                else
                {
                    foreach (NetData tempData in netDataStream2)
                    {
                        NetDataHandling(tempData);
                    }
                    netDataStream2.Clear();
                    netDataStreamUse = true;
                }
            }

            headAnchorRecv.transform.position = HeadTransPos;
            headAnchorRecv.transform.eulerAngles = HeadTransRot;
            headAnchorRecv.transform.localScale = terrainVolume.transform.localScale * optRangeLeftOrg;

            leftHandAnchorRecv.transform.position = LeftHandTransPos;
            leftHandAnchorRecv.transform.eulerAngles = LeftHandTransRot;
            leftHandAnchorRecv.transform.localScale = terrainVolume.transform.localScale * optRangeLeftOrg;

            rightHandAnchorRecv.transform.position = RightHandTransPos;
            rightHandAnchorRecv.transform.eulerAngles = RightHandTransRot;
            rightHandAnchorRecv.transform.localScale = terrainVolume.transform.localScale * optRangeRightOrg;

            if (doNetVC)
            {
                handBehaviour.NetVoxelSetting(NetVCPos, NetVCRot, NetVCMaterialSet, NetVCRng, NetVCOpt, NetVCContinue, NetVCMirror);
                doNetVC = false;
            }

            if (doNetVS)
            {
                handBehaviour.NetVoxelSmoothing(NetVSPos, NetVSRng, NetVSContinue, NetVSMirror);
                doNetVS = false;
            }

            if (doNetVP)
            {
                handBehaviour.NetVoxelPainting(NetVPPos, NetVPMaterialSet, NetVPRng, NetVPContinue, NetVPMirror);
                doNetVP = false;
            }
        }
    }

    void OnApplicationQuit()
    {

        recvThread.Abort();
        if (clientUDP != null)
        {
            clientUDP.Close();
        }

    }

    void SendMessage(string message)
    {
        try
        {
            EndPoint point = new IPEndPoint(IPAddress.Parse(serverIP), sendPort);
            clientUDP.SendTo(Encoding.UTF8.GetBytes(message), point);

            //Debug.Log("Send: " + point.ToString() + " " + message);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private void MessageHandling(string message)
    {
        //Debug.Log(message);

        char[] delimiterChars = { '|' };
        string[] words = message.Split(delimiterChars);
        foreach (string singlemessage in words)
        {
            if (IsValidJson(singlemessage))
            {
                NetData jsonMsg = JsonConvert.DeserializeObject<NetData>(singlemessage);
                //lock (lockObj)
                {
                    if (netDataStreamUse)
                    {
                        netDataStream2.Add(jsonMsg);
                    }
                    else
                    {
                        netDataStream1.Add(jsonMsg);
                    }
                }
            }
        }
    }

    private void NetDataHandling(NetData netData)
    {
        VRMode vrmode = cameraManager.GetVRMode();

        int userID = netData.ClientID;
        if (userID != myID)
        {
            // update user's transform
            NetMark userPos = (NetMark)netData.ClientNetMark;
            switch (userPos)
            {
            case NetMark.headpos:
                if (vrmode == VRMode.OculusVR)
                {
                    HeadTransPos = new Vector3(netData.PosX, netData.PosY - volumeDistance, netData.PosZ);
                    HeadTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                }
                else
                {
                    HeadTransPos = new Vector3(netData.PosX, netData.PosY + volumeDistance, netData.PosZ);
                    HeadTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                }
                break;

            case NetMark.lefthandpos:
                if (vrmode == VRMode.OculusVR)
                {
                    LeftHandTransPos = new Vector3(netData.PosX, netData.PosY - volumeDistance, netData.PosZ);
                    LeftHandTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                }
                else
                {
                    LeftHandTransPos = new Vector3(netData.PosX, netData.PosY + volumeDistance, netData.PosZ);
                    LeftHandTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                }
                break;

            case NetMark.righthandpos:
                if (vrmode == VRMode.OculusVR)
                {
                    RightHandTransPos = new Vector3(netData.PosX, netData.PosY - volumeDistance, netData.PosZ);
                    RightHandTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                }
                else
                {
                    RightHandTransPos = new Vector3(netData.PosX, netData.PosY + volumeDistance, netData.PosZ);
                    RightHandTransRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                }
                break;

            case NetMark.sculptoropt:
                NetVCPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                NetVCRot = new Vector3(netData.RotX, netData.RotY, netData.RotZ);
                NetVCRng = new Vector3i((int)netData.SclX, (int)netData.SclY, (int)netData.SclZ);
                NetVCMaterialSet.weights[0] = (byte)netData.M0;
                NetVCMaterialSet.weights[1] = (byte)netData.M1;
                NetVCMaterialSet.weights[2] = (byte)netData.M2;
                NetVCMaterialSet.weights[3] = (byte)netData.M3;
                NetVCOpt = (OptShape)netData.Optshape;
                NetVCContinue = netData.CalcContinue;
                NetVCMirror = netData.ActiveMirror;
                doNetVC = true;
                break;

            case NetMark.smoothopt:
                NetVSPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                NetVSRng = new Vector3i((int)netData.SclX, (int)netData.SclY, (int)netData.SclZ);
                NetVSContinue = netData.CalcContinue;
                NetVSMirror = netData.ActiveMirror;
                doNetVS = true;
                break;

            case NetMark.paintopt:
                NetVPPos = new Vector3(netData.PosX, netData.PosY, netData.PosZ);
                NetVPRng = new Vector3i((int)netData.SclX, (int)netData.SclY, (int)netData.SclZ);
                NetVPMaterialSet.weights[0] = (byte)netData.M0;
                NetVPMaterialSet.weights[1] = (byte)netData.M1;
                NetVPMaterialSet.weights[2] = (byte)netData.M2;
                NetVPMaterialSet.weights[3] = (byte)netData.M3;
                NetVPContinue = netData.CalcContinue;
                NetVPMirror = netData.ActiveMirror;
                doNetVP = true;
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

    public void SendOptMessage(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, bool calcContinue, bool activeMirror)
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
            CalcContinue = calcContinue,
            ActiveMirror = activeMirror,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public void SendOptMessage(Vector3 Pos, Vector3i range, bool calcContinue, bool activeMirror)
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
            CalcContinue = calcContinue,
            ActiveMirror = activeMirror,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public void SendOptMessage(Vector3 Pos, Vector3i range, MaterialSet materialSet, bool calcContinue, bool activeMirror)
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
            CalcContinue = calcContinue,
            ActiveMirror = activeMirror,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("Local IP Address Not Found!");
    }
}
