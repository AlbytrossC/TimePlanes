using UnityEngine;

/// <summary>
/// Destroys the target (default: this GameObject) once it is beyond the camera
/// view plus a margin. Used by enemies, formation members, and later bullets.
/// By default the object must have been on-screen at least once before it can
/// be destroyed, so things spawning just above the screen (e.g. formation
/// members trailing behind their engine) are not killed before entering.
/// </summary>
public class DestroyWhenOffscreen : MonoBehaviour
{
    [Tooltip("Extra world units beyond the screen edge allowed before destruction.")]
    [SerializeField] private float margin = 2f;
    [Tooltip("What to destroy. Defaults to this GameObject; a child can point at its formation root instead.")]
    [SerializeField] private GameObject target;
    [Tooltip("Only allow destruction after the object has been on-screen at least once.")]
    [SerializeField] private bool requireOnScreenFirst = true;

    private Camera _camera;
    private bool _hasBeenOnScreen;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_camera == null) return;
        float halfH = _camera.orthographicSize;
        float halfW = halfH * _camera.aspect;
        Vector3 c = _camera.transform.position;
        Vector3 p = transform.position;

        bool outsideX = p.x < c.x - halfW - margin || p.x > c.x + halfW + margin;
        bool outsideY = p.y < c.y - halfH - margin || p.y > c.y + halfH + margin;

        if (!outsideX && !outsideY &&
            p.x >= c.x - halfW && p.x <= c.x + halfW && p.y >= c.y - halfH && p.y <= c.y + halfH)
            _hasBeenOnScreen = true;

        if (requireOnScreenFirst && !_hasBeenOnScreen) return;

        if (outsideX || outsideY)
            Destroy(target != null ? target : gameObject);
    }
}
