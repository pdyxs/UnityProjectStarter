using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace I2.Loc
{
	using TranslationDictionary = Dictionary<string, TranslationQuery>;

	public struct TranslationQuery
	{
        public string OrigText;
        public string Text;           // Text without Tags
        public string LanguageCode;
		public string[] TargetLanguagesCode;
		public string[] Results;			// This is filled google returns the translations
        public string[] Tags;               // This are the sections of the orignal text that shoudn't be translated
	}

	public static partial class GoogleTranslation
	{
		static List<WWW> mCurrentTranslations = new List<WWW>();
#region Multiple Translations

		public static void Translate( TranslationDictionary requests, Action<TranslationDictionary, string> OnTranslationReady, bool usePOST = true )
		{
			WWW www = GetTranslationWWW( requests, usePOST );
			I2.Loc.CoroutineManager.Start(WaitForTranslation(www, OnTranslationReady, requests));
		}

        public static bool ForceTranslate(TranslationDictionary requests, bool usePOST = true)
        {
            WWW www = GetTranslationWWW(requests, usePOST);
            while (!www.isDone);

            string errorMsg = www.error;

            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg.Contains("rewind"))
                    return ForceTranslate(requests, false); // use GET instead
                return false;
            }

            var bytes = www.bytes;
            var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
            var error = ParseTranslationResult(wwwText, requests);
            return string.IsNullOrEmpty(error);
        }


        public static WWW GetTranslationWWW(  TranslationDictionary requests, bool usePOST=true )
		{
            #if !UNITY_5_6_OR_NEWER
                usePOST = false;
            #endif
            var sb = new StringBuilder ();

			foreach (var kvp in requests)
			{
				var request = kvp.Value;
				if (sb.Length>0)
					sb.Append("<I2Loc>");

				sb.Append(GoogleLanguages.GetGoogleLanguageCode(request.LanguageCode));
				sb.Append(":");
				for (int i=0; i<request.TargetLanguagesCode.Length; ++i)
				{
					if (i!=0) sb.Append(",");
					sb.Append( GoogleLanguages.GetGoogleLanguageCode(request.TargetLanguagesCode[i]) );
				}
				sb.Append("=");

				var text = (TitleCase(request.Text) == request.Text) ? request.Text.ToLowerInvariant() : request.Text;

                if (usePOST)
                {
                    sb.Append(text);
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(text));
                    if (sb.Length > 4000)
                        break;
                }                    
			}

            if (usePOST)
            {
                WWWForm form = new WWWForm();
                form.AddField("action", "Translate");
                form.AddField("list", sb.ToString());

                WWW www = new WWW(LocalizationManager.GetWebServiceURL(), form);
                return www;
            }
            else
            {
                string url = string.Format("{0}?action=Translate&list={1}", LocalizationManager.GetWebServiceURL(), sb.ToString());
                //Debug.Log(url);
                return new WWW(url);
            }
        }
		
		static IEnumerator WaitForTranslation(WWW www, Action<TranslationDictionary, string> OnTranslationReady, TranslationDictionary requests)
		{
			mCurrentTranslations.Add (www);
			while (!www.isDone)
                yield return null;

			int numWWW = mCurrentTranslations.Count;
			mCurrentTranslations.Remove (www);
			if (numWWW == mCurrentTranslations.Count) 
			{
				// Translation was canceled using CancelCurrentGoogleTranslations()
				yield break;
			}

            string errorMsg = www.error;

            if (!string.IsNullOrEmpty(errorMsg))
			{
                // check for 
                if (errorMsg.Contains("rewind"))
                //if (errorMsg.Contains("necessary data rewind wasn't possible"))
                {
                    // use GET instead
                    Translate(requests, OnTranslationReady, false);
                    yield break;
                }
				//Debug.LogError (www.error);
				OnTranslationReady(requests, www.error);
			}
			else
			{
                var bytes = www.bytes;
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                errorMsg = ParseTranslationResult(wwwText, requests);
				OnTranslationReady( requests, errorMsg );
			}
		}

		public static string ParseTranslationResult( string html, TranslationDictionary requests )
		{
			//Debug.Log(html);
			// Handle google restricting the webservice to run
			if (html.StartsWith("<!DOCTYPE html>") || html.StartsWith("<HTML>"))
			{
                if (html.Contains("The script completed but did not return anything"))
                    return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
                else
                if (html.Contains("Service invoked too many times in a short time"))
                    return ""; // ignore and try again
                else
                    return "There was a problem contacting the WebService. Please try again later\n" + html;
			}

			string[] texts = html.Split (new string[]{"<I2Loc>"}, StringSplitOptions.None);
			string[] splitter = new string[]{"<i2>"};
			int i = 0;

			var Keys = requests.Keys.ToArray();
			foreach (var text in Keys)
			{
				var temp = FindQueryFromOrigText(text, requests);
                var fullText = texts[i++];
                if (temp.Tags != null)
                {
                    for (int j = 0, jmax = temp.Tags.Length; j < jmax; ++j)
                        fullText = fullText.Replace(  /*"{[" + j + "]}"*/ ((char)(0x2600+j)).ToString(), temp.Tags[j]);
                }

                temp.Results = fullText.Split (splitter, StringSplitOptions.None);

				// Google has problem translating this "This Is An Example"  but not this "this is an example"
				if (TitleCase(text)==text)
				{
					for (int j=0; j<temp.Results.Length; ++j)
						temp.Results[j] = TitleCase(temp.Results[j]);
				}
				requests[temp.OrigText] = temp;
			}
			return null;
		}

        static TranslationQuery FindQueryFromOrigText( string origText, TranslationDictionary dict)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value.OrigText == origText)
                    return kvp.Value;
            }
            return default(TranslationQuery);
        }

		public static bool IsTranslating()
		{
			return mCurrentTranslations.Count>0;
		}

		public static void CancelCurrentGoogleTranslations()
		{
			mCurrentTranslations.Clear ();
		}

#endregion
	}
}

