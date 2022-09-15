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
    private Dictionary<GameObject, bool> registeredObjectSwitchStates;
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

        if (registeredObjectSwitchStates == null) {
            registeredObjectSwitchStates = new Dictionary<GameObject, bool>();
        } else {
            registeredObjectSwitchStates.Clear();
        }
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

    public void RegisterSwitchStatesInBulter() {
        butlerManager.SendToButlerCreateEntity("switchState", "on", new string[] { "on" }, true);
        butlerManager.SendToButlerCreateEntity("switchState", "off", new string[] { "off" }, true);
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
                butlerManager.SendToButlerCreateEntity("object", child.name, new string[] { child.name }, butlerObject.isGenderMale, butlerObject.additionalClasses);

                //Register color if appropriate
                if (butlerObject.colorizer != null) {
                    if (!registeredObjectColors.ContainsKey(child.gameObject)) {
                        Color objColor = butlerObject.colorizer.GetCurrentColor(child.gameObject);
                        registeredObjectColors[child.gameObject] = UnityToButlerColor(objColor);
                    }
                    string color = registeredObjectColors[child.gameObject];
                    butlerManager.SendToButlerColorInfo(child.name, color);
                }

                //Register switch state if appropriate
                if (butlerObject.switcher != null) {
                    if (!registeredObjectSwitchStates.ContainsKey(child.gameObject)) {
                        bool objSwitchState = butlerObject.switcher.GetSwitchState(child.gameObject);
                        registeredObjectSwitchStates[child.gameObject] = objSwitchState;
                    }
                    bool switchState = registeredObjectSwitchStates[child.gameObject];
                    butlerManager.SendToButlerSwitchStateInfo(child.name, UnityToButlerSwitchState(switchState));
                }
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
                    butlerManager.SendToButlerCreateEntity("object", newObject.name, new string[] { newObject.name }, butlerObjects[i].isGenderMale, butlerObjects[i].additionalClasses);
                    registeredObjects[butlerObjects[i].idName].Add(newObject);
                    ButlerManager.ButlerEvent colorizerEvent = new ButlerManager.ButlerEvent("colorizer", newObject.name, butlerEvent.color, butlerEvent.location, "off");
                    ProcessColorizationEvent(colorizerEvent);
                    ButlerManager.ButlerEvent switcherEvent = new ButlerManager.ButlerEvent("switcher", newObject.name, butlerEvent.color, butlerEvent.location, butlerEvent.switchValue);
                    ProcessSwitchEvent(switcherEvent);
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
                    if (colorizer != null) {
                        colorizer.Colorize(registeredObjects[type][j], ButlerToUnityColor(butlerEvent.color));
                        butlerManager.SendToButlerColorInfo(registeredObjects[type][j].name, butlerEvent.color);
                        registeredObjectColors[registeredObjects[type][j]] = butlerEvent.color;
                    }
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

    /*
     * TODO :
     * + Créer un ScriptableObject ButlerSwitcherSO avec un LightSwitcherSO qui permet d'allumer une lumière basique
     * + Dans le ButlerObjectSO, rajouter une variable ButlerSwitcherSO, qu'on laissera à null pour la plupart des objets
     * + Créer un Colorizer pour les lumières
     * + Enregistrer l'état des ampoules auprès du Butler quand on lance l'appli (de la même façon qu'on enregistre les couleurs), et uniquement pour les objets pertinents (qui ont un Switcher)
     * + Mettre à jour la fonction ProcessCreationEvent, qui doit aussi maintenant prendre en compte l'état de l'objet (allumé ou éteint) et l'appliquer lorsque c'est pertinent, et retransmettre au Butler
     * - Créer dans le Butler une règle qui nous envoie un événement de type switcher, sur le modèle du colorizer.
     */
    public void ProcessSwitchEvent(ButlerManager.ButlerEvent butlerEvent) {
        bool found = false;
        List<string> types = new List<string>(registeredObjects.Keys);
        int i = 0;
        while (!found && i < types.Count) {
            string type = types[i];
            int j = 0;
            while (!found && j < registeredObjects[type].Count) {
                if (registeredObjects[type][j].name == butlerEvent.objectName) {
                    ButlerSwitcherSO switcher = FindButlerObject(type).switcher;
                    if (switcher != null) {
                        bool unitySwitchState = ButlerToUnitySwitchState(butlerEvent.switchValue);
                        switcher.Switch(registeredObjects[type][j], unitySwitchState);
                        butlerManager.SendToButlerSwitchStateInfo(registeredObjects[type][j].name, butlerEvent.switchValue);
                        registeredObjectSwitchStates[registeredObjects[type][j]] = unitySwitchState;
                    }
                    found = true;
                }
                j++;
            }
            i++;
        }

        if (!found) {
            Debug.LogError("Could not switch butler object: unknown object \"" + butlerEvent.objectName + "\"");
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

    public static string UnityToButlerSwitchState(bool switchState) {
        return switchState ? "on" : "off";
    }

    public static bool ButlerToUnitySwitchState(string switchState) {
        return switchState == "on" ? true : false;
    }
}
