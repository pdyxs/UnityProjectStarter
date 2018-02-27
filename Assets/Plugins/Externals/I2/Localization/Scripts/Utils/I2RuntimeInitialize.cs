using System.Collections.Generic;
using UnityEngine;


namespace I2.Loc
{ 
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class I2RuntimeInitialize : RuntimeInitializeOnLoadMethodAttribute
    {
        #if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
            public I2RuntimeInitialize() : base(RuntimeInitializeLoadType.BeforeSceneLoad)
            {
            }
        #else
            public I2RuntimeInitialize()
            {
            }
        #endif
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    #if UNITY_EDITOR
        public class I2EditorInitialize : UnityEditor.InitializeOnLoadAttribute
        {
        }
    #else
        public class I2EditorInitialize : System.Attribute
        {
        }
    #endif
}

