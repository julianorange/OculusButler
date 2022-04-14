using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dpoch.SocketIO;
using System.Text;
using UnityEngine.Networking;
using System;

public class ButlerManager : MonoBehaviour
{

    //private static readonly string BUTLER_IP = "127.0.0.1"; // localhost
    //public static readonly string BUTLER_IP = "192.168.50.178"; // chez julian
    public static readonly string BUTLER_IP = "192.168.1.58"; // à orange

    [System.Serializable]
    public class ButlerEvent
    {

        public string eventType;
        public string objectName;
        public string color;
        public ButlerLocation location;

        public ButlerEvent(string _eventType, string _objectName, string _color, ButlerLocation _location) {
            eventType = _eventType;
            objectName = _objectName;
            color = _color;
            location = _location;
        }

        public override string ToString() {
            string str = "eventType: " + eventType;
            str += ", objectName: " + objectName;
            str += ", color: " + color;
            str += ", location: " + location;
            return str;
        }
    }

    [System.Serializable]
    public class ButlerLocation
    {
        public string x;
        public string y;
        public string z;

        public override string ToString() {
            return "{x:" + x + ", y:" + y + ", z:" + z + "}";
        }
    }

    [System.Serializable]
    public class ButlerFact
    {
        [System.Serializable]
        public class Result
        {
            public string location;
            public string focus;

            public override string ToString() {
                string str = "{";
                int i = 0;
                if (location != null && location.Trim() != "") {
                    str += "location: " + location;
                    i++;
                }
                if (focus != null && focus.Trim() != "") {
                    str += i > 0 ? ", " : "";
                    str += "focus: " + focus;
                    i++;
                }
                str += "}";
                return str;
            }
        }

        public Result[] result;

        public override string ToString() {
            if (result == null)
                return "null";
            string str = "[";
            int i = 0;
            foreach (Result r in result) {
                if (i > 0) {
                    str += ", ";
                }
                str += i + ":";
                str += r.ToString();
                i++;
            }
            str += "]";
            return str;
        }
    }

    public enum ButlerFactType
    {
        CURRENT_LOCATION, FOCUS
    }

    public ButlerObjectManager objectManager;
    public UITextDisplay butlerTextDisplay;

    private SocketIO socket;
    private string lastFocus;
    private List<string> unityBehaviours;

    private float checkInterval = 0.5f; //in seconds
    private float lastCheck;

    private ButlerFact currentLocation;
    private ButlerFact focus;

    // Start is called before the first frame update
    void Start() {

        socket = new SocketIO("ws://" + BUTLER_IP + ":3001/socket.io/?EIO=4&transport=websocket");

        socket.OnOpen += () => Debug.Log("Socket open");

        socket.On("socket id", (ev) => {
            socket.Emit("socket id", "unity");
            Debug.Log("socket id \"unity\" sent to Butler.");
        });

        socket.On("event", (ev) => {
            //Debug.Log("BUTLER EVENT : " + ev.Data[0]);
            ButlerEvent myEvent = null;
            try {
                myEvent = ev.Data[0].ToObject<ButlerEvent>();
            } catch (Exception e) {
                //Debug.LogError(e.Message);
                //Debug.Log("*** Butler event received with error: " + ev.Data[0]);
                butlerTextDisplay.DisplayText(ev.Data[0].ToString());
            }
            if (myEvent != null)
                ProcessButlerEvent(myEvent);
        });
        socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
        socket.OnClose += () => {
            Debug.Log("Socket closed");
        };
        socket.OnError += (err) => Debug.Log("Socket Error: " + err);

        socket.Connect();

        lastFocus = "";

        InitBehavioursList();
        lastCheck = checkInterval;
    }

    private void InitBehavioursList() {
        if (unityBehaviours == null) {
            unityBehaviours = new List<string>();
        } else {
            unityBehaviours.Clear();
        }
    }

    private void Update() {

        if (lastCheck >= checkInterval) {
            lastCheck = 0;
            CheckButlerFacts();
        }
        lastCheck += Time.deltaTime;
    }

    private void CheckButlerFacts() {
        AskButlerForCurrentLocation();
        AskButlerForFocus();
    }

