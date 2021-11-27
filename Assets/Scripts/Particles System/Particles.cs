using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    [SerializeField] GameObject followAt;
    [SerializeField] float yOffset;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(followAt.transform.position.x, followAt.transform.position.y + yOffset, followAt.transform.position.z);
    }
}
