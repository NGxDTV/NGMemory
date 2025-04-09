using System;

namespace NGMemory.Easy
{
    /// <summary>
    /// Bietet einfache Hilfsmethoden für den Umgang mit Enums.
    /// </summary>
    public static class EasyEnums
    {
        /// <summary>
        /// Wandelt das Enum Register in seinen Namen als String um.
        /// Beispiel: Register.Rax => "Rax"
        /// </summary>
        public static string GetRegisterName(NGMemory.Enums.Register register)
        {
            return register.ToString();
        }

        /// <summary>
        /// Wandelt das Enum MessageBoxType in seinen Namen als String um.
        /// Beispiel: MessageBoxType.MB_OK => "MB_OK"
        /// </summary>
        public static string GetMessageBoxTypeName(NGMemory.Enums.MessageBoxType msgBoxType)
        {
            return msgBoxType.ToString();
        }
    }
}
