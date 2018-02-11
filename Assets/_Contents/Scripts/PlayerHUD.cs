using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour {

    public GameObject Tips;

	// Use this for initialization
	void Start () {
        Tips.SetActive(false);

    }

    public void ShowTips()
    {
        Tips.SetActive(true);
    }

}
