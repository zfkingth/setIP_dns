using SetDNSApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunAs
{
    class Program
    {
        private static readonly string[] _menus = new string[]{
                "菜单",
                "0.列出所有配置",
                "1.设置为阿里DNS",
                "2.设置为ctg DNS",
                "3.设置为翻墙DNS",
                "4.设置为有线网卡为固定IP(需要插上网线)",
                "5.设置为有线网卡DHCP（需要插上网线）",
                "请输入菜单编号："
    };

        static void Main(string[] args)
        {
            bool loop = true;


            while (loop)
            {
                Console.WriteLine();
                foreach (var menu in _menus)
                {
                    Console.WriteLine(menu);
                }
                int index = -1;
                string msg = "";

                string inputString = Console.ReadLine();
                switch (inputString)
                {
                    case "0": SetNetwork.showInfo(); break;
                    case "1": SetNetwork.SetDnsConfig("223.5.5.5,223.6.6.6"); SetNetwork.showInfo(); break;
                    case "2": SetNetwork.SetDnsConfig("10.37.12.3,10.66.12.3"); SetNetwork.showInfo(); break;
                    case "3": SetNetwork.SetDnsConfig("208.67.222.222,208.67.220.220"); SetNetwork.showInfo(); break;
                    case "4":

                        WMIHelper.GetWiredIndex(ref index, ref msg);
                        if (index != -1)
                        {
                            //找到了物理网上
                            WMIHelper.SetWiredIP(index, "10.37.85.71", "255.255.255.0", "10.37.85.1", "10.37.12.3,10.66.12.3");
                        }
                        WMIHelper.showInfo(index);
                        break;
                    case "5":
                        WMIHelper.GetWiredIndex(ref index, ref msg);
                        if (index != -1)
                        {
                            //找到了物理网上
                            WMIHelper.SetDHCP(index);
                        }
                        WMIHelper.showInfo(index);
                        break;
                    default: loop = false; break;
                }
            }





        }
    }
}
