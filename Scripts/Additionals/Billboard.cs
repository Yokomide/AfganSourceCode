using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (_cameraTransform != null)
        {
            transform.LookAt(transform.position + _cameraTransform.forward, _cameraTransform.up);
        }
    }
}