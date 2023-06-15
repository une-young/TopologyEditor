using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTextBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        LineRenderer  lineRenderer = this.transform.parent.gameObject.GetComponent<LineRenderer>();

        float distance = (lineRenderer.GetPosition(0) - lineRenderer.GetPosition(1)).magnitude;

        TextMesh text =gameObject.GetComponent<TextMesh>();

        text.text = distance.ToString("0.0") + "m";        

        Vector3 center = (lineRenderer.GetPosition(0) + lineRenderer.GetPosition(1)) / 2.0f;

        transform.position = center + new Vector3(0.0f,0.2f,0.0f); //도형 앞으로 나와서 가리지 않게
	}
}
