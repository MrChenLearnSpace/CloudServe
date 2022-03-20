using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointMessage {
    public Vector3 pos;
    public Cubemap cubemap;
    
    static int width;

    
 
    public PointMessage() {
        pos = Vector3.zero;
        width = NetCloudServerManager.instance.width;
       
        cubemap = new Cubemap(width, TextureFormat.RGBA32, false);
    }

    #region 接收
    public void ResolveToPointMessage(int[] Coord,int CubeMapFace,Texture2D texture) {
        pos.x = Coord[0];
        pos.y = Coord[1];
        pos.z = Coord[2];
        switch (CubeMapFace) {
            case 0: cubemap.SetPixels( texture.GetPixels(), CubemapFace.PositiveX); break;
            case 1: cubemap.SetPixels( texture.GetPixels(), CubemapFace.NegativeX); break;
            case 2: cubemap.SetPixels( texture.GetPixels(), CubemapFace.PositiveY); break;
            case 3: cubemap.SetPixels( texture.GetPixels(), CubemapFace.NegativeY); break;
            case 4: cubemap.SetPixels( texture.GetPixels(), CubemapFace.PositiveZ); break;
            case 5: cubemap.SetPixels( texture.GetPixels(), CubemapFace.NegativeZ); break;
        }
        
        cubemap.Apply();

    }
    Color[] ColorToCubemap(byte[] byt) {
        Texture2D textureTest = new Texture2D(width, width);
        textureTest.LoadImage(byt);
        textureTest.Apply();
        
        return textureTest.GetPixels() ;
    }
    #endregion




}
