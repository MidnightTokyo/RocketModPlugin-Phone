using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Phone
{
    public class CommandPhone : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "phone";

        public string Help => string.Empty;

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "phone" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            if (command.Length == 0)
            {
                UnturnedChat.Say(user, Main.Instance.Translate("command_phone_invalid_parameter"), Color.red);
                return;
            }

            switch (command[0].ToLower())
            {
                case "buy":
                    if (Main.Cfg.AllowToBuyAPhoneThroughTheCommand)
                        PhoneManager.BuyPhone(user);
                    else
                        UnturnedChat.Say(user, Main.Instance.Translate("command_phone_disabled_buy"));
                    break;
                case "register":
                    PhoneManager.RegisterSIMCard(user);
                    break;
                case "info":
                    PhoneManager.GetInfo(user);
                    break;
                case "addbalance":
                    if (command.Length < 2)
                    {
                        uint num;
                        if (uint.TryParse(command[1], out num))
                        {
                            PhoneManager.AddBalance(user, num);
                        }
                        else
                        {
                            UnturnedChat.Say(user, Main.Instance.Translate("command_phone_invalid_parameter_add_money"));
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(user, Main.Instance.Translate("command_phone_invalid_parameter_add_money"));
                    }
                    break;
                case "sms":
                    if (!(command.Length < 3))
                    {
                        string text = "";
                        for (int i = 2; i < command.Length; i++)
                        {
                            text = text + command[i] + " ";
                        }
                        text.Trim();
                        PhoneManager.SendMessage(user, command[1], text);
                    }
                    else
                    {
                        UnturnedChat.Say(user, Main.Instance.Translate("command_phone_failed_to_send_message"));
                    }
                    break;
                default:
                    UnturnedChat.Say(user, Main.Instance.Translate("command_phone_invalid_parameter"));
                    break;
            }
        }
    }
}
