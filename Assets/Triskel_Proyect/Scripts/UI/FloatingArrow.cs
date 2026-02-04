using UnityEngine;

public class FloatingArrow : MonoBehaviour
{
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private float speed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
