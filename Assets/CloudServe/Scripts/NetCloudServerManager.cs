using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetCloudServerManager : MonoBehaviour
{
    public static NetCloudServerManager instance;
    public static CloudService cs;

    public int width = 32;
    public int HandleMsgNum = 15;
    public int BUFFER_SIZE = 16384;
    public InputField hostInput;
    public InputField portInput;
    public Text debug;
    
    public List<PointMessage> PM = new List<PointMessage>();

    public Camera cam;
    private void Awake() {
        cam = Camera.main;
        instance = this;
        cs = new CloudService();
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (cs.status)
            cs.msgHandle.Running();

        if ( Input.GetKeyDown("s")) {
            ServiceStart();
        }
        
    }
    public void ServiceStart() {
        if (cs.status)
            return;
        string host = "";
        int port = 1234;
        if (hostInput.text.Length == 0) {
            host = "192.168.50.142";
        }
        else {
            host = hostInput.text;
            port = int.Parse(portInput.text);
        }
        cs.ServiceStart(host, port);
    }

}
