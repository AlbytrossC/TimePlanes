using UnityEngine;

/// <summary>
/// Destroys this GameObject once it has no children left. Put it on formation
/// roots: once every member has been destroyed (shot down or flown off-screen)
/// the empty group-logic object cleans itself up.
/// </summary>
public class DestroyWhenChildrenGone : MonoBehaviour
{
    private void LateUpdate()
    {
        if (transform.childCount == 0)
            Destroy(gameObject);
    }
}
