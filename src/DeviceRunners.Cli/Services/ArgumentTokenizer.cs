namespace DeviceRunners.Cli.Services;

/// <summary>
/// Splits a command-line fragment into individual arguments using the quoting rules a
/// shell would apply.
/// </summary>
/// <remarks>
/// <para>
/// Needed because options such as <c>--browser-args</c> receive a whole fragment as a
/// single value — the shell that launched the CLI only stripped the outer quotes, so the
/// inner grouping still has to be resolved.
/// </para>
/// <para>
/// A backslash is only an escape character inside double quotes, so Windows paths can be
/// written without doubling: <c>--user-data-dir=C:\temp\profile</c> means what it looks
/// like. Inside double quotes, <c>\"</c> produces a literal quote.
/// </para>
/// </remarks>
public static class ArgumentTokenizer
{
	public static IReadOnlyList<string> Tokenize(string? value)
	{
		var results = new List<string>();

		if (string.IsNullOrWhiteSpace(value))
			return results;

		var current = new System.Text.StringBuilder();
		var hasToken = false;
		var quote = '\0';

		for (var i = 0; i < value.Length; i++)
		{
			var c = value[i];

			if (quote != '\0')
			{
				// Only double quotes support escaping, matching POSIX shells.
				if (quote == '"' && c == '\\' && i + 1 < value.Length && value[i + 1] == '"')
				{
					current.Append('"');
					i++;
					continue;
				}

				if (c == quote)
				{
					quote = '\0';
					continue;
				}

				current.Append(c);
				continue;
			}

			if (c is '"' or '\'')
			{
				// An empty quoted section still produces an argument.
				quote = c;
				hasToken = true;
				continue;
			}

			if (char.IsWhiteSpace(c))
			{
				if (hasToken)
				{
					results.Add(current.ToString());
					current.Clear();
					hasToken = false;
				}

				continue;
			}

			current.Append(c);
			hasToken = true;
		}

		if (quote != '\0')
			throw new FormatException($"Unbalanced {quote} quote in arguments: {value}");

		if (hasToken)
			results.Add(current.ToString());

		return results;
	}
}
