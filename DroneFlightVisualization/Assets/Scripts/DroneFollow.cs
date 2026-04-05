using UnityEngine;

public class DroneFollow : MonoBehaviour
{
    public Transform Target;

    void Start()
    {
        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    }

    void LateUpdate()
    {
        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    }
}
