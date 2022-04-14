using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButlerObjectManager : MonoBehaviour
{

    public ButlerManager butlerManager;
    public ButlerObjectSO[] butlerObjects;
    public ButlerColorSO[] butlerColors;
    public ButlerObjectSelectorSO currentObjectSelector;
    public ButlerObjectMove butlerObjectMove;

    private Dictionary<string, List<GameObject>> registeredObjects;
    private Dictionary<GameObject, string> registeredObjectColors;
    private GameObject instantiatedGhost;

    void Start() {
        InitRegistrationLists();
        RegisterObjectTypesInButler();
        RegisterColorsInButler();
        RegisterInstantiatedObjectsInButler();
    }

    public void InitRegistrationLists() {

        if (registeredObjects == null)
            registeredObjects = new Dictionary<string, List<GameObject>>();
        else
            registeredObjects.Clear();

        if (registeredObjectColors == null)
            registeredObjectColors = new Dictionary<GameObject, string>();
        else
            registeredObjectColors.Clear();
    }

    public void RegisterObjectTypesInButler() {

        foreach (ButlerObjectSO butlerObjectSO in butlerObjects) {
            butlerManager.SendToButlerCreateEntity("objectType", butlerObjectSO.idName, butlerObjectSO.texts, butlerObjectSO.isGenderMale);
            registeredObjects[butlerObjectSO.idName] = new List<GameObject>();
        }
    }

    public void RegisterColorsInButler() {
        foreach (ButlerColorSO butlerColorSO in butlerColors) {
            butlerManager.SendToButlerCreateEntity("color", butlerColorSO.idName, butlerColorSO.texts, true);
        }
    }

    public void RegisterInstantiatedObjectsInButler() {

        foreach (Transform child in transform) {
            int i = 0;
            ButlerObjectSO butlerObject = null;
            while (butlerObject == null && i < butlerObjects.Length) {
                if (child.name.Contains(butlerObjects[i].idName)) {
                    butlerObject = butlerObjects[i];
                }
                i++;
            }

            if (butlerObject != null) {
                registeredObjects[butlerObject.idName].Add(child.gameObject);
                butlerManager.SendToButlerCreateEntity("objet", child.name, new string[] { child.name }, butlerObject.isGenderMale, butlerObject.additionalClasses);
                if (!registeredObjectColors.ContainsKey(child.gameObject)) {
                    Color objColor = butlerObject.colorizer.GetCurrentColor(child.gameObject);
                    registeredObjectColors[child.gameObject] = UnityToButlerColor(objColor);
                }
                string color = registeredObjectColors[child.gameObject];
                butlerManager.SendToButlerColorInfo(child.name, color);
            }
        }
    }

    public void ProcessCreationEvent(ButlerManager.ButlerEvent butlerEvent) {

        bool found = false;
        int i = 0;
        while (!found && i < butlerObjects.Length) {
            if (butlerObjects[i].idName == butlerEvent.objectName) {
                Vector3 rotation = Vector3.zero;
                Vector3? position = null;
                if (instantiatedGhost != null) {
                    rotation = instantiatedGhost.transform.eulerAngles;
                    position = instantiatedGhost.transform.position;
                    Destroy(instantiatedGhost);
                    butlerObjectMove.Init();
                    butlerObjectMove.SetIsGhost(false);
                }
                int id = GetInstantiatedObjectCount(butlerObjects[i].idName) + 1;
                if (butlerEvent.location == null) {
                    instantiatedGhost = butlerObjects[i].creator.InstantiateGhost(butlerEvent, butlerObjects[i], id, transform);
                    currentObjectSelector.theObject = instantiatedGhost;
                    butlerObjectMove.Init();
                    butlerObjectMove.SetIsGhost(true);
                } else {
                    GameObject newObject = butlerObjects[i].creator.InstantiateObject(butlerEvent, butlerObjects[i], butlerManager, id, transform, position, rotation);
                    butlerManager.SendToButlerCreateEntity("objet", newObject.name, new string[] { newObject.name }, butlerObjects[i].isGenderMale, butlerObjects[i].additionalClasses);
                    registeredObjects[butlerObjects[i].idName].Add(newObject);
                    ButlerManager.ButlerEvent colorizerEvent = new ButlerManager.ButlerEvent("colorizer", newObject.name, butlerEvent.color, butlerEvent.location);
                    ProcessColorizationEvent(colorizerEvent);
                }
                found = true;
            } else {
                i++;
            }
        }

        if (i == butlerObjects.Length) {
            Debug.LogError("Could not create butler object: unknown type \"" + butlerEvent.objectName + "\"");
        }
    }
    public void ProcessColorizationEvent(ButlerManager.ButlerEvent butlerEvent) {

        bool found = false;
        List<string> types = new List<string>(registeredObjects.Keys);
        int i = 0;
        while (!found && i < types.Count) {
            string type = types[i];
            int j = 0;
            while (!found && j < registeredObjects[type].Count) {
                if (registeredObjects[type][j].name == butlerEvent.objectName) {
                    ButlerColorizerSO colorizer = FindButlerObject(type).colorizer;
                    colorizer.Colorize(registeredObjects[type][j], ButlerToUnityColor(butlerEvent.color));
                    butlerManager.SendToButlerColorInfo(registeredObjects[type][j].name, butlerEvent.color);
                    registeredObjectColors[registeredObjects[type][j]] = butlerEvent.color;
                    found = true;
                }
                j++;
            }
            i++;
        }

        if (!found) {
            Debug.LogError("Could not color butler object: unknown object \"" + butlerEvent.objectName + "\"");
        }
    }

    private ButlerObjectSO FindButlerObject(string objType) {
        bool found = false;
        int i = 0;
        ButlerObjectSO res = null;
        while (!found && i < butlerObjects.Length) {
            if (butlerObjects[i].idName == objType) {
                res = butlerObjects[i];
                found = true;
            } else {
            }
            i++;
        }
        return res;
    }

    public int GetInstantiatedObjectCount(string type) {

        return registeredObjects[type].Count;
    }

    public Color ButlerToUnityColor(string butlerColor) {
        Color unityColor = Color.white;
        bool found = false;
        int i = 0;
        while (!found && i < butlerColors.Length) {
            ButlerColorSO colorSO = butlerColors[i];
            if (butlerColor == colorSO.idName) {
                unityColor = colorSO.color;
                found = true;
            }
            i++;
        }
        return unityColor;
    }

    public string UnityToButlerColor(Color color) {
        string butlerColor = "blanc";
        bool found = false;
        int i = 0;
        while (!found && i < butlerColors.Length) {
            if (ColorUtility.ToHtmlStringRGB(color) == ColorUtility.ToHtmlStringRGB(butlerColors[i].color)) {
                butlerColor = butlerColors[i].idName;
                found = true;
            }
            i++;
        }
        return butlerColor;
    }

    public static Vector3 ButlerToUnityPosition(string posX, string posY, string posZ) {
        Vector3 res = Vector3.zero;
        //if (Application.platform == RuntimePlatform.WindowsEditor)//TODO à clarifier selon la plateforme et la version de Unity....
        res = new Vector3(float.Parse(posX.Replace(".", ",")), float.Parse(posY.Replace(".", ",")), float.Parse(posZ.Replace(".", ",")));
        //else
        //res = new Vector3(float.Parse(posX), float.Parse(posY), float.Parse(posZ));
        return res;
    }
}
