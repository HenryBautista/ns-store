namespace NsStore.Application.Common;

/// <summary>
/// Makes <c>?search=</c> ignore accents as well as case, so "telefono" finds "Teléfono" and
/// "nunez" finds "Núñez" — which is how the counter actually types.
/// </summary>
/// <remarks>
/// Two halves that have to agree. <see cref="Normalize"/> folds the term the caller typed, in C#.
/// <see cref="Unaccent"/> folds the column, in SQL: it is mapped to PostgreSQL's <c>unaccent()</c>
/// in <c>AppDbContext.OnModelCreating</c>, and the test harness registers a SQLite function of the
/// same name backed by <see cref="StripDiacritics"/>, so the suite exercises the real predicate
/// rather than a provider fork.
///
/// The fold is a table rather than <c>String.Normalize(FormD)</c> because the whole solution builds
/// with <c>InvariantGlobalization</c>, where normalization silently returns its input. The table
/// covers the Latin-1 letters — every accent Spanish uses, and the same one-to-one mapping
/// <c>unaccent</c> applies to that range. Characters outside it (ß, Æ, Œ, Latin Extended-A) are left
/// alone: <c>unaccent</c> expands some of those into two letters, and a fold that disagreed with the
/// database would quietly stop matching instead of matching more.
///
/// Deliberately not applied to uniqueness checks (client CI, catalog names, usernames, serials):
/// folding those would make "Peña" collide with "Pena", which is a change to a business rule rather
/// than a convenience while searching.
/// </remarks>
public static class SearchText
{
    private const string Accented =
        "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏ" +
        "ÐÑÒÓÔÕÖØÙÚÛÜÝ" +
        "àáâãäåçèéêëìíîï" +
        "ðñòóôõöøùúûüýÿ";

    /// <summary>Character for character in step with <see cref="Accented"/>.</summary>
    private const string Plain =
        "AAAAAACEEEEIIII" +
        "DNOOOOOOUUUUY" +
        "aaaaaaceeeeiiii" +
        "dnoooooouuuuyy";

    /// <summary>Only ever called inside an EF query, where it becomes the SQL function.</summary>
    public static string Unaccent(string value) =>
        throw new InvalidOperationException(
            $"{nameof(Unaccent)} exists to be translated to SQL and cannot run on the client.");

    /// <summary>Folds a search term the same way <see cref="Unaccent"/> folds the column.</summary>
    public static string Normalize(string value) => StripDiacritics(value).ToLowerInvariant();

    public static string StripDiacritics(string value)
    {
        char[]? folded = null;

        for (var i = 0; i < value.Length; i++)
        {
            var index = Accented.IndexOf(value[i]);
            if (index < 0)
            {
                continue;
            }

            // Most terms carry no accent at all, so the copy only happens once one shows up.
            folded ??= value.ToCharArray();
            folded[i] = Plain[index];
        }

        return folded is null ? value : new string(folded);
    }
}
