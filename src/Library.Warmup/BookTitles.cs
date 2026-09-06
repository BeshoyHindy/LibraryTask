namespace Library.Warmup;

public static class BookTitles
{
    public static string Reverse(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        var characters = title.ToCharArray();
        var left = 0;
        var right = characters.Length - 1;

        while (left < right)
        {
            (characters[left], characters[right]) = (characters[right], characters[left]);
            left++;
            right--;
        }

        // Reversing character by character leaves every surrogate pair back to front, where the two
        // halves no longer form the original code point, so put each pair back in order.
        var index = 0;

        while (index < characters.Length - 1)
        {
            if (char.IsLowSurrogate(characters[index]) && char.IsHighSurrogate(characters[index + 1]))
            {
                (characters[index], characters[index + 1]) = (characters[index + 1], characters[index]);
                index += 2;
            }
            else
            {
                index++;
            }
        }

        return new string(characters);
    }

    public static string Replicate(string title, int count)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var characters = new char[checked(title.Length * count)];
        var next = 0;

        for (var repetition = 0; repetition < count; repetition++)
        {
            for (var index = 0; index < title.Length; index++)
            {
                characters[next] = title[index];
                next++;
            }
        }

        return new string(characters);
    }
}
