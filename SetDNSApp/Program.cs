using System;

namespace SetDNSApp
{
    class Program
    {
        private static readonly string[] _menus = new string[]{
            "菜单",
               "1.设置为翻墙DNS",
        "2.设置为ctg DNS",
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

                string inputString = Console.ReadLine();
                switch (inputString)
                {
                    case "1": SetNetwork.SetDnsConfig("208.67.222.222,208.67.220.220"); SetNetwork.showInfo(); break;
                    case "2": SetNetwork.SetDnsConfig("10.37.12.3,10.66.12.3"); SetNetwork.showInfo(); break;
                    default: loop = false; break;
                }
            }





        }
    }
}
