/*
Original Script By: Ditzel
Edited for PC/Editor + Mobile Support

Uses of this script:
 For controlling camera movement: Pan, zoom and rotate
 
How to use:
1. Create empty GameObject at (0,0,0) named "CameraController"
2. Attach this script to it
3. Assign Main Camera in Inspector
4. Set zoom bounds

PC Controls:
- Left Mouse Drag: Pan camera
- Mouse Scroll: Zoom in/out

Mobile Controls:
- One Finger Drag: Pan camera
- Two Finger Pinch: Zoom in/out
*/

using UnityEngine;

public class ScrollAndPinch : MonoBehaviour
{
    public Camera Camera;
    public bool Rotate = false;
    public float DecreaseCameraPanSpeed = 2f;
    public float CameraUpperHeightBound = 20f;
    public float CameraLowerHeightBound = 10f;
    public float MouseZoomSpeed = 0.5f;  // REDUCED from 3 to 0.5 for more conservative zoom

    private Vector3 cameraStartPosition;
    private Vector2 lastMousePos;
    private Plane groundPlane;
    private Transform cameraParent;

    private void Awake()
    {
        if (Camera == null)
            Camera = Camera.main;

        if (Camera == null)
        {
            Debug.LogError("ScrollAndPinch: Main Camera not found!");
            enabled = false;
            return;
        }

        cameraStartPosition = Camera.transform.position;
        lastMousePos = Input.mousePosition;
        
        // Create ground plane at y=0
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        // Get camera parent if it exists (for Cinemachine support)
        cameraParent = Camera.transform.parent;
        
        // Auto-correct zoom bounds if not set properly
        if (CameraUpperHeightBound <= 0)
        {
            CameraUpperHeightBound = 20f;
            Debug.LogWarning("CameraUpperHeightBound was 0, set to 20. Adjust in Inspector if needed.");
        }
        if (CameraLowerHeightBound <= 0)
        {
            CameraLowerHeightBound = 10f;
            Debug.LogWarning("CameraLowerHeightBound was 0, set to 10. Adjust in Inspector if needed.");
        }
        
        Debug.Log("ScrollAndPinch initialized.");
        Debug.Log("  Camera at: " + Camera.transform.position);
        Debug.Log("  Camera Parent: " + (cameraParent != null ? cameraParent.name : "None"));
        Debug.Log("  Zoom bounds: [" + (cameraStartPosition.y - CameraLowerHeightBound) + ", " + 
            (cameraStartPosition.y + CameraUpperHeightBound) + "]");
    }

    private void Update()
    {
#if UNITY_IOS || UNITY_ANDROID
        HandleTouchInput();
#else
        HandleMouseInput();
#endif
    }

    private void HandleMouseInput()
    {
        Vector2 currentMousePos = Input.mousePosition;
        
        // Validate mouse position
        if (float.IsInfinity(currentMousePos.x) || float.IsInfinity(currentMousePos.y) ||
            float.IsNaN(currentMousePos.x) || float.IsNaN(currentMousePos.y))
        {
            lastMousePos = currentMousePos;
            return;
        }

        // Also validate lastMousePos
        if (float.IsInfinity(lastMousePos.x) || float.IsInfinity(lastMousePos.y) ||
            float.IsNaN(lastMousePos.x) || float.IsNaN(lastMousePos.y))
        {
            lastMousePos = currentMousePos;
            return;
        }

        // Pan with left mouse button
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = currentMousePos - lastMousePos;
            
            if (mouseDelta.magnitude > 0.5f)
            {
                Ray rayPrev = Camera.ScreenPointToRay(lastMousePos);
                Ray rayCurr = Camera.ScreenPointToRay(currentMousePos);
                
                if (groundPlane.Raycast(rayPrev, out float distPrev) && 
                    groundPlane.Raycast(rayCurr, out float distCurr))
                {
                    Vector3 posPrev = rayPrev.GetPoint(distPrev);
                    Vector3 posCurr = rayCurr.GetPoint(distCurr);
                    Vector3 movement = (posPrev - posCurr) / DecreaseCameraPanSpeed;
                    
                    ApplyCameraMovement(movement);
                    Debug.Log("Pan: " + movement);
                }
            }
        }

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 currentPos = GetCameraPosition();
            Vector3 newPos = currentPos + Vector3.up * scroll * MouseZoomSpeed;
            
