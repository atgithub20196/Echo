using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Async : MonoBehaviour {


	// Use this for initialization
	void Start () {
		//创建定时器
		Timer timer = new Timer(TimeOut, null, 5000, 0);
	}
	
	//回调函数
	private void TimeOut(System.Object state){
		Debug.Log("铃铃铃");
	}
}
