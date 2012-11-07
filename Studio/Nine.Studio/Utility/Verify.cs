namespace Nine.Studio
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// A static class for retail validated assertions.
    /// </summary>
    [DebuggerStepThrough]
    static class Verify
    {
        public static void IsNeitherNullNorEmpty(string value, string name)
        {
            // Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
            const string errorMessage = "The parameter can not be either null or empty.";
            if (null == value)
                throw new ArgumentNullException(name, errorMessage);
            if ("" == value)
                throw new ArgumentException(errorMessage, name);
        }
        
        public static void IsNeitherNullNorWhitespace(string value, string name)
        {
            // Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
            const string errorMessage = "The parameter can not be either null or empty or consist only of white space characters.";
            if (null == value)
                throw new ArgumentNullException(name, errorMessage);
            if ("" == value.Trim())
                throw new ArgumentException(errorMessage, name);
        }

        public static void IsNotNull<T>(T obj, string name) where T : class
        {
            if (null == obj)
                throw new ArgumentNullException(name);
        }

        public static void IsFalse(bool statement, string name)
        {
            if (statement)
                throw new ArgumentException("", name);
        }

        public static void IsTrue(bool statement, string name, string message)
        {
            if (!statement)
                throw new ArgumentException(message, name);
        }

        public static void BoundedInteger(int lowerBoundInclusive, int value, int upperBoundExclusive, string parameterName)
        {
            if (value < lowerBoundInclusive || value >= upperBoundExclusive)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The integer value must be bounded with [{0}, {1})", lowerBoundInclusive, upperBoundExclusive), parameterName);
        }
        
        public static void IsAssignableFrom<T>(object parameter, string parameterName)
        {
            Verify.IsNotNull(parameter, "parameter");

            if (!typeof(T).IsAssignableFrom(parameter.GetType()))
                throw new ArgumentException("The type of this parameter does not support a required interface", parameterName);
        }

        public static void IsAssignableFrom(object parameter, Type type, string parameterName)
        {
            Verify.IsNotNull(type, "type");
            Verify.IsNotNull(parameter, "parameter");

            if (!type.IsAssignableFrom(parameter.GetType()))
                throw new ArgumentException("The type of this parameter does not support a required interface", parameterName);
        }
        
        public static void IsValidFileName(string parameter, string parameterName)
        {
            Verify.IsNeitherNullNorEmpty(parameter, parameterName);

            var invalidChar = '0';
            var invalidFilenameChars = Path.GetInvalidFileNameChars();

            for (int i = 0; i < parameter.Length; i++)
            {
                for (int j = 0; j < invalidFilenameChars.Length; j++)
                    if (parameter[i] == invalidFilenameChars[j])
                    {
                        invalidChar = parameter[i];
                        break;
                    }
            }

            if (invalidChar != '0')
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The parameter contains illegal fileName charactors {0}.", invalidChar.ToString()), parameterName);
        }

        public static void IsValidPath(string parameter, string parameterName)
        {
            Verify.IsNeitherNullNorEmpty(parameter, parameterName);

            var invalidChar = '0';
            var invalidPathChars = Path.GetInvalidPathChars();

            for (int i = 0; i < parameter.Length; i++)
            {
                for (int j = 0; j < invalidPathChars.Length; j++)
                    if (parameter[i] == invalidPathChars[j])
                    {
                        invalidChar = parameter[i];
                        break;
                    }
            }

            if (invalidChar != '0')
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The parameter contains illegal path charactors {0}.", invalidChar.ToString()), parameterName);
        }

        public static void FileExists(string filePath, string parameterName)
        {
            Verify.IsNeitherNullNorEmpty(filePath, parameterName);
            if (!File.Exists(filePath))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The file \"{0}\" does not exist", filePath), parameterName);
        }

        public static void DirectoryExists(string directory, string parameterName)
        {
            Verify.IsNeitherNullNorEmpty(directory, parameterName);
            if (!Directory.Exists(directory))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The directory \"{0}\" does not exist", directory), parameterName);
        }
    }
}
