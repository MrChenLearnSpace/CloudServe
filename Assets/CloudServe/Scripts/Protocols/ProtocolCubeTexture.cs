using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ProtocolCubeTexture : ProtocolBase {
    public int width = NetCloudServerManager.instance.width;


    public int[] Coord = new int[] { 0, 0, 0 };
    public Texture2D srcTexture;
    public int CubemapFace = -1;
    
    //解码器
    public override ProtocolBase Decode(byte[] readBuff, int start, int length) {
        ProtocolCubeTexture proTexture = new ProtocolCubeTexture();
        ProtocolByte proByte = new ProtocolByte();
        proByte.bytes = readBuff;
        for(int i=0;i<3;i++) {
            proTexture.Coord[i] = proByte.GetInt(start, ref start);
        }
        CubemapFace = proByte.GetInt(start, ref start);

        //图片解析
        int srcStart = start;
        int srcLength = readBuff.Length-start;
        byte[] srcbyte = new byte[srcLength];
        Array.Copy(readBuff, srcStart, srcbyte, 0, srcLength);
        srcTexture = new Texture2D(width,width);
        proTexture.srcTexture.LoadImage(srcbyte);
        proTexture.srcTexture.Apply();
        return (ProtocolBase) proTexture;
    }
   
    //编码器
    public override byte[] Encode() {

        //立方体面编译
        
        ProtocolByte proByte = new ProtocolByte();
        
        for(int i=0;i<3;i++) {
            proByte.AddInt(Coord[i]);
        }
        proByte.AddInt(CubemapFace);
        byte[] cubeFaceBytes = proByte.Encode();
        //图片编译
        if (srcTexture == null)
            return null;
        byte[] srcTextureByte = srcTexture.EncodeToJPG();
        //byte[] lenByte = BitConverter.GetBytes(srcTextureByte.Length + cubeFaceBytes.Length);
        byte[] byt = cubeFaceBytes.Concat(srcTextureByte).ToArray();
        return byt;
    }
    public override string GetName() {

        return "ProtocolCubeTexture";
    }
    public override string GetDesc() {
        string str = "";
        byte[] srcTextureByte = srcTexture.EncodeToJPG();
        if (srcTextureByte == null) return str;
        for (int i = 0; i < srcTextureByte.Length; i++) {
            int b = (int)srcTextureByte[i];
            str += b.ToString() + " ";
        }
        return str;
    }
}
