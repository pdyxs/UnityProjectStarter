using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace I2.Loc
{
	using TranslationDictionary = Dictionary<string, TranslationQuery>;

	public static partial class GoogleTranslation
	{
		public static bool CanTranslate ()
		{
			return (LocalizationManager.Sources.Count > 0 && 
					!string.IsNullOrEmpty (LocalizationManager.GetWebServiceURL()));
		}

        #region Single Translation

        // LanguageCodeFrom can be "auto"
        // After the translation is returned from Google, it will call OnTranslationReady(TranslationResult, ErrorMsg)
        // TranslationResult will be null if translation failed
        public static void Translate( string text, string LanguageCodeFrom, string LanguageCodeTo, Action<string, string> OnTranslationReady )
		{
            LocalizationManager.InitializeIfNeeded();
            if (!GoogleTranslation.CanTranslate())
            {
                OnTranslationReady(null, "WebService is not set correctly or needs to be reinstalled");
                return;
            }
            //LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);

            if (LanguageCodeTo==LanguageCodeFrom)
            {
                OnTranslationReady(text, null);
                return;
            }

            TranslationDictionary queries = new TranslationDictionary();


            // Unsupported language
            if (string.IsNullOrEmpty(LanguageCodeTo))
            {
                OnTranslationReady(string.Empty, null);
                return;
            }


            CreateQueries(text, LanguageCodeFrom, LanguageCodeTo, queries);   // can split plurals into several queries

			Translate(queries, (results,error)=>
			{
					if (!string.IsNullOrEmpty(error) || results.Count==0)
					{
						OnTranslationReady(null, error);
						return;
					}

					string result = RebuildTranslation( text, queries, LanguageCodeTo);				// gets the result from google and rebuilds the text from multiple queries if its is plurals
					OnTranslationReady( result, null );
			});
		}

        // Query google for the translation and waits until google returns
        // On some Unity versions (e.g. 2017.1f1) unity doesn't handle well waiting for WWW in the main thread, so this call can fail
        // In those cases, its advisable to use the Async version  (GoogleTranslation.Translate(....))
        public static string ForceTranslate ( string text, string LanguageCodeFrom, string LanguageCodeTo )
        {
            TranslationDictionary dict = new TranslationDictionary();
            AddQuery(text, LanguageCodeFrom, LanguageCodeTo, dict);

            WWW www = GetTranslationWWW(dict);
        	while (!www.isDone);

        	if (!string.IsNullOrEmpty(www.error))
        	{
        		//Debug.LogError ("-- " + www.error);
        		return string.Empty;
        	}
        	else
        	{
                var bytes = www.bytes;
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                var error = ParseTranslationResult(wwwText, dict);
                if (!string.IsNullOrEmpty(error))
                    return null;
                else
                    return GetQueryResult(text, LanguageCodeTo, dict);

                /*var bytes = www.bytes;
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                if (wwwText.StartsWith("<!DOCTYPE html>") || wwwText.StartsWith("<HTML>"))
                    return string.Empty;
                return wwwText;*/
        	}
        }

        public static void CreateQueries( string text, string LanguageCodeFrom, string LanguageCodeTo, TranslationDictionary dict )
		{
			if (!text.Contains ("[i2p_")) 
			{
				AddQuery (text, LanguageCodeFrom, LanguageCodeTo, dict);
				return;
			}

			// Get pluralType 'Plural'
			int idx0 = 0;
			int idx1 = text.IndexOf ("[i2p_");
			if (idx1 == 0)  // Handle case where the text starts with a plural tag
			{
				idx0 = text.IndexOf ("]", idx1)+1;
				idx1 = text.IndexOf ("[i2p_");
				if (idx1 < 0) idx1 = text.Length;
			}

			var pluralText = text.Substring (idx0, idx1 - idx0);

			var regex = new Regex(@"{\[(.*?)\]}");

			for (var i = (ePluralType)0; i <= ePluralType.Plural; ++i) 
			{
				if (!GoogleLanguages.LanguageHasPluralType(LanguageCodeTo, i.ToString()))
					continue;

				var newText = pluralText;
				int testNumber = GoogleLanguages.GetPluralTestNumber (LanguageCodeTo, i);
				newText = regex.Replace(newText, testNumber.ToString());

				AddQuery (newText, LanguageCodeFrom, LanguageCodeTo, dict);
			}
		}

		public static void AddQuery( string text, string LanguageCodeFrom, string LanguageCodeTo, TranslationDictionary dict )
		{
			if (string.IsNullOrEmpty (text))
				return;
			
			if (!dict.ContainsKey (text)) 
			{
                var query = new TranslationQuery() { OrigText = text, LanguageCode = LanguageCodeFrom, TargetLanguagesCode = new string[] { LanguageCodeTo } };
                query.Text = text;
                ParseNonTranslatableElements(ref query);
                dict[text] = query;
			}
			else
			{
				var query = dict [text];
				if (System.Array.IndexOf (query.TargetLanguagesCode, LanguageCodeTo) < 0) {
					query.TargetLanguagesCode = query.TargetLanguagesCode.Concat (new string[]{ LanguageCodeTo }).Distinct ().ToArray ();
				}
				dict [text] = query;
			}
		}

        static int FindClosingTag(string tag, MatchCollection matches, int startIndex)
        {
            for (int i = startIndex, imax = matches.Count; i < imax; ++i)
            {
                var newTag = GetCaptureMatch(matches[i]);
                if (newTag[0]=='/' && tag.StartsWith(newTag.Substring(1)))
                    return i;
            }
            return -1;
        }

        static string GetCaptureMatch(Match match)
        {

            for (int i=match.Groups.Count-1; i>=0; --i)
                if (match.Groups[i].Success)
                {
                    return match.Groups[i].ToString();
                }
            return match.ToString();
        }

        static void ParseNonTranslatableElements( ref TranslationQuery query )
        {
            //\[i2nt].*\[\/i2nt]
            var matches = Regex.Matches(  query.Text, @"\{\[(.*?)]}|\[(.*?)]|\<(.*?)>");
            if (matches == null || matches.Count == 0)
                return;

            string finalText = query.Text;
            List<string> finalTags = new List<string>();
            for (int i=0, imax=matches.Count; i<imax; ++i)
            {
                var tag = GetCaptureMatch( matches[i] );
                int iClosingTag = FindClosingTag(tag, matches, i); //  find [/tag] or </tag>

                if (iClosingTag < 0)
                {
                    // Its not a tag, its a parameter
                    var fulltag = matches[i].ToString();
                    if (fulltag.StartsWith("{[") && fulltag.EndsWith("]}"))
                    {
                        finalText = finalText.Replace(fulltag, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());  //  0x2600 is the start of the UNICODE Miscellaneous Symbols table, so they are not going to be translated by google
                        finalTags.Add(fulltag);
                    }
                    continue;
                }

                if (tag == "i2nt")
                {
                    var tag1 = query.Text.Substring(matches[i].Index, (matches[iClosingTag].Index-matches[i].Index) + matches[iClosingTag].Length);
                    finalText = finalText.Replace(tag1, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());      
                    finalTags.Add(tag1);
                }
                else
                {
                    var tag1 = matches[i].ToString();
                    finalText = finalText.Replace(tag1, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());
                    finalTags.Add(tag1);

                    var tag2 = matches[iClosingTag].ToString();
                    finalText = finalText.Replace(tag2, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());
                    finalTags.Add(tag2);
                }
            }

            query.Text = finalText;
            query.Tags = finalTags.ToArray();
        }

        public static string GetQueryResult(string text, string LanguageCodeTo, TranslationDictionary dict)
        {
            if (!dict.ContainsKey(text))
                return null;

            var query = dict[text];
            if (query.Results == null || query.Results.Length < 0)
                return null;

            if (string.IsNullOrEmpty(LanguageCodeTo))
                return query.Results[0];

            int idx = System.Array.IndexOf(query.TargetLanguagesCode, LanguageCodeTo);
            if (idx < 0)
                return null;

            return query.Results[idx];
        }


        public static string RebuildTranslation( string text, TranslationDictionary dict, string LanguageCodeTo )
		{
			if (!text.Contains ("[i2p_")) 
			{
				return GetTranslation (text, LanguageCodeTo, dict);
			}

			// Get pluralType 'Plural'
			int idx0 = 0;
			int idx1 = text.IndexOf ("[i2p_");
			if (idx1 == 0)  // Handle case where the text starts with a plural tag
			{
				idx0 = text.IndexOf ("]", idx1)+1;
				idx1 = text.IndexOf ("[i2p_");
				if (idx1 < 0) idx1 = text.Length;
			}
			var pluralText = text.Substring (idx0, idx1 - idx0);
			var match = Regex.Match(pluralText, @"{\[(.*?)\]}");
			var param = (match == null ? string.Empty : match.Value);
						

			var sb = new System.Text.StringBuilder ();

			var newText = pluralText;
			int testNumber = GoogleLanguages.GetPluralTestNumber (LanguageCodeTo, ePluralType.Plural);
			newText = newText.Replace(param, testNumber.ToString());
			var translation = GetTranslation (newText, LanguageCodeTo, dict);
			string pluralTranslation = translation.Replace (testNumber.ToString (), param);
			sb.Append ( pluralTranslation );

			for (var i = (ePluralType)0; i < ePluralType.Plural; ++i)
			{
				if (!GoogleLanguages.LanguageHasPluralType(LanguageCodeTo, i.ToString()))
					continue;

				newText = pluralText;
				testNumber = GoogleLanguages.GetPluralTestNumber (LanguageCodeTo, i);
				newText = newText.Replace(param, testNumber.ToString());

				translation = GetTranslation (newText, LanguageCodeTo, dict);

				translation = translation.Replace (testNumber.ToString (), param);


				if (!string.IsNullOrEmpty (translation) && translation!=pluralTranslation) 
				{
					sb.Append ("[i2p_");
					sb.Append (i.ToString ());
					sb.Append (']');
					sb.Append (translation);
				}
			}

			return sb.ToString ();
		}

		static string GetTranslation( string text, string LanguageCodeTo, TranslationDictionary dict )
		{
			if (!dict.ContainsKey (text))
				return null;
			var query = dict [text];

			int langIdx = System.Array.IndexOf (query.TargetLanguagesCode, LanguageCodeTo);
			if (langIdx < 0)
				return "";

            if (query.Results == null)
                return "";
			return query.Results [langIdx];
		}


		/*static string ParseTranslationResult( string html, string OriginalText )
		{
			try
			{
				// This is a Hack for reading Google Translation while Google doens't change their response format
				int iStart = html.IndexOf("TRANSLATED_TEXT") + "TRANSLATED_TEXT='".Length;
				int iEnd = html.IndexOf("';INPUT_TOOL_PATH", iStart);
				
				string Translation = html.Substring( iStart, iEnd-iStart);
				
				// Convert to normalized HTML
				Translation = System.Text.RegularExpressions.Regex.Replace(Translation,
				                                                           @"\\x([a-fA-F0-9]{2})",
				                                                           match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)));
				
				// Convert ASCII Characters
				Translation = System.Text.RegularExpressions.Regex.Replace(Translation,
				                                                           @"&#(\d+);",
				                                                           match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value)));
				
				Translation = Translation.Replace("<br>", "\n");
				
				if (OriginalText.ToUpper()==OriginalText)
					Translation = Translation.ToUpper();
				else
					if (UppercaseFirst(OriginalText)==OriginalText)
						Translation = UppercaseFirst(Translation);
				else
					if (TitleCase(OriginalText)==OriginalText)
						Translation = TitleCase(Translation);
				
				return Translation;
			}
			catch (System.Exception ex) 
			{ 
				Debug.LogError(ex.Message); 
				return string.Empty;
			}
		}*/

#endregion

		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToLower().ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
		public static string TitleCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}

#if NETFX_CORE
			var sb = new StringBuilder(s);
			sb[0] = char.ToUpper(sb[0]);
			for (int i = 1, imax=s.Length; i<imax; ++i)
			{
				if (char.IsWhiteSpace(sb[i - 1]))
					sb[i] = char.ToUpper(sb[i]);
				else
					sb[i] = char.ToLower(sb[i]);
			}
			return sb.ToString();
#else
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
#endif
		}
	}
}

