using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Utils
{
    /// <summary>
    /// Helps with json conversion
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Remove everything exept json data from string
        /// </summary>
        /// <param name="dirtyJson">String with json data in it</param>
        /// <returns>Clean json string ready to convert</returns>
        public static string ConvertToJsonString(string dirtyJson)
        {
            ReadOnlySpan<char> cleanJson = dirtyJson;

            int balance = 0;
            int i = 0;

            do
            {
                char currentChar = dirtyJson[i];

                if (currentChar == '{')
                    balance++;
                else if (currentChar == '}')
                    balance--;

                i++;
            } while (balance > 0);

            return cleanJson.Slice(0, i - 1).ToString();
        }

    }
}
