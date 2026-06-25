using System.Text;

namespace DeviceRunners.Cli.Commands;

/// <summary>
/// Settings that expose the Microsoft Testing Platform "simple" test filters
/// (xUnit v3's <c>--filter-class</c> / <c>--filter-method</c> /
/// <c>--filter-namespace</c> / <c>--filter-trait</c> family and their
/// <c>not-</c> variants).
/// </summary>
public interface IMtpSimpleFilterSettings
{
	string[]? FilterClass { get; }
	string[]? FilterNotClass { get; }
	string[]? FilterMethod { get; }
	string[]? FilterNotMethod { get; }
	string[]? FilterNamespace { get; }
	string[]? FilterNotNamespace { get; }
	string[]? FilterTrait { get; }
	string[]? FilterNotTrait { get; }
}

/// <summary>
/// Translates the Microsoft Testing Platform "simple" filters into the
/// DeviceRunners <c>--filter</c> expression that the on-device
/// <c>TestCaseFilter</c> understands.
/// </summary>
/// <remarks>
/// Property mapping: class → <c>ClassName</c>, method → <c>FullyQualifiedName</c>,
/// namespace → <c>Namespace</c>, trait <c>n=v</c> → property <c>n</c> value <c>v</c>.
/// Multiple values of the same kind OR together; different kinds AND together;
/// the <c>not-</c> variants exclude (AND-ed negations). <c>*</c> wildcards are
/// left intact so the on-device evaluator can apply them.
/// </remarks>
public static class MtpFilterTranslator
{
	/// <summary>
	/// Whether any of the simple filter options were specified.
	/// </summary>
	public static bool HasSimpleFilters(IMtpSimpleFilterSettings settings) =>
		HasValues(settings.FilterClass) || HasValues(settings.FilterNotClass) ||
		HasValues(settings.FilterMethod) || HasValues(settings.FilterNotMethod) ||
		HasValues(settings.FilterNamespace) || HasValues(settings.FilterNotNamespace) ||
		HasValues(settings.FilterTrait) || HasValues(settings.FilterNotTrait);

	/// <summary>
	/// Validates the trait filters, which must use the <c>name=value</c> form.
	/// Returns an error message for the first malformed entry, or <c>null</c> when
	/// every trait filter is well-formed. This prevents a nameless trait (e.g.
	/// <c>=Fast</c>) from being silently reinterpreted as a different filter kind.
	/// </summary>
	public static string? ValidateTraits(IMtpSimpleFilterSettings settings)
	{
		foreach (var entry in Clean(settings.FilterTrait).Concat(Clean(settings.FilterNotTrait)))
		{
			// A valid trait needs a non-empty name before the '=' separator, so the
			// separator must be present (eq >= 0) and not the first character (eq > 0).
			if (entry.IndexOf('=') <= 0)
				return $"Invalid trait filter '{entry}'. Traits must be specified as name=value.";
		}

		return null;
	}

	/// <summary>
	/// Builds the combined <c>--filter</c> expression, or <c>null</c> when no
	/// simple filters were specified.
	/// </summary>
	public static string? Translate(IMtpSimpleFilterSettings settings)
	{
		var groups = new List<string>();

		AddPropertyGroup(groups, "ClassName", settings.FilterClass, negate: false);
		AddPropertyGroup(groups, "ClassName", settings.FilterNotClass, negate: true);
		AddPropertyGroup(groups, "FullyQualifiedName", settings.FilterMethod, negate: false);
		AddPropertyGroup(groups, "FullyQualifiedName", settings.FilterNotMethod, negate: true);
		AddPropertyGroup(groups, "Namespace", settings.FilterNamespace, negate: false);
		AddPropertyGroup(groups, "Namespace", settings.FilterNotNamespace, negate: true);
		AddTraitGroup(groups, settings.FilterTrait, negate: false);
		AddTraitGroup(groups, settings.FilterNotTrait, negate: true);

		return groups.Count == 0 ? null : string.Join(" & ", groups);
	}

	static void AddPropertyGroup(List<string> groups, string property, string[]? values, bool negate)
	{
		var cleaned = Clean(values);
		if (cleaned.Count == 0)
			return;

		var op = negate ? "!=" : "=";
		var conditions = cleaned.Select(v => $"{property}{op}{Escape(v)}").ToList();
		groups.Add(Combine(conditions, negate));
	}

	static void AddTraitGroup(List<string> groups, string[]? values, bool negate)
	{
		var cleaned = Clean(values);
		if (cleaned.Count == 0)
			return;

		var op = negate ? "!=" : "=";
		var conditions = new List<string>(cleaned.Count);
		foreach (var entry in cleaned)
		{
			var eq = entry.IndexOf('=');
			var name = eq >= 0 ? entry[..eq] : entry;
			var value = eq >= 0 ? entry[(eq + 1)..] : string.Empty;
			conditions.Add($"{Escape(name)}{op}{Escape(value)}");
		}

		groups.Add(Combine(conditions, negate));
	}

	// Inclusive conditions of the same kind OR together; exclusions AND together
	// (a test is dropped when it matches any of them). Multi-condition groups are
	// parenthesized so they compose safely with the cross-kind AND join.
	static string Combine(List<string> conditions, bool negate)
	{
		if (conditions.Count == 1)
			return conditions[0];

		var joined = string.Join(negate ? "&" : "|", conditions);
		return $"({joined})";
	}

	static List<string> Clean(string[]? values) =>
		values is null
			? new List<string>()
			: values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();

	static bool HasValues(string[]? values) => Clean(values).Count > 0;

	// Escapes the expression's structural characters so names and values are
	// treated literally. '*' is intentionally left intact so it stays a wildcard.
	static string Escape(string value)
	{
		var sb = new StringBuilder(value.Length);
		foreach (var c in value)
		{
			if (c is '\\' or '(' or ')' or '&' or '|' or '=' or '~' or '!')
				sb.Append('\\');
			sb.Append(c);
		}
		return sb.ToString();
	}
}
