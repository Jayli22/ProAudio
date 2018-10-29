using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveScript : MonoBehaviour {
    private Vector3 Direction;
    public float speed;
    public float destorytime;
    // Use this for initialization
    void Start () {
        if (transform.position.x >0)
        {
            if(transform.position.y > 0)
            {
                Direction.x = -1f;
                Direction.y = Random.Range(-1f, 0f);
            }
            if (transform.position.y < 0)
            {
                Direction.x = -1f;
                Direction.y = Random.Range(0f, 1f);
            }
        }
        else
        {
            if (transform.position.y > 0)
            {
                Direction.x = 1f;
                Direction.y = Random.Range(-1f, 0f);
            }
            if (transform.position.y < 0)
            {
                Direction.x = 1f;
                Direction.y = Random.Range(0f, 1f);
            }
        }


    }

    // Update is called once per frame
    void Update () {
        Move();
        if (Vector3.Distance(new Vector3(0, 0, 0), transform.position) > 5)
        {


            Destroy(gameObject);
        }
        }

    void Move()
    {
        transform.position += Direction * speed *Time.deltaTime;
    }


    private void OnTriggerEnter(Collider other)
    {

        if(other.name == "CharacterPlayer")
            Destroy(gameObject);
       
    }
}
