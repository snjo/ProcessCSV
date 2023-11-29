namespace ProcessCsvLibrary
{
    public static class Tools
    {
        public static string SafeIndex(this string[] array, int index, string error = "")
        {
            if (index < 0 || index >= array.Length)
                return error;
            else
                return array[index];
        }

        public static int SafeIndex(this int[] array, int index, int error = 0)
        {
            if (index < 0 || index >= array.Length)
                return error;
            else
                return array[index];
        }

        public static string SafeIndex(this List<string> stringList, int index, string error = "")
        {
            if (index < 0 || index >= stringList.Count)
                return error;
            else
                return stringList[index];
        }

        public static List<string>? SafeIndex(this List<List<string>> stringList, int index, List<string>? error = null)
        {
            //Debug.WriteLine("SafeIndex: " + stringList.Count + ", index: " + index);
            if (index < 0 || index >= stringList.Count)
            {
                return error;
            }
            else
            {
                //Debug.WriteLine("returning: " + stringList[index].ToString());
                return stringList[index];
            }
        }

        public static List<Field>? SafeIndex(this List<Record> stringList, int index, List<Field>? error = null)
        {
            //Debug.WriteLine("SafeIndex: " + stringList.Count + ", index: " + index);
            if (index < 0 || index >= stringList.Count)
            {
                return error;
            }
            else
            {
                //Debug.WriteLine("returning: " + stringList[index].ToString());
                return stringList[index].Fields;
            }
        }

        public static Field? SafeIndex(this List<Field> stringList, int index)
        {
            if (index < 0 || index >= stringList.Count)
                return null;
            else
                return stringList[index];
        }

        public static string GetDelimiter(string? delimiterText)
        {
            if (delimiterText != null)
            {
                switch (delimiterText)
                {
                    case "tab":
                        return "\t";
                    case "comma":
                        return ",";
                    case "semicolon":
                        return ";";
                    default:
                        return delimiterText;
                }
            }
            return ",";
        }
    }
}
