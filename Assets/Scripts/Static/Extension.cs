using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extension
{
    public static void MoveForward(this Transform trans, float direction, float speed)
    {
        trans.Translate(new Vector3(0,0,direction) * speed * Time.deltaTime);
    }
}
