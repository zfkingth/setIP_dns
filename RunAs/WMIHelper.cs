using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace RunAs
{
    class WMIHelper
    {
        /*
         *一、通过WMI获取物理适配器序号
NetEnabled: 是否启用了适配器，True为启用，False为禁用;
PhysicalAdapter: 适配器是否物理或逻辑适配器,True为物理，False为逻辑;
         */
        public static List<int> GetUseIndex()
        {
            List<int> list = new List<int>();
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                bool p1 = Convert.ToBoolean(mo["NetEnabled"]);
                bool p2 = Convert.ToBoolean(mo["PhysicalAdapter"]);
                if (p1 && p2)
                {
                    list.Add(Convert.ToInt32((mo["Index"])));
                }
            }
            return list;
        }

        /*
         * 
         * 二、过滤虚拟网卡、无线网卡

通过“Characteristics”这个值来确定网卡的类型是虚拟网卡还是物理网卡。Characteristics 值在注册表在HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\【连接索引号】\下，在windows中，Characteristics 的取值如下，Characteristics项可以有1个或多个如下的值(多值应计算总和)：【备注：在Windows7和Windows10下确认，Characteristics为dword，不可能多个值，这里的多个值，计算总和暂时未知。】

0x1

NCF_VIRTUAL

说明组件是个虚拟适配器

0x2

NCF_SOFTWARE_ENUMERATED

说明组件是一个软件模拟的适配器

0x4

NCF_PHYSICAL

说明组件是一个物理适配器

0x8

NCF_HIDDEN

说明组件不显示用户接口

0x10

NCF_NO_SERVICE

说明组件没有相关的服务(设备驱动程序)

0x20

NCF_NOT_USER_REMOVABLE

说明不能被用户删除(例如，通过控制面板或设备管理器)

0x40

NCF_MULTIPORT_INSTANCED_ADAPTER

说明组件有多个端口，每个端口作为单独的设备安装。每个端口有自己的hw_id(组件ID) 并可被单独安装，这只适合于EISA适配器

0x80

NCF_HAS_UI

说明组件支持用户接口(例如，Advanced Page或Customer Properties Sheet)

0x400

NCF_FILTER

说明组件是一个过滤器

如果是物理网卡：Characteristics & NCF_PHYSICAL ==NCF_PHYSICAL

判断有线网卡和无线网卡，注册表路径：HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\【连接索引号】\Ndi\Interfaces

路径下的键值：LowerRange，如果Value包含wifi或者wlan，（验证的两台设备Value=“wlan,ethernet,vwifi”），表示无线网卡，具体代码如下：
         */

        public static bool GetWiredIndex(ref int index, ref string msg)
        {
            try
            {
                List<int> allenable = GetUseIndex();
                if (allenable.Count == 0)
                {
                    msg = "未查找到有效网络连接";
                    return false;
                }
                List<int> allReal = new List<int>();
                foreach (int ii in allenable)
                {
                    var vv = Convert.ToInt32(GetCharacteristics(@"SYSTEM\ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\" + ii.ToString("D4"), "Characteristics"));
                    if ((vv & 0x4) == 0x4)//区分物理网卡、虚拟网卡
                    {
                        allReal.Add(ii);
                    }
                }
                if (allReal.Count == 0)
                {
                    msg = "未查找到有效物理网卡";
                    return false;
                }

                int rv = -1;
                foreach (int ii in allReal)
                {
                    var vv = GetCharacteristics(@"SYSTEM\ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\" + ii.ToString("D4") + @"\Ndi\Interfaces", "LowerRange").ToString();
                    if (!vv.Contains("wifi") && !vv.Contains("wlan"))
                    {
                        rv = ii;
                        break;
                    }
                }
                if (rv == -1)
                {
                    msg = "未查找到有效有线网卡";
                    return false;
                }
                else
                {
                    index = rv;
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        private static object GetCharacteristics(string name, string key)
        {
            object registData;
            using (RegistryKey hkml = Registry.LocalMachine)
            {
                RegistryKey software = hkml.OpenSubKey(name, true);
                registData = software.GetValue(key).ToString();
            }
            return registData;
        }


        /*
         * 三、设置有线网卡IP、子网掩码、网关、DNS
         */

        public static bool SetWiredIP(int index, string ip, string subnetMask, string ipGateway, string dns)
        {

            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (Convert.ToInt32(mo["Index"]) == index)
                {
                    //设置ip地址和子网掩码
                    inPar = mo.GetMethodParameters("EnableStatic");
                    inPar["IPAddress"] = new string[] { ip };
                    inPar["SubnetMask"] = new string[] { subnetMask };
                    outPar = mo.InvokeMethod("EnableStatic", inPar, null);

                    //设置网关地址
                    inPar = mo.GetMethodParameters("SetGateways");
                    inPar["DefaultIPGateway"] = new string[] { ipGateway };
                    outPar = mo.InvokeMethod("SetGateways", inPar, null);

                    //设置DNS
                    inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    inPar["DNSServerSearchOrder"] = dns.Split(',');
                    outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                    return true;
                }
            }
            return false;
        }

        public static bool SetDHCP(int index)
        {

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (Convert.ToInt32(mo["Index"]) == index)
                {
                    mo.InvokeMethod("SetDNSServerSearchOrder", null);
                    mo.InvokeMethod("EnableStatic", null);
                    mo.InvokeMethod("SetGateways", null);
                    mo.InvokeMethod("EnableDHCP", null);
                    return true;
                }
            }
            return false;
        }

        public static bool showInfo(int index)
        {

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (Convert.ToInt32(mo["Index"]) == index)
                {
                    var result = (string[])mo["IPAddress"];
                    Console.WriteLine($"ip: {result[0]}");


                    result = (string[])mo["IPSubnet"];
                    Console.WriteLine($"子网掩码: {result[0]}");

                    result = (string[])mo["DefaultIPGateway"];
                    Console.WriteLine($"网关: {result[0]}");

                    result = (string[])mo["DNSServerSearchOrder"];
                    foreach (var item in result)
                        Console.WriteLine($"dns: {item}");


                    return true;
                }
            }
            return false;
        }
    }
}
