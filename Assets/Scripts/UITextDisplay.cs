using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextDisplay : MonoBehaviour
{
    public enum ExpandDirection
    {
        UP, DOWN
    }

    [SerializeField] private GameObject textBubblePrefab;
    [SerializeField] private bool autoErase;
    [SerializeField] private float autoEraseTime;
    [SerializeField] private Color textColor;
    [SerializeField] private float yOffsetPosition;
    [SerializeField] private ExpandDirection expandDirection;
    [SerializeField] private int maxBubbleNumber;

    private Coroutine eraseCoroutine;
    private Coroutine autoEraseCoroutine;

    private List<GameObject> currentBubbles;

    private void Start() {

        currentBubbles = new List<GameObject>();
    }

    public void DisplayText(string t) {

        if (eraseCoroutine != null) {
            StopCoroutine(eraseCoroutine);
            foreach (GameObject b in currentBubbles) {
                b.GetComponent<TMP_Text>().alpha = 1;
            }
        }
        if (autoEraseCoroutine != null) {
            StopCoroutine(autoEraseCoroutine);
        }
        GameObject bubble = Instantiate<GameObject>(textBubblePrefab);
        bubble.transform.SetParent(transform, false);
        ShiftCurrentBubbles();
        ((RectTransform)bubble.transform).anchoredPosition3D = new Vector3(0, yOffsetPosition, 0);
        //((RectTransform)bubble.transform).anchoredPosition3D = new Vector3(0, 0, 0);

        TMP_Text text = bubble.GetComponent<TMP_Text>();
        text.text = t;
        text.color = textColor;
        text.alpha = 1;

        currentBubbles.Add(bubble);

        if (maxBubbleNumber > 0) {
            while (currentBubbles.Count > maxBubbleNumber) {
                Destroy(currentBubbles[0]);
                currentBubbles.RemoveAt(0);
            }
        }

        if (autoErase) {
            autoEraseCoroutine = StartCoroutine(AutoEraseCoroutine());
        }
    }

    private void ShiftCurrentBubbles() {
        float direction = expandDirection == ExpandDirection.DOWN ? -1 : 1;
        float defaultShiftHeight = 10;
        for (int i = 0; i < currentBubbles.Count; i++) {
            Vector3 pos = ((RectTransform)currentBubbles[i].transform).anchoredPosition3D;
            ((RectTransform)currentBubbles[i].transform).anchoredPosition3D = new Vector3(pos.x, pos.y + (defaultShiftHeight * direction), pos.z);
        }
    }

    public void EraseText() {

        eraseCoroutine = StartCoroutine(EraseTextCoroutine());
    }

    private IEnumerator EraseTextCoroutine() {

        float vanishDuration = 1f;
        float t = 0f;
        float step = 0.05f;
        while (t <= vanishDuration) {
            foreach (GameObject bubble in currentBubbles) {
                bubble.GetComponent<TMP_Text>().alpha = Mathf.Lerp(1, 0, t / vanishDuration);
            }
            yield return new WaitForSeconds(step);
            t += step;
        }
        foreach (GameObject bubble in currentBubbles) {
            Destroy(bubble);
        }
        currentBubbles.Clear();
    }

    private IEnumerator AutoEraseCoroutine() {

        yield return new WaitForSeconds(autoEraseTime);
        EraseText();
    }
}
