using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour
{

    public Transform target;
    public float distance = 5.0f;
    float tempDis;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    public string inputCHrz = "Horizontal_c_p1";
    public string inputCVrt = "Vertical_c_p1";

    private Rigidbody rigidbody;

    float x = 0.0f;
    float y = 0.0f;

    // Use this for initialization
    void Start()
    {
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		inputCHrz += "_mac";
		inputCVrt += "_mac";
		#endif
		
        tempDis = distance;
        target.GetComponent<BoatController>().cam = gameObject;

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            x += Input.GetAxis(inputCHrz) * xSpeed * tempDis * 0.02f;
            y -= Input.GetAxis(inputCVrt) * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            //distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                y += 1f;
            }
            else
            {
                //Vector3 optPos = rotation * new Vector3(0.0f,0.0f,-distance) + target.position;
                //if (Physics.Linecast(target.position, optPos, out hit))
                //{
                //    tempDis = hit.distance;
                //}
                //else
                //{
                //    tempDis = distance;
                //}
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -tempDis);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}