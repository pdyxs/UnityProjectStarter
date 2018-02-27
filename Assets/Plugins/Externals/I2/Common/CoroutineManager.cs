﻿using UnityEngine;
using System.Collections;

namespace I2
{
	// This class is used to spawn coroutines from outside of MonoBehaviors
	public class CoroutineManager : MonoBehaviour 
	{
		public static CoroutineManager pInstance
		{
			get{
				if (mInstance==null)
				{
					GameObject GO = new GameObject( "_Coroutiner" );
                    GO.hideFlags = HideFlags.HideAndDontSave;
					mInstance = GO.AddComponent<CoroutineManager>();
                }
                return mInstance;
			}
		}

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static Coroutine Start(IEnumerator coroutine)
		{
			return pInstance.StartCoroutine(coroutine);
		}
		private static CoroutineManager mInstance;
	}
}
