using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TelevisionController : MonoBehaviour
{
    public VideoPlayer player;
    public RawImage image;

    public void Switch(bool b) {
        image.enabled = b;
        if (b) {
            player.Play();
        } else {
            player.Stop();
        }
    }

    public bool GetSwitchState() {
        return image.enabled;
    }
}
