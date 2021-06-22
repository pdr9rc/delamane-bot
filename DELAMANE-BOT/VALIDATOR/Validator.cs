using System;
using System.Collections.Generic;
using System.Text;

namespace VALIDATOR
{
    public class Validator
    {
        public static int Validate(string input, IDictionary<string, string> sessionAttr)
        {
            try
            {
                int size = int.Parse(sessionAttr["RemediationLength"]);
                if (size == 1)
                {
                    if (input == "yes" || input == "1")
                        return 1;
                    else if (input == "no")
                        return 0;
                }
                int _input = int.Parse(input);
                if (_input >= size)
                    return -1;
                return _input;
             }
            catch
            {
                return -1;
            }
        }
    }
}
