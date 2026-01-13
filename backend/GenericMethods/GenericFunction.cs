namespace EMSSolution.GenericMethods
{
    public class GenericFunction
    {
        public static void WriteLog(string LogFileName, string Content)
        {
            string filePath = $@"C:\Windows\Temp\{LogFileName}{DateTime.Now.Day.ToString("00")}{DateTime.Now.Month.ToString("00")}.txt";

            // Use StreamWriter to write to the file
            try
            {
                // Create or overwrite the file
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine(Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

    }
}
