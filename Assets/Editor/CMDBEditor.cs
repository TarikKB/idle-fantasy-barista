using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CMDatabase))]
public class CMDBEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CMDatabase database = (CMDatabase)target;

        SerializedProperty coffeeMakersProperty = serializedObject.FindProperty("coffeeMakers");

        EditorGUILayout.LabelField("Coffee Maker Database", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(coffeeMakersProperty, true);

        EditorGUILayout.Space(15);

        if (database.coffeeMakers == null || database.coffeeMakers.Count == 0)
        {
            EditorGUILayout.HelpBox("No coffee makers have been added to this database yet.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.LabelField("Coffee Maker Stats", EditorStyles.boldLabel);

        foreach (CMData coffeeMaker in database.coffeeMakers)
        {
            if (coffeeMaker == null)
            {
                EditorGUILayout.HelpBox("Missing coffee maker reference.", MessageType.Warning);
                continue;
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(coffeeMaker.coffeeMakerName, EditorStyles.boldLabel);

            EditorGUILayout.ObjectField("Asset", coffeeMaker, typeof(CMData), false);
            EditorGUILayout.ObjectField("Icon", coffeeMaker.icons[0], typeof(Sprite), false);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Brew Time", coffeeMaker.brewTimeSeconds + " seconds");
            EditorGUILayout.LabelField("Coffee Produced", coffeeMaker.coffeeProduced.ToString());

            EditorGUILayout.LabelField("Sell Price", "$" + coffeeMaker.sellPrice);
            EditorGUILayout.LabelField("Beans Required", coffeeMaker.beansRequired.ToString());

            EditorGUILayout.LabelField("Purchase Cost", "$" + coffeeMaker.purchaseCost);
            EditorGUILayout.LabelField("Unlock Level", coffeeMaker.unlockLevel.ToString());

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}