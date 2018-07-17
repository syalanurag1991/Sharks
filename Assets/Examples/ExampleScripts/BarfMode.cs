using UnityEngine;

public class BarfMode : MonoBehaviour
{
    private bool _go = false;

    private Vector3 _startingPosition;
    private Quaternion _startingQuaternion;

    void Start()
    {
        _startingPosition = transform.position;
        _startingQuaternion = transform.rotation;
    }
    
	void Update ()
	{
	    if (Input.GetKeyUp(KeyCode.B))
	    {
	        _go = !_go;
	    }

	    if (Input.GetKeyUp(KeyCode.R))
	    {
	        transform.position = _startingPosition;
	        transform.rotation = _startingQuaternion;
	    }

	    if (_go)
	    {
	        transform.position += (0.1f * Time.deltaTime * new Vector3(1, 1, 1));

	        transform.rotation *= Quaternion.Euler(Time.deltaTime * 2.0f, Time.deltaTime * 2.0f, 0.0f);
	    }
	}
}
