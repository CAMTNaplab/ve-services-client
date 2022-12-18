using Colyseus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VEServicesClient
{
    public class ClientInstance : MonoBehaviour
    {
        public static ClientInstance Instance { get; private set; }

        public string address = "localhost:2567";
        public bool secured = false;
        public string secretKey = "secret";

        private ColyseusClient _client;
        public ColyseusClient Client
        {
            get
            {
                _client.Settings.useSecureProtocol = secured;
                return _client;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
