using UnityEngine;
using System;

namespace I2.Loc
{
    public static class PersistentStorage
    {
#if UNITY_WEBGL || UNITY_WSA || NETFX_CORE
        public static bool Save( string fileName, string data )
        {
            try
            {
                PlayerPrefs.SetString(fileName, data);
                return true;
            }
            catch ( Exception )
            {
                Debug.LogError("Error saving PlayerPrefs " + fileName);
                return false;
            }
        }

        public static string Load( string fileName )
        {
            return PlayerPrefs.GetString(fileName, null);
        }

        public static void Delete( string fileName )
        {
            PlayerPrefs.DeleteKey(fileName);
        }
#else
        public static bool Save(string fileName, string data)
        {
            try
            {
                var path = Application.persistentDataPath + "/" + fileName + ".loc";
                System.IO.File.WriteAllText(path, data, System.Text.Encoding.UTF8 );
                return true;
            }
            catch (Exception)
            {
                Debug.LogError("Error saving file " + fileName);
                return false;
            }
        }

        public static string Load(string fileName)
        {
            try
            {
                var path = Application.persistentDataPath + "/" + fileName + ".loc";
                return System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void Delete( string fileName )
        {
            try
            {
                var path = Application.persistentDataPath + "/" + fileName + ".loc";
                System.IO.File.Delete(path);
            }
            catch (Exception)
            {
            }
        }
#endif


        public static bool SaveFile(string fileName, string data)
        {
            try
            {
                //Debug.Log("Saving file: " + fileName);
                System.IO.File.WriteAllText(fileName, data, System.Text.Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                Debug.LogError("Error saving file " + fileName);
                return false;
            }
        }

        public static string LoadFile(string fileName)
        {
            try
            {
                return System.IO.File.ReadAllText(fileName, System.Text.Encoding.UTF8);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void DeleteFile(string fileName)
        {
            try
            {
                System.IO.File.Delete(fileName);
            }
            catch (Exception)
            {
            }
        }

        public static bool HasFile(string fileName)
        {
            return System.IO.File.Exists(fileName);
        }

    }
}