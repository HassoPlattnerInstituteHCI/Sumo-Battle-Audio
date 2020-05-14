using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PantoHandle : PantoBehaviour
{
    protected bool isUpper = true;
    private GameObject handledGameObject;
    private MeHandle meObject;
    private float speed;
    private bool inTransition = false;

    public IEnumerator SwitchTo(GameObject newHandle, float newSpeed)
    {
        if (inTransition)
        {
            Debug.LogWarning("Discarding not yet reached gameObject" + gameObject);
        }
        Debug.Log("Switching to:" + newHandle.name);
        handledGameObject = newHandle;
        speed = newSpeed;
        inTransition = true;

        while (inTransition)
        {
            yield return new WaitForSeconds(.01f);
        }
    }

    public void UnregisterHandledObject()
    {
        handledGameObject = null;
        pantoSync.FreeHandle(isUpper);
    }

    public void MeObject(MeHandle newMeObject)
    {
        meObject = newMeObject;
        pantoSync.registerHandle(this);
    }

    public bool HasMeObject()
    {
        return meObject != null;
    }

    public void NewPantoPositions(Vector3 upperHandlePos, Vector3 lowerHandlePos, float upperHandleRot, float lowerHandleRot)
    {
        if (isUpper)
        {
            UpdateMePosition(upperHandlePos, upperHandleRot);
        }
        else
        {
            UpdateMePosition(lowerHandlePos, lowerHandleRot);
        }
    }

    private void UpdateMePosition(Vector3 newPosition, float newRotation)
    {
        if (meObject == null)
        {
            return;
        }
        if (meObject.onlyRotation)
        {
            pantoSync.SetDebugObjects(isUpper, null, newRotation);
            return;
        }
        newPosition = meObject.UpdateMePosition(newPosition, newRotation);
        if (pantoSync.debug)
        {
            pantoSync.SetDebugObjects(isUpper, newPosition, newRotation);
        }
    }

    float MaxMovementSpeed()
    {
        return float.PositiveInfinity;
    }

    public void OverlayScriptedMotion(ScriptedMotion motion)
    {

    }

    public IEnumerator TraceObjectByPoints(List<GameObject> cornerObjects, float speed)
    {
        for (int i = 0; i < cornerObjects.Count; i++)
        {
            yield return SwitchTo(cornerObjects[i], speed);
        }
        yield return SwitchTo(cornerObjects[0], speed);
    }

    void Update()
    {
        if (handledGameObject == null)
        {
            Debug.Log("No active GameObject");
            return;
        }

        if (inTransition)
        {
            Vector3 currentPos = GetPantoSync().GetHandlePosition(isUpper);
            Vector3 goalPos = handledGameObject.transform.position;
            Vector3 distance = goalPos - currentPos;
            //float multiplier = Mathf.Min(Time.deltaTime, 0.25f); //Time.deltaTime > 0.25 ? 0.25 : Time.deltaTime;
            Vector3 movement = (distance).normalized * speed;
            if (distance.magnitude <= movement.magnitude)
            {
                Debug.Log("Reached: " + handledGameObject.name);
                inTransition = false;
            }
            else
            {
                if (handledGameObject.GetComponent<ItHandle>() != null && handledGameObject.GetComponent<ItHandle>().positionOnly)
                {
                    GetPantoSync().UpdateHandlePosition(currentPos + movement, null, isUpper);
                }
                else
                {
                    GetPantoSync().UpdateHandlePosition(currentPos + movement, handledGameObject.transform.eulerAngles.y, isUpper);
                }
            }
        }
        if (!inTransition)
        {
            GetPantoSync().UpdateHandlePosition(handledGameObject.transform.position, handledGameObject.transform.eulerAngles.y, isUpper);
        }
    }
}