    private void ProcessButlerEvent(ButlerEvent butlerEvent) {

        if (butlerEvent.eventType == "creator") {
            objectManager.ProcessCreationEvent(butlerEvent);
        } else if (butlerEvent.eventType == "colorizer") {
            objectManager.ProcessColorizationEvent(butlerEvent);
        } else {
            Debug.LogError("Unknown butler event type: " + butlerEvent.eventType);
        }
    }

    public void SendToButlerFocusOn(string objectName) {

        if (objectName != lastFocus) {
            string json = "{ \"kind\":\"formula\", \"author\":\"unity\", \"content\":\"(B unity (unity_focus " + objectName + "))\"}";
            StartCoroutine(SendEventToButler(json));
            lastFocus = objectName;
        }
    }

    public void SendToButlerColorInfo(string objectName, string color) {

        string json = "{ \"kind\":\"formula\", \"author\":\"unity\", \"content\":\"(B unity (unity_color " + objectName + " " + color + "))\"}";
        StartCoroutine(SendEventToButler(json));
    }

    public void SendUtteranceToButler(string utterance) {

        string json = "{\"kind\" :\"utterance\", \"author\":\"unity\", \"content\" :\"" + utterance + "\"}";
        StartCoroutine(SendEventToButler(json));
    }

    public void SendToButlerCreateEntity(string objectClass, string objectName, string[] texts, bool genderMale, string[] additionalClasses = null) {

        string _objectName = objectName.Replace(" ", "_").Replace("'", "");

        /*Il faut remplacer tout ça. Ici on ajoute des implications logiques sur les instances d'objets créées, il faut créer des implications logiques sur les objectType
        *On ne doit plus avoir (=> (entity object meubletélé1) (entity objet meubletélé1))
        *D'abord, il faut faire en sorte que les instances d'objets se voient attribuer l'objectType en class name plutôt que le terme "objet"
        *à la place de l'entité (entity objet meubletélé1), on aura (entity meubletélé meubletélé1)
        *Puis en créera des implications logiques à partir de ça
        *(=> (entity meubletélé ?x) (entity meuble ?x)), ou encore (entity meubletélé ?x) (entity objet ?x)), ou même (entity meuble ?x) (entity objet ?x)) (à voir pour celui là), etc.
        *Il faudra créer les entités correspondant à chaque implication :
        *{class: objectType,
        * name: meuble,
        * texts: ["meuble", "meubles",...]
        * facts: ["(=> (entity meubletélé ?x) (entity meuble ?x))"] //<= ici l'implication en question
        * }
        * Enfin, on pourra avoir l'intention suivante
        * intent: (=> (entity %objectType% ?x) (color ?x rouge))
        * dans la règle NLU qui va bien, et qui permettra de colorer n'importe quelle classe d'objets
        */
        string additionalClassesStr = "";
        if (additionalClasses != null) {
            for (int i = 0; i < additionalClasses.Length; i++) {
                additionalClassesStr += "\"(=> (entity " + objectClass + " " + _objectName + ") (entity " + additionalClasses[i] + " " + _objectName + "))\"";
                if (i < additionalClasses.Length - 1) {
                    additionalClassesStr += ",";
                }
                additionalClassesStr += "\n";
            }
        }

        //Texts
        string textsStr = "";
        for (int i = 0; i < texts.Length; i++) {
            textsStr += "\"" + texts[i] + "\"";
            if (i < texts.Length - 1) {
                textsStr += ",";
            }
            textsStr += "\n";
        }

        string json = "{ \"title\":\"Unity " + _objectName + " entity\"," +
                        "\"id\":\"unity_" + _objectName + "\", " +
                        "\"entities\":[" +
                            "{" +
                                "\"class\": \"" + objectClass + "\"," +
                                "\"name\": \"" + _objectName + "\"," +
                                "\"gender\": \"" + (genderMale ? "male" : "female") + "\"," +
                                "\"number\": \"singular\"," +
                                "\"texts\": [" +
                                    textsStr +
                                "]" +
                            "}" +
                        "]," +
                        "\"facts\":[" +
                            additionalClassesStr +
                        "]" +
                      "}";
        Debug.Log(json);

        unityBehaviours.Add("unity_" + _objectName);
        StartCoroutine(SendBehaviourToButler(json));
    }

