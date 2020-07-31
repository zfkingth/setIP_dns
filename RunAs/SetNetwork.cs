using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace SetDNSApp
{
    class SetNetwork
    {
        public static void setIP(string ip_address, string subnet_mask)
        {
            ManagementClass objMC =
              new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    ManagementBaseObject setIP;
                    ManagementBaseObject newIP =
                      objMO.GetMethodParameters("EnableStatic");

                    newIP["IPAddress"] = new string[] { ip_address };
                    newIP["SubnetMask"] = new string[] { subnet_mask };

                    setIP = objMO.InvokeMethod("EnableStatic", newIP, null);
                }
            }
        }

        public static void setGateway(string gateway)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    ManagementBaseObject setGateway;
                    ManagementBaseObject newGateway =
                      objMO.GetMethodParameters("SetGateways");

                    newGateway["DefaultIPGateway"] = new string[] { gateway };
                    newGateway["GatewayCostMetric"] = new int[] { 1 };

                    setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);
                }
            }
        }

        internal static void setAuto()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (!(bool)mo["IPEnabled"]) continue;
                mo.InvokeMethod("SetDNSServerSearchOrder", null);
                mo.InvokeMethod("EnableStatic", null);
                mo.InvokeMethod("SetGateways", null);
                mo.InvokeMethod("EnableDHCP", null);
                break;
            }
        }

        public static void setDNS(string NIC, string DNS)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    // if you are using the System.Net.NetworkInformation.NetworkInterface
                    // you'll need to change this line to
                    // if (objMO["Caption"].ToString().Contains(NIC))
                    // and pass in the Description property instead of the name 
                    if (objMO["Caption"].Equals(NIC))
                    {
                        ManagementBaseObject newDNS =
                          objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        newDNS["DNSServerSearchOrder"] = DNS.Split(',');
                        ManagementBaseObject setDNS =
                          objMO.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                    }
                }
            }
        }


        public static void SetDnsConfig(string dsnString)
        {


            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");


            ManagementObjectCollection moc = mc.GetInstances();


            string nic = string.Empty;


            foreach (ManagementObject mo in moc)
            {

                if ((bool)mo["ipEnabled"])
                {
                    nic = mo["Caption"].ToString();
                    if ((bool)mo["IPEnabled"])
                    {
                        if (mo["Caption"].Equals(nic))
                        {
                            ManagementBaseObject dnsEntry = mo.GetMethodParameters("SetDNSServerSearchOrder");



                            dnsEntry["DNSServerSearchOrder"] = dsnString.Split(',');//Two ip addresses you want to set         


                            ManagementBaseObject dnsMbo = mo.InvokeMethod("SetDNSServerSearchOrder", dnsEntry, null);

                            int returnCode = int.Parse(dnsMbo["returnvalue"].ToString(), CultureInfo.InvariantCulture);//This will give you the return code you can use to evaluate if its not working  

                            break;


                        }


                    }


                }

            }
        }



        internal static void showInfo()
        {
            //获取本地连接ip 掩码 网关 DNS
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface bendi in interfaces)
            {
                //if (bendi.Name.ToString().Equals("本地连接") && bendi.NetworkInterfaceType.ToString().Equals("Ethernet"))
                if (bendi.Speed != -1)
                {
                    Console.WriteLine(bendi.Name);

                    IPInterfaceProperties ip = bendi.GetIPProperties();
                    //获取Ip 掩码
                    for (int i = 0; i < ip.UnicastAddresses.Count; i++)
                    {
                        //不插网线会得到一个保留地址 169.254.126.164
                        if (ip.UnicastAddresses[i].Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (ip.UnicastAddresses[i].Address != null)
                                Console.WriteLine(ip.UnicastAddresses[i].Address.ToString());
                            //如果不插网线 获取不了掩码 返回null 
                            if (ip.UnicastAddresses[i].IPv4Mask != null)
                                Console.WriteLine(ip.UnicastAddresses[i].IPv4Mask.ToString());
                        }
                    }
                    //获取网关
                    if (ip.GatewayAddresses.Count > 0)
                        Console.WriteLine(ip.GatewayAddresses[0].Address.ToString());
                    //获取DNS     
                    //不要DnsAddresses[0].Address.ToString() 不正确 还有警告  “System.Net.IPAddress.Address”已过时:  
                    if (ip.DnsAddresses.Count > 0)
                        Console.WriteLine(ip.DnsAddresses[0].ToString());
                    //备用DNS
                    if (ip.DnsAddresses.Count > 1)
                        Console.WriteLine(ip.DnsAddresses[1].ToString());
                    Console.WriteLine();


                }

            }
        }
    }
}
