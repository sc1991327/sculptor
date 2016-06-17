using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;
using Cubiquity;

public class LoadConfig : MonoBehaviour {

    public int userNumber;
    public string userIP;
    public string serverIP;
    public int sendPort;
    public int recvPort;

    // Use this for initialization
    void Awake () {

        string configFileName = Paths.voxelDatabases + "/Config.txt";
        Load(configFileName);

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private bool Load(string fileName)
    {
        // Handle any problems that might arise when reading the text
        try
        {
            string line;

            StreamReader theReader = new StreamReader(fileName, Encoding.Default);
            using (theReader)
            {
                do
                {
                    line = theReader.ReadLine();
                    if (line != null)
                    {
                        // Do whatever you need to do with the text line, it's a string now
                        // In this example, I split it into arguments based on comma
                        // deliniators, then send that array to DoStuff()
                        string[] entries = line.Split('=');
                        if (entries.Length == 2)
                        {
                            switch (entries[0])
                            {
                                case "userNumber":
                                    userNumber = IntParseFast(entries[1]);
                                    break;
                                case "userIP":
                                    userIP = entries[1];
                                    break;
                                case "serverIP":
                                    serverIP = entries[1];
                                    break;
                                case "sendPort":
                                    sendPort = IntParseFast(entries[1]);
                                    break;
                                case "recvPort":
                                    recvPort = IntParseFast(entries[1]);
                                    break;
                            }
                        }
                            
                    }
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
                return true;
            }
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public int IntParseFast(string value)
    {
        int result = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char letter = value[i];
            result = 10 * result + (letter - 48);
        }
        return result;
    }

    public int IntParseFast(char value)
    {
        int result = 0;
        result = 10 * result + (value - 48);
        return result;
    }

}
