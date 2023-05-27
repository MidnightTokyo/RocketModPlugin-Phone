using Newtonsoft.Json;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.GraphicsBuffer;

namespace Phone
{
    internal static class PhoneManager
    {
        private static Thread SaveThreadWorker;
        private static readonly string NumbersPath = Utils.SavingPath("database.json");
        private static List<PhoneNumber> Numbers = new List<PhoneNumber>();
        private static ulong LastNumber = 0;

        public static void Initialization()
        {
            LoadBase();
            CheckMaxNumberInBase();
            SaveThreadWorker = new Thread(() =>
            {
                for (; ; )
                {
                    Thread.Sleep(300000);
                    SaveBase();
                }
            });
            SaveThreadWorker.Start();
        }

        public static void Finalizing()
        {
            SaveThreadWorker.Abort();
            SaveThreadWorker = null;
            SaveBase();
        }

        public static void SendMessage(UnturnedPlayer user, string number, string message)
        {
            if (user.Inventory.has(Main.Cfg.PhoneID) == null)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_phone_not_found"), Color.red);
                return;
            }

            if (Main.Cfg.NeedToHoldPhoneInHandForSMS)
            {
                if (!user.Player.equipment.isEquipped || user.Player.equipment.itemID != Main.Cfg.PhoneID)
                {
                    UnturnedChat.Say(user, Main.Instance.Translate("command_phone_is_not_in_hand"), Color.red);
                    return;
                }
            }

            int numbers = Numbers.Count;
            PhoneNumber userNumber = null;
            bool found = false;
            for (int i = 0; i < numbers; i++)
            {
                userNumber = Numbers[i];
                if (userNumber.SteamID == user.CSteamID.m_SteamID)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_no_simcard"), Color.red);
                return;
            }

            if (userNumber.Balance < Main.Cfg.SMSCost)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_balance_no_money", Main.Cfg.SMSCost), Color.red);
                return;
            }

            found = false;
            PhoneNumber targetNumber = null;
            for (int i = 0; i < numbers; i++)
            {
                targetNumber = Numbers[i];
                if (targetNumber.Number == number)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_invalid_number_entered"), Color.red);
                return;
            }

            UnturnedPlayer target = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(targetNumber.SteamID));
            if (target == null || target.Player == null)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_caller_is_not_in_network"), Color.red);
                return;
            }

            userNumber.Balance -= Main.Cfg.SMSCost;
            UnturnedChat.Say(user, "[SMS] " + userNumber.Number + " > " + number + ": " + message, Color.yellow);
            UnturnedChat.Say(target, "[SMS] " + userNumber.Number + " > " + number + ": " + message, Color.yellow);
        }

        public static void AddBalance(UnturnedPlayer user, uint value)
        {
            if (user.Experience < value)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_no_money_to_top_up"), Color.red);
                return;
            }

            int numbers = Numbers.Count;
            PhoneNumber number;
            for (int i = 0; i < numbers; i++)
            {
                number = Numbers[i];
                if (number.SteamID == user.CSteamID.m_SteamID)
                {
                    user.Experience -= value;
                    number.Balance += value;
                    UnturnedChat.Say(user, Main.Instance.Translate("command_phone_balance_topped_up"), Color.yellow);
                    return;
                }
            }

            UnturnedChat.Say(user, Main.Instance.Translate("command_phone_no_simcard"), Color.red);
        }

        public static void GetInfo(UnturnedPlayer user)
        {
            int numbers = Numbers.Count;
            PhoneNumber number;
            for (int i = 0; i < numbers; i++)
            {
                number = Numbers[i];
                if (number.SteamID == user.CSteamID.m_SteamID)
                {
                    UnturnedChat.Say(user, Main.Instance.Translate("command_phone_get_info", number.Number, number.Balance), Color.yellow);
                    return;
                }
            }

            UnturnedChat.Say(user, Main.Instance.Translate("command_phone_no_simcard"), Color.red);
        }

        public static void RegisterSIMCard(UnturnedPlayer user)
        {
            if (user.Experience < Main.Cfg.SIMCardCost)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_no_money_to_register_simcard", Main.Cfg.SIMCardCost), Color.red);
                return;
            }

            int numbers = Numbers.Count;
            for (int i = 0; i < numbers; i++)
            {
                if (Numbers[i].SteamID == user.CSteamID.m_SteamID)
                {
                    UnturnedChat.Say(user, Main.Instance.Translate("command_phone_simcard_is_already_registered"), Color.red);
                    return;
                }
            }

            uint defaultBalance = Main.Cfg.DefaultBalance;
            ulong newNumber = LastNumber + 1;
            LastNumber++;
            string newNumberString = Utils.NumberToString(newNumber, 1000000);
            Numbers.Add(new PhoneNumber(newNumberString, user.CSteamID.m_SteamID, defaultBalance));
            UnturnedChat.Say(user, Main.Instance.Translate("command_phone_simcard_is_registered", newNumberString, defaultBalance), Color.yellow);
        }

        public static void BuyPhone(UnturnedPlayer user)
        {
            uint phoneCost = Main.Cfg.PhoneCost;

            if (user.Experience < phoneCost)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_no_money_to_buy", phoneCost), Color.red);
                return;
            }

            ItemAsset itemAsset = Assets.find(EAssetType.ITEM, Main.Cfg.PhoneID) as ItemAsset;
            if (user.Inventory.tryAddItem(new Item(itemAsset, EItemOrigin.ADMIN), true))
            {
                user.Experience = user.Experience - phoneCost;
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_success_buying_phone"), Color.yellow);
            }
            else
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_failed_buying_phone"), Color.red);
            }
        }

        private static void CheckMaxNumberInBase()
        {
            int numbers = Numbers.Count;
            PhoneNumber number;
            for (int i = 0; i < numbers; i++)
            {
                number = Numbers[i];
                ulong num = ulong.Parse(number.Number);
                if (num > LastNumber) LastNumber = num;
            }
        }

        private static void LoadBase()
        {
            if (!Directory.Exists(Utils.DirPath)) Directory.CreateDirectory(Utils.DirPath);
            if (File.Exists(NumbersPath))
            {
                string FileContent = File.ReadAllText(NumbersPath, Encoding.UTF8);
                if (FileContent.Length != 0)
                {
                    Numbers = JsonConvert.DeserializeObject<List<PhoneNumber>>(FileContent);
                }
            }
            else
                File.Create(NumbersPath);
        }

        private static void SaveBase()
        {
            string text = JsonConvert.SerializeObject(Numbers, Formatting.Indented);
            byte[] encodeText = Encoding.UTF8.GetBytes(text);
            FileStream uwu = new FileStream(NumbersPath, FileMode.Create);
            uwu.Write(encodeText, 0, encodeText.Length);
            uwu.Close();
        }
    }
}
