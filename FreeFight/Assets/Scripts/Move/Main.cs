using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    // for human model prefab
    public GameObject humanPrefab;
    // human list
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans = 
        new Dictionary<string, BaseHuman>();
    // switch to control when to show the offline information 
    // private bool isShow = false;
    private bool isGamePromtInfo = false;
    internal static bool isNetworkPromtInfo = false;

    // Start is called before the first frame update
    void Start()
    {
        // network module
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("List", OnList);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.AddListener("Attack", OnAttack);
        NetManager.AddListener("Hit", OnHit);
        NetManager.AddListener("Die", OnDie);

        // called only once, so produce only one client ID
        // NetManager.Connect("192.168.0.103", 8888);
        try
        {
            // NetManager.Connect("239o4651o8.wicp.vip", 27300);
            NetManager.Connect("127.0.0.1", 8888);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive Fail: " + ex.ToString());
            Debug.Log("Jump Scene");
            ClearSceneData.LoadSceneClear("EndGame");
        }

        // Add a charactor
        GameObject obj = (GameObject)Instantiate(humanPrefab);
        float x = Random.Range(0, 20);
        float z = Random.Range(0, 20);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        obj.name = "EthanCtr";
        // asynchronous BeginConnect in NetManager.Connect will cause an emtpy 
        // string returned by NetManager.GetDesc when the socket is not yet 
        // completely created, use while to wait for a sucessful connection
        myHuman.description = NetManager.GetDesc();

        // Send Enter protocol
        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        string sendStr = "Enter|";
        sendStr += NetManager.GetDesc() + ",";
        sendStr += pos.x + ",";
        sendStr += pos.y + ",";
        sendStr += pos.z + ",";
        // = is used to separate each complete procotol within a sending bytes 
        sendStr += eul.y + "="; 
        NetManager.Send(sendStr);
        // Send player List protocol
        NetManager.Send("List|=");
        InvincibleState(myHuman.gameObject);
    }

    private void InvincibleState(GameObject human)
    {
        Debug.Log("Enter an Invincivle State");
        human.GetComponent<Collider>().enabled = false;
        StartCoroutine("StartInvincibleState", human);
    }

    private IEnumerator StartInvincibleState(GameObject human)
    {
        SkinnedMeshRenderer render =
            human.transform.Find("EthanBody").GetComponent<SkinnedMeshRenderer>();
        float totalTime = 3.0f;
        float changingTime = 0;
        bool isDisplay = true;

        while (totalTime > 0.0f)
        {
            totalTime -= Time.deltaTime;

            changingTime += Time.deltaTime;
            if (changingTime >= 0.08f)
            {
                if (isDisplay)
                {
                    render.enabled = false;
                    isDisplay = false;
                    changingTime = 0;
                }
                else
                {
                    render.enabled = true;
                    isDisplay = true;
                    changingTime = 0;
                }
            }
            yield return null;
        }
        Debug.Log("Invincivle State Over");
        render.enabled = true;
        human.GetComponent<Collider>().enabled = true;
    }

    private void OnGUI()
    {
        if (isGamePromtInfo)
        {
            GUI.skin.label.fontSize = 50;
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 400, 200),
                "Game Over!");
        }
        if (isNetworkPromtInfo)
        {
            GUI.skin.label.fontSize = 30;
            GUI.Label(new Rect(10, 10, 1000, 200),
                "Unable to communicate with server, you are in offline mode!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }

    private void OnEnter(string msgArgs)
    {
        Debug.Log("OnEnter " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');
        if (split.Length < 5) return;
        
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        float euY = float.Parse(split[4]);
        // eliminate the scenario that the message is sent by its own client
        if (desc == NetManager.GetDesc()) return;
        // Add a synchronized character 
        GameObject obj = (GameObject)Instantiate(humanPrefab);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, euY, 0);
        obj.name = "Ethansync";
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.description = desc;
        otherHumans.Add(desc, h);
        InvincibleState(h.gameObject);
    }
    private void OnList(string msgArgs)
    {
        Debug.Log("OnList " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');

        int count = split.Length / 7;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i * 7 + 0];
            float x = float.Parse(split[i * 7 + 1]);
            float y = float.Parse(split[i * 7 + 2]);
            float z = float.Parse(split[i * 7 + 3]);
            float eulY = float.Parse(split[i * 7 + 4]);
            int HP = int.Parse(split[i * 7 + 5]);
            string isDead = split[i * 7 + 6];
            // eliminate the scenario that the message is sent by its own client
            // and the character in its client is dead
            if (desc == NetManager.GetDesc() || isDead.ToUpper() == "YES") continue;
            // Add a synchronized character 
            GameObject obj = (GameObject)Instantiate(humanPrefab);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, eulY, 0);
            obj.name = "Ethansync";
            obj.transform.Find("Canvas/HP1F").GetComponent<Slider>().value = HP / 100.0f;
            obj.transform.Find("Canvas/HP2F").GetComponent<Slider>().value = HP / 100.0f;
            obj.transform.Find("Canvas/Text2").GetComponent<Text>().text = HP + " /100";
            BaseHuman h = obj.AddComponent<SyncHuman>();
            h.description = desc;
            h.currentHP = HP;
            h.preHP = HP;
            otherHumans.Add(desc, h);
        }
    }

    private void OnMove(string msgArgs)
    {
        Debug.Log("OnMove " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');
        if (split.Length < 5) return;

        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        // Move
        if (!otherHumans.ContainsKey(desc)) return;
        BaseHuman h = otherHumans[desc];
        Vector3 targetPos = new Vector3(x, y, z);
        h.MoveTo(targetPos);
    }

    private void OnLeave(string msgArgs)
    {
        Debug.Log("OnLeave " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');
        if (split.Length < 1) return;

        string desc = split[0];
        // remove the client charactor
        if (!otherHumans.ContainsKey(desc)) return;
        BaseHuman h = otherHumans[desc];
        Destroy(h.gameObject);
        otherHumans.Remove(desc);
    }

    private void OnAttack(string msgArgs)
    {
        Debug.Log("OnAttack " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');
        if (split.Length < 2) return;

        string desc = split[0];
        float eulY = float.Parse(split[1]);
        // synchronize attacking action
        if (!otherHumans.ContainsKey(desc))
            return;
        SyncHuman h = (SyncHuman)otherHumans[desc];
        h.SyncAttack(eulY);
    }

    private void OnHit(string msgArgs)
    {
        Debug.Log("OnHit " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');
        if (split.Length < 4) return;

        string attackDesc = split[0];
        string hitDesc = split[1];
        float attackAngleY = float.Parse(split[2]);
        int currentHP = int.Parse(split[3]);

        // I've been punched 
        if (hitDesc == NetManager.GetDesc())
        {
            // true represents a DamageHP effect will be performed
            myHuman.SetHP(currentHP, true, attackAngleY);
        }
        // Synchronous character's been hit and the attacker is myHuman itself
        else if (otherHumans.ContainsKey(hitDesc) && 
            attackDesc == NetManager.GetDesc())
        {
            SyncHuman h = (SyncHuman)otherHumans[hitDesc];
            h.SetHP(currentHP, true, attackAngleY);
        }
        // Synchronous character's been hit and the attacker is also a synchronous character 
        else if (otherHumans.ContainsKey(hitDesc) &&
           attackDesc != NetManager.GetDesc())
        {
            SyncHuman h = (SyncHuman)otherHumans[hitDesc];
            // false represents no DamageHP effect will be performed
            h.SetHP(currentHP, false, 0);
        }
        else return;
    }

    private void OnDie(string msgArgs)
    {
        Debug.Log("OnDie " + msgArgs);
        // parameter analysis
        if (msgArgs == null || msgArgs == "") return;
        string[] split = msgArgs.Split(',');
        if (split.Length < 4) return;

        string attackDesc = split[0];
        string hitDesc = split[1];
        float attackAngleY = float.Parse(split[2]);
        int currentHP = int.Parse(split[3]);
        // I'm dead 
        if (hitDesc == NetManager.GetDesc())
        {
            if (myHuman.isDead)
            {
                Debug.Log("Already Die!");
                return;
            }
            myHuman.SetHP(currentHP, true, attackAngleY);
            Debug.Log("Game Over!");
            isGamePromtInfo = true; // used for OnGUI() displaying game over
            myHuman.isDead = true;
        }
        // Synchronous character's dead and the attacker is myHuman itself
        else if (otherHumans.ContainsKey(hitDesc) &&
            attackDesc == NetManager.GetDesc())
        {
            SyncHuman h = (SyncHuman) otherHumans[hitDesc];
            if (h.isDead)
            {
                Debug.Log("Already Die!");
                return;
            }
            h.SetHP(currentHP, true, attackAngleY);
            h.isDead = true;
        }
        // Synchronous character's been hit and the attacker is also a synchronous character 
        else if (otherHumans.ContainsKey(hitDesc) &&
           attackDesc != NetManager.GetDesc())
        {
            SyncHuman h = (SyncHuman)otherHumans[hitDesc];
            if (h.isDead)
            {
                Debug.Log("Already Die!");
                return;
            }
            h.SetHP(currentHP, false, 0);
            h.isDead = true;
        }
        else return;
    }
}
