using UnityEngine;

public class WallWrap : MonoBehaviour
{
    public float nudgeAmount = 1.0f;

    void OnTriggerEnter(Collider other)
    {
        // Determine which wall this is based on the name
        if (Mathf.Abs(gameObject.transform.position.x) > 0)
        {
            TeleportObject(other.gameObject, "LR");
        }
        else if (Mathf.Abs(gameObject.transform.position.z) > 0)
        {
            TeleportObject(other.gameObject, "UD");
        }
        else Debug.Log("Could not determine direction");
    }

    void TeleportObject(GameObject obj, string direction)
    {
        if (direction == "LR")
        {
            //Debug.Log("LR " + obj.name);

            // Invert the X position and nudge towards the center
            float newX = obj.transform.position.x * -1 * 0.9f;
            obj.transform.position = new Vector3(newX, 0, obj.transform.position.z);
        }
        else if (direction == "UD")
        {
            //Debug.Log("UD " + obj.name);

            // Invert the Z position and nudge towards the center
            float newZ = (obj.transform.position.z * -1 * 0.9f);
            obj.transform.position = new Vector3(obj.transform.position.x, 0, newZ);
        }
    }
}
