using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ButlerObject", menuName = "ScriptableObjects/ButlerObject")]
public class ButlerObjectSO : ScriptableObject
{

    public string idName;
    public string[] texts;
    public bool isGenderMale;
    public GameObject prefab;
    public GameObject ghostPrefab;
    public ButlerObjectCreatorSO creator;
    public ButlerColorizerSO colorizer;
    public ButlerSwitcherSO switcher;
    public string[] additionalClasses;
}
