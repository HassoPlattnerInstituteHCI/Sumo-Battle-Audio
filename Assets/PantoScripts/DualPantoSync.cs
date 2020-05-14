using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class DualPantoSync : MonoBehaviour
{
    public delegate void SyncDelegate(ulong handle);
    public delegate void HeartbeatDelegate(ulong handle);
    public delegate void LoggingDelegate(IntPtr msg);
    public delegate void PositionDelegate(ulong handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R8, SizeConst = 10)] double[] positions);
    public bool debug = false;
    public float debugScrollSpeed = 10.0f;
    public KeyCode toggleVisionKey = KeyCode.Space;
    protected ulong Handle;

    private static List<PantoHandle> registeredPantoHandles = new List<PantoHandle>();

    // bounds are defined by center and extent
    private static Vector2[] pantoBounds = { new Vector2(0, -110), new Vector2(320, 160) };
    private static Vector2[] unityBounds;

    [Tooltip("Upper middle of the Play Area, if you use the default size, you don't need to update this")]
    public static Vector3 handleDefaultPosition = new Vector3(0f, 0f, 24f); //unityToPanto(new Vector2(0, -32));
    private static Vector3 upperHandlePos = handleDefaultPosition;
    private static Vector3 lowerHandlePos = handleDefaultPosition;
    private static Vector3 upperGodObject = handleDefaultPosition;
    private static Vector3 lowerGodObject = handleDefaultPosition;
    private static float lowerHandleRot = 0f;
    private static float upperHandleRot = 0f;

    private bool isBlindModeOn = false;
    private ushort currentObstacleId = 0;
    private GameObject debugLowerObject;
    private GameObject debugUpperObject;

    [DllImport("serial")]
    private static extern uint GetRevision();
    [DllImport("serial")]
    private static extern void SetSyncHandler(SyncDelegate func);
    [DllImport("serial")]
    private static extern void SetHeartbeatHandler(HeartbeatDelegate func);
    [DllImport("serial")]
    private static extern void SetLoggingHandler(LoggingDelegate func);
    [DllImport("serial")]
    private static extern ulong Open(IntPtr port);
    [DllImport("serial")]
    private static extern void Close(ulong handle);
    [DllImport("serial")]
    private static extern void Poll(ulong handle);
    [DllImport("serial")]
    private static extern void SendSyncAck(ulong handle);
    [DllImport("serial")]
    private static extern void SendHeartbeatAck(ulong handle);
    [DllImport("serial")]
    private static extern void SendMotor(ulong handle, byte controlMethod, byte pantoIndex, float positionX, float positionY, float rotation);
    [DllImport("serial")]
    private static extern void FreeMotor(ulong handle, byte controlMethod, byte pantoIndex);
    [DllImport("serial")]
    private static extern void SetPositionHandler(PositionDelegate func);
    [DllImport("Serial")]
    private static extern void CreateObstacle(ulong handle, byte pantoIndex, ushort obstacleId, float vector1x, float vector1y, float vector2x, float vector2y);
    [DllImport("Serial")]
    private static extern void AddToObstacle(ulong handle, byte pantoIndex, ushort obstacleId, float vector1x, float vector1y, float vector2x, float vector2y);
    [DllImport("Serial")]
    private static extern void RemoveObstacle(ulong handle, byte pantoIndex, ushort obstacleId);
    [DllImport("Serial")]
    private static extern void EnableObstacle(ulong handle, byte pantoIndex, ushort obstacleId);
    [DllImport("Serial")]
    private static extern void DisableObstacle(ulong handle, byte pantoIndex, ushort obstacleId);

    private static void SyncHandler(ulong handle)
    {
        Debug.Log("Received sync");
        SendSyncAck(handle);
    }

    private static void HeartbeatHandler(ulong handle)
    {
        Debug.Log("Received heartbeat");
        SendHeartbeatAck(handle);
    }

    private static void LogHandler(IntPtr msg)
    {
        String message = Marshal.PtrToStringAnsi(msg);
        if (message.Contains("Free heap") || message.Contains("Task \"Physics\"") || message.Contains("Task \"I/O\""))
        {
            return;
        }
        else if (message.Contains("failed"))
        {
            Debug.LogError(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    private static void PositionHandler(ulong handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R8, SizeConst = 10)] double[] positions)
    {
        //Debug.Log("Received positions: (" + positions[5] + "|" + positions[6] + "rot:" + positions[7] + ")");
        Vector2 unityPosUpper = pantoToUnity(new Vector2((float)positions[0], (float)positions[1]));
        Vector2 unityGodUpper = pantoToUnity(new Vector2((float)positions[3], (float)positions[4]));
        upperHandlePos = new Vector3(unityPosUpper.x, 0, unityPosUpper.y);
        upperHandleRot = PantoToUnityRotation(positions[2]);
        upperGodObject = new Vector3(unityGodUpper.x, 0, unityGodUpper.y);

        Vector2 unityPosLower = pantoToUnity(new Vector2((float)positions[5], (float)positions[6]));
        Vector2 unityGodLower = pantoToUnity(new Vector2((float)positions[8], (float)positions[9]));
        lowerHandlePos = new Vector3(unityPosLower.x, 0, unityPosLower.y);
        lowerHandleRot = PantoToUnityRotation(positions[7]);
        lowerGodObject = new Vector3(unityGodLower.x, 0, unityGodLower.y);

        updatePantoHandles();
    }

    private static void updatePantoHandles()
    {
        //foreach (PantoHandle.)
        foreach (PantoHandle pantoHandle in registeredPantoHandles)
        {
            pantoHandle.NewPantoPositions(upperHandlePos, lowerHandlePos, upperHandleRot, lowerHandleRot);
        }
    }

    public Vector3 GetHandlePosition(bool isUpper)
    {
        if (debug)
        {
            GameObject debugObject = getDebugObject(isUpper);
            return debugObject.transform.position;
        }
        if (isUpper)
        {
            return upperHandlePos;
        }
        else
        {
            return lowerHandlePos;
        }
    }

    private static ulong OpenPort(string port)
    {
        return Open(Marshal.StringToHGlobalAnsi(port));
    }

    private GameObject getDebugObject(bool isUpper)
    {
        if (isUpper)
        {
            return debugUpperObject;
        }
        else
        {
            return debugLowerObject;
        }
    }

    void Awake()
    {
        if (!debug)
        {
            Debug.Log("Serial protocol revision: " + GetRevision());
            SetLoggingHandler(LogHandler);
            SetSyncHandler(SyncHandler);
            SetHeartbeatHandler(HeartbeatHandler);
            SetPositionHandler(PositionHandler);
            // should be discovered automatically
            Handle = OpenPort("//.//COM3");
        }
        else
        {
            CreateDebugObjects();
        }
    }

    private void CreateDebugObjects()
    {
        UnityEngine.Object prefab = Resources.Load("Handle");

        debugLowerObject = Instantiate(prefab) as GameObject;
        debugLowerObject.transform.GetChild(0).GetComponent<Renderer>().material = Resources.Load("Materials/DebugLower") as Material;
        debugLowerObject.transform.position = new Vector3(0f, 0.5f, 10.0f);

        debugUpperObject = Instantiate(prefab) as GameObject;
        debugUpperObject.transform.GetChild(0).GetComponent<Renderer>().material = Resources.Load("Materials/DebugUpper") as Material;
        debugUpperObject.transform.position = new Vector3(0f, 0.5f, 10.0f);
    }

    void OnDestroy()
    {
        Close(Handle);
    }

    void Update()
    {
        if (!debug)
        {
            Poll(Handle);
            //TODO Why is this here?
            //FreeHandle(false);
        }
        else
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float mouseRotation = Input.mouseScrollDelta.y * debugScrollSpeed;
            Vector3 position = new Vector3(mousePosition.x, 0.0f, mousePosition.z);
            upperHandleRot = debugUpperObject.transform.eulerAngles.y + mouseRotation;
            upperHandlePos = position;
            lowerHandleRot = debugLowerObject.transform.eulerAngles.y + mouseRotation;
            lowerHandlePos = position;
            updatePantoHandles();
        }

        if (Input.GetKeyDown(toggleVisionKey))
        {
            toggleBlindMode();
        }
    }

    private void toggleBlindMode()
    {
        if (!debug)
        {
            if (isBlindModeOn)
            {
                Camera.main.farClipPlane = 1;
            }
            else
            {
                Camera.main.farClipPlane = 1000;
            }
        }
        else
        {
            Light upperLight = debugUpperObject.transform.GetChild(1).GetComponent<Light>();
            Light lowerLight = debugLowerObject.transform.GetChild(1).GetComponent<Light>();
            upperLight.enabled = !upperLight.enabled;
            lowerLight.enabled = !lowerLight.enabled;
            if (!GetComponent<UpperHandle>().HasMeObject()) { upperLight.range = 9; }
            if (!GetComponent<LowerHandle>().HasMeObject()) { lowerLight.range = 9; }

            GameObject lights = GameObject.Find("Light");
            if (lights.GetComponent<Light>()) lights.GetComponent<Light>().enabled = !lights.GetComponent<Light>().enabled;
            foreach (Transform child in lights.transform)
            {
                child.GetComponent<Light>().enabled = !child.GetComponent<Light>().enabled;
            }
        }
        isBlindModeOn = !isBlindModeOn;
    }

    public void FreeHandle(bool isUpper)
    {
        if (!debug)
        {
            FreeMotor(Handle, (byte)0, isUpper ? (byte)0 : (byte)1);
        }
    }

    private Vector3 GetPositionWithObstacles(Vector3 currentPosition, Vector3 wantedPosition)
    {
        return wantedPosition;

    }

    public void UpdateHandlePosition(Vector3 position, float? rotation, bool isUpper)
    {
        Vector2 pantoPoint = unityToPanto(new Vector2(position.x, position.z));
        if (IsInBounds(pantoPoint))
        {
            if (debug)
            {
                GameObject debugObject = getDebugObject(isUpper);
                //TODO make it so position can be null
                if (position != null) debugObject.transform.position = GetPositionWithObstacles(debugObject.transform.position, (Vector3)position);
                if (rotation != null) debugObject.transform.eulerAngles = new Vector3(0, (float)rotation, 0);
                return;

            }
            else
            {
                //TODO Send Rotation
                float pantoRotation = 0;// UnityToPantoRotation(rotation);
                SendMotor(Handle, (byte)0, isUpper ? (byte)0 : (byte)1, pantoPoint.x, pantoPoint.y, pantoRotation);
            }
        }
        else
        {
            Debug.LogWarning("Position not in bounds: " + pantoPoint);
        }
    }

    public void SetDebugObjects(bool isUpper, Vector3? position, float? rotation)
    {
        GameObject debugObject = getDebugObject(isUpper);
        if (position != null) debugObject.transform.position = (Vector3)position;
        if (rotation != null) debugObject.transform.eulerAngles = new Vector3(0, (float)rotation, transform.eulerAngles.z);
    }

    private static float UnityToPantoRotation(float rotation)
    {
        Debug.Log("Rotation: " + rotation);
        return 0;
    }

    private static float PantoToUnityRotation(double pantoDegrees)
    {
        Debug.Log(pantoDegrees);
        return (float)((180f / Mathf.PI) * (-pantoDegrees));
    }

    private static Vector2 unityToPanto(Vector2 point)
    {
        float newX = (point.x - unityBounds[0].x) * pantoBounds[1].x / unityBounds[1].x + pantoBounds[0].x;
        float newY = (point.y - unityBounds[0].y) * pantoBounds[1].y / unityBounds[1].y + pantoBounds[0].y;
        return new Vector2(newX, newY);
    }
    private static Vector2 pantoToUnity(Vector2 point)
    {
        float newX = (point.x - pantoBounds[0].x) * unityBounds[1].x / pantoBounds[1].x + unityBounds[0].x;
        float newY = (point.y - pantoBounds[0].y) * unityBounds[1].y / pantoBounds[1].y + unityBounds[0].y;
        return new Vector2(newX, newY);
    }

    private void testConversion()
    {
        Assert.IsTrue(unityToPanto(new Vector2(25.0f, -25.0f)) == new Vector2(160.0f, -190.0f));
        Assert.IsTrue(unityToPanto(new Vector2(0.0f, 0.0f)) == new Vector2(0.0f, -110.0f));
        Assert.IsTrue(unityToPanto(new Vector2(0.0f, -25.0f)) == new Vector2(0.0f, -190.0f));

        Assert.IsTrue(new Vector2(25.0f, -25.0f) == pantoToUnity(new Vector2(160.0f, -190.0f)));
        Assert.IsTrue(new Vector2(0.0f, 0.0f) == pantoToUnity(new Vector2(0.0f, -110.0f)));
        Assert.IsTrue(new Vector2(0.0f, -25.0f) == pantoToUnity(new Vector2(0.0f, -190.0f)));
    }

    private bool IsInBounds(Vector2 point)
    {
        //should be betwwen -160 and 160
        bool hortCorrect = point.x >= (pantoBounds[0].x - pantoBounds[1].x * 0.5) && point.x <= (pantoBounds[0].x + pantoBounds[1].x * 0.5);
        //should be betwwen -30 and -190
        bool vertCorrect = point.y >= (pantoBounds[0].y - pantoBounds[1].y * 0.5) && point.y <= (pantoBounds[0].y + pantoBounds[1].y * 0.5);
        return hortCorrect && vertCorrect;
    }

    public void SetUnityBounds(Vector3 origin, Vector3 extent)
    {
        Debug.Log("Play Area setting bounds");
        unityBounds = new Vector2[] { new Vector2(origin.x, origin.z), new Vector2(extent.x, extent.z) };
    }

    public void registerHandle(PantoHandle newHandle)
    {
        registeredPantoHandles.Add(newHandle);
    }

    public void unregisterHandle(PantoHandle newHandle)
    {
        registeredPantoHandles.Remove(newHandle);
    }

    public ushort GetNextObstacleId()
    {
        return ++currentObstacleId;
    }

    public void CreateObstacle(byte pantoIndex, ushort obstacleId, Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 pantoStartPoint = unityToPanto(startPoint);
        Vector2 pantoEndPoint = unityToPanto(endPoint);
        CreateObstacle(Handle, pantoIndex, obstacleId, pantoStartPoint.x, pantoStartPoint.y, pantoEndPoint.x, pantoEndPoint.y);
    }

    public void AddToObstacle(byte pantoIndex, ushort obstacleId, Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 pantoStartPoint = unityToPanto(startPoint);
        Vector2 pantoEndPoint = unityToPanto(endPoint);
        AddToObstacle(Handle, pantoIndex, obstacleId, pantoStartPoint.x, pantoStartPoint.y, pantoEndPoint.x, pantoEndPoint.y);
    }

    public void EnableObstacle(byte pantoIndex, ushort obstacleId)
    {
        EnableObstacle(Handle, pantoIndex, obstacleId);
    }

    public void DisableObstacle(byte pantoIndex, ushort obstacleId)
    {
        DisableObstacle(Handle, pantoIndex, obstacleId);
    }

    public void RemoveObstacle(byte pantoIndex, ushort obstacleId)
    {
        RemoveObstacle(Handle, pantoIndex, obstacleId);
    }
}
