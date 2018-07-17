using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    bool _go = false;

	void Start ()
    {
        _go = true;
	}

    void Update ()
    {
        if (_go)
        {
            transform.Rotate(Vector3.up * 10 * Time.deltaTime);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _go = !_go;
        }
    }
}
