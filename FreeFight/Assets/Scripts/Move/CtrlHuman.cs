using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    // MouseEffect prefab
    private GameObject mouseEffect;

    // Use this for initialization
    new void Start()
    {
        base.Start();
        mouseEffect = (GameObject)Resources.Load("Prefabs/MouseEffect");
        AddAnimationEvent();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0))
        {
            if (isAttacking) return;
            // Returns a ray going from camera through a screen point.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // used to get information back from a raycast
            RaycastHit hit;

            // return true when the ray intersects any collider, otherwise false.
            // out hit will obtain the hit point when returning true
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Terrain"))
            {
                // keep our character within the terrain
                if ((1 <= hit.point.x && hit.point.x <= 499) &&
                    (1 <= hit.point.z && hit.point.z <= 499))
                {
                    // destroy the previous MouseEffects if there is any
                    Destroy(GameObject.FindGameObjectWithTag("MouseEffect"));
                    // The impact point in world space where the ray hit the collider.
                    MoveTo(hit.point);
                    // make the effect when clicking the mouse
                    // entrust the task to destroying go to go's own script
                    GameObject go = Instantiate(mouseEffect);
                    go.transform.position = hit.point;

                    // Send Move protocol (character's position information)
                    float xD = hit.point.x - this.transform.position.x;
                    float zD = hit.point.z - this.transform.position.z;
                    Vector3 direction = new Vector3(xD, 0, zD);
                    float angleY = Vector3.Angle(Vector3.forward, direction);
                    string sendStr = "Move|";
                    sendStr += NetManager.GetDesc() + ",";
                    sendStr += hit.point.x + ",";
                    sendStr += hit.point.y + ",";
                    sendStr += hit.point.z + ",";
                    sendStr += angleY + "=";
                    NetManager.Send(sendStr);
                }
            }
        }

        // prevent attack animation frames from getting stuck
        if (base.DisableAttackOnce)
        {
            base.DisableAttackOnce = false;
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking) return;
            if (isMoving) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && 
                (hit.collider.CompareTag("Terrain") || hit.collider.name == "Ethansync"))
            {              
                transform.LookAt(hit.point);
                // keep the character x and y axis rotation unchanged
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                Attack();

                // send attack protocol 
                string sendStr = "Attack|";
                sendStr += NetManager.GetDesc() + ",";
                sendStr += transform.eulerAngles.y + "=";
                NetManager.Send(sendStr);
            }
        }
    }

    private void AddAnimationEvent()
    {
        //获取动画组件中所有动画
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            //根据动画名字  找到你要添加的动画
            if (string.Equals(clips[i].name, "Attack"))
            {
                //添加动画事件
                AnimationEvent events = new AnimationEvent();
              
                //添加事件  不带参数
                events.functionName = "AttackDetermine";
                events.time = 0.2f;
                clips[i].AddEvent(events);
                break;
            }
        }
        animator.Rebind();
    }

    public void AttackDetermine()
    {
        // static function RaycastAll (origin : Vector3, direction : Vector3, distance : float = Mathf.Infinity, layermask : int = kDefaultRaycastLayers)
        // RaycastHit[]

        Vector3 lineStart = transform.position + 2.5f * Vector3.up;
        RaycastHit[] hits = Physics.RaycastAll(lineStart, transform.forward, 10);
        foreach (RaycastHit hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj == this.gameObject) continue;

            SyncHuman h = hitObj.GetComponent<SyncHuman>();
            if (h == null)
                return;
            string sendStr = "Hit|";
            sendStr += NetManager.GetDesc() + ",";
            sendStr += h.description + ",";
            sendStr += transform.eulerAngles.y + "=";
            NetManager.Send(sendStr);
        }


        //RaycastHit hit1;
        //Vector3 lineStart = transform.position + 2.5f * Vector3.up;
        //Vector3 lineEnd = lineStart + 8 * transform.forward;

        //if (Physics.Linecast(lineStart, lineEnd, out hit1))
        //{
        //    GameObject hitObj = hit1.collider.gameObject;
        //    if (hitObj == gameObject)
        //        return;
        //    SyncHuman h = hitObj.GetComponent<SyncHuman>();
        //    if (h == null)
        //        return;
        //    string sendStr = "Hit|";
        //    sendStr += NetManager.GetDesc() + ",";
        //    sendStr += h.description + ",";
        //    NetManager.Send(sendStr);
        //}
    }
}
