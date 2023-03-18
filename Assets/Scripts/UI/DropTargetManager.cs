using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public class DropTargetManager : MonoBehaviour
    {
        private static DropTargetManager instance;

        public static DropTargetManager Instance
        {
            get { return instance; }
        }

        private void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            this.gameObject.SetActive(false);
        }
    }
}