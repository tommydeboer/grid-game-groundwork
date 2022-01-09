using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    Player player;

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + new Vector3(0, 13, -10);
    }
}