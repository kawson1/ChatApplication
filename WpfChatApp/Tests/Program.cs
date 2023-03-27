class Program
{
    static void Main(string[] args)
    {
        test();
        Console.ReadKey();
    }

    static async Task test()
    {
        if(await Tt() == 1)
        {
            Console.WriteLine("If koniec");
        }
        Console.WriteLine("Po ifie");
    }

    static async Task<int> Tt()
    {
        await Task.Delay(2000);
        return 1;
    }
}