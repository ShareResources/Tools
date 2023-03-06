
using System.Runtime.InteropServices;

namespace Network
{
    /// <summary>
    /// 网络状态
    /// </summary>
    public class NetWorkStatus
    {
        [DllImport("winInet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        /// <summary>
        /// 判断网络的链接状态
        /// </summary>
        /// <returns></returns>
        public static bool GetNetConnectedState()
        {
            int iNetStates = 0;
            System.Int32 dwFlag = new int();
            if (!InternetGetConnectedState(ref dwFlag, 0))
            {
                //链接网络失败
                iNetStates = 1;
            }
            return iNetStates == 0;
        }
    }
}


