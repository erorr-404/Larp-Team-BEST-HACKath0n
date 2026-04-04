
/// <summary>
/// Клас із статичним методом для перевірки, чи є передане значення числом.
/// </summary>
public class IsThisANum {

    /// <summary>
    /// Ця функція перевіряє, чи є передане значення числом. Вона підтримує всі стандартні числові типи C#, включаючи цілі числа, числа з плаваючою комою та десяткові числа. Якщо значення є null або не є числом, функція повертає false.
    /// </summary>
    /// <param name="value"> Значення, яке потрібно перевірити </param>
    /// <returns> true, якщо значення є числом, інакше false </returns>
    public static bool IsNumeric(object value)
    {
        if (value == null)
            return false;

        switch (Type.GetTypeCode(value.GetType()))
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return true;
            default:
                return false;
        }
    }
}
