namespace RobotCleaner.Exceptions
{
         public class InvalidCommandException : Exception
        {
            public InvalidCommandException(string message) : base(message) { }
        }

        public class InvalidDirectionException : Exception
        {
            public InvalidDirectionException() {}
            public InvalidDirectionException(string message) : base(message) { }
        }

        public class InvalidStartingPointException: Exception
        {
            public InvalidStartingPointException() : base("Invalid starting point data.") { }
        }
}