            float minY = cameraStartPosition.y - CameraLowerHeightBound;
            float maxY = cameraStartPosition.y + CameraUpperHeightBound;
            
            // Clamp to bounds instead of blocking
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            
            Debug.Log("Scroll detected: " + scroll + ", New Y: " + newPos.y + " (clamped)");
            
            ApplyCameraPosition(newPos);
            Debug.Log("Zoom: " + newPos.y);
        }

        lastMousePos = currentMousePos;
    }

    private Vector3 GetCameraPosition()
    {
        // If camera has parent (Cinemachine), move parent; else move camera
        if (cameraParent != null)
            return cameraParent.position;
        return Camera.transform.position;
    }

    private void ApplyCameraMovement(Vector3 movement)
    {
        if (cameraParent != null)
        {
            cameraParent.position += movement;
        }
        else
        {
            Camera.transform.position += movement;
        }
    }

    private void ApplyCameraPosition(Vector3 newPosition)
    {
        if (cameraParent != null)
        {
            cameraParent.position = newPosition;
        }
        else
        {
            Camera.transform.position = newPosition;
        }
    }

    private void HandleTouchInput()
    {
        // Single touch - pan
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Ray rayPrev = Camera.ScreenPointToRay(touch.position - touch.deltaPosition);
                Ray rayCurr = Camera.ScreenPointToRay(touch.position);
                
                if (groundPlane.Raycast(rayPrev, out float distPrev) && 
                    groundPlane.Raycast(rayCurr, out float distCurr))
                {
                    Vector3 posPrev = rayPrev.GetPoint(distPrev);
                    Vector3 posCurr = rayCurr.GetPoint(distCurr);
                    Vector3 movement = (posPrev - posCurr) / DecreaseCameraPanSpeed;
                    
                    ApplyCameraMovement(movement);
                }
            }
        }

        // Two touch - pinch zoom
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            Vector2 pos0 = touch0.position;
            Vector2 pos1 = touch1.position;
            Vector2 pos0Prev = pos0 - touch0.deltaPosition;
            Vector2 pos1Prev = pos1 - touch1.deltaPosition;
            
            float prevDist = Vector2.Distance(pos0Prev, pos1Prev);
            float currDist = Vector2.Distance(pos0, pos1);
            
            if (prevDist > 0)
            {
                float zoomRatio = currDist / prevDist;
                
                if (zoomRatio > 0.9f && zoomRatio < 1.1f) // Avoid extreme values
                {
                    Ray rayPrev = Camera.ScreenPointToRay(pos0Prev);
                    Ray rayCurr = Camera.ScreenPointToRay(pos0);
                    
                    if (groundPlane.Raycast(rayPrev, out float distPrev) && 
                        groundPlane.Raycast(rayCurr, out float distCurr))
                    {
                        Vector3 posPrev = rayPrev.GetPoint(distPrev);
                        Vector3 posCurr = rayCurr.GetPoint(distCurr);
                        
                        Vector3 currentPos = GetCameraPosition();
                        Vector3 newPos = Vector3.LerpUnclamped(posCurr, currentPos, 1 / zoomRatio);
                        
                        // Apply zoom bounds
                        if (newPos.y <= cameraStartPosition.y + CameraUpperHeightBound &&
                            newPos.y >= cameraStartPosition.y - CameraLowerHeightBound)
                        {
                            ApplyCameraPosition(newPos);
                        }
                    }
                }
            }
        }
    }

    //Returns the point between first and final finger position
    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = Camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = Camera.ScreenPointToRay(touch.position);
        if (groundPlane.Raycast(rayBefore, out var enterBefore) && groundPlane.Raycast(rayNow, out var enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = Camera.ScreenPointToRay(screenPos);
        if (groundPlane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 5);
    }
}
