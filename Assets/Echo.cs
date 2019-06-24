﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Net;
using System.Text;

public class Echo : MonoBehaviour
{
    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    //定义套接字
    Socket socket;
    //UGUI
    public InputField InputFeld;
    public Text text;
    //接收缓冲区
    byte[] readBuff = new byte[1024];
    ///已接收的消息体的总长度
    int buffCount = 0;
    //显示文字
    string recvStr = "";
    public void Connection()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        //为了精简代码：使用同步Connect
        //             不考虑抛出异常
        IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
        IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
        socket.Connect(ipEp);

        socket.BeginReceive(readBuff, buffCount, 1024 - buffCount, 0, ReceiveCallback, socket);
    }


    //点击连接按钮

    public void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //获取接收数据长度
            int count = socket.EndReceive(ar);
            buffCount += count;
            //解析接收的数据
            OnReceiveData();
            //继续接收数据
            socket.BeginReceive(readBuff, buffCount, 1024 - buffCount, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }
    public void OnReceiveData()
    {
        if (buffCount <= 2) return;
        //如果是完整的就去处理它
        UInt16 bodyLength = BitConverter.ToUInt16(readBuff, 0);
        //判断消息体长度
        if (buffCount < 2 + bodyLength)
        {
            if (buffCount == readBuff.Length)
            {
                //TODO:
                //即将接收的数据大于readBuff缓冲区 但每次最多只能接收和readBuff数组一样多的字节流的情况
                //这里添加一下代码就好了 然后提交到GitHub
                //嘿嘿 在是一次vs提交信息 edit by 本地库
                return;
            }
            return;
        }
        string s = System.Text.Encoding.UTF8.GetString(readBuff, 2, bodyLength);
        Debug.Log("[Rece 4]s=" + s);
        //更新缓冲区 把已经处理完的数据从缓冲区移除 未处理的从后面移到缓冲区的最前面
        //已经处理完的消息体长度 也是Copy方法从这个索引开始拷贝
        //int start = 2 + bodyLength;
        int start = buffCount;
        //在缓冲区剩余的未处理的消息的总长度  也是Copy方法要拷贝的字节长度
        int count = buffCount - start;
        Array.Copy(readBuff, start, readBuff, 0, count);
        buffCount -= start;
        //如果有更多消息就处理它
        recvStr = s + '\n' + recvStr;
        //继续读取消息
        OnReceiveData();
    }
    //点击发送按钮
    public void Send()
    {
        string sendStr = InputFeld.text;
        //组装协议
        byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
        UInt16 len = (UInt16)bodyBytes.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        //把bodyBytes接在lenBytes得后面并返回一个新得数组
        byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();
        int count = sendBytes.Length;
        socket.Send(sendBytes);
    }
    public void Update()
    {
        text.text = recvStr;
    }
}
