using System;
using System.Text.RegularExpressions;

namespace CNP.Helper
{
    public static class Validator
    {
        static Regex _argName = new Regex("[_a-z][_a-zA-Z0-9]{0,30}", RegexOptions.Compiled);
        
        public static void AssertArgumentName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Argument name must be a non-null, non-empty string.");
            if (!_argName.IsMatch(name))
                throw new Exception("Argument name must start with _ or a-z, and continue with _,a-z,A-Z, or 0-9 up to 30 characters.");
        }
    }
}