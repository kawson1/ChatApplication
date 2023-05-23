class Program
{
    static bool loop = true;
    public static Task WaitingLoop()
    {
        return Task.Run(() =>
        {
            while (loop)
            {
                Console.WriteLine("Loop");
                Thread.Sleep(1000);
            }
        });
    }

    static void Main(string[] args)
    {
        WaitingLoop();
        Console.WriteLine("Uruchomilem Waiting loop");
        Console.ReadKey();
        loop = false;
        Console.ReadKey();
    }
}