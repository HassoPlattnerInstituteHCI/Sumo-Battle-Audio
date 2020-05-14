using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItHandle : HighLevelHandle
{
    private const float BaseSpeed = 0.2f;
    public bool positionOnly = true;

    public new void deactivate()
    {
        base.deactivate();
        pantoHandle.UnregisterHandledObject();
    }

    public IEnumerator SwitchTo(GameObject gameObject)
    {
        if (getIsActive())
        {
            pantoHandle.UnregisterHandledObject();
            yield return pantoHandle.SwitchTo(gameObject, BaseSpeed);
        }
    }

    public IEnumerator SwitchToNewPosition(Vector3 newPosition)
    {
        if (getIsActive())
        {
            pantoHandle.UnregisterHandledObject();
            gameObject.transform.position = newPosition;
            yield return pantoHandle.SwitchTo(gameObject, BaseSpeed);
        }
    }
}
