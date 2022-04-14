using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateObjectData : MonoBehaviour
{

    public ButlerColorizerSO colorizer;

    private ButlerObjectDataSO data;

    void Awake() {

        if (data == null) {
            CreateData();
        }
    }

    void Update() {
        //Debug.Log("Updating data of " + name + ": position = " + data.position);
        //transform.position = data.position;
        //transform.eulerAngles = data.rotation;
        //if (colorizer != null) {
        //    colorizer.Colorize(gameObject, data.color);
        //}
    }

    private void CreateData() {
        data = ButlerObjectDataSO.CreateInstance<ButlerObjectDataSO>();
        //data.Init(transform.position, transform.eulerAngles, colorizer != null ? Color.white : colorizer.GetCurrentColor(gameObject));
        if (colorizer != null) {
            data.Init(transform.position, transform.eulerAngles, colorizer.GetCurrentColor(gameObject));
        } else {
            data.Init(transform.position, transform.eulerAngles, Color.white);
        }
    }

    public ButlerObjectDataSO GetButlerObjectData() {

        if (data == null) {
            CreateData();
        }
        return data;
    }
}
