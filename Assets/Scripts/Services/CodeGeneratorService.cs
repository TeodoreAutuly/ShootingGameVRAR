using System;
using System.Text;

public interface ICodeGenerator
{
    string Generate(int length);
}

public class AlphanumericCodeGenerator : ICodeGenerator
{
    private const string Charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly Random random = new Random();

    public string Generate(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "La longueur doit être supérieure à 0.");
        }

        StringBuilder builder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int index = random.Next(0, Charset.Length);
            builder.Append(Charset[index]);
        }

        return builder.ToString();
    }
}