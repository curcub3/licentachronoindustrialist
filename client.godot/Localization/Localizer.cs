using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using Core.Simulation.Data;

namespace Client.Localization
{
    public static class Localizer
    {
        private const string Locale = "ro";
        private const string TranslationPath = "res://Localization/ro.csv";
        private static readonly Dictionary<string, string> Messages = new();
        private static bool _loaded;

        public static void Initialize()
        {
            if (_loaded)
                return;

            TranslationServer.SetLocale(Locale);
            LoadCsv();
            _loaded = true;
        }

        public static string Tr(string key)
        {
            Initialize();
            return Messages.TryGetValue(key, out var value) ? value : key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, Tr(key), args);
        }

        public static string ProductName(Product product) => Tr($"PRODUCT_{product.Id}");
        public static string ProductName(int productId) => Tr($"PRODUCT_{productId}");
        public static string Phase(DayPhase phase) => Tr($"PHASE_{phase}");
        public static string Mood(EconomicMood mood) => Tr($"MOOD_{mood}");
        public static string Event(GameEventType type) => Tr($"EVENT_{type}");
        public static string EventPrompt(GameEventType type) => Tr($"EVENT_PROMPT_{type}");
        public static string EventOptionA(GameEventType type) => Tr($"EVENT_A_{type}");
        public static string EventOptionB(GameEventType type) => Tr($"EVENT_B_{type}");
        public static string Shelf(ShelfDisplayType type) => Tr($"SHELF_{type}");
        public static string ShopItem(int id) => Tr($"SHOP_ITEM_{id}");

        public static string SupportEffect(string? effect)
        {
            return effect switch
            {
                null or "" or "None" => Tr("SUPPORT_NONE"),
                "Absorbed part of the import-cost shock." => Tr("SUPPORT_ABSORB_IMPORT"),
                "Passed import pressure to customers." => Tr("SUPPORT_PASS_IMPORT"),
                "Full refund protected trust." => Tr("SUPPORT_FULL_REFUND"),
                "Store credit contained the complaint." => Tr("SUPPORT_STORE_CREDIT"),
                "Refund request accepted." => Tr("SUPPORT_REFUND_ACCEPTED"),
                "Refund denied; reputation suffered." => Tr("SUPPORT_REFUND_DENIED"),
                "Investigated warranty issue with supplier." => Tr("SUPPORT_INVESTIGATED_WARRANTY"),
                "Warranty dispute left unresolved." => Tr("SUPPORT_WARRANTY_DELAYED"),
                _ => effect
            };
        }

        private static void LoadCsv()
        {
            using var file = FileAccess.Open(TranslationPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PushWarning($"Localizer: missing translation file {TranslationPath}");
                return;
            }

            bool first = true;
            while (!file.EofReached())
            {
                var row = file.GetCsvLine();
                if (row.Length < 2)
                    continue;

                if (first)
                {
                    first = false;
                    if (row[0] == "key")
                        continue;
                }

                if (!string.IsNullOrWhiteSpace(row[0]))
                    Messages[row[0]] = string.Join(",", row, 1, row.Length - 1);
            }
        }
    }
}
