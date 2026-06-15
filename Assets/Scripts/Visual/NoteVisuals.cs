using UnityEngine;

public class NoteVisuals : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] float bobAmplitude = 0.2f;
    [SerializeField] float bobFrequency = 2f;

  
    [SerializeField] Transform visualTransform;

    float initialY;
    float timeOffset;

    void Awake()
    {
        if (visualTransform != null)
        {
            initialY = visualTransform.localPosition.y;
        }
    }

    void OnEnable()
    {
      
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (visualTransform == null) return;

        visualTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

        float newY = initialY + Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;

        Vector3 pos = visualTransform.localPosition;
        pos.y = newY;
        visualTransform.localPosition = pos;
    }
}