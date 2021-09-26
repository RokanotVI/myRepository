using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();//Обычная очередь Queue<T>

    private void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)//Делегат Func делает работает также как Action, но может возращать не только void
    {
        ThreadStart threadStart = delegate //Анонимный метод для потока
        {
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        lock (dataQueue)//блокирует другие потоки когда это используется
        {
            dataQueue.Enqueue(new ThreadInfo (callback, data)); //Добавление элемента в конец очереди
        }
    }

    void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parametr);
            }
        }
    }

    struct ThreadInfo //Обобщенная структура, созднанна для использования Мешдаты и Мапдаты
    {
        public readonly Action<object> callback;//Делегат Action является обобщенным, принимает параметры и возвращает значение void
        public readonly object parametr;

        public ThreadInfo(Action<object> callback, object parametr)
        {
            this.callback = callback;
            this.parametr = parametr;
        }
    }
}
