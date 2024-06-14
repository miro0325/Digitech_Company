using UnityEngine;

[ExecuteInEditMode]
public class InGamePlayerItemIKTester : MonoBehaviour
{
    [SerializeField] private ItemBase item;
    [Space(10)]
    [SerializeField] private InGamePlayerIKHandler bodyHandIK;
    [SerializeField] private InGamePlayerIKHandler camHandIK;

    private void Update()
    {
        if(item)
        {
            camHandIK?.SetHandIKTarget(item.LeftHandPoint, item.RightHandPoint);
            bodyHandIK?.SetHandIKTarget(item.LeftHandPoint, item.RightHandPoint);
        }
        else
        {
            camHandIK?.SetHandIKTarget(null, null);
            bodyHandIK?.SetHandIKTarget(null, null);
        }
    }
}