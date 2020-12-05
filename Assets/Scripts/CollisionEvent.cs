using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 衝突判定を行いたいオブジェクトにこのスクリプトをAddComponentする
public class CollisionEvent : MonoBehaviour
{
    public Primitive Primitive { get; set; }

    private void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == Ball.BALL_NAME)
            Primitive.Parent.Collided = true;
    }
}
