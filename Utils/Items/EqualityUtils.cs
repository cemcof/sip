namespace sip.Utils.Items;

public static class EqualityComparerExtensions
{
    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> using the given <see cref="IEqualityComparer{T}"/> to compare
    /// the specified <paramref name="item"/> with an input element.
    /// </summary>
    /// <typeparam name="T">The type of elements to compare.</typeparam>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use for the comparison.</param>
    /// <param name="item">The item to compare against.</param>
    /// <returns>
    /// A <see cref="Predicate{T}"/> that compares an input element to <paramref name="item"/>
    /// using the provided <paramref name="comparer"/>.
    /// </returns>
    /// <remarks>
    /// This extension method is particularly useful when you need to use custom equality logic 
    /// with methods that accept a <see cref="Predicate{T}"/>, such as <see>
    ///     <cref>List{T}.FindIndex</cref>
    /// </see>
    /// .
    /// Since methods like <see>
    ///     <cref>List{T}.FindIndex</cref>
    /// </see>
    /// do not accept an <see cref="IEqualityComparer{T}"/>,
    /// you can use this method to create a predicate that performs comparisons using custom equality logic.
    /// </remarks>
    /// <example>
    /// The following example demonstrates using the method with a case-insensitive string comparison:
    /// <code>
    /// var list = new List&lt;string&gt; { "apple", "Banana", "cherry" };
    /// var comparer = StringComparer.OrdinalIgnoreCase;
    /// var predicate = comparer.EqualsWith("banana");
    /// int index = list.FindIndex(predicate); // Returns 1
    /// </code>
    /// </example>
    public static Predicate<T> EqualsWith<T>(this IEqualityComparer<T> comparer, T item)
    {
        return input => comparer.Equals(item, input);
    }
}
