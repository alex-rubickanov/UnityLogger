using UnityEngine;

namespace Rubickanov.Logger
{
    public static class RubiLoggerStatic
    {
        private static Color defaultCategoryColor = new Color(0.7882f, 0.7882f, 0.7882f);
        
        public static void Log(LogLevel logLevel, object message, string senderName, Color? categoryColor = null, LogOutput logOutput = LogOutput.Console)
        {
            if (categoryColor == null)
            {
                categoryColor = defaultCategoryColor;
            }
            
            
        }
        
        
    }
    
}

