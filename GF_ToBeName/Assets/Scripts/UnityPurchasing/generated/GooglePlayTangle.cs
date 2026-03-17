// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("nUGlcSQelDk5AkSMUxXmkN/U4IJY6mlKWGVuYULuIO6fZWlpaW1oayjJ0oFCin8hVxStz2uTX/gP30GTeq3EYdK788sNnm2gwPNSIQjg+wZo6XZLwnw2ElHnyf+0HGufENRUyyYoWo1Mhnibt5AXD7rG8OLrfvmFSTFkojdUjiSBY0CoSoZw+bOEhtmV+YxhRGMdHQgr3twJzkMZDvdDL7PWNAdNc31e29ClfZefk3peXB3pdfd37BIDQpZj8fsMFIeQbmdHoVDqaWdoWOppYmrqaWlo00xoy9QUqRT5aR7fQRxrbw4xRCkY8iJft8/DN64eqm8FjkBkWMWnFQEZfyn5DUYhe9gHpRXv4hakR7Q7qW24weCrw+V1lxTDSK0XbWpraWhp");
        private static int[] order = new int[] { 11,11,9,12,4,5,9,9,9,13,11,12,13,13,14 };
        private static int key = 104;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
