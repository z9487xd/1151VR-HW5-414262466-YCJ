using UnityEngine;
public class BallController : MonoBehaviour
{
    [HideInInspector] public CaptureGame game;
    void OnCollisionEnter(Collision collision)
    {
        if(!game)return;
        if(collision.collider.GetComponentInParent<CaptureTarget>())game.Hit(this);
        else game.Miss();
    }
}
