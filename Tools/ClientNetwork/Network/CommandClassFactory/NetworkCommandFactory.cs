using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nullspace
{

    public class NetworkCommandFactory
    {
        private static Dictionary<int, Type> mAllCommandClasses = new Dictionary<int, Type>();
        private static bool isRegistered = false;

        public static void RegisterCommand()
        {
            if (!isRegistered)
            {
                mAllCommandClasses.Add(NetworkCommandType.HeartCodec, typeof(HeartCommand));
                mAllCommandClasses.Add(NetworkCommandType.GM, typeof(GMCommand));
                isRegistered = true;
            }
        }
        public static void RegisterCommand(string spacename, Assembly ass)
        {
            if (!isRegistered)
            {
                var types = ass.GetTypes();
                foreach (var item in types)
                {
                    if (item.Namespace == spacename)
                    {
                        var type = item.BaseType;
                        while (type != null)
                        {
                            if (type == typeof(NetworkCommand))
                            {
                                NetworkCommandTypeAttributeAttribute attr = NetworkCommandTypeAttributeAttribute.GetCustomAttribute(item, typeof(NetworkCommandTypeAttributeAttribute), false) as NetworkCommandTypeAttributeAttribute;
                                if (!mAllCommandClasses.ContainsKey(attr.Id))
                                {
                                    mAllCommandClasses.Add(attr.Id, item);
                                }
                                break;
                            }
                            else
                            {
                                type = type.BaseType;
                            }
                        }
                    }
                }
                isRegistered = true;
            }
        }

        public static NetworkCommand GetCommand(int mid)
        {
            if (mAllCommandClasses.ContainsKey(mid))
            {
                return (NetworkCommand)Activator.CreateInstance(mAllCommandClasses[mid]);
            }
            return null;
        }
    }

}
