using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMoveScript : MonoBehaviour {
    public float speed;
    public int hp=3;
    public Text hptext;
    public Canvas losecanvas;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Move();
        hptext.text = "HP:" + hp;

    }
    //Moveing
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");


        //restrict moving area
        //6.4,4.8
        Vector3 ps = transform.position;
        if( Vector3.Distance(new Vector3(0,0,0), (transform.position + new Vector3(x, y, 0) * speed * Time.deltaTime ))< 4.8)
            transform.position += new Vector3(x, y, 0) * speed * Time.deltaTime;

        //ps.x = Mathf.Clamp(ps.x, -6.4f, 6.4f);
        //ps.y = Mathf.Clamp(ps.y, -4.8f, 4.8f);
        //transform.position = ps;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        hp--;
        if(hp<1)
        {
            Destroy(gameObject);
            losecanvas.enabled = true;
        }
    }
}
