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

public enum NetMark { headpos, lefthandpos, righthandpos, sculptoropt, smoothopt};

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

    public GameObject headAnchor = null;
    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    private Socket clientSocket;
    private Thread thread;
    private byte[] receiveData = new byte[1024];
    private string receiveMessage = "";

    public List<Transform> headTrans;
    public List<Transform> leftHandTrans;
    public List<Transform> rightHandTrans;

    // Use this for initialization
    void Start() {

        ConnectToServer();

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
        SendPosMessage(NetMark.headpos, headAnchor.transform);
        SendPosMessage(NetMark.lefthandpos, leftHandAnchor.transform);
        //SendPosMessage(NetMark.righthandpos, rightHandAnchor.transform);
    }

    void SendMessage(string message)
    {
        byte[] senddata = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(senddata);
    }

    private void MessageHandling(string message)
    {
        if (IsValidJson(message))
        {
            NetData jsonMsg = JsonConvert.DeserializeObject<NetData>(message);
            Debug.Log("ID: " + jsonMsg.ClientID);
            Debug.Log("Mark: " + jsonMsg.ClientNetMark);
        }
        else
        {
            Debug.Log("Json Failed.");
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
                //Exception in parsing json
                Debug.Log(jex.Message);
                return false;
            }
            catch (Exception ex) //some other exception
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
            RotX = trans.rotation.x,
            RotY = trans.rotation.y,
            RotZ = trans.rotation.z,
            SclX = trans.localScale.x,
            SclY = trans.localScale.y,
            SclZ = trans.localScale.z,
        };
        string tempMsg = JsonConvert.SerializeObject(jsonMsg, Formatting.Indented);
        SendMessage(tempMsg);
    }

    public void SendOptMessage(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror)
    {
        NetMark optmark = NetMark.sculptoropt;
        string temp = "[" + myID + "," + optmark + "," + Pos.x + "," + Pos.y + "," + Pos.z + "," + RotateEuler.x + "," + RotateEuler.y + "," + RotateEuler.z + ","
            + materialSet.weights[0] + "," + materialSet.weights[1] + "," + materialSet.weights[2] + "," + materialSet.weights[3] + ","
            + range.x + "," + range.y + "," + range.z + "," + optshape + "," + activeMirror + "]";
        SendMessage(temp);
    }

    public void SendOptMessage(Vector3 Pos, Vector3i range, bool activeMirror)
    {
        NetMark optmark = NetMark.smoothopt;
        string temp = "[" + myID + "," + optmark + "," + Pos.x + "," + Pos.y + "," + Pos.z + ","
            + range.x + "," + range.y + "," + range.z + "," + activeMirror + "]";
        SendMessage(temp);
    }

}
