using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;

public class Movement : NetworkBehaviour
{

    [SerializeField] private Vector3 movement = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Client]
    // Update is called once per frame
    void Update()
    {
        if(!hasAuthority)
        {
            return;
        }
        
        if(!Input.GetKeyDown(KeyCode.UpArrow)) { return; }

        transform.Translate(movement);
        
    }
}
