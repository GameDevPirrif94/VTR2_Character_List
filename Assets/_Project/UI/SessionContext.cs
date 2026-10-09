using VTR.Core.Models;
using VTR.Data;

namespace VTR.UI
{
    public static class SessionContext
    {
        public static string PlayerName = "";
        public static bool IsGameMaster = false;

        public static Chronicle CurrentChronicle;
        public static Character CurrentCharacter;
        public static CharacterDraft CurrentDraft;

        /// <summary>true = мастер смотрит чужой чар-лист.</summary>
        public static bool IsGMViewingCharacter = false;

        /// <summary>clientId игрока, чей персонаж сейчас открыт у мастера. 0 = свой/нет.</summary>
        public static ulong GMViewingOwnerId = 0;

        private static ContentLibrary _library;
        public static ContentLibrary Library
        {
            get
            {
                if (_library == null) _library = ContentLibrary.Load();
                return _library;
            }
        }

        public static void Reset()
        {
            PlayerName = "";
            IsGameMaster = false;
            CurrentChronicle = null;
            CurrentCharacter = null;
            CurrentDraft = null;
            IsGMViewingCharacter = false;
            GMViewingOwnerId = 0;
        }
    }
}