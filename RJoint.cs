using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RJoint : MonoBehaviour
{

    public float speed = 0.1f; //speed of joint rotation
    private const float threshold = 0.1f;

    public bool draw; //whether to draw gizmos

    public int limit_upper = 180;
    public int limit_lower = 0;
    public int zero_pos; //zero position in degrees
    public Quaternion initial_rotation;

    public int pwmID;
    public float position; //unused, will store servo angle value
    private int setPoint; //setpoint angle
    public bool onTarget;

    public enum JointMode { Setpoint, Free };
    public JointMode mode;

    public enum ServoType { M_180, M_270};
    public ServoType motor_type;

    //info about connections
    public RJoint nextJoint;
    public RJoint prevJoint;


    public GameObject origin_set_obj; //object for setting origin of joint rotation

    public bool changed; //has value been changed in software for arduino?
    public List<Transform> children; 

    // Start is called before the first frame update
    void Start()
    {
        //set rotation points based on connection points
        initial_rotation = transform.rotation;
        position = zero_pos;
        setPoint = (int)position;

        //add all transform children to children and to list
        for (int count = 0; count < transform.childCount; count++) {
            children.Add(transform.GetChild(count));
        }
        foreach (Transform child in children)
        {
            if(child.parent != transform)
            {
                child.parent = transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //show joint position objects if draw is true
        origin_set_obj.SetActive(draw);

        //continuous call of rotate when in setpoint mode
        if(mode == JointMode.Setpoint)
        {
            rotate(setPoint);
        }
    }


    /// <summary>
    /// freely rotate in a direction
    /// </summary>
    /// <param name="direction"></param>
    public void rotate(bool direction)
    {
        if (direction && position < limit_upper || !direction && position > limit_lower)
        {
            onTarget = false;

            float _speed = direction ? speed : -speed;
            position += _speed;
            transform.RotateAround(origin_set_obj.transform.position, origin_set_obj.transform.up, _speed);

        }
    }

    public int getSetPoint()
    {
        return setPoint;
    }

    public void setSetPoint(int point)
    {
        if (point > limit_upper)
            point = limit_upper;
        if (point < limit_lower)
            point = limit_lower;

        setPoint = point;
    }



    /// <summary>
    /// Rotate to reach a setpoint
    /// </summary>
    /// <param name="_setpoint"></param>
    public void rotate(int _setpoint)
    {

        setSetPoint(_setpoint);
        mode = JointMode.Setpoint;

        bool direction = position < setPoint ? true : false;

        //dead zone
        if (Mathf.Abs(position - setPoint) < threshold)
        {
            onTarget = true;
            return;
        }
        else
        {
            onTarget = false;
        }

        float _speed = direction ? speed : -speed;
        //_speed = speed * (-(1 / (1 + 10*Mathf.Abs(position - setPoint))) + 1); //equation to zero out speed when close to 0


        position += _speed;

        transform.RotateAround(origin_set_obj.transform.position, origin_set_obj.transform.up, _speed);
    }

    /// <summary>
    /// Adds a new object to this joint
    /// </summary>
    /// <param name="t"></param>
    public void attachPart(Transform t)
    {
        t.parent = transform;
        children.Add(transform);
    }
}
