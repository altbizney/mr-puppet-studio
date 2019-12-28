using UnityEngine;

public class TiltVelocityDirection : MonoBehaviour
{
    //public float speed = 5f;
    public float tilt = 5f;

    public float SphereSize = 5f;
    public float Interval = 1f;
    public float SmoothTime = 0.1f;

    private Rigidbody rb;
    private Vector3 target, current, velocity;

    private void OnEnable()
    {
        InvokeRepeating("Jump", 0f, Interval);
        target = current = transform.position;

        rb = GetComponent<Rigidbody>();
    }

    private void OnDisable()
    {
        CancelInvoke("Jump");
    }

    void FixedUpdate()
    {
        //rb.velocity = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * speed;

        current = Vector3.SmoothDamp(current, target, ref velocity, SmoothTime);

        rb.MovePosition(current);
        rb.rotation = (Quaternion.Euler(rb.velocity.z * -tilt, 0f, rb.velocity.x * -tilt));
    }

    private void Jump()
    {
        target = Random.insideUnitSphere * SphereSize;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(Vector3.zero, SphereSize);
    }

}
