using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanning : MonoBehaviour
{

    public GameObject cam;
    Vector3 currentMousePos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                //if(hit.transform.name == "Plane")
                //{
                    cam.transform.RotateAround(Vector3.zero, Vector3.up, Input.mousePosition.x - currentMousePos.x);
                //}
            }
        }

        currentMousePos = Input.mousePosition;
    }
}
