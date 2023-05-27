using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;

namespace Phone
{
    public class Config : IRocketPluginConfiguration
    {
        public uint PhoneCost;
        public ushort PhoneID;
        public uint DefaultBalance;
        public uint SIMCardCost;
        public uint SMSCost;
        public bool NeedToHoldPhoneInHandForSMS;
        public bool AllowToBuyAPhoneThroughTheCommand;

        public void LoadDefaults()
        {
            PhoneCost = 100;
            PhoneID = 100;
            SIMCardCost = 200;
            DefaultBalance = 200;
            SMSCost = 10;
            NeedToHoldPhoneInHandForSMS = true;
            AllowToBuyAPhoneThroughTheCommand = false;
        }
    }

    public class Main : RocketPlugin<Config>
    {
        public static Main Instance;
        public static Config Cfg;

        protected override void Load()
        {
            Instance = this;
            Cfg = Configuration.Instance;

            PhoneManager.Initialization();
        }

        protected override void Unload()
        {
            PhoneManager.Finalizing();

            Cfg = null;
            Instance = null;
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "command_phone_invalid_parameter", "Unknown parameter." },
            { "command_phone_disabled_buy", "Buying through the command is not allowed." },
            { "command_phone_failed_buying_phone", "Failed to buy a phone." },
            { "command_phone_success_buying_phone", "You bought a phone." },
            { "command_phone_no_money_to_buy", "Not enough money, the price of the phone is {0}." },
            { "command_phone_simcard_is_registered", "You have registered a SIM card. Your number: {0}. Balance of SIM card: {1}." },
            { "command_phone_simcard_is_already_registered", "You already have a SIMCard." },
            { "command_phone_no_money_to_register_simcard", "Not enough money, the price of the number is {0}." },
            { "command_phone_get_info", "Your number is {0}. Balance: {0}." },
            { "command_phone_balance_topped_up", "The balance has been topped up." },
            { "command_phone_invalid_parameter_add_money", "Enter the amount." },
            { "command_phone_no_money_to_top_up", "Not enough money to top up your balance." },
            { "command_phone_failed_to_send_message", "Specify the recipient and message." },
            { "command_phone_invalid_number_entered", "The number entered is incorrect." },
            { "command_phone_caller_is_not_in_network", "The caller is not within range of the network." },
            { "command_phone_no_simcard", "You don't have a SIM card." },
            { "command_phone_is_not_in_hand", "You must hold the phone in your hand to send SMS." },
            { "command_phone_phone_not_found", "You don't have a phone." },
            { "command_phone_balance_no_money", "There are not enough funds on the balance to send a message. SMS cost is {0}." },
        };
    }
}
