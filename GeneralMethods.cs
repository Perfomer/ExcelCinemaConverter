using System;

namespace ExcelConverter
{
    class GeneralMethods
    {
        
        public const string WRONG_PATH_ERROR = "WrongPathError";

        public static string CutPath(string path, bool format)
        {
            char separator = format ? '.' : '\\';
            for (int i = path.Length - 1; i > 0; i--)
            {
                if (path[i] == separator) return path.Substring(0, i);
            }
            return WRONG_PATH_ERROR;
        }

        public static string TranslateWord(string word)
        {
            string result = "";
            for (int i = 0; i < word.Length; i++)
            {
                result += TranslateLetter(word[i]);
            }

            return result;
        }

        private static string TranslateLetter(char letter)
        {
            bool isUpper = Char.IsUpper(letter);
            string result = letter.ToString();

            switch (Char.ToLower(letter))
            {
                case 'а': result = "a"; break;
                case 'б': result = "b"; break;
                case 'в': result = "v"; break;
                case 'г': result = "g"; break;
                case 'д': result = "d"; break;
                case 'е': result = "e"; break;
                case 'ё': result = "yo"; break;
                case 'ж': result = "zh"; break;
                case 'з': result = "z"; break;
                case 'и': result = "i"; break;
                case 'й': result = "y"; break;
                case 'к': result = "k"; break;
                case 'л': result = "l"; break;
                case 'м': result = "m"; break;
                case 'н': result = "n"; break;
                case 'о': result = "o"; break;
                case 'п': result = "p"; break;
                case 'р': result = "r"; break;
                case 'с': result = "s"; break;
                case 'т': result = "t"; break;
                case 'у': result = "u"; break;
                case 'ф': result = "f"; break;
                case 'х': result = "kh"; break;
                case 'ц': result = "c"; break;
                case 'ч': result = "ch"; break;
                case 'ш': result = "sh"; break;
                case 'ы': result = "y"; break;
                case 'щ': result = "sch"; break;
                case 'э': result = "e"; break;
                case 'ю': result = "yu"; break;
                case 'я': result = "ya"; break;
                case 'ъ': result = ""; break;
                case 'ь': result = ""; break;
                case ' ': result = ""; break;
            }
            if (isUpper) result = result.ToUpper();

            return result;
        }

    }
}
