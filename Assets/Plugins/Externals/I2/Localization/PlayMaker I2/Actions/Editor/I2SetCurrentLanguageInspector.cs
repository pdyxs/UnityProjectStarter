using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMakerEditor;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{

    [CustomActionEditor(typeof(I2SetCurrentLanguage))]
    public class I2SetCurrentLanguageInspector : CustomActionEditor
    {
        I2SetCurrentLanguage _target;
		LanguageSource mSource;

        public override bool OnGUI()
        {

            var changed = false;

            _target = (I2SetCurrentLanguage) target;

			EditField("LanguageSource");

            EditField("SelectionMode");

            if (_target.SelectionMode == I2LocPlayMaker_SelectionMode.FsmVariable)
            {
                EditField("language");
            }
            else
            {
                changed = EditLanguageSelection();
            }

            EditField("success");
            EditField("successEvent");
            EditField("failureEvent");


            return GUI.changed || changed;
        }


        bool EditLanguageSelection()
        {
            var _changed = false;

			if (_target.language == null)
				return false;
			
            // make sure our FsmString is not pointing to a variable.
            _target.language.UseVariable = false;


            LocalizationManager.UpdateSources();
            string[] Languages = LocalizationManager.GetAllLanguages().ToArray();
            System.Array.Sort(Languages);


            int index = System.Array.IndexOf(Languages, _target.language.Value);

            int newIndex = EditorGUILayout.Popup("Language", index, Languages);
            if (newIndex != index)
            {
                index = newIndex;
                _changed = true;
                if (index < 0 || index >= Languages.Length)
                    _target.language.Value = string.Empty;
                else
                    _target.language.Value = Languages[index];
            }

            return _changed;
        }


    }
}