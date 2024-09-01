using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public GameObject healthBarPrefab; // Assign the health bar prefab in the inspector
    public Transform canvasHUD; // Assign your HUD canvas in the inspector

    public void CreateHealthBarForObject(GameObject targetObject)
    {
        IDamageableKarl damageInterface = targetObject.GetComponent<IDamageableKarl>();

        if (damageInterface != null)
        {
            // Instantiate the health bar under the CanvasHUD
            GameObject prefabInstance = Instantiate(healthBarPrefab, canvasHUD);

            // Set the target and offset for the health bar
            HealthBarScript scriptInstance = prefabInstance.GetComponent<HealthBarScript>();
            scriptInstance.targetToFollow = targetObject;            
        }
    }
}
