using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoteOB : MonoBehaviour
{
    private GameObject player;

    private PlayerGunSelector gunSelector;

    public bool canBePress;

    public KeyCode keyTopress;

    private bool wasProcessed = false;

    public float perfectAdjust = 0.1f;

    public float hitPoint;

    public GameObject hitEffect, perfectEffect;

    Vector3 position = new Vector3(0f, 0f, 10f);


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if(player != null )
            gunSelector = player.GetComponent<PlayerGunSelector>();
    }

    // Update is called once per frame  
    void Update()
    {
        if (player != null) 
        {
            if (Input.GetKeyDown(keyTopress) && gunSelector.activeGun.canShoot)
            {
                if (canBePress && !wasProcessed)
                {
                    wasProcessed = true;  // 防止重複判定

                    if (transform.position.x >= hitPoint - perfectAdjust && transform.position.x <= hitPoint + perfectAdjust)
                    {
                        GameManager.instance.PerfectHit();
                        Instantiate(perfectEffect, position, Quaternion.identity);
                    }
                    else
                    {
                        GameManager.instance.NormalHit();
                        Instantiate(hitEffect, position, Quaternion.identity);
                    }
                    // ScoreManager.instance.changeMultiply(0.1f);

                    // ScoreManager.instance.AddScore(100);

                    Destroy(gameObject);

                    // GameManager.instance.NoteHit();
                }

            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Activator")
        {
            canBePress = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
         if (collision.tag == "Activator" && !wasProcessed)
        {
            wasProcessed = true;
            canBePress = false;

            // GameManager.instance.NoteMiss();
        }
    }
}
