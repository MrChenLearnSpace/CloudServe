using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgHandle 
{
    public int num = NetCloudServerManager.instance.HandleMsgNum;
    Dictionary<string, ProtocolBase> oneMessager = new Dictionary<string, ProtocolBase>();
    public List<ProtocolBase> msgList = new List<ProtocolBase>();

    ProtocolByte assistProtocol;


    public void Running() {
        while(msgList.Count>0&&num>0) {
            num--;
            //处理长度和将协议分开
            assistProtocol = msgList[0] as ProtocolByte;
            if(assistProtocol==null) {
                Debug.Log("消息转化失败");
                lock (msgList) {
                    msgList.RemoveAt(0);
                }
                return;
            }
            int start = 0;
            string IpAddress = assistProtocol.GetString(start, ref start);
            string protocolName=assistProtocol.GetString(start, ref start);
            //int protocolNameLen=System.Text.Encoding.UTF8.GetBytes(protocolName).Length + sizeof(Int32);
            switch (protocolName) {
                case "ProtocolCubeTexture":
                    ProtocolCubeTexture protocolCubeTexture = new ProtocolCubeTexture();
                    ProtocolCubeTexture aprotocol = protocolCubeTexture.Decode(assistProtocol.bytes, start, assistProtocol.bytes.Length) as ProtocolCubeTexture;
                    if (aprotocol == null) break;
                    PointMessage point = new PointMessage();
                    point.ResolveToPointMessage(aprotocol.Coord, aprotocol.CubemapFace, aprotocol.srcTexture);
                    NetCloudServerManager.instance.PM.Add(point);
                    break;

                case "Start":
                    StartProcessData(assistProtocol, start, IpAddress);

                    break;
            }
            lock(msgList) {
                msgList.RemoveAt(0);
            }
            //ProtocolCubeTexture protocol = msgList[0] as ProtocolCubeTexture;

        }
        num = NetCloudServerManager.instance.HandleMsgNum;

    }

    private void StartProcessData(ProtocolByte protocol,int start,string SendIPAddress) {
        ProtocolStr protocolStr = new ProtocolStr();
        Debug.Log("start" + start + " lenth" + assistProtocol.bytes.Length);
        ProtocolStr protocolstr = protocolStr.Decode(assistProtocol.bytes, start, assistProtocol.bytes.Length-start) as ProtocolStr;
        NetCloudServerManager.instance.debug.text += ("\n" + protocolstr.str);
        string[] CoordStrs = protocolstr.str.Split(',');
        float[] Coord = new float[3];
        for (int i = 0; i < 3; i++)
            Coord[i] = float.Parse(CoordStrs[i]);
        Camera camm = NetCloudServerManager.instance.cam;
        camm.transform.position = new Vector3( Coord[0], Coord[1], Coord[2]);
        PointMessage point = new PointMessage();
        
        if ( camm.RenderToCubemap(point.cubemap)) {
            int index = -1;
            Conn[] conns = NetCloudServerManager.cs.conns;
            for (int i = 0; i < conns.Length; i++) {
                if (conns[i] == null)
                    continue;
                if (!conns[i].isUse)
                    continue;
                if(conns[i].GetAdress()==SendIPAddress) {
                    index = i;
                }
                //print(" 将消息传播" + conns[i].GetAdress());
            }
            if (index < 0)
                return;
            int width = NetCloudServerManager.instance.width;
            ProtocolCubeTexture protocolCube = new ProtocolCubeTexture();
            protocolCube.srcTexture = new Texture2D(width, width);
            for (int i = 0; i < 3; i++)
                protocolCube.Coord[i] = int.Parse(CoordStrs[i]);
            for (int i=0;i<6;i++) {
                protocolCube.CubemapFace = i;
                protocolCube.srcTexture.SetPixels(point.cubemap.GetPixels((CubemapFace)i));
                protocolCube.srcTexture.Apply();
                conns[index].SendMsg(protocolCube);
            }
        }
        else {
            NetCloudServerManager.instance.debug.text+= ("\n" + protocolstr.str+"渲染失败");
        }
        
        

    }
}
