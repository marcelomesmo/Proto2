using UnityEngine;

public class MoveableObjectScript : MonoBehaviour
{
    //when the user clicks on the object and holds, the user can move the object around
    private Vector3 offset;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    private void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPosition() + offset;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = cam.WorldToScreenPoint(transform.position).z; 
        return cam.ScreenToWorldPoint(mousePoint);
    }
    
}
