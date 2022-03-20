using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class CloudService 
{
    public Socket Listerfd;
    public Conn[] conns;
    public int maxConn = 50;
    
    public bool status = false;
    public MsgHandle msgHandle;

    public CloudService() {
        msgHandle = new MsgHandle();
    }
    public int NewIndex() {
        if (conns == null) {
            return -1;
        }
        for (int i = 0; i < conns.Length; i++) {
            if (conns[i] == null) {
                conns[i] = new Conn();
                return i;
            }
            else if (conns[i].isUse == false) {
                return i;
            }
        }
        return -1;
    }
    public void ServiceStart(string host, int port) {
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++) {
            conns[i] = new Conn();
        }
        Listerfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAdr = IPAddress.Parse(host);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        Listerfd.Bind(ipEp);
        Listerfd.Listen(maxConn);
        Listerfd.BeginAccept(AcceptCb, null);
        status = true;
        NetCloudServerManager.instance.debug.text+=("\n"+"[服务器]启动成功");
    }
    virtual public void AcceptCb(IAsyncResult ar) {
        Socket socket = Listerfd.EndAccept(ar);
        try {
            //接收客户端
            int index = NewIndex();
            if (index < 0) {
                socket.Close();
                Debug.Log("[警告]连接已满");
            }
            else {
                Conn conn = conns[index];
                conn.Init(socket);
                string host = conn.GetAdress();
                Debug.Log("客户端连接:[" + host + "] conn池ID:" + index);
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReciveCb, conn);//接收的同时调用ReciveCb回调函数
            }
            Listerfd.BeginAccept(AcceptCb, null);//再次调用AcceprCb回调函数
        }
        catch (Exception e) {
            Debug.Log("AccpetCb 失败:" + e.Message);
        }
    }
    virtual public void ReciveCb(IAsyncResult ar) {
        Conn conn = (Conn)ar.AsyncState;//这个AsyncState就是上面那个BeginRecive函数里面最后一个参数
        lock (conn) {
            try {
                int count = conn.socket.EndReceive(ar);//返回接收的字节数
                                                       //没有信息就关闭
                if (count <= 0) {
                    Debug.Log("收到[" + conn.GetAdress() + "] 断开连接");
                    conn.Close();
                    return;
                }
                conn.buffCount += count;
                ProcessData(conn);
                #region(不使用协议的数据处理)
                /*//数据处理
                string str = System.Text.Encoding.UTF8.GetString(conn.readBffer,0,count);
                Console.WriteLine("收到[" + conn.GetAdress() + "] 数据"+str);
                str = conn.GetAdress() + ":" + str;
                byte[] wrBuffer = System.Text.Encoding.Default.GetBytes(str);
                for (int i = 0; i < conns.Length; i++)
                {
                    if (conns[i]==null||!conns[i].isUse)//没有连接或者未被使用就跳过
                    {
                        continue;
                    }
                    Console.WriteLine("将消息传播给+",conns[i].GetAdress());
                    conns[i].socket.Send(wrBuffer);
                }*/
                #endregion
                //继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReciveCb, conn);
            }
            catch (Exception e) {
                Debug.Log("Recive失败" + e.Message);
            }
        }
    }
    virtual public void ProcessData(Conn conn) {
        //小于字节长度
        if (conn.buffCount < sizeof(Int32)) {
            return;
        }
        Debug.Log("接收到了 " + conn.buffCount + " 个字节");
        Array.Copy(conn.readBuff, conn.lenByte, sizeof(Int32));
        conn.msgLength = BitConverter.ToInt32(conn.lenByte, 0);
        //小于最小要求长度则返回表示未接收完全
        if (conn.buffCount < conn.msgLength + sizeof(Int32)) {
            return;
        }
        #region(不使用协议的消息处理)
        //处理消息
        /*string str = Encoding.UTF8.GetString(conn.readBffer,sizeof(Int32),conn.msgLen);//不使用协议的获取字符串
        Console.WriteLine("收到消息 [" + conn.GetAdress() + "] " + str);
        Send(conn, str);*/
        #endregion

        //这里接收信息有个细节，因为之前发送回来的信息又被加了一次长度，相当于要把他所有的信息接收完了
        //才算接收成功，然后再把前面的sizeof(Int32)去掉，剩下的就是带长度的信息了
        ProtocolByte proto = new ProtocolByte();
        ProtocolByte protoStr = new ProtocolByte();
        ProtocolByte protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength) as ProtocolByte;
        protoStr.AddString(conn.GetAdress());
        protocol.bytes = protoStr.bytes.Concat(protocol.bytes).ToArray();
        lock(msgHandle.msgList) {
            msgHandle.msgList.Add(protocol);
        }
        Debug.Log( protocol.GetDesc());


        //清除已处理的消息
        int count = conn.buffCount - conn.msgLength - sizeof(Int32);
        Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
        conn.buffCount = count;
        //如果还有多余信息就继续处理
        if (conn.buffCount > 0) {
            ProcessData(conn);
        }
    }
    /*
    private void AcceptCb(IAsyncResult ar) {
        try {
            Socket socket = Listerfd.EndAccept(ar);
            int index = NewIndex();
            if (index < 0) {
                socket.Close();
                // print("[警告]连接已满");
                NetCloudServerManager.instance.debug.text += ("\n" + "[警告]连接已满");
            }
            else {
                Conn conn = conns[index];
                conn.Init(socket);
                string adr = conn.GetAdress();
                // print("客户端连接[" + adr + "] conn池ID:" + index);
                NetCloudServerManager.instance.debug.text += ("\n" + "客户端连接[" + adr + "] conn池ID:" + index);
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            Listerfd.BeginAccept(AcceptCb, null);
        }
        catch (Exception e) {
            // print("AcceptCb失败:" + e.Message);
            NetCloudServerManager.instance.debug.text += ("\n" + "AcceptCb失败:" + e.Message);
        }
    }
    private void ReceiveCb(IAsyncResult ar) {
        Conn conn = (Conn)ar.AsyncState;
        //try {
        int count = conn.socket.EndReceive(ar);
        if (count <= 0) {
            //print("收到[" + conn.GetAdress() + "]连接断开");
            NetCloudServerManager.instance.debug.text += ("\n" + "收到[" + conn.GetAdress() + "]连接断开");
            conn.Close();
            return;
        }
        conn.buffCount = conn.buffCount + count;
        conn.ProcessData();
        conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
        #region 旧版
        //socket.BeginReceive(readBuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None, ReceiveCb, readBuff);
        //string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
        //print("收到[" + conn.GetAdress() + "]数据 " + str);//图片这里要改
        //NCSM.HandleMassages.Add(conn.GetAdress() + " " + str);
        //print(conn.GetAdress() + " " + str);
        //byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        //for (int i = 0; i < conns.Length; i++) {
        //    if (conns[i] == null)
        //        continue;
        //    if (!conns[i].isUse)
        //        continue;
        //    print(" 将消息传播" + conns[i].GetAdress());
        //    conns[i].socket.Send(bytes);


        //}
        #endregion
    }
    */

}
