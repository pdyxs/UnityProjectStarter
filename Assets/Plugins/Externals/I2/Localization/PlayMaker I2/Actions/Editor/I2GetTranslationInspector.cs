using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMakerEditor;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{

    [CustomActionEditor(typeof(I2GetTranslation))]
    public class I2GetTranslationInspector : CustomActionEditor
    {
        I2GetTranslation _target;

        public override bool OnGUI()
        {

            var changed = false;

            _target = (I2GetTranslation) target;

            EditField("SelectionMode");

            if (_target.SelectionMode == I2LocPlayMaker_SelectionMode.FsmVariable)
            {
                EditField("term");
            }
            else
            {
                changed = EditTermSelection();
            }

            EditField("translation");

            return GUI.changed || changed;
        }


        bool EditTermSelection()
        {
			if (_target.term == null || _target.term.IsNone)
                return false;

            // make sure our FsmString is not pointing to a variable.
            _target.term.UseVariable = false;

            var terms = (_target.mSource != null) ? _target.mSource.GetTermsList() : LocalizationManager.GetTermsList();
            terms.Sort(System.StringComparer.OrdinalIgnoreCase);
            terms.Add("");
            terms.Add("<inferred from text>");
            terms.Add("<none>");
            var aTerms = terms.ToArray();


            var index = (_target.term.Value == "-" || _target.term.Value == "" ? aTerms.Length - 1: 
                        (_target.term.Value == " " ? aTerms.Length - 2 : 
                        System.Array.IndexOf( aTerms, _target.term.Value)));

            var newIndex = EditorGUILayout.Popup("Term", index, aTerms);

            if (index == newIndex) return false;

            _target.term.Value = (newIndex < 0 || newIndex == aTerms.Length - 1) ? string.Empty : aTerms[newIndex];
            if (newIndex == aTerms.Length - 1)
                _target.term.Value = "-";
            else if (newIndex < 0 || newIndex == aTerms.Length - 2)
                _target.term.Value = string.Empty;
            else
                _target.term.Value = aTerms[newIndex];

            return true;
        }


    }
}