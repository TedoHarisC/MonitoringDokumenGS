public static class ExceptionExtensions
{
    public static List<string> GetAllMessages(this Exception ex)
    {
        var list = new List<string>();
        while (ex != null)
        {
            list.Add(ex.Message);
            ex = ex.InnerException!;
        }
        return list;
    }
}
