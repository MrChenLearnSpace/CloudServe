using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
public class Conn 
{
    public const int BUFFER_SIZE = 16384;
    public Socket socket;
    public bool isUse = false;
    public byte[] readBuff = new byte[BUFFER_SIZE];
    public int buffCount = 0;
    public Int32 msgLength = 0;
    public byte[] lenByte = new byte[sizeof(Int32)];

    public ProtocolByte assistProtolByte;
    public Conn() {
        readBuff = new byte[BUFFER_SIZE];
        assistProtolByte = new ProtocolByte();
    }
    public void Init(Socket socket) {
        this.socket = socket;
        isUse = true;
        buffCount = 0;
    }
    public int BuffRemain() {
        return BUFFER_SIZE - buffCount;
    }
    public string GetAdress() {
        if (!isUse) { return "无法获取地址"; }
        return socket.RemoteEndPoint.ToString();
    }
    public void Close() {
        if (!isUse) {
            return;
        }
        Console.WriteLine("[断开连接]" + GetAdress());
        socket.Close();
        isUse = false;
    }
    
    public bool SendMsg(ProtocolBase protol) {
        string proName = protol.GetName(); ;
        return SendMsg(protol, proName);

    }
    public bool SendMsg(ProtocolBase protol, string protolName) {

        ProtocolByte protocolByte1 = new ProtocolByte();
        protocolByte1.AddString(protolName);
        byte[] protolNameByte = protocolByte1.bytes;
        byte[] b = protol.Encode();
        byte[] len1 = BitConverter.GetBytes(protolNameByte.Length + b.Length);
        byte[] sendByte = len1.Concat(protolNameByte).Concat(b).ToArray();
        socket.Send(sendByte);
        Debug.Log("sendByte " + sendByte.Length);
        Debug.Log("sendBytes " + GetDesc(sendByte));
        return true;
    }
    public  string GetDesc(byte[] bytes) {
        string str = "";
        if (bytes == null) return str;
        for (int i = 0; i < bytes.Length; i++) {
            int b = (int)bytes[i];
            str += b.ToString() + " ";
        }
        return str;
    }
}