    public void AskButlerForCurrentLocation() {

        string formula = "(unity_current_location ?location)";
        StartCoroutine(GetFactFromButler(formula, ButlerFactType.CURRENT_LOCATION));
    }

    public void AskButlerForFocus() {

        string formula = "(unity_focus ?focus)";
        StartCoroutine(GetFactFromButler(formula, ButlerFactType.FOCUS));
    }

    private void OnApplicationQuit() {

        foreach (string id in unityBehaviours) {
            StartCoroutine(DeleteBehaviourInButler(id));
        }
    }

    private void OnApplicationPause(bool pause) {

        //Workaround to handle the exit of an oculus application, as OnApplicationQuit is never called. Should be commented when used on another device?
        if (pause) {
            foreach (string id in unityBehaviours) {
                StartCoroutine(DeleteBehaviourInButler(id));
            }
            objectManager.InitRegistrationLists();
            InitBehavioursList();
        } else {
            InitBehavioursList();
            objectManager.InitRegistrationLists();
            objectManager.RegisterObjectTypesInButler();
            objectManager.RegisterColorsInButler();
            objectManager.RegisterInstantiatedObjectsInButler();
        }
    }

    private IEnumerator SendEventToButler(string json) {
        UnityWebRequest www = new UnityWebRequest("http://" + BUTLER_IP + ":3001/api/v1/events", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("butler:.Butler0")));
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        } else {
            Debug.Log("Event \"" + json + "\" message successfully sent to Butler");
        }
        www.Dispose();
    }

    private IEnumerator SendBehaviourToButler(string json) {
        UnityWebRequest www = new UnityWebRequest("http://" + BUTLER_IP + ":3001/api/v1/behaviours", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("butler:.Butler0")));
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        } else {
            Debug.Log("Behaviour \"" + json + "\" successfully sent to Butler");
        }
        www.Dispose();
    }

    private IEnumerator DeleteBehaviourInButler(string id) {
        UnityWebRequest www = new UnityWebRequest("http://" + BUTLER_IP + ":3001/api/v1/behaviours/" + id, "DELETE");
        www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("butler:.Butler0")));
        www.SetRequestHeader("accept", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        } else {
            Debug.Log("Behaviour \"" + id + "\" successfully deleted in the Butler");
        }
        www.Dispose();
    }

    private IEnumerator GetFactFromButler(string formula, ButlerFactType factType) {
        UnityWebRequest www = new UnityWebRequest("http://" + BUTLER_IP + ":3001/api/v1/data/facts?formula=" + formula + "&format=result", "GET");
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("butler:.Butler0")));
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        } else {
            string answer = Encoding.UTF8.GetString(www.downloadHandler.data);
            switch (factType) {
                case ButlerFactType.CURRENT_LOCATION:
                    currentLocation = JsonUtility.FromJson<ButlerFact>(answer);
                    break;
                case ButlerFactType.FOCUS:
                    focus = JsonUtility.FromJson<ButlerFact>(answer);
                    break;
                default:
                    break;
            }
        }
        www.Dispose();
    }

    public Vector3 GetCurrentLocationFact() {

        Vector3 res = Vector3.zero;
        if (currentLocation != null && currentLocation.result != null && currentLocation.result.Length > 0) {
            string location = currentLocation.result[currentLocation.result.Length - 1].location;
            location = location.Replace("{", "");
            location = location.Replace("}", "");
            string[] split = location.Split(',');
            foreach (string token in split) {
                string tk = token;
                //TODO: make some tests on other platforms
                //if (Application.platform == RuntimePlatform.WindowsEditor)
                tk = token.Replace(".", ",");
                string[] tokenSplit = tk.Split(':');
                if (tokenSplit[0] == "x")
                    res.x = float.Parse(tokenSplit[1]);
                else if (tokenSplit[0] == "y")
                    res.y = float.Parse(tokenSplit[1]);
                else if (tokenSplit[0] == "z")
                    res.z = float.Parse(tokenSplit[1]);
            }
        }
        return res;
    }

    public bool HasCurrentLocationFact() {

        return currentLocation != null && currentLocation.result != null && currentLocation.result.Length > 0;
    }
}
