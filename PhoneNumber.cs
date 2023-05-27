using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone
{
    public class PhoneNumber
    {
        public string Number;
        public ulong SteamID;
        public uint Balance;

        public PhoneNumber(string number, ulong steamID, uint balance)
        {
            Number = number;
            SteamID = steamID;
            Balance = balance;
        }
    }
}
