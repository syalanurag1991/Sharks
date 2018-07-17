//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System;
using UnityEngine;
using System.IO;

#if NET_4_6
using SharpEXR;
#endif

namespace Vulcan
{
    public class EXRLoader
    {
        const string _decodedMapFolderName = "DecodedWarpMapCache";
        const string DEFAULT_EXTENSION = ".bytes";

        public static Texture2D DecodeEXR(string exrFileName, int textureWidth, int textureHeight, string mapsPath, string cachePath = _decodedMapFolderName)
        {
            Texture2D decodedEXR;
            if (exrFileName != null && textureWidth > 0 && textureHeight > 0)
            {
                byte[] exrBytes;
                try
                {
                    exrBytes = LoadDecodedEXR(exrFileName, mapsPath, cachePath);
                }
                catch (Exception e)
                {
                    exrBytes = null;
                    Debug.LogErrorFormat("[EXRLoader] Failed to load exrFile {0} Error: {1}", exrFileName, e.Message);
                }

                if (exrBytes != null)
                {
                    decodedEXR = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false, true);
                    decodedEXR.filterMode = FilterMode.Point;
                    decodedEXR.wrapMode = TextureWrapMode.Clamp;
                    decodedEXR.anisoLevel = 0;
                    decodedEXR.LoadRawTextureData(exrBytes);
                    decodedEXR.Apply();
                }
                else
                {
                    decodedEXR = null;
                }
            }
            else
            {
                decodedEXR = null;
                Debug.LogErrorFormat("[EXRLoader] Invalid parameter");
            }

            // Debug Only
            // System.IO.File.WriteAllBytes(exrFileName, decodedEXR.EncodeToEXR());

            return decodedEXR;
        }

        public static void ClearCachedEXRs()
        {
            string[] filePaths = System.IO.Directory.GetFiles(Application.persistentDataPath + _decodedMapFolderName, "*" + DEFAULT_EXTENSION);
            foreach (string filePath in filePaths)
            {
                System.IO.File.Delete(filePath);
            }
        }

        static void CacheDecodedEXR(byte[] exrBytes, string assetName, string cachePath)
        {
            string directory = cachePath;
            string path = Path.Combine(directory, assetName);
            path = Path.ChangeExtension(path, DEFAULT_EXTENSION);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                File.WriteAllBytes(path, exrBytes);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[EXRLoader] Failed to save decoded exr to file: {0} Error: {1}", path, e.Message);
            }
        }

        static byte[] LoadDecodedEXR(string fileName, string mapPath, string cacheFolderName)
        {
            byte[] decodedEXR = null;

            string cacheDirectory = Path.Combine(mapPath, cacheFolderName);
            string path = Path.Combine(cacheDirectory, fileName);
            path = Path.ChangeExtension(path, DEFAULT_EXTENSION);
            //
            // Try to load the cached version
            //
            if (Directory.Exists(cacheDirectory) && File.Exists(path))
            {
                //
                // Cached version seems to exist, try to load
                //
                try
                {
                    decodedEXR = File.ReadAllBytes(path);
                }
                catch (Exception e)
                {
                    decodedEXR = null;
                    Debug.LogErrorFormat("[EXRLoader] Failed to load exr file from: {0}  Error {1}", path, e.Message);
                }
            }
            else
            {
                //
                // No cached version exists, load the raw file and cached it
                //

                string exrPath = Path.Combine(mapPath, fileName);
                exrPath = Path.ChangeExtension(exrPath, DEFAULT_EXTENSION);
#if NET_4_6
                try
                {
                    EXRFile exrFile = EXRFile.FromFile(exrPath);
                    EXRPart part = exrFile.Parts[0];
                    part.OpenParallel(exrPath);
                    decodedEXR = part.GetBytes(ImageDestFormat.RGBA32, GammaEncoding.Linear);
                }
                catch (Exception e)
                {
                    decodedEXR = null;
                    Debug.LogErrorFormat("[EXRLoader] Failed to load exr file: {0} Error: {1}", exrPath, e.Message);
                }
#endif

                //
                // Cache if successful
                //
                if (decodedEXR != null)
                {
                    CacheDecodedEXR(decodedEXR, fileName, cacheDirectory);
                }
            }

            return decodedEXR;
        }
    }
}