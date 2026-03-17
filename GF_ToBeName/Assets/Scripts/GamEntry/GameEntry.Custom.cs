using UnityEngine;

namespace NewSideGame
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        public static PoolManager PoolManager { get; private set; }

        public static EventManager EventManager { get; private set; }

        private static void InitCustomComponents()
        {
            PoolManager = UnityGameFramework.Runtime.GameEntry.GetComponent<PoolManager>();
            EventManager = UnityGameFramework.Runtime.GameEntry.GetComponent<EventManager>();
        }
    }
}